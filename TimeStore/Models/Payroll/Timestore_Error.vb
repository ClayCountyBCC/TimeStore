Namespace Models


  Public Class Timestore_Error
    Property pay_period_ending As Date
    Property employee_id As Integer
    Property error_text As String

    Public Sub New()
    End Sub

    Public Sub New(PayPeriodEnding As Date, EmployeeId As Integer, Text As String)
      pay_period_ending = PayPeriodEnding
      employee_id = EmployeeId
      error_text = Text
    End Sub

    Public Shared Function GetErrors(PayPeriodEnding As Date) As List(Of Timestore_Error)
      Dim dp As New DynamicParameters
      dp.Add("@pay_period_ending", PayPeriodEnding)
      Dim query As String = "
        SELECT
          employee_id
          ,pay_period_ending
          ,error_text 
          FROM Timestore_Errors
        WHERE
          pay_period_ending = @pay_period_ending
        ORDER BY employee_id;"
      Return Get_Data(Of Timestore_Error)(query, dp, ConnectionStringType.Timestore)
    End Function

    Public Shared Function SaveErrors(PayPeriodEnding As Date, ByRef timecards As List(Of GenericTimecard)) As Boolean
      Dim ts_errors As New List(Of Timestore_Error)
      For Each tc In timecards
        For Each w In tc.WarningList
          ts_errors.Add(New Timestore_Error(tc.payPeriodStart.AddDays(13),
                                              Integer.Parse(tc.employeeID),
                                              "Timestore Warning: " & w))
        Next
        For Each w In tc.ErrorList
          ts_errors.Add(New Timestore_Error(tc.payPeriodStart.AddDays(13),
                                              Integer.Parse(tc.employeeID),
                                              "Timestore Error: " & w))
        Next
      Next
      Dim dt As DataTable = BuildDataTable()
      For Each t In ts_errors
        dt.Rows.Add(t.employee_id, t.pay_period_ending, t.error_text)
      Next
      Dim dp As New DynamicParameters
      dp.Add("@TSE", dt.AsTableValuedParameter("TimestoreErrors"))
      Dim query As String = "
        INSERT INTO Timestore_Errors (pay_period_ending, employee_id, error_text)
        SELECT
          T.pay_period_ending
          ,T.employee_id
          ,T.error_text
        FROM @TSE AS T
"
      Dim i = Exec_Query(query, dp, ConnectionStringType.Timestore)
      Return i > 0
    End Function

    Private Shared Function BuildDataTable() As DataTable
      Dim dt As New DataTable("TimestoreErrors")
      dt.Columns.Add("employee_id", Type.GetType("System.Int32"))
      dt.Columns.Add("pay_period_ending", Type.GetType("System.DateTime"))
      dt.Columns.Add("error_text", Type.GetType("System.String"))
      Return dt
    End Function

  End Class
End Namespace