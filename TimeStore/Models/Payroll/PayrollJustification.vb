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

  End Class

End Namespace
