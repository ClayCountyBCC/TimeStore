Imports Dapper
Imports System.Data
Imports System.Data.SqlClient

Namespace Models.Paystub

  Public Class PaystubLeave
    Property leave_code_name As String
    Property leave_code_short_name As String
    Property leave_balance As Decimal
    Property leave_taken As Decimal
    Property leave_earned As Decimal
    Property accrual_rate As Decimal
    Property bank_maximum As Decimal
    Property calculated_accrual_rate As Decimal

    Public Shared Function Get_Leave(employee_id As Integer, check_number As String) As List(Of PaystubLeave)
      Dim dp As New DynamicParameters()
      dp.Add("@employee_id", employee_id)
      dp.Add("@check_number", check_number)

      Dim Query As String = $"
        SELECT
          LTRIM(RTRIM(LT.title)) leave_code_name
          ,LTRIM(RTRIM(LT.ck_title)) leave_code_short_name
          ,lv_bal leave_balance
          ,lv_tak leave_taken
          ,lv_ear leave_earned
          ,LT.acc_rate accrual_rate
          ,LT.roll_lim bank_maximum          
          ,LT.acc_rate * CASE WHEN LT.lv_code IN ('300', '400', '401', '402', '403', '404', '405', '406', '407') THEN 106 ELSE 80 END  calculated_accrual_rate
        FROM check_leave CL
        INNER JOIN levtable LT ON CL.lv_code=LT.lv_code        
        WHERE
          CL.empl_no=CAST(@employee_id AS VARCHAR(10))
          AND check_no = @check_number
          AND CL.lv_code NOT IN ('900');"

      Return Get_Data(Of PaystubLeave)(Query, dp, ConnectionStringType.FinPlus)
    End Function

  End Class
End Namespace