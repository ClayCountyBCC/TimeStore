Imports System.Data
Imports System.Data.SqlClient
Imports Dapper

Namespace Models

  Public Class PayrollData
    Property employee_id As Integer
    Property pay_period_ending As Date
    Property paycode As String
    Property payrate As Double
    Property project_code As String
    Property hours As Double
    Property amount As Double
    Property orgn As String
    Property classify As String
    Property changed_by As String = ""
    Property changed_on As Date?
    Property messages As New List(Of String)
    Property paycode_detail As Paycode
    Property compared As Boolean = False

    Public Sub New()
    End Sub

    Public Shared Function GetAllBasePayrollData(pay_period_ending As Date,
                                                 ByRef paycodes As Dictionary(Of String, Paycode))
      Dim dp As New DynamicParameters
      dp.Add("@pay_period_ending", pay_period_ending)
      Dim query As String = "
      SELECT
        employee_id
        ,pay_period_ending
        ,paycode
        ,payrate
        ,project_code
        ,hours
        ,amount
        ,orgn
        ,classify
        ,NULL changed_by
        ,NULL changed_on
      FROM Base_Payroll_Data
      WHERE
        pay_period_ending=@pay_period_ending
      ORDER BY orgn, employee_id"
      Dim data = Get_Data(Of PayrollData)(query, dp, ConnectionStringType.Timestore)
      For Each d In data
        If paycodes.ContainsKey(d.paycode) Then d.paycode_detail = paycodes(d.paycode)
      Next
      Return data
    End Function

    Public Shared Function GetAllPayrollChanges(pay_period_ending As Date,
                                                ByRef paycodes As Dictionary(Of String, Paycode))
      Dim dp As New DynamicParameters
      dp.Add("@pay_period_ending", pay_period_ending)
      Dim query As String = "
      SELECT
        employee_id
        ,pay_period_ending
        ,paycode
        ,payrate
        ,project_code
        ,hours
        ,amount
        ,orgn
        ,classify
        ,NULL changed_by
        ,NULL changed_on
      FROM Payroll_Changes
      WHERE
        pay_period_ending=@pay_period_ending
      ORDER BY orgn, employee_id"
      Dim data = Get_Data(Of PayrollData)(query, dp, ConnectionStringType.Timestore)
      If paycodes IsNot Nothing Then
        For Each d In data
          If paycodes.ContainsKey(d.paycode) Then d.paycode_detail = paycodes(d.paycode)
        Next
      End If
      Return data
    End Function

    Public Shared Function GetAllFinplusPayrates(pay_period_ending As Date,
                                                 ByRef paycodes As Dictionary(Of String, Paycode))
      Dim dp As New DynamicParameters
      dp.Add("@pay_period_ending", pay_period_ending)
      Dim query As String = "
      SELECT
        employee_id
        ,pay_period_ending
        ,paycode
        ,CASE WHEN rate_number != 1 THEN 0 ELSE payrate END payrate
        ,'' project_code
        ,pay_hours hours
        ,CASE WHEN rate_number != 1 THEN payrate ELSE CAST(payrate * pay_hours AS DECIMAL(12, 2)) END amount
        ,home_orgn orgn
        ,classify
        ,NULL changed_by
        ,NULL changed_on
      FROM Finplus_Payrates
      WHERE
        pay_period_ending=@pay_period_ending
      ORDER BY orgn, employee_id"
      Dim data = Get_Data(Of PayrollData)(query, dp, ConnectionStringType.Timestore)
      For Each d In data
        If paycodes.ContainsKey(d.paycode) Then d.paycode_detail = paycodes(d.paycode)
      Next
      Return data
    End Function

    Public Shared Function GetCheckPayInformation(employee_id As Integer,
                                                  check_number As String) As List(Of PayrollData)
      Dim dp As New DynamicParameters
      dp.Add("@employee_id", employee_id)
      dp.Add("@check_number", check_number)
      Dim query As String = "
        SELECT
          empl_no employee_id
          ,code paycode
          ,amt amount
          ,classify
          ,hours
          ,payrate
        FROM checkhi2
        wHERE
          earn_ded='P'
          AND check_no=@check_number
          AND empl_no=@employee_id
        ORDER BY empl_no, code;"
      Return Get_Data(Of PayrollData)(query, dp, ConnectionStringType.FinPlus)
    End Function


    Private Shared Function BuildDataTable() As DataTable
      Dim dt As New DataTable("Payroll_Changes_Data")
      'dt.Columns.Add("work_hours_id", Type.GetType("System.Int64"))
      dt.Columns.Add("employee_id", Type.GetType("System.Int32"))
      dt.Columns.Add("pay_period_ending", Type.GetType("System.DateTime"))
      dt.Columns.Add("paycode", Type.GetType("System.String"))
      dt.Columns.Add("payrate", Type.GetType("System.Decimal"))
      dt.Columns.Add("project_code", Type.GetType("System.String"))
      dt.Columns.Add("hours", Type.GetType("System.Double"))
      dt.Columns.Add("amount", Type.GetType("System.Double"))
      dt.Columns.Add("orgn", Type.GetType("System.String"))
      dt.Columns.Add("classify", Type.GetType("System.String"))
      Return dt
    End Function

    Public Shared Function SavePayrollChanges(pay_period_ending As Date, username As String, change_data As List(Of PayrollData)) As Boolean
      If change_data.Count = 0 Then Return False
      Dim employee_id As Integer = change_data.First.employee_id

      Dim dt As DataTable = BuildDataTable()
      For Each c In change_data
        dt.Rows.Add(c.employee_id, c.pay_period_ending, c.paycode, c.payrate, c.project_code, c.hours, c.amount, c.orgn, c.classify)
      Next
      Dim dp As New DynamicParameters
      dp.Add("@pay_period_ending", pay_period_ending)
      dp.Add("@username", username)
      dp.Add("@employee_id", employee_id)
      dp.Add("@PCD", dt.AsTableValuedParameter("Payroll_Changes_Data"))

      Dim query As String = "
        MERGE TimeStore.dbo.Payroll_Changes WITH (HOLDLOCK) AS PC

        USING @PCD AS PCD ON PC.employee_id = PCD.employee_id
          AND PC.pay_period_ending = PCD.pay_period_ending
          AND PC.paycode = PCD.paycode
          AND PC.payrate = PCD.payrate
          AND PC.project_code = PCD.project_code
      

        WHEN MATCHED THEN
          UPDATE
            SET 
              hours = PCD.hours
              ,amount = PCD.amount
              ,orgn = PCD.orgn
              ,classify = PCD.classify
              ,changed_by = @username
              ,changed_on=GETDATE()

        WHEN NOT MATCHED BY TARGET THEN
          INSERT (
            employee_id
            ,pay_period_ending
            ,paycode
            ,payrate
            ,project_code
            ,hours
            ,amount
            ,orgn
            ,classify
            ,changed_by
            ,changed_on
          )
          VALUES (
            PCD.employee_id
            ,@pay_period_ending
            ,PCD.paycode
            ,PCD.payrate
            ,PCD.project_code
            ,PCD.hours
            ,PCD.amount
            ,PCD.orgn
            ,PCD.classify
            ,@username
            ,GETDATE()
          )
        WHEN NOT MATCHED BY SOURCE 
          AND PC.pay_period_ending = @pay_period_ending
          AND PC.employee_id = @employee_id THEN
          DELETE;
"


      Try

        Using db As IDbConnection = New SqlConnection(GetCS(ConnectionStringType.Timestore))
          Dim i = db.Execute(query, dp)
          Return i > 0
        End Using
      Catch ex As Exception
        Dim e As New ErrorLog(ex, "Payroll Changes Data")
        Return False
      End Try
    End Function

  End Class

End Namespace