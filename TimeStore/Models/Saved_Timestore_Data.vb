Imports System.Data
Imports System.Data.SqlClient
Imports Dapper

Namespace Models

  Public Class Saved_TimeStore_Data

    Property work_hours_id As Long = 0
    Property employee_id As Integer = 0
    Property dept_id As String = ""
    Property pay_period_ending As Date = Date.MaxValue
    Property work_date As Date = Date.MaxValue
    Property work_times As String = ""
    Property disaster_work_times As String = ""
    Property disaster_work_hours As Double = 0
    Property disaster_name As String = ""
    Property disaster_work_type As String = ""
    'Property disaster_period_type As Integer = 0 ' used by the client to determine how we should ask for disaster hours.  1 = always ask, 2 = don't ask but allow disaster hours to be entered. 0 = no disaster hours should be entered.
    'Property disaster_rule As Integer = 0
    Property disaster_normal_scheduled_hours As Decimal = 0
    Property break_credit As Double = 0
    Property work_hours As Double = 0
    Property holiday As Double = 0
    Property leave_without_pay As Double = 0
    Property total_hours As Double = 0
    Property out_of_class As Boolean = False
    Property doubletime_hours As Double = 0
    Property vehicle As Integer = 0
    Property comment As String = ""
    Property by_employeeid As Integer = 0
    Property by_username As String = ""
    Property by_machinename As String = ""
    Property by_ip_address As String = ""
    Property date_updated As Date = Date.MaxValue
    Property HoursToApprove As New List(Of Saved_TimeStore_Data_To_Approve)

    Public Sub New()

    End Sub

    Public Sub New(tctd As TimecardTimeData, tca As Timecard_Access)
      employee_id = tctd.EmployeeID
      dept_id = tctd.DepartmentNumber
      pay_period_ending = CType(tctd.PPD, Date).AddDays(13)
      work_hours = tctd.WorkHours
      work_date = tctd.WorkDate
      work_times = tctd.WorkTimes
      disaster_work_times = tctd.DisasterWorkTimes
      disaster_work_hours = tctd.DisasterWorkHours
      disaster_name = tctd.DisasterName
      disaster_normal_scheduled_hours = tctd.DisasterNormalScheduledHours
      'disaster_period_type = tctd.DisasterPeriodType
      disaster_work_type = tctd.DisasterWorkType
      break_credit = tctd.BreakCreditHours
      holiday = tctd.HolidayHours
      leave_without_pay = tctd.LWOPHours
      total_hours = tctd.TotalHours
      doubletime_hours = tctd.DoubleTimeHours
      vehicle = tctd.Vehicle
      comment = tctd.Comment
      out_of_class = tctd.OutOfClass
      by_employeeid = tca.EmployeeID
      by_username = tca.UserName
      by_machinename = tca.MachineName
      by_ip_address = tca.IPAddress
    End Sub

    Private Shared Function GetWorkHoursQuery() As String
      Dim query As String = "
        SELECT 
          work_hours_id,
          employee_id,
          dept_id,
          pay_period_ending,
          work_date,
          work_times,
          disaster_work_times,
          disaster_work_hours,
          ISNULL(D.Name, '') disaster_name,          
          ISNULL(W.disaster_normal_scheduled_hours, -1) disaster_normal_scheduled_hours,          
          ISNULL(disaster_work_type, '') disaster_work_type,
          break_credit,
          work_hours,
          holiday,
          leave_without_pay,
          total_hours,
          vehicle,
          comment,
          out_of_class,
          date_added,
          date_last_updated date_updated,
          by_employeeid,
          by_username,
          by_machinename,
          by_ip_address,
          doubletime_hours 
        FROM TimeStore.dbo.Work_Hours W
        LEFT OUTER JOIN TimeStore.dbo.Disaster_Period D ON W.work_date BETWEEN D.StartDate AND D.EndDate"

      Return query
    End Function

    Public Shared Function GetByEmployeeAndWorkday(WorkDate As Date, EmployeeID As Integer) As Saved_TimeStore_Data
      Dim dp As New DynamicParameters()
      dp.Add("@WorkDate", WorkDate)
      dp.Add("@EmployeeID", EmployeeID)
      Dim query As String = GetWorkHoursQuery() + "
        WHERE 
          W.employee_id = @EmployeeID
          AND W.work_date = @WorkDate
        ORDER BY work_date ASC, employee_id ASC"
      Try
        Dim l = Get_Data(Of Saved_TimeStore_Data)(query, dp, ConnectionStringType.Timestore)
        If l.Count = 0 Then
          Return New Saved_TimeStore_Data
        Else
          Dim st = l.First
          st.HoursToApprove.Add(Saved_TimeStore_Data_To_Approve.GetByEmployeeAndWorkday(st.work_date, st.employee_id))
          Return st
        End If
      Catch ex As Exception
        Dim e As New ErrorLog(ex, query)
        Return Nothing
      End Try
    End Function

    Public Shared Function GetAllByDateRange(Start As Date, EndDate As Date) As List(Of Saved_TimeStore_Data)
      Dim dp As New DynamicParameters()
      Start = GetPayPeriodStart(Start)
      dp.Add("@Start", Start)
      dp.Add("@End", EndDate)
      Dim query As String = GetWorkHoursQuery() + "
        WHERE 
          W.work_date BETWEEN @Start AND @End 
        ORDER BY W.work_date ASC, W.employee_id ASC"
      Try
        Dim stl = Get_Data(Of Saved_TimeStore_Data)(query, dp, ConnectionStringType.Timestore)
        Dim stlApp = Saved_TimeStore_Data_To_Approve.GetAllByDateRange(Start, EndDate)
        For Each s In stl
          s.HoursToApprove.AddRange((From sA In stlApp
                                     Where sA.work_hours_id = s.work_hours_id
                                     Select sA).ToList)
        Next
        Return stl
      Catch ex As Exception
        Dim e As New ErrorLog(ex, query)
        Return Nothing
      End Try
    End Function

    Public Shared Function GetByEmployeeAndDateRange(Start As Date, EndDate As Date, EmployeeID As Integer) As List(Of Saved_TimeStore_Data)
      Dim dp As New DynamicParameters()
      dp.Add("@Start", Start)
      dp.Add("@End", EndDate)
      dp.Add("@EmployeeID", EmployeeID)
      Dim query As String = GetWorkHoursQuery() + "
        WHERE 
          W.employee_id = @EmployeeID
          AND W.work_date BETWEEN @Start AND @End 
        ORDER BY W.work_date ASC, W.employee_id ASC"
      Try
        'Return Get_Data(Of Saved_TimeStore_Data)(query, dp, ConnectionStringType.Timestore)
        Dim stl = Get_Data(Of Saved_TimeStore_Data)(query, dp, ConnectionStringType.Timestore)
        Dim stlApp = Saved_TimeStore_Data_To_Approve.GetByEmployeeAndDateRange(Start, EndDate, EmployeeID)
        For Each s In stl
          s.HoursToApprove.AddRange((From sA In stlApp
                                     Where sA.work_hours_id = s.work_hours_id
                                     Select sA).ToList)
        Next
        Return stl
      Catch ex As Exception
        Dim e As New ErrorLog(ex, query)
        Return Nothing
      End Try

    End Function

  End Class

End Namespace