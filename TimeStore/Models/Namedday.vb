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
      bd(0) = New Date(Now.Year - 1, Birthday.Month, Birthday.Day)
      bd(1) = New Date(Now.Year, Birthday.Month, Birthday.Day)
      bd(2) = New Date(Now.Year + 1, Birthday.Month, Birthday.Day)
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