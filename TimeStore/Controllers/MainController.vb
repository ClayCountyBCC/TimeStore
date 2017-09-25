Imports System.Web.Mvc
Imports System.Security.Cryptography
Imports TimeStore.Models
Imports System.Runtime.Caching

Namespace Controllers
  Public Class MainController
    Inherits Controller

    Private cache As myCache
    Private defaultCIP As New CacheItemPolicy()

    Private Function GetTimeCardAccess(EmployeeId As Integer) As Timecard_Access
      Dim key As String = "tca," & EmployeeId
      defaultCIP.AbsoluteExpiration = Now.AddHours(12)
      Return myCache.GetItem(key, defaultCIP)
    End Function

    Private Function GetTimeCardAccess(UserName As String) As Timecard_Access
      Return GetTimeCardAccess(GetEmployeeIDFromAD(UserName))
    End Function

    ' GET: Main
    Function Index(Optional ByVal ppd As Integer = 0) As ActionResult
      'UpdateCurrentLeaveBank("8/27/2014")
      'Dim dTmp As Date = GetPayPeriodStart(Today)
      Dim tca As Timecard_Access = GetTimeCardAccess(Request.LogonUserIdentity.Name)
      If Not tca.Backend_Reports_Access Then Return New HttpUnauthorizedResult
      Dim dtmp As Date = GetPayPeriodStart(Today.AddDays(ppd * 14))
      'If Today = dTmp And Now.Hour < 12 Then dTmp = GetPayPeriodStart(Today.AddDays(-1))
      'UpdateCurrentLeaveBank(Today)
      'Dim Tmp = GetPublicSafetyEmployeeData_EPP(dtmp).OrderBy(Function(b) b.EmployeeData.Department) _
      '                                .ThenBy(Function(n) n.EmployeeData.EmployeeLastName) _
      '                                .ThenBy(Function(j) j.EmployeeData.EmployeeFirstName).ToList()
      'Dim e As List(Of EmployeeOutput) = EPP_To_Output(Tmp)
      Dim depts() As String = {"1703", "2103"} ' Removed 2102 "2102",
      Dim TmpTC As List(Of GenericTimecard) = GetTimeCards(dtmp)
      Dim t = (From tc In TmpTC Where depts.Contains(tc.departmentNumber) Select tc) _
                    .OrderBy(Function(b) b.departmentNumber) _
                    .ThenBy(Function(n) n.lastName) _
                    .ThenBy(Function(j) j.firstName).ToList
      Dim e As List(Of EmployeeOutput) = GenericTimeCard_To_Output(t)
      tca.PayPeriodDisplayDate = dtmp.ToShortDateString
      tca.EmployeeOutputList = e
      Return View(tca)
    End Function

    Function Exceptions(Optional ByVal ppd As Integer = 0, Optional et As String = "") As ActionResult
      Dim tca As Timecard_Access = GetTimeCardAccess(Request.LogonUserIdentity.Name)
      If Not tca.Backend_Reports_Access Then Return New HttpUnauthorizedResult
      Dim dtmp As Date = GetPayPeriodStart(Today.AddDays(ppd * 14))
      ' If we're looking at this before Noon, we should default to the current payperiod.
      If dtmp = Today And Now.Hour < PayPeriodEndingCutoff And ppd = 0 Then dtmp = GetPayPeriodStart(Today.AddDays(-1))
      Dim telist As List(Of TimecardTimeException) = Get_All_Timecard_Exceptions(dtmp)

      If et.Length > 0 Then
        Select Case et
          Case "PS"
            Dim depts() As String = {"1703", "2103"} ' Removed 2102
            telist = (From t In telist Where depts.Contains(t.Department) Select t).ToList
          Case "PW"
            Dim depts() As String = {"3701A"} ' To be revised
            telist = (From t In telist Where depts.Contains(t.Department) Select t).ToList
          Case "TC"
            Dim depts() As String = {"1703", "2103"} ' Removed 2102
            telist = (From t In telist Where Not depts.Contains(t.Department) Select t).ToList
        End Select
      End If
      tca.TimecardTimeExceptionList = telist
      tca.PayPeriodDisplayDate = dtmp.ToShortDateString
      Return View(tca)
    End Function

    Function Crosstab(Optional ByVal ppd As Integer = 0, Optional et As String = "") As ActionResult
      Dim tca As Timecard_Access = GetTimeCardAccess(Request.LogonUserIdentity.Name)
      If Not tca.Backend_Reports_Access Then Return New HttpUnauthorizedResult
      Dim dtmp As Date = GetPayPeriodStart(Today.AddDays(ppd * 14))
      ' If we're looking at this before Noon, we should default to the current payperiod.
      If dtmp = Today And Now.Hour < PayPeriodEndingCutoff And ppd = 0 Then dtmp = GetPayPeriodStart(Today.AddDays(-1))
      Dim cl As List(Of Crosstab) = GetCrosstabData(dtmp) _
                        .OrderBy(Function(o) IIf(o.orgn = "", "9999", o.orgn)) _
                        .ThenBy(Function(o) IIf(o.LastName = "", "zzzzzzzz", o.LastName)).ToList
      'Dim f = (From c In cl Order By c.orgn, c.LastName.Length > 0, c.LastName Ascending Select c).ToList
      Return View(cl)
    End Function

    Function UploadFinanceData(ppdIndex As Integer) As JsonNetResult
      Dim ppe As Date = GetPayPeriodStart(Today.AddDays(ppdIndex * 14)).AddDays(13)
      Dim jnr As New JsonNetResult
      jnr.JsonRequestBehavior = JsonRequestBehavior.DenyGet
      Dim tca As Timecard_Access = GetTimeCardAccess(Request.LogonUserIdentity.Name)
      If tca.Backend_Reports_Access = False Then
        jnr.Data = "Error: You do not have access to this function."
        Add_Timestore_Note(tca.EmployeeID, ppe, "This user attempted to use the UploadFinanceData function.", Request.LogonUserIdentity.Name)
      Else
        Add_Timestore_Note(tca.EmployeeID, ppe, "Started the Post to Finance Process", Request.LogonUserIdentity.Name)
        Dim UseProduction As Boolean = False
        Select Case Environment.MachineName.ToUpper
          Case "CLAYBCCDV10", "MISLL03", "MISML01"
            UseProduction = False
          Case "CLAYBCCIIS01"
            UseProduction = True
        End Select
        Dim t As Boolean = SavedTimeToFinplusProcess(ppe, UseProduction)
        'Dim t As Boolean = False
        If t Then
          jnr.Data = "Success"
          Add_Timestore_Note(tca.EmployeeID, ppe, "Post to Finance Process has completed.", Request.LogonUserIdentity.Name)
        Else
          jnr.Data = "An Error occurred, please contact MIS for more information."
          Add_Timestore_Note(tca.EmployeeID, ppe, "Post to Finance Process did not complete due to errors.", Request.LogonUserIdentity.Name)
        End If
      End If
      Return jnr
    End Function


  End Class
End Namespace


'Public Function Test()
'    Dim payperiodstart As Date = GetPayPeriodStart(Today)
'    GetTimeCards(payperiodstart)
'    Dim sw As New Stopwatch
'    sw.Start()
'    For a As Integer = 1 To 50
'        GetTimeCards(payperiodstart)
'    Next
'    sw.Stop()
'    Dim s As String = "total calculation time: " & sw.ElapsedMilliseconds / 1000
'    Dim v As String = ""
'End Function
'Function Main() As ViewResult
'    Dim dTmp As Date = GetPayPeriodStart(Today)
'    If Today = dTmp And Now.Hour < 12 Then dTmp = GetPayPeriodStart(Today.AddDays(-1))
'    Dim Tmp = GetPublicSafetyEmployeeData_EPP(dTmp)
'    Dim e As List(Of EmployeeOutput) = EPP_To_Output(Tmp)
'    Return View(e)
'End Function

'Function OldExceptions(Optional ByVal ppd As Integer = 0) As ViewResult
'    Dim dtmp As Date = GetPayPeriodStart(Today.AddDays(ppd * 14))
'    'Dim dTmp As Date = GetPayPeriodStart(Today)
'    'If Today = dTmp And Now.Hour < 12 Then dTmp = GetPayPeriodStart(Today.AddDays(-1))
'    Dim telist As List(Of TelestaffTimeException) = Get_All_Telestaff_Exceptions(dtmp)
'    Dim tca As Timecard_Access = GetTimeCardAccess(Request.LogonUserIdentity.Name)
'    tca.TelestaffTimeExceptionList = telist
'    tca.PayPeriodDisplayDate = dtmp.ToShortDateString
'    Return View(tca)
'End Function
'Private Function GetTimeCardAccess(UserName As String) As Timecard_Access
'    Dim tca As Timecard_Access = HttpContext.Cache("tca_" & UserName)
'    If tca Is Nothing Then
'        tca = New Timecard_Access(GetEmployeeIDFromAD(UserName))
'        HttpContext.Cache.Insert("tca_" & UserName, tca, Nothing, Now.AddHours(1), TimeSpan.Zero)
'    End If
'    If HttpContext.Cache("employeeList") Is Nothing Then
'        HttpContext.Cache.Insert("employeeList", GetEmployeeListFromFinPlus, Nothing, Now.AddHours(12), TimeSpan.Zero)
'    End If
'    Return tca
'End Function
' GET: Main/Details/5
'Function Details(ByVal id As Integer) As ActionResult
'    Return View()
'End Function

'' GET: Main/Create
'Function Create() As ActionResult
'    Return View()
'End Function
'' POST: Main/Create
'<HttpPost()>
'Function Create(ByVal collection As FormCollection) As ActionResult
'    Try
'        ' TODO: Add insert logic here

'        Return RedirectToAction("Index")
'    Catch
'        Return View()
'    End Try
'End Function

'' GET: Main/Edit/5
'Function Edit(ByVal id As Integer) As ActionResult
'    Return View()
'End Function

'' POST: Main/Edit/5
'<HttpPost()>
'Function Edit(ByVal id As Integer, ByVal collection As FormCollection) As ActionResult
'    Try
'        ' TODO: Add update logic here

'        Return RedirectToAction("Index")
'    Catch
'        Return View()
'    End Try
'End Function
'' GET: Main/Delete/5
'Function Delete(ByVal id As Integer) As ActionResult
'    Return View()
'End Function

'' POST: Main/Delete/5
'<HttpPost()>
'Function Delete(ByVal id As Integer, ByVal collection As FormCollection) As ActionResult
'    Try
'        ' TODO: Add delete logic here

'        Return RedirectToAction("Index")
'    Catch
'        Return View()
'    End Try
'End Function