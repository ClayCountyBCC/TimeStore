Imports Dapper
Imports System.Data
Imports System.Data.SqlClient

Namespace Models.Paystub

  Public Class PaystubEarnings

    Property pay_code_name As String
    Property pay_code_short_name As String
    Property payrate As Decimal
    Property hours As Decimal
    Property amount As Decimal

    Public Shared Function Get_Earnings(employee_id As Integer, check_number As String) As List(Of PaystubEarnings)
      Dim dp As New DynamicParameters()
      dp.Add("@employee_id", employee_id)
      dp.Add("@check_number", check_number)

      Dim Query As String = $"
        SELECT
          LTRIM(RTRIM(P.title)) pay_code_name
          ,LTRIM(RTRIM(P.ck_title)) pay_code_short_name
          ,CH2.payrate
          ,CH2.hours
          ,CH2.amt amount
        FROM checkhis CH
        INNER JOIN checkhi2 CH2 ON CH.check_no=CH2.check_no
        LEFT OUTER JOIN paytable P ON CH2.code = P.pay_code AND CH2.earn_ded='P'
        WHERE   
          CH2.earn_ded='P'
          AND CH2.empl_no=CAST(@employee_id AS VARCHAR(10))
          AND CH2.check_no=@check_number
        ORDER BY CH2.code"

      Return Get_Data(Of PaystubEarnings)(Query, dp, ConnectionStringType.FinPlus)
    End Function

  End Class
End Namespace
