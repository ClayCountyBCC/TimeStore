Imports System.Runtime.Caching

Namespace Models.Paystub


  Public Class PaystubList
    Property employee_id As Integer
    Property check_number As String
    Property check_date As Date
    Property transaction_date As Date
    Property pay_period_start_date As Date
    Property pay_stub_year As Integer
    Property is_voided As Boolean

    Public Shared Function Get_Paystubs_By_Employee(employee_id As Integer) As List(Of PaystubList)
      Dim dp As New DynamicParameters()
      dp.Add("@employee_id", employee_id)

      Dim Query As String = $"
          SELECT
          H.empl_no employee_id
          ,LTRIM(RTRIM(H.check_no)) check_number
          ,iss_date check_date
          ,trans_date transaction_date
          ,start_date pay_period_start_date
          ,YEAR(iss_date) pay_stub_year
          ,CASE WHEN ISNULL(H.man_void, '') = 'V' THEN 1 ELSE 0 END is_voided
        FROM checkhis H
        INNER JOIN check_ytd Y ON H.empl_no = Y.empl_no AND H.check_no = Y.check_no
        WHERE
          H.empl_no = CAST(@employee_id AS VARCHAR(10))
          AND ISNULL(H.man_void, '') != 'V'
        ORDER BY
          iss_date DESC"
      Return Get_Data(Of PaystubList)(Query, dp, ConnectionStringType.FinPlus)
    End Function

    Public Shared Function Get_All_Recent_Paystubs() As List(Of PaystubList)
      Dim Query As String = $"
        DECLARE @check_date DATE = (SELECT MAX(iss_date) FROM checkhis);
        SELECT
          empl_no employee_id
          ,LTRIM(RTRIM(check_no)) check_number
          ,iss_date check_date
          ,trans_date transaction_date
          ,start_date pay_period_start_date
          ,YEAR(iss_date) pay_stub_year      
        FROM checkhis
        WHERE
          iss_date = @check_date
        ORDER BY
          empl_no DESC"
      Return Get_Data(Of PaystubList)(Query, ConnectionStringType.FinPlus)
    End Function

    Public Shared Function Get_All_Recent_Paystubs_Cached() As List(Of PaystubList)
      Dim CIP As New CacheItemPolicy
      CIP.AbsoluteExpiration = Today.AddDays(1) ' Make it expire at midnight today
      Dim recent_paystubs As List(Of PaystubList) = myCache.GetItem("recent_paystubs", CIP)
      Return recent_paystubs
    End Function

  End Class
End Namespace