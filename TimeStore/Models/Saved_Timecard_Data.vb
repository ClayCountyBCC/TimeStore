Namespace Models
  Public Class Saved_Timecard_Data
    Public Property EmployeeId As Integer = 0
    Public Property PayPeriodEnding As Date = Date.MinValue
    Public Property Approved As Integer = 0
    Public Property Hours As Double = 0
    Public Property PayCode As String = ""
    Public Property PayRate As Double = 0
    Public Property ProjectCode As String = ""
    Public Property DepartmentNumber As String = ""
    Public Property Classify As String = ""
    Public Property DataType As String = "telestaff"
    Public Property AccessType As Integer = 1
    Public Property ReportsTo As Integer = 0
    Public Property Initial_Approval_By_EmployeeID As Integer = 0
    Public Property Initial_Approval_By_Name As String = ""
    Public Property Initial_Approval_Date As Date = Date.MinValue
    Public Property Initial_Approval_EmployeeID_AccessType As Integer = 1
    Public Property Final_Approval_By_EmployeeID As Integer = 0
    Public Property Final_Approval_By_Name As String = ""
    Public Property Final_Approval_Date As Date = Date.MinValue
  End Class
End Namespace