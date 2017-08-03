Namespace Models
  Public Class TimecardTimeException
    Public Property EmployeeId As Integer = 0
    Public Property EmployeeName As String = ""
    Public Property Department As String = ""
    Public Property ExceptionType As String = "" ' Warning Or Error
    Public Property Message As String = ""
    Public Sub New(eid As Integer, eName As String, eType As String, errorMessage As String, departmentId As String)
      EmployeeId = eid
      EmployeeName = eName
      ExceptionType = eType
      Message = errorMessage
      Department = departmentId
    End Sub

    Public Sub New(f As FinanceData, eType As String, errorMessage As String)
      EmployeeId = f.EmployeeId
      EmployeeName = f.EmployeeName
      Department = f.Department
      ExceptionType = eType
      Message = errorMessage
    End Sub
  End Class
End Namespace