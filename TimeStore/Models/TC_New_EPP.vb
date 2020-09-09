Namespace Models


  Public Class TC_New_EPP

    Public PayPeriodStart As Date = GetPayPeriodStart(Today)
    Public EmployeeData As FinanceData
    Public TL As New List(Of TimecardTimeData)
    Public BaseTS As New List(Of TimeSegment)
    Public CompTimeEarnedWeek1 As Double = 0
    Public CompTimeEarnedWeek2 As Double = 0
    Public WarningList As New List(Of String)
    Public ErrorList As New List(Of String)
    Public DisasterPayRules As List(Of DisasterEventRules) = New List(Of DisasterEventRules)
    Private _DoesOutOfClassApply As Boolean? = Nothing
    Private _DisasterWorkDates As List(Of Date) = Nothing

    Public tsRegular As New List(Of TimeSegment)
    Public Regular As New Dictionary(Of Integer, Double)
    Public tsRegularOvertime As New List(Of TimeSegment)
    Public RegularOvertime As New Dictionary(Of Integer, Double)
    Public tsOvertime As New List(Of TimeSegment)
    Public Overtime As New Dictionary(Of Integer, Double)
    Public tsDoubletime As New List(Of TimeSegment)
    Public Doubletime As New Dictionary(Of Integer, Double)
    Public CompTimeEarnedAndApplied As New Dictionary(Of Integer, Double)
    Public tsCompTimeEarnedAndApplied As New List(Of TimeSegment)

    Public tsDisasterRegular As New List(Of TimeSegment)
    Public DisasterRegular As New Dictionary(Of Integer, Double)
    Public tsDisasterStraight As New List(Of TimeSegment)
    Public DisasterStraight As New Dictionary(Of Integer, Double)
    Public tsDisasterOvertime As New List(Of TimeSegment)
    Public DisasterOvertime As New Dictionary(Of Integer, Double)
    Public tsDisasterDoubletime As New List(Of TimeSegment)
    Public DisasterDoubletime As New Dictionary(Of Integer, Double)

    Private _Total_Hours As Dictionary(Of Integer, Double) = Nothing
    Private _Week_TL As Dictionary(Of Integer, List(Of TimecardTimeData)) = Nothing
    Private _HoursForOTByWeek As Double = -1

    Private Function FilterTS(segments As List(Of TimeSegment), week As Integer, disaster_period As Integer) As List(Of TimeSegment)
      If week = 0 And disaster_period = 0 Then Return segments
      Return (From s In segments
              Where (s.week = week Or week = 0) And
                   (s.disaster_period_id = disaster_period Or disaster_period = 0)
              Select s).ToList
    End Function

    Public ReadOnly Property DisasterRegularByDisaster(week As Integer, disaster_period_id As Integer) As Double
      Get
        Return (From t In FilterTS(tsDisasterRegular, week, disaster_period_id) Select t.total_hours).Sum
      End Get
    End Property

    Public ReadOnly Property DisasterStraightByDisaster(week As Integer, disaster_period_id As Integer) As Double
      Get
        Return (From t In FilterTS(tsDisasterStraight, week, disaster_period_id) Select t.total_hours).Sum
      End Get
    End Property

    Public ReadOnly Property DisasterOvertimeByDisaster(week As Integer, disaster_period_id As Integer) As Double
      Get
        Return (From t In FilterTS(tsDisasterOvertime, week, disaster_period_id) Select t.total_hours).Sum
      End Get
    End Property

    Public ReadOnly Property DisasterDoubletimeByDisaster(week As Integer, disaster_period_id As Integer) As Double
      Get
        Return (From t In FilterTS(tsDisasterDoubletime, week, disaster_period_id) Select t.total_hours).Sum
      End Get
    End Property


    Private ReadOnly Property DoesOutOfClassApply As Boolean
      Get
        If Not _DoesOutOfClassApply.HasValue Then
          _DoesOutOfClassApply = False
          Dim PubWorksDepartments() As String = {"3701", "3711", "3712"}
          If PubWorksDepartments.Contains(EmployeeData.Department) Then
            Dim out_of_class_count = (From t In TL
                                      Where t.OutOfClass
                                      Select t).Count
            _DoesOutOfClassApply = out_of_class_count > 0
          End If

        End If
        Return _DoesOutOfClassApply.Value
      End Get
    End Property

    Public Sub New(ByRef TCTD As List(Of TimecardTimeData),
               F As FinanceData,
               pps As Date,
               Optional CTE As Saved_TimeStore_Comp_Time_Earned = Nothing)
      EmployeeData = F
      TL = TCTD
      PayPeriodStart = pps
      DisasterPayRules = DisasterEventRules.Get_Cached_Disaster_Rules(pps.AddDays(13))
      Process_Times()
      'PopulateDisasterWorkDates() ' Removed on 8/21/2020
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
      DisasterPayRules = DisasterEventRules.Get_Cached_Disaster_Rules(pps.AddDays(13))
      Process_Times()
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

    Private Sub Init_Values()
      For i = 0 To 2 Step 1
        Regular(i) = 0
        RegularOvertime(i) = 0
        Overtime(i) = 0
        Doubletime(i) = 0
        DisasterRegular(i) = 0
        DisasterStraight(i) = 0
        DisasterOvertime(i) = 0
        DisasterDoubletime(i) = 0
      Next
    End Sub

    Private Sub Finalize_Values()
      For i = 1 To 2 Step 1
        Regular(0) += Regular(i)
        RegularOvertime(0) += RegularOvertime(i)
        Overtime(0) += Overtime(i)
        Doubletime(0) += Doubletime(i)
        DisasterRegular(0) += DisasterRegular(i)
        DisasterStraight(0) += DisasterStraight(i)
        DisasterOvertime(0) += DisasterOvertime(i)
        DisasterDoubletime(0) += DisasterDoubletime(i)
      Next
      Test_Values()
    End Sub

    Private Sub Test_Values()
      If Not EmployeeData.IsExempt Then
        If Regular(0) <> (From t In tsRegular Select t.total_hours).Sum Then
          Dim el As New ErrorLog("regular doesn't match", EmployeeData.EmployeeId, "", "", "")
        End If
      End If

      If DisasterRegular(0) <> (From t In tsDisasterRegular Select t.total_hours).Sum Then
        Dim el As New ErrorLog("disasterregular doesn't match", EmployeeData.EmployeeId, "", "", "")
      End If
      If RegularOvertime(0) <> (From t In tsRegularOvertime Select t.total_hours).Sum Then
        Dim el As New ErrorLog("regular overtime doesn't match", EmployeeData.EmployeeId, "", "", "")
      End If
      If DisasterStraight(0) <> (From t In tsDisasterStraight Select t.total_hours).Sum Then
        Dim el As New ErrorLog("disaster straight doesn't match", EmployeeData.EmployeeId, "", "", "")
      End If
      If Overtime(0) <> (From t In tsOvertime Select t.total_hours).Sum Then
        Dim el As New ErrorLog("overtime doesn't match", EmployeeData.EmployeeId, "", "", "")
      End If
      If DisasterOvertime(0) <> (From t In tsDisasterOvertime Select t.total_hours).Sum Then
        Dim el As New ErrorLog("disaster overtime doesn't match", EmployeeData.EmployeeId, "", "", "")
      End If
      If Doubletime(0) <> (From t In tsDoubletime Select t.total_hours).Sum Then
        Dim el As New ErrorLog("doubletime doesn't match", EmployeeData.EmployeeId, "", "", "")
      End If
      If DisasterDoubletime(0) <> (From t In tsDisasterDoubletime Select t.total_hours).Sum Then
        Dim el As New ErrorLog("disaster doubletime doesn't match", EmployeeData.EmployeeId, "", "", "")
      End If
    End Sub

    Public Sub Process_Times()
      Init_Values()
      Dim segments As New List(Of TimeSegment)
      For Each t In TL
        Dim payrate As Double = EmployeeData.Base_Payrate

        payrate = IIf(DoesOutOfClassApply AndAlso t.OutOfClass, payrate * 1.05, payrate)

        Dim tslist = TimeSegment.Create(t, PayPeriodStart, payrate)

        'Dim bccount = (From ts In tslist Where ts.work_type = WorkType.BreakCredit Select ts).Count

        BaseTS.AddRange(tslist)
      Next

      ' check
      'For i As Integer = 1 To 2
      '  Dim week As Integer = i
      '  Dim work_hours As Double = (From t In Week_TL(week)
      '                              Select t.WorkHours).Sum

      '  Dim break_credits As Double = (From t In Week_TL(week)
      '                                 Select t.BreakCreditHours).Sum

      '  Dim ts_work_hours As Double = (From t In BaseTS
      '                                 Where t.week = week
      '                                 Select t.total_hours).Sum

      '  Dim ts_break_credits As Double = (From t In BaseTS
      '                                    Where t.week = week And
      '                                      t.work_type = WorkType.BreakCredit
      '                                    Select t.total_hours).Sum

      '  Dim diff = work_hours = ts_work_hours

      'Next

      If EmployeeData.IsExempt Then
        Process_Times_Exempt()
      Else
        Process_Times_Non_Exempt()
      End If
      Finalize_Values()
    End Sub

    Public Sub Process_Times_Exempt()
      For i As Integer = 1 To 2
        Dim week As Integer = i

        Dim regular_ts = (From tts In BaseTS
                          Where tts.week = week And
                            tts.work_type <> WorkType.Disaster
                          Order By tts.start_time Ascending
                          Select tts).ToList

        ' regular first
        For Each rts In regular_ts
          Regular(rts.week) += rts.total_hours
          tsRegular.Add(rts)
        Next
        ' then disaster
        Dim disaster_ts = (From tts In BaseTS
                           Where tts.week = week And
                               tts.work_type = WorkType.Disaster
                           Order By tts.start_time Ascending
                           Select tts).ToList

        For Each dts In disaster_ts
          If dts.disaster_pay_rule = 0 Then
            DisasterRegular(dts.week) += dts.total_hours
            tsDisasterRegular.Add(dts)
          Else
            Handle_Disaster_Regular_Exempt(dts)
          End If

        Next
        If (EmployeeData.HireDate <= PayPeriodStart) Then
          If Total_Hours(week) > 0 Then
            Regular(week) = Math.Max(40 - (DisasterRegular(week) + Non_Working_Hours(week)), 0)
          Else
            Regular(week) = 0
          End If

        Else
          Dim weekstart As Date = IIf(week = 1, PayPeriodStart, PayPeriodStart.AddDays(7))
          Dim weekend As Date = weekstart.AddDays(6)
          If EmployeeData.HireDate >= weekstart AndAlso EmployeeData.HireDate <= weekend Then
            ' If they were hired in this week, then they will be paid based on their hours worked, up to 40 hours
            Regular(week) = Math.Max(Math.Min(Regular(week), 40), 0)
          Else
            Regular(week) = Math.Max(40 - (DisasterRegular(week) + Non_Working_Hours(week)), 0)
          End If

        End If


      Next


    End Sub

    Public Sub Process_Times_Non_Exempt()
      For i As Integer = 1 To 2
        Dim week As Integer = i

        Dim regular_ts = (From tts In BaseTS
                          Where tts.week = week And
                            tts.work_type <> WorkType.Disaster And
                            tts.disaster_pay_rule = -1
                          Order By tts.start_time Ascending
                          Select tts).ToList

        ' regular first
        For Each rts In regular_ts
          Select Case rts.work_type
            Case WorkType.DisasterDoubletime
              DisasterDoubletime(week) += rts.total_hours
              tsDisasterDoubletime.Add(rts)

            Case WorkType.Doubletime
              Doubletime(week) += rts.total_hours
              tsDoubletime.Add(rts)

            Case WorkType.BreakCredit
              Handle_Regular_Non_Exempt(rts)

            Case WorkType.Regular
              Handle_Regular_Non_Exempt(rts)

          End Select
        Next
        ' then disaster
        Dim disaster_ts = (From tts In BaseTS
                           Where tts.week = week And
                               (tts.work_type = WorkType.Disaster Or
                             (tts.work_type = WorkType.BreakCredit And
                             tts.disaster_pay_rule <> -1))
                           Order By tts.start_time Ascending
                           Select tts).ToList

        For Each dts In disaster_ts
          If dts.disaster_pay_rule = 0 Then
            Handle_Regular_Non_Exempt(dts)
          Else
            Handle_Disaster_Regular_Non_Exempt(dts)
          End If

        Next


      Next
    End Sub

    Private Sub Handle_Regular_Non_Exempt(ts As TimeSegment)
      Dim IsDisaster = ts.work_type = WorkType.Disaster Or (ts.work_type = WorkType.BreakCredit AndAlso ts.disaster_pay_rule <> -1)
      Dim RegularDiff = HoursForOTByWeek - (Regular(ts.week) + DisasterRegular(ts.week) + Non_Working_Hours(ts.week))
      If RegularDiff = 0 Then
        If Non_Working_Hours(ts.week) > 0 Then

          Dim RegularOTDiff = Non_Working_Hours(ts.week) - RegularOvertime(ts.week) - DisasterStraight(ts.week)
          If RegularOTDiff = 0 Then
            If IsDisaster Then
              tsDisasterOvertime.Add(ts)
              DisasterOvertime(ts.week) += ts.total_hours
            Else
              tsOvertime.Add(ts)
              Overtime(ts.week) += ts.total_hours
            End If


          Else
            If RegularOTDiff > 0 Then
              If RegularOTDiff >= ts.total_hours Then
                If IsDisaster Then
                  tsDisasterStraight.Add(ts)
                  DisasterStraight(ts.week) += ts.total_hours
                Else
                  tsRegularOvertime.Add(ts)
                  RegularOvertime(ts.week) += ts.total_hours
                End If

              Else
                Dim new_end = ts.start_time.AddHours(RegularOTDiff)
                Dim new_ts = ts.Clone(new_end, ts.end_time)
                ts.end_time = new_end
                ts.Update()
                If IsDisaster Then
                  tsDisasterStraight.Add(ts)
                  DisasterStraight(ts.week) += ts.total_hours
                Else
                  tsRegularOvertime.Add(ts)
                  RegularOvertime(ts.week) += ts.total_hours
                End If
                Handle_Regular_Non_Exempt(new_ts)
              End If
            Else
              ' we have a problem
              Dim el As New ErrorLog("RegularOTDiff calculation error", ts.work_date.ToShortDateString, RegularOTDiff.ToString, "", "")
            End If
          End If
        Else
          If IsDisaster Then
            tsDisasterOvertime.Add(ts)
            DisasterOvertime(ts.week) += ts.total_hours
          Else
            tsOvertime.Add(ts)
            Overtime(ts.week) += ts.total_hours
          End If
        End If

      Else
        If RegularDiff > 0 Then
          If RegularDiff >= ts.total_hours Then
            If IsDisaster Then
              tsDisasterRegular.Add(ts)
              DisasterRegular(ts.week) += ts.total_hours
            Else
              tsRegular.Add(ts)
              Regular(ts.week) += ts.total_hours
            End If

          Else
            ' diff is less than ts.total_hours, so we'll need to split the ts.
            ' the amount up to diff will go into regular, the remainder will be processed by a new handle_regular
            Dim new_end = ts.start_time.AddHours(RegularDiff)
            Dim new_ts = ts.Clone(new_end, ts.end_time)
            ts.end_time = new_end
            ts.Update()
            If IsDisaster Then
              tsDisasterRegular.Add(ts)
              DisasterRegular(ts.week) += ts.total_hours
            Else
              tsRegular.Add(ts)
              Regular(ts.week) += ts.total_hours
            End If

            Handle_Regular_Non_Exempt(new_ts)
          End If
        Else
          ' we have a problem
          Dim el As New ErrorLog("RegularDiff calculation error", ts.work_date.ToShortDateString, ts.start_time_raw, ts.end_time_raw, "")
        End If
      End If
    End Sub

    Private Sub Handle_Disaster_Regular_Non_Exempt(ts As TimeSegment)
      ' Here we will need to handle disaster pay rules and hours worked
      ' the disaster segments should already have the correct pay rules tied to each segment.
      Dim normallyscheduled As Double = GetNormallyScheduledHours(ts)
      If normallyscheduled > 0 Then
        Dim diff = normallyscheduled - GetHoursWorked(ts)
        If diff = 0 Then
          Handle_Disaster_Regular_Non_Exempt_By_Payrule(ts)
        Else
          If diff > 0 Then
            If diff >= ts.total_hours Then

              Handle_Regular_Non_Exempt(ts)

            Else

              Dim new_end = ts.start_time.AddHours(diff)
              Dim new_ts = ts.Clone(new_end, ts.end_time)
              ts.end_time = new_end
              ts.Update()
              Handle_Disaster_Regular_Non_Exempt_By_Payrule(new_ts)
              Handle_Regular_Non_Exempt(ts)

            End If

          Else
            Dim el As New ErrorLog("Disaster Calculation normally scheduled error", ts.work_date.ToShortDateString, ts.start_time_raw, ts.end_time_raw, "")
          End If
        End If
      Else
        Handle_Disaster_Regular_Non_Exempt_By_Payrule(ts)
      End If

    End Sub

    Private Sub Handle_Disaster_Regular_Exempt(ts As TimeSegment)
      ' Here we will need to handle disaster pay rules and hours worked
      ' the disaster segments should already have the correct pay rules tied to each segment.
      Dim normallyscheduled As Double = GetNormallyScheduledHours(ts)
      If normallyscheduled > 0 Then
        Dim diff = normallyscheduled - GetHoursWorked(ts)
        If diff = 0 Then
          Handle_Disaster_Regular_Exempt_By_Payrule(ts)
        Else
          If diff > 0 Then
            If diff >= ts.total_hours Then

              DisasterRegular(ts.week) += ts.total_hours
              tsDisasterRegular.Add(ts)

            Else

              Dim new_end = ts.start_time.AddHours(diff)
              Dim new_ts = ts.Clone(new_end, ts.end_time)
              ts.end_time = new_end
              ts.Update()
              Handle_Disaster_Regular_Exempt_By_Payrule(new_ts)
              DisasterRegular(ts.week) += ts.total_hours
              tsDisasterRegular.Add(ts)

            End If

          Else
            Dim el As New ErrorLog("Disaster Calculation normally scheduled error", ts.work_date.ToShortDateString, ts.start_time_raw, ts.end_time_raw, "")
          End If
        End If
      Else
        Handle_Disaster_Regular_Exempt_By_Payrule(ts)
      End If

    End Sub

    Private Sub Handle_Disaster_Regular_Exempt_By_Payrule(ts As TimeSegment)
      Select Case ts.disaster_pay_rule
        Case -1
          ' uh oh
          Dim el As New ErrorLog("Disaster Calculation pay rule missing error", ts.work_date.ToShortDateString, ts.start_time_raw, ts.end_time_raw, "")
        Case 0
          DisasterRegular(ts.week) += ts.total_hours
          tsDisasterRegular.Add(ts)

        Case 1, 2
          DisasterStraight(ts.week) += ts.total_hours
          tsDisasterStraight.Add(ts)

        Case Else
          Dim el As New ErrorLog("Disaster pay rule calculation error", ts.work_date.ToShortDateString, ts.start_time_raw, ts.end_time_raw, "")
      End Select
    End Sub

    Private Sub Handle_Disaster_Regular_Non_Exempt_By_Payrule(ts As TimeSegment)
      Select Case ts.disaster_pay_rule
        Case -1
          ' uh oh
          Dim el As New ErrorLog("Disaster Calculation pay rule missing error", ts.work_date.ToShortDateString, ts.start_time_raw, ts.end_time_raw, "")
        Case 0
          If ts.work_date.DayOfWeek = DayOfWeek.Sunday Then
            DisasterDoubletime(ts.week) += ts.total_hours
            tsDisasterDoubletime.Add(ts)
          Else
            Handle_Regular_Non_Exempt(ts)
          End If

        Case 1
          If ts.work_date.DayOfWeek = DayOfWeek.Sunday Then
            DisasterDoubletime(ts.week) += ts.total_hours
            tsDisasterDoubletime.Add(ts)
          Else
            DisasterOvertime(ts.week) += ts.total_hours
            tsDisasterOvertime.Add(ts)
          End If
        Case 2
          DisasterDoubletime(ts.week) += ts.total_hours
          tsDisasterDoubletime.Add(ts)

        Case Else
          Dim el As New ErrorLog("Disaster pay rule calculation error", ts.work_date.ToShortDateString, ts.start_time_raw, ts.end_time_raw, "")
      End Select
    End Sub

    Private Function GetNormallyScheduledHours(ts As TimeSegment) As Double
      Dim tctds = (From t In TL
                   Where t.WorkDate = ts.work_date
                   Select t)
      If tctds.Count > 0 Then
        Return tctds.First.DisasterNormalScheduledHours
      Else
        Return 0
      End If
    End Function

    Private Function GetHoursWorked(ts As TimeSegment) As Double
      Dim regular As Double = (From r In tsRegular
                               Where r.work_date = ts.work_date
                               Select r.total_hours).Sum
      Dim disaster As Double = (From r In tsDisasterRegular
                                Where r.work_date = ts.work_date
                                Select r.total_hours).Sum
      Return regular + disaster

    End Function

    Public Sub Catch_Exceptions()
      Try


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

        If DisasterPayRules.Count > 0 Then

          Dim badWorkDates As New List(Of Date)
          For Each ts In (From b In BaseTS
                          Where b.work_type = WorkType.Disaster
                          Order By b.start_time Ascending
                          Select b).ToList()

            If Not badWorkDates.Contains(ts.work_date) Then
              Dim period_start = (From d In DisasterPayRules
                                  Where d.period_id = ts.disaster_period_id
                                  Select d.StartDateTime).Min
              Dim period_end = (From d In DisasterPayRules
                                Where d.period_id = ts.disaster_period_id
                                Select d.EndDateTime).Max
              Dim period = (From d In DisasterPayRules
                            Where d.period_id = ts.disaster_period_id
                            Select d.period_name).First

              If ts.start_time < period_start OrElse ts.end_time > period_end Then


                Dim EventInfo As String = period & " is from " & period_start.ToLongDateString & " " & period_start.ToShortTimeString & " to " & period_end.ToLongDateString & " " & period_end.ToShortTimeString

                ErrorList.Add("Your special event hours entered on " & ts.work_date.ToShortDateString & " fall outside of the event start and end time. (" & EventInfo & ")")
                badWorkDates.Add(ts.work_date)
              End If
            End If

          Next
        End If

        For Each t In TL
          Dim totalWork = t.WorkHours + t.BreakCreditHours
          Dim totalDisasterWork = (From dwh In t.DisasterWorkHoursList
                                   Select dwh.DisasterWorkHours).Sum
          If totalDisasterWork > totalWork Then
            ErrorList.Add("The work hours entered on " & t.WorkDate.ToShortDateString & " are less than the special event work hours entered.  If the special event work hours are correct, then the work hours should be updated to reflect this.")
          End If
          'If t.AdminHours > 8 Then
          '    WarningList.Add("8 or more Admin leave hours entered on " & t.WorkDate.ToShortDateString & ".")
          'End If



          'For Each dwh In t.DisasterWorkHoursList
          '  Dim totaldisasterhours As Double = dwh.DisasterWorkHoursByRule(0) + dwh.DisasterWorkHoursByRule(1) + dwh.DisasterWorkHoursByRule(2)
          '  If dwh.DisasterWorkHours > totaldisasterhours Then
          '    Dim dpr As DisasterEventRules = (From d In DisasterPayRules
          '                                     Where d.period_id = dwh.DisasterPeriodId
          '                                     Select d).First()

          '  End If
          'Next




          If t.TotalHours > 11.5 And totalWork < 11 Then
            WarningList.Add("Your hours on " & t.WorkDate.ToShortDateString & " may need correcting.  If you are using leave, you have probably used too many leave hours.")
          End If

          If t.WorkDate = "9/2/2019" Or t.WorkDate = "9/3/2019" Or t.WorkDate = "9/4/2019" Then
            If t.BreakCreditHours > 0 Then
              WarningList.Add("No breaks should be entered on this date.")
            End If
          End If

          For Each dwh In t.DisasterWorkHoursList

            If dwh.DisasterWorkHours > 0 And dwh.DisasterWorkType.Trim.Length = 0 Then
              ErrorList.Add("You must select what type of work was performed for the special event hours on " & t.WorkDate.ToShortDateString & ".")
            End If

          Next

          'If t.DisasterWorkHours > 0 And t.DisasterWorkHoursByRule.ContainsKey(2) Then

          '  If t.DisasterWorkHoursByRule(2) > 0 And t.DisasterNormalScheduledHours > 0 Then
          '    ErrorList.Add("No one was scheduled to work on " & t.WorkDate.ToShortDateString & ".  The normally scheduled hours should be removed.")
          '  End If

          '  If t.DisasterWorkTimes.Length = 0 Then
          '    ErrorList.Add("The disaster work hours on " & t.WorkDate.ToShortDateString & " will need to be re-entered.")
          '  End If

          'End If
          If t.ScheduledLWOPHours > 0 And EmployeeData.isFulltime Then
            ErrorList.Add("Scheduled LWOP hours used on " & t.WorkDate.ToShortDateString & ".  These are to be used by part time employees only.")
          End If

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

        If EmployeeData.IsExempt Then
          Catch_Exempt_Exceptions()
        Else
          Catch_Non_Exempt_Exceptions()
        End If
      Catch ex As Exception
        Dim e As New ErrorLog(ex, "")
      End Try
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
            If Not EmployeeData.IsExempt AndAlso Total_Hours(1) < 40 Then
              ErrorList.Add("Employee does not have 40 hours recorded in Week 1.")
            End If
          End If
        End If
        If Now > PayPeriodStart.AddDays(11) Then
          If EmployeeData.HireDate >= PayPeriodStart.AddDays(7) AndAlso
            EmployeeData.HireDate <= PayPeriodStart.AddDays(13) Then
            ' They were hired this week, so we'll relax the hour requirements
          Else
            If Not EmployeeData.IsExempt AndAlso Total_Hours(2) < 40 Then
              ErrorList.Add("Employee does not have 40 hours recorded in Week 2.")
            End If
          End If
        End If
      End If

    End Sub

    Private Sub Catch_Exempt_Exceptions()
      Dim shiftTen() As String = {"1055"}
      Dim shiftMax As Double = 10 ' 10 hour shift  change from 8 hours
      If shiftTen.Contains(EmployeeData.Classify) Then shiftMax = 10
      For Each t In TL

        If t.WorkHours < 7.5 Then
          If t.Total_Non_Working_Hours > 0 Then
            If t.WorkHours + t.Total_Non_Working_Hours < 8 Then
              ErrorList.Add("Not enough hours entered on " & t.WorkDate.ToShortDateString & ". You must have 8 total hours or 10 total hours if you are using any leave hours, depending on the length of your work shift.")
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
                WarningList.Add("You have must either 4 work days of at least 9.5 hours worked, or 5 work days of at least 7.5 hours worked in Week 1.")
              End If
            End If
            If Now > PayPeriodStart.AddDays(12) Then
              If ((From t In Week_TL(2) Select t Where t.TotalHours >= 9.5).Count < 4) AndAlso
                  ((From t In Week_TL(2) Select t Where t.TotalHours >= 7.5).Count < 5) Then
                WarningList.Add("You have must either 4 work days of at least 9.5 hours worked, or 5 work days of at least 7.5 hours worked in Week 1.")
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

    Public ReadOnly Property Non_Working_Hours(Week As Integer) As Double
      Get
        Return Sick_All(Week) + Vacation(Week) + Comp_Time_Used(Week) +
              LWOP_All(Week) + SickLeavePool(Week) + Term_Hours(Week) +
              DisasterAdminLeave(Week, 0)
      End Get
    End Property

    Public ReadOnly Property HoursForOTByWeek As Double
      Get
        If _HoursForOTByWeek = -1 Then
          _HoursForOTByWeek = EmployeeData.HoursNeededForOvertime / 2
        End If
        Return _HoursForOTByWeek
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

    Public ReadOnly Property Total_Hours(Week As Integer) As Double
      Get
        If _Total_Hours Is Nothing Then
          _Total_Hours = New Dictionary(Of Integer, Double)
          _Total_Hours(1) = (From t In Week_TL(1) Select t.TotalHours).Sum
          _Total_Hours(2) = (From t In Week_TL(2) Select t.TotalHours).Sum
          _Total_Hours(0) = _Total_Hours(1) + _Total_Hours(2)
        End If
        Return _Total_Hours(Week)
      End Get
    End Property

    Private ReadOnly Property Week_TL(Week As Integer) As List(Of TimecardTimeData)
      Get
        If _Week_TL Is Nothing Then
          _Week_TL = New Dictionary(Of Integer, List(Of TimecardTimeData))
          _Week_TL(0) = TL
          _Week_TL(1) = (From t In TL
                         Where t.WorkDate >= PayPeriodStart And t.WorkDate < PayPeriodStart.AddDays(7)
                         Select t).ToList
          _Week_TL(2) = (From t In TL
                         Where t.WorkDate >= PayPeriodStart.AddDays(7) And t.WorkDate < PayPeriodStart.AddDays(14)
                         Select t).ToList
        End If
        Return _Week_TL(Week)
      End Get
    End Property

    Public ReadOnly Property Double_Time_Base(Week As Integer) As Double
      Get
        Return (From t In Week_TL(Week) Select t.DoubleTimeHours).Sum
      End Get
    End Property

    Public ReadOnly Property DisasterAdminLeave(Week As Integer, period_id As Integer) As Double
      Get
        Dim hours As Double = 0
        For Each t In Week_TL(Week)
          Dim dwh_list As List(Of DisasterWorkHours)
          If period_id = 0 Then
            dwh_list = t.DisasterWorkHoursList
          Else
            dwh_list = (From d In t.DisasterWorkHoursList
                        Where d.DisasterPeriodId = period_id
                        Select d).ToList()
          End If
          For Each dwh In dwh_list
            hours += dwh.DisasterAdminHours
          Next
        Next
        Return hours
        'Return (From t In Week_TL(Week)
        '      Select t.AdminDisaster).Sum
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

    Public ReadOnly Property BreakCredit(Week As Integer) As Double
      Get
        Return (From t In Week_TL(Week) Select t.BreakCreditHours).Sum
      End Get
    End Property

  End Class
End Namespace
