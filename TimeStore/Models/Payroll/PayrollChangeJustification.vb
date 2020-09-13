Namespace Models

  Public Class PayrollChangeJustification
    Property id As Integer
    Property pay_period_ending As Date
    Property employee_id As Int16
    Property justification As String
    Property added_by As String
    Property added_on As Date

    Public Sub New()
    End Sub

    Public Shared Function GetJustificationsByPayPeriod(PayPeriodEnding As Date) As List(Of PayrollChangeJustification)
      Dim dp As New DynamicParameters
      dp.Add("@pay_period_ending", PayPeriodEnding)
      Dim query As String = "
        SELECT
          id
          ,pay_period_ending
          ,employee_id
          ,justification
          ,added_on
          ,added_by
        FROM Payroll_Justification
        WHERE
          pay_period_ending=@pay_period_ending
        ORDER BY 
          employee_id;"
      Return Get_Data(Of PayrollChangeJustification)(query, dp, ConnectionStringType.Timestore)
    End Function

    Public Shared Function GetJustificationsByEmployeeAndPayPeriod(EmployeeId As Integer, PayPeriodEnding As Date) As List(Of PayrollChangeJustification)
      Dim dp As New DynamicParameters
      dp.Add("@pay_period_ending", PayPeriodEnding)
      dp.Add("@employee_id", EmployeeId)
      Dim query As String = "
        SELECT
          id
          ,pay_period_ending
          ,employee_id
          ,justification
          ,added_on
          ,added_by
        FROM Payroll_Justification
        WHERE
          pay_period_ending=@pay_period_ending
          AND employee_id = @employee_id
        ORDER BY 
          employee_id, id;"
      Return Get_Data(Of PayrollChangeJustification)(query, dp, ConnectionStringType.Timestore)
    End Function

    Private Shared Function BuildDataTable() As DataTable
      Dim dt As New DataTable("Payroll_Change_Justifications")
      'dt.Columns.Add("work_hours_id", Type.GetType("System.Int64"))
      dt.Columns.Add("id", Type.GetType("System.Int32"))
      dt.Columns.Add("employee_id", Type.GetType("System.Int32"))
      dt.Columns.Add("pay_period_ending", Type.GetType("System.DateTime"))
      dt.Columns.Add("justification", Type.GetType("System.String"))
      Return dt
    End Function

    Public Shared Function SaveJustifications(PayPeriodEnding As Date, EmployeeId As Integer, Username As String, Justifications As List(Of PayrollChangeJustification)) As List(Of PayrollChangeJustification)
      Dim dp As New DynamicParameters
      dp.Add("@username", Username)

      Dim dt As DataTable = BuildDataTable()
      For Each j In Justifications
        dt.Rows.Add(j.id, j.employee_id, j.pay_period_ending, j.justification)
      Next
      dp.Add("@JUST", dt.AsTableValuedParameter("Payroll_Change_Justifications"))
      Dim query As String = "
        MERGE TimeStore.dbo.Payroll_Justification WITH (HOLDLOCK) AS PJ

        USING @JUST AS J ON PJ.id = J.id

        WHEN MATCHED THEN
          UPDATE
            SET 
              justification = J.justification
              ,added_by = @username
              ,added_on=GETDATE()

        WHEN NOT MATCHED BY TARGET THEN
          INSERT (
            employee_id
            ,pay_period_ending
            ,justification
            ,added_by
            ,added_on
          )
          VALUES (
            J.employee_id
            ,J.pay_period_ending
            ,J.justification
            ,@username
            ,GETDATE()
          );"

      Dim i = Exec_Query(query, dp, ConnectionStringType.Timestore)
      Return GetJustificationsByEmployeeAndPayPeriod(EmployeeId, PayPeriodEnding)
    End Function

    Public Shared Function DeleteJustification(id As Integer) As Boolean
      If id = -1 Then Return False
      Dim dp As New DynamicParameters
      dp.Add("@id", id)
      Dim query As String = "
        DELETE FROM Payroll_Justification
        WHERE id = @id;"
      Return Exec_Query(query, dp, ConnectionStringType.Timestore) > -1
    End Function

  End Class

End Namespace
