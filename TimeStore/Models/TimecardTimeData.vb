Imports System.Data.SqlClient

Namespace Models

  Public Class TimecardTimeData
    Public Enum TimeCardDataSource
      Timecard = 0
      TimeStore = 1
      Telestaff = 2
    End Enum
    Property DataSource As TimeCardDataSource = TimeCardDataSource.Timecard
    Property WorkHoursID As Long = 0
    Property EmployeeID As Integer = 0
    Property DepartmentNumber As String = ""
    Property WorkDate As Date
    Property WorkTimes As String = ""
    Property WorkHours As Double = 0
    Property DisasterName As String = ""
    Property DisasterPeriodType As Integer = 0
    Property DisasterWorkTimes As String = ""
    Property DisasterWorkHours As Double = 0
    Property DisasterWorkType As String = ""
    Property DisasterRule As Integer = 0
    Property BreakCreditHours As Double = 0
    Property HolidayHours As Double = 0
    Property VacationHours As Double = 0
    Property SickHours As Double = 0
    Property SickFamilyLeave As Double = 0
    Property SickLeavePoolHours As Double = 0
    Property CompTimeUsed As Double = 0
    Property CompTimeEarned As Double = 0
    Property AdminHours As Double = 0
    Property AdminBereavement As Double = 0
    Property AdminDisaster As Double = 0
    Property AdminWorkersComp As Double = 0
    Property AdminJuryDuty As Double = 0
    Property AdminMilitaryLeave As Double = 0
    Property AdminOther As Double = 0
    Property ScheduledLWOPHours As Double = 0
    Property LWOPSuspensionHours As Double = 0
    Property LWOPHours As Double = 0
    Property DoubleTimeHours As Double = 0
    Property TotalHours As Double = 0
    Property PPD As String = ""
    Property Comment As String = ""
    Property Vehicle As Integer = 0
    Property OnCallMinimumHours As Double = 0
    Property OnCallWorkHours As Double = 0
    Property OnCallWorkTimes As String = ""
    Property OnCallTotalHours As Double = 0
    Property TermHours As Double = 0
    Property TerminationDate As Date = Date.MaxValue
    Property IsApproved As Boolean = True
    Property OutOfClass As Boolean = False
    ReadOnly Property Total_Non_Working_Hours As Double
      Get
        Return VacationHours + SickHours + SickFamilyLeave + SickLeavePoolHours + CompTimeUsed + AdminHours +
          AdminBereavement + AdminDisaster + AdminWorkersComp + AdminJuryDuty + AdminMilitaryLeave +
          AdminOther + ScheduledLWOPHours + LWOPSuspensionHours + LWOPHours
      End Get
    End Property

    Public Function To_Saved_TimeStore_Data() As Saved_TimeStore_Data
      If EmployeeID = 0 Then
        Return New Saved_TimeStore_Data
      Else
        Return Saved_TimeStore_Data.GetByEmployeeAndWorkday(Me.WorkDate, Me.EmployeeID)
      End If
    End Function

    Public Sub New()

    End Sub

    Public Sub New(STD As Saved_TimeStore_Data)
      ' The only thing not loaded via this method is the Comp Time Earned
      ' Comp Time Earned will be loaded once at the Pay Period level, not at the day level, which this is.
      Load_Saved_Timestore_Data(STD)
      Load_Hours_To_Approve(STD.HoursToApprove)
    End Sub

    Private Sub Load_Saved_Timestore_Data(STD As Saved_TimeStore_Data)
      DataSource = TimeCardDataSource.TimeStore
      EmployeeID = STD.employee_id
      DepartmentNumber = STD.dept_id
      BreakCreditHours = STD.break_credit
      Comment = STD.comment
      HolidayHours = STD.holiday
      LWOPHours = STD.leave_without_pay
      TotalHours = STD.total_hours
      DoubleTimeHours = STD.doubletime_hours
      Vehicle = STD.vehicle
      WorkDate = STD.work_date
      WorkHours = STD.work_hours
      WorkTimes = STD.work_times
      DisasterName = STD.disaster_name
      DisasterPeriodType = STD.disaster_period_type
      DisasterWorkHours = STD.disaster_work_hours
      DisasterWorkTimes = STD.disaster_work_times
      DisasterWorkType = STD.disaster_work_type
      DisasterRule = STD.disaster_rule
      OutOfClass = STD.out_of_class
    End Sub

    Private Sub Load_Hours_To_Approve(STDTA As List(Of Saved_TimeStore_Data_To_Approve))
      Dim ignoreList() As Integer = {10, 11}
      IsApproved = ((From std In STDTA
                     Where Not ignoreList.Contains(std.field_id) And
                       Not std.is_approved
                     Select std).Count = 0)
      For Each s In STDTA
        Select Case s.field_id
          Case 1
            WorkHours = s.hours_used
          Case 2
            VacationHours = s.hours_used
          Case 3
            SickHours = s.hours_used
          Case 4
            CompTimeUsed = s.hours_used
          Case 5
            AdminBereavement = s.hours_used
          Case 6
            AdminJuryDuty = s.hours_used
          Case 7
            AdminMilitaryLeave = s.hours_used
          Case 8
            AdminWorkersComp = s.hours_used
          Case 9
            AdminOther = s.hours_used
          Case 10
            OnCallMinimumHours = s.hours_used
          Case 11
            OnCallWorkHours = s.hours_used
          Case 13
            OnCallTotalHours = s.hours_used
            OnCallWorkTimes = s.worktimes
          Case 14
            SickLeavePoolHours = s.hours_used
          Case 15
            LWOPSuspensionHours = s.hours_used
          Case 16
            ScheduledLWOPHours = s.hours_used
          Case 17
            SickFamilyLeave = s.hours_used
          Case 18
            TermHours = s.hours_used
          Case 19
            AdminDisaster = s.hours_used
        End Select
      Next
    End Sub

    Public ReadOnly Property TerminationDateDisplay As String
      Get
        Return TerminationDate.ToShortDateString
      End Get
    End Property

    Public ReadOnly Property IsTerminated As Boolean
      Get
        Return TerminationDate <> Date.MaxValue
      End Get
    End Property

    Public Function CreateHoursOutput() As Dictionary(Of String, Timestore_Field_With_Hours)
      Dim TSF As Dictionary(Of String, Timestore_Field) = Get_Cached_Timestore_Fields_By_Name()
      Dim d As New Dictionary(Of String, Timestore_Field_With_Hours)
      ' I will need to add in Work Hours once I know how it's going to work with the payrate change.
      'd.Add("WorkHours", New Timestore_Field_With_Hours(TSF("WorkHours"), WorkHours))
      d.Add("VacationHours", New Timestore_Field_With_Hours(TSF("VacationHours"), VacationHours))
      d.Add("SickHours", New Timestore_Field_With_Hours(TSF("SickHours"), SickHours))
      d.Add("CompTimeUsed", New Timestore_Field_With_Hours(TSF("CompTimeUsed"), CompTimeUsed))
      d.Add("SickLeavePoolHours", New Timestore_Field_With_Hours(TSF("SickLeavePoolHours"), SickLeavePoolHours))
      d.Add("OnCallMinimumHours", New Timestore_Field_With_Hours(TSF("OnCallMinimumHours"), OnCallMinimumHours))
      d.Add("OnCallWorkHours", New Timestore_Field_With_Hours(TSF("OnCallWorkHours"), OnCallWorkHours))
      d.Add("OnCallTotalHours", New Timestore_Field_With_Hours(TSF("OnCallTotalHours"), OnCallTotalHours))

      d.Add("AdminBereavement", New Timestore_Field_With_Hours(TSF("AdminBereavement"), AdminBereavement))
      d.Add("AdminDisaster", New Timestore_Field_With_Hours(TSF("AdminDisaster"), AdminDisaster))
      d.Add("AdminJuryDuty", New Timestore_Field_With_Hours(TSF("AdminJuryDuty"), AdminJuryDuty))
      d.Add("AdminMilitaryLeave", New Timestore_Field_With_Hours(TSF("AdminMilitaryLeave"), AdminMilitaryLeave))
      d.Add("AdminWorkersComp", New Timestore_Field_With_Hours(TSF("AdminWorkersComp"), AdminWorkersComp))
      d.Add("AdminOther", New Timestore_Field_With_Hours(TSF("AdminOther"), AdminOther))
      d.Add("LWOPSuspension", New Timestore_Field_With_Hours(TSF("LWOPSuspension"), LWOPSuspensionHours))
      d.Add("ScheduledLWOP", New Timestore_Field_With_Hours(TSF("ScheduledLWOP"), ScheduledLWOPHours))
      d.Add("SickFamilyLeave", New Timestore_Field_With_Hours(TSF("SickFamilyLeave"), SickFamilyLeave))
      d.Add("TermHours", New Timestore_Field_With_Hours(TSF("TermHours"), TermHours))
      'd.Add("AdminHours", New Timestore_Field_With_Hours(TSF("AdminHours"), AdminHours))
      'd.Add("TotalHours", New Timestore_Field_With_Hours(TSF("WorkHours"), TotalHours))
      'd.Add("BreakCreditHours", New Timestore_Field_With_Hours(TSF("WorkHours"), BreakCreditHours)
      'd.Add("LWOPHours", New Timestore_Field_With_Hours(TSF("WorkHours"), LWOPHours)
      'd.Add("HolidayHours", New Timestore_Field_With_Hours(TSF("WorkHours"), HolidayHours)
      'd.Add("CompTimeEarned", CompTimeEarned)
      'd.Add("DoubleTimeHours", DoubleTimeHours)

      Return d
    End Function

    'Public Function CreateTimesOutput() As Dictionary(Of String, String)
    '  Dim d As New Dictionary(Of String, String)
    '  d.Add("OnCallWorkTimes", OnCallWorkTimes)
    '  d.Add("WorkTimes", WorkTimes)
    '  Return d
    'End Function

    Public Function Validate(myTca As Timecard_Access, Username As String) As Boolean
      If DisasterWorkTimes Is Nothing Then DisasterWorkTimes = ""
      If DisasterWorkType Is Nothing Then DisasterWorkType = ""
      If Comment Is Nothing Then Comment = ""
      If WorkTimes Is Nothing Then WorkTimes = ""
      If OnCallWorkTimes Is Nothing Then OnCallWorkTimes = ""
      Select Case DepartmentNumber
        Case "3701", "3711", "3712"
        Case Else
          OutOfClass = False
      End Select
      'If DepartmentNumber <> "3701" Then OutOfClass = False

      If Vehicle <> 0 AndAlso Vehicle <> 1 Then
        Return False
      End If
      For Each kvp As KeyValuePair(Of String, Timestore_Field_With_Hours) In CreateHoursOutput()
        If kvp.Value.Field_Hours Mod 0.25 > 0 Then Return False
        If kvp.Value.Field_Hours > 24 Or kvp.Value.Field_Hours < 0 Then Return False
      Next
      If EmployeeID = 0 Then Return False
      Select Case DepartmentNumber
        Case "3701", "3711", "3712"
          Dim std = Saved_TimeStore_Data.GetByEmployeeAndWorkday(WorkDate, EmployeeID)
          If OutOfClass Or std.out_of_class Then
            ' let's check the current value of this in the db
            ' if we are changing it, let's add a note
            If OutOfClass <> std.out_of_class And myTca.Raw_Access_Type > 1 Then
              Dim ppe = GetPayPeriodStart(WorkDate).AddDays(13)
              Dim note = ""
              If OutOfClass Then
                note = "Added Out of Class Pay for " & WorkDate.ToShortDateString & "."
              Else
                note = "Removed Out of Class Pay for " & WorkDate.ToShortDateString & "."
              End If
              Add_Timestore_Note(EmployeeID, ppe, note, Username)
            End If
          End If
      End Select
      Return True
    End Function

    Public Function Save(tca As Timecard_Access) As Boolean

      Dim std As New Saved_TimeStore_Data(Me, tca)
      Dim dt As DataTable = PopulateData()

      Dim dp As New DynamicParameters()

      ' Add an output parameter to find out if the transaction completed.
      ' update the stored procedure and turn it into a transaction that 
      ' can be rolled back.
      dp.Add("@employee_id", std.employee_id)
      dp.Add("@dept_id", std.dept_id)
      dp.Add("@pay_period_ending", std.pay_period_ending)
      dp.Add("@work_date", std.work_date)
      dp.Add("@work_times", std.work_times)
      dp.Add("@disaster_work_times", std.disaster_work_times)
      dp.Add("@disaster_work_hours", std.disaster_work_hours)
      dp.Add("@disaster_work_type", std.disaster_work_type)
      dp.Add("@break_credit", std.break_credit)
      dp.Add("@work_hours", std.work_hours)
      dp.Add("@holiday", std.holiday)
      dp.Add("@leave_without_pay", std.leave_without_pay)
      dp.Add("@total_hours", std.total_hours)
      dp.Add("@doubletime_hours", std.doubletime_hours)
      dp.Add("@vehicle", std.vehicle)
      dp.Add("@comment", std.comment)
      dp.Add("@out_of_class", std.out_of_class)
      dp.Add("@by_employeeid", std.by_employeeid)
      dp.Add("@by_username", std.by_username)
      dp.Add("@by_machinename", std.by_machinename)
      dp.Add("@by_ip_address", std.by_ip_address)
      dp.Add("@result", dbType:=DbType.Int32, direction:=ParameterDirection.Output)
      dp.Add("@HTA", dt.AsTableValuedParameter("HoursToApproveData"))

      Try

        Using db As IDbConnection = New SqlConnection(GetCS(ConnectionStringType.Timestore))
          Dim i = db.Execute("SaveTimecardDay", dp, commandType:=CommandType.StoredProcedure)
          Dim result As Integer = dp.Get(Of Integer)("@result")
          Select Case result
            'Case 0 ' nothing happened
            '  Return True
            'Case -1 ' an error happened
            '  Return False
            Case 0, 1 ' the transaction was committed
              Return True
            Case Else
              Dim e As New ErrorLog("TimecardTimeData.Save error", std.employee_id.ToString, std.work_date.ToShortDateString, "", "")
              Return False
          End Select
        End Using
      Catch ex As Exception
        Dim e As New ErrorLog(ex, "SaveTimecardDay")
        Return False
      End Try
    End Function

    Private Shared Function BuildDataTable() As DataTable
      Dim dt As New DataTable("HoursToApproveData")
      'dt.Columns.Add("work_hours_id", Type.GetType("System.Int64"))
      dt.Columns.Add("field_id", Type.GetType("System.Int16"))
      dt.Columns.Add("work_times", Type.GetType("System.String"))
      dt.Columns.Add("hours_used", Type.GetType("System.Double"))
      Return dt
    End Function

    Private Function PopulateData() As DataTable
      Dim dt As DataTable = BuildDataTable()

      If VacationHours > 0 Then
        dt.Rows.Add(2, "", VacationHours)
      End If
      If SickHours > 0 Then
        dt.Rows.Add(3, "", SickHours)
      End If
      If CompTimeUsed > 0 Then
        dt.Rows.Add(4, "", CompTimeUsed)
      End If
      If AdminBereavement > 0 Then
        dt.Rows.Add(5, "", AdminBereavement)
      End If
      If AdminJuryDuty > 0 Then
        dt.Rows.Add(6, "", AdminJuryDuty)
      End If
      If AdminMilitaryLeave > 0 Then
        dt.Rows.Add(7, "", AdminMilitaryLeave)
      End If
      If AdminWorkersComp > 0 Then
        dt.Rows.Add(8, "", AdminWorkersComp)
      End If
      If AdminOther > 0 Then
        dt.Rows.Add(9, "", AdminOther)
      End If
      If OnCallMinimumHours > 0 Then
        dt.Rows.Add(10, "", OnCallMinimumHours)
      End If
      If OnCallWorkHours > 0 Then
        dt.Rows.Add(11, "", OnCallWorkHours)
      End If
      If OnCallTotalHours > 0 Or OnCallWorkTimes.Length > 0 Then
        dt.Rows.Add(13, OnCallWorkTimes, OnCallTotalHours)
      End If
      If SickLeavePoolHours > 0 Then
        dt.Rows.Add(14, "", SickLeavePoolHours)
      End If
      If LWOPSuspensionHours > 0 Then
        dt.Rows.Add(15, "", LWOPSuspensionHours)
      End If
      If ScheduledLWOPHours > 0 Then
        dt.Rows.Add(16, "", ScheduledLWOPHours)
      End If
      If SickFamilyLeave > 0 Then
        dt.Rows.Add(17, "", SickFamilyLeave)
      End If
      If TermHours > 0 Then
        dt.Rows.Add(18, "", TermHours)
      End If
      If AdminDisaster > 0 Then
        dt.Rows.Add(19, "", AdminDisaster)
      End If
      Return dt
    End Function

  End Class
End Namespace