Imports Dapper
Imports System.Data
Imports System.Data.SqlClient

Namespace Models.Paystub



  Public Class Paystub
    Property department As String
    Property employee_name As String
    Property address_line_1 As String
    Property address_line_2 As String
    Property address_line_3 As String
    Property employee_id As Integer
    Property check_number As String
    Property year_to_date_gross As Decimal
    Property pay_date As Date
    Property pay_period_ending As Date
    Property earnings As List(Of PaystubEarnings) = New List(Of PaystubEarnings)
    Property leave As List(Of PaystubLeave) = New List(Of PaystubLeave)
    Property deductions As List(Of PaystubDeductions) = New List(Of PaystubDeductions)


    Public Sub New()

    End Sub

    Public Shared Function Get_Paystub(employee_id As Integer, check_number As String) As Paystub
      Dim dp As New DynamicParameters()
      dp.Add("@employee_id", employee_id)
      dp.Add("@check_number", check_number)

      Dim Query As String = $"
        SELECT
          LTRIM(RTRIM(E.home_orgn)) department
          ,LTRIM(RTRIM(E.f_name)) + ' ' + ISNULL(LTRIM(RTRIM(E.m_name)) + ' ', '') + LTRIM(RTRIM(ISNULL(E.l_name, ''))) + ' ' + LTRIM(RTRIM(ISNULL(E.name_suffix, ''))) employee_name 
          ,ISNULL(LTRIM(RTRIM(E.addr1)), '') address_line_1
          ,ISNULL(LTRIM(RTRIM(E.addr2)), '') address_line_2
          ,LTRIM(RTRIM(ISNULL(LTRIM(RTRIM(E.city)), '') + ' ' + E.state_id + ' ' + E.zip)) address_line_3
          ,LTRIM(RTRIM(CY.empl_no)) employee_id
          ,LTRIM(RTRIM(CY.check_no)) check_number
          ,CY.tearn_y year_to_date_gross  
          ,CH.iss_date pay_date
          ,CH.trans_date pay_period_ending
        FROM check_ytd CY
        LEFT OUTER JOIN checkhis CH ON CY.empl_no = CH.empl_no
          AND CY.check_no = CH.check_no
        INNER JOIN employee E ON CY.empl_no=E.empl_no
        WHERE
          CY.empl_no=CAST(@employee_id AS VARCHAR(10))
          AND CY.check_no=@check_number"

      Dim paystubs = Get_Data(Of Paystub)(Query, dp, ConnectionStringType.FinPlus)
      If paystubs.Count() <> 1 Then
        Return Nothing
      End If
      Dim paystub = paystubs.First

      paystub.earnings = PaystubEarnings.Get_Earnings(employee_id, check_number)
      paystub.leave = PaystubLeave.Get_Leave(employee_id, check_number)
      paystub.deductions = PaystubDeductions.Get_Deductions(employee_id, check_number)

      Return paystub
    End Function






  End Class

End Namespace
