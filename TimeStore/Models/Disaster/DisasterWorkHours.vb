Namespace Models


  Public Class DisasterWorkHours
    Property WorkHoursId As Long
    Property DisasterPeriodId As Integer
    Property DisasterWorkTimes As String
    Property DisasterWorkHours As Decimal
    Property DisasterWorkType As String
    Property DateAdded As Date

    Public Sub New()
    End Sub


    Private Shared Function Get_Disaster_Work_Hours_Query() As String
      Dim query As String = "
        SELECT
          DW.work_hours_id WorkHoursId
          ,DW.disaster_period_id DisasterPeriodId
          ,DW.disaster_work_times DisasterWorkTimes
          ,DW.disaster_work_hours DisasterWorkHours
          ,DW.disaster_work_type DisasterWorkType
          ,DW.date_added DateAdded
        FROM TimeStore.dbo.Disaster_Work_Hours DW
        INNER JOIN Timestore.dbo.Work_Hours W ON DW.work_hours_id = W.work_hours_id
"
      Return query
    End Function

    Public Shared Function Get_All_By_Date_Range(ByVal Start As Date, ByVal EndDate As Date) As List(Of DisasterWorkHours)
      Dim dp As New DynamicParameters()
      dp.Add("@Start", Start)
      dp.Add("@End", EndDate)
      Dim query As String = Get_Disaster_Work_Hours_Query() + "
        WHERE
          W.work_date BETWEEN @Start AND @End
        ORDER BY W.work_date ASC, W.employee_id ASC"
      Try
        Return Get_Data(Of DisasterWorkHours)(query, dp, ConnectionStringType.Timestore)
      Catch ex As Exception
        Dim e As New ErrorLog(ex, query)
        Return Nothing
      End Try
    End Function

    Public Shared Function Get_By_Employee_And_Date_Range(ByVal Start As Date, ByVal EndDate As Date, ByVal EmployeeID As Integer) As List(Of DisasterWorkHours)
      Dim dp As New DynamicParameters()
      dp.Add("@Start", Start)
      dp.Add("@End", EndDate)
      dp.Add("@EmployeeID", EmployeeID)
      Dim query As String = Get_Disaster_Work_Hours_Query() + "
        WHERE
          W.work_date BETWEEN @Start AND @End
          AND W.employee_id = @EmployeeID
        ORDER BY W.work_date ASC, W.employee_id ASC"
      Try
        Return Get_Data(Of DisasterWorkHours)(query, dp, ConnectionStringType.Timestore)
      Catch ex As Exception
        Dim e As New ErrorLog(ex, query)
        Return Nothing
      End Try
    End Function

    Public Shared Function Get_By_Employee_And_WorkDay(ByVal WorkDate As Date, ByVal EmployeeID As Integer) As List(Of DisasterWorkHours)
      Dim dp As New DynamicParameters()
      dp.Add("@WorkDate", WorkDate)
      dp.Add("@EmployeeID", EmployeeID)
      Dim query As String = Get_Disaster_Work_Hours_Query() + "
        WHERE
          W.work_date = @WorkDate
          AND W.employee_id = @EmployeeID
        ORDER BY W.work_date ASC, W.employee_id ASC"
      Try
        Return Get_Data(Of DisasterWorkHours)(query, dp, ConnectionStringType.Timestore)
      Catch ex As Exception
        Dim e As New ErrorLog(ex, query)
        Return Nothing
      End Try
    End Function

    Public Shared Function PopulateDisasterWorkHours(DisasterWorkHoursList As List(Of DisasterWorkHours)) As DataTable
      Dim dt = BuildDataTable()
      Try
        For Each dwh In DisasterWorkHoursList
          dt.Rows.Add(dwh.DisasterPeriodId, dwh.DisasterWorkTimes, dwh.DisasterWorkType, dwh.DisasterWorkHours)
        Next
      Catch ex As Exception
        Dim e As New ErrorLog(ex, "")
      End Try
      Return dt
    End Function

    Private Shared Function BuildDataTable() As DataTable
      Dim dt As New DataTable("DisasterWorkHours")
      'dt.Columns.Add("work_hours_id", Type.GetType("System.Int64"))
      dt.Columns.Add("disaster_period_id", Type.GetType("System.Int32"))
      dt.Columns.Add("disaster_work_times", Type.GetType("System.String"))
      dt.Columns.Add("disaster_work_type", Type.GetType("System.String"))
      dt.Columns.Add("disaster_work_hours", Type.GetType("System.Decimal"))
      Return dt
    End Function

  End Class

End Namespace
