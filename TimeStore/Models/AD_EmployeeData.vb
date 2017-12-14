Namespace Models
  Public Class AD_EmployeeData
    Property EmployeeID As Integer = 0
    Property Name As String = ""
    Property EmailAddress As String = ""
    Property Username As String = ""
    Property DatePasswordChanged As DateTime = DateTime.MaxValue

    Public Sub New(EID As Integer,
                   EmployeeName As String,
                   Email As String,
                   User As String,
                   PasswordDate As DateTime)

      EmployeeID = EID
      Name = EmployeeName
      EmailAddress = Email
      Username = User
      DatePasswordChanged = PasswordDate
    End Sub
  End Class
End Namespace