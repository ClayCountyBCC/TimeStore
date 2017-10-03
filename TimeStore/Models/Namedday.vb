Namespace Models
  Public Class Namedday
    Property Dept As String = ""
    Property Name As String = ""
    Property NamedDate As Date = Date.MinValue

    Public Sub New(EmployeeName As String, EmployeeDate As Date, EmployeeDept As String)
      Name = EmployeeName
      NamedDate = EmployeeDate
      Dept = EmployeeDept
    End Sub

    Private Function GetCurrentBirthdays(Birthday As Date) As Date()
      Dim bd(2) As Date
      Try
        Dim m As Integer = Birthday.Month
        Dim d As Integer = Birthday.Day
        If m = 2 And d = 29 Then d = 28
        bd(0) = New Date(Now.Year - 1, m, d)
        bd(1) = New Date(Now.Year, m, d)
        bd(2) = New Date(Now.Year + 1, m, d)
      Catch ex As Exception
        Dim e As New ErrorLog(ex, "")
        Return Nothing
      End Try

      Return bd
    End Function

    Public Function ToList() As List(Of Namedday)
      Dim bd() As Date = GetCurrentBirthdays(NamedDate)
      Dim bl As New List(Of Namedday)
      For a As Integer = bd.GetLowerBound(0) To bd.GetUpperBound(0)
        bl.Add(New Namedday(Name, bd(a), Dept))
      Next
      Return bl
    End Function
  End Class
End Namespace