Namespace Models



  Public Class PayrollChangeComparison
    'Property pay_period_ending As Date
    'Property employee_id As Integer
    Private _has_changed As Boolean = False
    ReadOnly Property has_changed As Boolean
      Get
        Return _has_changed
      End Get
    End Property
    Property status As String = "Same"
    Property original As PayrollData = Nothing
    Property changed As PayrollData = Nothing

    Public Sub New()
    End Sub

    'PayPeriodEnding As Date, EmployeeId As Integer, 
    Public Sub New(Orig As PayrollData,
                   Change As PayrollData)
      'Me.pay_period_ending = PayPeriodEnding
      'Me.employee_id = EmployeeId
      'Me.status = Status
      Me.original = Orig
      Me.changed = Change
      UpdateStatus()
    End Sub

    Public Shared Function CreateComparisons(Original As List(Of PayrollData),
                                             Changed As List(Of PayrollData)) As List(Of PayrollChangeComparison)
      Dim comparisons As New List(Of PayrollChangeComparison)

      Original.Take(1)

      Do While Original.Count() > 0
        Dim o As PayrollData = Original.First
        o.compared = True

        Dim found_changed = (From c In Changed
                             Where c.paycode = o.paycode
                             Order By c.payrate Ascending
                             Select c)

        If found_changed.Count = 0 Then
          comparisons.Add(New PayrollChangeComparison(o, Nothing))
        Else
          Dim f = found_changed.First
          comparisons.Add(New PayrollChangeComparison(o, f))
          Changed.Remove(f)
        End If
        Original.Remove(o)
      Loop
      Do While Changed.Count() > 0
        Dim c As PayrollData = Changed.First
        c.compared = True
        comparisons.Add(New PayrollChangeComparison(Nothing, c))
        Changed.Remove(c)
      Loop
      Return comparisons
    End Function

    Private Sub UpdateStatus()
      If original Is Nothing Then
        status = "New"
        _has_changed = True
        Exit Sub
      End If
      If changed Is Nothing Then
        status = "Removed"
        _has_changed = True
        Exit Sub
      End If
      If original.payrate <> changed.payrate OrElse
        original.hours <> changed.hours OrElse
        original.amount <> changed.amount OrElse
        original.project_code <> changed.project_code OrElse
        original.classify <> changed.classify Then
        status = "Changed"
        _has_changed = True
      End If
    End Sub
  End Class
End Namespace