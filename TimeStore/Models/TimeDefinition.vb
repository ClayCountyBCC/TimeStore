Namespace Models


  Public Class TimeDefinition
    Property WorkDate As Date
    Property Week As Integer
    Property StartTime As Date
    Property EndTime As Date
    Property ts As TimeSpan
    Property number_hours As Double
    Property disaster_period_id As Integer
    Property disaster_payrule As Integer
    Property payrate As Double

    Public Sub New(wd As Date,
                   starttime As String,
                   endtime As String,
                   dpr As List(Of DisasterEventRules),
                   week As Integer,
                   pr As Double)
      Me.payrate = pr
      Me.WorkDate = wd
      Me.StartTime = Date.Parse(WorkDate.ToShortDateString & " " & starttime.Trim)
      Me.EndTime = Date.Parse(WorkDate.ToShortDateString & " " & endtime.Trim)
      If Me.EndTime.Second = 59 Then Me.EndTime = Me.EndTime.AddSeconds(1)
      Me.ts = Me.EndTime.Subtract(Me.StartTime)
      Me.number_hours = Me.ts.TotalHours

    End Sub

    Public Shared Function FindBreakCreditGap(TCTD As TimecardTimeData) As Date
      ' This function tells us when the break credit should be applied
      ' This is important because when disaster pay rules are in effect,
      ' we have to know what value to associate the break credit with, in terms of
      ' Overtime, Regular Overtime, or Double Overtime.
      If TCTD.BreakCreditHours = 0 OrElse TCTD.WorkTimes.Length = 0 Then Return Nothing

      Dim times = TCTD.WorkTimes.Split("-")

      If times.Length = 1 Then Return Nothing

      Dim max As Integer = times.GetUpperBound(0)

      If (max + 1) Mod 2 = 1 Then max -= 1

      For i As Integer = max To 0 Step -2
        If i = 0 Then Exit For
        Dim st As Date = Date.Parse(TCTD.WorkDate.ToShortDateString & " " & times(i - 1))
        Dim et As Date = Date.Parse(TCTD.WorkDate.ToShortDateString & " " & times(i - 2))
        If st.Subtract(et).TotalHours >= 1 Then
          ' st.addMinutes(-30) will be when the break credit fits in.
          times(i - 1) = st.AddMinutes(-30).ToShortTimeString
          Exit For
        End If
      Next
      Return String.Join(" - ", times)
    End Function

    Public Function CreateByDay(t As TimecardTimeData, pr As Double, week As Integer) As List(Of TimeDefinition)
      ' Use the FixBreakCreditGap in this function.

    End Function

    Public Function SplitStart()

    End Function

    Public Function SplitEnd()

    End Function

  End Class

End Namespace
