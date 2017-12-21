Namespace Models
  Public Class GenericTimecard

    Public Class Work
      Public Property WorkTime As String = ""
      Public Property Description As String = ""
      Public Sub New(newWork As String, Desc As String)
        WorkTime = newWork
        Description = Desc
      End Sub
    End Class

    Public Class DailyData
      Property workDate As Date
      ReadOnly Property workDateDisplay As String
        Get
          Return workDate.ToShortDateString
        End Get
      End Property
      ReadOnly Property workDateDayOfWeek As String
        Get
          Return workDate.ToString("ddd")
        End Get
      End Property
      Property workTime As String = ""
      Property onCallWorkTime As String = ""
      Property workTimeList As New List(Of Work)
      Property comment As String = ""
      Property workHours As New WorkType()
      Property workHoursList As New List(Of WorkType)
      Public ReadOnly Property totalWorkHours As Double
        Get
          If workHoursList.Count > 0 Then
            Return (From w In workHoursList Select w.hours).Sum
          Else
            Return workHours.hours
          End If
        End Get
      End Property
    End Class

    Public Class WorkType
      Property name As String = ""
      Property payCode As String = ""
      Property hours As Double = 0
      Property leftToRightOrder As Integer = 0
      Property payRate As Double = 0
      ReadOnly Property shortPayRate As Double
        Get
          Return Math.Round(payRate, 2)
        End Get
      End Property

      Public Sub New()

      End Sub

      Public Sub New(NameToUse As String, GH As GroupedHours, LeftToRightOrderToUse As Integer, Optional pr As Double = 0)
        If pr > 0 Then
          payRate = pr
          hours = GH.TotalHours(pr)
        Else
          hours = GH.TotalHours
        End If
        name = NameToUse
        leftToRightOrder = LeftToRightOrderToUse
        payCode = GH.PayCode
      End Sub

      Public Sub New(NameToUse As String, HoursToUse As Double, LeftToRightOrderToUse As Integer, payCodeToUse As String, Optional pr As Double = 0)
        name = NameToUse
        hours = HoursToUse
        leftToRightOrder = LeftToRightOrderToUse
        payRate = pr
        payCode = payCodeToUse
      End Sub

      Public Sub New(HoursToUse As Double, payCodeToUse As String, pr As Double)
        Dim Paycodes As Dictionary(Of String, String) = myCache.GetItem("paycodes")
        Dim ltrOrder As Dictionary(Of String, Integer) = myCache.GetItem("ltrorder")
        name = PayCodes(payCodeToUse)
        hours = HoursToUse
        leftToRightOrder = ltrOrder(payCodeToUse)
        payRate = pr
        payCode = payCodeToUse
      End Sub

    End Class

    Property payPeriodStart As Date
    Public ReadOnly Property payPeriodStartDisplay As String
      Get
        Return payPeriodStart.ToShortDateString
      End Get
    End Property
    Public ReadOnly Property payPeriodEndingDisplay As String
      Get
        Return payPeriodStart.AddDays(13).ToShortDateString
      End Get
    End Property
    Public ReadOnly Property isPubWorks As Boolean
      Get
        Return department = "3701"
      End Get
    End Property
    Property employeeID As String
    Property employeeName As String
    Property lastName As String
    Property firstName As String
    Public ReadOnly Property EmployeeDisplay As String
      Get
        Return lastName & ", " & firstName & " - " & employeeID.ToString
      End Get
    End Property
    Property departmentNumber As String
    Property department As String
    Property GroupName As String = ""
    Property hireDate As Date
    Property TelestaffProfileType As TelestaffProfileType = Models.TelestaffProfileType.Office
    Property classify As String
    Property title As String
    Property Payrate As Double = 0
    'Property HolidayPayRate As Double = 0
    ReadOnly Property shortPayrate As String
      Get
        Return Payrate.ToString("C2")
      End Get
    End Property
    Property isFullTime As Boolean
    Property isHolidayTimeBankable As Boolean = False
    Public ReadOnly Property fullTimeStatus
      Get
        If isFullTime Then
          Return "Full Time"
        Else
          Return "Part Time"
        End If
      End Get
    End Property
    Property isExempt As Boolean
    Public ReadOnly Property exemptStatus
      Get
        If isExempt Then
          Return "Exempt"
        Else
          Return "Non Exempt"
        End If
      End Get
    End Property
    Property foundEmployee As Boolean
    Property scheduledHours As Double
    Property hoursForOvertime As Double
    Property bankedVacation As Double
    Property bankedHoliday As Double
    Property bankedSick As Double
    Property bankedComp As Double
    Property HolidaysInPPD As String()
    Property holidayIncrement As Double = 0
    Property TerminationDate As Date = Date.MaxValue
    Property WarningList As New List(Of String)
    Property ErrorList As New List(Of String)
    Public ReadOnly Property isTerminated As Boolean
      Get
        Return TerminationDate <> Date.MaxValue
      End Get
    End Property
    Public ReadOnly Property TerminationDateDisplay As String
      Get
        Return TerminationDate.ToShortDateString
      End Get
    End Property
    Property RawTime As New List(Of DailyData)
    ReadOnly Property RawTime_Week1 As List(Of DailyData)
      Get
        Return (From r In RawTime
                Order By r.workDate Ascending
                Where r.workDate >= payPeriodStart And
                  r.workDate < payPeriodStart.AddDays(7)
                Select r).ToList
      End Get
    End Property
    ReadOnly Property RawTime_Week2 As List(Of DailyData)
      Get
        Return (From r In RawTime
                Order By r.workDate Ascending
                Where r.workDate > payPeriodStart.AddDays(6)
                Select r).ToList
      End Get
    End Property
    Property timeList As New List(Of WorkType)
    Property calculatedTimeList As New List(Of WorkType)
    Property calculatedTimeList_Week1 As New List(Of WorkType)
    Property calculatedTimeList_Week2 As New List(Of WorkType)
    Property RawTCTD As New List(Of TimecardTimeData)
    Property approvalTimeList As New List(Of WorkType)
    Property Notes As New List(Of Note)
    Property ErrorOccurred As Boolean = False
    Property ErrorText As String = ""
    Property Current_Timecard_Data As New List(Of Saved_Timecard_Data)
    Private _used_Types As New List(Of WorkType)
    Public Property DisasterName_Display As String = ""
    Public Property DisasterPeriodType_Display As Integer = 0
    Public ReadOnly Property IsLeaveApproved As Boolean
      Get
        Return ((From t In RawTCTD Where t.IsApproved = False Select t).Count = 0)
      End Get
    End Property



    ReadOnly Property showAddTime As Boolean
      Get
        If Data_Type = "timecard" Then
          Return (Days_Since_PPE < 2)
        Else
          Return False
        End If
      End Get
    End Property

    ReadOnly Property HolidayHoursChoice As String()
      Get
        ' Here we need to look at the saved data and see if we have any hours under the 134 or 122 pay codes
        ' This function needs to either return "" for codes not found, "Bank" for pay code 122, or "Paid" for pay code 134.
        'Dim choices As List(Of String) = New List(Of String)
        'If Current_Timecard_Data.Count = 0 Then Return choices.ToArray
        'Dim paid = (From c In Current_Timecard_Data
        '            Where c.PayCode = "134" Select "Paid").ToList
        'Dim bank = (From c In Current_Timecard_Data
        '            Where c.PayCode = "122" Select "Bank").ToList
        'If paid.Count > 0 Then choices.AddRange(paid)
        'If bank.Count > 0 Then choices.AddRange(bank)
        'Return choices.ToArray
        Dim choices(HolidaysInPPD.Length - 1) As String
        For a As Integer = 0 To HolidaysInPPD.Length - 1
          choices(a) = "None"
        Next

        If Current_Timecard_Data.Count = 0 OrElse
        HolidaysInPPD.Length = 0 Then Return choices
        Dim HolidayPayCodes As String() = {"134", "122", "800"}

        Dim tmp = (From c In Current_Timecard_Data
                   Where HolidayPayCodes.Contains(c.PayCode)
                   Group By PayCode = c.PayCode Into PaycodeGroup = Group,
                     totalHours = Sum(c.Hours)
                   Select New With {PayCode, totalHours})
        If tmp.Count > 0 Then
          For i As Integer = 0 To choices.GetUpperBound(0)
            Dim hourType As String = ""
            Select Case tmp(i).PayCode
              Case "122"
                hourType = "Bank"
              Case "134"
                hourType = "Paid"
              Case "800"
                hourType = "Ineligible"
            End Select

            Dim val = tmp(i).totalHours / holidayIncrement
            For d As Integer = 1 To val
              choices(i) = hourType
              If d < val Then i += 1
            Next

          Next
        End If
        'For Each pc In tmp
        '  If pc.totalHours > 0 Then
        '    Dim i = pc.totalHours / holidayIncrement ' should be a whole number
        '    Dim hourType As String = ""
        '    Select Case pc.PayCode
        '      Case "122"
        '        hourType = "Bank"
        '      Case "134"
        '        hourType = "Paid"
        '    End Select
        '    For d As Integer = 0 To i
        '      choices.Add(hourType)
        '    Next
        '  Else
        '    choices.Add("Ineligible")
        '  End If
        'Next
        'If HolidaysInPPD.Length > choices.Count Then
        '  For d As Integer = choices.Count To HolidaysInPPD.Length
        '    choices.Add("None")
        '  Next
        'End If



        'If tmp.Count > 0 Then
        '  Dim choice As String = ""
        '  If choices.Length > 1 Then ' only one holiday
        '    If tmp.Count = 1 Then
        '      If (From t In tmp Where t.PayCode = "134" Select t).Count > 0 Then
        '        choice = "Paid"
        '      Else
        '        choice = "Bank"
        '      End If
        '      For a As Integer = choices.GetLowerBound(0) To choices.GetUpperBound(0)
        '        choices(a) = choice
        '      Next
        '    Else
        '      Dim a As Integer = 0
        '      For Each t In tmp
        '        If t.PayCode = "134" Then
        '          choice = "Paid"
        '        Else
        '          choice = "Bank"
        '        End If
        '        Dim x As Double = t.Hours / holidayIncrement
        '        Do While x > 0
        '          choices(a) = choice
        '          x -= 1
        '          a += 1
        '        Loop
        '      Next
        '    End If

        '  Else
        '    If (From t In tmp Where t.PayCode = "134" Select t).Count > 0 Then
        '      choices(0) = "Paid"
        '    Else
        '      choices(0) = "Bank"
        '    End If
        '  End If
        'End If
        Return choices
      End Get
    End Property

    ReadOnly Property BankedHoursPaid As Double
      Get
        ' Here we need to look at the saved data and see if we have any hours under the 124 pay code
        If Current_Timecard_Data.Count = 0 Then Return 0
        Dim tmp = (From c In Current_Timecard_Data Where c.PayCode = "124" Select c.Hours).Sum
        Return tmp
      End Get
    End Property

    ReadOnly Property HolidayHoursUsed As Double
      Get
        Return (From c In calculatedTimeList Where c.payCode = "123" Select c.hours).Sum
      End Get
    End Property

    Public ReadOnly Property Days_Since_PPE() As Integer
      Get
        Return Today.Subtract(payPeriodStart.AddDays(13)).TotalDays
      End Get
    End Property

    Public ReadOnly Property DepartmentDisplay() As String
      Get
        Return department & " (" & departmentNumber & ")"
      End Get
    End Property

    Public ReadOnly Property Approval_Level As Integer
      Get
        ' 0 = Not approve
        ' 1 = Initial Approval completed
        ' 2 = Final Approval completed
        If Current_Timecard_Data.Count = 0 Then
          Return 0
        Else
          Return (From c In Current_Timecard_Data Select c.Approved).First
        End If
      End Get
    End Property

    Public ReadOnly Property Initial_Approval_EmployeeID As Integer
      Get
        If Approval_Level = 0 Then
          Return 0
        Else
          Return (From c In Current_Timecard_Data Select c.Initial_Approval_By_EmployeeID).First
        End If
      End Get
    End Property

    Public ReadOnly Property Initial_Approval_EmployeeID_Access_Type As Integer
      Get
        If Approval_Level = 0 Then
          Return 1
        Else
          Return (From c In Current_Timecard_Data Select c.Initial_Approval_EmployeeID_AccessType).First
        End If
      End Get
    End Property

    Public ReadOnly Property Initial_Approval_DateTime As String
      Get
        If Approval_Level = 0 Then
          Return ""
        Else
          Return (From c In Current_Timecard_Data Select c.Initial_Approval_Date).First.ToString
        End If
      End Get
    End Property

    ReadOnly Property Initial_Approval_By As String
      Get
        If Approval_Level = 0 Then
          Return ""
        Else
          Return (From c In Current_Timecard_Data Select c.Initial_Approval_By_Name).First
        End If
      End Get
    End Property

    Public ReadOnly Property Final_Approval_EmployeeID As Integer
      Get
        If Approval_Level < 2 Then
          Return 0
        Else
          Return (From c In Current_Timecard_Data Select c.Final_Approval_By_EmployeeID).First
        End If
      End Get
    End Property

    Public ReadOnly Property Final_Approval_DateTime As String
      Get
        If Approval_Level < 2 Then
          Return Date.MinValue
        Else
          Return (From c In Current_Timecard_Data Select c.Final_Approval_Date).First.ToString
        End If
      End Get
    End Property

    ReadOnly Property Final_Approval_By As String
      Get
        If Approval_Level < 2 Then
          Return ""
        Else
          Return (From c In Current_Timecard_Data Select c.Final_Approval_By_Name).First
        End If
      End Get
    End Property

    Public ReadOnly Property Data_Type As String
      Get
        If Current_Timecard_Data.Count = 0 Then
          Select Case departmentNumber
            Case "1703", "2103"
              Return "telestaff"
            Case "2102"
              If payPeriodStart < CType("3/23/2016", Date) Then
                Return "telestaff"
              Else
                Return "timecard"
              End If
            Case Else
              Return "timecard"
          End Select
        Else
          Return (From c In Current_Timecard_Data Select c.DataType).First
        End If
      End Get
    End Property

    Public ReadOnly Property Reports_To As Integer
      Get
        If Current_Timecard_Data.Count = 0 Then
          Return 0
        Else
          Return (From c In Current_Timecard_Data Select c.ReportsTo).First
        End If
      End Get
    End Property

    Public ReadOnly Property Access_Type As Integer
      Get
        If Current_Timecard_Data.Count = 0 Then
          Return 1
        Else
          Return (From c In Current_Timecard_Data Select c.AccessType).First
        End If
      End Get
    End Property

    Private Sub Load_Saved_Time()
      ' This function will query and format the data in the timecard table and add it to the 
      ' Current_Timecard_Data list

      Dim dbc As New Tools.DB(GetCS(ConnectionStringType.Timestore), toolsAppId, toolsDBError)
      'Dim query As String = "USE Timestore; SELECT * FROM Saved_Time WHERE employee_id=" & employeeID & " AND pay_period_ending='" & payPeriodEndingDisplay & "';"
      Dim sbQ As New StringBuilder
      With sbQ
        .AppendLine("USE TimeStore;")
        .AppendLine("SELECT ISNULL(A.data_type, CASE WHEN ST.orgn IN ('1703', '2103') THEN ") ' Removed 2102
        .AppendLine(" 'telestaff' ELSE 'timecard' END) AS data_type, ISNULL(A.access_type, 1) AS access_type, ")
        .AppendLine("ISNULL(A2.access_type, ISNULL(A.access_type, 1)) AS initial_approval_employeeid_access_type, ")
        .AppendLine("ISNULL(A.reports_to, 0) AS reports_to, ST.pay_period_ending, ST.employee_id, ST.paycode, ")
        .AppendLine("ST.payrate, ST.hours, ST.amount, ST.orgn, ST.classify, ST.date_added, ST.date_updated, ")
        .AppendLine("ST.initial_approval_username, ST.initial_approval_employeeid, ST.initial_approval_machine_name, ")
        .AppendLine("ST.initial_approval_ip_address, ST.initial_approval_date, ")
        .AppendLine("ST.final_approval_username, ST.final_approval_employeeid, ST.final_approval_machine_name, ")
        .AppendLine("ST.final_approval_ip_address, ST.final_approval_date ")
        .AppendLine("FROM Saved_Time ST ")
        .AppendLine("LEFT OUTER JOIN Access A ON ST.employee_id = A.employee_id ")
        .AppendLine("LEFT OUTER JOIN Access A2 ON ST.initial_approval_employeeid = A2.employee_id ")
        .Append("WHERE ST.pay_period_ending = '").Append(payPeriodEndingDisplay)
        .Append("' AND ST.employee_id=").Append(employeeID).Append(";")
      End With
      Try
        Dim ds As DataSet = dbc.Get_Dataset(sbQ.ToString)
        Current_Timecard_Data.AddRange((From d In ds.Tables(0).AsEnumerable Select New Saved_Timecard_Data With {
                                  .Approved = Set_Approval_Level(d), .DepartmentNumber = d("orgn"),
                                  .EmployeeId = d("employee_id"), .Hours = d("hours"), .Classify = d("classify"),
                                  .PayCode = d("paycode"), .PayPeriodEnding = d("pay_period_ending"), .PayRate = d("payrate"),
                                  .AccessType = d("access_type"), .DataType = d("data_type"), .ReportsTo = d("reports_to"),
                                  .Initial_Approval_By_EmployeeID = IsNull(d("initial_approval_employeeid"), 0),
                                  .Initial_Approval_Date = IsNull(d("initial_approval_date"), Date.MinValue),
                                  .Initial_Approval_By_Name = Get_Employee_Name(.Initial_Approval_By_EmployeeID),
                                  .Final_Approval_By_EmployeeID = IsNull(d("final_approval_employeeid"), 0),
                                  .Final_Approval_Date = IsNull(d("final_approval_date"), Date.MinValue),
                                  .Final_Approval_By_Name = Get_Employee_Name(.Final_Approval_By_EmployeeID),
                                  .Initial_Approval_EmployeeID_AccessType = d("initial_approval_employeeid_access_type")}).ToList)
      Catch ex As Exception
        Log(ex)
      End Try
    End Sub

    Private Sub Load_Notes()
      '
      Dim dbc As New Tools.DB(GetCS(ConnectionStringType.Timestore), toolsAppId, toolsDBError)
      'Dim query As String = "USE Timestore; SELECT * FROM Saved_Time WHERE employee_id=" & employeeID & " AND pay_period_ending='" & payPeriodEndingDisplay & "';"
      Dim sbQ As New StringBuilder
      With sbQ
        .AppendLine("USE TimeStore;")
        .AppendLine("SELECT note, note_date_added, note_added_by FROM notes N ")
        .Append("WHERE N.pay_period_ending = '").Append(payPeriodEndingDisplay)
        .Append("' AND N.employee_id=").Append(employeeID)
        .AppendLine("ORDER BY note_date_added DESC;")
      End With
      Try
        Dim ds As DataSet = dbc.Get_Dataset(sbQ.ToString)
        Notes.AddRange((From d In ds.Tables(0).AsEnumerable Select New Note With {
                              .Added_By = d("note_added_by"), .Date_Added = d("note_date_added"),
                              .Note = d("note"), .EmployeeID = employeeID, .PayPeriodEnding = payPeriodStart.AddDays(13)}).ToList)
      Catch ex As Exception
        Log(ex)
      End Try
    End Sub

    Public Sub Load()
      If employeeID.Length = 0 Then Exit Sub
      Load_Saved_Time()
      Load_Notes()
    End Sub

    Public Function Save(Optional allowDataSave As Boolean = False) As Boolean
      ' This function will save the contents of this timecard to the timecard table in the Timestore database.
      ' If will also remove any approval already added, so this should only be used if you've determined the record
      ' doesn't exist yet or needs to be updated, then this is the right function to call.
      ' It will need to be smart enough to know which fields to use based on the datatype
      Handle_Saved_Holiday_Data()
      'allowDataSave = True ' For Hurricane Matthew special calculations ' 777
      If Days_Since_PPE < 1 Or (Days_Since_PPE = 1 AndAlso Now.Hour < PayPeriodEndingCutoff) Or allowDataSave Then
        ' We don't want to save the data that's out of date.  No changes should be made past the first day of the next payperiod.
        If Current_Timecard_Data.Count = 0 Then
          Return Save_Time()
        Else
          If Not Check_Current_Time_Versus_Saved_Time() Then
            ' First, if this is approved, we need to add a note.
            If Approval_Level <> 0 Then Add_Note("Approval Removed, Hours or Payrate has changed.")
            Delete_Saved_Timecard_Data()
            Return Save_Time()
          ElseIf ErrorList.Count > 0 And Approval_Level > 0 Then
            Add_Note("Approval Removed, an Error is present.")
            Delete_Saved_Timecard_Data()
            Return Save_Time()
          End If
        End If
      End If
      Return False
    End Function

    Private Sub Handle_Saved_Holiday_Data()
      ' This function will check the current_timecard_data for holiday data that doesn't come from
      ' Telestaff. This data is only present in the Timestore saved_time table.
      ' If it finds it, it will add it to the approval and calculated time.
      ' I should only do this if the person is in one of the Public Safety Depts
      Dim Paycodes As Dictionary(Of String, String) = myCache.GetItem("paycodes")
      Dim PS() As String = {"1703", "2103"}
      Dim HolidayPayCodes() As String = {"124", "134", "122", "800"}
      If PS.Contains(departmentNumber) Then

        Dim tmp = (From ctd In Current_Timecard_Data
                   Where HolidayPayCodes.Contains(ctd.PayCode)
                   Select ctd).ToList

        PopulatePayCodes()

        For Each c In tmp
          Select Case c.PayCode
            Case "122", "124", "134", "800"
              Try
                timeList.RemoveAll(Function(x) x.payCode = c.PayCode)
                calculatedTimeList.RemoveAll(Function(x) x.payCode = c.PayCode)
                approvalTimeList.Add(New WorkType(PayCodes(c.PayCode), c.Hours, 16, c.PayCode, c.PayRate))
                timeList.Add(New WorkType(PayCodes(c.PayCode), c.Hours, 16, c.PayCode, c.PayRate))
                calculatedTimeList.Add(New WorkType(PayCodes(c.PayCode), c.Hours, 16, c.PayCode, c.PayRate))
              Catch ex As Exception
                Log(ex)
              End Try

          End Select
        Next

        If Not WarningList.Contains("Multiple Payrates for this employee in this pay period.") Then
          If (From c In Current_Timecard_Data Select c.PayRate Distinct).Count > 1 Then
            WarningList.Add("Multiple Payrates for this employee in this pay period.")
          End If
        End If

      End If

    End Sub

    Private Function Check_Current_Time_Versus_Saved_Time() As Boolean
      ' Returns true if the user's time has changed.
      ' First we check the current timecard data against the calculated timelist
      Dim tmpCL As List(Of WorkType) = (From cl In calculatedTimeList
                                        Where cl.hours > 0
                                        Select cl).ToList
      For Each c In Current_Timecard_Data
        Dim test As Integer = (From w In tmpCL
                               Where w.hours = c.Hours And
                               w.payCode = c.PayCode And
                               Math.Round(w.payRate, 5) = c.PayRate
                               Select w).Count
        If test <> 1 Then
          Return False
        End If
      Next
      ' If they get here, then the current timecard data is atleast present in the calculated timelist, now we 
      ' need to see if the reverse is true.
      For Each c In tmpCL
        Dim test As Integer = (From w In Current_Timecard_Data
                               Where w.Hours = c.hours And w.PayCode = c.payCode And
                               w.PayRate = Math.Round(c.payRate, 5) Select w).Count
        If test <> 1 Then
          Return False
        End If
      Next
      ' Here are some Public Safety checks for holidays.
      Dim psDepts() As String = {"1703", "2103"}
      If psDepts.Contains(departmentNumber) Then
        ' Here we are checking to see if their payrate has changed, if so, we remove the data.
        ' First we look at what their payrate is going to be at the end of the pay period.
        ' If they are moved in office, it will convert their office rate into a field rate.
        ' The field rate will also work just fine for dispatch, because it only changes the
        ' rate if the field person moves in office, and dispatch employees never move in office.
        Dim tpi As New Telestaff_Profile_Info(employeeID, payPeriodStart.AddDays(13))
        Dim HolidayPayrate As Double = tpi.FieldPayrate
        If (From c In Current_Timecard_Data
            Where c.PayRate <> HolidayPayrate And
            (c.PayCode = "134" Or c.PayCode = "124")
            Select c).Count > 0 Then
          Add_Note("Approval Removed, Holiday hours Payrate has changed.")
          Delete_Saved_Timecard_Data(False)
          Return False
        End If
        ' Here we're also going to check to see if they have the Holiday hours banked necessary to 
        ' support what they've requested to be paid out.  
        Dim holidayhourspaid = (From c In Current_Timecard_Data
                                Where c.PayCode = "124"
                                Select c.Hours).Sum
        If holidayhourspaid > bankedHoliday Then
          Add_Note("Holiday data removed, along with any approvals. You have " & bankedHoliday.ToString() & " hours banked and were attempting to be paid for " & holidayhourspaid & " hours.")
          Delete_Saved_Timecard_Data(False)
          Return False
        End If
        ' Last, we'll be checking to make sure that if a selection for their holiday
        ' has been made, that they actually have the necessary hours to use
        ' this is for pay codes 122 (holiday hours banked) and 134 (holiday hours paid)
        ' Basically, if someone is a field employee at the beginning of the pay period
        ' and move in office after they have made their election, then their holiday
        ' choices will need to be removed.
        If HolidayHoursChoice.Count > HolidaysInPPD.Length Then
          'Add_Note("Holiday data removed, along with any approvals.")
          'Delete_Saved_Timecard_Data(False)
          'Return False
          Return True
        End If
      End If
      Return True
    End Function

    Private Sub Delete_Saved_Timecard_Data(Optional IgnoreHoliday As Boolean = True)
      Clear_Saved_Timestore_Data(employeeID, payPeriodStart, IgnoreHoliday)
    End Sub

    Private Sub Build_Timecard_Data_From_Current()
      Dim PS() As String = {"1703", "2103"} ' , "2102"} ' Removed 2102
      Dim holidayPayCodes() As String = {"122", "124", "134", "800"}
      Dim tmp As New List(Of Saved_Timecard_Data)
      tmp.AddRange((From c In Current_Timecard_Data
                    Where holidayPayCodes.Contains(c.PayCode)
                    Select c)) 'c.PayCode = "134" Or c.PayCode = "124" Or c.PayCode = "122"
      Current_Timecard_Data.Clear()

      If (From ctltmp In calculatedTimeList
          Where holidayPayCodes.Contains(ctltmp.payCode)
          Select ctltmp).Count = 0 Then
        Current_Timecard_Data.AddRange(tmp)
      End If

      Dim ppe As Date = payPeriodStart.AddDays(13)
      For Each c In (From ctl In calculatedTimeList
                     Where ctl.hours > 0
                     Select ctl)
        Dim std As New Saved_Timecard_Data
        std.EmployeeId = employeeID
        std.DataType = "timecard"
        If PS.Contains(departmentNumber) Then
          std.DataType = "telestaff"
        End If

        std.PayCode = c.payCode
        std.PayRate = c.payRate
        std.Hours = c.hours
        std.PayPeriodEnding = ppe
        std.Approved = False
        std.Classify = classify
        std.DepartmentNumber = departmentNumber
        Current_Timecard_Data.Add(std)
      Next
    End Sub

    Private Function Save_Time() As Boolean
      Build_Timecard_Data_From_Current()

      Dim ignorePaycodes() As String = {"122", "134", "124"}

      For Each c In (From ctd In Current_Timecard_Data
                     Where Not ignorePaycodes.Contains(ctd.PayCode)
                     Select ctd).ToList
        If Not Save_Hours(employeeID, payPeriodStart.AddDays(13), c.PayCode, c.Hours, c.PayRate, c.DepartmentNumber, c.Classify) Then
          Return False
          Exit For
        End If
      Next
      Return True
    End Function

    Public Function Add_Note(Note As String) As Boolean
      ' This function will add a note to the notes table in the Timestore database for this timecard.
      Return Add_Timestore_Note(employeeID, payPeriodEndingDisplay, Note)

    End Function

    Public ReadOnly Property Get_Types_Used() As String()
      Get
        Return (From u In _used_Types Order By u.leftToRightOrder Ascending Select u.name).ToArray
      End Get
    End Property

    Public Sub New(ByVal Username As String)
      Dim eid As Integer = AD_EmployeeData.GetEmployeeIDFromAD(Username)
      payPeriodStart = GetPayPeriodStart(Today)
      Load_EmployeeData(eid)
    End Sub

    Public Sub New(ByVal EmployeeID As Integer)
      payPeriodStart = GetPayPeriodStart(Today)
      Load_EmployeeData(EmployeeID)
    End Sub

    Public Sub New(e As EPP, Optional allowDataSave As Boolean = False)
      foundEmployee = True
      Load_EPP(e)
      Save(allowDataSave)
    End Sub

    Public Sub New(e As TC_EPP, Optional allowDataSave As Boolean = False)
      foundEmployee = True
      Load_TCTD(e)
      Save(allowDataSave)
    End Sub

    Public Sub New(e As EPP, STD As List(Of Saved_Timecard_Data), SavedNotes As List(Of Note), Optional allowDataSave As Boolean = False)
      foundEmployee = True
      Load_EPP(e)
      Current_Timecard_Data.AddRange(STD)
      Notes.AddRange(SavedNotes)
      Save(allowDataSave)
    End Sub

    Public Sub New(e As TC_EPP, STD As List(Of Saved_Timecard_Data), SavedNotes As List(Of Note), Optional allowDataSave As Boolean = False)
      foundEmployee = True
      Load_TCTD(e)
      Current_Timecard_Data.AddRange(STD)
      Notes.AddRange(SavedNotes)
      Save(allowDataSave)
    End Sub

    Public Sub New(StartDate As Date, EmployeeID As Integer)
      payPeriodStart = GetPayPeriodStart(StartDate)
      Load_EmployeeData(EmployeeID)
    End Sub

    Public Sub New(StartDate As Date, Username As String)
      payPeriodStart = GetPayPeriodStart(StartDate)
      Dim eid As Integer = AD_EmployeeData.GetEmployeeIDFromAD(Username)
      Load_EmployeeData(eid)
    End Sub

    Private Sub Load_EmployeeData(EmployeeID As Integer)
      ' We're going to get the data from Finplus, look at their department number
      ' if they are in 1703, 2102, 2103, we're going to check Telestaff.
      ' Otherwise we are going to check TimeCard.
      ' If they are not found in either, we set foundemployee = false, otherwise we set it to true.
      ' The upside to this is we'll have some form of data to give the end user,
      ' and we can just check that and give a message as to why we only returned finplus data.
      Dim f As List(Of FinanceData) = GetEmployeeDataFromFinPlus(EmployeeID)
      If f.Count = 1 Then
        foundEmployee = True
        Dim fd As FinanceData = f.First()
        'If StartDate < CType("8/11/2015", Date) Or StartDate > CType("8/25/2015", Date) 
        If EmployeeID = 2201 And (payPeriodStart < CType("8/12/2015", Date) Or payPeriodStart > CType("8/25/2015", Date)) Then
          Dim tctd As List(Of TimecardTimeData) = GetEmployeeDataFromTimecardOrTimeStore(payPeriodStart, EmployeeID)
          Dim e As New TC_EPP(tctd, fd, payPeriodStart)
          Load_TCTD(e)
        Else
          Select Case fd.Department
            Case "1703" ' Telestaff
              Dim ttdl As List(Of TelestaffTimeData) = GetEmployeeDataFromTelestaff(payPeriodStart, EmployeeID)
              Dim e As New EPP(ttdl, fd, payPeriodStart)
              Load_EPP(e)
            Case "2102", "2103"
              ' For these departments we want to make them look in Telestaff first, and then
              ' fall back to Timestore if nothing is found.

              Dim ttdl As List(Of TelestaffTimeData) = GetEmployeeDataFromTelestaff(payPeriodStart, EmployeeID)
              If ttdl.Count > 0 Then
                Dim e As New EPP(ttdl, fd, payPeriodStart)
                Load_EPP(e)
              Else
                Dim tctd As List(Of TimecardTimeData) = GetEmployeeDataFromTimecardOrTimeStore(payPeriodStart, EmployeeID)
                Dim e As New TC_EPP(tctd, fd, payPeriodStart)
                Load_TCTD(e)
              End If

            Case Else ' Going to try the timecard database for everything else.
              Dim tctd As List(Of TimecardTimeData) = GetEmployeeDataFromTimecardOrTimeStore(payPeriodStart, EmployeeID)
              Dim e As New TC_EPP(tctd, fd, payPeriodStart)
              Load_TCTD(e)
          End Select
        End If

      Else
        ' We didn't find this employee, we should do something.
        foundEmployee = False
        ErrorOccurred = True
        If f.Count > 1 Then
          ErrorText = "Employee ID was found multiple times in Finplus."
        Else
          ErrorText = "Employee ID was not found in Finplus."
        End If
      End If
      If ErrorText.Length = 0 Then
        Load()
        Save()
      End If
    End Sub

    Private Sub Load_FinanceData(f As FinanceData)
      Payrate = f.Base_Payrate
      employeeID = f.EmployeeId
      lastName = f.EmployeeLastName
      firstName = f.EmployeeFirstName
      employeeName = f.EmployeeName
      bankedHoliday = f.Banked_Holiday_Hours
      bankedComp = f.Banked_Comp_Hours
      bankedSick = f.Banked_Sick_Hours
      bankedVacation = f.Banked_Vacation_Hours
      departmentNumber = f.Department
      department = f.DepartmentName
      hireDate = f.HireDate
      hoursForOvertime = f.HoursNeededForOvertime
      classify = f.Classify
      title = f.JobTitle
      isFullTime = f.isFulltime
      scheduledHours = f.HoursNeededForOvertime
      TerminationDate = f.TerminationDate
      ' For Testing
      'bankedHoliday = 48
    End Sub

    Private Sub Load_EPP(Employee As EPP)
      Load_FinanceData(Employee.EmployeeData)
      ' Right now, the payrate in the timecard is based on Finplus.
      ' But if we want to base that on Telestaff, we'll have to use something
      ' like what's commented out below.
      'If (Employee.Payrates.Count > 0) Then
      '  Payrate = (From e In Employee.Timelist
      '             Order By e.WorkDate Ascending
      '             Select e.PayRate).Last
      'End If
      payPeriodStart = Employee.PayPeriodStart
      GroupName = Employee_Data.GetGroupName(employeeID)
      isExempt = Employee.IsExempt
      WarningList = Employee.WarningList
      ErrorList = Employee.ErrorList
      TelestaffProfileType = Employee.TelestaffProfileType
      'RawTCTD.AddRange(Employee.RawTimeList)

      isHolidayTimeBankable = Not (Employee.TelestaffProfileType = TelestaffProfileType.Office)
      'If Employee.TelestaffProfileType = TelestaffProfileType.Office And bankedHoliday > 0 Then
      '    ' If the employee has banked hours and is an office worker, they may be in office for an OJI
      '    ' in which case we would need to convert their payrate from office to field to pay out for holidays.
      '    If Employee.RawTimeList.Count = 0 Then
      '        ' If they are marked as office and have hours but they don't have any time recorded in telestaff yet,
      '        ' we're going to assume that they may be ok, so we'll just use the telestaff rate.
      '        HolidayPayRate = Employee.EmployeeData.Base_Payrate
      '    Else
      '        ' We're going to convert their rate to the field rate.
      '        HolidayPayRate = Calculate_Reverse_Telestaff_Office_Payrate(Employee.EmployeeData.Base_Payrate)
      '    End If
      'End If

      'Dim holidays As List(Of Date) = getHolidayList(payPeriodStart.Year, False)
      'If payPeriodStart.Year <> payPeriodStart.AddDays(13).Year Then
      '  holidays.AddRange(getHolidayList(payPeriodStart.AddDays(13).Year, False))
      'End If

      'holidays = (From holiday In holidays Where holiday >= payPeriodStart And
      '                                 holiday < payPeriodStart.AddDays(14)
      '            Select holiday).ToList

      Dim holidays As List(Of Date) = Get_Holidays_By_Payperiod(payPeriodStart)

      'Dim TmpHolidays As List(Of Date) = (From h In getHolidayList(payPeriodStart.Year, False) Where h >= payPeriodStart _
      '                                          And h < payPeriodStart.AddDays(14) Select h).ToList
      Dim actualHolidays As New List(Of String)
      For Each h In holidays
        Dim tpi As New Telestaff_Profile_Info(Employee.EmployeeData.EmployeeId, h)
        If Not tpi.ProfileError AndAlso Not tpi.ProfileType = TelestaffProfileType.Office Then
          actualHolidays.Add(h.ToShortDateString)
        End If
        'Dim tmp = (From rt In Employee.RawTimeList
        '           Where (rt.ProfileType = TelestaffProfileType.Field Or
        '             rt.ProfileType = TelestaffProfileType.Dispatch) And
        '             rt.ProfileStartDate <= h And rt.ProfileEndDate >= h Select rt)
        'If tmp.Count > 0 Then actualHolidays.Add(h.ToShortDateString)
      Next
      HolidaysInPPD = actualHolidays.ToArray

      Select Case Employee.TelestaffProfileType
        Case TelestaffProfileType.Field
          holidayIncrement = 24
        Case TelestaffProfileType.Dispatch
          holidayIncrement = 12
        Case TelestaffProfileType.Office
          If Employee.EmployeeData.Banked_Holiday_Hours > 0 Then
            holidayIncrement = 24
          End If
      End Select
      'If holidayIncrement > 0 AndAlso isHolidayInPPD Then
      '    bankedHoliday -= holidayIncrement
      'End If
      Dim worktypelist = (From tl In Employee.Timelist Select tl.WorkTypeAbrv, tl.WorkType).Distinct
      'Dim TelestaffLegend As List(Of TelestaffDescriptionLegend) = (From wtl In worktypelist Select New TelestaffDescriptionLegend With {.ShortName = wtl.WorkTypeAbrv, .Description = wtl.WorkType}).ToList

      Dim x As New DailyData
      For Each t In Employee.RawTimeList
        If x.workDate = Nothing Then x.workDate = t.WorkDate
        If x.workDate <> t.WorkDate Then
          x.workTime = x.workTime.Trim
          RawTime.Add(x)
          x = New DailyData
          x.workDate = t.WorkDate
        End If
        If x.comment.Length > 0 And t.Comment.Length > 0 Then x.comment &= vbCrLf
        x.comment &= t.Comment
        Dim sTmp As String = t.StartTime.ToShortTimeString & " - " & t.EndTime.ToShortTimeString & " (" & t.WorkTypeAbrv & ")"
        Dim xWork As New Work(sTmp, t.WorkType)
        x.workTimeList.Add(xWork)
        x.workTime &= " " & sTmp
        Dim wtName As String = "", wtLROrder As Integer = -1
        Select Case t.WorkCode
          Case "002", "230", "131", "231", "232", "130"  ' straight time aka regular
            wtName = "Regular Work"
            wtLROrder = 0
          Case "090", "095" ' Leave without pay
            wtName = "Leave Without Pay"
            wtLROrder = 10
          Case "101", "100" ' vacation
            wtName = "Vacation"
            wtLROrder = 1
          Case "134" ' paid holiday
            wtName = "Holiday Paid"
            wtLROrder = 3
          Case "110", "111" ' sick 
            wtName = "Sick"
            wtLROrder = 2
          Case "122" ' Holiday Time Bank
            wtName = "Holiday Time Banked"
            wtLROrder = 4
          Case "123" ' Holiday time bank Hours Requested to be used
            wtName = "Holiday Time Used"
            wtLROrder = 5
          Case "120" ' Comp time accrued
            wtName = "Comp Time Banked"
            wtLROrder = 6
          Case "121" ' Banked comp time used
            wtName = "Comp Time Used"
            wtLROrder = 7
          Case Else
            wtName = "Other"
            wtLROrder = 100
        End Select
        If x.workHoursList.Exists(Function(y) y.name = wtName) Then
          Dim wt As WorkType = x.workHoursList.Find(Function(y) y.name = wtName)
          wt.hours += t.WorkHours
        Else
          Dim wt As New WorkType
          wt.name = wtName
          wt.hours = t.WorkHours
          wt.leftToRightOrder = wtLROrder
          Check_Work_Type(wt)
          x.workHoursList.Add(wt)
        End If
      Next
      RawTime.Add(x)
      ' Now that we've handled the raw time, let's add the processed time
      Add_Timelists(Employee)
      Check_Exceptions(Employee)
    End Sub

    Private Sub Check_Exceptions(E As EPP)
      ' Here we're going to check a few things that will depend on data from Timestore
      ' that the Telestaff class won't have.
      Dim hl = Get_Holidays_By_Payperiod(E.PayPeriodStart)
      If hl.Count > 0 Then

        Dim firstHoliday As Date = hl.First
        Dim usedHolidays = (From tl In E.Timelist
                            Where tl.WorkCode = "123"
                            Select tl).ToList

        ' if the amount of hours used prior to the holiday is greater than their
        ' banked amount, we're going to throw an error
        Dim TotalHoursUsedBeforeHoliday = (From u In usedHolidays
                                           Where u.WorkDate < firstHoliday
                                           Select u.WorkHours).Sum
        If E.Banked_Holiday_Hours < TotalHoursUsedBeforeHoliday Then
          ErrorList.Add("Too many Holiday hours used. Holiday hours cannot be used before they are accrued.")
        End If
      End If
    End Sub

    Private Sub Add_Timelists(e As EPP)
      Dim w() As String = {"ML", "EL", "ADM", "OJI"}
      Dim pnw As Double = 0, tmp As Double = 0

      For Each p In e.Payrates
        pnw = 0
        ' timeList has the data we're going to use to let the person approve their time.
        'Dim tl As List(Of TelestaffTimeData) = (From t In e.Timelist Where w.Contains(t.WorkTypeAbrv) _
        '                                                And t.PayRate = p Select t).ToList
        'For Each t In tl
        '  timeList.Add(New WorkType(t.WorkType, t.WorkHours, 12, "002", p))
        '  pnw += t.WorkHours
        'Next

        Dim mytl = (From t In e.Timelist
                    Where w.Contains(t.WorkTypeAbrv) And t.PayRate = p
                    Group By WorkType = t.WorkType Into WorkTypeGroup = Group,
                   totalHours = Sum(t.WorkHours)
                    Select New With {WorkType, totalHours})
        pnw = 0
        For Each t In mytl
          timeList.Add(New WorkType(t.WorkType, t.totalHours, 12, "002", p))
          pnw += t.totalHours
        Next


        tmp = e.Regular.TotalHours(p) + e.Scheduled_Overtime.TotalHours(p) +
                  e.Scheduled_Regular_Overtime.TotalHours(p) + e.Unscheduled_Overtime.TotalHours(p) +
                  e.Unscheduled_Regular_Overtime.TotalHours(p) + e.Unscheduled_Double_Overtime.TotalHours(p) -
                  pnw

        Dim totaldisasterhours = e.Disaster_Regular.TotalHours(p) + e.Disaster_Doubletime.TotalHours(p) +
          e.Disaster_Overtime.TotalHours(p) + e.Disaster_StraightTime.TotalHours(p)

        timeList.Add(New WorkType("Regular Work", tmp, 0, "002", p))
        timeList.Add(New WorkType("Disaster Regular Work", totaldisasterhours, 0, "002", p))

        If e.Union_Time_Pool.TotalHours(p) > 0 Then
          timeList.Add(New WorkType("Union Time Pool", e.Union_Time_Pool, 12, p))
          calculatedTimeList.Add(New WorkType("Union Time Pool", e.Union_Time_Pool, 12, p))
        End If
        If Not isExempt Then
          calculatedTimeList.Add(New WorkType("Regular Work", e.Regular, 0, p))
          calculatedTimeList.Add(New WorkType("Scheduled OT 1.0", e.Scheduled_Regular_Overtime, 5, p))
          calculatedTimeList.Add(New WorkType("Scheduled OT 1.5", e.Scheduled_Overtime, 1, p))
          calculatedTimeList.Add(New WorkType("Unscheduled OT 1.0", e.Unscheduled_Regular_Overtime, 7, p))
          calculatedTimeList.Add(New WorkType("Unscheduled OT 1.5", e.Unscheduled_Overtime, 8, p))
          calculatedTimeList.Add(New WorkType("Unscheduled OT 2.0", e.Unscheduled_Double_Overtime, 9, p))
          calculatedTimeList.Add(New WorkType("Disaster Regular Hours", e.Disaster_Regular, 11, p))
          calculatedTimeList.Add(New WorkType("Disaster Admin Hours", e.Admin_Leave_Disaster, 11, p))
          calculatedTimeList.Add(New WorkType("Disaster Hours 1.0", e.Disaster_StraightTime, 11, p))
          calculatedTimeList.Add(New WorkType("Disaster Hours 1.5", e.Disaster_Overtime, 11, p))
          calculatedTimeList.Add(New WorkType("Disaster Hours 2.0", e.Disaster_Doubletime, 11, p))
          Select Case e.TelestaffProfileType
            Case TelestaffProfileType.Dispatch, TelestaffProfileType.Field
              timeList.Add(New WorkType("Holiday", e.Holiday_Paid, 6, p))
              timeList.Add(New WorkType("Holiday Time Banked", e.Holiday_Time_Banked, 10, p))
              timeList.Add(New WorkType("Holiday Time Used", e.Holiday_Time_Used, 11, p))
              'timeList.Add(New WorkType("Banked Holiday Time Paid", ))
              calculatedTimeList.Add(New WorkType("Holiday", e.Holiday_Paid, 6, p))
              calculatedTimeList.Add(New WorkType("Holiday Time Banked", e.Holiday_Time_Banked, 10, p))
              calculatedTimeList.Add(New WorkType("Holiday Time Used", e.Holiday_Time_Used, 11, p))

            Case TelestaffProfileType.Office
              timeList.Add(New WorkType("Comp Time Banked", e.Comp_Time_Banked, 10, p))
              timeList.Add(New WorkType("Comp Time Used", e.Comp_Time_Used, 11, p))

              calculatedTimeList.Add(New WorkType("Comp Time Banked", e.Comp_Time_Banked, 10, p))
              calculatedTimeList.Add(New WorkType("Comp Time Used", e.Comp_Time_Used, 11, p))
          End Select
        Else
          calculatedTimeList.Add(New WorkType("Disaster Hours 1.0", e.Disaster_StraightTime, 11, p))
          tmp = 80 - e.Vacation.TotalHours - e.Sick.TotalHours - e.Leave_Without_Pay.TotalHours
          calculatedTimeList.Add(New WorkType("Regular Work", tmp, 0, "002", p))
        End If

        timeList.Add(New WorkType("Term Hours", e.Term_Hours, 2, p))
        timeList.Add(New WorkType("LWOP", e.Leave_Without_Pay, 2, p))
        timeList.Add(New WorkType("Vacation", e.Vacation, 3, p))
        timeList.Add(New WorkType("Sick", e.Sick, 4, p))

        calculatedTimeList.Add(New WorkType("Term Hours", e.Term_Hours, 2, p))
        calculatedTimeList.Add(New WorkType("LWOP", e.Leave_Without_Pay, 2, p))
        calculatedTimeList.Add(New WorkType("Vacation", e.Vacation, 3, p))
        calculatedTimeList.Add(New WorkType("Sick", e.Sick, 4, p))
      Next

      timeList.RemoveAll(Function(x) x.hours = 0)

      ' The Paid Non Working list is going to be used during Final Approval to tell how many Paid Non Working hours 
      ' the employee has received over the course of the pay period.
      'Dim adminCheck = (From t In e.Timelist Where t.WorkTypeAbrv = "ADM"
      '                  Group By workDate = t.WorkDate Into adminGroup = Group,
      '                                        totalAdmin = Sum(t.WorkHours)
      '                  Select New With {workDate, totalAdmin})

      ''Dim wtl = (From t In e.Timelist
      ''           Where w.Contains(t.WorkTypeAbrv)
      ''           Group By WorkTypeAbrv = t.WorkTypeAbrv Into WorkTypeGroup = Group,
      ''             totalHours = Sum(t.WorkHours)
      ''           Select New With {WorkTypeAbrv, totalHours})
      ''pnw = 0
      ''For Each t In wtl
      ''  approvalTimeList.Add(New WorkType(t.WorkTypeAbrv, t.totalHours, 0, "002"))
      ''  pnw += t.totalHours
      ''Next

      'Dim wtl As List(Of TelestaffTimeData) = (From t In e.Timelist
      '                                         Where w.Contains(t.WorkTypeAbrv)
      '                                         Select t).ToList
      'pnw = 0
      'For Each t In wtl
      '  approvalTimeList.Add(New WorkType(t.WorkType, t.WorkHours, 0, "002"))
      '  pnw += t.WorkHours
      'Next
      ''If Not e.IsExempt Then
      ''  approvalTimeList.Add(New WorkType("Regular Work", e.Regular.TotalHours - pnw, 0, "002"))
      ''  approvalTimeList.Add(New WorkType("Scheduled OT 1.0", e.Scheduled_Regular_Overtime, 5))
      ''  approvalTimeList.Add(New WorkType("Scheduled OT 1.5", e.Scheduled_Overtime, 1))
      ''  approvalTimeList.Add(New WorkType("Unscheduled OT 1.0", e.Unscheduled_Regular_Overtime, 7))
      ''  approvalTimeList.Add(New WorkType("Unscheduled OT 1.5", e.Unscheduled_Overtime, 8))
      ''  approvalTimeList.Add(New WorkType("Unscheduled OT 2.0", e.Unscheduled_Double_Overtime, 9))
      ''  Select Case e.TelestaffProfileType
      ''    Case TelestaffProfileType.Dispatch, TelestaffProfileType.Field
      ''      approvalTimeList.Add(New WorkType("Holiday", e.Holiday_Paid, 6))
      ''      approvalTimeList.Add(New WorkType("Holiday Time Banked", e.Holiday_Time_Banked, 10))
      ''      approvalTimeList.Add(New WorkType("Holiday Time Used", e.Holiday_Time_Used, 11))

      ''    Case TelestaffProfileType.Office

      ''      approvalTimeList.Add(New WorkType("Comp Time Banked", e.Comp_Time_Banked, 10))
      ''      approvalTimeList.Add(New WorkType("Comp Time Used", e.Comp_Time_Used, 11))
      ''  End Select
      ''Else
      ''  tmp = 80 - e.Vacation.TotalHours - e.Sick.TotalHours - e.Leave_Without_Pay.TotalHours - pnw
      ''  approvalTimeList.Add(New WorkType("Regular Work", tmp, 0, "002"))
      ''End If
      ''If e.Union_Time_Pool.TotalHours() > 0 Then
      ''approvalTimeList.Add(New WorkType("Union Time Pool", e.Union_Time_Pool, 12))
      ''End If
      ''approvalTimeList.Add(New WorkType("Term Hours", e.Term_Hours, 2))
      ''approvalTimeList.Add(New WorkType("LWOP", e.Leave_Without_Pay, 2))
      ''approvalTimeList.Add(New WorkType("Vacation", e.Vacation, 3))
      ''approvalTimeList.Add(New WorkType("Sick", e.Sick, 4))
      approvalTimeList.Clear()
      approvalTimeList.AddRange(calculatedTimeList)
      approvalTimeList.RemoveAll(Function(x) x.hours = 0)


    End Sub

    Private Sub Add_Timelist(e As TC_EPP)
      ' going to capture how much time we pull out for the approvaltimelist
      Dim approvaltime As Double = 0
      ' Now that we've handled the raw time, let's add the processed time
      timeList.Add(New WorkType("Term Hours", e.Term_Hours(0), 2, "095"))
      timeList.Add(New WorkType("LWOP", e.LWOP(0), 2, "090"))
      timeList.Add(New WorkType("Scheduled LWOP", e.Scheduled_LWOP(0), 15, "090"))
      timeList.Add(New WorkType("LWOP - Suspension", e.LWOP_Suspension(0), 16, "090"))
      timeList.Add(New WorkType("Vacation", e.Vacation(0), 3, "100"))
      timeList.Add(New WorkType("Sick", e.Sick(0), 4, "110"))
      timeList.Add(New WorkType("Family Sick Leave", e.Sick_Family_Leave(0), 4, "110"))
      timeList.Add(New WorkType("Vehicle", e.TakeHomeVehicle(0), 15, "046"))


      If e.SickLeavePool(0) > 0 Then
        timeList.Add(New WorkType("Sick Leave Pool", e.SickLeavePool(0), 4, "006"))
        approvalTimeList.Add(New WorkType("Sick Leave Pool", e.SickLeavePool(0), 4, "006"))
        calculatedTimeList.Add(New WorkType("Sick Leave Pool", e.SickLeavePool(0), 4, "006", Payrate))
      End If

      approvalTimeList.Add(New WorkType("LWOP", e.LWOP(0), 2, "090", Payrate))
      approvalTimeList.Add(New WorkType("Term Hours", e.Term_Hours(0), 2, "095", Payrate))
      approvalTimeList.Add(New WorkType("Vacation", e.Vacation(0), 3, "100", Payrate))
      approvalTimeList.Add(New WorkType("Vehicle", e.TakeHomeVehicle(0), 15, "046", Payrate))
      approvalTimeList.Add(New WorkType("Sick", e.Sick(0), 4, "110", Payrate))
      approvalTimeList.Add(New WorkType("Family Sick Leave", e.Sick_Family_Leave(0), 4, "110", Payrate))

      calculatedTimeList.Add(New WorkType("Regular", e.Calculated_Regular(0), 0, "002", Payrate))
      calculatedTimeList.Add(New WorkType("Disaster Regular", e.Calculated_DisasterRegular(0), 0, "299", Payrate))
      calculatedTimeList.Add(New WorkType("Term hours", e.Term_Hours(0), 2, "095", Payrate))
      calculatedTimeList.Add(New WorkType("LWOP", e.LWOP_All(0), 2, "090", Payrate))
      calculatedTimeList.Add(New WorkType("Vacation", e.Vacation(0), 3, "100", Payrate))
      calculatedTimeList.Add(New WorkType("Sick", e.Sick_All(0), 4, "110", Payrate))
      calculatedTimeList.Add(New WorkType("Vehicle", e.TakeHomeVehicle(0), 15, "046", Payrate))


      If e.Holiday(0) > 0 Then
        approvalTimeList.Add(New WorkType("Holiday", e.Holiday(0), 12, "002", Payrate))
        approvaltime += e.Holiday(0)
        timeList.Add(New WorkType("Holiday", e.Holiday(0), 12, "002"))
        If e.HolidayHoursWorked(0) > 0 Then
          timeList.Add(New WorkType("Holiday Hours Worked", e.HolidayHoursWorked(0), 13, "002"))
        End If
      End If
      timeList.Add(New WorkType("Admin", e.Admin(0), 0, "002"))
      timeList.Add(New WorkType("Admin Bereavement", e.Admin_Bereavement(0), 0, "002"))
      timeList.Add(New WorkType("Admin Disaster", e.DisasterAdminLeave(0), 0, "300"))
      timeList.Add(New WorkType("Admin Jury Duty", e.Admin_JuryDuty(0), 0, "002"))
      timeList.Add(New WorkType("Admin Military Leave", e.Admin_MilitaryLeave(0), 0, "002"))
      timeList.Add(New WorkType("Admin Worker's Comp", e.Admin_WorkersComp(0), 0, "002"))
      timeList.Add(New WorkType("Admin Other", e.Admin_Other(0), 0, "002"))

      If Not e.IsExempt Then
        timeList.Add(New WorkType("On Call Hours Worked", e.OnCall_TotalHours(0), 0, "002"))
        timeList.Add(New WorkType("Unscheduled Overtime 2.0", e.Double_Time(0), 9, "232"))
        timeList.Add(New WorkType("Comp Time Banked", e.Comp_Time_Earned(0), 10, "120"))
        timeList.Add(New WorkType("Comp Time Used", e.Comp_Time_Used(0), 11, "121"))
        If e.BreakCredit(0) > 0 Then
          approvalTimeList.Add(New WorkType("Break Credit", e.BreakCredit(0), 12, "002", Payrate))
          approvaltime += e.BreakCredit(0)
          timeList.Add(New WorkType("Break Credit", e.BreakCredit(0), 12, "002"))
        End If
        approvalTimeList.Add(New WorkType("Unscheduled OT 1.0", e.Regular_Overtime(0), 5, "230", Payrate))
        approvalTimeList.Add(New WorkType("Unscheduled OT 1.5", e.Overtime(0), 1, "231", Payrate))
        approvalTimeList.Add(New WorkType("Unscheduled OT 2.0", e.Double_Time(0), 9, "232", Payrate))
        approvalTimeList.Add(New WorkType("Comp Time Banked", e.Comp_Time_Earned(0), 10, "120", Payrate))
        approvalTimeList.Add(New WorkType("Comp Time Used", e.Comp_Time_Used(0), 11, "121", Payrate))
        For a As Integer = 0 To 2
          Dim c As List(Of WorkType) = Nothing
          Select Case a
            Case 0
              c = calculatedTimeList
            Case 1
              c = calculatedTimeList_Week1
            Case 2
              c = calculatedTimeList_Week2
          End Select
          Select Case a
            Case 1, 2
              c.Add(New WorkType("Regular", e.Calculated_Regular(a), 0, "002", Payrate))
              c.Add(New WorkType("Disaster Regular", e.Calculated_DisasterRegular(a), 0, "299", Payrate))
              c.Add(New WorkType("Term Hours", e.Term_Hours(a), 2, "095", Payrate))
              c.Add(New WorkType("LWOP", e.LWOP_All(a), 2, "090", Payrate))
              c.Add(New WorkType("Vacation", e.Vacation(a), 3, "100", Payrate))
              c.Add(New WorkType("Sick", e.Sick_All(a), 4, "110", Payrate))
              c.Add(New WorkType("Vehicle", e.TakeHomeVehicle(a), 15, "046", Payrate))
          End Select
          c.Add(New WorkType("Unscheduled OT 1.0", e.Regular_Overtime(a), 5, "230", Payrate))
          c.Add(New WorkType("Unscheduled OT 1.5", e.Overtime(a), 1, "231", Payrate))
          c.Add(New WorkType("Unscheduled OT 2.0", e.Double_Time(a), 9, "232", Payrate))
          c.Add(New WorkType("Disaster Admin Hours", e.DisasterAdminLeave(a), 11, "300", Payrate))
          c.Add(New WorkType("Disaster Hours 1.5", e.DisasterOverTime(a), 11, "302", Payrate))
          c.Add(New WorkType("Disaster Hours 2.0", e.DisasterDoubleTime(a), 11, "303", Payrate))
          c.Add(New WorkType("Comp Time Banked", e.Comp_Time_Earned(a), 10, "120", Payrate))
          c.Add(New WorkType("Comp Time Used", e.Comp_Time_Used(a), 11, "121", Payrate))
        Next

      Else
        calculatedTimeList.Add(New WorkType("Disaster Admin Hours", e.DisasterAdminLeave(0), 11, "300", Payrate))
        calculatedTimeList.Add(New WorkType("Disaster Hours 1.0", e.DisasterStraightTime(0), 11, "301", Payrate))
        'calculatedTimeList.Add(New WorkType("Disaster Regular", e.DisasterRegular(0), 0, "299", Payrate))
        approvalTimeList.Add(New WorkType("Admin", e.Admin_Total(0), 0, "002", Payrate))
        approvaltime += e.Admin(0)
        'approvaltime += e.DisasterAdminLeave(0)
        'approvaltime += e.Admin_Total(0)

      End If
      'timeList.Add(New WorkType("Regular", Math.Max(e.Regular(0), 0), 0, "002"))
      timeList.Add(New WorkType("Regular", Math.Max(e.Regular(0) + e.DisasterStraightTime(0) + e.DisasterOverTime(0) + e.DisasterDoubleTime(0) - approvaltime, 0), 0, "002"))
      timeList.Add(New WorkType("Disaster Regular", Math.Max(e.DisasterRegular(0), 0), 0, "299"))
      'timeList.Add(New WorkType("Regular", Math.Max(e.Calculated_Regular(0) - approvaltime, 0), 0, "002"))
      timeList.RemoveAll(Function(x) x.hours = 0)

      approvalTimeList.RemoveAll(Function(x) x.hours = 0)
      approvalTimeList.Add(New WorkType("Regular", Math.Max(e.Calculated_Regular(0) - approvaltime, 0), 0, "002", Payrate))

      'If timeList.Sum(Function(n) n.hours) <> approvalTimeList.Sum(Function(j) j.hours) Then
      '  Log(toolsAppId, "Hours do not match",
      '      "timelist hours: " & timeList.Sum(Function(n) n.hours),
      '      "approval hours: " & approvalTimeList.Sum(Function(j) j.hours),
      '      "", LogType.Database)
      'End If

    End Sub

    Private Sub UpdateDisasterData()
      Try
        Dim tmp = (From t In RawTCTD
                   Where t.DisasterName.Length > 0
                   Select t).ToList

        If tmp.Count > 0 Then
          Dim f = tmp.First
          DisasterName_Display = f.DisasterName
          DisasterPeriodType_Display = f.DisasterPeriodtype
        Else
          Dim disasters As List(Of Disaster) = myCache.GetItem("disasterdates")
          Dim foundList As List(Of Disaster) = (From d In disasters
                                                Where payPeriodStart >= d.Disaster_Start And
                                                payPeriodStart <= d.Disaster_End
                                                Order By d.Disaster_Start
                                                Select d).ToList
          If foundList.Count > 0 Then
            Dim f = foundList.First
            DisasterName_Display = f.Name
            DisasterPeriodType_Display = f.Disaster_Period_Type
          End If

        End If
      Catch ex As Exception
        Dim e As New ErrorLog(ex, "")
      End Try
    End Sub


    Private Sub Load_TCTD(Employee As TC_EPP)
      Load_FinanceData(Employee.EmployeeData)
      GroupName = Employee_Data.GetGroupName(employeeID)
      payPeriodStart = Employee.PayPeriodStart
      isExempt = Employee.IsExempt
      WarningList = Employee.WarningList
      ErrorList = Employee.ErrorList
      RawTCTD.AddRange(Employee.TL)

      'load in the disaster info
      UpdateDisasterData()

      Dim holidays As List(Of Date) = getHolidayList(payPeriodStart.Year)
      If payPeriodStart.Year <> payPeriodStart.AddDays(13).Year Then
        holidays.AddRange(getHolidayList(payPeriodStart.AddDays(13).Year))
      End If
      HolidaysInPPD = (From h In holidays
                       Where h >= payPeriodStart And
                       h < payPeriodStart.AddDays(14)
                       Select h.ToShortDateString).ToArray


      For Each t In Employee.TL
        Dim x As New DailyData
        x.comment = t.Comment
        x.workDate = t.WorkDate
        x.workTime = t.WorkTimes
        x.onCallWorkTime = t.OnCallWorkTimes
        If x.workTime.Length > 0 Then
          ' Let's break up the times into groups of a start time and an end time. 
          Dim sList() As String = x.workTime.Split("-"), sTmp As String = ""
          For a As Integer = sList.GetLowerBound(0) To sList.GetUpperBound(0)
            If a Mod 2 = 1 Then
              sTmp &= " - " & sList(a).Trim
              x.workTimeList.Add(New Work(sTmp, ""))
            Else
              sTmp = sList(a).Trim
              If a = sList.GetUpperBound(0) Then x.workTimeList.Add(New Work(sTmp, ""))
            End If
          Next
        End If

        If x.onCallWorkTime.Length > 0 Then
          ' Let's break up the times into groups of a start time and an end time. 
          Dim sList() As String = x.onCallWorkTime.Split("-"), sTmp As String = ""
          For a As Integer = sList.GetLowerBound(0) To sList.GetUpperBound(0)
            If a Mod 2 = 1 Then
              sTmp &= " - " & sList(a).Trim
              x.workTimeList.Add(New Work(sTmp, "On Call Hours, Call Min - " & t.OnCallMinimumHours.ToString))
            Else
              sTmp = sList(a).Trim
              If a = sList.GetUpperBound(0) Then x.workTimeList.Add(New Work(sTmp, "On Call Hours, Call Min - " & t.OnCallMinimumHours.ToString))
            End If
          Next
        End If
        x.workHoursList.AddRange(TimecardTimeDataToWorkTypeList(t))
        RawTime.Add(x)
      Next
      Add_Timelist(Employee)

    End Sub

    Public Function TimecardTimeDataToWorkTypeList(tctd As TimecardTimeData) As List(Of WorkType)
      Dim wcl As New List(Of WorkType)
      If tctd.WorkHours > 0 Then wcl.Add(New WorkType("Regular Work", tctd.WorkHours, 0, ""))
      If tctd.BreakCreditHours > 0 Then wcl.Add(New WorkType("Break Credit", tctd.BreakCreditHours, 1, ""))
      If tctd.HolidayHours > 0 Then wcl.Add(New WorkType("Holiday", tctd.HolidayHours, 2, ""))
      If tctd.VacationHours > 0 Then wcl.Add(New WorkType("Vacation", tctd.VacationHours, 3, ""))
      Dim TotalSick As Double = tctd.SickHours + tctd.SickLeavePoolHours + tctd.SickFamilyLeave
      If TotalSick > 0 Then wcl.Add(New WorkType("Sick", TotalSick, 4, ""))
      Dim totalLWOP As Double = tctd.LWOPHours + tctd.LWOPSuspensionHours + tctd.ScheduledLWOPHours
      If tctd.TermHours > 0 Then wcl.Add(New WorkType("TermHours", tctd.TermHours, 12, ""))
      If totalLWOP > 0 Then wcl.Add(New WorkType("LWOP", totalLWOP, 12, ""))
      If tctd.CompTimeEarned > 0 Then wcl.Add(New WorkType("Comp Time Earned", tctd.CompTimeEarned, 5, ""))
      If tctd.CompTimeUsed > 0 Then wcl.Add(New WorkType("Comp Time Used", tctd.CompTimeUsed, 6, ""))
      If tctd.DoubleTimeHours > 0 Then wcl.Add(New WorkType("Double Time", tctd.DoubleTimeHours, 7, ""))
      Dim TotalAdmin As Double = tctd.AdminBereavement + tctd.AdminDisaster + tctd.AdminJuryDuty + tctd.AdminMilitaryLeave + tctd.AdminOther + tctd.AdminHours + tctd.AdminWorkersComp
      If TotalAdmin > 0 Then wcl.Add(New WorkType("Admin", TotalAdmin, 8, ""))
      'If tctd.AdminBereavement > 0 Then wcl.Add(New WorkType("Admin", tctd.AdminHours, 8, ""))
      If tctd.OnCallTotalHours > 0 Then wcl.Add(New WorkType("Call Adjusted Hours", tctd.OnCallTotalHours, 9, ""))
      Check_Work_Types(wcl)
      Return wcl
    End Function

    Private Sub Check_Work_Type(wt As WorkType)
      If (From u In _used_Types Where u.name = wt.name Select u).Count = 0 Then
        Dim x As New WorkType(wt.name, 0, wt.leftToRightOrder, wt.payCode)
        _used_Types.Add(x)
      End If
    End Sub

    Private Sub Check_Work_Types(wtl As List(Of WorkType))
      For Each wt In wtl
        If (From u In _used_Types Where u.name = wt.name Select u).Count = 0 Then
          Dim x As New WorkType(wt.name, 0, wt.leftToRightOrder, wt.payCode)
          _used_Types.Add(x)
        End If
      Next
    End Sub

  End Class
End Namespace