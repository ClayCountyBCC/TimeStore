Namespace Models
  Public Class Out_Of_Class
    ReadOnly Property Active As Integer
      Get
        If EmployeeID = -1 Then Return 0 Else Return 1
      End Get
    End Property
    Property EmployeeID As Integer = -1
    Property WorkDate As Date
    Property ServiceRequestNumber As String
    Property Reason As String
    Property AddedByEmployeeID As Integer
    Property AddedOn As Date
    Property ApprovedByEmployeeID As Integer
    Property ApprovedByUsername As String
    Property ApprovedByIPAddress As String
    Property ApprovedByMachinename As String
    Property ApprovedOn As Date

    Public Sub New()

    End Sub

    ' Need to add Save function if this ends up going live.
    Public Function Retrieve(EmployeeID As Integer, PayPeriodEnding As Date) As List(Of Out_Of_Class)
      Dim dp As New DynamicParameters
      dp.Add("@EmployeeID", EmployeeID)
      dp.Add("@PayPeriodending", PayPeriodEnding)
      Dim query As String = "
        USE TimeStore;
        SELECT employee_id
              ,work_date
              ,service_request_number
              ,reason
              ,added_by_employee_id
              ,added_on
              ,approved_by_employee_id
              ,approved_by_username
              ,approved_by_ip_address
              ,approved_by_machinename
              ,approved_on
          FROM Out_Of_Class OOC
          INNER JOIN Work_Date W ON OOC.employee_id = W.employee_id AND 
            OOC.work_date = W.work_date
          WHERE 
            OOC.employee_id = @EmployeeID AND
            W.pay_period_ending = @PayPeriodEnding"
      Try

        Return Get_Data(Of Out_Of_Class)(query, ConnectionStringType.Timestore)
      Catch ex As Exception
        Dim e As New ErrorLog(ex, query & vbCrLf & EmployeeID.ToString & vbCrLf & PayPeriodEnding.ToShortDateString)
        Return Nothing
      End Try
    End Function



  End Class

End Namespace
