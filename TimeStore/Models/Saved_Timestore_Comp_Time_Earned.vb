Namespace Models
  Public Class Saved_TimeStore_Comp_Time_Earned
    Property employee_id As Integer = 0
    Property pay_period_ending As Date = Date.MaxValue
    Property comp_time_earned_week1 As Double = 0
    Property comp_time_earned_week2 As Double = 0
    Property date_added As Date = Date.MaxValue
    Property date_last_updated As Date = Date.MaxValue
    Property by_employeeid As Integer = 0
    Property by_username As String = ""
    Property by_machinename As String = ""
    Property by_ip_address As String = ""

    Public Sub New(EmployeeID As Integer, PayPeriodEnding As Date)
      PopulateCompTimeEarnedData(EmployeeID, PayPeriodEnding)
    End Sub

    Private Sub PopulateCompTimeEarnedData(EmployeeID As Integer, PayPeriodEnding As Date)
      Dim dbc As New Tools.DB(GetCS(ConnectionStringType.Timestore), toolsAppId, toolsDBError)
      Dim sbQ As New StringBuilder
      With sbQ
        .AppendLine("SELECT employee_id, pay_period_ending, comp_time_earned_week1, comp_time_earned_week2, ")
        .AppendLine("date_added, date_last_updated, added_by_employeeid, added_by_username, added_by_machinename, ")
        .AppendLine("added_by_ip_address FROM Comp_Time_Earned_Hours ")
        .Append("WHERE employee_id=").Append(EmployeeID).Append(" AND pay_period_ending='")
        .Append(PayPeriodEnding.ToShortDateString).AppendLine("';")
      End With
      Dim dsTmp As DataSet
      Try
        dsTmp = dbc.Get_Dataset(sbQ.ToString)
        If dsTmp.Tables(0).Rows.Count = 0 Then
          Exit Sub
        Else
          Load(dsTmp.Tables(0).Rows(0))
        End If

      Catch ex As Exception
        Log(ex)
      End Try
    End Sub

    Private Sub DeleteCompTimeEarnedData(EmployeeID As Integer, PayPeriodEnding As Date)
      Dim dbc As New Tools.DB(GetCS(ConnectionStringType.Timestore), toolsAppId, toolsDBError)
      Dim sbQ As New StringBuilder
      With sbQ
        .AppendLine("DELETE FROM Comp_Time_Earned_Hours ")
        .Append("WHERE employee_id=").Append(EmployeeID).Append(" AND pay_period_ending='")
        .Append(PayPeriodEnding.ToShortDateString).AppendLine("';")
      End With
      Try
        dbc.ExecuteNonQuery(sbQ.ToString)
      Catch ex As Exception
        Log(ex)
      End Try
    End Sub

    Public Sub New(dr As DataRow)
      Load(dr)
    End Sub

    Private Sub Load(dr As DataRow)
      Try
        employee_id = dr("employee_id")
        pay_period_ending = dr("pay_period_ending")
        comp_time_earned_week1 = dr("comp_time_earned_week1")
        comp_time_earned_week2 = dr("comp_time_earned_week2")
        date_added = dr("date_added")
        date_last_updated = dr("date_last_updated")
        by_employeeid = dr("added_by_employeeid")
        by_username = dr("added_by_username")
        by_machinename = dr("added_by_machinename")
        by_ip_address = dr("added_by_ip_address")
      Catch ex As Exception
        Log(ex)
      End Try
    End Sub

    Public Sub Delete()
      DeleteCompTimeEarnedData(employee_id, pay_period_ending)
      Clear_Saved_Timestore_Data(employee_id, pay_period_ending.AddDays(-13))
      Add_Timestore_Note(employee_id, pay_period_ending, "Your hours have changed, your Comp Time choices and approval has been removed.")
    End Sub
  End Class
End Namespace