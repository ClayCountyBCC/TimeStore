Namespace Models
  Public Class TimeStore_Work_Hours
    Property Work_Hours_ID As Integer = 0
    Property EmployeeID As Integer = 0
    Property DepartmentID As String = ""
    Property WorkDate As Date
    Property WorkTimes As String = ""
    Property WorkHours As Double = 0
    Property BreakCredit As Double = 0
    Property HolidayHours As Double = 0
    Property LWOP As Double = 0
    Property TotalHours As Double = 0
    Property Vehicle As Integer = 0
    Property Comment As String = ""
    Property DateAdded As Date = Date.MinValue
    Property Added_By_EmployeeID As Integer = 0
    Property Added_By_Username As String = ""
    Property Added_By_Machinename As String = ""
    Property Added_By_IP_Address As String = ""
    Property Hours_To_Approve As New List(Of TimeStore_Hours_To_Approve)
  End Class
End Namespace