Namespace Models
  Public Class Raw_Timecard_Access
    Public Property Access_Type As Integer
    Property EmployeeId As Integer
    Property ReportsTo As Integer
    Property RequiresApproval As Boolean
    Property BackendReportsAccess As Boolean
    Property DepartmentsToApprove As List(Of String)
    Property DataType As String
    Property CanChangeAccess As Boolean
  End Class
End Namespace