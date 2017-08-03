Namespace Models


  Public Class Saved_TimeStore_Data
    Property work_hours_id As Long = 0
    Property employee_id As Integer = 0
    Property dept_id As String = ""
    Property pay_period_ending As Date = Date.MaxValue
    Property work_date As Date = Date.MaxValue
    Property work_times As String = ""
    Property break_credit As Double = 0
    Property work_hours As Double = 0
    Property holiday As Double = 0
    Property leave_without_pay As Double = 0
    Property total_hours As Double = 0
    Property doubletime_hours As Double = 0
    Property vehicle As Integer = 0
    Property comment As String = ""
    Property by_employeeid As Integer = 0
    Property by_username As String = ""
    Property by_machinename As String = ""
    Property by_ip_address As String = ""
    Property date_updated As Date = Date.MaxValue
    Property HoursToApprove As New List(Of Saved_TimeStore_Data_To_Approve)

    Public Sub New(dr As DataRow)
      Load(dr)
    End Sub

    Public Sub New(EmployeeID As Integer, WorkDate As Date)
      PopulateWorkHoursData(EmployeeID, WorkDate)
      If employee_id = 0 Then Exit Sub
      PopulateHoursToApproveData()
    End Sub

    Private Sub PopulateWorkHoursData(EmployeeID As Integer, WorkDate As Date)
      Dim dbc As New Tools.DB(GetCS(ConnectionStringType.Timestore), toolsAppId, toolsDBError)
      Dim sbQ As New StringBuilder
      With sbQ
        .Append("SELECT work_hours_id,employee_id,dept_id,pay_period_ending,work_date,")
        .Append("work_times,break_credit,work_hours,holiday,leave_without_pay,total_hours,")
        .Append("vehicle,comment,date_added,date_last_updated,by_employeeid,by_username")
        .Append(",by_machinename,by_ip_address,doubletime_hours ")
        .Append("FROM Work_Hours WHERE employee_id=")
        .Append(EmployeeID.ToString)
        .Append(" AND work_date='").Append(WorkDate.ToShortDateString)
        .Append("';")
      End With
      Dim dsTmp As DataSet
      Try
        dsTmp = dbc.Get_Dataset(sbQ.ToString)
        If dsTmp.Tables(0).Rows.Count = 0 Then
          Exit Sub
        Else
          Load(dsTmp.Tables(0).Rows(0))
        End If

      Catch ex As Exception
        Log(ex)
      End Try
    End Sub

    Private Sub PopulateHoursToApproveData()
      Dim dbc As New Tools.DB(GetCS(ConnectionStringType.Timestore), toolsAppId, toolsDBError)
      Dim sbQ As New StringBuilder
      With sbQ
        .Append("SELECT H.approval_hours_id,H.work_hours_id,H.field_id,H.worktimes,H.hours_used, ")
        .Append("H.payrate,H.date_added,A.approval_id,A.hours_approved,A.is_approved,A.by_employeeid, ")
        .Append("A.by_username,A.by_machinename,A.by_ip_address,A.note,A.date_approval_added ")
        .Append("FROM Hours_To_Approve H ")
        .Append("LEFT OUTER JOIN Approval_Data A ON H.approval_hours_id = A.approval_hours_id ")
        .Append("WHERE (A.is_approved IS NULL OR A.is_approved = 1) AND H.work_hours_id=")
        .Append(work_hours_id.ToString).Append(";")
      End With
      Dim dsTmp As DataSet
      Try
        dsTmp = dbc.Get_Dataset(sbQ.ToString)
        If dsTmp.Tables(0).Rows.Count = 0 Then
          Exit Sub
        Else
          For Each dr In dsTmp.Tables(0).Rows
            HoursToApprove.Add(New Saved_TimeStore_Data_To_Approve(dr))
          Next
        End If

      Catch ex As Exception
        Log(ex)
      End Try
    End Sub

    Private Sub Load(dr As DataRow)
      Try
        work_hours_id = dr("work_hours_id")
        employee_id = dr("employee_id")
        dept_id = dr("dept_id")
        pay_period_ending = dr("pay_period_ending")
        work_date = dr("work_date")
        work_times = dr("work_times")
        break_credit = dr("break_credit")
        work_hours = dr("work_hours")
        holiday = dr("holiday")
        leave_without_pay = dr("leave_without_pay")
        total_hours = dr("total_hours")
        doubletime_hours = dr("doubletime_hours")
        vehicle = dr("vehicle")
        comment = dr("comment")
        by_employeeid = dr("by_employeeid")
        by_username = dr("by_username")
        by_machinename = dr("by_machinename")
        by_ip_address = dr("by_ip_address")
        date_updated = dr("date_last_updated")
      Catch ex As Exception
        Log(ex)
      End Try
    End Sub
  End Class

End Namespace