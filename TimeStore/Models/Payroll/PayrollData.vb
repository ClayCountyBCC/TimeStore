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

    Public Sub New()
    End Sub

    Public Shared Function GetAllBasePayrollData(pay_period_ending As Date, ByRef paycodes As Dictionary(Of String, Paycode))
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

    Public Shared Function GetAllPayrollChanges(pay_period_ending As Date, ByRef paycodes As Dictionary(Of String, Paycode))
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
      For Each d In data
        If paycodes.ContainsKey(d.paycode) Then d.paycode_detail = paycodes(d.paycode)
      Next
      Return data
    End Function

    Public Shared Function GetAllFinplusPayrates(pay_period_ending As Date, ByRef paycodes As Dictionary(Of String, Paycode))
      Dim dp As New DynamicParameters
      dp.Add("@pay_period_ending", pay_period_ending)
      Dim query As String = "
      SELECT
        employee_id
        ,pay_period_ending
        ,paycode
        ,CASE WHEN classify='FF' THEN 0 ELSE payrate END payrate
        ,'' project_code
        ,pay_hours hours
        ,CASE WHEN classify='FF' THEN payrate ELSE payrate * pay_hours END amount
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


  End Class

End Namespace