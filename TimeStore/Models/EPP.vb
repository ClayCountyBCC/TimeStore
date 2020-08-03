Namespace Models
  Public Class EPP ' Employee Pay Period.
    Private _specialties As New List(Of String)
    Property EmployeeData As FinanceData
    Property Timelist As List(Of TelestaffTimeData)
    Property RawTimeList As List(Of TelestaffTimeData)
    Private _regular, _scheduled_overtime, _lwop, _vacation, _sick As New GroupedHours
    Private _scheduled_regular_overtime, _holiday_paid As New GroupedHours
    Private _unscheduled_overtime, _unscheduled_regular_overtime As New GroupedHours
    Private _unscheduled_double_overtime, _holiday_time_banked As New GroupedHours
    Private _holiday_time_used, _non_working, _term_hours As New GroupedHours
    Private _comp_time_banked, _comp_time_used, _non_paid, _union_time_pool As New GroupedHours
    Private _disaster_regular As New GroupedHours
    Private _disaster_straighttime As New GroupedHours ' regular overtime
    Private _disaster_overtime As New GroupedHours
    Private _disaster_doubletime As New GroupedHours
    Private _admin_leave_disaster As New GroupedHours
    Public DisasterPayRules As List(Of DisasterEventRules) = New List(Of DisasterEventRules)()
    Public Property Banked_Holiday_Hours As Double = 0
    Public Property TelestaffHoursNeededForOvertime As Double = Double.MinValue
    Public Property TelestaffProfileType As TelestaffProfileType
    Public Property TelestaffProfileID As Integer
    Public Property TelestaffProfileDescription As String
    Public Property PayPeriodStart As Date
    Public Property ErrorList As New List(Of String)
    Public Property WarningList As New List(Of String)

    Public Shared Function PopulateDisasterWorkDates(PayPeriodStart As Date, DisasterPayRules As List(Of DisasterEventRules)) As List(Of Date)
      Dim workdates As List(Of Date) = New List(Of Date)()
      If DisasterPayRules.Count = 0 Then Return workdates

      For Each dpr In DisasterPayRules
        If dpr.pay_rule = 1 Or dpr.pay_rule = 2 Then
          Dim d As Date = dpr.StartDate.Date
          Do While d < dpr.EndDate.Date
            If Not workdates.Contains(d) And d >= PayPeriodStart Then workdates.Add(d.Date)
            d = d.AddDays(1)
          Loop
        End If
      Next
      Return workdates
    End Function


    Private Sub Init_GroupedHours_Paycodes()
      _regular.PayCode = "002"
      _scheduled_overtime.PayCode = "131"
      _lwop.PayCode = "090"
      _term_hours.PayCode = "095"
      _scheduled_regular_overtime.PayCode = "130"
      _holiday_paid.PayCode = "134"
      _unscheduled_regular_overtime.PayCode = "230"
      _unscheduled_overtime.PayCode = "231"
      _unscheduled_double_overtime.PayCode = "232"
      _holiday_time_banked.PayCode = "122"
      _holiday_time_used.PayCode = "123"
      '_holiday_bank_paid.paycode = "124"
      _comp_time_banked.PayCode = "120"
      _comp_time_used.PayCode = "121"
      _union_time_pool.PayCode = "007"
      _admin_leave_disaster.PayCode = "300"
      _disaster_doubletime.PayCode = "303"
      _disaster_overtime.PayCode = "302"
      _disaster_straighttime.PayCode = "301"
      _disaster_regular.PayCode = "299"

      Select Case TelestaffProfileType
        Case TelestaffProfileType.Field
          _vacation.PayCode = "101"
          _sick.PayCode = "111"

        Case TelestaffProfileType.Dispatch, TelestaffProfileType.Office
          _vacation.PayCode = "100"
          _sick.PayCode = "110"

      End Select
    End Sub

    Private Function CloneTelestaffTimeData(ByVal T As List(Of TelestaffTimeData)) As List(Of TelestaffTimeData)
      Dim l As New List(Of TelestaffTimeData)
      For Each item In T
        l.Add(item.Clone)
      Next
      Return l
    End Function

    Sub New(T As List(Of TelestaffTimeData), F As FinanceData, PPS As Date)
      ', ProfileID As Integer, ProfileDesc As String)
      PayPeriodStart = PPS
      RawTimeList = CloneTelestaffTimeData(T)
      Timelist = T
      EmployeeData = F
      Banked_Holiday_Hours = F.Banked_Holiday_Hours
      TelestaffHoursNeededForOvertime = Get_Hours_Needed_For_Overtime()
      TelestaffProfileType = Get_Profile_Type()
      Init_GroupedHours_Paycodes()
      DisasterPayRules = DisasterEventRules.Get_Cached_Disaster_Rules(PPS.AddDays(13))
      ' Holiday Hours Special Handling, these hours will need to be removed from the green sheet printout.
      Handle_Holiday_Modifier(True)

      'TelestaffProfileDescription = ProfileDesc
      'TelestaffProfileID = ProfileID
      'Dim j As Double = Total_Hours

      Parse_Hours()
      Balance_Hours()
      Check_Exceptions()
      Handle_Holiday_Modifier(False)
    End Sub

    Private Sub Handle_Holiday_Modifier(Add As Boolean)
      ' 9/14/2016 update.  This code is flawed in that it is enabling
      ' people to use the holiday at any point in the pay period, not just
      ' on or after the holiday.
      If TelestaffProfileType = Models.TelestaffProfileType.Field Or TelestaffProfileType = Models.TelestaffProfileType.Dispatch Then
        ' The purpose of this bit of code is to handle holidays as they occur during the pay period.
        ' The Field staff can use the holiday as soon as they get it, so even though they might
        ' have 0 hours banked, they accrue one during the pay period and use it later in the pay period.
        Dim modifier As Integer = Get_Holiday_Hour_Modifier()

        Dim HolidayHours As Integer = Holiday_Hours_In_PayPeriod(modifier)
        If Add Then
          Banked_Holiday_Hours += HolidayHours
        Else
          Banked_Holiday_Hours -= HolidayHours
        End If

      End If
    End Sub

    Private Function Get_Holiday_Hour_Modifier() As Integer
      Select Case TelestaffProfileType
        Case TelestaffProfileType.Field
          Return 24
        Case TelestaffProfileType.Dispatch
          Return 12
        Case TelestaffProfileType.Office
          Return 0
        Case Else
          Return 0
      End Select
    End Function

    Private Function Holiday_Hours_In_PayPeriod(modifier As Integer) As Double
      Return Get_Holidays_By_Payperiod(PayPeriodStart, False).Count * modifier
    End Function

    'Private Sub Parse_Hours_New()
    '  ' let's handle the non-working, non-overtime eligible hours first
    '  Parse_Non_Working_Hours()
    '  If TelestaffProfileType = TelestaffProfileType.Office Then
    '    Parse_Working_Hours_Office()
    '  Else
    '    Parse_Working_Hours_Non_Office()
    '  End If


    'End Sub

    'Private Sub Parse_Working_Hours_Office()

    'End Sub

    'Private Sub Parse_Working_Hours_Non_Office()
    '  Dim Worked = (From t In Timelist
    '                Where t.CountsTowardsOvertime
    '                Order By t.WorkDate Ascending, t.StartTime Ascending
    '                Select t).ToList
    '  For Each t In Worked
    '    Select Case t.WorkCode
    '      Case "299", "300", "301", "302"
    '        ' make sure the hours stay in a disaster paycode
    '        Handle_Hours_Worked_Non_Office(t, True)
    '      Case "303"
    '        _disaster_doubletime.Add(t)
    '      Case "232"
    '        _unscheduled_double_overtime.Add(t)
    '      Case Else
    '        ' make sure the hours stay in a non-disaster paycode
    '        Handle_Hours_Worked_Non_Office(t)
    '    End Select
    '  Next
    'End Sub

    'Private Sub Handle_Hours_Worked_Non_Office(ByRef t As TelestaffTimeData, Optional IsDisaster As Boolean = False)
    '  ' first let's figure out where these hours go.
    '  If Worked_Hours + t.WorkHours > TelestaffHoursNeededForOvertime Then
    '    'we're in OT territory now.
    '    If TelestaffHoursNeededForOvertime - Worked_Hours > 0 Then

    '    End If
    '    Dim diff As Double = TelestaffHoursNeededForOvertime - Worked_Hours - t.WorkHours
    '    If diff Then

    '    End If
    '  Else
    '    Get_Grouped_Hours(t.WorkCode).Add(t)
    '  End If
    '  If Scheduled_Hours > TelestaffHoursNeededForOvertime Then

    '  End If
    'End Sub

    'Private Function Get_Grouped_Hours(paycode As String) As GroupedHours
    '  Select Case paycode
    '    Case "002"
    '      Return Regular
    '    Case "007"
    '      Return Union_Time_Pool
    '    Case "090"
    '      Return Leave_Without_Pay
    '    Case "095"
    '      Return Term_Hours
    '    Case "100", "110"
    '      Return Sick
    '    Case "101", "111"
    '      Return Vacation
    '    Case "120"
    '      Return Comp_Time_Banked
    '    Case "121"
    '      Return Comp_Time_Used
    '    Case "122"
    '      Return Holiday_Time_Banked
    '    Case "123"
    '      Return Holiday_Time_Used
    '    Case "130"
    '      Return Scheduled_Regular_Overtime
    '    Case "131"
    '      Return Scheduled_Overtime
    '    Case "134"
    '      Return Holiday_Paid
    '    Case "230"
    '      Return Unscheduled_Regular_Overtime
    '    Case "231"
    '      Return Unscheduled_Overtime
    '    Case "232"
    '      Return Unscheduled_Double_Overtime

    '    Case "299"
    '      Return Disaster_Regular
    '    Case "300"
    '      Return Admin_Leave_Disaster
    '    Case "301"
    '      Return Disaster_StraightTime
    '    Case "302"
    '      Return Disaster_Overtime
    '    Case "303"
    '      Return Disaster_Doubletime
    '    Case Else
    '      Log("Unhandled Payroll Code", paycode, "", "")
    '      Return Non_Paid
    '  End Select
    'End Function

    'Private Sub Parse_Non_Working_Hours()
    '  Dim NoOT = (From t In Timelist
    '              Where Not t.CountsTowardsOvertime
    '              Select t).ToList
    '  For Each t In NoOT
    '    Select Case t.WorkCode
    '      Case "002"
    '        _regular.Add(t)
    '        _non_working.Add(t)
    '      Case "007"
    '        _union_time_pool.Add(t)
    '        _non_working.Add(t)
    '      Case "300"
    '        _admin_leave_disaster.Add(t)
    '        _non_working.Add(t)
    '      Case "123", "430" ' Holiday time bank Hours Requested to be used
    '        _holiday_time_used.Add(t)
    '      Case "095"
    '        _term_hours.Add(t)
    '      Case "090" ' Leave without pay
    '        _lwop.Add(t)
    '      Case "101", "100" ' vacation
    '        _vacation.Add(t)
    '      Case "134" ' paid holiday
    '        _holiday_paid.Add(t)
    '      Case "110", "111" ' sick 
    '        _sick.Add(t)
    '      Case "230" ' Unscheduled Regular OT
    '        _unscheduled_regular_overtime.Add(t)
    '      Case Else
    '        _non_working.Add(t)
    '    End Select

    '  Next
    'End Sub

    Private Sub Parse_Hours()
      ' In this sub we're going to add the hours to the particular group they belong to based on their work code
      ' and work abreviation
      For Each T In Timelist

        If Not Handle_Disaster_Hours_Office(T) Then
          Select Case T.WorkCode
            Case "002" ' straight time aka regular
              If Not T.CountsTowardsOvertime Then
                _regular.Add(T)
                _non_working.Add(T.Clone)
              Else
                _regular.Add(T)
              End If
            Case "230" ' Unscheduled Regular OT
              _unscheduled_regular_overtime.Add(T)
            Case "131" ' Scheduled OT
              _scheduled_overtime.Add(T)
            Case "231" ' Unscheduled OT
              ' If an office staffer works on a sunday or a holiday, they get double overtime.
              _unscheduled_overtime.Add(T)
                    'If T.ProfileType = Models.TelestaffProfileType.Office And T.WorkDate.DayOfWeek = DayOfWeek.Sunday Then
                    '    _unscheduled_double_overtime.Add(T)
                    'Else

                    'End If

            Case "130" ' Scheduled Regular overtime
              _scheduled_regular_overtime.Add(T)
                    ' This one will probably not be used by Telestaff, it doesn't know what this is.
            Case "095"
              _term_hours.Add(T)
            Case "090" ' Leave without pay
              _lwop.Add(T)
            Case "101", "100" ' vacation
              _vacation.Add(T)
            Case "134" ' paid holiday
              _holiday_paid.Add(T)
            Case "110", "111" ' sick 
              _sick.Add(T)
            Case "232" ' Unscheduled Double OT
              _unscheduled_double_overtime.Add(T)
            Case "122" ' Holiday Time Bank
              _holiday_time_banked.Add(T)
            Case "123", "430" ' Holiday time bank Hours Requested to be used
              _holiday_time_used.Add(T)
            Case "120" ' Comp time accrued
              _comp_time_banked.Add(T)
            Case "121" ' Banked comp time used
              _comp_time_used.Add(T)
            Case "007"
              ' 007 is the Union Time Pool. It's not part of the green sheet numbers, it will need to be added
              ' in addition to those, in a separate area.  
              ' As for now, I'm going to stick it in the non_paid area just to make sure it's not lost somewhere.
              ' An exception will be added so that it will show up to the Public Safety payroll staff to do the manual
              ' addition to the greensheet.
              _union_time_pool.Add(T)
              _non_working.Add(T.Clone)
            Case "299"
              _disaster_regular.Add(T)
            Case "300"
              _admin_leave_disaster.Add(T)
              _non_working.Add(T.Clone)
            Case "301"
              _disaster_straighttime.Add(T)
            Case "302"
              _disaster_overtime.Add(T)
            Case "303"
              _disaster_doubletime.Add(T)

            Case Else
              If T.IsPaidTime Then
                Log("Unknown Payroll Code", T.WorkCode, T.EmployeeId.ToString, T.WorkTypeAbrv)
              Else
                _non_paid.Add(T)
              End If
          End Select
        End If
      Next
    End Sub

    Private Sub Balance_Hours()
      ' This function will compare the employee's hours with their banked hours along with
      ' their minimum needed for overtime and move the hours to different types as needed.

      ' First we're going to move the regular pay to overtime and the overtime to regular pay as needed.
      If TelestaffProfileType = TelestaffProfileType.Office Then
        Balance_Disaster_Hours()
      End If
      If Not IsExempt Then
        If Not IsOvertime_Calculated_Weekly Then
          Balance_Overtime_By_Payperiod_Hours()
        Else
          ' These are the weekly employees
          Balance_Overtime_By_Weekly_Hours()
        End If
      End If
      UpdateDisasterHours()
      Balance_Timebanks_Versus_Hours_Requested()
    End Sub

    Private Sub Balance_Disaster_Hours()
      If TelestaffProfileType = TelestaffProfileType.Office Then

        If IsExempt Then
          'Balance_Exempt_Disaster_By_Period_By_Day(Disaster_Regular)
          Balance_Exempt_Disaster_By_Period_By_Week(Disaster_Regular.Week1)
          Balance_Exempt_Disaster_By_Period_By_Week(Disaster_Regular.Week2)
          'Balance_Exempt_Disaster_By_Week(Regular.Week1, Disaster_Regular.Week1)
          'Balance_Exempt_Disaster_By_Week(Regular.Week2, Disaster_Regular.Week2)
        Else
          Balance_NonExempt_Disaster_By_Period_By_Week(Disaster_Regular.Week1)
          Balance_NonExempt_Disaster_By_Period_By_Week(Disaster_Regular.Week2)
        End If

      End If
    End Sub

    Private Sub UpdateDisasterHours()
      'Select Case ttd.WorkCode
      '  Case "299"
      '    ttd.WorkCode = "002"
      '  Case "301"
      '    ttd.WorkCode = "230"
      '  Case "302"
      '    ttd.WorkCode = "231"
      '  Case "303"
      '    ttd.WorkCode = "232"
      'End Select

      ' 002 - 299
      Dim regular_week1 = (From t In _regular.Week1
                           Where t.Finplus_Project_Code.Length > 0 And
                             t.DisasterRule = 0
                           Select t).ToList
      Dim regular_week2 = (From t In _regular.Week2
                           Where t.Finplus_Project_Code.Length > 0 And
                             t.DisasterRule = 0
                           Select t).ToList

      For Each t In regular_week1
        _disaster_regular.Add(t)
        _regular.Week1.Remove(t)
      Next

      For Each t In regular_week2
        _disaster_regular.Add(t)
        _regular.Week2.Remove(t)
      Next
      ' 230 to 301
      Dim unscheduled_ot1_0_week1 = (From t In _unscheduled_regular_overtime.Week1
                                     Where t.Finplus_Project_Code.Length > 0 And
                                       t.DisasterRule = 0
                                     Select t).ToList()

      Dim unscheduled_ot1_0_week2 = (From t In _unscheduled_regular_overtime.Week2
                                     Where t.Finplus_Project_Code.Length > 0 And
                                       t.DisasterRule = 0
                                     Select t).ToList()

      For Each t In unscheduled_ot1_0_week1
        _disaster_straighttime.Add(t)
        _unscheduled_regular_overtime.Week1.Remove(t)
      Next
      For Each t In unscheduled_ot1_0_week2
        _disaster_straighttime.Add(t)
        _unscheduled_regular_overtime.Week2.Remove(t)
      Next

      ' 231 to 302
      Dim unscheduled_ot1_5_week1 = (From t In _unscheduled_overtime.Week1
                                     Where t.Finplus_Project_Code.Length > 0 And
                                       t.DisasterRule = 0
                                     Select t).ToList()

      Dim unscheduled_ot1_5_week2 = (From t In _unscheduled_overtime.Week2
                                     Where t.Finplus_Project_Code.Length > 0 And
                                       t.DisasterRule = 0
                                     Select t).ToList()

      For Each t In unscheduled_ot1_5_week1
        _disaster_overtime.Add(t)
        _unscheduled_overtime.Week1.Remove(t)
      Next
      For Each t In unscheduled_ot1_5_week2
        _disaster_overtime.Add(t)
        _unscheduled_overtime.Week2.Remove(t)
      Next

      ' 232 to 303

      Dim unscheduled_ot2_week1 = (From t In _unscheduled_double_overtime.Week1
                                   Where t.Finplus_Project_Code.Length > 0 And
                                       t.DisasterRule = 0
                                   Select t).ToList()

      Dim unscheduled_ot2_week2 = (From t In _unscheduled_double_overtime.Week2
                                   Where t.Finplus_Project_Code.Length > 0 And
                                       t.DisasterRule = 0
                                   Select t).ToList()

      For Each t In unscheduled_ot2_week1
        _disaster_doubletime.Add(t)
        _unscheduled_double_overtime.Week1.Remove(t)
      Next
      For Each t In unscheduled_ot2_week2
        _disaster_doubletime.Add(t)
        _unscheduled_double_overtime.Week2.Remove(t)
      Next

      ' 131 to 302, just because it might happen
      Dim scheduled_ot1_5_week1 = (From t In _scheduled_overtime.Week1
                                   Where t.Finplus_Project_Code.Length > 0 And
                                       t.DisasterRule = 0
                                   Select t).ToList()

      Dim scheduled_ot1_5_week2 = (From t In _scheduled_overtime.Week2
                                   Where t.Finplus_Project_Code.Length > 0 And
                                       t.DisasterRule = 0
                                   Select t).ToList()

      For Each t In unscheduled_ot1_5_week1
        _disaster_overtime.Add(t)
        _scheduled_overtime.Week1.Remove(t)
      Next
      For Each t In unscheduled_ot1_5_week2
        _disaster_overtime.Add(t)
        _scheduled_overtime.Week2.Remove(t)
      Next

    End Sub

    Private Sub Balance_Exempt_Disaster_By_Period_By_Week(DisasterRegular As List(Of TelestaffTimeData))

      'Dim NormallyScheduled As Boolean = True
      'For Each t In DisasterRegular
      '  NormallyScheduled = Not (t.WorkDate.DayOfWeek = DayOfWeek.Saturday Or t.WorkDate.DayOfWeek = DayOfWeek.Sunday)
      '  If t.WorkDate = "9/2/2019" Then NormallyScheduled = False

      '  For Each dpr In DisasterPayRules

      '  Next
      'Next


      'Everything on these dates should be moved to disaster regular overtime.
      'Dim dates = (From t In Timelist
      '             Where t.DisasterRule = 1 And
      '               (t.WorkDate.DayOfWeek = DayOfWeek.Saturday Or
      '               t.WorkDate.DayOfWeek = DayOfWeek.Sunday)
      '             Select t.WorkDate).Distinct.ToList()
      Dim dates = PopulateDisasterWorkDates(PayPeriodStart, DisasterPayRules)

      If dates.Count = 0 Then Exit Sub
      For Each d In dates
        Dim dr As Double = (From t In DisasterRegular
                            Where t.WorkDate = d And
                              t.DisasterRule = 1
                            Select t.WorkHours).Sum
        ' no one is scheduled to work Saturdays or Sundays
        If d.DayOfWeek = DayOfWeek.Saturday Or d.DayOfWeek = DayOfWeek.Sunday Then
          Disaster_Regular.Move_Day(dr, d, DisasterRegular, Disaster_StraightTime, Timelist)
        Else
          If d = "9/2/2019" Or d = "9/3/2019" Then ' 9/2/2019 is hte holiday, 9/3 is the date of the full activation start.
            Disaster_Regular.Move_Day(dr, d, DisasterRegular, Disaster_StraightTime, Timelist)
          End If
        End If


      Next
      'For these dates, we need To move everything worked past 8 hours To the disaster regular overtime
      dates = (From t In Timelist
               Where t.DisasterRule = 1 And
                 t.WorkDate.DayOfWeek <> DayOfWeek.Saturday And
                 t.WorkDate.DayOfWeek <> DayOfWeek.Sunday
               Select t.WorkDate).Distinct.ToList

      If dates.Count = 0 Then Exit Sub

      Dim hoursToMove As Double = 0
      Dim tmp As Double = 0

      For Each d In dates
        Dim dr As Double = (From t In DisasterRegular
                            Where t.WorkDate = d And
                                t.DisasterRule = 1
                            Select t.WorkHours).Sum
        If dr > 8 Then
          hoursToMove = dr - 8 ' everything over 8 needs to get moved to disaster regular overtime
          If dr > 0 Then
            If dr - hoursToMove >= 0 Then
              tmp = hoursToMove
              hoursToMove = 0
            Else
              tmp = dr
              hoursToMove = hoursToMove - dr
            End If
            Disaster_Regular.Move_Day(tmp, d, DisasterRegular, Disaster_StraightTime, Timelist)
          End If
        End If
      Next
    End Sub

    Private Sub Balance_NonExempt_Disaster_By_Period_By_Week(DisasterRegular As List(Of TelestaffTimeData))
      'Everything on these dates should be moved to disaster regular overtime.
      Dim dates = PopulateDisasterWorkDates(PayPeriodStart, DisasterPayRules)

      If dates.Count = 0 Then Exit Sub

      For Each d In dates
        Dim dr As Double = (From t In DisasterRegular
                            Where t.WorkDate = d And
                              t.DisasterRule = 1
                            Select t.WorkHours).Sum
        ' no one is scheduled to work Saturdays or Sundays
        If d.DayOfWeek = DayOfWeek.Sunday Then

          Disaster_Regular.Move_Day(dr, d, DisasterRegular, Disaster_Doubletime, Timelist)

        Else

          If d.DayOfWeek = DayOfWeek.Saturday Then
            Disaster_Regular.Move_Day(dr, d, DisasterRegular, Disaster_Overtime, Timelist)

          Else
            If d = "9/2/2019" Then ' 9/2/2019 is hte holiday, 9/3 is the date of the full activation start.
              Disaster_Regular.Move_Day(dr, d, DisasterRegular, Disaster_Overtime, Timelist)
            End If
          End If
        End If


      Next

      Dim hoursToMove As Double = 0
      Dim tmp As Double = 0

      Dim NormallyScheduledHours As Double = 8

      'If EmployeeData.EmployeeId = 1158 Then
      '  NormallyScheduledHours = 10
      'End If

      For Each d In dates

        Dim dr As Double = (From t In DisasterRegular
                            Where t.WorkDate = d And
                              t.DisasterRule = 2
                            Select t.WorkHours).Sum
        Disaster_Regular.Move_Day(dr, d, DisasterRegular, Disaster_Doubletime, Timelist)

        dr = (From t In DisasterRegular
              Where t.WorkDate = d And
                              t.DisasterRule = 1
              Select t.WorkHours).Sum

        If d = "8/30/2019" And EmployeeData.EmployeeId = 1158 Then
          NormallyScheduledHours = 0
        End If
        If d = "9/3/2019" Or d = "9/2/2019" Or d = "9/4/2019" Then
          NormallyScheduledHours = 0
        End If

        If dr > NormallyScheduledHours Then
          hoursToMove = dr - NormallyScheduledHours ' everything over 8 needs to get moved to disaster regular overtime
          If dr > 0 Then
            If dr - hoursToMove >= 0 Then
              tmp = hoursToMove
              hoursToMove = 0
            Else
              tmp = dr
              hoursToMove = hoursToMove - dr
            End If
            Disaster_Regular.Move_Day(tmp, d, DisasterRegular, Disaster_Overtime, Timelist)
          End If
        End If
      Next

      ' let's try and do something here with disasterrule = 0 times.
      ' we've defaulted all of the covid disaster times to disaster rule 0
      ' so they will get the regular pay rules, but the data will be encoded with the 
      ' covid project code.
    End Sub


    'Private Sub Balance_Exempt_Disaster_By_Week(Disaster_Regular_week As List(Of TelestaffTimeData))
    '  'Everything on these dates should be moved to disaster regular overtime.
    '  'Dim dates = (From t In Timelist
    '  '             Where t.DisasterRule = 1 And
    '  '               (t.WorkDate.DayOfWeek = DayOfWeek.Saturday Or
    '  '               t.WorkDate.DayOfWeek = DayOfWeek.Sunday)
    '  '             Select t.WorkDate).Distinct.ToList()
    '  Dim dates = PopulateDisasterWorkDates(PayPeriodStart, DisasterPayRules)

    '  If dates.Count = 0 Then Exit Sub
    '  For Each d In dates
    '    Dim r As Double = (From t In Regular_Week
    '                       Where t.WorkDate = d And
    '                         t.DisasterRule = 1
    '                       Select t.WorkHours).Sum
    '    Dim dr As Double = (From t In Disaster_Regular_week
    '                        Where t.WorkDate = d And
    '                          t.DisasterRule = 1
    '                        Select t.WorkHours).Sum
    '    Regular.Move_Day(r, d, Regular_Week, Disaster_StraightTime, Timelist)
    '    Disaster_Regular.Move_Day(dr, d, Disaster_Regular_week, Disaster_StraightTime, Timelist)
    '  Next
    '  'For these dates, we need To move everything worked past 8 hours To the disaster regular overtime
    '  dates = (From t In Timelist
    '           Where t.DisasterRule = 1 And
    '             t.WorkDate.DayOfWeek <> DayOfWeek.Saturday And
    '             t.WorkDate.DayOfWeek <> DayOfWeek.Sunday
    '           Select t.WorkDate).Distinct.ToList

    '  If dates.Count = 0 Then Exit Sub

    '  Dim hoursToMove As Double = 0
    '  Dim tmp As Double = 0

    '  For Each d In dates
    '    Dim r As Double = (From t In Regular_Week
    '                       Where t.WorkDate = d And
    '                           t.DisasterRule = 1
    '                       Select t.WorkHours).Sum
    '    Dim dr As Double = (From t In Disaster_Regular_week
    '                        Where t.WorkDate = d And
    '                            t.DisasterRule = 1
    '                        Select t.WorkHours).Sum
    '    If dr + r > 8 Then
    '      hoursToMove = dr + r - 8 ' everything over 8 needs to get moved to disaster regular overtime
    '      If dr > 0 Then
    '        If dr - hoursToMove >= 0 Then
    '          tmp = hoursToMove
    '          hoursToMove = 0
    '        Else
    '          tmp = dr
    '          hoursToMove = hoursToMove - dr
    '        End If
    '        Disaster_Regular.Move_Day(tmp, d, Disaster_Regular_week, Disaster_StraightTime, Timelist)
    '      End If
    '      If hoursToMove > 0 Then
    '        If r > 0 Then
    '          If r - hoursToMove >= 0 Then
    '            tmp = hoursToMove
    '            hoursToMove = 0
    '          Else
    '            tmp = r
    '            hoursToMove = hoursToMove - r
    '          End If
    '          Regular.Move_Day(tmp, d, Disaster_Regular_week, Disaster_StraightTime, Timelist)
    '        End If
    '      End If
    '    End If
    '  Next
    'End Sub

    'Private Sub Balance_NonExempt_Disaster_By_Week(Regular_Week As List(Of TelestaffTimeData),
    '                                            Disaster_Regular_week As List(Of TelestaffTimeData))
    '  'Everything on these dates should be moved to disaster regular overtime.
    '  Dim dates = (From t In Timelist
    '               Where t.DisasterRule = 2
    '               Select t.WorkDate).Distinct.ToList()

    '  If dates.Count = 0 Then Exit Sub

    '  Dim hoursToMove As Double = 0
    '  Dim tmp As Double = 0

    '  For Each d In dates
    '    Dim r As Double = (From t In Regular_Week
    '                       Where t.WorkDate = d And
    '                         t.DisasterRule = 2
    '                       Select t.WorkHours).Sum
    '    Dim dr As Double = (From t In Disaster_Regular_week
    '                        Where t.WorkDate = d And
    '                          t.DisasterRule = 2
    '                        Select t.WorkHours).Sum
    '    If dr + r > 8 Then
    '      hoursToMove = dr + r - 8 ' everything over 8 needs to get moved to disaster regular overtime
    '      If dr > 0 Then
    '        If dr - hoursToMove >= 0 Then
    '          tmp = hoursToMove
    '          hoursToMove = 0
    '        Else
    '          tmp = dr
    '          hoursToMove = hoursToMove - dr
    '        End If
    '        Disaster_Regular.Move_Day(tmp, d, Disaster_Regular_week, Disaster_Overtime, Timelist)
    '      End If
    '      If hoursToMove > 0 Then
    '        If r > 0 Then
    '          If r - hoursToMove >= 0 Then
    '            tmp = hoursToMove
    '            hoursToMove = 0
    '          Else
    '            tmp = r
    '            hoursToMove = hoursToMove - r
    '          End If
    '          Regular.Move_Day(tmp, d, Regular_Week, Disaster_Overtime, Timelist)
    '        End If
    '      End If
    '    End If
    '  Next
    'End Sub

    Private Function Handle_Disaster_Hours_Office(ByRef T As TelestaffTimeData) As Boolean
      Return False
      'If T.DisasterRule = 0 Or
      '  Not T.IsWorkingTime Or
      '  Not TelestaffProfileType = TelestaffProfileType.Office Then Return False

      'If IsExempt Then
      '  If T.DisasterRule = 2 Then Return False
      '  Select Case T.WorkCode
      '    Case "002", "299"
      '      Return False
      '    Case Else
      '      If (T.WorkDate.DayOfWeek = DayOfWeek.Saturday Or
      '        T.WorkDate.DayOfWeek = DayOfWeek.Sunday) Then
      '        Disaster_StraightTime.Add(T)
      '        Return True
      '      End If
      '  End Select
      '  Return False
      'Else
      '  Select Case T.DisasterRule
      '    Case 1
      '      Disaster_Doubletime.Add(T)
      '      Return True
      '    Case 2

      '  End Select

      'End If
      'Return False
    End Function


    Private Sub Check_Exceptions()
      Dim tmpList As New List(Of TimecardTimeException)

      Dim e As EPP = Me
      tmpList.AddRange(Get_Telestaff_Exceptions(e))
      For Each t In e.RawTimeList ' We have to use the rawtimelist because we've already modified the data, so it won't be correct.
        tmpList.AddRange(Get_Telestaff_Exceptions(e, t))
      Next
      'tmpList.AddRange(Get_Telestaff_Exceptions(e, e.Timelist))
      For Each tet In tmpList
        If tet.ExceptionType = TelestaffExceptionType.exceptionError Then
          If Not ErrorList.Contains(tet.Message) Then ErrorList.Add(tet.Message)
        Else
          If Not WarningList.Contains(tet.Message) Then WarningList.Add(tet.Message)
        End If
      Next
    End Sub



    Private Sub Balance_Timebanks_Versus_Hours_Requested()
      ' We also need to check the sick, vacation, holiday hours used and comp time used against
      ' their respective banks.
      ' Holiday time moves from holiday to vacation to LWOP if they don't have the necessary time banked
      ' Sick time moves to LWOP if there isn't enough banked.
      Dim tmp As Double = 0
      If Sick.TotalHours > EmployeeData.Banked_Sick_Hours Then
        tmp = EmployeeData.Banked_Sick_Hours - (EmployeeData.Banked_Sick_Hours Mod 0.25)
        ErrorList.Add("Too many Sick hours used. Banked Sick Hours: " & EmployeeData.Banked_Sick_Hours & " -- Requested Sick Hours: " & Sick.TotalHours)
        Sick.Move_Last(Sick.TotalHours - tmp, _lwop, Timelist)
      End If
      If Holiday_Time_Used.TotalHours > Banked_Holiday_Hours Then
        tmp = Banked_Holiday_Hours - (Banked_Holiday_Hours Mod 0.25)
        ErrorList.Add("Too many Holiday hours used. Banked Holiday Hours: " & Banked_Holiday_Hours & " -- Requested Holiday Hours: " & Holiday_Time_Used.TotalHours)
        Holiday_Time_Used.Move_Last(Holiday_Time_Used.TotalHours - tmp, _lwop, Timelist)
      End If
      If Vacation.TotalHours > EmployeeData.Banked_Vacation_Hours Then
        tmp = EmployeeData.Banked_Vacation_Hours - (EmployeeData.Banked_Vacation_Hours Mod 0.25)
        ErrorList.Add("Too many Vacation hours used. Banked Vacation Hours: " & EmployeeData.Banked_Vacation_Hours & " -- Requested Vacation Hours: " & Vacation.TotalHours)
        Vacation.Move_Last(Vacation.TotalHours - tmp, _lwop, Timelist)
      End If
    End Sub

    Private Sub Balance_Overtime_By_Payperiod_Hours()
      Dim toMove As Double = 0
      If Scheduled_Hours > TelestaffHoursNeededForOvertime Then
        ' the hours greater will need to be either scheduled overtime or
        ' scheduled regular overtime
        Dim diff As Double = (Scheduled_Hours - TelestaffHoursNeededForOvertime)
        If Total_Non_Working_Hours >= diff Then
          ' Since they have more scheduled non-working hours than they do potential hours of scheduled overtime, 
          ' then all of the potential scheduled overtime hours are going to be scheduled regular overtime
          Regular.Move_Last(diff, _scheduled_regular_overtime, Timelist)
        Else
          Regular.Move_Last(Total_Non_Working_Hours, _scheduled_regular_overtime, Timelist)
          Regular.Move_Last(diff - Total_Non_Working_Hours, _scheduled_overtime, Timelist)
        End If
      Else
        ' If their scheduled hours aren't enough for Overtime, we need to check to make sure that we don't need to 
        ' move some hours from Unscheduled OT to Unscheduled Reg.
        If Total_Hours > Scheduled_Hours Then
          Dim diff As Double = Total_Hours - Scheduled_Hours
          If diff <= TelestaffHoursNeededForOvertime - Scheduled_Hours Then

            If Unscheduled_Overtime.TotalHours >= diff Then
              Unscheduled_Overtime.Move_First(diff, _unscheduled_regular_overtime, Timelist)
              diff = 0
            Else
              toMove = diff - Unscheduled_Overtime.TotalHours
              Unscheduled_Overtime.Move_First(diff, _unscheduled_regular_overtime, Timelist)
              diff = diff - toMove
            End If
            If diff > 0 And Disaster_Overtime.TotalHours > 0 Then
              Disaster_Overtime.Move_First(diff, _disaster_straighttime, Timelist)
            End If

          Else
            toMove = TelestaffHoursNeededForOvertime - Scheduled_Hours
            If Unscheduled_Overtime.TotalHours >= toMove Then
              Unscheduled_Overtime.Move_First(toMove, _unscheduled_regular_overtime, Timelist)
              toMove = 0
            Else
              If Unscheduled_Overtime.TotalHours > 0 Then
                Dim tmp As Double = toMove - Unscheduled_Overtime.TotalHours
                Unscheduled_Overtime.Move_First(toMove - Unscheduled_Overtime.TotalHours, _unscheduled_regular_overtime, Timelist)
                toMove = tmp
              End If

            End If
            If toMove > 0 And Disaster_Overtime.TotalHours > 0 Then
              Disaster_Overtime.Move_First(toMove, _disaster_straighttime, Timelist)
            End If
          End If
        End If
      End If
      If Regular.TotalHours +
        Disaster_Regular.TotalHours +
        Total_Non_Working_Hours > TelestaffHoursNeededForOvertime Then

        Dim diff As Double = (Regular.TotalHours +
          Disaster_Regular.TotalHours +
          Total_Non_Working_Hours) -
          TelestaffHoursNeededForOvertime -
          Non_Working.TotalHours

        If diff > 0 Then
          'If there are any scheduled_regular_overtime hours, 
          ' they've already been subtracted from the total non working hours
          If diff - (Total_Non_Working_Hours -
            Scheduled_Regular_Overtime.TotalHours -
            Disaster_StraightTime.TotalHours) > 0 Then
            ' This is the total number of unscheduled OT
            'Dim e As New ErrorLog("Found Telestaff calculation condition " + EmployeeData.EmployeeId.ToString, "please validate", "", "", "")
            toMove = diff - (Total_Non_Working_Hours - Scheduled_Regular_Overtime.TotalHours - Disaster_StraightTime.TotalHours)
            If Regular.TotalHours > 0 And toMove > 0 Then
              Dim regular_diff = Math.Max(Regular.TotalHours, toMove)
              toMove = toMove - regular_diff
              Regular.Move_First(regular_diff, _unscheduled_overtime, Timelist)
              'Regular.Move_First(diff - toMove, _unscheduled_overtime, Timelist) ' Not sure why this is here
            End If
            If (Disaster_Regular.TotalHours > 0 And toMove > 0) Then
              Disaster_Regular.Move_First(toMove, _disaster_overtime, Timelist)
            End If
          End If
        End If


      End If
      If (Total_Non_Working_Hours > Scheduled_Regular_Overtime.TotalHours +
        Disaster_StraightTime.TotalHours +
        Unscheduled_Regular_Overtime.TotalHours) AndAlso
        Unscheduled_Overtime.TotalHours + Disaster_Overtime.TotalHours > 0 Then

        Dim diff As Double = Total_Non_Working_Hours +
          (TelestaffHoursNeededForOvertime - Scheduled_Hours) -
          (Scheduled_Regular_Overtime.TotalHours +
          Unscheduled_Regular_Overtime.TotalHours +
          Disaster_StraightTime.TotalHours)
        'If there are any scheduled_regular_overtime hours, they've already been subtracted from the total non working hours
        If diff > Unscheduled_Overtime.TotalHours Then
          toMove = Unscheduled_Overtime.TotalHours
          Unscheduled_Overtime.Move_First(toMove, _unscheduled_regular_overtime, Timelist)
          diff = diff - toMove
        Else
          Unscheduled_Overtime.Move_First(diff, _unscheduled_regular_overtime, Timelist)
          diff = 0
        End If
        If diff > 0 And Disaster_Overtime.TotalHours > 0 Then
          If diff > Disaster_Overtime.TotalHours Then
            Disaster_Overtime.Move_First(Disaster_Overtime.TotalHours, _disaster_straighttime, Timelist)
          Else
            Disaster_Overtime.Move_First(diff, _disaster_straighttime, Timelist)
          End If
        End If
      Else
        If Regular.TotalHours +
          Disaster_Regular.TotalHours +
          Scheduled_Regular_Overtime.TotalHours +
          Disaster_StraightTime.TotalHours +
          Unscheduled_Regular_Overtime.TotalHours < TelestaffHoursNeededForOvertime Then

          Dim diff As Double = TelestaffHoursNeededForOvertime -
            (Regular.TotalHours +
            Disaster_Regular.TotalHours +
            Scheduled_Regular_Overtime.TotalHours +
            Unscheduled_Regular_Overtime.TotalHours +
            Disaster_StraightTime.TotalHours)

          If diff > Unscheduled_Overtime.TotalHours Then
            toMove = Unscheduled_Overtime.TotalHours
            Unscheduled_Overtime.Move_First(toMove, _unscheduled_regular_overtime, Timelist)
            diff = diff - toMove
          Else
            Unscheduled_Overtime.Move_First(diff, _unscheduled_regular_overtime, Timelist)
            diff = 0
          End If
          If diff > 0 And Disaster_Overtime.TotalHours > 0 Then
            If diff > Disaster_Overtime.TotalHours Then
              Disaster_Overtime.Move_First(Disaster_Overtime.TotalHours, _disaster_straighttime, Timelist)
            Else
              Disaster_Overtime.Move_First(diff, _disaster_straighttime, Timelist)
            End If
          End If
        End If
      End If
    End Sub

    Private Sub Balance_Overtime_By_Weekly_Hours()
      ' Week 1
      If Scheduled_Hours_Week1 > HoursNeededForOvertimeByWeek Then
        ' the hours greater will need to be either scheduled overtime or
        ' scheduled regular overtime
        'Dim diff As Double = (Scheduled_Hours - TelestaffHoursNeededForOvertime)
        Dim diff As Double = (Scheduled_Hours_Week1 - HoursNeededForOvertimeByWeek)
        If Total_Non_Working_Hours_Week1 >= diff Then
          ' Since they have more scheduled non-working hours than they do potential hours of scheduled overtime, 
          ' then all of the potential scheduled overtime hours are going to be scheduled regular overtime
          Regular.Move_Week1(diff, _scheduled_regular_overtime, Timelist)
          'Regular.Move_Week1(Total_Non_Working_Hours_week1 - diff, _)

        Else
          Regular.Move_Week1(Total_Non_Working_Hours_Week1, _scheduled_regular_overtime, Timelist)
          Regular.Move_Week1(diff - Total_Non_Working_Hours_Week1, _scheduled_overtime, Timelist)
        End If
      Else
        ' If their scheduled hours aren't enough for Overtime, we need to check to make sure that we don't need to 
        ' move some hours from Unscheduled OT to Unscheduled Reg.
        If Total_Hours_Week1 > Scheduled_Hours_Week1 Then
          Dim diff As Double = Total_Hours_Week1 - Scheduled_Hours_Week1
          If diff <= HoursNeededForOvertimeByWeek - Scheduled_Hours_Week1 Then
            Unscheduled_Overtime.Move_Week1(diff, _unscheduled_regular_overtime, Timelist)
          Else
            Unscheduled_Overtime.Move_Week1(TelestaffHoursNeededForOvertime - Scheduled_Hours,
                                            _unscheduled_regular_overtime, Timelist)
          End If
        End If
      End If
      If Unscheduled_Overtime.TotalHours_Week1 > 0 Then
        If Total_Hours_Week1 <= HoursNeededForOvertimeByWeek Then
          Unscheduled_Overtime.Move_Week1(Unscheduled_Overtime.TotalHours_Week1, _unscheduled_regular_overtime, Timelist)
        End If
        Dim DisasterTotalOvertimeHoursWeek1 = Disaster_Doubletime.TotalHours_Week1 + Disaster_Overtime.TotalHours_Week1 + Disaster_StraightTime.TotalHours_Week1
        If Total_Hours_Week1 - DisasterTotalOvertimeHoursWeek1 <= HoursNeededForOvertimeByWeek Then
          Unscheduled_Overtime.Move_Week2(Unscheduled_Overtime.TotalHours_Week1, _unscheduled_regular_overtime, Timelist)
        End If
      End If



      If (Total_Non_Working_Hours_Week1 > (Scheduled_Regular_Overtime.TotalHours_Week1 +
        Comp_Time_Banked.TotalHours_Week1 + Unscheduled_Regular_Overtime.TotalHours_Week1) -
                    Unscheduled_Double_Overtime.TotalHours_Week1) AndAlso Unscheduled_Overtime.TotalHours_Week1 > 0 Then
        Dim diff As Double = Total_Hours_Week1 + Comp_Time_Banked.TotalHours_Week1 - Total_Non_Working_Hours_Week1 -
          HoursNeededForOvertimeByWeek - Unscheduled_Double_Overtime.TotalHours_Week1
        'If there are any scheduled_regular_overtime hours, they've already been subtracted from the total non working hours
        If diff >= 0 Then
          ' This is the total number of unscheduled OT
          Unscheduled_Overtime.Move_Week1(Unscheduled_Overtime.TotalHours_Week1 - diff, _unscheduled_regular_overtime, Timelist)
          'Unscheduled_Overtime.Move(diff - (diff - (Total_Non_Working_Hours - Scheduled_Regular_Overtime.TotalHours)), _unscheduled_regular_overtime, PayPeriodStart)
        ElseIf diff < 0 Then
          Unscheduled_Overtime.Move_Week1(Unscheduled_Overtime.TotalHours_Week1, _unscheduled_regular_overtime, Timelist)
        End If
      End If

      ' Week 2
      If Scheduled_Hours_Week2 > HoursNeededForOvertimeByWeek Then
        ' the hours greater will need to be either scheduled overtime or
        ' scheduled regular overtime
        'Dim diff As Double = (Scheduled_Hours - TelestaffHoursNeededForOvertime)
        Dim diff As Double = (Scheduled_Hours_Week2 - HoursNeededForOvertimeByWeek)
        If Total_Non_Working_Hours_Week2 >= diff Then
          ' Since they have more scheduled non-working hours than they do potential hours of scheduled overtime, 
          ' then all of the potential scheduled overtime hours are going to be scheduled regular overtime
          Regular.Move_Week2(diff, _scheduled_regular_overtime, Timelist)
        Else
          Regular.Move_Week2(Total_Non_Working_Hours_Week2, _scheduled_regular_overtime, Timelist)
          Regular.Move_Week2(diff - Total_Non_Working_Hours_Week2, _scheduled_overtime, Timelist)
        End If
      Else
        ' If their scheduled hours aren't enough for Overtime, we need to check to make sure that we don't need to 
        ' move some hours from Unscheduled OT to Unscheduled Reg.
        If Total_Hours_Week2 > Scheduled_Hours_Week2 Then
          Dim diff As Double = Total_Hours_Week2 - Scheduled_Hours_Week2
          If diff <= HoursNeededForOvertimeByWeek - Scheduled_Hours_Week2 Then
            Unscheduled_Overtime.Move_Week2(diff, _unscheduled_regular_overtime, Timelist)
          Else
            Unscheduled_Overtime.Move_Week2(TelestaffHoursNeededForOvertime - Scheduled_Hours, _unscheduled_regular_overtime, Timelist)
          End If
        End If
      End If
      If Total_Hours_Week2 <= HoursNeededForOvertimeByWeek AndAlso Unscheduled_Overtime.TotalHours_Week2 > 0 Then
        Unscheduled_Overtime.Move_Week2(Unscheduled_Overtime.TotalHours_Week2, _unscheduled_regular_overtime, Timelist)
      End If
      Dim DisasterTotalOvertimeHoursWeek2 = Disaster_Doubletime.TotalHours_Week2 + Disaster_Overtime.TotalHours_Week2 + Disaster_StraightTime.TotalHours_Week2
      If Total_Hours_Week2 - DisasterTotalOvertimeHoursWeek2 <= HoursNeededForOvertimeByWeek AndAlso
        Unscheduled_Overtime.TotalHours_Week2 > 0 Then
        Unscheduled_Overtime.Move_Week2(Unscheduled_Overtime.TotalHours_Week2, _unscheduled_regular_overtime, Timelist)
      End If
      If (Total_Non_Working_Hours_Week2 > (Scheduled_Regular_Overtime.TotalHours_Week2 + Comp_Time_Banked.TotalHours_Week2 + Unscheduled_Regular_Overtime.TotalHours_Week2) -
                    Unscheduled_Double_Overtime.TotalHours_Week2) AndAlso Unscheduled_Overtime.TotalHours_Week2 > 0 Then
        Dim diff As Double = Total_Hours_Week2 + Comp_Time_Banked.TotalHours_Week2 - Total_Non_Working_Hours_Week2 - HoursNeededForOvertimeByWeek - Unscheduled_Double_Overtime.TotalHours_Week2
        'If there are any scheduled_regular_overtime hours, they've already been subtracted from the total non working hours
        If diff >= 0 Then
          ' This is the total number of unscheduled OT
          Unscheduled_Overtime.Move_Week2(Unscheduled_Overtime.TotalHours_Week2 - diff, _unscheduled_regular_overtime, Timelist)
          'Unscheduled_Overtime.Move(diff - (diff - (Total_Non_Working_Hours - Scheduled_Regular_Overtime.TotalHours)), _unscheduled_regular_overtime, PayPeriodStart)
        ElseIf diff < 0 Then
          Unscheduled_Overtime.Move_Week2(Unscheduled_Overtime.TotalHours_Week2, _unscheduled_regular_overtime, Timelist)
        End If
      End If

    End Sub

    Public ReadOnly Property Union_Time_Pool As GroupedHours
      Get
        Return _union_time_pool
      End Get
    End Property

    Public ReadOnly Property Regular As GroupedHours
      Get
        Return _regular
      End Get
    End Property

    Public ReadOnly Property Non_Paid As GroupedHours
      Get
        Return _non_paid
      End Get
    End Property

    Public ReadOnly Property Term_Hours As GroupedHours
      Get
        Return _term_hours
      End Get
    End Property

    Public ReadOnly Property Leave_Without_Pay As GroupedHours
      Get
        Return _lwop
      End Get
    End Property

    Public ReadOnly Property Scheduled_Overtime As GroupedHours
      Get
        Return _scheduled_overtime
      End Get
    End Property

    Public ReadOnly Property Comp_Time_Used As GroupedHours
      Get
        Return _comp_time_used
      End Get
    End Property

    Public ReadOnly Property Comp_Time_Banked As GroupedHours
      Get
        Return _comp_time_banked
      End Get
    End Property

    Public ReadOnly Property Non_Working As GroupedHours
      Get
        Return _non_working
      End Get
    End Property

    Public ReadOnly Property Holiday_Time_Used As GroupedHours
      Get
        Return _holiday_time_used
      End Get
    End Property

    Public ReadOnly Property Holiday_Time_Banked As GroupedHours
      Get
        Return _holiday_time_banked
      End Get
    End Property

    Public ReadOnly Property Unscheduled_Regular_Overtime As GroupedHours
      Get
        Return _unscheduled_regular_overtime
      End Get
    End Property

    Public ReadOnly Property Holiday_Paid As GroupedHours
      Get
        Return _holiday_paid
      End Get
    End Property

    Public ReadOnly Property Scheduled_Regular_Overtime As GroupedHours
      Get
        Return _scheduled_regular_overtime
      End Get
    End Property

    Public ReadOnly Property Sick As GroupedHours
      Get
        Return _sick
      End Get
    End Property

    Public ReadOnly Property Vacation As GroupedHours
      Get
        Return _vacation
      End Get
    End Property

    Public ReadOnly Property Unscheduled_Double_Overtime As GroupedHours
      Get
        Return _unscheduled_double_overtime
      End Get
    End Property

    Public ReadOnly Property Unscheduled_Overtime As GroupedHours
      Get
        Return _unscheduled_overtime
      End Get
    End Property

    Public ReadOnly Property Disaster_Overtime As GroupedHours
      Get
        Return _disaster_overtime
      End Get
    End Property

    Public ReadOnly Property Disaster_Doubletime As GroupedHours
      Get
        Return _disaster_doubletime
      End Get
    End Property

    Public ReadOnly Property Disaster_StraightTime As GroupedHours
      Get
        Return _disaster_straighttime
      End Get
    End Property

    Public ReadOnly Property Disaster_Regular As GroupedHours
      Get
        Return _disaster_regular
      End Get
    End Property

    Public ReadOnly Property Admin_Leave_Disaster As GroupedHours
      Get
        Return _admin_leave_disaster
      End Get
    End Property

    ReadOnly Property HoursNeededForOvertimeByWeek() As Double
      Get
        Return TelestaffHoursNeededForOvertime / 2
      End Get
    End Property

    ReadOnly Property IsOvertime_Calculated_Weekly() As Boolean
      Get
        Return TelestaffProfileType = TelestaffProfileType.Office
      End Get
    End Property

    ReadOnly Property IsExempt() As Boolean
      Get
        Return (EmployeeData.EmployeeType = "E")
      End Get
    End Property

    ReadOnly Property Total_Hours()
      Get
        Return (From t In Timelist Where t.IsPaidTime Select t.WorkHours).Sum()
      End Get
    End Property

    ReadOnly Property Total_Hours_Week1()
      Get
        Return (From t In Timelist Where t.WorkDate >= PayPeriodStart And
                        t.WorkDate < PayPeriodStart.AddDays(7) And t.IsPaidTime Select t.WorkHours).Sum()
      End Get
    End Property

    ReadOnly Property Total_Hours_Week2()
      Get
        Return (From t In Timelist Where t.WorkDate >= PayPeriodStart.AddDays(7) And
                        t.WorkDate < PayPeriodStart.AddDays(14) And t.IsPaidTime Select t.WorkHours).Sum()
      End Get
    End Property

    'ReadOnly Property Total_Calculated_Hours()
    '  Get
    '    ' This is how we test to make sure that we are balancing our numbers across the board.
    '    Return Regular.TotalHours + Scheduled_Overtime.TotalHours +
    '      Leave_Without_Pay.TotalHours + Vacation.TotalHours + Sick.TotalHours +
    '      Scheduled_Regular_Overtime.TotalHours + Holiday_Time_Used.TotalHours +
    '      Unscheduled_Regular_Overtime.TotalHours + Unscheduled_Overtime.TotalHours +
    '      Unscheduled_Double_Overtime.TotalHours + Term_Hours.TotalHours +
    '      Disaster_Regular.TotalHours + Disaster_Overtime.TotalHours +
    '      Disaster_Doubletime.TotalHours + Disaster_StraightTime.TotalHours
    '  End Get
    'End Property


    ReadOnly Property Worked_Hours() As Double
      Get
        Select Case TelestaffProfileType
          Case TelestaffProfileType.Field, TelestaffProfileType.Dispatch
            Return Regular.TotalHours + Disaster_Regular.TotalHours
          Case TelestaffProfileType.Office
            Return Regular.TotalHours + Disaster_Regular.TotalHours
          Case Else
            Return 0
        End Select
      End Get
    End Property

    ReadOnly Property Scheduled_Hours() As Double
      Get
        Select Case TelestaffProfileType
          Case TelestaffProfileType.Field, TelestaffProfileType.Dispatch
            Return Regular.TotalHours + Vacation.TotalHours +
              Holiday_Time_Used.TotalHours + Leave_Without_Pay.TotalHours +
              Sick.TotalHours + Comp_Time_Used.TotalHours +
              Union_Time_Pool.TotalHours + Disaster_Regular.TotalHours
          Case TelestaffProfileType.Office
            If IsExempt Then
              Return 80
            Else
              Dim tmp As Double = Regular.TotalHours + Vacation.TotalHours + Leave_Without_Pay.TotalHours + Sick.TotalHours + Comp_Time_Used.TotalHours + Union_Time_Pool.TotalHours + Disaster_Regular.TotalHours
              If tmp > 80 Then Return tmp Else Return 80
            End If
          Case Else
            Return 0
        End Select
      End Get
    End Property

    ReadOnly Property Scheduled_Hours_Week1() As Double
      Get
        Select Case TelestaffProfileType
          Case TelestaffProfileType.Field, TelestaffProfileType.Dispatch
            Return Regular.TotalHours_Week1 + Vacation.TotalHours_Week1 + Holiday_Time_Used.TotalHours_Week1 +
              Leave_Without_Pay.TotalHours_Week1 + Sick.TotalHours_Week1 + Comp_Time_Used.TotalHours_Week1 +
              Union_Time_Pool.TotalHours_Week1            '+ Disaster_Regular.TotalHours_Week1
          Case TelestaffProfileType.Office
            If IsExempt Then
              Return 40
            Else
              Dim tmp As Double = Regular.TotalHours_Week1 + Vacation.TotalHours_Week1 +
                Leave_Without_Pay.TotalHours_Week1 + Sick.TotalHours_Week1 +
                Comp_Time_Used.TotalHours_Week1 + Union_Time_Pool.TotalHours_Week1 '+ Disaster_Regular.TotalHours_Week1
              If tmp > 40 Then Return tmp Else Return 40
            End If
          Case Else
            Return 0
        End Select
      End Get
    End Property

    ReadOnly Property Scheduled_Hours_Week2() As Double
      Get
        Select Case TelestaffProfileType
          Case TelestaffProfileType.Field, TelestaffProfileType.Dispatch
            Return Regular.TotalHours_Week2 + Vacation.TotalHours_Week2 + Holiday_Time_Used.TotalHours_Week2 +
              Leave_Without_Pay.TotalHours_Week2 + Sick.TotalHours_Week2 + Comp_Time_Used.TotalHours_Week2 +
              Union_Time_Pool.TotalHours_Week2            '+ Disaster_Regular.TotalHours_Week2
          Case TelestaffProfileType.Office
            If IsExempt Then
              Return 40
            Else
              Dim tmp As Double = Regular.TotalHours_Week2 + Vacation.TotalHours_Week2 +
                Leave_Without_Pay.TotalHours_Week2 + Sick.TotalHours_Week2 + Comp_Time_Used.TotalHours_Week2 +
                Union_Time_Pool.TotalHours_Week2 '+ Disaster_Regular.TotalHours_Week2
              If tmp > 40 Then Return tmp Else Return 40
            End If
          Case Else
            Return 0
        End Select
      End Get
    End Property

    ReadOnly Property Real_Scheduled_Hours_Week1() As Double
      Get
        Select Case TelestaffProfileType
          Case TelestaffProfileType.Field, TelestaffProfileType.Dispatch
            Return Regular.TotalHours_Week1 + Vacation.TotalHours_Week1 + Holiday_Time_Used.TotalHours_Week1 +
                                Leave_Without_Pay.TotalHours_Week1 + Sick.TotalHours_Week1 + Comp_Time_Used.TotalHours_Week1 +
                                Union_Time_Pool.TotalHours_Week1 + Disaster_Regular.TotalHours_Week1 +
                                Admin_Leave_Disaster.TotalHours_Week1
          Case TelestaffProfileType.Office
            If IsExempt Then
              Return 40
            Else
              Dim tmp As Double = Regular.TotalHours_Week1 + Vacation.TotalHours_Week1 +
                Leave_Without_Pay.TotalHours_Week1 + Sick.TotalHours_Week1 +
                Comp_Time_Used.TotalHours_Week1 + Union_Time_Pool.TotalHours_Week1 +
                Disaster_Regular.TotalHours_Week1 + Disaster_Doubletime.TotalHours_Week1 +
                Admin_Leave_Disaster.TotalHours_Week1
              Return tmp
            End If
          Case Else
            Return 0
        End Select
      End Get
    End Property

    ReadOnly Property Real_Scheduled_Hours_Week2() As Double
      Get
        Select Case TelestaffProfileType
          Case TelestaffProfileType.Field, TelestaffProfileType.Dispatch
            Return Regular.TotalHours_Week2 + Vacation.TotalHours_Week2 + Holiday_Time_Used.TotalHours_Week2 +
                                Leave_Without_Pay.TotalHours_Week2 + Sick.TotalHours_Week2 + Comp_Time_Used.TotalHours_Week2 +
                                Union_Time_Pool.TotalHours_Week2 + Disaster_Regular.TotalHours_Week2 +
                                Admin_Leave_Disaster.TotalHours_Week2
          Case TelestaffProfileType.Office
            If IsExempt Then
              Return 40
            Else
              Dim tmp As Double = Regular.TotalHours_Week2 + Vacation.TotalHours_Week2 +
                Leave_Without_Pay.TotalHours_Week2 + Sick.TotalHours_Week2 +
                Comp_Time_Used.TotalHours_Week2 + Union_Time_Pool.TotalHours_Week2 +
                Disaster_Regular.TotalHours_Week2 + Disaster_Doubletime.TotalHours_Week2 +
                Admin_Leave_Disaster.TotalHours_Week2
              Return tmp
            End If
          Case Else
            Return 0
        End Select
      End Get
    End Property

    ReadOnly Property Total_Non_Working_Hours() As Double
      Get
        Return Vacation.TotalHours + Sick.TotalHours +
          Leave_Without_Pay.TotalHours + Holiday_Time_Used.TotalHours +
          Non_Working.TotalHours + Term_Hours.TotalHours
      End Get
    End Property

    ReadOnly Property Total_Non_Working_Hours_Week1() As Double
      Get
        Return Vacation.TotalHours_Week1 + Sick.TotalHours_Week1 +
          Leave_Without_Pay.TotalHours_Week1 + Holiday_Time_Used.TotalHours_Week1 +
          Non_Working.TotalHours_Week1 + Comp_Time_Used.TotalHours_Week1 +
          Term_Hours.TotalHours_Week1
      End Get
    End Property

    ReadOnly Property Total_Non_Working_Hours_Week2() As Double
      Get
        Return Vacation.TotalHours_Week2 + Sick.TotalHours_Week2 +
          Leave_Without_Pay.TotalHours_Week2 + Holiday_Time_Used.TotalHours_Week2 +
          Non_Working.TotalHours_Week2 + Comp_Time_Used.TotalHours_Week2 +
          Term_Hours.TotalHours_Week2
      End Get
    End Property

    ReadOnly Property Number_of_Payrates() As Integer
      Get
        Return Payrates.Count
      End Get
    End Property

    ReadOnly Property Payrates() As List(Of Double)
      Get
        Return (From t In Timelist Select t.PayRate).Distinct.ToList
      End Get
    End Property

    ReadOnly Property HasHoliday As Boolean
      Get
        Return Get_Holidays_By_Payperiod(PayPeriodStart, False).Count > 0
        'Dim HL As List(Of Date) = getHolidayList(PayPeriodStart.Year, False)
        'Return ((From h In HL Where h >= PayPeriodStart And h < PayPeriodStart.AddDays(14) Select h).Count > 0)
      End Get
    End Property

    Public ReadOnly Property Holiday_Requested() As Double
      Get
        Return (From t In Timelist
                Where t.WorkType = "123" Or t.WorkType = "430"
                Select t.WorkHours).Sum
      End Get
    End Property

    Public ReadOnly Property Vacation_Requested() As Double
      Get
        Return (From t In Timelist
                Where t.WorkType = "101" Or t.WorkType = "100"
                Select t.WorkHours).Sum
      End Get
    End Property

    Public ReadOnly Property Sick_Requested() As Double
      Get
        Return (From t In Timelist
                Where t.WorkType = "110" Or t.WorkType = "111"
                Select t.WorkHours).Sum
      End Get
    End Property

    Public Function Get_Profile_Type() As TelestaffProfileType
      If (From t In Timelist
          Where t.ProfileType = TelestaffProfileType.Field
          Select t).Count > 0 Then
        Return TelestaffProfileType.Field
      ElseIf (From t In Timelist
              Where t.ProfileType = TelestaffProfileType.Dispatch
              Select t).Count > 0 Then
        Return TelestaffProfileType.Dispatch
      Else
        Return TelestaffProfileType.Office
      End If
    End Function

    Public Function Get_Hours_Needed_For_Overtime() As Double
      If Timelist.Count = 0 Then Return 0
      Return (From t In Timelist
              Where t.ProfileType = Get_Profile_Type()
              Select t.FLSAHoursRequirement).First
    End Function

    Private Function Get_Telestaff_Exceptions(e As EPP) As List(Of TimecardTimeException)
      Dim TEList As New List(Of TimecardTimeException)
      If e.TelestaffProfileType = TelestaffProfileType.Office And e.EmployeeData.isFulltime And e.EmployeeData.HireDate <= PayPeriodStart Then
        If Real_Scheduled_Hours_Week1 < 40 And Now > e.PayPeriodStart.AddDays(6).AddHours(8) Then
          TEList.Add(New TimecardTimeException(e.EmployeeData, TelestaffExceptionType.exceptionError, "Not enough hours scheduled in Week 1."))
        End If
        If Real_Scheduled_Hours_Week2 < 40 And Now > e.PayPeriodStart.AddDays(13).AddHours(8) Then
          TEList.Add(New TimecardTimeException(e.EmployeeData, TelestaffExceptionType.exceptionError, "Not enough hours scheduled in Week 2."))
        End If
      End If
      If e.TelestaffProfileType = TelestaffProfileType.Office Then
        If e.Comp_Time_Banked.TotalHours + EmployeeData.Banked_Comp_Hours - Comp_Time_Used.TotalHours > 32 Then
          TEList.Add(New TimecardTimeException(e.EmployeeData, TelestaffExceptionType.exceptionError, "You can only bank a maximum of 32 hours of Comp Time."))
        End If

      End If

      If _term_hours.TotalHours > 0 Then
        TEList.Add(New TimecardTimeException(e.EmployeeData, TelestaffExceptionType.exceptionWarning, "Employee has Term Hours entered."))
      End If
      If e.Total_Hours = 0 Then
        'If e.Timelist.Count = 0 Then
        TEList.Add(New TimecardTimeException(e.EmployeeData, TelestaffExceptionType.exceptionError, "No hours entered in this payperiod."))
      End If

      If e.EmployeeData.Banked_Sick_Hours <= 24 And e.TelestaffProfileType = TelestaffProfileType.Field Then
        TEList.Add(New TimecardTimeException(e.EmployeeData, TelestaffExceptionType.exceptionWarning, "Employee has " & e.EmployeeData.Banked_Sick_Hours & " Sick hours banked."))
      End If

      ' Checking to see if they are using more than 24 hours of admin leave in this pay period
      ' if so, want to throw up a warning.
      If (From t In e.Timelist Where t.WorkTypeAbrv = "ADM" Select t.WorkHours).Sum > 24 Then
        TEList.Add(New TimecardTimeException(e.EmployeeData, TelestaffExceptionType.exceptionWarning, "More than 24 hours of Admin Leave used."))
      End If

      ' Here we're going to do some checks for some of the jobs that
      ' are restricted on vacation use for their first 180 days.
      Dim ffclass() As String = {"0740", "0530", "0690", "0230", "0110"}
      If ffclass.Contains(e.EmployeeData.Classify) Then
        Dim vTypes() As String = {"V", "VBC"}
        If e.PayPeriodStart.Subtract(e.EmployeeData.HireDate).TotalDays < 181 Then
          If (From t In e.Timelist
              Where vTypes.Contains(t.WorkTypeAbrv) AndAlso
                    t.WorkDate.Subtract(e.EmployeeData.HireDate).TotalDays < 181
              Select t).Count > 0 Then
            TEList.Add(New TimecardTimeException(e.EmployeeData, TelestaffExceptionType.exceptionError, "Field employees cannot use Vacation hours for their first 180 days."))
          End If
        End If
      End If
      ' No employees are able to request leave for their first 90 days
      Dim leaveTypes() As String = {"VS", "SS", "S", "V", "VBC", "SLOT", "DSL"}
      If e.PayPeriodStart.Subtract(e.EmployeeData.HireDate).TotalDays < 91 Then
        If (From t In e.Timelist
            Where leaveTypes.Contains(t.WorkTypeAbrv.ToUpper) AndAlso
                    t.WorkDate.Subtract(e.EmployeeData.HireDate).TotalDays < 91
            Select t).Count > 0 Then
          TEList.Add(New TimecardTimeException(e.EmployeeData, TelestaffExceptionType.exceptionError, "Employees cannot use leave hours for their first 90 days."))
        End If
      End If



      ' Here we're going to test some basic stuff
      ' Each telestaff profile type has an expected maximum number of hours we should see in
      ' Regular and Scheduled Overtime.
      Select Case e.TelestaffProfileType
        Case TelestaffProfileType.Dispatch

          If e.Regular.TotalHours > 80 Then
            TEList.Add(New TimecardTimeException(e.EmployeeData, TelestaffExceptionType.exceptionWarning, "Too many Regular hours detected."))
          End If
          If e.Scheduled_Overtime.TotalHours > 4 Then
            TEList.Add(New TimecardTimeException(e.EmployeeData, TelestaffExceptionType.exceptionWarning, "Too many Scheduled Overtime hours detected."))
          End If

        Case TelestaffProfileType.Field

          If e.Regular.TotalHours > 106 Then
            TEList.Add(New TimecardTimeException(e.EmployeeData, TelestaffExceptionType.exceptionWarning, "Too many Regular hours detected."))
          End If

          If e.Scheduled_Overtime.TotalHours > 14 Then
            TEList.Add(New TimecardTimeException(e.EmployeeData, TelestaffExceptionType.exceptionWarning, "Too many Scheduled Overtime hours detected."))
          End If

          ' Here we're going to check to make sure that there aren't any swap errors.
          ' A swap error is one where someone does a swap but it's for a shift that isn't theirs.
          If (From tt In e.Timelist Where tt.ProfileType = TelestaffProfileType.Field And tt.WorkTypeAbrv = "Straight"
              Select tt.ShiftType).Count > 0 AndAlso (From tt In e.Timelist Where tt.ProfileType = TelestaffProfileType.Field _
                            And tt.WorkTypeAbrv = "SWAP" Select tt.ShiftType).Count > 0 Then
            Dim WorkTypes() As String = {"Straight", "H", "S", "V"}
            Dim regularshift As String = (From tt In e.Timelist Where tt.ProfileType = TelestaffProfileType.Field And
                                WorkTypes.Contains(tt.WorkTypeAbrv) Order By tt.WorkDate Descending Select tt.ShiftType).First
            If (From t In e.Timelist Where t.ProfileType = TelestaffProfileType.Field And
                            t.WorkTypeAbrv = "SWAP" And t.ShiftType <> regularshift Select t).Count > 0 Then
              TEList.Add(New TimecardTimeException(e.EmployeeData, TelestaffExceptionType.exceptionWarning, "Swap was not done on regular shift."))
            End If
          End If


        Case TelestaffProfileType.Office

          If Unscheduled_Overtime.TotalHours_Week1 + Scheduled_Overtime.TotalHours_Week1 > 0 And
           Total_Hours_Week1 < HoursNeededForOvertimeByWeek Then
            TEList.Add(New TimecardTimeException(e.EmployeeData, TelestaffExceptionType.exceptionWarning, "Too many overtime hours for hours worked in Week 1."))
          End If

          If Unscheduled_Overtime.TotalHours_Week2 + Scheduled_Overtime.TotalHours_Week2 > 0 And
           Total_Hours_Week2 < HoursNeededForOvertimeByWeek Then
            TEList.Add(New TimecardTimeException(e.EmployeeData, TelestaffExceptionType.exceptionWarning, "Too many overtime hours for hours worked in Week 2."))
          End If

          If Not e.IsExempt And (e.Regular.TotalHours + Vacation.TotalHours + Sick.TotalHours > 80) Then
            TEList.Add(New TimecardTimeException(e.EmployeeData, TelestaffExceptionType.exceptionWarning, "Too many Regular hours detected."))
          End If
          If e.Scheduled_Overtime.TotalHours > 0 Then
            TEList.Add(New TimecardTimeException(e.EmployeeData, TelestaffExceptionType.exceptionWarning, "Too many Scheduled Overtime hours detected."))
          End If
          ' Let's check to see if anyone has any worked time on a holiday
          If Not e.IsExempt Then
            ' We're going to look for any holiday rows.  IF they have them, then we'll also look
            ' to see if they have any regular work rows.
            Dim holidayFound = (From t In e.Timelist Where t.WorkTypeAbrv = "H" Select t.WorkDate Distinct)
            If holidayFound.Count > 0 Then
              For Each h In holidayFound
                If (From t In e.Timelist Where t.WorkDate = h And t.WorkTypeAbrv <> "H" Select t).Count > 0 Then
                  TEList.Add(New TimecardTimeException(e.EmployeeData, TelestaffExceptionType.exceptionWarning, "Employee has more than just Holiday hours on a holiday."))
                  Exit For
                End If

              Next
            End If

            If e.Unscheduled_Double_Overtime.TotalHours > 0 Then
              ' They should only be able to put in double time on a holiday or Sunday
              Dim tl = (From t In e.Timelist Where t.WorkCode = "232").ToList
              Dim hl As List(Of Date) = getHolidayList(e.PayPeriodStart.Year, False)
              For Each t In tl
                If t.WorkDate.DayOfWeek <> DayOfWeek.Sunday Then
                  If Not hl.Contains(t.WorkDate) Then
                    TEList.Add(New TimecardTimeException(e.EmployeeData, TelestaffExceptionType.exceptionWarning, "Employee double time hours but the time is not on a Sunday or a Holiday."))
                    Exit For
                  End If
                End If
              Next

            End If

          Else
            'Dim adminFound = (From t In e.Timelist Where t.WorkTypeAbrv = "ADM")
            Dim adminCheck = (From t In e.Timelist Where t.WorkTypeAbrv = "ADM"
                              Group By workDate = t.WorkDate Into adminGroup = Group,
                                              totalAdmin = Sum(t.WorkHours)
                              Select New With {workDate, totalAdmin})
            Dim overAdmin = (From a In adminCheck Where a.totalAdmin > 7 Select a.workDate)
            If overAdmin.Count > 0 Then
              For Each oa In overAdmin
                TEList.Add(New TimecardTimeException(e.EmployeeData, TelestaffExceptionType.exceptionWarning, "Too many Admin Hours used on " & oa.ToShortDateString & "."))
              Next
            End If
          End If

      End Select

      ' Let's start off by looking for overlapping times.
      For i As Integer = 0 To e.RawTimeList.Count - 1
        Dim startTime As Date = e.RawTimeList(i).StartTime
        Dim endTime As Date = e.RawTimeList(i).EndTime
        Dim bFound As Boolean = False
        For j As Integer = 0 To e.RawTimeList.Count - 1
          If i <> j Then
            If (e.RawTimeList(j).StartTime > startTime And e.RawTimeList(j).StartTime < endTime) Or
                (e.RawTimeList(j).EndTime > startTime And e.RawTimeList(j).EndTime < endTime) Or
                (e.RawTimeList(j).StartTime = startTime And e.RawTimeList(j).EndTime = endTime) Then
              TEList.Add(New TimecardTimeException(e.EmployeeData, TelestaffExceptionType.exceptionError, "Overlapping Shifts detected on " & e.RawTimeList(i).WorkDate.ToShortDateString & "."))
              'bFound = True
              'Exit For
            End If
          End If
        Next
        'If bFound Then
        '    TEList.Add(New TimecardTimeException(e.EmployeeData, TelestaffExceptionType.exceptionError, "Overlapping Shifts detected."))
        '    Exit For
        'End If
      Next

      ' let's check total hours, make sure we don't have any days with more than 24 hours.
      Dim check = (From t In e.Timelist
                   Group By workDate = t.WorkDate Into adminGroup = Group,
                     totalHours = Sum(t.WorkHours)
                   Select New With {workDate, totalHours})
      Dim finalCheck = (From c In check Where c.totalHours > 24 Select c)
      If finalCheck.Count > 0 Then
        For Each f In finalCheck
          TEList.Add(New TimecardTimeException(e.EmployeeData, TelestaffExceptionType.exceptionError, "Too many hours entered on " & f.workDate.ToShortDateString & "."))
        Next
      End If


      If e.Payrates.Count > 1 Then
        TEList.Add(New TimecardTimeException(e.EmployeeData, TelestaffExceptionType.exceptionWarning, "Multiple Payrates for this employee in this pay period."))
      End If

      If e.Vacation_Requested > e.EmployeeData.Banked_Vacation_Hours Then
        TEList.Add(New TimecardTimeException(e.EmployeeData, TelestaffExceptionType.exceptionError,
                                e.Vacation_Requested & " Vacation hours requested, only " & e.EmployeeData.Banked_Vacation_Hours &
                                " vacation hours banked."))
      End If
      If e.Sick_Requested > e.EmployeeData.Banked_Sick_Hours Then
        TEList.Add(New TimecardTimeException(e.EmployeeData, TelestaffExceptionType.exceptionError,
                                e.Sick_Requested & " Sick hours requested, only " & e.EmployeeData.Banked_Sick_Hours &
                                " sick hours banked."))
      End If
      If e.Holiday_Requested > e.Banked_Holiday_Hours Then
        TEList.Add(New TimecardTimeException(e.EmployeeData, TelestaffExceptionType.exceptionError,
                                e.Holiday_Requested & " Holiday hours requested, only " & e.Banked_Holiday_Hours &
                                " holiday hours banked."))
      End If
      If e.Leave_Without_Pay.TotalHours > 0 Then
        TEList.Add(New TimecardTimeException(e.EmployeeData, TelestaffExceptionType.exceptionWarning,
                                        "Employee has hours in Leave Without Pay, please verify."))
      End If

      Return TEList
    End Function

    Private Function Get_Telestaff_Exceptions(e As EPP, t As TelestaffTimeData) As List(Of TimecardTimeException)
      Dim disasterWorkPayCodes As New List(Of String) From {
        "299",
        "301",
        "302",
        "303"
      }
      Dim TEList As New List(Of TimecardTimeException)
      If t.Staffing_Detail.Length = 0 And disasterWorkPayCodes.Contains(t.WorkCode) Then
        TEList.Add(New TimecardTimeException(e.EmployeeData, TelestaffExceptionType.exceptionError, "Disaster Detail code is missing for hours worked on: " & t.WorkDate.ToShortDateString & "."))
      End If
      If t.PayRate = 0 Then
        TEList.Add(New TimecardTimeException(e.EmployeeData, TelestaffExceptionType.exceptionError, "Payrate not set for this job."))
      End If
      If t.EndTime.Subtract(t.StartTime).TotalHours <> t.WorkHours Then
        TEList.Add(New TimecardTimeException(e.EmployeeData, TelestaffExceptionType.exceptionError, "Longer shift duration than start time and end time support."))
      End If
      If t.IsPaidTime Then
        Select Case t.ProfileType
          Case TelestaffProfileType.Field
            If t.WorkType.ToUpper.Contains("STAFF") Or t.WorkType.ToUpper.Contains("DISPATCH") Then
              ' Probably needs to be changed.
              TEList.Add(New TimecardTimeException(e.EmployeeData, TelestaffExceptionType.exceptionWarning, "Check Work Type for this job."))
            End If
          Case TelestaffProfileType.Dispatch
            If t.WorkType.ToUpper.Contains("STAFF") Then
              TEList.Add(New TimecardTimeException(e.EmployeeData, TelestaffExceptionType.exceptionWarning, "Check Work Type for this job."))
            End If
          Case Else
            If t.WorkType.ToUpper.Contains("DISPATCH") Then
              TEList.Add(New TimecardTimeException(e.EmployeeData, TelestaffExceptionType.exceptionWarning, "Check Work Type for this job."))
            End If
            If Not e.IsExempt And t.WorkHours > 7 Then
              Dim brlist As List(Of TelestaffTimeData) = (From br In e.Timelist Where br.WorkDate = t.WorkDate And br.WorkTypeAbrv.ToUpper = "BR" Select br).ToList
              If brlist.Count = 0 Then
                TEList.Add(New TimecardTimeException(e.EmployeeData, TelestaffExceptionType.exceptionWarning, "Check that a break was entered."))
              End If
            End If
        End Select
        If t.WorkHours Mod 0.25 > 0 Then
          TEList.Add(New TimecardTimeException(e.EmployeeData, TelestaffExceptionType.exceptionError, "Hours need to be adjusted, round up or down to the nearest quarter hour."))
        End If
      End If
      Select Case t.WorkTypeAbrv
        Case "SLWP"
          TEList.Add(New TimecardTimeException(e.EmployeeData, TelestaffExceptionType.exceptionWarning, "Sick Leave Without Pay, the sick leave bank will need to be manually updated."))
        'Case "ST12", "ST10", "SU10", "SU12", "OT10",
        '     "OT12", "OTLC10", "OTLC12", "OTLR10",
        '     "OTLR12", "SUO", "SUE", "SUBC", "STO",
        '     "STE", "STBC", "OTSUE", "OTSUO", "OTSUO",
        '     "OTSUBC", "OTMSUO", "OTMSUBC", "OTMSUE",
        '     "OTLCSUO", "OTLCSUBC", "OTLCSUE", "OTLRSUO",
        '     "OLTRSUBC", "OTLRSUE", "OTSUED", "OTSUOD"

        Case "OTLR"
          If t.WorkHours > 3 Then
            TEList.Add(New TimecardTimeException(e.EmployeeData, TelestaffExceptionType.exceptionWarning, "OLTR longer than 3 hours, please verify that this is correct."))
          End If
      End Select
      If Is_Stepup(t.WorkTypeAbrv) Then
        TEList.Add(New TimecardTimeException(e.EmployeeData, TelestaffExceptionType.exceptionWarning, "Step up pay, double check that this employee's rate was correct."))
        If t.Specialties <> t.ProfileSpecialties Then
          TEList.Add(New TimecardTimeException(e.EmployeeData, TelestaffExceptionType.exceptionError, "Telestaff has stored the wrong specialties on " & t.WorkDate.ToShortDateString + ". This will need to be corrected in order to calculate the step up pay correctly."))
        End If
      Else
        If t.PayRate <> e.EmployeeData.Base_Payrate Then
          TEList.Add(New TimecardTimeException(e.EmployeeData, TelestaffExceptionType.exceptionWarning, "Employee's payrate in Telestaff different than Finplus rate."))
        End If
      End If

      If t.RequiresApproval Then
        TEList.Add(New TimecardTimeException(e.EmployeeData, TelestaffExceptionType.exceptionError, "Exception Hours require approval."))
      End If
      If t.WorkCode = "007" Then
        TEList.Add(New TimecardTimeException(e.EmployeeData, TelestaffExceptionType.exceptionWarning, "Employee has hours in Union Time Pool (007)."))
      End If
      Return TEList
    End Function


  End Class
End Namespace