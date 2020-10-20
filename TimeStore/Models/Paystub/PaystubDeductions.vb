Imports Dapper
Imports System.Data
Imports System.Data.SqlClient

Namespace Models.Paystub

  Public Class PaystubDeductions
    Property ded_code_full_name As String
    Property ded_code_short_name As String
    Property amount As Decimal
    Property year_to_date_deductions As Decimal
    Property contributions As Decimal

    Public Shared Function Get_Deductions(employee_id As Integer, check_number As String) As List(Of PaystubDeductions)
      Dim dp As New DynamicParameters()
      dp.Add("@employee_id", employee_id)
      dp.Add("@check_number", check_number)

      Dim Query As String = $"
        WITH DeductionsAndFederalCodes AS (

          SELECT
            ded_cd
            ,title
            ,ck_title
          FROM dedtable
          UNION
          SELECT
            '*FT '
            ,'FEDERAL TAX'
            ,'FED TAX'
          UNION
          SELECT
            '*FI '
            ,'FICA'
            ,'FICA'  
          UNION
          SELECT
            '*FM '
            ,'MEDICARE'
            ,'MEDICARE'  
        )

        SELECT
          LTRIM(RTRIM(ISNULL(B.desc_x, DT.title))) ded_code_full_name
          ,LTRIM(RTRIM(ISNULL(B.ck_title, DT.ck_title))) ded_code_short_name
          ,ISNULL(CH2.amt, 0) amount
          ,CD.taken_y year_to_date_deductions
          ,ISNULL(CH2.fringe, 0) contributions
        FROM check_ded CD
        LEFT OUTER JOIN checkhi2 CH2 ON CD.check_no=CH2.check_no 
          AND CD.empl_no=CH2.empl_no  
          AND CH2.code = CD.ded_cd
          AND CH2.earn_ded IN ('D', 'T')
        LEFT OUTER JOIN DeductionsAndFederalCodes DT ON CD.ded_cd=DT.ded_cd
        LEFT OUTER JOIN bnktable B ON CD.bank = B.code
        WHERE
          CD.check_no=@check_number
          AND CD.empl_no=CAST(@employee_id AS VARCHAR(10))
          AND 
            (CD.taken_y > 0  
            OR ISNULL(CH2.amt, 0) > 0
            OR ISNULL(CH2.fringe, 0) > 0)
        ORDER BY CASE WHEN CD.ded_cd = '9948' THEN 'zzzzz' ELSE CD.ded_cd END"

      Return Get_Data(Of PaystubDeductions)(Query, dp, ConnectionStringType.FinPlus)
    End Function

  End Class
End Namespace