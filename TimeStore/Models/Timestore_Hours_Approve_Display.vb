Namespace Models
  Public Class TimeStore_Approve_Hours_Display
    Property access_type As Integer = 0
    Property reports_to As Integer = 0
    Property dept_id As String = ""
    Property employee_id As Integer = 0
    Property employee_name As String = ""
    Property work_date As Date = Date.MaxValue
    Property pay_period_ending As Date = Date.MaxValue
    Property field As Timestore_Field = Nothing
    Property hours_used As Double = 0
    Property work_times As String = ""
    Property hours_approved As Double = 0
    Property approval_hours_id As Long = 0
    Property approval_id As Long = 0
    Property is_approved As Integer = 0
    Property payrate As Double?
    Property note As String = ""
    Property comment As String = ""
    Property date_approval_added As Date = Date.MaxValue
    Property by_employeeid As Integer = 0
    Property by_username As String = ""
    Property by_machinename As String = ""
    Property by_ip_address As String = ""
    ReadOnly Property Approved As Boolean
      Get
        Return is_approved = 1
      End Get
    End Property

    Public Sub New(dr As DataRow, EmployeeName As String)
      If EmployeeName.Length = 0 Then
        Dim eid As Integer = dr("employee_id")
        EmployeeName = GetEmployeeDataFromFinPlus(eid).First.EmployeeName
      End If
      Load(dr, EmployeeName)
    End Sub

    Private Sub Load(dr As DataRow, EmployeeName As String)
      Try
        employee_name = EmployeeName
        employee_id = dr("employee_id")
        access_type = dr("access_type")
        reports_to = dr("reports_to")
        dept_id = dr("dept_id")
        pay_period_ending = dr("pay_period_ending")
        work_date = dr("work_date")
        field = Get_TimeStore_Fields_By_ID()(dr("field_id"))
        hours_used = dr("hours_used")
        work_times = dr("worktimes")
        comment = dr("comment")
        If Not IsDBNull(dr("payrate")) Then
          payrate = dr("payrate")
        End If
        approval_hours_id = dr("approval_hours_id")
        If Not IsDBNull(dr("approval_id")) Then
          approval_id = dr("approval_id")
          hours_approved = dr("hours_approved")
          is_approved = dr("is_approved")
          note = dr("note")
          date_approval_added = dr("date_approval_added")
          by_employeeid = dr("by_employeeid")
          by_username = dr("by_username")
          by_machinename = dr("by_machinename")
          by_ip_address = dr("by_ip_address")
        End If

      Catch ex As Exception
        Log(ex)
      End Try
    End Sub
  End Class
End Namespace