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
    Property break_credit As Double = 0
    Property work_hours As Double = 0
    Property holiday As Double = 0
    Property leave_without_pay As Double = 0
    Property total_hours As Double = 0
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
      pay_period_ending = tctd.PPD
      work_hours = tctd.WorkHours
      work_date = tctd.WorkDate
      work_times = tctd.WorkTimes
      disaster_work_times = tctd.DisasterWorkTimes
      disaster_work_hours = tctd.DisasterWorkHours
      break_credit = tctd.BreakCreditHours
      holiday = tctd.HolidayHours
      leave_without_pay = tctd.LWOPHours
      total_hours = tctd.TotalHours
      doubletime_hours = tctd.DoubleTimeHours
      vehicle = tctd.Vehicle
      comment = tctd.Comment
      by_employeeid = tca.EmployeeID
      by_username = tca.UserName
      by_machinename = tca.MachineName
      by_ip_address = tca.IPAddress
    End Sub

    Private Shared Function GetWorkHoursQuery() As String
      Dim query As String = "
        USE TimeStore;
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
          break_credit,
          work_hours,
          holiday,
          leave_without_pay,
          total_hours,
          vehicle,
          comment,
          date_added,
          date_last_updated date_updated,
          by_employeeid,
          by_username,
          by_machinename,
          by_ip_address,
          doubletime_hours 
        FROM Work_Hours
        LEFT OUTER JOIN Disaster_Data D ON Work_Hours.work_date BETWEEN D.Disaster_Start AND D.Disaster_End"
      Return query
    End Function

    Public Shared Function GetByEmployeeAndWorkday(WorkDate As Date, EmployeeID As Integer) As Saved_TimeStore_Data
      Dim dp As New DynamicParameters()
      dp.Add("@WorkDate", WorkDate)
      dp.Add("@EmployeeID", EmployeeID)
      Dim query As String = GetWorkHoursQuery() + "
        WHERE 
          employee_id = @EmployeeID
          AND work_date = @WorkDate
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
          work_date BETWEEN @Start AND @End 
        ORDER BY work_date ASC, employee_id ASC"
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
          employee_id = @EmployeeID
          AND work_date BETWEEN @Start AND @End 
        ORDER BY work_date ASC, employee_id ASC"
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

    Public Function Save() As Long
      Dim query As String = "
        USE TimeStore;

        SET NOCOUNT, XACT_ABORT ON;

        MERGE Work_Hours WITH (HOLDLOCK) AS W

        USING (SELECT @work_date, @employee_id) AS NW (work_date, employee_id)
          ON NW.employee_id = W.employee_id AND NW.work_date = W.work_date

        WHEN MATCHED THEN
          
          UPDATE 
             SET 
                ,dept_id = @dept_id
                ,pay_period_ending = @pay_period_ending
                ,work_times = @work_times
                ,disaster_work_times = @disaster_work_times
                ,disaster_work_hours = @disaster_work_hours
                ,break_credit = @break_credit
                ,work_hours = @work_hours
                ,holiday = @holiday
                ,leave_without_pay = @leave_without_pay
                ,total_hours = @total_hours
                ,doubletime_hours = @doubletime_hours
                ,vehicle = @vehicle
                ,comment = @comment
                ,out_of_class = @out_of_class
                ,date_last_updated = GETDATE()
                ,by_employeeid = 
                ,by_username = <by_username, varchar(100),>
                ,by_machinename = <by_machinename, varchar(100),>
                ,by_ip_address = <by_ip_address, varchar(20),>

        WHEN NOT MATCHED THEN

          );"
      Return -1
    End Function

  End Class

End Namespace