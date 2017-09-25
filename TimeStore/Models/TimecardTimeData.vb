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
    Property DisasterWorkTimes As String = ""
    Property DisasterWorkHours As Double = 0
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
    Property OutOfClass As New Out_Of_Class

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
      DisasterWorkHours = STD.disaster_work_hours
      DisasterWorkTimes = STD.disaster_work_times
      DisasterRule = STD.disaster_rule
    End Sub

    Private Sub Load_Hours_To_Approve(STDTA As List(Of Saved_TimeStore_Data_To_Approve))
      Dim ignoreList() As Integer = {10, 11}
      IsApproved = ((From std In STDTA
                     Where Not ignoreList.Contains(std.field_id) And
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

    Public Function CreateTimesOutput() As Dictionary(Of String, String)
      Dim d As New Dictionary(Of String, String)
      d.Add("OnCallWorkTimes", OnCallWorkTimes)
      d.Add("WorkTimes", WorkTimes)
      Return d
    End Function

    Public Function Validate() As Boolean
      If DisasterWorkTimes Is Nothing Then DisasterWorkTimes = ""
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

    '    Public Function Save(tca As Timecard_Access) As Boolean


    '      ' here's what this process is going to do:
    '      'User chooses to save record
    '      'check If work record exists
    '      Dim existing = To_Saved_TimeStore_Data()
    '      'New Saved_TimeStore_Data(T.EmployeeID, T.WorkDate)
    '      'Dim existing = Get_Saved_Timestore_Data_by_Date(T.EmployeeID, T.WorkDate)

    '      If existing.employee_id = 0 Then ' record does Not exist

    '        Dim workID As Long = Save_Timestore_Data(Me, tca) '   Insert New work record, return work record ID
    '        If Save_Hours_To_Approve(Me, workID, tca) Then
    '          ' If this user's leave doesn't require approval, let's go ahead and approve everything.
    '          If Not tca.RequiresApproval Then
    '            existing = To_Saved_TimeStore_Data()
    '            'New Saved_TimeStore_Data(T.EmployeeID, T.WorkDate)
    '            For Each hta In existing.HoursToApprove
    '              Finalize_Leave_Request(True, hta.approval_hours_id, hta.hours_used, "", tca)
    '            Next
    '          Else
    '            'let's remove any current approvals.
    '            Dim payperiodstart As Date = GetPayPeriodStart(T.WorkDate)
    '            If Clear_Saved_Timestore_Data(T.EmployeeID, payperiodstart) = -1 Then
    '              Add_Timestore_Note(T.EmployeeID, payperiodstart.AddDays(13), "Approval Removed, Hours or Payrate has changed.")
    '            End If
    '          End If
    '          Return True
    '        Else
    '          Return False
    '        End If


    '        ' 8/26/2015 Going to skip the auto approving.
    '        '   check person saving rows' approval level (Check Authority)
    '        '       If they Then have the authority To "pre-approve" the time
    '        '		    insert the hours into the hours To approve table Using the work record id, capturing the ID inserted.
    '        '           Use the returned ID to save the approval data for each hours to add the rows to approve record.

    '        '       If they do not have the authority to pre-approve
    '        '           insert the hours into the hours to approve table using the work record id

    '      Else ' record exists

    '        If Not Update_Timestore_Work_Data(T, tca, existing) Then Return False

    '        Select Case Update_Hours_To_Approve(T, tca, existing)
    '          Case 5 ' data was updated and everything was good.
    '            'let's remove any current approvals.
    '            Dim payperiodstart As Date = GetPayPeriodStart(T.WorkDate)
    '            If Clear_Saved_Timestore_Data(T.EmployeeID, payperiodstart) = -1 Then
    '              Add_Timestore_Note(T.EmployeeID, payperiodstart.AddDays(13), "Approval Removed, Hours or Payrate has changed. **")
    '            End If
    '          Case 2 ' data wasn't updated but it completed normally
    '            If Not Compare_Existing_To_Current(existing, T) Then
    '              Dim payperiodstart As Date = GetPayPeriodStart(T.WorkDate)
    '              If Clear_Saved_Timestore_Data(T.EmployeeID, payperiodstart) = -1 Then
    '                Add_Timestore_Note(T.EmployeeID, payperiodstart.AddDays(13), "Approval Removed, Hours or Payrate has changed. *")
    '              End If
    '            End If
    '          Case 1 ' data was updated but there was an error
    '            Return False
    '          Case -1 ' error
    '            Return False
    '        End Select
    '        If Not tca.RequiresApproval Then
    '          existing = T.To_Saved_TimeStore_Data
    '          'New Saved_TimeStore_Data(T.EmployeeID, T.WorkDate)
    '          For Each hta In existing.HoursToApprove
    '            If Not hta.is_approved Then Finalize_Leave_Request(True, hta.approval_hours_id, hta.hours_used, "", tca)
    '          Next
    '        End If
    '        Return True

    '        '   Return ID Of existing data
    '        '       get list of hours to approve rows with that ID
    '        '       get list of approvals that match those IDs
    '        '       if the hours to approve are already approved and the new hours are greater than the hours approved
    '        '           then remove the approval.
    '        '       update work record with New data (ID stays the same), update date_last_updated with current date/time
    '        '       Loop through each Hours to approve row And update the used_hours And worktimes field to match the current saved value, unless it Is denied, then enforce it to zero.

    '        '       insert any New hours to approve that were Not present

    '      End If




    '    End Function

    '    Private Function SaveWorkData(tca As Timecard_Access)
    '      Dim query As String = "


    '"

    '    End Function
  End Class
End Namespace