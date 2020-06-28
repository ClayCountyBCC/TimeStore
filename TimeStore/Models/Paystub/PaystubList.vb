Namespace Models.Paystub


  Public Class PaystubList
    Property employee_id As Integer
    Property check_number As String
    Property check_date As Date
    Property transaction_date As Date
    Property pay_period_start_date As Date

    Public Shared Function Get_Paystubs(employee_id As Integer, pay_stub_year As Integer) As List(Of PaystubList)
      Dim dp As New DynamicParameters()
      dp.Add("@employee_id", employee_id)
      dp.Add("@pay_stub_year", pay_stub_year)

      Dim Query As String = $"
        SELECT
          empl_no employee_id
          ,check_no check_number
          ,iss_date check_date
          ,trans_date transaction_date
          ,start_date pay_period_start_date
        FROM checkhis
        WHERE
          YEAR(trans_date) = @pay_stub_year
          AND empl_no = CAST(@employee_id AS VARCHAR(10))
        ORDER BY
          trans_date DESC
"

      Return Get_Data(Of PaystubList)(Query, dp, ConnectionStringType.FinPlus)
    End Function



  End Class
End Namespace