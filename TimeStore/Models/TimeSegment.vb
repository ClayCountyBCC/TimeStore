Namespace Models

  Public Enum WorkType
    Regular = 0
    Disaster = 1
    BreakCredit = 2
    Doubletime = 3
    DisasterDoubletime = 4
    Admin = 5
    HolidayRegularOvertime = 6
  End Enum

  Public Class TimeSegment
    Public start_time_raw As String
    Public end_time_raw As String
    Public start_time As Date
    Public end_time As Date
    Public total_hours As Decimal
    Public ts As TimeSpan
    Public work_date As Date
    Public week As Integer = -1
    Public work_type As WorkType
    Public disaster_period_id As Integer = -1
    Public disaster_pay_rule As Integer = -1
    Public payrate As Double
    Public pay_period_start As Date

    Public Sub New(start_raw As String,
                   end_raw As String,
                   workdate As Date,
                   wt As WorkType,
                   pay_period_start As Date,
                   period_id As Integer,
                   pr As Double)
      Try
        work_type = wt
        payrate = pr
        start_time_raw = start_raw.Trim
        end_time_raw = end_raw.Trim
        work_date = workdate
        Me.pay_period_start = pay_period_start
        SetWeek()

        Dim wd As String = work_date.ToShortDateString() & " "

        start_time = Date.Parse(wd & start_time_raw)
        end_time = Date.Parse(wd & end_time_raw)
        If end_time.Second = 59 Then end_time = end_time.AddSeconds(1)
        ts = end_time.Subtract(start_time)
        total_hours = ts.TotalHours

        disaster_period_id = period_id
      Catch ex As Exception
        Dim e As New ErrorLog(ex, "")
      End Try
    End Sub

    Public Sub New(start_date As Date,
                   end_date As Date,
                   wt As WorkType,
                   pps As Date,
                   period_id As Integer,
                   pr As Double)
      work_type = wt
      work_date = start_date.Date
      start_time = start_date
      end_time = end_date
      Me.pay_period_start = pps
      SetWeek()
      payrate = pr
      disaster_period_id = period_id
      Update()


    End Sub

    Private Sub SetWeek()
      If work_date >= pay_period_start AndAlso work_date < pay_period_start.AddDays(7) Then
        week = 1
      Else
        If work_date >= pay_period_start.AddDays(7) AndAlso work_date < pay_period_start.AddDays(14) Then
          week = 2
        Else
          week = -1
        End If
      End If
    End Sub

    Public Sub Update()
      ' if the start or end time have been updated, we can run this to update everything else
      start_time_raw = start_time.ToShortTimeString
      end_time_raw = end_time.ToShortTimeString
      ts = end_time.Subtract(start_time)
      total_hours = ts.TotalHours
    End Sub

    Public Shared Function Create(tctd As TimecardTimeData, pay_period_start As Date, payrate As Double) As List(Of TimeSegment)
      ' Break credit?!
      ' We will handle the payrate calculations outside of this function
      Dim dpr = DisasterEventRules.Get_Cached_Disaster_Rules(pay_period_start.AddDays(13))
      Dim regular_segments As New List(Of TimeSegment)
      Dim disaster_segments As New List(Of TimeSegment)
      If tctd.WorkTimes.Length > 0 Then
        regular_segments.AddRange(CreateSpecific(tctd.WorkTimes,
                                                 WorkType.Regular,
                                                 -1,
                                                 tctd.WorkDate,
                                                 pay_period_start,
                                                 payrate))
      End If

      If tctd.OnCallWorkTimes.Length > 0 Then
        regular_segments.AddRange(CreateSpecific(tctd.OnCallWorkTimes,
                                                 WorkType.Regular,
                                                 -1,
                                                 tctd.WorkDate,
                                                 pay_period_start,
                                                 payrate))
      End If



      If tctd.BreakCreditHours > 0 Then
        ' if the entire day is nothing but disaster hours, the 
        ' break credit is eligible for being paid at the disaster pay rule rate.
        ' it will not be included in the disaster hours.
        Dim bcstart = FindBreakCreditGap(regular_segments)
        If bcstart.HasValue Then
          Dim bcend As Date = bcstart.Value.AddMinutes(30)
          Dim bcts As New TimeSegment(bcstart.Value.ToShortTimeString,
                                    bcend.ToShortTimeString,
                                    tctd.WorkDate,
                                    WorkType.BreakCredit,
                                    pay_period_start,
                                    -1,
                                    payrate)
          regular_segments.Add(bcts)
        Else
          Dim e As New ErrorLog("break credit not handled", "", "", "", "")
        End If
      End If

      If dpr.Count() > 0 Then
        For Each dwh In tctd.DisasterWorkHoursList
          disaster_segments.AddRange(CreateSpecific(dwh.DisasterWorkTimes,
                                                    WorkType.Disaster,
                                                    dwh.DisasterPeriodId,
                                                    tctd.WorkDate,
                                                    pay_period_start,
                                                    payrate))
        Next
        disaster_segments = RefineDisasterHours(disaster_segments, dpr)
      End If
      ' now all of the time that's been allocated to the disaster
      ' will need to be removed from the regular hours
      Dim combined As New List(Of TimeSegment)
      If disaster_segments.Count > 0 Then

        combined.AddRange(RefineRegularHours(regular_segments, disaster_segments))
      Else
        combined.AddRange(regular_segments)
      End If

      If tctd.DoubleTimeHours > 0 Then
        RefineDoubletimeHours(tctd.DoubleTimeHours, combined)
      End If

      If tctd.HolidayHours > 0 Then
        combined.AddRange(RefineHolidayHours(tctd, pay_period_start, payrate))
      End If

      Dim total_admin As Double = tctd.AdminBereavement + tctd.AdminHours + tctd.AdminJuryDuty +
        tctd.AdminMilitaryLeave + tctd.AdminOther + tctd.AdminWorkersComp + tctd.OnCallMinimumHours
      If total_admin > 0 Then
        combined.Add(RefineAdminHours(tctd, total_admin, pay_period_start, payrate))
      End If
      Return combined
    End Function

    Private Shared Function RefineHolidayHours(tctd As TimecardTimeData, pay_period_start As Date, payrate As Double) As List(Of TimeSegment)
      Dim tslist As New List(Of TimeSegment)
      Dim start_date = tctd.WorkDate.Date
      Dim end_date = start_date.AddHours(tctd.HolidayHours)

      Dim wh = tctd.WorkHours + tctd.BreakCreditHours
      Dim h = tctd.HolidayHours
      Dim dt = tctd.DoubleTimeHours
      Dim diff = wh - dt
      Dim totalworked = wh - diff
      If diff < h Then
        Dim new_end = start_date.AddHours(diff)
        tslist.Add(New TimeSegment(start_date, new_end, WorkType.HolidayRegularOvertime, pay_period_start, -1, payrate))
        tslist.Add(New TimeSegment(new_end, end_date, WorkType.Regular, pay_period_start, -1, payrate))
      Else
        tslist.Add(New TimeSegment(start_date, end_date, WorkType.HolidayRegularOvertime, pay_period_start, -1, payrate))
      End If

      Return tslist
    End Function

    Private Shared Function RefineAdminHours(tctd As TimecardTimeData, total_admin_hours As Double, pay_period_start As Date, payrate As Double) As TimeSegment
      Dim start_date = tctd.WorkDate.Date
      Dim end_date = start_date.AddHours(total_admin_hours)

      Dim ts As New TimeSegment(start_date, end_date, WorkType.Admin, pay_period_start, -1, payrate)
      Return ts
    End Function

    Private Shared Sub RefineDoubletimeHours(DoubletimeHours As Double, ByRef segments As List(Of TimeSegment))
      ' In this function, we'll take the number of doubletime hours and find/assign a segment to those hours
      ' we'll try and associate the regular hours first, only if there are some hours left over,
      ' we'll try and associate the disaster hours with it.
      ' if the segment is regular, or disaster with pay rule 0, we assign them to doubletime
      ' if they are disaster and payrule 1 or 2, we'll move them to disaster doubletime
      ' this function should be run before the ApplyHolidayHours function, because we don't want those hours
      ' to be mixed up here.
      ' This function happens at the day level
      Dim assigned As Double = 0
      Dim regular_ts = (From s In segments
                        Where s.work_type = WorkType.Regular Or
                          (s.work_type = WorkType.Disaster AndAlso s.disaster_pay_rule = 0)
                        Select s).ToList

      For Each r In regular_ts
        If r.total_hours = (DoubletimeHours - assigned) Then
          r.work_type = WorkType.Doubletime
          assigned = DoubletimeHours
          Exit For
        Else
          If r.total_hours > (DoubletimeHours - assigned) Then
            ' we're going to need to split this segment
            Dim newEndTime = r.start_time.AddHours(DoubletimeHours - assigned)
            Dim new_r = r.Clone(newEndTime, r.end_time)
            segments.Add(new_r)
            r.end_time = newEndTime
            r.Update()
            r.work_type = WorkType.Doubletime
          Else
            'r.total_hours is less than doubletimehours - assigned
            ' we split and keep looking
            r.work_type = WorkType.Doubletime
            assigned += r.total_hours
          End If
        End If
      Next

      If assigned > 0 Then
        Dim disaster_ts = (From s In segments
                           Where s.work_type = WorkType.Disaster
                           Select s).ToList
        For Each d In disaster_ts
          If d.total_hours = (DoubletimeHours - assigned) Then
            d.work_type = WorkType.DisasterDoubletime
            assigned = DoubletimeHours
            Exit For
          Else
            If d.total_hours > (DoubletimeHours - assigned) Then
              ' we're going to need to split this segment
              Dim newEndTime = d.start_time.AddHours(DoubletimeHours - assigned)
              Dim new_r = d.Clone(newEndTime, d.end_time)
              segments.Add(new_r)
              d.end_time = newEndTime
              d.Update()
              d.work_type = WorkType.DisasterDoubletime
            Else
              'd.total_hours is less than doubletimehours - assigned
              ' we split and keep looking
              d.work_type = WorkType.DisasterDoubletime
              assigned += d.total_hours
            End If
          End If
        Next
      End If
    End Sub

    Private Shared Function RefineDisasterHours(segments As List(Of TimeSegment), dpr As List(Of DisasterEventRules)) As List(Of TimeSegment)
      Dim disaster_segments = New List(Of TimeSegment)
      For Each ds In segments
        Dim rules As List(Of DisasterEventRules) = (From d In dpr
                                                    Where d.period_id = ds.disaster_period_id
                                                    Order By d.StartDateTime Ascending
                                                    Select d).ToList()

        If rules.Count > 0 Then ' if rules.count = 0 then this segment's hourse are not valid disaster hours.
          For Each rule In rules
            If ds.start_time < rule.EndDateTime AndAlso rule.StartDateTime < ds.end_time Then
              If ds.start_time < rule.StartDateTime Then
                Dim starttime As Date = rule.StartDateTime
                Dim endtime As Date = ds.end_time
                If endtime > rule.EndDateTime Then endtime = rule.EndDateTime

                Dim ts As TimeSegment = ds.Clone(starttime, endtime)
                ts.disaster_pay_rule = rule.pay_rule
                disaster_segments.Add(ts)
              Else
                If ds.end_time > rule.EndDateTime Then
                  Dim ts As TimeSegment = ds.Clone(ds.start_time, rule.EndDateTime)
                  ts.disaster_pay_rule = rule.pay_rule
                  disaster_segments.Add(ts)
                Else
                  ds.disaster_pay_rule = rule.pay_rule
                  disaster_segments.Add(ds)
                  Exit For
                End If
              End If
            End If
          Next
        End If
      Next
      Return disaster_segments
    End Function

    Private Shared Sub UpdateRegularDisasterOverlap(ts As TimeSegment,
                                                    disaster As List(Of TimeSegment),
                                                    ByRef combined As List(Of TimeSegment))

      If ts.work_type = WorkType.BreakCredit Then Exit Sub

      For Each d In disaster
        ' if disaster hours overlap this chunk, reduce it.
        If ts.start_time < d.end_time AndAlso d.start_time < ts.end_time Then

          If d.start_time >= ts.start_time AndAlso d.end_time <= ts.end_time Then

            If d.start_time = ts.start_time Then

              If d.end_time = ts.end_time Then
                'combined.Add(ts) ' this segment already exists as disaster hours, no need to re-add it.
                Exit Sub
              Else
                ts.start_time = d.end_time
                ts.Update()
              End If

            Else
              ' d.start_time is greater than ts.start_time
              Dim new_ts = ts.Clone(d.end_time, ts.end_time)
              ts.end_time = d.start_time
              ts.Update()
              UpdateRegularDisasterOverlap(new_ts, disaster, combined)
            End If

          End If
        End If
      Next
      If ts.total_hours > 0 Then combined.Add(ts)

    End Sub

    Private Shared Function RefineRegularHours(regular As List(Of TimeSegment), disaster As List(Of TimeSegment)) As List(Of TimeSegment)
      Dim combined = New List(Of TimeSegment)
      combined.AddRange(disaster)

      For Each r In regular
        UpdateRegularDisasterOverlap(r, disaster, combined)
      Next
      ' After we've done this, let's figure out what to do with the break credit
      ' if there are no regular hours in combined, after we're done
      ' we'll check for the existence of a break credit hour timesegment in the regular hours
      ' if it's there, we'll figure out if it should have any pay rules associated with it
      ' and then put it into the combined section.
      ' we'll look to see which disaster timesegment's start time is the same
      ' as the break credit's end time, and use that timesegment's disaster pay rule.
      Dim bc = (From r In regular
                Where r.work_type = WorkType.BreakCredit
                Select r).ToList()
      If bc.Count() > 0 Then

        Dim break_credit = bc.First

        Dim total_regular = (From c In combined
                             Where c.work_type = WorkType.Regular
                             Select c).Count()
        If total_regular > 0 Then
          ' we'll add the break credit back in with no pay rule

          break_credit.disaster_pay_rule = -1
          combined.Add(break_credit)
        Else
          Dim d = (From c In combined
                   Where c.start_time = break_credit.end_time
                   Select c).ToList()

          If d.Count() > 0 Then
            break_credit.disaster_period_id = d.First.disaster_period_id
            ' now that we have the period, we can figure out the pay rule
            Dim rules = (From r In DisasterEventRules.Get_Cached_Disaster_Rules(break_credit.pay_period_start.AddDays(13))
                         Where r.period_id = break_credit.disaster_period_id
                         Select r).ToList

            For Each rule In rules
              If break_credit.start_time < rule.EndDateTime AndAlso rule.StartDateTime < break_credit.end_time Then
                If break_credit.start_time < rule.StartDateTime Then
                  Dim starttime As Date = rule.StartDateTime
                  Dim endtime As Date = break_credit.end_time
                  If endtime > rule.EndDateTime Then endtime = rule.EndDateTime

                  Dim ts As TimeSegment = break_credit.Clone(starttime, endtime)
                  ts.disaster_pay_rule = rule.pay_rule
                  combined.Add(ts)
                Else
                  If break_credit.end_time > rule.EndDateTime Then
                    Dim ts As TimeSegment = break_credit.Clone(break_credit.start_time, rule.EndDateTime)
                    ts.disaster_pay_rule = rule.pay_rule
                    combined.Add(ts)
                  Else
                    break_credit.disaster_pay_rule = rule.pay_rule
                    combined.Add(break_credit)
                    Exit For
                  End If
                End If
              End If
            Next
          Else
            ' this should be an error if it happens.
            break_credit.disaster_pay_rule = -1
            combined.Add(break_credit)
          End If

        End If
      End If

      Return combined
    End Function

    Public Shared Function CreateSpecific(WorkTimes As String,
                                          wt As WorkType,
                                          period_id As Integer,
                                          wd As Date,
                                          pps As Date,
                                          payrate As Double) As List(Of TimeSegment)
      Dim segments As New List(Of TimeSegment)

      Dim times As String() = WorkTimes.Split("-")

      Dim max As Integer = times.GetUpperBound(0)

      If (max + 1) Mod 2 = 1 Then max -= 1 '
      For i As Integer = 0 To max Step 2
        Try
          Dim s As TimeSegment = New TimeSegment(times(i), times(i + 1), wd, wt, pps, period_id, payrate)
          segments.Add(s)
        Catch ex As Exception
          Dim el As New ErrorLog(ex, "")
        End Try
      Next
      Return segments
    End Function

    Public Shared Function FindBreakCreditGap(segments As List(Of TimeSegment)) As Date?
      If segments.Count <= 1 Then Return Nothing
      Dim tmp = (From s In segments
                 Order By s.start_time Descending
                 Select s).ToArray
      For i As Integer = tmp.GetLowerBound(0) To tmp.GetUpperBound(0)
        If tmp(i).start_time.Subtract(tmp(i + 1).end_time).TotalHours >= 1 Then
          Return tmp(i).start_time.AddMinutes(-30)
        End If
      Next
      Return Nothing
    End Function

    Public Function Clone(starttime As Date, endtime As Date) As TimeSegment
      Dim ts As New TimeSegment(Me.start_time_raw,
                                Me.end_time_raw,
                                Me.work_date,
                                Me.work_type,
                                Me.pay_period_start,
                                Me.disaster_period_id,
                                Me.payrate)
      ts.disaster_pay_rule = Me.disaster_pay_rule
      ts.start_time = starttime
      ts.end_time = endtime
      ts.Update()
      Return ts
    End Function

  End Class

End Namespace

