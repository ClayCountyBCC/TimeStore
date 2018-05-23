Namespace Models
  Public Class Hours_To_Approve_Display
    Private Property _f As Timestore_Field = Nothing
    Property access_type As Integer = 0
    Property reports_to As Integer = 0
    Property dept_id As String = ""
    Property employee_id As Integer = 0
    Property employee_name As String = ""
    Property work_date As Date = Date.MaxValue
    ReadOnly Property work_date_display As String
      Get
        If work_date = Date.MaxValue Then
          Return ""
        Else
          Return work_date.ToShortDateString
        End If
      End Get
    End Property
    Property pay_period_ending As Date = Date.MaxValue
    Property field_id As Integer = 0
    Property field As Timestore_Field = Nothing
    Property hours_used As Double = 0
    Property work_times As String = ""
    Property hours_approved As Double = 0
    Property approval_hours_id As Long = 0
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
    ReadOnly Property Finalized As Boolean
      Get
        Return by_employeeid > 0
      End Get
    End Property

    Public Shared Function GetHoursToApproveForDisplay(StartDate As Date,
                                                       Optional EmployeeID As Integer = 0,
                                                       Optional DeptId As String = "",
                                                       Optional IncludeReportsTo As Boolean = False) As List(Of Hours_To_Approve_Display)

      Dim dp As New DynamicParameters
      dp.Add("@Start", StartDate)

      If DeptId.Length > 0 Then
        dp.Add("@DeptId", DeptId)
      End If
      If EmployeeID > 0 Then
        dp.Add("@EmployeeId", EmployeeID)
      End If

      Dim query As String = $"
        SELECT 
          ISNULL(A.access_type, 1) AS access_type, 
          ISNULL(A.reports_to, 0) AS reports_to, 
          W.dept_id, 
          W.employee_id, 
          W.work_date, 
          W.pay_period_ending, 
          W.comment, 
          H.field_id, 
          H.hours_used, 
          H.worktimes, 
          H.payrate, 
          H.approval_hours_id, 
          H.date_added, 
          H.is_approved, 
          H.date_approval_added, 
          H.by_employeeid, 
          H.by_username, 
          H.by_machinename, 
          H.by_ip_address, 
          H.note
        FROM Work_Hours W 
        INNER JOIN Hours_To_Approve H ON W.work_hours_id = H.work_hours_id 
        INNER JOIN Timestore_Fields TF ON H.field_id = TF.field_id 
        LEFT OUTER JOIN Access AS A ON W.employee_id = A.employee_id
        WHERE 
          1=1
          AND TF.enabled = 1
          AND TF.requires_approval = 1
          AND H.hours_used > 0
          AND work_date >= @Start
          { If(DeptId.Length > 0,
              If(IncludeReportsTo And EmployeeID > 0,
                " AND (W.dept_id=@DeptId OR A.reports_to=@EmployeeId) ",
                " AND W.dept_id=@DeptId "),
            "") }
          { If(EmployeeID > 0 And DeptId.Length = 0, " AND W.employee_id=@EmployeeId ", "") }
          ORDER BY dept_id, employee_id, work_date;"
      Try
        Dim ld = Get_Data(Of Hours_To_Approve_Display)(query, dp, ConnectionStringType.Timestore)
        Dim aded As Dictionary(Of Integer, AD_EmployeeData) = GetADEmployeeData()
        Dim fl = GetCachedEmployeeDataFromFinplus()
        Dim tsf As Dictionary(Of Integer, Timestore_Field) = Get_TimeStore_Fields_By_ID()
        For Each l In ld
          If aded.ContainsKey(l.employee_id) Then
            l.employee_name = aded(l.employee_id).Name
          Else
            Dim found = (From f In fl
                         Where f.EmployeeId = l.employee_id
                         Select f.EmployeeName).ToList()
            If found.Count > 0 Then
              l.employee_name = found.First
            Else
              l.employee_name = "Employee # " & l.employee_id.ToString
            End If
          End If

          l.field = tsf(l.field_id)
        Next
        Return ld.OrderBy(Function(x) x.dept_id).ThenBy(Function(x) x.employee_name).ToList
      Catch ex As Exception
        Log(ex)
        Return Nothing
      End Try
    End Function


  End Class
End Namespace