Namespace Models
  Public Class EmployeeOutput
    Property EmployeeName As String = ""
    Property EmployeeId As Integer = 0
    Property ProfileType As Integer = 0
    Property Department As String = ""
    Property ScheduledHours As Double = 0
    Property HoursNeededForOvertime As Double = 0
    Property HireDate As Date = Date.MinValue
    Property FinPlusPayrate As Double = 0
    Property TelestaffPayrate As Double = 0
    Property Banked_Vacation As Double = 0
    Property Banked_Holiday As Double = 0
    Property Banked_Sick As Double = 0
    Property Banked_Comp As Double = 0
    Property Regular As Double = 0
    Property Scheduled_Overtime As Double = 0
    Property Term_Hours As Double = 0
    Property Absent_Without_Pay As Double = 0
    Property Vacation As Double = 0
    Property Sick As Double = 0
    Property Scheduled_Regular_Overtime As Double = 0
    Property Holiday As Double = 0
    Property Unscheduled_Regular_Overtime As Double = 0
    Property Unscheduled_Overtime As Double = 0
    Property Unscheduled_Double_Overtime As Double = 0
    Property Holiday_Time_Banked As Double = 0
    Property Holiday_Time_Used As Double = 0
    Property Comp_Time_Banked As Double = 0
    Property Comp_Time_Used As Double = 0
    Property StaffEmployee As Boolean = False
  End Class
End Namespace