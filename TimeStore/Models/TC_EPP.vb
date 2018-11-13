Namespace Models
  Public Class TC_EPP
    Public PayPeriodStart As Date = GetPayPeriodStart(Today)
    Public EmployeeData As FinanceData
    Public TL As List(Of TimecardTimeData)
    Public CompTimeEarnedWeek1 As Double = 0
    Public CompTimeEarnedWeek2 As Double = 0
    Public WarningList As New List(Of String)
    Public ErrorList As New List(Of String)
    Public DisasterHoursValue() As Double = {Double.MinValue, Double.MinValue}

    Public Sub New(ByRef TCTD As List(Of TimecardTimeData), F As FinanceData, pps As Date, Optional CTE As Saved_TimeStore_Comp_Time_Earned = Nothing)
      EmployeeData = F
      TL = TCTD
      PayPeriodStart = pps
      If Not CTE Is Nothing Then
        If Overtime(1) < CTE.comp_time_earned_week1 Or Overtime(2) < CTE.comp_time_earned_week2 Then
          CTE.Delete()
        Else
          CompTimeEarnedWeek1 = CTE.comp_time_earned_week1
          CompTimeEarnedWeek2 = CTE.comp_time_earned_week2
        End If
      End If
      Catch_Exceptions()
    End Sub

    Public Sub New(ByRef TCTD As List(Of TimecardTimeData), F As FinanceData, pps As Date)
      EmployeeData = F
      TL = TCTD
      PayPeriodStart = pps
      If TCTD.Count > 0 Then
        If TCTD.First.DataSource = TimecardTimeData.TimeCardDataSource.TimeStore Then
          Dim CTE As New Saved_TimeStore_Comp_Time_Earned(F.EmployeeId, pps.AddDays(13))
          If Overtime(1) < CTE.comp_time_earned_week1 Or Overtime(2) < CTE.comp_time_earned_week2 Then
            CTE.Delete()
          Else
            CompTimeEarnedWeek1 = CTE.comp_time_earned_week1
            CompTimeEarnedWeek2 = CTE.comp_time_earned_week2
          End If
        End If
      End If
      Catch_Exceptions()
    End Sub

    ReadOnly Property IsExempt() As Boolean
      Get
        Return (EmployeeData.EmployeeType = "E")
      End Get
    End Property

    Public ReadOnly Property Total_Hours(Week As Integer)
      Get
        Return (From t In Week_TL(Week) Select t.TotalHours).Sum
      End Get
    End Property

    Private ReadOnly Property Week_TL(Week As Integer) As List(Of TimecardTimeData)
      Get
        Select Case Week
          Case 1
            Return (From t In TL
                    Where t.WorkDate >= PayPeriodStart And
                      t.WorkDate < PayPeriodStart.AddDays(7)
                    Select t).ToList
          Case 2
            Return (From t In TL
                    Where t.WorkDate >= PayPeriodStart.AddDays(7) And
                      t.WorkDate < PayPeriodStart.AddDays(14)
                    Select t).ToList
          Case Else
            Return TL
        End Select
      End Get
    End Property

    Public ReadOnly Property HolidayHoursMoved(Week As Integer) As Double
      Get
        Dim Holidays As List(Of Date) = getHolidayList(PayPeriodStart.Year)
        ' This is incase the pay period spans the year's end.  
        ' This is important because we have multiple holidays at the end
        ' of the year and then one right in the beginning.
        If PayPeriodStart.Year <> PayPeriodStart.AddDays(13).Year Then
          Holidays.AddRange(getHolidayList(PayPeriodStart.AddDays(13).Year))
        End If
        Return (From w In Week_TL(Week) Where w.HolidayHours > 0 And
                                          Not Holidays.Contains(w.WorkDate)
                Select w.HolidayHours).Sum
      End Get
    End Property

    Public ReadOnly Property HolidayHoursWorked(Week As Integer) As Double
      Get
        Return (From w In Week_TL(Week) Where w.HolidayHours > 0 And
                                          w.WorkHours > 0 Select w.WorkHours).Sum
      End Get
    End Property

    Public ReadOnly Property HolidayHoursWorkedDifference(Week As Integer) As Double
      Get
        Return (From w In Week_TL(Week) Where w.HolidayHours > 0 And w.WorkHours > 0
                Select w.WorkHours - w.DoubleTimeHours).Sum
      End Get
    End Property

    Public ReadOnly Property Regular(Week As Integer) As Double
      Get

        'DisasterHours = (From t In Week_TL)
        Return Math.Max((From t In Week_TL(Week)
                         Select t.WorkHours).Sum +
            BreakCredit(Week) + Holiday(Week) + OnCall_HoursWorked(Week) -
            HolidayHoursWorkedDifference(Week) - DisasterDoubleTime(Week) -
            DisasterStraightTime(Week) -
            DisasterOverTime(Week) - Math.Max(DisasterRegular(Week), 0), 0)
        ' old version to test on call hour change
        'Return (From t In Week_TL(Week) Select t.WorkHours).Sum +
        '    BreakCredit(Week) + Holiday(Week) + OnCall_TotalHours(Week) -
        '    HolidayHoursWorkedDifference(Week)


        'If HolidayHoursWorked(Week) > 0 Then
        '  Return (From t In Week_TL(Week) Select t.WorkHours).Sum +
        '    BreakCredit(Week) + Holiday(Week) + OnCall_TotalHours(Week) -
        '    HolidayHoursWorkedDifference(Week)

        'Else
        '  Return (From t In Week_TL(Week) Select t.WorkHours).Sum +
        '    BreakCredit(Week) + Holiday(Week) + OnCall_TotalHours(Week) 
        'End If
      End Get
    End Property

    Public ReadOnly Property Calculated_Regular(Week As Integer) As Double
      Get
        Select Case Week
          Case 1, 2
            Dim val As Double = Total_Hours(Week) - DisasterDoubleTime(Week) -
              DisasterOverTime(Week) - DisasterStraightTime(Week)
            ' Introducing change to handle people who are exempt and work less than 40 hours per week.
            'If val > 40 Then val = 40
            'If EmployeeData.EmployeeType = "E" And val > 0 And val < 40 Then val = 40
            ' There is one person currently who is exempt but is only paid for 12 hours per pay period. 
            ' Any hours in addition to that are reduced to 12.
            If Term_Hours(0) = 0 Then

              If IsExempt AndAlso EmployeeData.HireDate < PayPeriodStart Then
                Dim HoursForOTByWeek As Double = EmployeeData.HoursNeededForOvertime / 2
                If val > HoursForOTByWeek Then val = HoursForOTByWeek
                If val > 0 And val < HoursForOTByWeek Then val = HoursForOTByWeek
              Else
                'If DisasterHours(Week) > 0 Then
                '  val = val - Double_Time(Week) - DisasterHours(Week) - DisasterOT(Week)
                'End If
                If val > 40 Then
                  val = 40
                End If


              End If

            End If

            val = val - Sick_All(Week) - Vacation(Week) - Comp_Time_Used(Week) -
              LWOP_All(Week) - SickLeavePool(Week) - Term_Hours(Week) -
              Calculated_DisasterRegular(Week) - DisasterAdminLeave(Week) - Out_Of_Class(Week)
            If val < 0 Then val = 0
            Return val
          Case Else
            Return Calculated_Regular(1) + Calculated_Regular(2)
        End Select

      End Get
    End Property

    Public ReadOnly Property Out_Of_Class(Week As Integer) As Double
      Get
        Select Case Week
          Case 1, 2
            Dim PubWorksDepartments() As String = {"3701", "3711", "3712"}
            Return (From t In Week_TL(Week)
                    Where t.OutOfClass And PubWorksDepartments.Contains(t.DepartmentNumber)
                    Select t.WorkHours).Sum

          Case Else
            Return Out_Of_Class(1) + Out_Of_Class(2)
        End Select

      End Get
    End Property


    Public ReadOnly Property DisasterRegular(week As Integer) As Double
      Get
        Return (From t In Week_TL(week)
                Where t.DisasterRule = 2
                Select t.DisasterWorkHours).Sum -
                DisasterOverTime(week) -
                DisasterAdminLeave(week)

      End Get
    End Property

    Public ReadOnly Property Calculated_DisasterRegular(Week As Integer) As Double
      Get
        Select Case Week
          Case 1, 2
            'Dim val As Double = Total_Hours(Week) - DisasterDoubleTime(Week) - DisasterOverTime(Week) - DisasterStraightTime(Week)
            Dim val As Double = DisasterRegular(Week)
            If Term_Hours(0) = 0 Then
              If IsExempt AndAlso EmployeeData.HireDate < PayPeriodStart Then
                Dim HoursForOTByWeek As Double = EmployeeData.HoursNeededForOvertime / 2
                If val > HoursForOTByWeek Then val = HoursForOTByWeek
                If val > 0 And val < HoursForOTByWeek Then val = HoursForOTByWeek
              Else
                If val > 40 Then
                  val = 40
                End If
              End If
              'If val > DisasterRegular(Week) Then
              '  val = DisasterRegular(Week)
              'End If
            End If
            val = val - Sick_All(Week) - Vacation(Week) - Comp_Time_Used(Week) -
              LWOP_All(Week) - SickLeavePool(Week) - Term_Hours(Week) - DisasterAdminLeave(Week)
            Return Math.Max(val, 0)
          Case Else
            Return Calculated_DisasterRegular(1) + Calculated_DisasterRegular(2)
        End Select
      End Get
    End Property

    Public ReadOnly Property DisasterDoubleTime(Week As Integer) As Double
      Get
        If IsExempt Then
          Return 0
        Else
          Return (From t In Week_TL(Week)
                  Where t.DisasterRule = 1
                  Select t.WorkHours + t.BreakCreditHours).Sum

        End If
      End Get
    End Property

    Public ReadOnly Property DisasterOverTime(Week As Integer) As Double
      Get
        If IsExempt Then
          Return 0
        Else
          Return (From t In Week_TL(Week)
                  Where t.DisasterRule = 2 And
                      t.WorkHours + t.BreakCreditHours > 8 And
                    t.DoubleTimeHours = 0
                  Select t.WorkHours + t.BreakCreditHours - 8).Sum
        End If
      End Get
    End Property

    Public ReadOnly Property DisasterAdminLeave(Week As Integer) As Double
      Get
        Return (From t In Week_TL(Week)
                Select t.AdminDisaster).Sum
      End Get
    End Property

    Public ReadOnly Property DisasterStraightTime(Week As Integer) As Double
      Get
        If IsExempt Then
          Dim weekday = (From t In Week_TL(Week)
                         Where t.DisasterRule = 1 And
                           t.WorkHours > 8 And
                           t.WorkDate.DayOfWeek <> DayOfWeek.Saturday And
                           t.WorkDate.DayOfWeek <> DayOfWeek.Sunday
                         Select t.WorkHours - 8).Sum
          Dim weekend = (From t In Week_TL(Week)
                         Where t.DisasterRule = 1 And
                           (t.WorkDate.DayOfWeek = DayOfWeek.Saturday Or
                           t.WorkDate.DayOfWeek = DayOfWeek.Sunday)
                         Select t.WorkHours).Sum
          Return weekday + weekend
        Else
          Return 0
        End If
      End Get
    End Property

    Public ReadOnly Property OnCall_MinimumHours(Week As Integer) As Double
      Get
        Return (From t In Week_TL(Week) Select t.OnCallMinimumHours).Sum
      End Get
    End Property

    Public ReadOnly Property OnCall_HoursWorked(Week As Integer) As Double
      Get
        Return (From t In Week_TL(Week) Select t.OnCallWorkHours).Sum
      End Get
    End Property

    Public ReadOnly Property OnCall_TotalHours(Week As Integer) As Double
      Get
        Return (From t In Week_TL(Week) Select t.OnCallTotalHours).Sum
      End Get
    End Property

    Public ReadOnly Property BreakCredit(Week As Integer) As Double
      Get
        Return (From t In Week_TL(Week) Select t.BreakCreditHours).Sum
      End Get
    End Property

    Public ReadOnly Property Holiday(Week As Integer) As Double
      Get
        Return (From t In Week_TL(Week) Select t.HolidayHours).Sum
      End Get
    End Property

    Public ReadOnly Property Admin_Total(Week As Integer) As Double
      Get
        Return Admin(Week) + Admin_Other(Week) + Admin_Bereavement(Week) +
          Admin_JuryDuty(Week) + Admin_MilitaryLeave(Week) +
          Admin_WorkersComp(Week)
      End Get
    End Property

    Public ReadOnly Property Admin(Week As Integer) As Double
      Get
        Return (From t In Week_TL(Week) Select t.AdminHours).Sum
      End Get
    End Property

    Public ReadOnly Property Admin_Bereavement(Week As Integer) As Double
      Get
        Return (From t In Week_TL(Week) Select t.AdminBereavement).Sum
      End Get
    End Property

    Public ReadOnly Property Admin_Disaster(Week As Integer) As Double
      Get
        Return (From t In Week_TL(Week) Select t.AdminDisaster).Sum
      End Get
    End Property

    Public ReadOnly Property Admin_JuryDuty(Week As Integer) As Double
      Get
        Return (From t In Week_TL(Week) Select t.AdminJuryDuty).Sum
      End Get
    End Property

    Public ReadOnly Property Admin_MilitaryLeave(Week As Integer) As Double
      Get
        Return (From t In Week_TL(Week) Select t.AdminMilitaryLeave).Sum
      End Get
    End Property

    Public ReadOnly Property Admin_WorkersComp(Week As Integer) As Double
      Get
        Return (From t In Week_TL(Week) Select t.AdminWorkersComp).Sum
      End Get
    End Property

    Public ReadOnly Property Admin_Other(Week As Integer) As Double
      Get
        Return (From t In Week_TL(Week) Select t.AdminOther).Sum
      End Get
    End Property

    Public ReadOnly Property Term_Hours(Week As Integer) As Double
      Get
        Return (From t In Week_TL(Week) Select t.TermHours).Sum
      End Get
    End Property

    Public ReadOnly Property LWOP_All(Week As Integer) As Double
      Get
        Return (From t In Week_TL(Week) Select t.LWOPHours + t.LWOPSuspensionHours + t.ScheduledLWOPHours).Sum
      End Get
    End Property

    Public ReadOnly Property LWOP(Week As Integer) As Double
      Get
        Return (From t In Week_TL(Week) Select t.LWOPHours).Sum
      End Get
    End Property

    Public ReadOnly Property LWOP_Suspension(Week As Integer) As Double
      Get
        Return (From t In Week_TL(Week) Select t.LWOPSuspensionHours).Sum
      End Get
    End Property

    Public ReadOnly Property Scheduled_LWOP(Week As Integer) As Double
      Get
        Return (From t In Week_TL(Week) Select t.ScheduledLWOPHours).Sum
      End Get
    End Property

    Public ReadOnly Property Vacation(Week As Integer) As Double
      Get
        Return (From t In Week_TL(Week) Select t.VacationHours).Sum

      End Get
    End Property

    Public ReadOnly Property Sick_All(Week As Integer) As Double
      Get
        Return (From t In Week_TL(Week) Select t.SickHours + t.SickFamilyLeave).Sum
      End Get
    End Property

    Public ReadOnly Property Sick(Week As Integer) As Double
      Get
        Return (From t In Week_TL(Week) Select t.SickHours).Sum
      End Get
    End Property

    Public ReadOnly Property Sick_Family_Leave(Week As Integer) As Double
      Get
        Return (From t In Week_TL(Week) Select t.SickFamilyLeave).Sum
      End Get
    End Property

    Public ReadOnly Property SickLeavePool(Week As Integer) As Double
      Get
        Return (From t In Week_TL(Week) Select t.SickLeavePoolHours).Sum
      End Get
    End Property

    Public ReadOnly Property Comp_Time_Earned(Week As Integer) As Double
      Get
        Dim CTE As Double = (From t In Week_TL(Week) Select t.CompTimeEarned).Sum

        If CTE = 0 Then
          Select Case Week
            Case 1
              CTE = CompTimeEarnedWeek1
            Case 2
              CTE = CompTimeEarnedWeek2
            Case Else
              CTE = CompTimeEarnedWeek1 + CompTimeEarnedWeek2
          End Select
        End If
        Return CTE
      End Get
    End Property

    Public ReadOnly Property Comp_Time_Used(Week As Integer) As Double
      Get
        Return (From t In Week_TL(Week) Select t.CompTimeUsed).Sum
      End Get
    End Property

    Public ReadOnly Property Double_Time(Week As Integer) As Double
      Get
        Return (From t In Week_TL(Week) Select t.DoubleTimeHours).Sum
      End Get
    End Property

    Public ReadOnly Property Non_Working_Paid_Time(Week As Integer) As Double
      Get
        Return Admin(Week) + Vacation(Week) + Sick(Week) + SickLeavePool(Week) + LWOP_All(Week) + Comp_Time_Used(Week) + Admin_Disaster(Week)
      End Get
    End Property

    Public ReadOnly Property Overtime(Week As Integer) As Double
      Get
        If IsExempt Then
          Return 0
        Else
          Dim Diff As Double = 0
          'Dim newDiff As Double = 0
          'newDiff = Regular(Week) - Double_Time(Week) - Comp_Time_Earned(Week) - 40 - Non_Working_Paid_Time(Week)
          Select Case Week
            Case 1, 2
              Diff = Regular(Week) + DisasterRegular(Week) -
                Double_Time(Week) - Comp_Time_Earned(Week) -
                DisasterDoubleTime(Week) - 40
              Return Math.Max(Diff, 0)
            Case Else
              Return Overtime(1) + Overtime(2)
          End Select
        End If
      End Get
    End Property

    Public ReadOnly Property Regular_Overtime(Week As Integer) As Double
      Get
        If IsExempt Then
          Return 0
        Else
          Dim diff As Double = 0
          Select Case Week
            Case 1, 2
              diff = Total_Hours(Week) - 40 - Overtime(Week) -
                Comp_Time_Earned(Week) - Double_Time(Week) -
                (HolidayHoursWorked(Week) - HolidayHoursWorkedDifference(Week)) -
                DisasterStraightTime(Week) - DisasterDoubleTime(Week) -
                DisasterOverTime(Week)


              'If diff > 0 Then Return diff Else Return 0
              Return Math.Max(diff, 0)
            Case Else
              Return Regular_Overtime(1) + Regular_Overtime(2)
          End Select
        End If
      End Get
    End Property

    Public ReadOnly Property TakeHomeVehicle(Week As Integer) As Double
      Get
        Return (From t In Week_TL(Week) Select t.Vehicle).Sum
        'Select Case Week
        '    Case 1
        '        Return (From t In TL Where t.WorkDate >= PayPeriodStart And t.WorkDate < PayPeriodStart.AddDays(7)
        '                Select t.Vehicle).Sum
        '    Case 2
        '        Return (From t In TL Where t.WorkDate >= PayPeriodStart.AddDays(7) And t.WorkDate < PayPeriodStart.AddDays(14)
        '                Select t.Vehicle).Sum
        '    Case Else
        '        Return (From t In TL Select t.Vehicle).Sum
        'End Select
      End Get
    End Property

    ' Calculated Fields:
    Private ReadOnly Property Calculated_Work(Week As Integer) As Double
      Get
        Return (From t In Week_TL(Week) Select t.WorkHours + t.BreakCreditHours + t.HolidayHours).Sum
      End Get
    End Property

    'Public ReadOnly Property Calculated_Unscheduled_Overtime(Week As Integer) As Double
    '    Get
    '        If Not IsExempt Then
    '            Dim val As Double = Calculated_Work(Week) - Double_Time(Week) - Comp_Time_Earned(Week)
    '            If val > 0 Then
    '                Return val
    '            Else
    '                Return 0
    '            End If
    '        Else
    '            Return 0
    '        End If
    '    End Get
    'End Property



    'Public ReadOnly Property Calculated_Regular_Overtime(Week As Integer) As Double
    '    Get
    '        Dim val As Double = Total_Hours(Week) - 40 - Overtime(Week) - Comp_Time_Earned(Week) - Double_Time(Week)
    '        If val > 0 Then Return val Else Return 0
    '    End Get
    'End Property

    Public Sub Catch_Exceptions()
      ' Holiday time <= 8
      If Now > CType("9/22/2015", Date) Then
        If TL.Count > 1 AndAlso TL.First.DataSource = TimecardTimeData.TimeCardDataSource.Timecard Then
          WarningList.Add("Employee has time in the timecard system.")
        End If
      End If
      If EmployeeData.HireDate > PayPeriodStart Then
        WarningList.Add("Employee just started this pay period.")
      End If
      For a As Integer = 1 To 2
        If LWOP(a) > 40 Then
          ErrorList.Add("Too many Leave Without Pay hours used in Week " & a.ToString & ".")
        End If
      Next

      For Each t In TL
        'If t.AdminHours > 8 Then
        '    WarningList.Add("8 or more Admin leave hours entered on " & t.WorkDate.ToShortDateString & ".")
        'End If
        If t.AdminHours >= 8 Then
          WarningList.Add("8+ hours of Admin leave on " & t.WorkDate.ToShortDateString & ".")
        End If
        If t.WorkTimes.Trim.Length > 0 AndAlso t.WorkTimes.Trim.Split("-").Length Mod 2 = 1 Then
          ErrorList.Add("Missing a start or end time on " & t.WorkDate.ToShortDateString & ".")
        End If
        ' Catch time if regular hours are less than .25 hours
        If t.WorkHours Mod 0.25 > 0 Then
          ErrorList.Add("Regular working hours must be in 15 minute increments.")
        End If
        If t.WorkHours > 0 And t.HolidayHours > 0 Then
          WarningList.Add("Employee has work hours on a Holiday.")
        End If
        If t.SickLeavePoolHours > 0 Then
          WarningList.Add("Employee has hours in Sick Leave Pool.")
        End If

        If t.WorkHours < t.DoubleTimeHours Then
          ErrorList.Add("Employee has double time hours but not enough work hours listed on " & t.WorkDate.ToShortDateString & ".")
        End If

        If t.WorkDate.Subtract(EmployeeData.HireDate).TotalDays < 91 Then
          If t.VacationHours > 0 Or t.SickFamilyLeave > 0 Or t.SickHours > 0 Or
              t.SickLeavePoolHours > 0 Then
            ErrorList.Add("Employee has Sick or Vacation hours and is still in first 90 days.")
          End If
        End If

      Next

      If Term_Hours(0) > 0 Then
        WarningList.Add("Employee has term hours entered.")
      End If
      ' Catch if using more sick / vacation / comp time than accrued
      If Sick(0) > EmployeeData.Banked_Sick_Hours Then
        ErrorList.Add("Using too many Sick hours, only " & EmployeeData.Banked_Sick_Hours & " banked.")
      End If
      If Vacation(0) > EmployeeData.Banked_Vacation_Hours Then
        ErrorList.Add("Using too many Vacation hours, only " & EmployeeData.Banked_Vacation_Hours & " banked.")
      End If
      If Comp_Time_Used(1) > EmployeeData.Banked_Comp_Hours Then
        ErrorList.Add("Using too many Comp Time Used hours, only " & EmployeeData.Banked_Comp_Hours & " banked.")
      End If
      If Comp_Time_Used(2) > EmployeeData.Banked_Comp_Hours + (CompTimeEarnedWeek1 * 1.5) Then
        ErrorList.Add("Using too many Comp Time Used hours, only " & EmployeeData.Banked_Comp_Hours + (CompTimeEarnedWeek1 * 1.5) & " banked.")
      End If
      If TL.Count = 0 AndAlso Now > PayPeriodStart.AddDays(11) Then
        ' we only want to show this message near the end of the pay period, or after the pay period is over.
        ErrorList.Add("Employee has no time entered this pay period.")
      End If

      If (From t In TL Select t.AdminBereavement + t.AdminHours + t.AdminDisaster +
                         t.AdminJuryDuty + t.AdminMilitaryLeave +
                         t.AdminOther + t.AdminWorkersComp).Sum > 24 Then
        WarningList.Add("More than 24 hours of Admin Leave used.")
      End If

      If IsExempt Then
        Catch_Exempt_Exceptions()
      Else
        Catch_Non_Exempt_Exceptions()
      End If
    End Sub

    Private Sub Catch_Non_Exempt_Exceptions()
      ' No admin leave
      ' Comp Time Calculation errors
      ' Warning if part time and > 24 hours
      If Not EmployeeData.isFulltime And (Total_Hours(1) > 24 Or Total_Hours(2) > 24) Then
        WarningList.Add("Employee is part time and has more than 24 hours worked for the week, please notify HR.")
      End If

      ' On Call Checks
      Dim OnCallHours() As String = {"11:59:59 PM", "12:00 AM"}
      For Each t In TL
        If OnCallHours.Any(Function(s) t.OnCallWorkTimes.Contains(s)) Then
          WarningList.Add("Employee has On Call hours at Midnight. Please check that call min was calculated correctly.")
          Exit For

        End If
      Next

      If EmployeeData.isFulltime And Term_Hours(0) = 0 Then
        If Now > PayPeriodStart.AddDays(6) Then
          If EmployeeData.HireDate >= PayPeriodStart AndAlso
            EmployeeData.HireDate <= PayPeriodStart.AddDays(13) Then
            ' They were hired in this week, so we'll relax the hour requirements
          Else
            If Not IsExempt AndAlso Total_Hours(1) < 40 Then
              ErrorList.Add("Employee does not have 40 hours recorded in Week 1.")
            End If
          End If
        End If
        If Now > PayPeriodStart.AddDays(11) Then
          If EmployeeData.HireDate >= PayPeriodStart.AddDays(7) AndAlso
            EmployeeData.HireDate <= PayPeriodStart.AddDays(13) Then
            ' They were hired this week, so we'll relax the hour requirements
          Else
            If Not IsExempt AndAlso Total_Hours(2) < 40 Then
              ErrorList.Add("Employee does not have 40 hours recorded in Week 2.")
            End If
          End If
        End If
      End If

    End Sub

    Private Sub Catch_Exempt_Exceptions()
      Dim shiftTen() As String = {"1055"}
      Dim shiftMax As Double = 8
      If shiftTen.Contains(EmployeeData.Classify) Then shiftMax = 10
      For Each t In TL

        If t.WorkHours < 7.5 Then
          If t.Total_Non_Working_Hours > 0 Then
            If t.WorkHours + t.Total_Non_Working_Hours < 8 Then
              ErrorList.Add("Not enough hours entered on " & t.WorkDate.ToShortDateString & ". You must have 8 total hours if you are using any leave hours.")
            End If
          End If
        End If
        ' More than 8 hours recorded when using Sick / Vacation / Admin
        If t.TotalHours > shiftMax And (t.Total_Non_Working_Hours > 0) Then
          ErrorList.Add("Too many hours recorded on " & t.WorkDate.ToShortDateString & ". You only need to record 8 hours total when you are using Sick / Vacation / Admin leave.")
        End If
        ' Hours in OT / Double OT / Comp Time / 
        If t.DoubleTimeHours > 0 Or t.CompTimeEarned > 0 Or t.CompTimeUsed > 0 Then
          ErrorList.Add("Exempt employees do not get double time or comp time.  Please correct the hours on " & t.WorkDate.ToShortDateString & " to proceed.")
        End If
        '' Atleast 1 hour of regular with admin leave
        'If t.AdminHours >= 8 AndAlso t.WorkHours < 1 Then
        '    ErrorList.Add("Too many admin hours used on " & t.WorkDate.ToShortDateString & ". You must record atleast 1 hour of regular time in order to use Admin Leave.")
        'End If
      Next
      ' Must have atleast 7.5 hours per day, 5 times in a given week, 
      ' or 10 hours a day, 4 days in a given pay week.
      If Term_Hours(0) = 0 Then
        If EmployeeData.HoursNeededForOvertime = 80 And EmployeeData.HireDate < PayPeriodStart Then
          If shiftTen.Contains(EmployeeData.Classify) Then
            If Now > PayPeriodStart.AddDays(6) Then
              If ((From t In Week_TL(1) Select t Where t.TotalHours >= 9.5).Count < 4) AndAlso
                ((From t In Week_TL(1) Select t Where t.TotalHours >= 7.5).Count < 5) Then
                WarningList.Add("Not enough workdays of at least 9.5 hours recorded in Week 1.")
              End If
            End If
            If Now > PayPeriodStart.AddDays(12) Then
              If ((From t In Week_TL(2) Select t Where t.TotalHours >= 9.5).Count < 4) AndAlso
                  ((From t In Week_TL(2) Select t Where t.TotalHours >= 7.5).Count < 5) Then
                WarningList.Add("Not enough workdays of at least 9.5 hours recorded in Week 2.")
              End If
            End If
          Else
            If (From t In Week_TL(1) Select t Where t.TotalHours >= 7.5).Count < 5 AndAlso Now > PayPeriodStart.AddDays(6) Then
              WarningList.Add("Not enough workdays of at least 7.5 hours recorded in Week 1.")
            End If
            If (From t In Week_TL(2) Select t Where t.TotalHours >= 7.5).Count < 5 AndAlso Now > PayPeriodStart.AddDays(12) Then
              WarningList.Add("Not enough workdays of at least 7.5 hours recorded in Week 2.")
            End If
          End If

          ' Must have atleast 37.5 hours per week
          If Total_Hours(1) < 37.5 AndAlso Now > PayPeriodStart.AddDays(6) Then
            ErrorList.Add("Not enough hours recorded in Week 1.  You must have at least 37.5 hours recorded per week.")
          End If
          If Total_Hours(2) < 37.5 AndAlso Now > PayPeriodStart.AddDays(12) Then
            ErrorList.Add("Not enough hours recorded in Week 2.  You must have at least 37.5 hours recorded per week.")
          End If
        End If
      End If
    End Sub

  End Class
End Namespace