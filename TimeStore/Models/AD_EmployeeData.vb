Namespace Models
  Public Class AD_EmployeeData
    Property EmployeeID As Integer = 0
    Property Name As String = ""
    Property EmailAddress As String = ""
    Property Username As String = ""

    Public Sub New(EID As Integer, EmployeeName As String, Email As String, User As String)
      EmployeeID = EID
      Name = EmployeeName
      EmailAddress = Email
      Username = User
    End Sub
  End Class
End Namespace