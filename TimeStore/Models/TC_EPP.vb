Namespace Models
  Public Class TC_EPP
    Public PayPeriodStart As Date = GetPayPeriodStart(Today)
    Public EmployeeData As FinanceData
    Public TL As List(Of TimecardTimeData)
    Public CompTimeEarnedWeek1 As Double = 0
    Public CompTimeEarnedWeek2 As Double = 0
    Public WarningList As New List(Of String)
    Public ErrorList As New List(Of String)
    Public DisasterPayRules As List(Of DisasterEventRules) = New List(Of DisasterEventRules)



    Private _DisasterWorkDates As List(Of Date) = Nothing
    Private _Total_Hours As Dictionary(Of Integer, Double) = Nothing
    Private _Week_TL As Dictionary(Of Integer, List(Of TimecardTimeData)) = Nothing
    Private _Regular As Dictionary(Of Integer, Double) = Nothing
    Private _Calculated_Regular As Dictionary(Of Integer, Double) = Nothing
    Private _DisasterRegular As Dictionary(Of Tuple(Of Integer, Integer), Double) = Nothing
    Private _DisasterHoursTotal As Dictionary(Of Integer, Double) = Nothing
    Private _DoesOutOfClassApply As Nullable(Of Boolean) = Nothing
    Private _Out_Of_Class_Regular As Dictionary(Of Integer, Double) = Nothing
    Private _Out_Of_Class_Disaster As Dictionary(Of Integer, Double) = Nothing

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

    Public ReadOnly Property DisasterWorkDates As List(Of Date)
      Get
        If _DisasterWorkDates Is Nothing Then
          If DisasterPayRules.Count = 0 Then
            _DisasterWorkDates = New List(Of Date)
          Else
            _DisasterWorkDates = (From e In EventsByWorkDate.Get_By_PayPeriod(PayPeriodStart)
                                  Select e.work_date).Distinct.ToList()
          End If
        End If
        Return _DisasterWorkDates
      End Get
    End Property

    'Private Sub PopulateDisasterWorkDates()
    '  If DisasterPayRules.Count = 0 Then Exit Sub

    '  ' Old version, replaced on 8/21/2020
    '  'For Each dpr In DisasterPayRules
    '  '  If dpr.pay_rule = 1 Or dpr.pay_rule = 2 Then
    '  '    Dim d As Date = dpr.StartDate.Date
    '  '    Do While d < dpr.EndDate.Date
    '  '      If Not DisasterWorkDates.Contains(d) And d >= PayPeriodStart Then DisasterWorkDates.Add(d.Date)
    '  '      d = d.AddDays(1)
    '  '    Loop
    '  '  End If
    '  'Next
    'End Sub

    Private Sub Calculate_Disaster_Hours_By_Rule()

      If DisasterPayRules.Count = 0 Then Exit Sub

      Try


        Dim workDate As String = ""
        Dim startTime As Date
        Dim endTime As Date
        Dim tsStartTime As Date
        Dim tsEndTime As Date

        For Each t In TL
          For Each dwh In t.DisasterWorkHoursList

            dwh.DisasterWorkHoursByRule(0) = 0
            dwh.DisasterWorkHoursByRule(1) = 0
            dwh.DisasterWorkHoursByRule(2) = 0
            dwh.DisasterWorkTimesByRule(0) = New List(Of TimeSpan)()
            dwh.DisasterWorkTimesByRule(1) = New List(Of TimeSpan)()
            dwh.DisasterWorkTimesByRule(2) = New List(Of TimeSpan)()
            't.DisasterWorkHoursByRule(0) = 0
            't.DisasterWorkHoursByRule(1) = 0
            't.DisasterWorkHoursByRule(2) = 0

            't.DisasterWorkTimesByRule(0) = New List(Of TimeSpan)()
            't.DisasterWorkTimesByRule(1) = New List(Of TimeSpan)()
            't.DisasterWorkTimesByRule(2) = New List(Of TimeSpan)()



            If dwh.DisasterWorkTimes.Length > 0 Then
              workDate = t.WorkDate.ToShortDateString

              Dim times As String()
              times = dwh.DisasterWorkTimes.Split("-")

              Dim max As Integer = times.GetUpperBound(0)

              If (max + 1) Mod 2 = 1 Then max -= 1 '

              For i As Integer = 0 To max Step 2
                Try
                  startTime = Date.Parse(workDate & " " & times(i).Trim)
                  endTime = Date.Parse(workDate & " " & times(i + 1).Trim)
                  If endTime.Second = 59 Then endTime = endTime.AddSeconds(1)

                  For Each dpr In (From d In DisasterPayRules
                                   Where d.period_id = dwh.DisasterPeriodId
                                   Select d)

                    If startTime < dpr.EndDateTime And dpr.StartDateTime < endTime Then
                      ' our timespans overlap, so we should do some calculations
                      tsStartTime = startTime
                      tsEndTime = endTime

                      If startTime < dpr.StartDateTime Then
                        If t.WorkDate = "8/29/2019" Then
                          'ErrorList.Add("Disaster hours entered on 8/29/2019 before the disaster was declared at 8:00 AM.")
                        Else
                          tsStartTime = dpr.StartDateTime
                        End If

                      End If

                      If endTime > dpr.EndDateTime Then tsEndTime = dpr.EndDateTime

                      Dim ts = tsEndTime.Subtract(tsStartTime)
                      dwh.DisasterWorkTimesByRule(dpr.pay_rule).Add(ts)
                    End If
                  Next
                Catch ex As Exception
                  Log(ex)
                End Try

              Next


            End If
            For i = 0 To 2 Step 1
              dwh.DisasterWorkHoursByRule(i) += (From ts In dwh.DisasterWorkTimesByRule(i)
                                                 Select ts.TotalHours).Sum

              dwh.DisasterHoursTotal += dwh.DisasterWorkHoursByRule(i)
              't.DisasterWorkHours += dwh.DisasterHoursTotal
            Next
            dwh.DisasterHoursRegular = dwh.DisasterWorkHoursByRule(0)
          Next

          Dim totaldisasterhours As Double = (From d In t.DisasterWorkHoursList
                                              Select d.DisasterHoursTotal).Sum
          If t.BreakCreditHours > 0 Then
            If t.TotalHours = t.BreakCreditHours + totaldisasterhours Then
              ' only in this situation does the break credit get applied to the disaster.
              ' we'll find the disaster with the largest disaster hours, and then 
              ' find the rule with the largest hours, and add .5 hours to it.
              Dim maxdisasterhours As Double = (From d In t.DisasterWorkHoursList
                                                Select d.DisasterHoursTotal).Max
              For Each dwh In t.DisasterWorkHoursList
                If dwh.DisasterHoursTotal = maxdisasterhours Then
                  If dwh.DisasterWorkHoursByRule(2) > dwh.DisasterWorkHoursByRule(1) AndAlso dwh.DisasterWorkHoursByRule(2) > dwh.DisasterWorkHoursByRule(0) Then
                    dwh.DisasterWorkHoursByRule(2) += 0.5
                  Else
                    If dwh.DisasterWorkHoursByRule(1) > dwh.DisasterWorkHoursByRule(2) AndAlso dwh.DisasterWorkHoursByRule(2) > dwh.DisasterWorkHoursByRule(0) Then
                      dwh.DisasterWorkHoursByRule(1) += 0.5

                    Else
                      dwh.DisasterWorkHoursByRule(0) += 0.5
                      dwh.DisasterHoursRegular += 0.5
                    End If
                  End If
                  dwh.DisasterHoursTotal += 0.5

                  Exit For
                End If
              Next
            End If
          End If

          ' Let's apply the pay rules here
          ' The pay rules are as follows: 
          ' These rules are applied at the day level.
          ' Non-Exempt
          '•	During partial activation for an emergency, non-exempt employees required to work the activation 
          '     will be paid one And one half times their regular rate of pay for time worked outside of their 
          '     normal scheduled work hours.
          '•	During full activation for an emergency when business operations are also suspended, non-exempt 
          '     employees required to work the activation will be paid two times their regular rate of pay for 
          '     time worked outside of their normal scheduled work hours.
          '•	During partial Or full activation for an emergency, for work performed on Sundays by those Not 
          '     regularly scheduled to work on Sundays, pay will be at two times their regular rate of pay for 
          '     time worked.

          ' Exempt
          '•	During partial activation for an emergency, exempt employees required to work the activation will 
          '     be paid their regular rate of pay for time worked outside of their normal number of scheduled 
          '     work hours.
          '•	During full activation for an emergency when business operations are also suspended, exempt 
          '     employees required to work the activation will be paid at their regular rate of pay for 
          '     additional hours worked.

          If EmployeeData.IsExempt Then

            If t.DisasterNormalScheduledHours > 0 Then

              If t.TotalHours > t.DisasterNormalScheduledHours Then
                Dim diff = t.TotalHours - t.DisasterNormalScheduledHours
                For Each dwh_calc In t.DisasterWorkHoursList

                  If diff > 0 AndAlso dwh_calc.DisasterWorkHoursByRule(1) + dwh_calc.DisasterWorkHoursByRule(2) > 0 Then
                    If dwh_calc.DisasterWorkHoursByRule(2) > 0 Then
                      If diff > dwh_calc.DisasterWorkHoursByRule(2) Then

                        diff -= dwh_calc.DisasterWorkHoursByRule(2)
                        dwh_calc.DisasterHoursStraight += dwh_calc.DisasterWorkHoursByRule(2)

                      Else
                        dwh_calc.DisasterHoursStraight += diff
                        dwh_calc.DisasterHoursRegular = dwh_calc.DisasterWorkHoursByRule(2) - diff

                        diff = 0
                      End If
                    End If
                    If diff > 0 Then
                      If dwh_calc.DisasterWorkHoursByRule(1) > 0 Then
                        If diff > dwh_calc.DisasterWorkHoursByRule(1) Then
                          diff -= dwh_calc.DisasterWorkHoursByRule(1)
                          dwh_calc.DisasterHoursStraight = dwh_calc.DisasterWorkHoursByRule(1)
                        Else
                          dwh_calc.DisasterHoursStraight += diff
                          dwh_calc.DisasterHoursRegular = dwh_calc.DisasterWorkHoursByRule(1) - diff
                          diff = 0
                        End If
                      End If
                    Else
                      dwh_calc.DisasterHoursRegular += dwh_calc.DisasterWorkHoursByRule(1)
                    End If

                  Else
                    dwh_calc.DisasterHoursRegular = dwh_calc.DisasterWorkHoursByRule(0) +
                      dwh_calc.DisasterWorkHoursByRule(1) +
                      dwh_calc.DisasterWorkHoursByRule(2)
                  End If
                Next
              Else
                For Each dwh_calc In t.DisasterWorkHoursList

                  dwh_calc.DisasterHoursRegular = dwh_calc.DisasterWorkHoursByRule(0) +
                    dwh_calc.DisasterWorkHoursByRule(1) +
                    dwh_calc.DisasterWorkHoursByRule(2)

                Next
              End If

            Else
              For Each dwh_calc In t.DisasterWorkHoursList
                dwh_calc.DisasterHoursStraight += dwh_calc.DisasterWorkHoursByRule(1) +
                  dwh_calc.DisasterWorkHoursByRule(2)
              Next
            End If

          Else
            ' Non-Exempt

            If t.WorkDate.DayOfWeek = DayOfWeek.Sunday Then

              If t.DisasterNormalScheduledHours > 0 Then

                If t.TotalHours > t.DisasterNormalScheduledHours Then

                  Dim diff = t.TotalHours - t.DisasterNormalScheduledHours
                  For Each dwh_calc In t.DisasterWorkHoursList

                    If diff > 0 AndAlso dwh_calc.DisasterWorkHoursByRule(1) + dwh_calc.DisasterWorkHoursByRule(2) > 0 Then
                      If dwh_calc.DisasterWorkHoursByRule(2) > 0 Then
                        If diff > dwh_calc.DisasterWorkHoursByRule(2) Then

                          diff -= dwh_calc.DisasterWorkHoursByRule(2)
                          dwh_calc.DisasterHoursDoubletime += dwh_calc.DisasterWorkHoursByRule(2)

                        Else
                          dwh_calc.DisasterHoursDoubletime += diff
                          dwh_calc.DisasterHoursRegular += (dwh_calc.DisasterWorkHoursByRule(2) - diff)
                          diff = 0

                        End If
                      End If
                      If diff > 0 Then
                        If dwh_calc.DisasterWorkHoursByRule(1) > 0 Then
                          If diff > dwh_calc.DisasterWorkHoursByRule(1) Then

                            diff -= dwh_calc.DisasterWorkHoursByRule(1)
                            dwh_calc.DisasterHoursDoubletime = dwh_calc.DisasterWorkHoursByRule(1)

                          Else

                            dwh_calc.DisasterHoursDoubletime += diff
                            dwh_calc.DisasterHoursRegular += (dwh_calc.DisasterWorkHoursByRule(1) - diff)
                            diff = 0

                          End If
                        End If
                      Else
                        dwh_calc.DisasterHoursRegular += dwh_calc.DisasterWorkHoursByRule(1)
                      End If

                    Else
                      dwh_calc.DisasterHoursRegular = dwh_calc.DisasterWorkHoursByRule(0) +
                        dwh_calc.DisasterWorkHoursByRule(1) +
                        dwh_calc.DisasterWorkHoursByRule(2)
                    End If

                  Next

                Else

                  For Each dwh_calc In t.DisasterWorkHoursList
                    dwh_calc.DisasterHoursRegular = dwh_calc.DisasterWorkHoursByRule(0) +
                      dwh_calc.DisasterWorkHoursByRule(1) +
                      dwh_calc.DisasterWorkHoursByRule(2)
                  Next

                End If

              Else
                For Each dwh_calc In t.DisasterWorkHoursList

                  dwh_calc.DisasterHoursDoubletime = dwh_calc.DisasterWorkHoursByRule(1) +
                    dwh_calc.DisasterWorkHoursByRule(2)

                Next
              End If

            Else
              ' It's not a sunday, so hours in payrule 1 can't be doubletime.

              If t.DisasterNormalScheduledHours > 0 Then

                If t.TotalHours > t.DisasterNormalScheduledHours Then

                  Dim diff = t.TotalHours - t.DisasterNormalScheduledHours
                  For Each dwh_calc In t.DisasterWorkHoursList

                    If diff > 0 AndAlso dwh_calc.DisasterWorkHoursByRule(1) + dwh_calc.DisasterWorkHoursByRule(2) > 0 Then

                      If dwh_calc.DisasterWorkHoursByRule(2) > 0 Then
                        If diff > dwh_calc.DisasterWorkHoursByRule(2) Then

                          diff -= dwh_calc.DisasterWorkHoursByRule(2)
                          dwh_calc.DisasterHoursDoubletime = dwh_calc.DisasterWorkHoursByRule(2)

                        Else
                          dwh_calc.DisasterHoursDoubletime = diff
                          dwh_calc.DisasterHoursRegular += (dwh_calc.DisasterWorkHoursByRule(2) - diff)
                          diff = 0
                        End If
                      End If
                      If diff > 0 Then
                        If dwh_calc.DisasterWorkHoursByRule(1) > 0 Then
                          If diff > dwh_calc.DisasterWorkHoursByRule(1) Then

                            diff -= dwh_calc.DisasterWorkHoursByRule(1)
                            dwh_calc.DisasterHoursOvertime = dwh_calc.DisasterWorkHoursByRule(1)

                          Else

                            dwh_calc.DisasterHoursOvertime = diff
                            dwh_calc.DisasterHoursRegular += (dwh_calc.DisasterWorkHoursByRule(1) - diff)
                            diff = 0

                          End If
                        End If
                      Else
                        dwh_calc.DisasterHoursRegular += dwh_calc.DisasterWorkHoursByRule(1)
                      End If


                    Else
                      dwh_calc.DisasterHoursRegular = dwh_calc.DisasterWorkHoursByRule(0) +
                      dwh_calc.DisasterWorkHoursByRule(1) +
                      dwh_calc.DisasterWorkHoursByRule(2)
                    End If
                  Next

                Else

                  For Each dwh_calc In t.DisasterWorkHoursList
                    dwh_calc.DisasterHoursRegular = dwh_calc.DisasterWorkHoursByRule(0) +
                      dwh_calc.DisasterWorkHoursByRule(1) +
                      dwh_calc.DisasterWorkHoursByRule(2)
                  Next

                End If

              Else
                For Each dwh_calc In t.DisasterWorkHoursList

                  dwh_calc.DisasterHoursOvertime = dwh_calc.DisasterWorkHoursByRule(1)
                  dwh_calc.DisasterHoursDoubletime = dwh_calc.DisasterWorkHoursByRule(2)

                Next
              End If

            End If

          End If

        Next

      Catch ex As Exception
        Log(ex)
      End Try

    End Sub

    'Private Sub Handle_Break_Credit_For_Disasters(ByRef TCTD As TimecardTimeData)
    '  ' The rules for break credits and disasters are that if there are any
    '  ' non-disaster work hours, the break credit is not eligible to be considered as disaster work hours
    '  ' What we'll do is find the gap that has to be present in order to be eligible for a break credit.
    '  ' There can be multiple gaps, so we'll just find the first one.
    '  ' Then we'll insert 30 minutes of disaster work into the  disaster hours object
    '  ' that has the latest end time.
    '  If TCTD.BreakCreditHours = 0 Then Exit Sub

    '  Dim TotalDisasterHours As Double = (From dwh In TCTD.DisasterWorkHoursList
    '                                      Select dwh.DisasterHoursTotal).Sum


    '  If TotalDisasterHours + TCTD.BreakCreditHours = TCTD.TotalHours Then
    '    ' We'll put it in whichever disaster work hours has the latest work time end
    '    ' we can tell this by just taking dwh.DisasterWorkTtimes, split it on "-", and taking the last 
    '    ' element in the array
    '    Dim WorkDate As Date = TCTD.WorkDate
    '    Dim maxTime As Date = Nothing
    '    Dim maxTimeValue As String = ""
    '    For Each dwh In TCTD.DisasterWorkHoursList
    '      If dwh.DisasterWorkTimes.Length > 0 Then
    '        Dim dTmp As Date
    '        Dim times = dwh.DisasterWorkTimes.Split("-")
    '        dTmp = DateTime.Parse(WorkDate & " " & times(times.Length))
    '        If maxTime = Nothing OrElse dTmp > maxTime Then
    '          maxTime = dTmp
    '          maxTimeValue = times(times.Length)
    '        End If
    '      End If
    '    Next
    '    ' now maxTime / maxTimeValue are equal to the latest disaster time for this date.
    '    For Each dwh In TCTD.DisasterWorkHoursList
    '      If dwh.DisasterWorkTimes.Length > 0 Then
    '        Dim times = dwh.DisasterWorkTimes.Split("-")
    '        If times(times.Length) = maxTimeValue Then
    '          ' we now know which disasterworkhours to put it into
    '          ' but now the question is, which disasterWorkHoursByRule do we add it to?
    '          ' 

    '          Exit Sub
    '        End If
    '      End If
    '    Next
    '  Else
    '    Exit Sub
    '  End If

    'End Sub

    Private Sub Calculate_Disaster_Hours_By_Hours_Worked()
      ' This function is going to calculate if hours should be moved from
      ' regular to overtime based on total hours worked for a week
      If EmployeeData.IsExempt OrElse DisasterPayRules.Count() = 0 Then Exit Sub
      ' if total hours worked - disaster overtime by payrule and disaster double time > hours needed for overtime
      For Week As Integer = 1 To 2
        If DisasterRegular(Week, 0) > 0 Then


          Dim TotalRegular = Regular(Week)
          Dim TotalDisasterRegular = DisasterRegular(Week, 0)
          Dim NonWorking = Sick_All(Week) +
            Vacation(Week) +
            Comp_Time_Used(Week) +
                LWOP_All(Week) +
                SickLeavePool(Week) +
                Term_Hours(Week) +
                DisasterAdminLeave(Week, 0)

          Dim diff = (TotalRegular + TotalDisasterRegular + NonWorking) - 40

          If diff > 0 Then
            If diff > TotalDisasterRegular Then diff = TotalDisasterRegular

            Dim thisWeek = (From w In Week_TL(Week)
                            Order By w.WorkDate Descending
                            Select w).ToList()


            If NonWorking > 0 Then

              Dim nonworkingdiff As Double = NonWorking

              If diff > NonWorking Then
                ' we want to move some hours into disasterstraighttime
                ' and then move the rest into disasterovertime

                diff = diff - NonWorking

              Else
                ' we want to move all of diff into disasterstraighttime
                nonworkingdiff = diff
                diff = 0
              End If

              For Each t In thisWeek
                For Each dwh In t.DisasterWorkHoursList
                  If dwh.DisasterHoursRegular > 0 And nonworkingdiff > 0 Then
                    If nonworkingdiff > dwh.DisasterHoursRegular Then
                      dwh.DisasterHoursStraight += dwh.DisasterHoursRegular
                      nonworkingdiff -= dwh.DisasterHoursRegular
                      dwh.DisasterHoursRegular = 0
                    Else
                      dwh.DisasterHoursRegular -= nonworkingdiff
                      dwh.DisasterHoursStraight += nonworkingdiff
                      nonworkingdiff = 0
                      Exit For
                    End If

                  End If
                Next
                If nonworkingdiff = 0 Then Exit For

              Next


            End If

            If diff > 0 Then

              For Each t In thisWeek
                For Each dwh In t.DisasterWorkHoursList
                  If dwh.DisasterHoursRegular > 0 And diff > 0 Then
                    If diff > dwh.DisasterHoursRegular Then
                      dwh.DisasterHoursOvertime += dwh.DisasterHoursRegular
                      diff -= dwh.DisasterHoursRegular
                      dwh.DisasterHoursRegular = 0
                    Else
                      dwh.DisasterHoursRegular -= diff
                      dwh.DisasterHoursOvertime += diff
                      diff = 0
                      Exit For
                    End If

                  End If
                Next
                If diff = 0 Then Exit For

              Next
            End If

          End If
        End If
      Next

    End Sub



    'Function CalculateDisasterHours(TL As List(Of TimecardTimeData),
    '                              DisasterStart As Date,
    '                              DisasterEnd As Date) As Double
    '  Dim TotalHours As Double = 0
    '  Dim TotalDayHours As Double = 0
    '  Dim workDate As String = ""
    '  For Each t In TL
    '    Try

    '      TotalDayHours = 0
    '      workDate = t.WorkDate.ToShortDateString
    '      Dim times = t.WorkTimes.Split("-")
    '      Dim max As Integer = times.GetUpperBound(0)
    '      If max + 1 Mod 2 = 1 Then
    '        max -= 1 '
    '      End If
    '      For i As Integer = 0 To max Step 2
    '        Dim startTime As Date = Date.Parse(workDate & " " & times(i).Trim)
    '        Dim endTime As Date = Date.Parse(workDate & " " & times(i + 1).Trim)
    '        If startTime >= DisasterStart Or
    '        (endTime <= DisasterEnd And
    '        endTime >= DisasterStart) Then
    '          ' This is how we find out that this chunk of time is in our range.
    '          If startTime < DisasterStart Then startTime = DisasterStart
    '          If endTime > DisasterEnd Then endTime = DisasterEnd
    '          If endTime.Minute = 59 Then endTime = endTime.AddSeconds(1)
    '          Dim ts = endTime.Subtract(startTime)
    '          TotalDayHours += ts.Hours
    '          TotalDayHours += ts.Minutes / 60
    '          If TotalDayHours > 8 Then
    '            TotalDayHours = 8
    '            Exit For
    '          End If

    '        End If
    '        'If endTime.Subtract(startTime)
    '      Next
    '      TotalHours += TotalDayHours
    '    Catch ex As Exception
    '      Log(ex)
    '    End Try
    '  Next
    '  Return TotalHours
    'End Function

    'Function CalculateDisasterOT(TL As List(Of TimecardTimeData),
    '                            DisasterStart As Date,
    '                            DisasterEnd As Date) As Double
    '  Dim TotalHours As Double = 0
    '  Dim TotalDayHours As Double = 0
    '  Dim workDate As String = ""
    '  For Each t In TL
    '    Try

    '      TotalDayHours = 0
    '      workDate = t.WorkDate.ToShortDateString
    '      Dim times = t.WorkTimes.Split("-")
    '      Dim max As Integer = times.GetUpperBound(0)
    '      If max + 1 Mod 2 = 1 Then
    '        max -= 1 '
    '      End If
    '      For i As Integer = 0 To max Step 2
    '        Dim startTime As Date = Date.Parse(workDate & " " & times(i).Trim)
    '        Dim endTime As Date = Date.Parse(workDate & " " & times(i + 1).Trim)
    '        If startTime >= DisasterStart Or
    '        (endTime <= DisasterEnd And
    '        endTime >= DisasterStart) Then
    '          ' This is how we find out that this chunk of time is in our range.
    '          If startTime < DisasterStart Then startTime = DisasterStart
    '          If endTime > DisasterEnd Then endTime = DisasterEnd
    '          If endTime.Minute = 59 Then endTime = endTime.AddSeconds(1)
    '          Dim ts = endTime.Subtract(startTime)
    '          TotalDayHours += ts.Hours
    '          TotalDayHours += ts.Minutes / 60
    '        End If
    '        'If endTime.Subtract(startTime)
    '      Next
    '      If TotalDayHours > 8 Then
    '        TotalDayHours -= 8
    '      Else
    '        TotalDayHours = 0
    '      End If
    '      TotalHours += TotalDayHours
    '    Catch ex As Exception
    '      Log(ex)
    '    End Try
    '  Next
    '  Return TotalHours
    'End Function


    Public Sub New(ByRef TCTD As List(Of TimecardTimeData),
                   F As FinanceData,
                   pps As Date,
                   Optional CTE As Saved_TimeStore_Comp_Time_Earned = Nothing)
      EmployeeData = F
      TL = TCTD
      PayPeriodStart = pps

      DisasterPayRules = DisasterEventRules.Get_Cached_Disaster_Rules(pps.AddDays(13))
      'PopulateDisasterWorkDates() ' Removed on 8/21/2020
      Calculate_Disaster_Hours_By_Rule()
      Calculate_Disaster_Hours_By_Hours_Worked()
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
      'PopulateDisasterWorkDates() Removed on 8/21/2020
      Calculate_Disaster_Hours_By_Rule()
      Calculate_Disaster_Hours_By_Hours_Worked()
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
        Return EmployeeData.IsExempt
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
        Try


          'If (_Regular Is Nothing) Then
          '  _Regular = New Dictionary(Of Integer, Double)
          '  _Regular(0) = 0
          '  _Regular(1) = 0
          '  _Regular(2) = 0
          '  For i = 1 To 2
          '    Dim TotalHours As Double = 0
          '    Dim HoursWorked As Double = (From t In Week_TL(i)
          '                                 Select t.WorkHours).Sum
          '    TotalHours += HoursWorked
          '    TotalHours += BreakCredit(i)
          '    TotalHours += Holiday(i)
          '    TotalHours += OnCall_HoursWorked(i)
          '    TotalHours -= HolidayHoursWorkedDifference(i)
          '    TotalHours -= DisasterDoubleTime(i, 0)
          '    TotalHours -= DisasterStraightTime(i, 0)
          '    TotalHours -= DisasterOverTime_By_Payrule(i, 0)
          '    TotalHours -= DisasterRegular(i, 0)

          '    _Regular(i) = Math.Max(TotalHours, 0)
          '  Next


          '  _Regular(0) = _Regular(1) + _Regular(2)

          'End If

          Dim TotalHours As Double = 0
          Dim HoursWorked As Double = (From t In Week_TL(Week)
                                       Select t.WorkHours).Sum
          TotalHours += HoursWorked
          TotalHours += BreakCredit(Week)
          TotalHours += Holiday(Week)
          TotalHours += OnCall_HoursWorked(Week)
          TotalHours -= HolidayHoursWorkedDifference(Week)
          TotalHours -= DisasterHoursTotal(Week)
          'TotalHours -= DisasterDoubleTime(Week, 0)
          'TotalHours -= DisasterStraightTime(Week, 0)
          'TotalHours -= DisasterOverTime_By_Payrule(Week, 0)
          'TotalHours -= DisasterRegular(Week, 0)

          Return TotalHours

          'Return Math.Max((From t In Week_TL(Week)
          '                 Select t.WorkHours).Sum +
          '    BreakCredit(Week) +
          '    Holiday(Week) +
          '    OnCall_HoursWorked(Week) -
          '    HolidayHoursWorkedDifference(Week) -
          '    DisasterDoubleTime(Week, 0) -
          '    DisasterStraightTime(Week, 0) -
          '    DisasterOverTime_By_Payrule(Week, 0) -
          '    DisasterRegular(Week, 0), 0)

          ' replaced on 8/23/2020
          'Return Math.Max((From t In Week_TL(Week)
          '                 Select t.WorkHours).Sum +
          '    BreakCredit(Week) + Holiday(Week) + OnCall_HoursWorked(Week) -
          '    HolidayHoursWorkedDifference(Week) - DisasterDoubleTime(Week, 0) -
          '    DisasterStraightTime(Week, 0) -
          '    DisasterOverTime(Week, 0) - Math.Max(DisasterRegular(Week, 0), 0), 0)


          'Return _Regular(Week)
        Catch ex As Exception
          Dim e As New ErrorLog(ex, "")
          Return 0
        End Try
      End Get
    End Property

    Public ReadOnly Property Calculated_Regular(Week As Integer) As Double
      Get
        'If _Calculated_Regular Is Nothing Then
        '  _Calculated_Regular = New Dictionary(Of Integer, Double)
        '  _Calculated_Regular(0) = 0
        '  _Calculated_Regular(1) = 0
        '  _Calculated_Regular(2) = 0
        '  For i = 1 To 2
        '    Dim val As Double = Total_Hours(i) -
        '      DisasterDoubleTime(i, 0) -
        '      DisasterOverTime(i, 0) -
        '      DisasterStraightTime(i, 0)
        '    If Term_Hours(0) = 0 Then

        '      If IsExempt AndAlso EmployeeData.HireDate < PayPeriodStart Then
        '        Dim HoursForOTByWeek As Double = EmployeeData.HoursNeededForOvertime / 2
        '        If val > HoursForOTByWeek Then val = HoursForOTByWeek
        '        If val > 0 And val < HoursForOTByWeek Then val = HoursForOTByWeek
        '      Else

        '        If val > 40 Then
        '          val = 40
        '        End If

        '      End If
        '    End If

        '    val = val - Sick_All(i) - Vacation(i) - Comp_Time_Used(i) -
        '      LWOP_All(i) - SickLeavePool(i) - Term_Hours(i) -
        '      Calculated_DisasterRegular(i, 0) - DisasterAdminLeave(i, 0) - Out_Of_Class(i)
        '    If val < 0 Then val = 0
        '    _Calculated_Regular(i) = val
        '  Next

        '  _Calculated_Regular(0) = _Calculated_Regular(1) + _Calculated_Regular(2)
        'End If
        'Return _Calculated_Regular(Week)
        Select Case Week
          Case 1, 2
            Dim val As Double = Total_Hours(Week) - DisasterHoursTotal(Week)
            ' replaced on 8/24/2020
            'Dim val As Double = Total_Hours(Week) - DisasterDoubleTime(Week, 0) -
            '  DisasterOverTime_By_Payrule(Week, 0) - DisasterStraightTime(Week, 0)
            ' There is one person currently who is exempt but is only paid for 12 hours per pay period. 
            ' Any hours in addition to that are reduced to 12.
            If Term_Hours(0) = 0 Then

              If IsExempt AndAlso EmployeeData.HireDate < PayPeriodStart Then
                Dim HoursForOTByWeek As Double = EmployeeData.HoursNeededForOvertime / 2
                If val > HoursForOTByWeek Then val = HoursForOTByWeek
                'If val > 0 And val < HoursForOTByWeek Then val = HoursForOTByWeek
              Else

                If val > 40 Then
                  val = 40
                End If

              End If
            End If

            val = val - Sick_All(Week) - Vacation(Week) - Comp_Time_Used(Week) -
              LWOP_All(Week) - SickLeavePool(Week) - Term_Hours(Week) -
              DisasterAdminLeave(Week, 0) - Out_Of_Class(Week)
            If val < 0 Then val = 0
            Return val
          Case Else
            Return Calculated_Regular(1) + Calculated_Regular(2)
        End Select

      End Get
    End Property

    'Public ReadOnly Property Out_Of_Class(Week As Integer) As Double
    '  Get
    '    If Not DoesOutOfClassApply Then Return 0


    '    'Select Case Week
    '    '  Case 1, 2
    '    '    ' Replaced on 8/26/2020
    '    '    'Dim PubWorksDepartments() As String = {"3701", "3711", "3712"}
    '    '    'Return (From t In Week_TL(Week)
    '    '    '        Where t.OutOfClass And PubWorksDepartments.Contains(t.DepartmentNumber)
    '    '    '        Select t.WorkHours + t.BreakCreditHours).Sum

    '    '  Case Else
    '    '    Return Out_Of_Class_Regular(1) + Out_Of_Class_Regular(2)
    '    'End Select
    '  End Get
    'End Property

    Public ReadOnly Property Out_Of_Class(Week As Integer) As Double
      Get
        Return Out_Of_Class_Regular(Week) + Out_Of_Class_Disaster(Week)
      End Get
    End Property

    Public ReadOnly Property Out_Of_Class_Regular(Week As Integer) As Double
      Get
        If Not DoesOutOfClassApply Then Return 0

        If _Out_Of_Class_Regular Is Nothing Then
          _Out_Of_Class_Regular(0) = 0
          _Out_Of_Class_Regular(1) = 0
          _Out_Of_Class_Regular(2) = 0
          For i = 1 To 2
            For Each t In Week_TL(i)
              If t.OutOfClass Then

                Dim totaldisasterhours = (From dwh In t.DisasterWorkHoursList
                                          Select dwh.DisasterHoursTotal).Sum

                _Out_Of_Class_Regular(i) += (t.WorkHours + t.BreakCreditHours) - totaldisasterhours
              End If
            Next
          Next
          _Out_Of_Class_Regular(0) = _Out_Of_Class_Regular(1) + _Out_Of_Class_Regular(2)
        End If
        Return _Out_Of_Class_Regular(Week)

      End Get
    End Property

    Public ReadOnly Property Out_Of_Class_Disaster(week As Integer) As Double
      Get
        If Not DoesOutOfClassApply Then Return 0

        If _Out_Of_Class_Disaster Is Nothing Then
          _Out_Of_Class_Disaster(0) = 0
          _Out_Of_Class_Disaster(1) = 0
          _Out_Of_Class_Disaster(2) = 0
          For i = 1 To 2
            For Each t In Week_TL(i)
              If t.OutOfClass Then

                Dim totaldisasterhours = (From dwh In t.DisasterWorkHoursList
                                          Select dwh.DisasterHoursTotal).Sum

                _Out_Of_Class_Disaster(i) += totaldisasterhours
              End If
            Next
          Next
          _Out_Of_Class_Disaster(0) = _Out_Of_Class_Disaster(1) + _Out_Of_Class_Disaster(2)
        End If
        Return _Out_Of_Class_Disaster(week)

      End Get
    End Property

    Public ReadOnly Property DisasterHoursTotal(week As Integer) As Double
      Get
        If _DisasterHoursTotal Is Nothing Then
          _DisasterHoursTotal = New Dictionary(Of Integer, Double)
          _DisasterHoursTotal(0) = 0
          _DisasterHoursTotal(1) = 0
          _DisasterHoursTotal(2) = 0
          For i = 1 To 2
            For Each t In Week_TL(i)
              For Each dwh In t.DisasterWorkHoursList
                _DisasterHoursTotal(i) += dwh.DisasterWorkHoursByRule(0) +
                  dwh.DisasterWorkHoursByRule(1) +
                  dwh.DisasterWorkHoursByRule(2)
              Next
              If t.BreakCreditHours > 0 Then
                If _DisasterHoursTotal(i) + t.BreakCreditHours = t.TotalHours Then
                  _DisasterHoursTotal(i) += t.BreakCreditHours
                End If
              End If
            Next
          Next
          _DisasterHoursTotal(0) = _DisasterHoursTotal(1) + _DisasterHoursTotal(2)
        End If
        Return _DisasterHoursTotal(week)
      End Get
    End Property



    Public ReadOnly Property DisasterRegular(week As Integer, period_id As Integer) As Double
      Get
        If DisasterPayRules.Count() = 0 Then Return 0

        If week = 0 Then Return DisasterRegular(1, period_id) + DisasterRegular(2, period_id)

        Dim Hours As Double = 0
        For Each t In Week_TL(week)

          Dim dwh_list = TimecardTimeData.GetDisasterWorkHoursListByWeekAndPeriod(week, period_id, t)

          Hours += (From dwh In dwh_list
                    Select dwh.DisasterHoursRegular).Sum
        Next
        If IsExempt Then
          Hours = Math.Min(40, Hours)
        End If
        Return Hours

        ' Replace 8/24/2020
        'Dim DisasterRegularTest As Double = 0


        'If DisasterPayRules.Count() = 0 Then Return 0

        'If week = 0 Then Return DisasterRegular(1, period_id) + DisasterRegular(2, period_id)

        'Dim hours As Double = 0

        'For Each t In Week_TL(week)


        '  Dim dwh_list As List(Of DisasterWorkHours)
        '  If period_id = 0 Then
        '    dwh_list = t.DisasterWorkHoursList
        '  Else
        '    dwh_list = (From d In t.DisasterWorkHoursList
        '                Where d.DisasterPeriodId = period_id
        '                Select d).ToList()
        '  End If

        '  DisasterRegularTest += (From dwh In dwh_list
        '                          Select dwh.DisasterHoursRegular).Sum

        '  For Each dwh In dwh_list
        '    ' Old version 
        '    hours += dwh.DisasterWorkHoursByRule(0)
        '    If t.DisasterNormalScheduledHours > 0 Then
        '      If t.DisasterNormalScheduledHours > dwh.DisasterWorkHoursByRule(1) + dwh.DisasterWorkHoursByRule(2) + t.BreakCreditHours Then
        '        hours += dwh.DisasterWorkHoursByRule(1) + dwh.DisasterWorkHoursByRule(2) + t.BreakCreditHours
        '      Else
        '        hours += t.DisasterNormalScheduledHours
        '      End If
        '      ' Old version
        '      'If t.DisasterNormalScheduledHours > (dwh.DisasterWorkHoursByRule(1) + dwh.DisasterWorkHoursByRule(2)) Then
        '      '  hours += (dwh.DisasterWorkHoursByRule(1) + dwh.DisasterWorkHoursByRule(2))
        '      'Else
        '      '  hours += t.DisasterNormalScheduledHours
        '      'End If
        '    End If
        '  Next

        'Next
        'If IsExempt Then
        '  hours = Math.Min(40, hours)
        'End If
        'Return hours
      End Get
    End Property

    Public ReadOnly Property Calculated_DisasterRegular(Week As Integer, period_id As Integer) As Double
      Get
        Select Case Week
          Case 1, 2
            Return DisasterRegular(Week, period_id)
            'Dim val As Double = Total_Hours(Week) - DisasterDoubleTime(Week) - DisasterOverTime(Week) - DisasterStraightTime(Week)
            'Dim val As Double = DisasterRegular(Week)
            'If Term_Hours(0) = 0 Then
            '  If IsExempt AndAlso EmployeeData.HireDate < PayPeriodStart Then
            '    Dim HoursForOTByWeek As Double = EmployeeData.HoursNeededForOvertime / 2
            '    If val > HoursForOTByWeek Then val = HoursForOTByWeek
            '    If val > 0 And val < HoursForOTByWeek Then val = HoursForOTByWeek
            '  Else
            '    If val > 40 Then
            '      val = 40
            '    End If
            '  End If
            '  'If val > DisasterRegular(Week) Then
            '  '  val = DisasterRegular(Week)
            '  'End If
            'End If
            'val = val - Sick_All(Week) - Vacation(Week) - Comp_Time_Used(Week) -
            '  LWOP_All(Week) - SickLeavePool(Week) - Term_Hours(Week) - DisasterAdminLeave(Week)
            'Return Math.Max(val, 0)
          Case Else
            Return Calculated_DisasterRegular(1, period_id) + Calculated_DisasterRegular(2, period_id)
        End Select
      End Get
    End Property

    Public ReadOnly Property DisasterDoubleTime(Week As Integer, period_id As Integer) As Double
      Get

        If DisasterPayRules.Count() = 0 Or IsExempt Then Return 0

        If Week = 0 Then Return DisasterDoubleTime(1, period_id) + DisasterDoubleTime(2, period_id)

        Dim doubletimeHours As Double = 0
        For Each t In Week_TL(Week)
          Dim dwh_list As List(Of DisasterWorkHours)
          If period_id = 0 Then
            dwh_list = t.DisasterWorkHoursList
          Else
            dwh_list = (From d In t.DisasterWorkHoursList
                        Where d.DisasterPeriodId = period_id
                        Select d).ToList()
          End If
          doubletimeHours += (From d In dwh_list
                              Select d.DisasterHoursDoubletime).Sum
        Next
        Return Math.Max(doubletimeHours, 0)
        'Replaced on 8/24/2020
        'Dim hours As Double = 0

        'For Each t In Week_TL(Week)
        '  Dim dwh_list As List(Of DisasterWorkHours)
        '  If period_id = 0 Then
        '    dwh_list = t.DisasterWorkHoursList
        '  Else
        '    dwh_list = (From d In t.DisasterWorkHoursList
        '                Where d.DisasterPeriodId = period_id
        '                Select d).ToList()
        '  End If

        '  For Each dwh In dwh_list
        '    If dwh.DisasterWorkHours > 0 Then
        '      ' The way the rules work is:
        '      ' If the employee is scheduled to work, only the hours worked in addition to 
        '      ' their normally scheduled hours are eligible to become double time
        '      ' If date worked is a Sunday, then it doesn't matter if it is for a partial or full activation
        '      If t.DisasterNormalScheduledHours > 0 Then
        '        Dim totalhours = dwh.DisasterWorkHoursByRule(1) + dwh.DisasterWorkHoursByRule(2) + t.BreakCreditHours
        '        Dim normaldiff = totalhours - t.DisasterNormalScheduledHours
        '        If t.WorkDate.DayOfWeek = DayOfWeek.Sunday Then
        '          hours += Math.Max(0, normaldiff)
        '        Else
        '          If normaldiff >= dwh.DisasterWorkHoursByRule(2) Then
        '            hours += dwh.DisasterWorkHoursByRule(2)
        '          End If
        '          If normaldiff < dwh.DisasterWorkHoursByRule(2) Then
        '            hours += Math.Max(normaldiff, 0)
        '          End If
        '        End If

        '      Else
        '        If t.WorkDate.DayOfWeek = DayOfWeek.Sunday Then
        '          hours += dwh.DisasterWorkHoursByRule(1) + dwh.DisasterWorkHoursByRule(2) + t.BreakCreditHours
        '        Else
        '          hours += dwh.DisasterWorkHoursByRule(2) + t.BreakCreditHours
        '        End If

        '      End If
        '      ' Removed on 8/23/2020
        '      'If t.WorkDate.DayOfWeek = DayOfWeek.Sunday And t.DisasterNormalScheduledHours <= 0 Then
        '      '  hours += dwh.DisasterWorkHoursByRule(1) + dwh.DisasterWorkHoursByRule(2) + t.BreakCreditHours
        '      'Else
        '      '  If t.DisasterNormalScheduledHours > 0 Then

        '      '  End If
        '      '  If dwh.DisasterWorkHoursByRule(2) > 0 Then
        '      '    hours += dwh.DisasterWorkHoursByRule(2) + t.BreakCreditHours
        '      '  End If
        '      'End If

        '    End If
        '  Next
        'Next
        'Return Math.Max(hours, 0)
        'If IsExempt Then
        '  Return 0
        'Else
        '  ' old 8/31/2019
        '  'Return (From t In Week_TL(Week)
        '  '        Where t.DisasterRule = 1
        '  '        Select t.WorkHours + t.BreakCreditHours).Sum
        '  Return (From t In Week_TL(Week)
        '          Select t.WorkHours + t.BreakCreditHours).Sum
        'End If
      End Get
    End Property

    Public ReadOnly Property DisasterOverTime(Week As Integer, period_id As Integer) As Double
      Get
        Return DisasterOverTime_By_Hours_Worked(Week, period_id) + DisasterOverTime_By_Payrule(Week, period_id)
      End Get
    End Property

    Public ReadOnly Property DisasterOverTime_By_Hours_Worked(Week As Integer, period_id As Integer) As Double
      Get
        If IsExempt Then
          Return 0
        Else
          Dim Diff As Double = 0
          Select Case Week
            ' Old version, replaced on 8/23/2020
            'Case 1, 2
            '  Diff = Regular(Week) + DisasterRegular(Week, 0) -
            '    Double_Time(Week) - Comp_Time_Earned(Week) -
            '    DisasterDoubleTime(Week, 0) - 40
            Case 1, 2
              ' removed DisasterRegular from calculations
              Diff += Regular(Week)
              Diff += DisasterRegular(Week, 0)
              Diff -= Comp_Time_Earned(Week)
              'Diff -= DisasterDoubleTime(Week, 0)
              Diff -= 40

              'Diff = Regular(Week) +
              '  DisasterRegular(Week, 0) -
              '  Comp_Time_Earned(Week) -
              '  DisasterDoubleTime(Week, 0) - 40
              ' upper bound is DisasterRegular(week, period_id)
              ' lower bound is 0
              If DisasterRegular(Week, period_id) < Diff Then
                Return DisasterRegular(Week, period_id)
              Else
                Return Math.Max(Diff, 0)
              End If

            Case Else
              Return DisasterOverTime_By_Hours_Worked(1, period_id) + DisasterOverTime_By_Hours_Worked(2, period_id)
          End Select
        End If
      End Get
    End Property

    Public ReadOnly Property DisasterOverTime_By_Payrule(Week As Integer, period_id As Integer) As Double
      Get
        If DisasterPayRules.Count() = 0 Or IsExempt Then Return 0

        If Week = 0 Then Return DisasterOverTime(1, period_id) + DisasterOverTime(2, period_id)

        Dim overtimeHours As Double = 0
        For Each t In Week_TL(Week)
          Dim dwh_list = TimecardTimeData.GetDisasterWorkHoursListByWeekAndPeriod(Week, period_id, t)
          overtimeHours += (From d In dwh_list
                            Select d.DisasterHoursOvertime).Sum
        Next
        Return Math.Max(overtimeHours, 0)



        'If DisasterPayRules.Count() = 0 Or IsExempt Then Return 0

        'If Week = 0 Then Return DisasterOverTime_By_Payrule(1, period_id) + DisasterOverTime_By_Payrule(2, period_id)

        'Dim hours As Double = 0

        'For Each t In Week_TL(Week)
        '  Dim dwh_list As List(Of DisasterWorkHours)
        '  If period_id = 0 Then
        '    dwh_list = t.DisasterWorkHoursList
        '  Else
        '    dwh_list = (From d In t.DisasterWorkHoursList
        '                Where d.DisasterPeriodId = period_id
        '                Select d).ToList()
        '  End If
        '  For Each dwh In dwh_list

        '    If dwh.DisasterWorkHours > 0 Then
        '      If (t.WorkDate.DayOfWeek = DayOfWeek.Sunday And t.DisasterNormalScheduledHours > 0) Or
        '        t.WorkDate.DayOfWeek <> DayOfWeek.Sunday Then

        '        If t.DisasterNormalScheduledHours > 0 Then

        '          Dim difference = t.WorkHours + t.BreakCreditHours - t.DisasterNormalScheduledHours - dwh.DisasterWorkHoursByRule(2)
        '          If difference > 0 Then
        '            If difference > dwh.DisasterWorkHoursByRule(1) + t.BreakCreditHours Then
        '              hours += dwh.DisasterWorkHoursByRule(1) + t.BreakCreditHours
        '            Else
        '              hours += difference
        '            End If
        '          End If

        '        Else
        '          hours += dwh.DisasterWorkHoursByRule(1) + t.BreakCreditHours
        '        End If

        '      End If

        '    End If
        '  Next

        'Next
        'Return Math.Max(hours, 0)
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

    Public ReadOnly Property DisasterStraightTime(Week As Integer, period_id As Integer) As Double
      Get
        If DisasterPayRules.Count() = 0 Or Not IsExempt Then Return 0

        If Week = 0 Then Return DisasterStraightTime(1, period_id) + DisasterStraightTime(2, period_id)

        Dim hours As Double = 0

        Dim disasterStraightTimeTest As Double = 0

        For Each t In Week_TL(Week)
          Dim dwh_list As List(Of DisasterWorkHours)
          If period_id = 0 Then
            dwh_list = t.DisasterWorkHoursList
          Else
            dwh_list = (From d In t.DisasterWorkHoursList
                        Where d.DisasterPeriodId = period_id
                        Select d).ToList()
          End If

          disasterStraightTimeTest = (From d In dwh_list
                                      Select d.DisasterHoursStraight).Sum


          For Each dwh In dwh_list


            If dwh.DisasterWorkHoursByRule(2) > 0 Then
              hours += dwh.DisasterWorkHoursByRule(2) ' these hours are straighttime regardless
            Else
              If t.DisasterNormalScheduledHours > 0 Then
                Dim difference = t.WorkHours - t.DisasterNormalScheduledHours - dwh.DisasterWorkHoursByRule(0)
                If difference > 0 Then
                  If difference > dwh.DisasterWorkHoursByRule(1) Then
                    hours += dwh.DisasterWorkHoursByRule(1)
                  Else
                    hours += difference
                  End If
                End If
              Else
                hours += dwh.DisasterWorkHoursByRule(1)
              End If
            End If
            'hours += (difference - t.DisasterNormalScheduledHours)
          Next
        Next
        Return Math.Max(hours, 0)
        'If IsExempt Then
        '  ' old 8/31/2019
        '  'Dim weekday = (From t In Week_TL(Week)
        '  '               Where t.DisasterRule = 1 And
        '  '                 t.WorkHours > 8 And
        '  '                 t.WorkDate.DayOfWeek <> DayOfWeek.Saturday And
        '  '                 t.WorkDate.DayOfWeek <> DayOfWeek.Sunday
        '  '               Select t.WorkHours - 8).Sum
        '  'Dim weekend = (From t In Week_TL(Week)
        '  '               Where t.DisasterRule = 1 And
        '  '                 (t.WorkDate.DayOfWeek = DayOfWeek.Saturday Or
        '  '                 t.WorkDate.DayOfWeek = DayOfWeek.Sunday)
        '  '               Select t.WorkHours).Sum
        '  Dim weekday = (From t In Week_TL(Week)
        '                 Where t.WorkHours > 8 And
        '                   t.WorkDate.DayOfWeek <> DayOfWeek.Saturday And
        '                   t.WorkDate.DayOfWeek <> DayOfWeek.Sunday
        '                 Select t.WorkHours - 8).Sum
        '  Dim weekend = (From t In Week_TL(Week)
        '                 Where (t.WorkDate.DayOfWeek = DayOfWeek.Saturday Or
        '                   t.WorkDate.DayOfWeek = DayOfWeek.Sunday)
        '                 Select t.WorkHours).Sum
        '  Return weekday + weekend
        'Else
        '  Return 0
        'End If
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

    'Public ReadOnly Property Non_Working_Paid_Time(Week As Integer) As Double
    '  Get
    '    Return Admin(Week) + Vacation(Week) + Sick(Week) + SickLeavePool(Week) + LWOP_All(Week) + Comp_Time_Used(Week) + Admin_Disaster(Week)
    '  End Get
    'End Property

    Public ReadOnly Property Overtime(Week As Integer) As Double
      Get
        If IsExempt Then
          Return 0
        Else
          Dim Diff As Double = 0
          ' Old version, replaced on 8/23/2020
          Select Case Week
            ' Old version, replaced on 8/23/2020
            'Case 1, 2
            '  Diff = Regular(Week) + DisasterRegular(Week, 0) -
            '    Double_Time(Week) - Comp_Time_Earned(Week) -
            '    DisasterDoubleTime(Week, 0) - 40
            Case 1, 2
              ' removed DisasterRegular from calculations
              Diff = Regular(Week) -
                DisasterOverTime_By_Hours_Worked(Week, 0) -
                Comp_Time_Earned(Week) - 40
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
                DisasterStraightTime(Week, 0) - DisasterDoubleTime(Week, 0) -
                DisasterOverTime(Week, 0)


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



        For Each t In TL
          'If t.AdminHours > 8 Then
          '    WarningList.Add("8 or more Admin leave hours entered on " & t.WorkDate.ToShortDateString & ".")
          'End If

          For Each dwh In t.DisasterWorkHoursList
            Dim totaldisasterhours As Double = dwh.DisasterWorkHoursByRule(0) + dwh.DisasterWorkHoursByRule(1) + dwh.DisasterWorkHoursByRule(2)
            If dwh.DisasterWorkHours > totaldisasterhours Then
              Dim dpr As DisasterEventRules = (From d In DisasterPayRules
                                               Where d.period_id = dwh.DisasterPeriodId
                                               Select d).First()
              Dim EventInfo As String = dpr.period_name & " is from " & dpr.StartDateTime.ToLongDateString & " " & dpr.StartDateTime.ToShortTimeString & " to " & dpr.EndDateTime.ToLongDateString & " " & dpr.EndDateTime.ToShortTimeString

              ErrorList.Add("Your special event hours entered on " & t.WorkDate.ToShortDateString & " fall outside of the event start and end time. (" & EventInfo & ")")
            End If
          Next

          If t.WorkDate = "9/2/2019" Or t.WorkDate = "9/3/2019" Or t.WorkDate = "9/4/2019" Then
            If t.BreakCreditHours > 0 Then
              WarningList.Add("No breaks should be entered on this date.")
            End If
          End If

          For Each dwh In t.DisasterWorkHoursList

            If dwh.DisasterWorkHours > 0 And dwh.DisasterWorkType.Trim.Length = 0 Then
              ErrorList.Add("You must select what type of work was performed for the special event hours on " & t.WorkDate.ToShortDateString & ".")
            End If

            If t.DisasterWorkHours > t.WorkHours Then
              ErrorList.Add("The work hours entered on " & t.WorkDate.ToShortDateString & " are less than the special event work hours entered.  If the special event work hours are correct, then the work hours should be updated to reflect this.")
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

        If IsExempt Then
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



  End Class
End Namespace