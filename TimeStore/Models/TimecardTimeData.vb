Namespace Models
  Public Class TimecardTimeData
    Public Enum TimeCardDataSource
      Timecard = 0
      TimeStore = 1
    End Enum
    Property DataSource As TimeCardDataSource = TimeCardDataSource.Timecard
    Property EmployeeID As Integer
    Property DepartmentNumber As String = ""
    Property WorkDate As Date
    Property WorkTimes As String = ""
    Property WorkHours As Double = 0
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
    Property OutOfClass As New Out_Of_Class

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
    End Sub

    Private Sub Load_Hours_To_Approve(STDTA As List(Of Saved_TimeStore_Data_To_Approve))
      Dim ignoreList() As Integer = {10, 11}
      IsApproved = ((From std In STDTA Where Not ignoreList.Contains(std.field_id) And
                                                 Not std.is_approved Select std).Count = 0)
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

    Public Function CreateTimesOutput() As Dictionary(Of String, String)
      Dim d As New Dictionary(Of String, String)
      d.Add("OnCallWorkTimes", OnCallWorkTimes)
      d.Add("WorkTimes", WorkTimes)
      Return d
    End Function

    Public Function Validate() As Boolean
      If Comment Is Nothing Then Comment = ""
      If WorkTimes Is Nothing Then WorkTimes = ""
      If OnCallWorkTimes Is Nothing Then OnCallWorkTimes = ""
      If Vehicle <> 0 AndAlso Vehicle <> 1 Then
        Return False
      End If
      For Each kvp As KeyValuePair(Of String, Timestore_Field_With_Hours) In CreateHoursOutput()
        If kvp.Value.Field_Hours Mod 0.25 > 0 Then Return False
        If kvp.Value.Field_Hours > 24 Or kvp.Value.Field_Hours < 0 Then Return False
      Next
      If EmployeeID = 0 Then Return False
      Return True
    End Function
  End Class
End Namespace