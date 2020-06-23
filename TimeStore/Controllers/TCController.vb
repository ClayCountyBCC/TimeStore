'Imports System.Web.Caching
Imports System.Web.Mvc
Imports TimeStore.Models
Imports System.Runtime.Caching

Namespace Controllers
  Public Class TCController
    Inherits Controller

    Private cache As myCache

    'Private Function IsItPastCutoffDate(WorkDate As Date) As Boolean
    '  ' This function returns true if the current date is greater than the 
    '  ' cutoff for the date in the pay period that was passed.  
    '  ' Ugh, that doesn't mean anything.  Let me try again.
    '  ' The cutoff for changes to the pay period is 10 AM the day after the end of
    '  ' the pay period.  So if the pay period were to end on 9/6/2016,
    '  ' users would have until 9/7/2016 10:00 AM EDT to make changes.
    '  ' If the current date/time is after that for a given pay period ending date, 
    '  ' we return true.
    '  Dim specialDisasterPayPeriodStart As Date = Date.Parse("8/21/2019")
    '  Dim specialDisasterPayPeriodEnd As Date = specialDisasterPayPeriodStart.AddDays(13)
    '  If (WorkDate >= specialDisasterPayPeriodStart And WorkDate < specialDisasterPayPeriodEnd) Then
    '    Return Now > GetPayPeriodStart(WorkDate).AddDays(16).AddHours(17)
    '  Else
    '    Return Now > GetPayPeriodStart(WorkDate).AddDays(14).AddHours(PayPeriodEndingCutoff)
    '  End If
    'End Function

    Private Function GetTimeCardAccess(UserName As String) As Timecard_Access
#If DEBUG Then
      'UserName = "wardj"
#End If
      Dim EID As Integer = AD_EmployeeData.GetEmployeeIDFromAD(UserName)

      Dim myCookie As New HttpCookie("employeeid", EID.ToString)
      myCookie.Expires = Today.AddYears(1)
      myCookie.HttpOnly = False
      HttpContext.Response.SetCookie(myCookie)
      Return Timecard_Access.GetTimeCardAccess(EID)
    End Function

    Function Index() As ActionResult
      Dim x As Timecard_Access = GetTimeCardAccess(Request.LogonUserIdentity.Name)
      If x.Raw_Access_Type = Timecard_Access.Access_Types.No_Access Then
        Return New HttpUnauthorizedResult()
      Else
        Return View()
      End If
    End Function

    <HttpPost>
    <OutputCache(VaryByParam:="*", Duration:=0, NoStore:=True)>
    Function CurrentEmployee() As JsonNetResult
      Return Employee(AD_EmployeeData.GetEmployeeIDFromAD(Request.LogonUserIdentity.Name), 0)
    End Function

    <HttpPost>
    <OutputCache(VaryByParam:="*", Duration:=0, NoStore:=True)>
    Function Employee(ByVal EmployeeId As Integer, PayPeriod As Integer) As JsonNetResult
      'Dim tca As Timecard_Access = GetTimeCardAccess(Request.LogonUserIdentity.Name)
      'Dim edl As List(Of Employee_Data) = myCache.GetItem("employeeList", defaultCIP) 'HttpContext.Cache("employeeList")
      Dim ppd As Date = GetPayPeriodStart(Today.AddDays(PayPeriod * 14))
      'If Today = GetPayPeriodStart(Today) And Now.Hour < 10 And PayPeriod = 0 Then
      '  ppd = GetPayPeriodStart(Today.AddDays(-1 * 14))
      'End If
      Dim jnr As New JsonNetResult
      Try
        Dim eidToUse As Integer = AD_EmployeeData.GetEmployeeIDFromAD(Request.LogonUserIdentity.Name)
        If Timecard_Access.Check_Access_To_EmployeeId(eidToUse, EmployeeId) Then eidToUse = EmployeeId
        Dim x As New GenericTimecard(ppd, eidToUse)

        jnr.Data = x
        jnr.JsonRequestBehavior = JsonRequestBehavior.AllowGet
      Catch ex As Exception
        Dim e As New ErrorLog(ex, EmployeeId.ToString & " " & PayPeriod.ToString)
      End Try

      Return jnr
    End Function

    Public Function EmployeeList() As JsonNetResult
      Dim tca As Timecard_Access = GetTimeCardAccess(Request.LogonUserIdentity.Name)
      Dim edl As List(Of Employee_Data) = myCache.GetItem("employeelist") 'HttpContext.Cache("employeeList")
      Dim jnr As New JsonNetResult
      jnr.JsonRequestBehavior = JsonRequestBehavior.AllowGet
      Dim il As List(Of Integer) = Get_Reporting_Users(tca.EmployeeID)
      Dim higheraccesslevelusers As List(Of Integer) = Get_Higher_Access_Users(tca.Raw_Access_Type)
      Select Case tca.Raw_Access_Type
        Case Timecard_Access.Access_Types.All_Access
          jnr.Data = edl
        Case Timecard_Access.Access_Types.Department_1,
             Timecard_Access.Access_Types.Department_2,
             Timecard_Access.Access_Types.Department_3,
             Timecard_Access.Access_Types.Department_4,
             Timecard_Access.Access_Types.Department_5

          jnr.Data = (From e In edl
                      Where il.Contains(e.EmployeeID) Or
                        e.EmployeeID = tca.EmployeeID Or
                        (tca.DepartmentsToApprove.Contains(e.DepartmentID) And
                        Not higheraccesslevelusers.Contains(e.EmployeeID))
                      Select e).ToList

        Case Else
          jnr.Data = New List(Of String)
      End Select
      Return jnr
    End Function

    <HttpPost>
    <OutputCache(VaryByParam:="*", Duration:=0, NoStore:=True)>
    Public Function EmployeeList(ByVal Department As String) As JsonNetResult
      Dim tca As Timecard_Access = GetTimeCardAccess(Request.LogonUserIdentity.Name)
      Dim edl As List(Of Employee_Data) = myCache.GetItem("employeeList")
      Dim jnr As New JsonNetResult
      'jnr.JsonRequestBehavior = JsonRequestBehavior.AllowGet
      Dim testList() As String = {"ALL", "VIEW", "LEAVE"}
      If testList.Intersect(tca.DepartmentsToApprove).Any() Then
        'If tca.DepartmentsToApprove.Contains(testList(0)) Or
        '  tca.DepartmentsToApprove.Contains(testList(1)) Or
        '  tca.DepartmentsToApprove.Contains(testList(2)) Then

        jnr.Data = (From e In edl
                    Where e.DepartmentID = Department Or
                      tca.ReportsToList.Contains(e.EmployeeID)
                    Select e).ToList
      ElseIf tca.DepartmentsToApprove.Contains(Department) Then
        jnr.Data = (From e In edl
                    Where e.DepartmentID = Department Or
                      tca.ReportsToList.Contains(e.EmployeeID)
                    Select e).ToList
      Else
        jnr.Data = (From e In edl
                    Where tca.ReportsToList.Contains(e.EmployeeID)
                    Select e).ToList
      End If
      Return jnr
    End Function

    Public Function ReportsToList() As JsonNetResult
      ' This function is meant to return all the possible people to assign a person to report to.
      Dim tca As Timecard_Access = GetTimeCardAccess(Request.LogonUserIdentity.Name)
      Dim edl As List(Of Employee_Data) = myCache.GetItem("employeeList")
      Dim jnr As New JsonNetResult
      jnr.JsonRequestBehavior = JsonRequestBehavior.AllowGet
      Dim il As List(Of Integer) = Get_Valid_Reports_To_Users()
      If tca.CanChangeAccess Then
        jnr.Data = (From e In edl
                    Where il.Contains(e.EmployeeID) Or
                      e.EmployeeID = tca.EmployeeID
                    Select e).ToList
      Else
        jnr.Data = New List(Of String)
      End If
      Return jnr
    End Function

    Public Function PayPeriodList() As JsonNetResult
      ' This function is going to list the the pay period starting dates
      ' We're going to show the current pay period start date, the next pay period start date, and the previous two dates.
      Dim jnr As New JsonNetResult
      jnr.Data = GetPayPeriodStartList()
      jnr.JsonRequestBehavior = JsonRequestBehavior.AllowGet
      Return jnr
    End Function

    Public Function DepartmentList() As JsonNetResult
      Dim tca As Timecard_Access = GetTimeCardAccess(Request.LogonUserIdentity.Name)
      Dim dl As List(Of FinplusDepartment) = myCache.GetItem("departmentList") 'HttpContext.Cache("departmentList")
      Dim jnr As New JsonNetResult
      jnr.Data = dl
      jnr.JsonRequestBehavior = JsonRequestBehavior.AllowGet
      Return jnr
    End Function

    Public Function Access(Optional EmployeeId As Integer = 0) As JsonNetResult
      ' This function returns a timecard_access object for EmployeeId, or the current user if
      ' no employeeId was provided.  
      ' It also checks their access to make sure they have the necessary access to view this data.
      ' We need to make sure that the requesting user has access to view this user's access.
      Dim jnr As New JsonNetResult
      Dim myAccess As Timecard_Access = GetTimeCardAccess(Request.LogonUserIdentity.Name)

      If EmployeeId = 0 Then
        jnr.Data = myAccess
      Else
        If myAccess.Backend_Reports_Access Then
          jnr.Data = Timecard_Access.GetTimeCardAccess(EmployeeId)
        Else
          jnr.Data = "Error, Not Authorized"
        End If
      End If

      jnr.JsonRequestBehavior = JsonRequestBehavior.AllowGet
      Return jnr
    End Function

    <HttpPost>
    Public Function SaveAccess(Raw As Raw_Timecard_Access) As JsonNetResult
      ' We need to make sure that the requesting user has access to save this user's access.

      Dim jnr As New JsonNetResult
      jnr.Data = "Error"
      Dim User As String = Request.LogonUserIdentity.Name
      Dim myAccess As Timecard_Access = GetTimeCardAccess(User)
      If myAccess.Backend_Reports_Access Then
        Dim newTCA As New Timecard_Access(Raw, Request)
        If newTCA.Save Then jnr.Data = "Success"
      End If

      Return jnr
    End Function

    <HttpPost>
    Public Function ApproveInitial(AD As Approval_Data) As JsonNetResult
      Return Approve(AD, 1)
    End Function

    <HttpPost>
    Public Function ApproveFinal(AD As Approval_Data) As JsonNetResult
      Return Approve(AD, 2)
    End Function

    Private Function GetApprovalResult(ByRef AD As Approval_Data, ByRef ApprovalType As Integer, ByRef R As HttpRequestBase) As String
      ' This function is going to do the most common approval checks and provide the appropriate 
      ' message on return.
      If Compare_WorkTypeLists(AD.EmployeeID, AD.ViewPayPeriodEnding, AD.WorkTypeList) Then
        If Not Check_if_Already_Approved(AD.EmployeeID, AD.ViewPayPeriodEnding, ApprovalType) Then
          If Approve_Payperiod(R, AD.EmployeeID, AD.ViewPayPeriodEnding, ApprovalType) Then
            AD.Save_Note(AD.EmployeeID, AD.ViewPayPeriodEnding, ApprovalType, R.LogonUserIdentity.Name)
            Return "Success"
          Else
            If ApprovalType = 2 Then
              Return "Error: You are attempting to do the final approval on an employee that does not have an initial approval.  Please refresh and try again. "
            Else
              Return "Error: An unknown error occurred.  Please try again and submit a helpdesk ticket if this error persists."
            End If

          End If
        Else
          Return "Error: Already approved."
        End If
      Else
        Return "Error: Hours have changed, please refresh and try again."
      End If
    End Function

    <HttpPost>
    Private Function Approve(AD As Approval_Data, approvalType As Integer) As JsonNetResult
      ' Need to check to make sure their time hasn't changed since it was sent to them to view.
      ' Initial approvals are done from the timecard
      ' Final Approvals are done via the Approve Requests menu.

      ' Here's the way this should work:
      ' If someone is viewing their own timesheet and they want to do an initial approval, 
      ' we should always let them.
      ' If this is an initial approval, we should check their access to make sure they have
      ' the authority to approve this timecard.  If they do, we should also check if their
      ' approvals require a second approval.  If a second approval is not required, we should update
      ' both Initial and Final at the same time.
      ' If this is a final approval, we should check their access to make sure they have
      ' the authority to approve this timecard.  

      Dim myEID As Integer = AD_EmployeeData.GetEmployeeIDFromAD(Request.LogonUserIdentity.Name)
      Dim myTca = Timecard_Access.GetTimeCardAccess(myEID)
      'Dim myTca As New Timecard_Access(myEID, Request)

      Dim jnr As New JsonNetResult
      jnr.JsonRequestBehavior = JsonRequestBehavior.DenyGet
      ' First let's make sure we're not past the cutoff

      'If Today = GetPayPeriodStart(Today) AndAlso
      '  AD.PayPeriodStart = Today.AddDays(-14) AndAlso
      '  Now.Hour > 9 Then
      If IsItPastCutoffDate(AD.PayPeriodStart) Then
        '  AndAlso
        '(AD.PayPeriodStart = Today.AddDays(-14)) Or
        'Today.Subtract(AD.PayPeriodStart.AddDays(13)).TotalDays > 1) 

        jnr.Data = "Error: Unable to save this approval. Approvals must be completed by 10 AM on the first day of the new pay period."
      Else
        Select Case approvalType
          Case 1 ' Initial Approval
            Dim Approval As Integer = 1
            If Not myTca.RequiresApproval Then Approval = 3

            If myEID = AD.EmployeeID Then ' They are trying to initial approve their own timecard
              jnr.Data = GetApprovalResult(AD, Approval, Request)

            Else ' Someone else is trying to approve this user's time
              If Not myTca.Check_Access_To_EmployeeId(AD.EmployeeID) OrElse
                      myTca.DepartmentsToApprove.Contains("VIEW") OrElse
                      myTca.DepartmentsToApprove.Contains("LEAVE") Then
                jnr.Data = "Error: Unauthorized"
              Else
                jnr.Data = GetApprovalResult(AD, Approval, Request)
              End If
            End If

          Case 2 ' Final approval

            If AD.Initial_Approval_By_EmployeeID = 0 Then
              jnr.Data = "Error: No initial approval found."
            Else
              If (myEID = AD.EmployeeID Or myEID = AD.Initial_Approval_By_EmployeeID) Then
                ' You can't approve your own final approval
                jnr.Data = "Error: You can't approve your own time or time you have initially approved in this manner."
              ElseIf Not mytca.Check_Access_To_EmployeeId(AD.EmployeeID) OrElse
                      myTca.DepartmentsToApprove.Contains("VIEW") OrElse
                      myTca.DepartmentsToApprove.Contains("LEAVE") Then
                jnr.Data = "Error: Unauthorized"
              Else
                jnr.Data = GetApprovalResult(AD, approvalType, Request)
              End If
            End If

        End Select

      End If
      Return jnr
    End Function

    Private Function Compare_WorkTypeLists(EmployeeId As Integer, PPD As Date, WTL2 As List(Of GenericTimecard.WorkType)) As Boolean
      Dim gtc As New GenericTimecard(PPD, EmployeeId)
      Return Compare_WorkTypeLists((From wt In gtc.calculatedTimeList Where wt.hours > 0 Select wt).ToList,
                                   (From wt In WTL2 Where wt.hours > 0 Select wt).ToList)
    End Function

    Private Function Compare_WorkTypeLists(WTL1 As List(Of GenericTimecard.WorkType), WTL2 As List(Of GenericTimecard.WorkType)) As Boolean
      For Each w In WTL1
        If (From wtl In WTL2 Where wtl.payCode = w.payCode And wtl.payRate = w.payRate _
            And wtl.hours = w.hours Select wtl).Count <> 1 Then Return False
      Next
      For Each w In WTL2
        If (From wtl In WTL1 Where wtl.payCode = w.payCode And wtl.payRate = w.payRate _
            And wtl.hours = w.hours Select wtl).Count <> 1 Then Return False
      Next
      Return True
    End Function

    <HttpPost>
    Public Function TimeclockData(WorkDate As Date) As JsonNetResult
      Dim tca As Timecard_Access = GetTimeCardAccess(Request.LogonUserIdentity.Name)
      Dim jnr As New JsonNetResult
      If tca.Raw_Access_Type < Timecard_Access.Access_Types.Department_1 Then
        jnr.Data = ""
      Else
        jnr.Data = Timeclock_Data.View(WorkDate, tca.EmployeeID)
      End If
      Return jnr
    End Function

    Private Function GetApprovalData(type As String, ppdIndex As Integer, Optional unRestricted As Boolean = False) As JsonNetResult
      ' Unrestricted is used when we want to allow someone to view their staff's
      ' data regardless of who approved it.
      ' this is important for the Signature Required report and the Fema report,
      ' as well as the unapproved report.
      Dim payperiodstart As Date
      If ppdIndex = 0 Then
        payperiodstart = GetPayPeriodStart(Today)
        If Today = payperiodstart And Now.Hour < PayPeriodEndingCutoff Then payperiodstart = GetPayPeriodStart(Today.AddDays(-1))
      Else
        payperiodstart = GetPayPeriodStart(Today.AddDays(ppdIndex * 14))
      End If
      Dim payperiodend As Date = payperiodstart.AddDays(13)
      Dim tca As Timecard_Access = GetTimeCardAccess(Request.LogonUserIdentity.Name)

      'Dim tca As Timecard_Access = GetTimeCardAccess(Request.LogonUserIdentity.Name)

      Dim tmpTC As List(Of GenericTimecard) = GetTimeCards(payperiodstart)

      Dim approvalLevel As Integer = 5
      Dim adl As List(Of GenericTimecard)
      If tca.Raw_Access_Type = Timecard_Access.Access_Types.All_Access Then
        If unRestricted Then
          'Order By t.departmentNumber, t.employeeID
          adl = (From t In tmpTC
                 Order By t.employeeID Descending
                 Select t).ToList
        Else
            adl = (From t In tmpTC
                   Order By t.departmentNumber, t.employeeID
                   Where t.employeeID <> tca.EmployeeID
                   Select t).ToList
        End If

      Else
        If unRestricted Then
          adl = (From t In tmpTC
                 Order By t.departmentNumber, t.employeeID
                 Where t.Approval_Level < approvalLevel And
                   t.Access_Type < tca.Raw_Access_Type Select t).ToList
        Else
          adl = (From t In tmpTC
                 Order By t.departmentNumber, t.employeeID
                 Where t.Approval_Level < approvalLevel And
                   t.Initial_Approval_EmployeeID_Access_Type < tca.Raw_Access_Type And
                   t.Initial_Approval_EmployeeID <> tca.EmployeeID And
                   t.Access_Type < tca.Raw_Access_Type
                 Select t).ToList
        End If

      End If
      'Order By a.department, a.lastName
      adl = (From a In adl
             Order By a.employeeID Descending
             Select a).ToList


      Dim jnr As New JsonNetResult
      jnr.Data = New List(Of String)
      If Not tca.DepartmentsToApprove.Contains("LEAVE") Then

        If tca.Raw_Access_Type = Timecard_Access.Access_Types.All_Access Then
          jnr.Data = adl

        Else
          If tca.DepartmentsToApprove.Contains("ALL") Or
            tca.DepartmentsToApprove.Contains("VIEW") Then

            jnr.Data = (From a In adl
                        Where a.Access_Type <= tca.Raw_Access_Type
                        Select a).ToList

          ElseIf tca.DepartmentsToApprove.Count > 0 Then

            jnr.Data = (From a In adl
                        Where (tca.DepartmentsToApprove.Contains(a.departmentNumber) Or
                          tca.ReportsToList.Contains(a.employeeID)) And
                          a.Access_Type < tca.Raw_Access_Type
                        Select a).ToList

          Else
            jnr.Data = (From a In adl
                        Where tca.ReportsToList.Contains(a.employeeID)
                        Select a).ToList

          End If
        End If
      End If
      jnr.JsonRequestBehavior = JsonRequestBehavior.AllowGet
      Return jnr
    End Function

    <HttpPost>
    <OutputCache(VaryByParam:="*", Duration:=0, NoStore:=True)>
    Public Function Unapproved(Optional ppdIndex As Integer = 0) As JsonNetResult
      Return GetApprovalData("initial", ppdIndex)
    End Function

    <HttpPost>
    <OutputCache(VaryByParam:="*", Duration:=0, NoStore:=True)>
    Public Function InitiallyApproved(Optional ppdIndex As Integer = 0) As JsonNetResult
      Return GetApprovalData("final", ppdIndex)
    End Function

    <HttpPost>
    <OutputCache(VaryByParam:="*", Duration:=0, NoStore:=True)>
    Public Function UnrestrictedInitiallyApproved(Optional ppdIndex As Integer = 0) As JsonNetResult
      Return GetApprovalData("final", ppdIndex, True)
    End Function

    <HttpGet>
    Public Function Incentives(incentiveType As Integer) As JsonNetResult
      'Get_All_Incentive_Data

      ' This function returns a timecard_access object for EmployeeId, or the current user if
      ' no employeeId was provided.  
      ' It also checks their access to make sure they have the necessary access to view this data.
      ' We need to make sure that the requesting user has access to view this user's access.
      Dim jnr As New JsonNetResult
      Dim myAccess As Timecard_Access = GetTimeCardAccess(Request.LogonUserIdentity.Name)

      If myAccess.Backend_Reports_Access Then
        jnr.Data = (From d In Incentive.Get_All_Incentive_Data()
                    Order By d.Incentive_Amount Descending
                    Where d.Incentive_Type = incentiveType
                    Select d).ToList
      Else
        jnr.Data = "Error, Not Authorized"
      End If

      jnr.JsonRequestBehavior = JsonRequestBehavior.AllowGet
      Return jnr
    End Function

    <HttpPost>
    Public Function Incentives(I As List(Of Incentive)) As JsonNetResult
      Dim jnr As New JsonNetResult
      Dim myAccess As Timecard_Access = GetTimeCardAccess(Request.LogonUserIdentity.Name)

      If myAccess.Backend_Reports_Access Then
        If Incentive.Save_Incentive_Data(I) Then
          jnr.Data = "Success"
        Else
          jnr.Data = "Error, There was an error saving the data.  Please try again and check with MIS if the problem persists."
        End If
      Else
        jnr.Data = "Error, Not Authorized"
      End If
      jnr.JsonRequestBehavior = JsonRequestBehavior.DenyGet
      Return jnr

    End Function

    <HttpPost>
    Public Function SaveNoteToPayPeriod(EmployeeID As Integer, Note As String, PPE As Date) As JsonNetResult
      Dim jnr As New JsonNetResult
      jnr.JsonRequestBehavior = JsonRequestBehavior.DenyGet
      jnr.Data = ""
      Dim tca As Timecard_Access = GetTimeCardAccess(Request.LogonUserIdentity.Name)
      If tca.EmployeeID <> EmployeeID Then
        'Dim reqTCA As Timecard_Access = GetTimeCardAccess(Request.LogonUserIdentity.Name)
        If Not tca.Check_Access_To_EmployeeId(EmployeeID) Then
          jnr.Data = "Error: Unauthorized"
        End If
      End If
      If jnr.Data.ToString.Length = 0 Then
        If Add_Timestore_Note(EmployeeID, PPE, Note, Request.LogonUserIdentity.Name) Then
          jnr.Data = "Note Saved."
        Else
          jnr.Data = "There was an error attempting to add this note. "
        End If
      End If
      Return jnr
    End Function

    <HttpPost>
    Public Function SaveHolidays(hr As HolidayRequest) As JsonNetResult

      ' Employee ID is their employee Number
      ' ppdIndex is the index of the pay period from the current pay period. 
      '   Ie: The current pay period is 0.  The next pay period is 1, the previous pay period is -1
      ' Current Holiday Choice is either empty, Bank, or Paid.
      ' HolidayHoursPaid lets the user get paid in minimum increments for their banked holiday hours.
      '   If they don't have the hours, they can't request to be paid for them.
      If hr.CurrentHolidayChoice Is Nothing Then hr.CurrentHolidayChoice = New String() {}
      Dim jnr As New JsonNetResult
      jnr.JsonRequestBehavior = JsonRequestBehavior.DenyGet
      ' Let's make sure that payperiodstart is valid
      'Dim bPPCheck As Boolean = False
      'Dim startPPD As Integer = 0
      'If Today = GetPayPeriodStart(Today) And Now.Hour < 10 Then
      '  startPPD = -1
      'End If
      'For a As Integer = startPPD To 5
      '  Dim tmp As Date = GetPayPeriodStart(Today.AddDays(a * 14))
      '  If tmp = hr.PayPeriodStart Then
      '    bPPCheck = True
      '    Exit For
      '  End If
      'Next
      'If Not bPPCheck Then
      '  jnr.Data = "Error: Invalid Pay Period Start Date."
      '  Return jnr
      'End If
      'Dim payperiodend As Date = hr.PayPeriodStart.AddDays(13)

      Dim tca As Timecard_Access = GetTimeCardAccess(Request.LogonUserIdentity.Name)

      ' First let's make sure we're not past the cutoff

      'If (Today = hr.PayPeriodStart.AddDays(14) AndAlso Now.Hour > 10) Or Today.Subtract(payperiodend).TotalDays > 1 Then
      If IsItPastCutoffDate(hr.PayPeriodStart) Then
        jnr.Data = "Error: Unable to save this request. All entries must be completed by 11 AM on the first day of the new pay period."
      Else

        If tca.EmployeeID = hr.EmployeeID OrElse
          tca.Check_Access_To_EmployeeId(hr.EmployeeID) Then ' They are working on their own Holiday hours.

          Dim tc As New GenericTimecard(hr.PayPeriodStart, hr.EmployeeID)

          If tc.bankedHoliday < hr.BankedHolidaysPaid Then
            jnr.Data = "Error: Unable to save this request. You only have " & tc.bankedHoliday & " hours banked and are trying to use " & hr.BankedHolidaysPaid & "."
          Else
            If tc.HolidaysInPPD.Length = 0 AndAlso
              hr.CurrentHolidayChoice.Length > 0 Then
              jnr.Data = "Error: Unable to save this request. There is no holiday found in this pay period."
            Else

              If tc.HolidaysInPPD.Length = 0 AndAlso
                (From h In hr.CurrentHolidayChoice
                 Where h.ToUpper = "NONE"
                 Select h).Count <> hr.CurrentHolidayChoice.Length Then
                jnr.Data = "Error: Unable to save this request. You do not have access to bank Holiday time."
              Else
                If Compare_Holiday_Request_To_Timecard(hr, tc) Then
                  jnr.Data = "Your holiday choices have been saved. You do not need to reapprove your time."
                Else
                  If Not Update_Holiday_Data(hr.CurrentHolidayChoice,
                                             hr.BankedHolidaysPaid,
                                             tc,
                                             Request.LogonUserIdentity.Name) Then
                    jnr.Data = "Error: There was an error attempting to save this request.  Please try again and contact MIS if the problem recurrs."
                  Else
                    jnr.Data = "Your Holiday choices have been saved.  If you have already approved your time, you will need to reapprove it."
                  End If
                End If

              End If
            End If

          End If

        Else
          jnr.Data = "Error: Unauthorized"
        End If
      End If
      Return jnr
    End Function

    <HttpPost>
    Public Function SaveTimecardDay(SavedTCTD As TimecardTimeData) As ActionResult
      ' This function will take a TimecardTimeData and save it to the database for this employee
      ' It will also verify that it looks correct, and will not let the user save an invalid amount of time.
      ' it will also make sure that the user has access to do this for this employeeId
      If IsItPastCutoffDate(SavedTCTD.WorkDate) Then
        Return New HttpStatusCodeResult(403)
      Else
        Dim myEID As Integer = AD_EmployeeData.GetEmployeeIDFromAD(Request.LogonUserIdentity.Name)
        'Dim myTca As New Timecard_Access(myEID, Request)
        Dim myTca = Timecard_Access.GetTimeCardAccess(myEID)
        If Not SavedTCTD.Validate(myTca, Request.LogonUserIdentity.Name) Then Return New HttpStatusCodeResult(400)
        If myTca.Check_Access_To_EmployeeId(SavedTCTD.EmployeeID) Then
          If myTca.EmployeeID <> SavedTCTD.EmployeeID AndAlso
                    (myTca.DepartmentsToApprove.Contains("VIEW") Or
                    myTca.DepartmentsToApprove.Contains("LEAVE")) Then
            Return New HttpStatusCodeResult(403)
          Else
            'If Save_Timestore_Data(SavedTCTD, myTca) Then
            If SavedTCTD.Save(myTca) Then
              Return New HttpStatusCodeResult(200)
            Else
              Return New HttpStatusCodeResult(500)
            End If
          End If
        Else
          Return New HttpStatusCodeResult(403)
        End If

      End If
    End Function

    Public Function SaveCompTimeChoices(EmployeeID As Integer, PayPeriodEnding As Date,
                                        Week1 As Double, Week2 As Double) As ActionResult

      If IsItPastCutoffDate(PayPeriodEnding) Then
        Return New HttpStatusCodeResult(403)
      Else
        Dim myEID As Integer = AD_EmployeeData.GetEmployeeIDFromAD(Request.LogonUserIdentity.Name)
        'Dim myTca As New Timecard_Access(myEID, Request)
        Dim myTca = Timecard_Access.GetTimeCardAccess(myEID)
        If myTca.Check_Access_To_EmployeeId(EmployeeID) Then
          If myTca.EmployeeID <> EmployeeID AndAlso
              (myTca.DepartmentsToApprove.Contains("VIEW") Or
              myTca.DepartmentsToApprove.Contains("LEAVE")) Then
            Return New HttpStatusCodeResult(403)
          Else
            If Save_Comp_Time_Earned(EmployeeID, PayPeriodEnding, Week1, Week2, myTca) Then
              Return New HttpStatusCodeResult(200)
            Else
              Return New HttpStatusCodeResult(500)
            End If
          End If
        Else
          Return New HttpStatusCodeResult(403)
        End If
      End If
    End Function

    <HttpPost>
    Public Function Leave_Requests_By_Employee(employeeId As Integer) As JsonNetResult
      ' We get the employeeID from the username
      Return get_leave_requests(employeeId)
    End Function

    <HttpPost>
    Public Function All_Leave_Requests_By_Employee(employeeId As Integer) As JsonNetResult
      ' We get the employeeID from the username
      Return get_leave_requests(employeeId, "", CType("1/1/2015", Date))
    End Function

    <HttpPost>
    Public Function Leave_Requests_By_Department() As JsonNetResult
      Dim tca As Timecard_Access = GetTimeCardAccess(Request.LogonUserIdentity.Name)
      Dim eid As Integer = AD_EmployeeData.GetEmployeeIDFromAD(Request.LogonUserIdentity.Name)
      Dim f As List(Of FinanceData) = GetEmployeeDataFromFinPlus(eid)
      If f.Count = 1 Then
        Dim dept As String = f.First.Department
        If tca.Raw_Access_Type > Timecard_Access.Access_Types.User_Only Then
          If tca.DepartmentsToApprove.Count = 0 Then
            Return get_leave_requests(tca.EmployeeID, dept, Nothing, True)
          End If
        End If
        Return get_leave_requests(0, dept)

      Else
        ' if we don't get the expected information from finplus, let's just return this user's leave requests.
        Return get_leave_requests(eid)
      End If

    End Function

    <HttpPost>
    Public Function Leave_Requests() As JsonNetResult
      ' This returns all of the leave requests you have access to approve
      Return get_leave_requests()
    End Function

    Private Function get_leave_requests(Optional employeeId As Integer = 0,
                                        Optional deptId As String = "",
                                        Optional StartDate As Date = Nothing,
                                        Optional IncludeReportsTo As Boolean = False) As JsonNetResult

      Dim payperiodstart As Date
      If StartDate > Date.MinValue Then
        payperiodstart = GetPayPeriodStart(StartDate)
      Else
        payperiodstart = GetPayPeriodStart(Today)
        If Today = payperiodstart And Now.Hour < PayPeriodEndingCutoff Then
          payperiodstart = GetPayPeriodStart(Today.AddDays(-1))
        End If

      End If
        Dim bUseDept As Boolean = (deptId.Length > 0)
      Dim tca As Timecard_Access = GetTimeCardAccess(Request.LogonUserIdentity.Name)
      Dim jnr As New JsonNetResult

      ' Here we create a LeaveCalendarData object to communicate the user's dept back to them.
      ' What a pain.
      Dim myLC As New LeaveCalendarData
      myLC.MyDept = deptId
      ' Here, we want to make a determination if we passed a deptId, we want to eliminate that Id so we can 
      ' filter by depts later.
      If tca.DepartmentsToApprove.Count > 0 AndAlso deptId.Length > 0 Then deptId = ""

      Dim hta As List(Of Hours_To_Approve_Display) = Hours_To_Approve_Display.GetHoursToApproveForDisplay(payperiodstart, employeeId, deptId, IncludeReportsTo)
      If employeeId > 0 And tca.EmployeeID = employeeId Then
        ' We are restricting the download to just one EmployeeID,
        ' this is used on the employee's leave request page.
        myLC.leaveData = hta
      ElseIf employeeId = 0 And bUseDept Then
        If tca.DepartmentsToApprove.Contains("ALL") Or deptId.Length > 0 Then
          myLC.leaveData = hta
        Else
          'myLC.leaveData = (From a In hta Where (tca.DepartmentsToApprove.Contains(myLC.MyDept) Or
          '                  a.reports_to = tca.EmployeeID Or a.dept_id = myLC.MyDept) Select a).ToList
          Select Case myLC.MyDept
            Case "1805", "3701A"
              myLC.leaveData = (From a In hta
                                Where (tca.DepartmentsToApprove.Contains(a.dept_id) Or
                                  tca.ReportsToList.Contains(a.employee_id) Or
                                  (a.dept_id = "1805" Or a.dept_id = "3701A"))
                                Select a).ToList
            Case "1804", "1803"
              myLC.leaveData = (From a In hta
                                Where (tca.DepartmentsToApprove.Contains(a.dept_id) Or
                                  tca.ReportsToList.Contains(a.employee_id) Or
                                  (a.dept_id = "1804" Or a.dept_id = "1803"))
                                Select a).ToList
            Case Else
              myLC.leaveData = (From a In hta
                                Where (tca.DepartmentsToApprove.Contains(a.dept_id) Or
                                  tca.ReportsToList.Contains(a.employee_id) Or
                                  a.dept_id = myLC.MyDept) Select a).ToList
          End Select


        End If

      Else

        If tca.DepartmentsToApprove.Contains("ALL") Then
          myLC.leaveData = (From a In hta Where a.access_type <= tca.Raw_Access_Type _
                      And a.employee_id <> tca.EmployeeID Select a).ToList
        ElseIf tca.DepartmentsToApprove.Count > 0 Then
          'myLC.leaveData = (From a In hta Where (tca.DepartmentsToApprove.Contains(a.dept_id) _
          '            Or a.reports_to = tca.EmployeeID) And a.employee_id <> tca.EmployeeID _
          '            And a.access_type < tca.Raw_Access_Type Select a).ToList
          myLC.leaveData = (From a In hta
                            Where (tca.DepartmentsToApprove.Contains(a.dept_id) Or
                              tca.ReportsToList.Contains(a.employee_id)) And
                              a.employee_id <> tca.EmployeeID And
                              a.access_type < tca.Raw_Access_Type Select a).ToList
        ElseIf tca.ReportsToList.Count > 0 Then
          myLC.leaveData = (From a In hta
                            Where tca.ReportsToList.Contains(a.employee_id) And
                              a.employee_id <> tca.EmployeeID And
                              a.access_type < tca.Raw_Access_Type Select a).ToList
        End If
      End If
      jnr.Data = myLC
      Return jnr
    End Function

    Public Function Update_Leave_Request(employeeId As Integer,
                                         approved As Boolean,
                                         id As Long,
                                         workdate As Date,
                                         hours As Double,
                                         Note As String) As ActionResult
      ' This function will take some information from the browser based on the user's choices
      ' and approve a leave request, provided the user has access.  
      ' HTTP Status codes used to return:
      ' 200 - Everything working as expected
      ' 403 - You don't have access to approve this person's leave.
      ' 500 - An unknown error occurred
      ' 501 - The hours have changed or the leave requests no longer exists.
      ' If the leave request is denied, we will also look up the user's timecard to force an hours recalculation.
      If IsItPastCutoffDate(workdate) Then
        Return New HttpStatusCodeResult(403)
      Else

        Dim myEID As Integer = AD_EmployeeData.GetEmployeeIDFromAD(Request.LogonUserIdentity.Name)
        'Dim myTca As New Timecard_Access(myEID, Request)
        Dim myTca = Timecard_Access.GetTimeCardAccess(myEID)
        If employeeId = myTca.EmployeeID OrElse myTca.DepartmentsToApprove.Contains("VIEW") Then Return New HttpStatusCodeResult(403)
        If myTca.Check_Access_To_EmployeeId(employeeId) Then
          Select Case Finalize_Leave_Request(approved, id, hours, Note, myTca)
            'Case -5
            '  Return New HttpStatusCodeResult(501)
            Case > 0
              If Not approved AndAlso workdate < GetPayPeriodStart(Today).AddDays(14) Then Dim tc As New GenericTimecard(employeeId)
              Return New HttpStatusCodeResult(200)
            Case Else ' includes -1 for sql errors
              Return New HttpStatusCodeResult(500)
          End Select
        Else
          Return New HttpStatusCodeResult(403)
        End If

      End If

    End Function

    Public Function Approve_Bulk_Leave_Requests(ids As List(Of Long)) As ActionResult
      ' This function will take some information from the browser based on the user's choices
      ' and approve a leave request, provided the user has access.  
      ' HTTP Status codes used to return:
      ' 200 - Everything working as expected
      ' 403 - You don't have access to approve this person's leave.
      ' 500 - An unknown error occurred
      ' 501 - The hours have changed or the leave requests no longer exists.
      ' If the leave request is denied, we will also look up the user's timecard to force an hours recalculation.
      Dim employeeIds As List(Of Integer) = Get_Leave_Request_EmployeeIds(ids)
      If employeeIds.Count = 0 Then
        ' if there are no employees, we're going to just return success.
        ' but we may want to figure out if we should indicate to the user that no approvals were done
        ' probably because it was past the cutoff or they were already approved.
        Return New HttpStatusCodeResult(200)
      End If
      Dim myEID As Integer = AD_EmployeeData.GetEmployeeIDFromAD(Request.LogonUserIdentity.Name)
      Dim myTca = Timecard_Access.GetTimeCardAccess(myEID)
      If employeeIds.Contains(myEID) OrElse myTca.DepartmentsToApprove.Contains("VIEW") Then Return New HttpStatusCodeResult(403)

      For Each id In employeeIds
        If Not myTca.Check_Access_To_EmployeeId(id) Then
          Return New HttpStatusCodeResult(403)
        End If
      Next
      ' if they make it here, we'll do the bulk approval
      Dim i As Integer = Bulk_Approve_Leave_Requests(ids, myTca)
      Select Case i
        Case 0
          Return New HttpStatusCodeResult(200)
        Case -1
          Return New HttpStatusCodeResult(500)
        Case Else ' greater than 0 falls here
          Return New HttpStatusCodeResult(200)
      End Select
    End Function

    <HttpPost>
    Public Function GetHolidays() As JsonNetResult
      Dim dList As New List(Of Date)
      ' this gets the holidays for last year, the current year, and next year.
      For a As Integer = -1 To 1 Step 1
        dList.AddRange(getHolidayList(Now.Year + a))
      Next
      Dim jnr As New JsonNetResult
      jnr.Data = dList
      Return jnr
    End Function

    <HttpPost>
    Public Function GetBirthdays() As JsonNetResult
      Dim eid As Integer = AD_EmployeeData.GetEmployeeIDFromAD(Request.LogonUserIdentity.Name)
      Dim fl As List(Of FinanceData) = GetEmployeeDataFromFinPlus(eid)
      Dim aded As Dictionary(Of Integer, AD_EmployeeData) = AD_EmployeeData.GetCachedEmployeeDataFromAD() 'GetADEmployeeData()
      Dim tca As Timecard_Access = GetTimeCardAccess(Request.LogonUserIdentity.Name)
      Dim baseBdayList As List(Of Namedday) = Namedday.GetAllCachedBirthdays()
      Dim jnr As New JsonNetResult
      ' This section of code is for finding active employees that are in Finplus
      ' but aren't in Active Directory.
      Try

        If fl.Count = 1 Then
          Dim dept As String = fl.First.Department
          'fl = GetCachedEmployeeDataFromFinplus()

          'Dim flad = (From f In fl
          '            Where Not f.IsTerminated And
          '              f.BirthDate <> Date.MinValue And
          '              aded.ContainsKey(f.EmployeeId)
          '            Select f.BirthDate, f.Department, f.EmployeeId)
          Dim bdayList As List(Of Namedday)
          If tca.DepartmentsToApprove.Contains("ALL") Then
            'bdayList = (From f In flad Select f.Department, f.Name, f.BirthDate)

            bdayList = (From b In baseBdayList Select b).ToList()

          ElseIf tca.DepartmentsToApprove.Count > 0 Then
            bdayList = (From b In baseBdayList
                        Where tca.DepartmentsToApprove.Contains(b.Dept) Or
                          b.Dept = dept
                        Select b).ToList()

            'bdayList = (From f In flad
            '            Where tca.DepartmentsToApprove.Contains(f.Department) And
            '            f.Department = dept
            '            Select New Namedday(aded(f.EmployeeId).Name, f.BirthDate, f.Department)).ToList

          Else
            'bdayList = (From f In flad Where f.Department = dept
            '            Select New Namedday(aded(f.EmployeeId).Name, f.BirthDate, f.Department)).ToList
            bdayList = (From b In baseBdayList
                        Where b.Dept = dept
                        Select b).ToList()
          End If

          'Dim ndList As New List(Of Namedday)
          'For Each b In bdayList
          '  ndList.AddRange(b.ToList)
          'Next
          'jnr.Data = ndList
          jnr.Data = bdayList
        Else
          ' if we don't get the expected information from finplus, let's just return this user's leave requests.
          jnr.Data = ""
        End If
        Return jnr
      Catch ex As Exception
        Dim e As New ErrorLog(ex, "")
        jnr.Data = ""
        Return jnr
      End Try
    End Function

  End Class


End Namespace

