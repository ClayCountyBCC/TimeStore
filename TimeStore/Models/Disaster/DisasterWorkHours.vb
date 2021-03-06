﻿Namespace Models


  Public Class DisasterWorkHours
    Property WorkHoursId As Long
    Property DisasterPeriodId As Integer
    Property DisasterWorkTimes As String = ""
    Property DisasterWorkHours As Decimal = 0
    Property DisasterWorkType As String = ""
    Property DisasterAdminHours As Decimal = 0
    Property DateAdded As Date
    Property DisasterWorkTimesByRule As New Dictionary(Of Integer, List(Of TimeSpan))
    Property DisasterWorkHoursByRule As New Dictionary(Of Integer, Double)
    Property DisasterHoursTotal As Double = 0 ' This value is different than DisasterWorkHours because it is the total number of hours we've identified as disasterhours, not what the employee has indicated are disasterhours.
    Property DisasterHoursRegular As Double = 0
    Property DisasterHoursStraight As Double = 0
    Property DisasterHoursOvertime As Double = 0
    Property DisasterHoursDoubletime As Double = 0

    Public Sub New()
      DisasterWorkHoursByRule(0) = 0
      DisasterWorkHoursByRule(1) = 0
      DisasterWorkHoursByRule(2) = 0
      DisasterWorkTimesByRule(0) = New List(Of TimeSpan)()
      DisasterWorkTimesByRule(1) = New List(Of TimeSpan)()
      DisasterWorkTimesByRule(2) = New List(Of TimeSpan)()
    End Sub


    Private Shared Function Get_Disaster_Work_Hours_Query() As String
      Dim query As String = "
        SELECT
          DW.work_hours_id WorkHoursId
          ,DW.disaster_period_id DisasterPeriodId
          ,DW.disaster_work_times DisasterWorkTimes
          ,DW.disaster_work_hours DisasterWorkHours
          ,DW.disaster_work_type DisasterWorkType
          ,DW.disaster_admin_hours DisasterAdminHours
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
          If dwh.DisasterWorkTimes Is Nothing Then dwh.DisasterWorkTimes = ""
          If dwh.DisasterWorkType Is Nothing Then dwh.DisasterWorkType = ""
          dt.Rows.Add(dwh.DisasterPeriodId, dwh.DisasterWorkTimes, dwh.DisasterWorkType, dwh.DisasterWorkHours, dwh.DisasterAdminHours)
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
      dt.Columns.Add("disaster_admin_hours", Type.GetType("System.Decimal"))
      Return dt
    End Function

  End Class

End Namespace
