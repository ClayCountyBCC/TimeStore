Imports Dapper

Namespace Models
  Public Class Saved_TimeStore_Data_To_Approve
    Property approval_id As Long = 0
    Property approval_hours_id As Long = 0
    Property work_hours_id As Long = 0
    Property field_id As Integer = 0
    Property worktimes As String = ""
    Property hours_used As Double = 0
    Property hours_approved As Double = 0
    Property date_added As Date = Date.MaxValue
    Public ReadOnly Property is_approved As Boolean
      Get
        If Get_TimeStore_Fields_By_ID()(field_id).Requires_Approval Then
          Return is_approved_value
        Else
          Return True
        End If
      End Get
    End Property
    Property is_approved_value As Boolean
    Property approved_by_employee_id As Integer = 0
    Property approved_by_username As String = ""
    Property approved_by_machinename As String = ""
    Property approved_by_ip_address As String = ""
    Property note As String = ""
    Property date_approval_added As Date = Date.MaxValue

    Private Shared Function GetApprovalHoursQuery() As String
      Dim query As String = "
        USE TimeStore;
        SELECT 
          approval_hours_id,
          work_hours_id,
          field_id,
          worktimes,
          hours_used, 
          date_added,
          approval_id,
          hours_approved,
          is_approved is_approved_value,
          by_employeeid, 
          by_username,
          by_machinename,
          by_ip_address,
          note,
          date_approval_added 
        FROM Approval_Hours"
      Return query ' Approval_Hours is a view.  
    End Function

    Public Sub New()

    End Sub

    Public Shared Function GetByEmployeeAndWorkday(WorkDate As Date, EmployeeID As Integer) As Saved_TimeStore_Data_To_Approve
      Dim dp As New DynamicParameters()
      dp.Add("@WorkDate", WorkDate)
      dp.Add("@EmployeeID", EmployeeID)
      Dim query As String = GetApprovalHoursQuery() + "
        WHERE 
          employee_id = @EmployeeID
          AND work_date = @WorkDate
        ORDER BY work_date ASC, employee_id ASC"
      Dim l = Get_Data(Of Saved_TimeStore_Data_To_Approve)(query, dp, ConnectionStringType.Timestore)
      Return If(l.Count = 0, New Saved_TimeStore_Data_To_Approve, l.First)
    End Function

    Public Shared Function GetAllByDateRange(Start As Date, EndDate As Date) As List(Of Saved_TimeStore_Data_To_Approve)
      Dim dp As New DynamicParameters()
      dp.Add("@Start", Start)
      dp.Add("@End", EndDate)
      Dim query As String = GetApprovalHoursQuery() + "
        WHERE 
          work_date BETWEEN @Start AND @End 
        ORDER BY work_date ASC, employee_id ASC"
      Return Get_Data(Of Saved_TimeStore_Data_To_Approve)(query, dp, ConnectionStringType.Timestore)
    End Function

    Public Shared Function GetByEmployeeAndDateRange(Start As Date, EndDate As Date, EmployeeID As Integer) As List(Of Saved_TimeStore_Data_To_Approve)
      Dim dp As New DynamicParameters()
      dp.Add("@Start", Start)
      dp.Add("@End", EndDate)
      dp.Add("@EmployeeID", EmployeeID)
      Dim query As String = GetApprovalHoursQuery() + "
        WHERE 
          employee_id = @EmployeeID
          AND work_date BETWEEN @Start AND @End 
        ORDER BY work_date ASC, employee_id ASC"
      Return Get_Data(Of Saved_TimeStore_Data_To_Approve)(query, dp, ConnectionStringType.Timestore)
    End Function
  End Class
End Namespace