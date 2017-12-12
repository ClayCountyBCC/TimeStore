Imports System.Data.SqlClient
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
      Return dt
    End Function

    'Public Shared Function Save(work_hours_id As Integer, tctd As TimecardTimeData) As Boolean

    '  Dim dt As DataTable = PopulateData(work_hours_id, tctd)
    '  Dim dp As New DynamicParameters()
    '  dp.Add("@WorkHoursId", work_hours_id)

    '  Dim query As String = "
    '    MERGE TimeStore.dbo.Hours_To_Approve WITH (HOLDLOCK) AS HA

    '    USING @HTA AS HTA ON HA.work_hours_id=HTA.work_hours_id 
    '      AND HTA.field_id=HA.field_id

    '    WHEN MATCHED THEN
    '      UPDATE
    '        SET 
    '          worktimes=HTA.work_times,
    '          hours_used=HTA.hours_used,
    '          date_added=GETDATE()

    '    WHEN NOT MATCHED BY TARGET THEN
    '      INSERT (
    '        work_hours_id,
    '        field_id,
    '        worktimes,
    '        hours_used
    '      )
    '      VALUES (
    '        HTA.work_hours_id,
    '        HTA.field_id,
    '        HTA.work_times,
    '        HTA.hours_used
    '      )
    '    WHEN NOT MATCHED BY SOURCE AND HA.work_hours_id=@WorkHoursId THEN
    '      UPDATE 
    '        SET
    '          hours_used=0,
    '          date_added=GETDATE();"
    '  Try
    '    Using db As IDbConnection = New SqlConnection(GetCS(ConnectionStringType.Timestore))
    '      Dim i = db.Execute(query, New With {
    '                         .WorkHoursId = work_hours_id,
    '                         .HTA = dt
    '                         })
    '      Return True
    '    End Using
    '  Catch ex As Exception
    '    Dim e As New ErrorLog(ex, "")
    '    Return False
    '  End Try

    'End Function

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