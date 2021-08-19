Imports System.Data.SqlClient
Imports Dapper

Namespace Models
  Public Class Saved_TimeStore_Data_To_Approve
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
          Return approved_by_employee_id > 0
        Else
          Return True
        End If
      End Get
    End Property
    'Property is_approved_value As Boolean
    Property approved_by_employee_id As Integer = 0
    Property approved_by_username As String = ""
    Property approved_by_machinename As String = ""
    Property approved_by_ip_address As String = ""
    Property note As String = ""
    Property date_approval_added As Date = Date.MaxValue

    Private Shared Function GetApprovalHoursQuery() As String
      Dim query As String = "
        SELECT
          W.employee_id, 
          W.work_date, 
          H.approval_hours_id, 
          H.work_hours_id, 
          H.field_id, 
          H.worktimes, 
          H.hours_used, 
          H.date_added, 
          H.is_approved, 
          H.by_employeeid approved_by_employee_id, 
          H.by_username approved_by_username, 
          H.by_machinename approved_by_machinename, 
          H.by_ip_address approved_by_ip_address, 
          H.note, 
          H.date_approval_added
        FROM Hours_To_Approve H 
        INNER JOIN Work_Hours W ON W.work_hours_id = H.work_hours_id 
        WHERE 1 = 1
          AND H.is_approved = 1
          AND H.hours_used > 0
"
      Return query ' Approval_Hours is a view.  
    End Function

    Public Sub New()

    End Sub

    Private Shared Function BuildDataTable() As DataTable
      Dim dt As New DataTable
      dt.Columns.Add("work_hours_id", Type.GetType("System.Int64"))
      dt.Columns.Add("field_id", Type.GetType("System.Int16"))
      dt.Columns.Add("work_times", Type.GetType("System.String"))
      dt.Columns.Add("hours_used", Type.GetType("System.Double"))
      Return dt
    End Function

    Private Shared Function PopulateData(work_hours_id As Long,
                                  tctd As TimecardTimeData) As DataTable
      Dim dt As DataTable = BuildDataTable()

      If tctd.VacationHours > 0 Then
        dt.Rows.Add(work_hours_id, 2, "", tctd.VacationHours)
      End If
      If tctd.SickHours > 0 Then
        dt.Rows.Add(work_hours_id, 3, "", tctd.SickHours)
      End If
      If tctd.CompTimeUsed > 0 Then
        dt.Rows.Add(work_hours_id, 4, "", tctd.CompTimeUsed)
      End If
      If tctd.AdminBereavement > 0 Then
        dt.Rows.Add(work_hours_id, 5, "", tctd.AdminBereavement)
      End If
      If tctd.AdminJuryDuty > 0 Then
        dt.Rows.Add(work_hours_id, 6, "", tctd.AdminJuryDuty)
      End If
      If tctd.AdminMilitaryLeave > 0 Then
        dt.Rows.Add(work_hours_id, 7, "", tctd.AdminMilitaryLeave)
      End If
      If tctd.AdminWorkersComp > 0 Then
        dt.Rows.Add(work_hours_id, 8, "", tctd.AdminWorkersComp)
      End If
      If tctd.AdminOther > 0 Then
        dt.Rows.Add(work_hours_id, 9, "", tctd.AdminOther)
      End If
      If tctd.OnCallMinimumHours > 0 Then
        dt.Rows.Add(work_hours_id, 10, "", tctd.OnCallMinimumHours)
      End If
      If tctd.OnCallWorkHours > 0 Then
        dt.Rows.Add(work_hours_id, 11, "", tctd.OnCallWorkHours)
      End If
      If tctd.OnCallTotalHours > 0 Or tctd.OnCallWorkTimes.Length > 0 Then
        dt.Rows.Add(work_hours_id, 13, tctd.OnCallWorkTimes, tctd.OnCallTotalHours)
      End If
      If tctd.SickLeavePoolHours > 0 Then
        dt.Rows.Add(work_hours_id, 14, "", tctd.SickLeavePoolHours)
      End If
      If tctd.LWOPSuspensionHours > 0 Then
        dt.Rows.Add(work_hours_id, 15, "", tctd.LWOPSuspensionHours)
      End If
      If tctd.ScheduledLWOPHours > 0 Then
        dt.Rows.Add(work_hours_id, 16, "", tctd.ScheduledLWOPHours)
      End If
      If tctd.SickFamilyLeave > 0 Then
        dt.Rows.Add(work_hours_id, 17, "", tctd.SickFamilyLeave)
      End If
      If tctd.TermHours > 0 Then
        dt.Rows.Add(work_hours_id, 18, "", tctd.TermHours)
      End If
      If tctd.AdminDisaster > 0 Then
        dt.Rows.Add(work_hours_id, 19, "", tctd.AdminDisaster)
      End If
      If tctd.AdminCovid > 0 Then
        dt.Rows.Add(work_hours_id, 20, "", tctd.AdminCovid)
      End If
      Return dt
    End Function

    Public Shared Function GetByEmployeeAndWorkday(WorkDate As Date, EmployeeID As Integer) As Saved_TimeStore_Data_To_Approve
      Dim dp As New DynamicParameters()
      dp.Add("@WorkDate", WorkDate)
      dp.Add("@EmployeeID", EmployeeID)
      Dim query As String = GetApprovalHoursQuery() + "
          AND W.employee_id = @EmployeeID
          AND W.work_date = @WorkDate
        ORDER BY W.work_date ASC, W.employee_id ASC"
      Dim l = Get_Data(Of Saved_TimeStore_Data_To_Approve)(query, dp, ConnectionStringType.Timestore)
      Return If(l.Count = 0, New Saved_TimeStore_Data_To_Approve, l.First)
    End Function

    Public Shared Function GetAllByDateRange(Start As Date, EndDate As Date) As List(Of Saved_TimeStore_Data_To_Approve)
      Dim dp As New DynamicParameters()
      dp.Add("@Start", Start)
      dp.Add("@End", EndDate)
      Dim query As String = GetApprovalHoursQuery() + "
          AND W.work_date BETWEEN @Start AND @End 
        ORDER BY W.work_date ASC, W.employee_id ASC"
      Return Get_Data(Of Saved_TimeStore_Data_To_Approve)(query, dp, ConnectionStringType.Timestore)
    End Function

    Public Shared Function GetByEmployeeAndDateRange(Start As Date, EndDate As Date, EmployeeID As Integer) As List(Of Saved_TimeStore_Data_To_Approve)
      Dim dp As New DynamicParameters()
      dp.Add("@Start", Start)
      dp.Add("@End", EndDate)
      dp.Add("@EmployeeID", EmployeeID)
      Dim query As String = GetApprovalHoursQuery() + "
          AND employee_id = @EmployeeID
          AND W.work_date BETWEEN @Start AND @End 
        ORDER BY W.work_date ASC, W.employee_id ASC"
      Return Get_Data(Of Saved_TimeStore_Data_To_Approve)(query, dp, ConnectionStringType.Timestore)
    End Function
  End Class
End Namespace