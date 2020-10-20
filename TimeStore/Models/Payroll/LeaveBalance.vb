Namespace Models


  Public Class LeaveBalance
    Property employee_id As Integer
    Property pay_period_ending As Date
    Property lv1_code As String
    Property lv1_balance As Decimal
    Property lv2_code As String
    Property lv2_balance As Decimal
    Property lv3_code As String
    Property lv3_balance As Decimal
    Property lv4_code As String
    Property lv4_balance As Decimal
    Property lv5_code As String
    Property lv5_balance As Decimal
    Property lv6_code As String
    Property lv6_balance As Decimal
    Property lv7_code As String
    Property lv7_balance As Decimal
    Property lv8_code As String
    Property lv8_balance As Decimal
    Property lv9_code As String
    Property lv9_balance As Decimal

    Public Sub New()
    End Sub

    Public Shared Function GetLeaveBalances(PayPeriodEnding As Date) As Dictionary(Of Integer, Dictionary(Of String, Decimal))
      Dim dp As New DynamicParameters
      dp.Add("@pay_period_ending", PayPeriodEnding)
      Dim query As String = "
        SELECT
          empl_no employee_id
          ,pay_period_ending
          ,ISNULL(LTRIM(RTRIM(lv1_cd)), '')  lv1_code
          ,lv1_bal lv1_balance
          ,ISNULL(LTRIM(RTRIM(lv2_cd)), '')  lv2_code
          ,lv2_bal lv2_balance
          ,ISNULL(LTRIM(RTRIM(lv3_cd)), '')  lv3_code
          ,lv3_bal lv3_balance
          ,ISNULL(LTRIM(RTRIM(lv4_cd)), '')  lv4_code
          ,lv4_bal lv4_balance
          ,ISNULL(LTRIM(RTRIM(lv5_cd)), '')  lv5_code
          ,lv5_bal lv5_balance
          ,ISNULL(LTRIM(RTRIM(lv6_cd)), '')  lv6_code
          ,lv6_bal lv6_balance
          ,ISNULL(LTRIM(RTRIM(lv7_cd)), '')  lv7_code
          ,lv7_bal lv7_balance
          ,ISNULL(LTRIM(RTRIM(lv8_cd)), '')  lv8_code
          ,lv8_bal lv8_balance
          ,ISNULL(LTRIM(RTRIM(lv9_cd)), '')  lv9_code
          ,lv9_bal lv9_balance
        FROM Finplus_Payroll
        WHERE
          pay_period_ending = @pay_period_ending
        ORDER BY empl_no;"
      Dim data = Get_Data(Of LeaveBalance)(query, dp, ConnectionStringType.Timestore)
      Dim employee As New Dictionary(Of Integer, Dictionary(Of String, Decimal))
      For Each d In data
        Dim balance = New Dictionary(Of String, Decimal)
        If d.lv1_code.Length > 0 Then balance(FixLeaveCode(d.lv1_code)) = d.lv1_balance
        If d.lv2_code.Length > 0 Then balance(FixLeaveCode(d.lv2_code)) = d.lv2_balance
        If d.lv3_code.Length > 0 Then balance(FixLeaveCode(d.lv3_code)) = d.lv3_balance
        If d.lv4_code.Length > 0 Then balance(FixLeaveCode(d.lv4_code)) = d.lv4_balance
        If d.lv5_code.Length > 0 Then balance(FixLeaveCode(d.lv5_code)) = d.lv5_balance
        If d.lv6_code.Length > 0 Then balance(FixLeaveCode(d.lv6_code)) = d.lv6_balance
        If d.lv7_code.Length > 0 Then balance(FixLeaveCode(d.lv7_code)) = d.lv7_balance
        If d.lv8_code.Length > 0 Then balance(FixLeaveCode(d.lv8_code)) = d.lv8_balance
        If d.lv9_code.Length > 0 Then balance(FixLeaveCode(d.lv9_code)) = d.lv9_balance
        If balance.Keys.Count > 0 Then
          If Not employee.ContainsKey(d.employee_id) Then
            employee(d.employee_id) = balance
          End If
        End If
      Next
      Return employee
    End Function

    Public Shared Function GetLeaveBalancesByEmployee(PayPeriodEnding As Date, EmployeeId As Integer) As Dictionary(Of String, Decimal)
      Dim dp As New DynamicParameters
      dp.Add("@pay_period_ending", PayPeriodEnding)
      dp.Add("@employee_id", EmployeeId)
      Dim query As String = "
        SELECT
          empl_no employee_id
          ,pay_period_ending
          ,ISNULL(LTRIM(RTRIM(lv1_cd)), '')  lv1_code
          ,lv1_bal lv1_balance
          ,ISNULL(LTRIM(RTRIM(lv2_cd)), '')  lv2_code
          ,lv2_bal lv2_balance
          ,ISNULL(LTRIM(RTRIM(lv3_cd)), '')  lv3_code
          ,lv3_bal lv3_balance
          ,ISNULL(LTRIM(RTRIM(lv4_cd)), '')  lv4_code
          ,lv4_bal lv4_balance
          ,ISNULL(LTRIM(RTRIM(lv5_cd)), '')  lv5_code
          ,lv5_bal lv5_balance
          ,ISNULL(LTRIM(RTRIM(lv6_cd)), '')  lv6_code
          ,lv6_bal lv6_balance
          ,ISNULL(LTRIM(RTRIM(lv7_cd)), '')  lv7_code
          ,lv7_bal lv7_balance
          ,ISNULL(LTRIM(RTRIM(lv8_cd)), '')  lv8_code
          ,lv8_bal lv8_balance
          ,ISNULL(LTRIM(RTRIM(lv9_cd)), '')  lv9_code
          ,lv9_bal lv9_balance
        FROM Finplus_Payroll
        WHERE
          pay_period_ending = @pay_period_ending
          AND empl_no = @employee_id
        ORDER BY empl_no;"
      Dim data = Get_Data(Of LeaveBalance)(query, dp, ConnectionStringType.Timestore)
      If data Is Nothing OrElse data.Count() = 0 Then
        Return New Dictionary(Of String, Decimal)
      Else
        Dim d As LeaveBalance = data.First
        Dim balance = New Dictionary(Of String, Decimal)
        If d.lv1_code.Length > 0 Then balance(FixLeaveCode(d.lv1_code)) = d.lv1_balance
        If d.lv2_code.Length > 0 Then balance(FixLeaveCode(d.lv2_code)) = d.lv2_balance
        If d.lv3_code.Length > 0 Then balance(FixLeaveCode(d.lv3_code)) = d.lv3_balance
        If d.lv4_code.Length > 0 Then balance(FixLeaveCode(d.lv4_code)) = d.lv4_balance
        If d.lv5_code.Length > 0 Then balance(FixLeaveCode(d.lv5_code)) = d.lv5_balance
        If d.lv6_code.Length > 0 Then balance(FixLeaveCode(d.lv6_code)) = d.lv6_balance
        If d.lv7_code.Length > 0 Then balance(FixLeaveCode(d.lv7_code)) = d.lv7_balance
        If d.lv8_code.Length > 0 Then balance(FixLeaveCode(d.lv8_code)) = d.lv8_balance
        If d.lv9_code.Length > 0 Then balance(FixLeaveCode(d.lv9_code)) = d.lv9_balance
        Return balance
      End If

    End Function

    Private Shared Function FixLeaveCode(code As String) As String
      ' The leave codes do not match the lv_sub field in the paycode table exactly
      ' it is because we have multiple iterations of vacation for regular employees
      ' and union employees (see the levtable in Finplus)
      ' So this function just gets rid of the non-0 and replaces it with a 0.
      If code.Substring(code.Length - 1, 1) <> "0" Then
        Return code.Substring(0, 2) & "0"
      Else
        Return code
      End If
    End Function


  End Class
End Namespace