Namespace Models
  Public Class GroupedHours
    Public Week1 As New List(Of TelestaffTimeData)
    Public Week2 As New List(Of TelestaffTimeData)
    Private PayPeriodStart As Date = Date.MinValue
    Public Property PayCode As String = "000"

    Public Function ProjectCodes() As List(Of String)
      Dim codes As New List(Of String)
      codes.AddRange(ProjectCodes_Week1())
      codes.AddRange(ProjectCodes_Week2())
      Return codes.Distinct.ToList()
    End Function

    Public Function ProjectCodes_Week1() As List(Of String)
      Return (From w In Week1
              Select w.Finplus_Project_Code).Distinct.ToList()
    End Function

    Public Function ProjectCodes_Week2() As List(Of String)
      Return (From w In Week2
              Select w.Finplus_Project_Code).Distinct.ToList()
    End Function

    Public Function TotalHours() As Double
      Return TotalHours_Week1() + TotalHours_Week2()
    End Function

    Public Function TotalHours(Payrate As Double) As Double ' By Payrate
      Return TotalHours_Week1(Payrate) + TotalHours_Week2(Payrate)
    End Function

    Public Function TotalHours(Payrate As Double, projectCode As String) As Double ' By Payrate
      Return TotalHours_Week1(Payrate, projectCode) + TotalHours_Week2(Payrate, projectCode)
    End Function

    Public Function TotalHours_Week1() As Double
      Return (From w In Week1 Select w.WorkHours).Sum
    End Function

    Public Function TotalHours_Week2() As Double
      Return (From w In Week2 Select w.WorkHours).Sum
    End Function

    Public Function TotalHours_Week1(Payrate As Double) As Double
      Return (From w In Week1
              Where w.PayRate = Payrate
              Select w.WorkHours).Sum
    End Function

    Public Function TotalHours_Week2(Payrate As Double) As Double
      Return (From w In Week2
              Where w.PayRate = Payrate
              Select w.WorkHours).Sum
    End Function

    Public Function TotalHours_Week1(Payrate As Double, projectCode As String) As Double
      Return (From w In Week1
              Where w.PayRate = Payrate And
                w.Finplus_Project_Code = projectCode
              Select w.WorkHours).Sum
    End Function

    Public Function TotalHours_Week2(Payrate As Double, projectCode As String) As Double
      Return (From w In Week2
              Where w.PayRate = Payrate And
                w.Finplus_Project_Code = projectCode
              Select w.WorkHours).Sum
    End Function

    Public Sub Add(T As TelestaffTimeData)
      If PayPeriodStart = Date.MinValue Then PayPeriodStart = GetPayPeriodStart(T.WorkDate)
      ' This function is going to put the data into the right week
      If T.WorkDate < PayPeriodStart.AddDays(7) Then ' Week 1
        Week1.Add(T)
      Else ' Week 2
        Week2.Add(T)
      End If
    End Sub

    Public Sub Move_Day(NumberHours As Double,
                        WorkDate As Date,
                        ByRef Week As List(Of TelestaffTimeData),
                        ByRef Target As GroupedHours,
                        ByRef TL As List(Of TelestaffTimeData))
      ' We should never be trying to move more hours than are currently in this group
      If NumberHours <= 0 Then Exit Sub
      Dim HoursLeft As Double = NumberHours
      For Each w In (From t In Week
                     Where t.WorkDate = WorkDate
                     Select t)

        Dim clone = w.Clone

        If w.WorkHours >= HoursLeft Then
          clone.WorkHours = HoursLeft
          w.WorkHours -= HoursLeft
          TL.Add(clone)
          HoursLeft = 0
        Else
          HoursLeft = HoursLeft - w.WorkHours
          w.WorkHours = 0
        End If
        Target.Add(clone)
        If HoursLeft <= 0 Then Exit For
      Next
    End Sub

    Public Sub Move_First(NumberHours As Double, ByRef Target As GroupedHours, ByRef TL As List(Of TelestaffTimeData))
      ' Move_First is different than Move_Last in that it tries to move stuff from the beginning of the pay period to the end.
      ' We should never be trying to move more hours than are currently in this group
      If NumberHours <= 0 Then Exit Sub
      If NumberHours >= TotalHours() Then
        ' move all of the hours
        For Each w In Week2
          Target.Add(w)
        Next
        For Each w In Week1
          Target.Add(w)
        Next
        Week2.Clear()
        Week1.Clear()
      Else
        Dim x As Double = NumberHours
        Dim foundIndexes As New List(Of Integer)
        If Week1.Count > 0 Then
          For j As Integer = 0 To (Week1.Count - 1)
            If Week1(j).WorkHours <= x Then
              x = x - Week1(j).WorkHours
              Target.Add(Week1(j))
              foundIndexes.Insert(0, j)
              'Week1.RemoveAt(j)
            Else
              Dim c As TelestaffTimeData = Week1(j).Clone
              c.WorkHours = x
              Target.Add(c)
              TL.Add(c)
              Week1(j).WorkHours = Week1(j).WorkHours - x
              x = 0
            End If
            If x = 0 Then Exit For
          Next
          For Each j In foundIndexes
            Week1.RemoveAt(j)
          Next
          foundIndexes.Clear()
          'Dim j As Integer = Week1.Count - 1
          'Do While j >= 0
          '    j = j - 1
          'Loop
        End If
        If x = 0 Then Exit Sub
        If Week2.Count > 0 Then
          For j As Integer = 0 To Week2.Count - 1
            If Week2(j).WorkHours <= x Then
              x = x - Week2(j).WorkHours
              Target.Add(Week2(j))
              foundIndexes.Insert(0, j)
              'Week2.RemoveAt(j)
            Else
              Dim c As TelestaffTimeData = Week2(j).Clone
              c.WorkHours = x
              Target.Add(c)
              TL.Add(c)
              Week2(j).WorkHours = Week2(j).WorkHours - x
              x = 0
            End If
            If x = 0 Then Exit For
          Next
          For Each j In foundIndexes
            Week2.RemoveAt(j)
          Next
          foundIndexes.Clear()
          'Dim j As Integer = Week2.Count - 1
          'Do While j >= 0
          '    If x = 0 Then Exit Do
          '    j = j - 1
          'Loop
        End If
      End If

    End Sub

    Public Sub Move_Last(NumberHours As Double, ByRef Target As GroupedHours, ByRef TL As List(Of TelestaffTimeData))
      ' Move_Last is different than Move_First in that it tries to move stuff from the end of the pay period to the beginning.
      ' We should never be trying to move more hours than are currently in this group
      If NumberHours <= 0 Then Exit Sub
      If NumberHours >= TotalHours() Then
        ' move all of the hours
        For Each w In Week2
          Target.Add(w)
        Next
        For Each w In Week1
          Target.Add(w)
        Next
        Week2.Clear()
        Week1.Clear()
      Else
        Dim x As Double = NumberHours
        Dim foundIndexes As New List(Of Integer)

        If Week2.Count > 0 Then
          For j As Integer = Week2.Count - 1 To 0 Step -1
            If Week2(j).WorkHours <= x Then
              x = x - Week2(j).WorkHours
              Target.Add(Week2(j))
              foundIndexes.Insert(0, j)
            Else
              Dim c As TelestaffTimeData = Week2(j).Clone
              c.WorkHours = x
              Target.Add(c)
              TL.Add(c)
              Week2(j).WorkHours = Week2(j).WorkHours - x
              x = 0
            End If
            If x = 0 Then Exit For
          Next
          If foundIndexes.Count > 0 Then
            For j = foundIndexes.Count - 1 To 0 Step -1 'foundIndexes
              Week2.RemoveAt(foundIndexes(j))
            Next
          End If
          foundIndexes.Clear()
        End If


        If x = 0 Then Exit Sub

        If Week1.Count > 0 Then
          For j As Integer = Week1.Count - 1 To 0 Step -1
            'For j As Integer = 0 To (Week1.Count - 1)
            If Week1(j).WorkHours <= x Then
              x = x - Week1(j).WorkHours
              Target.Add(Week1(j))
              foundIndexes.Insert(0, j)
            Else
              Dim c As TelestaffTimeData = Week1(j).Clone
              c.WorkHours = x
              Target.Add(c)
              TL.Add(c)
              Week1(j).WorkHours = Week1(j).WorkHours - x
              x = 0
            End If
            If x = 0 Then Exit For
          Next
          'For Each j In foundIndexes
          '    Week1.RemoveAt(j)
          'Next
          If foundIndexes.Count > 0 Then
            For j = foundIndexes.Count - 1 To 0 Step -1 'foundIndexes
              Week1.RemoveAt(foundIndexes(j))
            Next
          End If
          foundIndexes.Clear()
        End If
      End If

    End Sub

    Public Sub Move_Week1(NumberHours As Double, ByRef Target As GroupedHours, ByRef TL As List(Of TelestaffTimeData))
      ' We should never be trying to move more hours than are currently in this group
      If NumberHours <= 0 Then Exit Sub
      If NumberHours >= TotalHours_Week1() Then
        ' move all of the hours
        For Each w In Week1
          Target.Add(w)
        Next
        Week1.Clear()
      Else
        Dim x As Double = NumberHours
        Dim foundIndexes As New List(Of Integer)
        If Week1.Count > 0 Then
          'For j As Integer = 0 To Week1.Count - 1
          ' Changed 3/11 to work from end to beginning.
          For j As Integer = Week1.Count - 1 To 0 Step -1
            If Week1(j).WorkHours <= x Then
              x = x - Week1(j).WorkHours
              Target.Add(Week1(j))
              foundIndexes.Insert(0, j)
              'Week1.RemoveAt(j)
            Else
              Dim c As TelestaffTimeData = Week1(j).Clone
              c.WorkHours = x
              Target.Add(c)
              TL.Add(c)
              Week1(j).WorkHours = Week1(j).WorkHours - x
              x = 0
            End If
            If x = 0 Then Exit For
          Next
          'For Each j In foundIndexes
          '    Week1.RemoveAt(j)
          'Next
          If foundIndexes.Count > 0 Then
            For j = foundIndexes.Count - 1 To 0 Step -1 'foundIndexes
              Week1.RemoveAt(foundIndexes(j))
            Next
          End If
          foundIndexes.Clear()
        End If
      End If
    End Sub

    Public Sub Move_Week2(NumberHours As Double, ByRef Target As GroupedHours, ByRef TL As List(Of TelestaffTimeData))
      ' We should never be trying to move more hours than are currently in this group
      If NumberHours <= 0 Then Exit Sub
      If NumberHours >= TotalHours_Week2() Then
        ' move all of the hours
        For Each w In Week2
          Target.Add(w)
        Next
        Week2.Clear()
      Else
        Dim x As Double = NumberHours
        Dim foundIndexes As New List(Of Integer)
        If Week2.Count > 0 Then
          'For j As Integer = 0 To Week2.Count - 1
          ' Changed 3/11 to work from end to beginning.
          For j As Integer = Week2.Count - 1 To 0 Step -1
            If Week2(j).WorkHours <= x Then
              x = x - Week2(j).WorkHours
              Target.Add(Week2(j))
              foundIndexes.Insert(0, j)
              'Week2.RemoveAt(j)
            Else
              Dim c As TelestaffTimeData = Week2(j).Clone
              c.WorkHours = x
              Target.Add(c)
              TL.Add(c)
              Week2(j).WorkHours = Week2(j).WorkHours - x
              x = 0
            End If
            If x = 0 Then Exit For
          Next
          'For Each j In foundIndexes
          '    Week2.RemoveAt(j)
          'Next
          If foundIndexes.Count > 0 Then
            For j = foundIndexes.Count - 1 To 0 Step -1 'foundIndexes
              Week2.RemoveAt(foundIndexes(j))
            Next
            foundIndexes.Clear()
          End If
        End If
      End If
    End Sub

  End Class
End Namespace