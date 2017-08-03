Imports System.DirectoryServices
Imports TimeStore.Models
Imports System.IO
Imports System.Data
Imports System.Data.SqlClient
Imports System.Runtime.Caching
Imports System.Linq.Dynamic
Imports System.Diagnostics
Imports Dapper

Public Module ModuleDataAccess

  Public Function Exec_Query(query As String, dbA As DynamicParameters, cst As ConnectionStringType) As Integer
    Try
      Using db As IDbConnection = New SqlConnection(GetCS(cst))
        Return db.Execute(query, dbA)
      End Using
    Catch ex As Exception
      Dim e As New ErrorLog(ex, query)
      Return -1
    End Try
  End Function



  Public Function GetCS(cst As ConnectionStringType) As String
    ' This function will return a specific connectionstring based on the machine it's currently running on
    ' MSIL03, CLAYBCCDV10 = Development / Testing
    ' CLAYBCCIIS01 = Production
    Select Case Environment.MachineName.ToUpper
      Case "MISML01", "CLAYBCCDV10" ' QA
        Select Case cst
          Case ConnectionStringType.Telestaff
            Return ConfigurationManager.ConnectionStrings("TimestoreProduction").ConnectionString
          Case ConnectionStringType.Timecard
            Return ConfigurationManager.ConnectionStrings("TimestoreQA").ConnectionString
            'Return ConfigurationManager.ConnectionStrings("TimecardProduction").ConnectionString
            'Return ConfigurationManager.ConnectionStrings("TimestoreProduction").ConnectionString
          Case ConnectionStringType.Timestore
            Return ConfigurationManager.ConnectionStrings("TimestoreQA").ConnectionString
            'Return ConfigurationManager.ConnectionStrings("TimestoreProduction").ConnectionString

          Case ConnectionStringType.FinPlus
            'Return ConfigurationManager.ConnectionStrings("FinplusQA").ConnectionString
            Return ConfigurationManager.ConnectionStrings("FinplusProduction").ConnectionString
          Case ConnectionStringType.Log
            Return ConfigurationManager.ConnectionStrings("Log").ConnectionString

          Case Else
            Return ""
        End Select

      Case "CLAYBCCIIS01" ' Production
        Select Case cst
          Case ConnectionStringType.Telestaff
            Return ConfigurationManager.ConnectionStrings("TimestoreProduction").ConnectionString
          Case ConnectionStringType.Timecard
            Return ConfigurationManager.ConnectionStrings("TimecardProduction").ConnectionString

          Case ConnectionStringType.Timestore
            Return ConfigurationManager.ConnectionStrings("TimestoreProduction").ConnectionString

          Case ConnectionStringType.FinPlus
            Return ConfigurationManager.ConnectionStrings("FinplusProduction").ConnectionString

          Case ConnectionStringType.Log
            Return ConfigurationManager.ConnectionStrings("Log").ConnectionString

          Case Else
            Return ""
        End Select

      Case Else
        Return ""

    End Select
  End Function

  Public Function GetADEmployeeData() As List(Of AD_EmployeeData)
    Dim key As String = "employee_ad_data"
    Dim adl As List(Of AD_EmployeeData) = myCache.GetItem(key)
    Return adl
  End Function

  Public Function GetEmployeeDataFromFinPlus() As List(Of FinanceData)
    Dim key As String = "employeedata" ' & PayPeriodStart.ToShortDateString
    Dim fdl As List(Of FinanceData) = myCache.GetItem(key)
    Return fdl
  End Function

  Public Function GetEmployeeDataFromFinPlus(DepartmentList As List(Of String)) As List(Of FinanceData)
    Dim key As String = "employeedata" ' & PayPeriodStart.ToShortDateString
    Dim fdl As List(Of FinanceData) = myCache.GetItem(key)
    Return (From f In fdl Where DepartmentList.Contains(f.Department) Select f).ToList
  End Function

  Public Function GetEmployeeDataFromFinPlus(EmployeeID As Integer) As List(Of FinanceData)
    Dim key As String = "employeedata" ' & PayPeriodStart.ToShortDateString
    Dim fdl As List(Of FinanceData) = myCache.GetItem(key)
    Return (From f In fdl Where f.EmployeeId = EmployeeID Select f).ToList
  End Function

  Public Function GetEmployeeDataFromFinPlus(DepartmentList As List(Of String), EmployeeID As Integer) As List(Of FinanceData)
    Dim key As String = "employeedata" '& PayPeriodStart.ToShortDateString
    Dim fdl As List(Of FinanceData) = myCache.GetItem(key)
    Return (From f In fdl Where DepartmentList.Contains(f.Department) And f.EmployeeId = EmployeeID Select f).ToList
  End Function

  Public Function GetAllEmployeeDataFromFinPlus() As List(Of FinanceData)
    ' This pulls the employee data from Pentamation for the list of departments 
    'Dim sbQuery As New StringBuilder, 
    Dim dbc As New Tools.DB(GetCS(ConnectionStringType.FinPlus), toolsAppId, toolsDBError)
    Dim query As String = "
      USE finplus50;
      SELECT 
        E.hire_date, 
        E.empl_no, 
        LTRIM(RTRIM(E.l_name)) AS l_name, 
        LTRIM(RTRIM(E.f_name)) AS f_name, 
        E.home_orgn AS department, 
        E.birthdate, C.title, 
        D.desc_x AS department_name, 
        PR.classify, P.part_time, 
        PR.rate, 
        PR.pay_hours, 
        PAY.lv1_cd, 
        PAY.lv1_bal, 
        PAY.lv2_cd, 
        PAY.lv2_bal, 
        PAY.lv3_cd, 
        PAY.lv3_bal, 
        PAY.lv4_cd, 
        PAY.lv4_bal, 
        PAY.lv5_cd, 
        PAY.lv5_bal, 
        P.empl_type, 
        P.term_date 
      FROM employee E 
      INNER JOIN person P ON E.empl_no=P.empl_no 
      INNER JOIN payrate PR ON E.empl_no=PR.empl_no 
      INNER JOIN clstable C on PR.classify = C.class_cd
      INNER JOIN payroll PAY ON E.empl_no = PAY.empl_no 
      INNER JOIN dept D ON E.home_orgn=D.code 
      WHERE 
        PR.pay_cd IN ('001', '002') 
        AND (PR.rate_no = 1) 
      ORDER BY E.l_name ASC, E.f_name ASC
"
    'With sbQuery ' This humongous sql query was pulled from the telestaff report system and then tweaked.
    '  .AppendLine("USE finplus50;")
    '  .AppendLine("SELECT E.hire_date, E.empl_no, LTRIM(RTRIM(E.l_name)) AS l_name, LTRIM(RTRIM(E.f_name)) AS f_name, ")
    '  .AppendLine("E.home_orgn AS department, E.birthdate, C.title, D.desc_x AS department_name, PR.classify, P.part_time, ")
    '  .AppendLine("PR.rate, PR.pay_hours, PAY.lv1_cd, PAY.lv1_bal, PAY.lv2_cd, PAY.lv2_bal, PAY.lv3_cd, PAY.lv3_bal, PAY.lv4_cd, PAY.lv4_bal, ")
    '  .AppendLine("PAY.lv5_cd, PAY.lv5_bal, P.empl_type, P.term_date FROM employee E INNER JOIN person P ON E.empl_no=P.empl_no ")
    '  .AppendLine("INNER JOIN payrate PR ON E.empl_no=PR.empl_no INNER JOIN clstable C on PR.classify = C.class_cd ")
    '  .AppendLine("INNER JOIN payroll PAY ON E.empl_no = PAY.empl_no INNER JOIN dept D ON E.home_orgn=D.code WHERE PR.pay_cd IN ('001', '002') ")
    '  .Append("AND (PR.rate_no = 1) ")
    '  '.Append("AND (P.term_date IS NULL OR P.term_date >= DATEADD(mm,-3, GETDATE())) ")
    '  'If DepartmentList.Length > 0 Then
    '  '    .Append(" AND E.home_orgn IN (").Append(DepartmentList).Append(")")
    '  'End If
    '  'If EmployeeID.Length > 0 Then
    '  '    .Append(" AND E.empl_no = '").Append(EmployeeID).Append("'")
    '  'End If
    '  .Append(" ORDER BY E.l_name ASC, E.f_name ASC")
    'End With
    Dim ds As DataSet = dbc.Get_Dataset(query)
    'Dim tb As DataSet = GetTimeBankData(StartDate)
    Try
      'Dim tmp As List(Of FinanceData) = (From dr In ds.Tables(0).AsEnumerable Select New FinanceData(dr)).ToList
      'tmp = (From dr In ds.Tables(0).AsEnumerable() Select GetEmployeePayPeriodByDataRow(dr)).ToList
      Return (From dr In ds.Tables(0).AsEnumerable Select New FinanceData(dr)).ToList
    Catch ex As Exception
      Log(ex)
      Return Nothing
    End Try
  End Function

  Public Function GetDepartmentListFromFinPlus() As List(Of FinplusDepartment)
    ' This pulls the employee data from Pentamation for the list of departments 
    Dim sbQuery As New StringBuilder, dbc As New Tools.DB(GetCS(ConnectionStringType.FinPlus), toolsAppId, toolsDBError)
    With sbQuery ' This humongous sql query was pulled from the telestaff report system and then tweaked.
      .AppendLine("USE finplus50;")
      .Append("SELECT LTRIM(RTRIM(UPPER(code))) AS code, LTRIM(RTRIM(UPPER(desc_x))) AS desc_x FROM dept ORDER BY desc_x ASC")
    End With
    Dim ds As DataSet = dbc.Get_Dataset(sbQuery.ToString)
    Try
      Dim tmp As New List(Of FinplusDepartment)(From d In ds.Tables(0).AsEnumerable() Select New FinplusDepartment With {
                  .Department = d("desc_x"), .DepartmentNumber = d("code")})
      Return tmp
    Catch ex As Exception
      Log(ex)
      Return Nothing
    End Try
  End Function

  Public Function GetEmployeeDataFromTimecardOrTimeStore(Startdate As Date, EmployeeID As Integer) As List(Of TimecardTimeData)
    If EmployeeID = 0 Then
      Return New List(Of TimecardTimeData)
    End If
    Dim tctdl As List(Of TimecardTimeData) = GetEmployeeDataFromTimeStore(Startdate, EmployeeID)
    If tctdl.Count = 0 Then
      Return GetEmployeeDataFromTimecard(Startdate, EmployeeID)
    Else
      Return tctdl
    End If
  End Function

  Public Function GetEmployeeDataFromTimecard(Startdate As Date, Optional EmployeeID As Integer = 0, Optional EndDate? As Date = Nothing) As List(Of TimecardTimeData)

    'If Username.Length > 0 Then EmployeeID = GetEmployeeIDFromAD(Username)
    ' This gets the current pay period start for the start date passed.  So you can pass today's date and it'll get the 
    ' current pay period, or if you want to be specific you can pass a specific pay period start date.
    If Not EndDate.HasValue Then Startdate = GetPayPeriodStart(Startdate)
    Dim sbQuery As New StringBuilder
    Dim dbc As New Tools.DB(GetCS(ConnectionStringType.Timecard), toolsAppId, toolsDBError)
    With sbQuery
      .AppendLine("USE TimeSheet_Info;")
      .Append("DECLARE @Start DATETIME = '").Append(Startdate.ToShortDateString).Append("';")
      If EndDate.HasValue Then
        .Append("DECLARE @End DATETIME = '").Append(EndDate.Value.ToShortDateString).Append("';")
      Else
        .AppendLine("DECLARE @End DATETIME = DATEADD(dd, 13, @Start);")
      End If

      .AppendLine("SELECT empl_no, home_orgn, ddate, timeFormatted, work, [break] AS breakcredit, holiday, vacation, sick, ct_paid, lwop, [admin], ")
      .AppendLine("call_adj, total, vehicle, dbl_time, comment, ppd, ct_earned, sickpool FROM tbltimeentry ")
      .AppendLine("WHERE ddate BETWEEN @Start AND @End ")
      If EmployeeID > 0 Then .Append("AND empl_no='").Append(EmployeeID).AppendLine("'")
      .AppendLine("ORDER BY ddate ASC, empl_no ASC")
    End With
    Try
      Dim ds As DataSet = dbc.Get_Dataset(sbQuery.ToString)
      Dim tmp As New List(Of TimecardTimeData)(From dr In ds.Tables(0).AsEnumerable() Select New TimecardTimeData With {
                           .EmployeeID = dr("empl_no"), .WorkDate = dr("ddate"), .DepartmentNumber = dr("home_orgn"),
                           .WorkTimes = dr("timeFormatted"), .WorkHours = dr("work"), .BreakCreditHours = dr("breakcredit"),
                           .HolidayHours = dr("holiday"), .VacationHours = dr("vacation"), .SickHours = dr("sick"),
                           .CompTimeUsed = dr("ct_paid"), .CompTimeEarned = dr("ct_earned"), .LWOPHours = dr("lwop"),
                           .AdminHours = dr("admin"), .TotalHours = dr("total"), .Vehicle = dr("vehicle"),
                           .DoubleTimeHours = dr("dbl_time"), .Comment = dr("comment"), .PPD = dr("ppd"),
                           .OnCallTotalHours = dr("call_adj"), .SickLeavePoolHours = dr("sickpool"),
                           .DataSource = TimecardTimeData.TimeCardDataSource.Timecard})
      Return tmp
    Catch ex As Exception
      Log(ex)
      Return Nothing
    End Try
  End Function

  Public Function GetEmployeeDataFromTimeStore(Startdate As Date, Optional EmployeeID As Integer = 0, Optional EndDate? As Date = Nothing) As List(Of TimecardTimeData)
    Dim STDL As List(Of Saved_TimeStore_Data) = GetWorkHoursDataFromTimeStore(Startdate, EmployeeID, EndDate)
    Return (From s In STDL Select New TimecardTimeData(s)).ToList
  End Function

  Public Function GetCompTimeEarnedDataFromTimeStore(Startdate As Date, Optional EmployeeID As Integer = 0, Optional EndDate? As Date = Nothing) As List(Of Saved_TimeStore_Comp_Time_Earned)
    Startdate = GetPayPeriodStart(Startdate)
    Dim sbQueryWorkHours As New StringBuilder
    Dim dbc As New Tools.DB(GetCS(ConnectionStringType.Timestore), toolsAppId, toolsDBError)
    With sbQueryWorkHours
      .AppendLine("USE TimeStore;")
      .Append("DECLARE @Start DATETIME = '").Append(Startdate.ToShortDateString).Append("';")
      If EndDate.HasValue Then
        .Append("DECLARE @End DATETIME = '").Append(EndDate.Value.ToShortDateString).Append("';")
      Else
        .AppendLine("DECLARE @End DATETIME = DATEADD(dd, 13, @Start);")
      End If
      .AppendLine("SELECT employee_id, pay_period_ending, comp_time_earned_week1, comp_time_earned_week2, ")
      .AppendLine("date_added, date_last_updated, added_by_employeeid, added_by_username, added_by_machinename, ")
      .AppendLine("added_by_ip_address FROM Comp_Time_Earned_Hours ")
      .AppendLine("WHERE pay_period_ending BETWEEN @Start AND @End ")
      If EmployeeID > 0 Then .Append("AND employee_id='").Append(EmployeeID).AppendLine("'")
      .AppendLine("ORDER BY pay_period_ending ASC, employee_id ASC")
    End With
    Try

      Dim ds As DataSet = dbc.Get_Dataset(sbQueryWorkHours.ToString)
      Dim STDL As List(Of Saved_TimeStore_Comp_Time_Earned) = (From d In ds.Tables(0).AsEnumerable
                                                               Select New Saved_TimeStore_Comp_Time_Earned(d)).ToList
      Return STDL
    Catch ex As Exception
      Log(ex)
      Return Nothing
    End Try
  End Function

  Private Function GetWorkHoursDataFromTimeStore(Startdate As Date, Optional EmployeeID As Integer = 0, Optional EndDate? As Date = Nothing) As List(Of Saved_TimeStore_Data)
    'If Username.Length > 0 Then EmployeeID = GetEmployeeIDFromAD(Username)
    ' This gets the current pay period start for the start date passed.  So you can pass today's date and it'll get the 
    ' current pay period, or if you want to be specific you can pass a specific pay period start date.
    Startdate = GetPayPeriodStart(Startdate)
    Dim sbQueryWorkHours As New StringBuilder
    Dim dbc As New Tools.DB(GetCS(ConnectionStringType.Timestore), toolsAppId, toolsDBError)
    With sbQueryWorkHours
      .AppendLine("USE TimeStore;")
      .Append("DECLARE @Start DATETIME = '").Append(Startdate.ToShortDateString).Append("';")
      If EndDate.HasValue Then
        .Append("DECLARE @End DATETIME = '").Append(EndDate.Value.ToShortDateString).Append("';")
      Else
        .AppendLine("DECLARE @End DATETIME = DATEADD(dd, 13, @Start);")
      End If
      .Append("SELECT work_hours_id,employee_id,dept_id,pay_period_ending,work_date,")
      .Append("work_times,break_credit,work_hours,holiday,leave_without_pay,total_hours,")
      .Append("vehicle,comment,date_added,date_last_updated,by_employeeid,by_username")
      .Append(",by_machinename,by_ip_address,doubletime_hours ")
      .Append("FROM Work_Hours ")
      .AppendLine("WHERE work_date BETWEEN @Start AND @End ")
      If EmployeeID > 0 Then .Append("AND employee_id='").Append(EmployeeID).AppendLine("'")
      .AppendLine("ORDER BY work_date ASC, employee_id ASC")
    End With
    Try
      Dim stdtaList As List(Of Saved_TimeStore_Data_To_Approve) = GetHoursToApproveDataFromTimeStore(Startdate, EmployeeID, EndDate)
      Dim STDL As New List(Of Saved_TimeStore_Data)
      Dim ds As DataSet = dbc.Get_Dataset(sbQueryWorkHours.ToString)
      For Each d In ds.Tables(0).Rows
        Dim std As New Saved_TimeStore_Data(d)
        std.HoursToApprove.AddRange((From s In stdtaList Where s.work_hours_id = std.work_hours_id).ToList)
        STDL.Add(std)
      Next
      Return STDL
    Catch ex As Exception
      Log(ex)
      Return Nothing
    End Try
  End Function

  Private Function GetHoursToApproveDataFromTimeStore(Startdate As Date, Optional EmployeeID As Integer = 0, Optional EndDate? As Date = Nothing) As List(Of Saved_TimeStore_Data_To_Approve)
    'If Username.Length > 0 Then EmployeeID = GetEmployeeIDFromAD(Username)
    ' This gets the current pay period start for the start date passed.  So you can pass today's date and it'll get the 
    ' current pay period, or if you want to be specific you can pass a specific pay period start date.
    Startdate = GetPayPeriodStart(Startdate)
    Dim sbQuery As New StringBuilder
    Dim dbc As New Tools.DB(GetCS(ConnectionStringType.Timestore), toolsAppId, toolsDBError)
    With sbQuery
      .AppendLine("USE TimeStore;")
      .Append("DECLARE @Start DATETIME = '").Append(Startdate.ToShortDateString).Append("';")
      If EndDate.HasValue Then
        .Append("DECLARE @End DATETIME = '").Append(EndDate.Value.ToShortDateString).Append("';")
      Else
        .AppendLine("DECLARE @End DATETIME = DATEADD(dd, 13, @Start);")
      End If
      .AppendLine("SELECT H.approval_hours_id,H.work_hours_id,H.field_id,H.worktimes,H.hours_used, ")
      .AppendLine("H.payrate,H.date_added,A.approval_id,A.hours_approved,A.is_approved,A.by_employeeid, ")
      .AppendLine("A.by_username,A.by_machinename,A.by_ip_address,A.note,A.date_approval_added ")
      .AppendLine("FROM Hours_To_Approve H ")
      .AppendLine("INNER JOIN Work_Hours W ON W.work_hours_id = H.work_hours_id")
      .AppendLine("LEFT OUTER JOIN Approval_Data A ON H.approval_hours_id = A.approval_hours_id ")
      .AppendLine("WHERE (A.is_approved IS NULL OR A.is_approved = 1) ")
      .AppendLine("AND H.hours_used > 0 ")
      .AppendLine("AND W.work_date BETWEEN @Start AND @End ")
      If EmployeeID > 0 Then .Append("AND W.employee_id='").Append(EmployeeID).AppendLine("' ")
      .AppendLine("ORDER BY W.work_date ASC, W.employee_id ASC")
    End With
    Try
      Dim ds As DataSet = dbc.Get_Dataset(sbQuery.ToString)
      'Dim htal As List(Of Saved_TimeStore_Data_To_Approve) = (From d In ds.Tables(0).AsEnumerable
      '                                                        Select New Saved_TimeStore_Data_To_Approve(d)).ToList
      Return (From d In ds.Tables(0).AsEnumerable Select New Saved_TimeStore_Data_To_Approve(d)).ToList

    Catch ex As Exception
      Log(ex)
      Return Nothing
    End Try
  End Function

  Public Function GetTimeStoreAccess(EmployeeID As Integer) As DataSet
    ' This function gets the Employee's Access from the Timestore database - Access table.  The data is converted into 
    ' a TCA object.
    Dim sbQuery As New StringBuilder, dbc As New Tools.DB(GetCS(ConnectionStringType.Timestore), toolsAppId, toolsDBError)
    With sbQuery
      .AppendLine("USE Timestore;")
      .AppendLine("SELECT TOP 1 * FROM Access WHERE employee_id=@eid;")
    End With
    Dim p(0) As SqlParameter
    p(0) = New SqlParameter("@eid", Data.SqlDbType.Int) With {.Value = EmployeeID}
    Try
      Dim ds As DataSet = dbc.Get_Dataset(sbQuery.ToString, p)
      Return ds
    Catch ex As Exception
      Log(ex)
      Return Nothing
    End Try
  End Function

  Public Function Get_Reporting_Users(employeeId As Integer) As List(Of Integer)
    ' This function will query the Access table in the Timestore database to return a list of all of the users set to 
    ' Report to employeeId
    Dim dbc As New Tools.DB(GetCS(ConnectionStringType.Timestore), toolsAppId, toolsDBError)
    Dim query As String = "USE TimeStore; SELECT employee_id FROM Access WHERE reports_to=" & employeeId
    Try
      'Dim ilist As New List(Of Integer)
      Dim ds As DataSet = dbc.Get_Dataset(query)
      Dim iList As List(Of Integer) = (From d In ds.Tables(0).AsEnumerable Select CType(d("employee_id"), Integer)).ToList
      Return iList
    Catch ex As Exception
      Log(ex)
      Return New List(Of Integer)
    End Try
  End Function

  Public Function Get_Higher_Access_Users(accessType As Integer) As List(Of Integer)
    ' This function will query the Access table in the Timestore database to return a list of all of the users set to 
    ' Report to employeeId
    Dim dbc As New Tools.DB(GetCS(ConnectionStringType.Timestore), toolsAppId, toolsDBError)
    Dim query As String = "USE TimeStore; SELECT employee_id FROM Access WHERE access_type >= " & accessType
    Try
      'Dim ilist As New List(Of Integer)
      Dim ds As DataSet = dbc.Get_Dataset(query)
      Dim iList As List(Of Integer) = (From d In ds.Tables(0).AsEnumerable Select CType(d("employee_id"), Integer)).ToList
      Return iList
    Catch ex As Exception
      Log(ex)
      Return New List(Of Integer)
    End Try
  End Function

  Public Function Get_Valid_Reports_To_Users() As List(Of Integer)
    ' This function will pull all of the employee numbers from the access table that have a level 2 or higher access level.
    ' the purpose of this is to filter out who is a valid target.
    Dim dbc As New Tools.DB(GetCS(ConnectionStringType.Timestore), toolsAppId, toolsDBError)
    Dim query As String = "USE TimeStore; SELECT employee_id FROM Access WHERE access_type > 1"
    Try
      'Dim ilist As New List(Of Integer)
      Dim ds As DataSet = dbc.Get_Dataset(query)
      Dim iList As List(Of Integer) = (From d In ds.Tables(0).AsEnumerable Select CType(d("employee_id"), Integer)).ToList
      Return iList
    Catch ex As Exception
      Log(ex)
      Return New List(Of Integer)
    End Try
  End Function

  Public Function Add_Timestore_Note(EmployeeId As Integer, PayPeriodEnding As Date, Note As String, Optional AddedBy As String = "SYSTEM") As Boolean
    Dim dbc As New Tools.DB(GetCS(ConnectionStringType.Timestore), toolsAppId, toolsDBError)
    Dim sbQ As New StringBuilder
    Dim P() As SqlParameter = New SqlParameter() _
        {
            New SqlParameter("@EmployeeId", Data.SqlDbType.Int) With {.Value = EmployeeId},
            New SqlParameter("@PayPeriodEnding", Data.SqlDbType.Date) With {.Value = PayPeriodEnding},
            New SqlParameter("@Note", Data.SqlDbType.VarChar, 1000) With {.Value = Note},
            New SqlParameter("@AddedBy", Data.SqlDbType.VarChar, 50) With {.Value = AddedBy}
        }
    Dim i As Integer = 0
    ' we update
    sbQ.Append("USE TimeStore; INSERT INTO notes (employee_id, pay_period_ending, note, note_added_by) VALUES ")
    sbQ.Append("(@EmployeeId, @PayPeriodEnding, @Note, @AddedBy);")
    Try
      i = dbc.ExecuteNonQuery(sbQ.ToString, P)
      Return i = 1
    Catch ex As Exception
      Log(ex)
      Return False
    End Try
  End Function

  Public Function GetEmployeeDataFromTelestaff(StartDate As Date, Optional ByVal EmployeeID As String = "", Optional ByVal EndDate? As Date = Nothing) As List(Of TelestaffTimeData)
    Dim I As List(Of Incentive) = myCache.GetItem("incentive")
    Dim sbQuery As New StringBuilder, dbc As New Tools.DB(GetCS(ConnectionStringType.Telestaff), toolsAppId, toolsDBError)
    With sbQuery ' This humongous sql query was pulled from the telestaff report system and then tweaked.
      .AppendLine("USE Telestaff;")
      .Append("DECLARE @Start DATETIME = '").Append(StartDate.ToShortDateString).Append("';")
      If EndDate.HasValue Then
        .Append("DECLARE @End DATETIME = '").Append(EndDate.Value.ToShortDateString).Append("';")
      Else
        .AppendLine("DECLARE @End DATETIME = DATEADD(dd, 13, @Start);")
      End If

      .AppendLine("SELECT R.payinfo_no_in, R.rsc_no_in, R.rsc_desc_ch, R.Rsc_Hourwage_db, R.Rsc_From_Da, R.Rsc_Thru_Da, AAA.* FROM (SELECT Resource_Master_Tbl.RscMaster_No_In, Staffing_Tbl.Staffing_Calendar_Da, ")
      .AppendLine("SUM(DATEDIFF(minute,Staffing_Tbl.Staffing_Start_Dt,Staffing_Tbl.Staffing_End_Dt))/60.00 as StaffingHours, Wstat_Cde_Tbl.Wstat_FLSA_Si, Wstat_Cde_Tbl.Wstat_WageFactor_In,")
      .AppendLine("Staffing_Tbl.Staffing_Benign_Si AS RequiresApproval, Resource_Master_Tbl.RscMaster_Name_Ch,Resource_Master_Tbl.RscMaster_EmployeeID_Ch, ")
      .AppendLine("Staffing_Tbl.Staffing_Start_Dt,Staffing_Tbl.Staffing_End_Dt,Job_Title_Tbl.Job_Abrv_Ch,Wstat_Type_Tbl.Wstat_Type_Desc_Ch,")
      .AppendLine("(CASE WHEN LTRIM(RTRIM(Wstat_Cde_Tbl.Wstat_Abrv_Ch)) = '' THEN 'Straight' ELSE UPPER(LTRIM(RTRIM(Wstat_Cde_Tbl.Wstat_Abrv_Ch))) END) AS WstatAbrv,")
      .AppendLine("Wstat_Cde_Tbl.Wstat_Name_Ch AS WstatName,Wstat_Payroll_ch,Pay_Information_Tbl.PayInfo_FlsaHours_In,")
      .AppendLine("Shift_tbl.shift_abrv_ch, shift_tbl.Shift_TimeDuration_Ch, shift_tbl.shift_type_no_in, ")
      .AppendLine("CASE WHEN Staffing_Tbl.Staffing_Benign_Si = 'Y' THEN ISNULL(Staffing_tbl.Staffing_Note_Vc, '') + ")
      .AppendLine("' *** Unapproved in Telestaff' ELSE ISNULL(Staffing_tbl.Staffing_Note_Vc, '') END AS Comment, ")
      .AppendLine("ISNULL(dbo.GetRscSpecialties(Resource_Tbl.Rsc_No_In,1), '') as Specialties, ISNULL(STRAT.strat_name_ch, '') AS StratName ")
      .AppendLine("FROM Staffing_Tbl ")
      .AppendLine("LEFT OUTER JOIN strategy_tbl STRAT ON Staffing_tbl.strat_no_in=STRAT.strat_no_in ")
      .AppendLine("JOIN Resource_Tbl ON Resource_Tbl.Rsc_No_In=Staffing_Tbl.Rsc_No_In ")
      .AppendLine("JOIN Wstat_Cde_Tbl ON Wstat_Cde_Tbl.Wstat_No_In=Staffing_Tbl.Wstat_No_In ")
      .AppendLine("JOIN Shift_Tbl ON Shift_Tbl.Shift_No_In=Staffing_Tbl.Shift_No_In ")
      .AppendLine("JOIN Wstat_Type_Tbl ON Wstat_Type_Tbl.Wstat_Type_No_In=Wstat_Cde_Tbl.Wstat_Type_No_In ")
      .AppendLine("JOIN Job_Title_Tbl ON Job_Title_Tbl.Job_No_In=Resource_Tbl.Job_No_In ")
      .AppendLine("LEFT OUTER JOIN Pay_Information_Tbl ON Pay_Information_Tbl.PayInfo_No_In=Resource_Tbl.PayInfo_No_In ")
      .AppendLine("JOIN Resource_Master_Tbl ON Resource_Master_Tbl.RscMaster_No_In=Resource_Tbl.RscMaster_No_In ")
      .AppendLine("JOIN Position_Tbl ON Position_Tbl.Pos_No_In=Staffing_Tbl.Pos_No_In ")
      .AppendLine("JOIN Unit_Tbl ON Unit_Tbl.Unit_No_In=Position_Tbl.Unit_No_In ")
      .AppendLine("JOIN Station_Tbl ON Station_Tbl.Station_No_In=Unit_Tbl.Station_No_In ")
      .AppendLine("WHERE Staffing_Tbl.Staffing_Calendar_Da BETWEEN @Start AND @End AND Station_Tbl.Region_No_In IN (4,2,5,6) ")
      ' Excluding the following work codes:
      ' OTR -  Overtime Reject, field personnel were offered OT by Telestaff and didn't take it.
      ' ORD - same as above but for Dispatch
      ' OTRR - reject for rapid hire.  Same as above
      ' DMWI - Dispatch shift trade, working.  This lets you know that the dispatcher traded shifts with someone and now they are working the shift in repayment.
      ' MWI - Same as above just for field personnel instead of dispatch
      ' SLOT - Sick leave on OT, person accepted OT but was sick.  Just used by Telestaff to fill the vacancy.
      ' BR - Break for staff employees, this is used to accurately calculate their schedules, rather than using the automatically calculated break.
      ' OJ - OJI on OT, these hours are not paid.
      ' ADMNSWAP - Admin leave on Swap time, so it's not paid time.
      ' OR - Off roster
      ' 8/1/2014, these were moved to the NonPaid array and are now manually excluded.
      ' The reason for the change is because we will need to see all of the time to correctly render a user's timesheet.
      '.AppendLine("AND Wstat_Cde_Tbl.Wstat_Abrv_Ch NOT IN ('ADMNSWAP', 'OTR', 'OTRR', 'DMWI', 'MWI', 'ORD', 'SLOT', 'BR', 'OJ', 'OR') ")
      ' 8/4/2014, we're going to keep the exclusions that are just used by telestaff to fill a spot on the roster.
      If EmployeeID.Length > 0 Then .Append("AND Resource_Master_Tbl.RscMaster_EmployeeID_Ch = '").Append(EmployeeID).AppendLine("'")

      'If Not EmployeeList Is Nothing Then
      '    .Append("AND Resource_Master_Tbl.RscMaster_EmployeeID_Ch IN (")
      '    For a As Integer = 0 To EmployeeList.Count - 1
      '        .Append("'").Append(EmployeeList(a)).Append("'")
      '        If a < EmployeeList.Count - 1 Then .Append(",")
      '    Next
      '    .AppendLine(") ")
      'End If
      If StartDate < CType("8/11/2015", Date) Or StartDate > CType("8/25/2015", Date) Then .Append("AND Resource_Master_Tbl.RscMaster_EmployeeID_Ch <> '2201' ") ' Jesse Hellard Exclusion while he works Animal Control and Public Safety until 8/4
      .AppendLine("AND Wstat_Cde_Tbl.Wstat_Abrv_Ch NOT IN ('OTR', 'OTRR', 'ORD', 'ORRD', 'NO', 'DPRN') ")
      '.AppendLine("AND Resource_Tbl.rsc_no_in <> 1795 ")
      ' Old version, this was determined to be incorrect when we have an end date entered that was after our start/end date
      '.AppendLine("AND (Resource_Master_Tbl.RscMaster_Thru_Da IS NULL OR Resource_Master_Tbl.RscMaster_Thru_Da BETWEEN @Start AND @End) ")
      .AppendLine("AND (Resource_Master_Tbl.RscMaster_Thru_Da IS NULL OR Resource_Master_Tbl.RscMaster_Thru_Da >= @Start) ")
      .AppendLine("GROUP BY Staffing_Tbl.Staffing_Calendar_Da,Shift_Tbl.Shift_Type_No_In,Staffing_Tbl.Rsc_No_In,Resource_Master_Tbl.RscMaster_No_In,")
      .AppendLine("Resource_Master_Tbl.RscMaster_Name_Ch,Resource_Master_Tbl.RscMaster_EmployeeID_Ch,Resource_Master_Tbl.RscMaster_PayrollID_Ch,")
      .AppendLine("Resource_Master_Tbl.RscMaster_Contact1_Ch,Resource_Master_Tbl.RscMaster_Contact2_Ch,Resource_Tbl.Rsc_Job_Level_Ch,")
      .AppendLine("Resource_Tbl.Rsc_No_In,Job_Title_Tbl.Job_Abrv_Ch,Wstat_Type_Tbl.Wstat_Type_No_In,Wstat_Type_Tbl.Wstat_Type_desc_Ch,")
      .AppendLine("Wstat_Cde_Tbl.Wstat_No_In,Wstat_Cde_Tbl.Wstat_Name_Ch,Wstat_Cde_Tbl.Wstat_Abrv_Ch,Wstat_Cde_Tbl.Wstat_Payroll_Ch,Staffing_tbl.Staffing_Note_Vc,")
      .AppendLine("Pay_Information_Tbl.PayInfo_FlsaHours_In,shift_tbl.shift_abrv_ch, shift_tbl.Shift_TimeDuration_Ch,Staffing_Tbl.Staffing_Start_Dt,")
      .AppendLine("Staffing_Tbl.Staffing_End_Dt, Staffing_Tbl.Staffing_Benign_Si,Wstat_Cde_Tbl.Wstat_FLSA_Si, Wstat_Cde_Tbl.Wstat_WageFactor_In,STRAT.strat_name_ch) AS AAA ")
      .AppendLine("LEFT OUTER JOIN Resource_Tbl R ON AAA.RscMaster_No_In = R.RscMaster_No_In ")
      .AppendLine("WHERE staffing_calendar_da BETWEEN ISNULL(R.Rsc_From_Da, @Start) AND ISNULL(R.Rsc_Thru_Da, @End) ")
      .AppendLine("ORDER BY AAA.RscMaster_Name_Ch ASC, AAA.staffing_calendar_da ASC, AAA.staffing_start_dt ASC") ' AND R.rsc_disable_si='N' ' Removed 8/20/2014
    End With
    Try
      Dim ds As DataSet = dbc.Get_Dataset(sbQuery.ToString)
      Dim tmp As New List(Of TelestaffTimeData)(
        From dbRow In ds.Tables(0).AsEnumerable()
        Select New TelestaffTimeData With {
          .EmployeeId = dbRow("RscMaster_EmployeeID_Ch"),
          .RequiresApproval = (dbRow("RequiresApproval") = "Y"),
          .WorkCode = IsNull(dbRow("Wstat_Payroll_Ch"), "000"),
          .WorkDate = dbRow("Staffing_Calendar_Da"),
          .WorkHours = IsNull(dbRow("StaffingHours"), "0"),
          .ShiftType = dbRow("shift_abrv_ch"),
          .Comment = dbRow("Comment"),
          .Job = dbRow("Job_Abrv_Ch"),
          .ConstantShift = (dbRow("shift_type_no_in") = 0),
          .ProfileType = dbRow("payinfo_no_in"),
          .ProfileID = dbRow("rsc_no_in"),
          .ProfileDesc = dbRow("rsc_desc_ch"),
          .StratName = dbRow("StratName"),
          .FLSAHoursRequirement = IsNull(dbRow("PayInfo_FlsaHours_In"), "0"),
          .WorkType = dbRow("WstatName"),
          .WorkTypeAbrv = dbRow("WstatAbrv"),
          .StartTime = dbRow("Staffing_Start_Dt"),
          .EndTime = dbRow("Staffing_End_Dt"),
          .IsWorkingTime = (dbRow("Wstat_Type_desc_Ch").ToString.Trim.ToUpper <> "NON WORKING"),
          .IsPaidTime = Not IsDBNull(dbRow("Wstat_WageFactor_In")),
          .WageFactor = IsNull(dbRow("Wstat_WageFactor_In"), 0),
          .CountsTowardsOvertime = (dbRow("Wstat_FLSA_Si").ToString.Trim.ToUpper = "Y"),
          .Specialties = dbRow("Specialties"),
          .ProfileStartDate = IsNull(dbRow("Rsc_From_Da"), Date.MinValue),
          .ProfileEndDate = IsNull(dbRow("Rsc_Thru_Da"), Date.MaxValue),
          .PayRate = Calculate_PayRate_With_Incentives(IsNull(dbRow("Rsc_Hourwage_db"), 0), .Specialties, .Job, .WorkTypeAbrv, .ProfileType, I)})
      '.ShiftDuration = dbRow("Shift_TimeDuration_Ch").ToString.Split(",")(1),
      Return tmp
    Catch ex As Exception
      Log(ex)
      Return Nothing
    End Try
  End Function

  Public Function GetTelestaffSpecialties() As List(Of Incentive)
    Dim sbQuery As New StringBuilder, dbc As New Tools.DB(GetCS(ConnectionStringType.Telestaff), toolsAppId, toolsDBError)
    With sbQuery ' This humongous sql query was pulled from the telestaff report system and then tweaked.
      .AppendLine("USE Telestaff;")
      .AppendLine("SELECT Spec_Name_Ch AS Specialty_Name, Spec_Abrv_Ch AS Specialty_Abrv ")
      .AppendLine("FROM Specialty_Tbl WHERE spec_disable_si='N' ORDER BY Spec_Name_Ch ASC")
    End With
    Try
      Dim ds As DataSet = dbc.Get_Dataset(sbQuery.ToString)
      Dim tmp As List(Of Incentive) = (From d In ds.Tables(0).AsEnumerable Select New Incentive With {
                                      .Incentive_Abrv = d("Specialty_Abrv"), .Incentive_Name = d("Specialty_Name"),
                                      .Incentive_Amount = 0, .Incentive_Type = 1}).ToList
      Return tmp
    Catch ex As Exception
      Log(ex)
      Return Nothing
    End Try
  End Function

  Public Function Get_All_Saved_Timecard_Data(PayPeriodEnding As Date) As List(Of Saved_Timecard_Data)
    Dim dbc As New Tools.DB(GetCS(ConnectionStringType.Timestore), toolsAppId, toolsDBError)

    'Dim query As String = "USE Timestore; SELECT * FROM Saved_Time WHERE pay_period_ending='" & PayPeriodEnding.ToShortDateString & "';"
    Dim sbQ As New StringBuilder
    With sbQ
      .AppendLine("USE TimeStore;")
      .AppendLine("SELECT ISNULL(A.data_type, CASE WHEN ST.orgn IN ('1703', '2103') THEN ") ' Removed 2102
      .AppendLine(" 'telestaff' ELSE 'timecard' END) AS data_type, ISNULL(A.access_type, 1) AS access_type, ")
      .AppendLine("ISNULL(A2.access_type, ISNULL(A.access_type, 1)) AS initial_approval_employeeid_access_type, ")
      .AppendLine("ISNULL(A.reports_to, 0) AS reports_to, ST.pay_period_ending, ST.employee_id, ST.paycode, ")
      .AppendLine("ST.payrate, ST.hours, ST.amount, ST.orgn, ST.classify, ST.date_added, ST.date_updated, ")
      .AppendLine("ST.initial_approval_username, ST.initial_approval_employeeid, ST.initial_approval_machine_name, ")
      .AppendLine("ST.initial_approval_ip_address, ST.initial_approval_date, ")
      .AppendLine("ST.final_approval_username, ST.final_approval_employeeid, ST.final_approval_machine_name, ")
      .AppendLine("ST.final_approval_ip_address, ST.final_approval_date ")
      .AppendLine("FROM Saved_Time ST ")
      .AppendLine("LEFT OUTER JOIN Access A ON ST.employee_id = A.employee_id ")
      .AppendLine("LEFT OUTER JOIN Access A2 ON ST.initial_approval_employeeid = A2.employee_id ")
      .Append("WHERE ST.pay_period_ending = '").Append(PayPeriodEnding.ToShortDateString).Append("' ")
    End With
    Try
      Dim ds As DataSet = dbc.Get_Dataset(sbQ.ToString)
      Dim TTD As List(Of Saved_Timecard_Data) = (From d In ds.Tables(0).AsEnumerable Select New Saved_Timecard_Data With {
                          .Approved = Set_Approval_Level(d), .DepartmentNumber = d("orgn"),
                          .EmployeeId = d("employee_id"), .Hours = d("hours"), .Classify = d("classify"),
                          .PayCode = d("paycode"), .PayPeriodEnding = d("pay_period_ending"), .PayRate = d("payrate"),
                          .AccessType = d("access_type"), .DataType = d("data_type"), .ReportsTo = d("reports_to"),
                          .Initial_Approval_By_EmployeeID = IsNull(d("initial_approval_employeeid"), 0),
                          .Initial_Approval_Date = IsNull(d("initial_approval_date"), Date.MinValue),
                          .Initial_Approval_By_Name = Get_Employee_Name(.Initial_Approval_By_EmployeeID),
                          .Final_Approval_By_EmployeeID = IsNull(d("final_approval_employeeid"), 0),
                          .Final_Approval_Date = IsNull(d("final_approval_date"), Date.MinValue),
                          .Final_Approval_By_Name = Get_Employee_Name(.Final_Approval_By_EmployeeID),
                          .Initial_Approval_EmployeeID_AccessType = d("initial_approval_employeeid_access_type")}).ToList
      Return TTD
    Catch ex As Exception
      Log(ex)
      Return New List(Of Saved_Timecard_Data)
    End Try
  End Function

  Public Function Get_Employee_Name(ByVal EmployeeID As Integer) As String
    If EmployeeID = 0 Then Return ""
    Dim f As List(Of FinanceData) = GetEmployeeDataFromFinPlus(EmployeeID)
    If f.Count <> 1 Then Return "" Else Return f.First.EmployeeName
    'Dim e = (From f In fdl Where f.EmployeeId = EmployeeID Select f)
    'If e.Count <> 1 Then Return "" Else Return e.First.EmployeeName
  End Function

  Public Function Get_All_Notes(PayPeriodEnding As Date) As List(Of Note)
    '
    Dim dbc As New Tools.DB(GetCS(ConnectionStringType.Timestore), toolsAppId, toolsDBError)
    'Dim query As String = "USE Timestore; SELECT * FROM Saved_Time WHERE employee_id=" & employeeID & " AND pay_period_ending='" & payPeriodEndingDisplay & "';"
    Dim sbQ As New StringBuilder
    With sbQ
      .AppendLine("USE TimeStore;")
      .AppendLine("SELECT note, note_date_added, note_added_by, employee_id FROM notes N ")
      .Append("WHERE N.pay_period_ending = '").Append(PayPeriodEnding.ToShortDateString)
      .AppendLine("' ORDER BY note_date_added DESC;")
    End With
    Try
      Dim ds As DataSet = dbc.Get_Dataset(sbQ.ToString)

      Dim Notes As List(Of Note) = (From d In ds.Tables(0).AsEnumerable Select New Note With {
                      .Added_By = d("note_added_by"), .Date_Added = d("note_date_added"),
                      .Note = d("note"), .EmployeeID = d("employee_id"), .PayPeriodEnding = PayPeriodEnding}).ToList
      Return Notes
    Catch ex As Exception
      Log(ex)
      Return New List(Of Note)
    End Try
  End Function

  Public Function Get_All_Incentive_Data() As List(Of Incentive)
    Dim dbc As New Tools.DB(GetCS(ConnectionStringType.Timestore), toolsAppId, toolsDBError)
    Dim sbQ As New StringBuilder
    With sbQ
      .AppendLine("USE TimeStore;")
      .AppendLine("SELECT incentive_type, incentive_abrv, incentive, amount, start_date, end_date ")
      .AppendLine("FROM Incentives ORDER BY incentive_type ASC, incentive_abrv ASC")
    End With
    Try
      Dim ds As DataSet = dbc.Get_Dataset(sbQ.ToString)
      Dim TTD As List(Of Incentive) = (From d In ds.Tables(0).AsEnumerable Select New Incentive With {
                                      .Incentive_Type = d("incentive_type"), .Start_Date = d("start_date"),
                                      .Incentive_Name = d("incentive"), .Incentive_Amount = d("amount"),
                                      .Incentive_Abrv = d("incentive_abrv"), .End_Date = d("end_date")}).ToList

      Dim TS As List(Of Incentive) = GetTelestaffSpecialties()
      For Each t In TS
        If (From i In TTD Where i.Incentive_Abrv = t.Incentive_Abrv Select i).Count = 0 Then
          TTD.Add(t)
        End If
      Next

      Return TTD
    Catch ex As Exception
      Log(ex)
      Return New List(Of Incentive)
    End Try
  End Function

  Public Function Save_Incentive_Data(Incentives As List(Of Incentive)) As Boolean
    If Incentives.Count = 0 Then Return True
    Dim dbc As New Tools.DB(GetCS(ConnectionStringType.Timestore), toolsAppId, toolsDBError)
    Dim incentiveType As Integer = (From i In Incentives Select i.Incentive_Type).First
    Dim incentiveAbrv() As String = (From i In Incentives Select i.Incentive_Abrv).ToArray
    Dim sbQ As New StringBuilder
    With sbQ
      .AppendLine("USE TimeStore;")
      .AppendLine("DELETE FROM Incentives WHERE incentive_type=")
      .Append(incentiveType).Append(" AND incentive_abrv IN (")
      For a As Integer = incentiveAbrv.GetLowerBound(0) To incentiveAbrv.GetUpperBound(0)
        .Append("'").Append(incentiveAbrv(a)).Append("'")
        If a < incentiveAbrv.GetUpperBound(0) Then .Append(",")
      Next
      .AppendLine(");")
    End With
    Dim x As Integer = 0
    Try
      x = dbc.ExecuteNonQuery(sbQ.ToString)
    Catch ex As Exception
      Log(ex)
      Return False
    End Try
    With sbQ
      .Clear()
      .AppendLine("INSERT INTO Incentives (incentive_type, incentive_abrv, incentive, amount) ")
      .AppendLine("VALUES (@IncentiveType, @IncentiveAbrv, @Incentive, @Amount);")
    End With
    Try
      For Each i In Incentives

        Dim p(3) As SqlParameter
        p(0) = New SqlParameter("@IncentiveType", Data.SqlDbType.Int) With {.Value = i.Incentive_Type}
        p(1) = New SqlParameter("@IncentiveAbrv", Data.SqlDbType.VarChar, 10) With {.Value = i.Incentive_Abrv}
        p(2) = New SqlParameter("@Incentive", Data.SqlDbType.VarChar, 50) With {.Value = i.Incentive_Name}
        p(3) = New SqlParameter("@Amount", Data.SqlDbType.Decimal) With {.Value = i.Incentive_Amount}

        x = dbc.ExecuteNonQuery(sbQ.ToString, p)
      Next
      Return True
    Catch ex As Exception
      Log(ex)
      Return False
    End Try
  End Function

  Public Function GetEmployeeListFromFinPlus() As List(Of Employee_Data)
    ' This pulls the employee data from Pentamation for the list of departments 
    'Dim TG As Dictionary(Of Integer, String) = GetTelestaffGroups() 'GetTelestaffGroupingData()
    'Dim sbQuery As New StringBuilder, dbc As New Tools.DB(GetCS(ConnectionStringType.FinPlus), toolsAppId, toolsDBError)
    'With sbQuery ' This humongous sql query was pulled from the telestaff report system and then tweaked.
    '    .AppendLine("SELECT E.empl_no, LTRIM(RTRIM(E.l_name)) AS l_name, LTRIM(RTRIM(E.f_name)) AS f_name, ")
    '    .AppendLine("E.home_orgn AS department, D.desc_x AS department_name, P.term_date FROM employee E ")
    '    .AppendLine("INNER JOIN person P ON P.empl_no=E.empl_no INNER JOIN dept D ON E.home_orgn=D.code ")
    '    .AppendLine("WHERE DATEDIFF(dd, P.term_date, GETDATE()) < 41 OR P.term_date IS NULL ORDER BY D.desc_x ASC, E.l_name ASC, E.f_name ASC")
    'End With
    'Dim ds As DataSet = dbc.Get_Dataset(sbQuery.ToString)
    'Dim edl As New List(Of Employee_Data)(From d In ds.Tables(0).AsEnumerable() Select New Employee_Data With {
    '    .DepartmentID = d("department").ToString.Trim, .DepartmentName = d("department_name").ToString.Trim,
    '    .EmployeeID = d("empl_no"), .Firstname = d("f_name").ToString.Trim, .Lastname = d("l_name").ToString.Trim,
    '    .Terminated = Not IsDBNull(d("term_date")), .GroupName = GetGroupName(.EmployeeID, TG)})
    'Try
    '    Return edl
    'Catch ex As Exception
    '    Log(ex)
    '    Return Nothing
    'End Try
    Dim key As String = "employeedata," & GetPayPeriodStart(Today).ToShortDateString
    Dim fdl As List(Of FinanceData) = myCache.GetItem(key)
    Return (From f In fdl
            Where f.TerminationDate = Date.MaxValue Or f.TerminationDate > Date.Parse("1/1/2015")
            Select New Employee_Data(f)).ToList()
    'Select New Employee_Data With {
    '  .DepartmentID = f.Department,
    '  .DepartmentName = f.DepartmentName,
    '  .EmployeeID = f.EmployeeId,
    '  .Firstname = f.EmployeeFirstName,
    '  .GroupName = GetGroupName(f.EmployeeId, TG),
    '  .Lastname = f.EmployeeLastName,
    '  .TerminationDateDisplay = f.TerminationDate.ToShortDateString,
    '  .Terminated = (f.TerminationDate < Date.MaxValue)
    '  }).ToList
  End Function

  Public Function GetTelestaffGroups() As Dictionary(Of Integer, String)
    Dim CIP As New CacheItemPolicy
    CIP.AbsoluteExpiration = Now.AddHours(1)
    Return myCache.GetItem("telestaffgroupingdata", CIP)
  End Function

  Public Function GetTelestaffGroupingData() As Dictionary(Of Integer, String)
    Dim query As String = "
      USE Telestaff;
      Select distinct RM.RscMaster_EmployeeID_Ch As EmployeeID,
      (Case When RIGHT(LTRIM(RTRIM(ST.station_abrv_ch)), 2) In ('15', '17', '18', '20', '22', '24') 
      THEN 'BC1 - Shift ' + SH.shift_abrv_ch ELSE 
    Case WHEN RIGHT(LTRIM(RTRIM(ST.station_abrv_ch)), 2) IN ('11', '13', '14', '23', '25', '26') 
    THEN 'BC2 - Shift ' + SH.shift_abrv_ch ELSE 
    Case WHEN U.unit_abrv_ch IN ('BC1', 'BC2') 
    THEN U.unit_abrv_ch + ' - Shift ' + SH.shift_abrv_ch ELSE 
    Case WHEN U.unit_abrv_ch = 'CCU' THEN 'Dispatch' ELSE
    Case WHEN U.unit_abrv_ch = 'LTH'THEN 'LOGISTICS' ELSE
    U.unit_name_ch END END END END END) AS OtherGroup
    FROM Staffing_tbl S INNER JOIN Shift_Tbl SH ON SH.Shift_No_In=S.Shift_No_In
    INNER Join Resource_Tbl R ON S.rsc_no_in = R.rsc_no_in 
    INNER JOIN resource_master_tbl RM ON R.RscMaster_No_In = RM.RscMaster_No_In 
    INNER Join Wstat_Cde_Tbl W ON W.Wstat_No_In=S.Wstat_No_In 
    INNER JOIN Position_Tbl P ON P.Pos_No_In=S.Pos_No_In 
    INNER Join Unit_Tbl U ON U.Unit_No_In=P.Unit_No_In 
    INNER JOIN Station_Tbl ST ON U.station_no_in = ST.station_no_in
    WHERE S.staffing_calendar_da BETWEEN CAST(DATEADD(dd, -2, GETDATE()) As Date)
    AND CAST(GETDATE() AS DATE)
    And W.wstat_abrv_ch Not IN ('MWI') AND LEFT(LTRIM(RTRIM(W.wstat_abrv_ch)), 2) <> 'OT'
    ORDER BY RM.RscMaster_EmployeeID_Ch ASC"
    'Dim sbQ As New StringBuilder
    Dim dbc As New Tools.DB(GetCS(ConnectionStringType.Telestaff), toolsAppId, toolsDBError)
    'With sbQ
    '  .AppendLine("USE Telestaff;")
    '  .AppendLine("SELECT distinct RM.RscMaster_EmployeeID_Ch AS EmployeeID,")
    '  .AppendLine("(CASE WHEN RIGHT(LTRIM(RTRIM(ST.station_abrv_ch)), 2) IN ('15', '17', '18', '20', '22', '24') ")
    '  .AppendLine("THEN 'BC1 - Shift ' + SH.shift_abrv_ch ELSE ")
    '  .AppendLine("CASE WHEN RIGHT(LTRIM(RTRIM(ST.station_abrv_ch)), 2) IN ('11', '13', '14', '23', '25', '26') ")
    '  .AppendLine("THEN 'BC2 - Shift ' + SH.shift_abrv_ch ELSE ")
    '  .AppendLine("CASE WHEN U.unit_abrv_ch IN ('BC1', 'BC2') ")
    '  .AppendLine("THEN U.unit_abrv_ch + ' - Shift ' + SH.shift_abrv_ch ELSE ")
    '  .AppendLine("CASE WHEN U.unit_abrv_ch = 'CCU' THEN 'Dispatch' ELSE")
    '  .AppendLine("CASE WHEN U.unit_abrv_ch = 'LTH'THEN 'LOGISTICS' ELSE")
    '  .AppendLine("U.unit_name_ch END END END END END) AS OtherGroup")
    '  .AppendLine("FROM Staffing_tbl S INNER JOIN Shift_Tbl SH ON SH.Shift_No_In=S.Shift_No_In")
    '  .AppendLine("INNER JOIN Resource_Tbl R ON S.rsc_no_in = R.rsc_no_in ")
    '  .AppendLine("INNER JOIN resource_master_tbl RM ON R.RscMaster_No_In = RM.RscMaster_No_In ")
    '  .AppendLine("INNER JOIN Wstat_Cde_Tbl W ON W.Wstat_No_In=S.Wstat_No_In ")
    '  .AppendLine("INNER JOIN Position_Tbl P ON P.Pos_No_In=S.Pos_No_In ")
    '  .AppendLine("INNER JOIN Unit_Tbl U ON U.Unit_No_In=P.Unit_No_In ")
    '  .AppendLine("INNER JOIN Station_Tbl ST ON U.station_no_in = ST.station_no_in")
    '  .Append("WHERE S.staffing_calendar_da BETWEEN '").Append(Today.AddDays(-2).ToShortDateString)
    '  .Append("' AND '").Append(Today.ToShortDateString).Append("'  ")
    '  .AppendLine("AND W.wstat_abrv_ch NOT IN ('MWI') AND LEFT(LTRIM(RTRIM(W.wstat_abrv_ch)), 2) <> 'OT'")
    '  .AppendLine("ORDER BY RM.RscMaster_EmployeeID_Ch ASC")
    'End With
    Try
      Dim ds As DataSet = dbc.Get_Dataset(query)
      Dim tg As New Dictionary(Of Integer, String)
      For Each d In ds.Tables(0).Rows
        If Not tg.TryGetValue(d("EmployeeID"), d("OtherGroup").ToString.Trim) Then tg.Add(d("EmployeeID"), d("OtherGroup").ToString.Trim)

      Next
      Return tg
    Catch ex As Exception
      Log(ex)
      Return Nothing
    End Try
  End Function



  'Public Function GetPublicSafetyEmployeeData_EPP(StartDate As Date) As List(Of EPP)
  '    Dim Depts As New List(Of String)
  '    Depts.Add("2103")
  '    Depts.Add("2102")
  '    Depts.Add("1703")
  '    Dim fin As List(Of FinanceData) = GetEmployeeDataFromFinPlus(StartDate, Depts)
  '    Dim tl As List(Of TelestaffTimeData) = GetEmployeeDataFromTelestaff(StartDate)
  '    'Dim tlgroups = (From t In tl Group By t.EmployeeId, t.ProfileType, t.PayRate, t.FLSAHoursRequirement Into g = Group)
  '    Dim newepp As New List(Of EPP)
  '    For Each f In fin
  '        'Dim fd = (From f In fin Where f.EmployeeId = t.EmployeeId Select f).First
  '        Dim ttd = (From t In tl Where t.EmployeeId = f.EmployeeId Select t).ToList
  '        If ttd.Count > 0 Then
  '            Dim e As New EPP(ttd, f, StartDate) ', t.ProfileID, t.ProfileDesc)
  '            newepp.Add(e)
  '        End If
  '    Next
  '    newepp.RemoveAll(Function(x) x.Timelist.Count = 0)
  '    Return newepp.OrderBy(Function(x) x.EmployeeData.Department).ToList
  'End Function

  'Private Function GetEmployeePayPeriodByDataRow(dr As DataRow) As FinanceData
  '    Dim x As New FinanceData
  '    Try
  '        With x
  '            .EmployeeId = dr("empl_no")
  '            .EmployeeLastName = dr("l_name").ToString.Trim
  '            .EmployeeFirstName = dr("f_name").ToString.Trim
  '            .EmployeeName = .EmployeeFirstName & " " & .EmployeeLastName
  '            .EmployeeType = IsNull(dr("empl_type"), "")
  '            .HireDate = dr("hire_date")
  '            .JobTitle = dr("title").ToString.Trim
  '            .Department = dr("department").ToString.Trim
  '            .HoursNeededForOvertime = dr("pay_hours")
  '            .Base_Payrate = dr("rate")
  '            .Banked_Holiday_Hours = 0
  '            .Banked_Comp_Hours = 0
  '            .Classify = dr("classify").ToString.Trim
  '            .DepartmentName = dr("department_name").ToString.Trim
  '            .isFulltime = (dr("part_time").ToString.Trim = "F")
  '            If Not IsDBNull(dr("term_date")) Then
  '                .TerminationDate = dr("term_date")
  '            End If
  '            Select Case IsNull(dr("lv5_cd"), "").ToString.Trim
  '                Case "500" ' Comp time bank
  '                    .Banked_Comp_Hours = IsNull(dr("lv5_bal"), Double.MinValue)
  '                Case "510" ' Holiday Bank
  '                    .Banked_Holiday_Hours = IsNull(dr("lv5_bal"), Double.MinValue)
  '                Case Else
  '            End Select
  '            .Banked_Vacation_Hours = IsNull(dr("lv2_bal"), Double.MinValue)
  '            .Banked_Sick_Hours = IsNull(dr("lv1_bal"), Double.MinValue)
  '        End With
  '        Return x
  '    Catch ex As Exception
  '        Log(ex)
  '        Return Nothing
  '    End Try
  'End Function

  'Private Function GetEmployeePayPeriodByDataRow(dr As DataRow, StartDate As Date, TimeBank As DataSet) As FinanceData
  '    Dim x As New FinanceData
  '    Try
  '        With x
  '            ' Basically this version of the function is for older data when we know that the data that's in pentamation is out of date
  '            ' we try and load the data from the Timestore database. 
  '            .EmployeeId = dr("empl_no")
  '            Dim t As DataRow = Nothing
  '            If (From tb In TimeBank.Tables(0).AsEnumerable() Where tb("employee_number") = .EmployeeId Select tb).Count > 0 Then
  '                t = (From tb In TimeBank.Tables(0).AsEnumerable() Where tb("employee_number") = .EmployeeId Select tb).First
  '            Else
  '                t = Nothing
  '            End If
  '            .EmployeeLastName = dr("l_name").ToString.Trim
  '            .EmployeeFirstName = dr("f_name").ToString.Trim
  '            .EmployeeName = .EmployeeFirstName & " " & .EmployeeLastName
  '            .EmployeeType = IsNull(dr("empl_type"), "")
  '            .HireDate = dr("hire_date")
  '            .JobTitle = dr("title").ToString.Trim
  '            .Department = dr("department").ToString.Trim
  '            .HoursNeededForOvertime = dr("pay_hours")
  '            '.PayPeriodStart = StartDate
  '            .Base_Payrate = dr("rate")
  '            .Banked_Holiday_Hours = 0
  '            .Banked_Comp_Hours = 0
  '            .Classify = dr("classify").ToString.Trim
  '            .DepartmentName = dr("department_name").ToString.Trim
  '            .isFulltime = (dr("part_time").ToString.Trim = "F")
  '            If Not IsDBNull(dr("term_date")) Then
  '                .TerminationDate = dr("term_date")
  '            End If
  '            If Not t Is Nothing Then ' If we find it, use it.
  '                Select Case IsNull(t("lv5_code"), "").ToString.Trim
  '                    Case "500" ' Comp time bank
  '                        .Banked_Comp_Hours = IsNull(t("lv5_balance"), Double.MinValue)
  '                    Case "510" ' Holiday Bank
  '                        .Banked_Holiday_Hours = IsNull(t("lv5_balance"), Double.MinValue)
  '                    Case Else
  '                End Select
  '                .Banked_Vacation_Hours = IsNull(t("lv2_balance"), Double.MinValue)
  '                .Banked_Sick_Hours = IsNull(t("lv1_balance"), Double.MinValue)
  '            Else
  '                Select Case IsNull(dr("lv5_cd"), "").ToString.Trim
  '                    Case "500" ' Comp time bank
  '                        .Banked_Comp_Hours = IsNull(dr("lv5_bal"), Double.MinValue)
  '                    Case "510" ' Holiday Bank
  '                        .Banked_Holiday_Hours = IsNull(dr("lv5_bal"), Double.MinValue)
  '                    Case Else
  '                End Select
  '                .Banked_Vacation_Hours = IsNull(dr("lv2_bal"), Double.MinValue)
  '                .Banked_Sick_Hours = IsNull(dr("lv1_bal"), Double.MinValue)
  '            End If
  '        End With
  '        Return x
  '    Catch ex As Exception
  '        Log(ex)
  '        Return Nothing
  '    End Try
  'End Function

  Public Function Approve_Payperiod(ByRef Req As System.Web.HttpRequestBase, EmployeeId As Integer,
                                    PayPeriodEnding As Date, ApprovalType As String) As Boolean

    Dim Machinename As String = Req.UserHostName
    Dim Username As String = Req.LogonUserIdentity.Name
    Dim IPAddress As String = Req.UserHostAddress
    Dim MyEID As Integer = GetEmployeeIDFromAD(Username)
    Return Approve_Payperiod(Username, MyEID, Machinename, IPAddress, EmployeeId, PayPeriodEnding, ApprovalType)

  End Function

  Public Function Approve_Payperiod(ApprovedByUsername As String, ApprovedByEmployeeId As Integer,
                                    Machinename As String, IPAddress As String, EmployeeID As Integer,
                                    PayPeriodEnding As Date, ApprovalType As Integer) As Boolean
    Dim dbc As New Tools.DB(GetCS(ConnectionStringType.Timestore), toolsAppId, toolsDBError)
    Dim sbQ As New StringBuilder

    Dim P(5) As SqlParameter
    P(0) = New SqlParameter("@EmployeeId", Data.SqlDbType.Int) With {.Value = EmployeeID}
    P(1) = New SqlParameter("@PayPeriodEnding", Data.SqlDbType.Date) With {.Value = PayPeriodEnding}
    P(2) = New SqlParameter("@AppEmployeeId", Data.SqlDbType.Int) With {.Value = ApprovedByEmployeeId}
    P(3) = New SqlParameter("@AppUsername", Data.SqlDbType.VarChar, 100) With {.Value = ApprovedByUsername}
    P(4) = New SqlParameter("@Machinename", Data.SqlDbType.VarChar, 100) With {.Value = Machinename}
    P(5) = New SqlParameter("@IpAddress", Data.SqlDbType.VarChar, 20) With {.Value = IPAddress}

    With sbQ
      .AppendLine("USE TimeStore;")
      .AppendLine("UPDATE Saved_Time SET ")

      Select Case ApprovalType
        Case 1 ' initial
          .AppendLine("initial_approval_username=@AppUsername, initial_approval_employeeid=@AppEmployeeId, ")
          .AppendLine("initial_approval_machine_name=@Machinename, initial_approval_ip_address=@IpAddress, ")
          .AppendLine("initial_approval_date=GETDATE() ")

        Case 2 ' Final
          .AppendLine("final_approval_username=@AppUsername, final_approval_employeeid=@AppEmployeeId, ")
          .AppendLine("final_approval_machine_name=@Machinename, final_approval_ip_address=@IpAddress, ")
          .AppendLine("final_approval_date=GETDATE() ")
        Case 3 ' Both
          .AppendLine("initial_approval_username=@AppUsername, initial_approval_employeeid=@AppEmployeeId, ")
          .AppendLine("initial_approval_machine_name=@Machinename, initial_approval_ip_address=@IpAddress, ")
          .AppendLine("initial_approval_date=GETDATE(), ")
          .AppendLine("final_approval_username=@AppUsername, final_approval_employeeid=@AppEmployeeId, ")
          .AppendLine("final_approval_machine_name=@Machinename, final_approval_ip_address=@IpAddress, ")
          .AppendLine("final_approval_date=GETDATE() ")

        Case Else
          Return False

      End Select
      .AppendLine("WHERE employee_id=@EmployeeId AND pay_period_ending=@PayPeriodEnding ")
      If ApprovalType = 2 Then
        ' We want to add a clause that lets it only update if the initial is not null
        .AppendLine("AND initial_approval_employeeid IS NOT NULL")
      End If
      .Append(";")
    End With
    Try
      Dim i As Integer
      i = dbc.ExecuteNonQuery(sbQ.ToString, P)
      Return i > 0
    Catch ex As Exception
      Log(ex)
      Return False
    End Try
  End Function

  Public Function Check_if_Already_Approved(EmployeeId As Integer, PayPeriodEnding As Date, ApprovalType As Integer) As Boolean
    ' This function will check to see if a user / ppd combinations is already approved and return true if it's already 
    ' approved and false if it is not.
    Dim dbc As New Tools.DB(GetCS(ConnectionStringType.Timestore), toolsAppId, toolsDBError)
    Dim sbQ As New StringBuilder

    Dim P(1) As SqlParameter
    P(0) = New SqlParameter("@EmployeeId", Data.SqlDbType.Int) With {.Value = EmployeeId}
    P(1) = New SqlParameter("@PayPeriodEnding", Data.SqlDbType.Date) With {.Value = PayPeriodEnding}
    With sbQ
      .AppendLine("USE TimeStore;")
      .AppendLine("SELECT COUNT(*) AS CNT FROM Saved_Time WHERE ")
      Select Case ApprovalType
        Case 1
          .AppendLine("initial_approval_date IS NOT NULL ")
        Case 2
          .AppendLine("final_approval_date IS NOT NULL ")
        Case 3
          .AppendLine("initial_approval_date IS NOT NULL AND final_approval_date IS NOT NULL ")
      End Select
      .AppendLine("AND employee_id=@EmployeeId AND pay_period_ending=@PayPeriodEnding;")
    End With
    Try
      Dim i As Integer = 0
      i = dbc.ExecuteScalar(sbQ.ToString, P)
      Return i > 0
    Catch ex As Exception
      Log(ex)
      Return False
    End Try
  End Function

  Public Function Set_Approval_Level(dr As DataRow) As Integer
    ' 0 = not approved
    ' 1 = initial approval
    ' 2 = final approval
    If Not IsDBNull(dr("final_approval_date")) Then Return 2
    If Not IsDBNull(dr("initial_approval_date")) Then Return 1
    Return 0
  End Function

  Public Function GetTimeCards(PayPeriodStart As Date, Optional allowDataSave As Boolean = False) As List(Of GenericTimecard)
    Dim PublicSafety() As String = {"1703", "2103"} ' Removed 2102 ' these two variables are the constant department ids
    Dim PublicWorks() As String = {"3701", "3709", "3711", "3712"} ' for these two departments.

    Dim PayPeriodEnding As Date = PayPeriodStart.AddDays(13)
    Dim gtc As New List(Of GenericTimecard)
    Dim std As List(Of Saved_Timecard_Data) = Get_All_Saved_Timecard_Data(PayPeriodEnding)
    Dim teledata As List(Of TelestaffTimeData) = GetEmployeeDataFromTelestaff(PayPeriodStart)
    Dim tcdata As List(Of TimecardTimeData) = GetEmployeeDataFromTimecard(PayPeriodStart)
    Dim tsdata As List(Of TimecardTimeData) = GetEmployeeDataFromTimeStore(PayPeriodStart)
    Dim tsctedata As List(Of Saved_TimeStore_Comp_Time_Earned) = GetCompTimeEarnedDataFromTimeStore(PayPeriodStart)
    Dim notes As List(Of Note) = Get_All_Notes(PayPeriodEnding)

    ' Let's pare down the employee list to just those that are not terminated, 
    ' or those that were terminated in the pay period.
    Dim employeeList As List(Of FinanceData) = (From el In GetEmployeeDataFromFinPlus()
                                                Order By el.Department, el.EmployeeLastName
                                                Where Not el.IsTerminated And
                                                Not PublicWorks.Contains(el.Department)
                                                Select el).ToList

    '(el.TerminationDate = Date.MaxValue Or
    '(el.TerminationDate > PayPeriodStart And
    'el.TerminationDate <= PayPeriodEnding)) And

    ' Now let's build some timecards
    For Each e In employeeList
      If PublicSafety.Contains(e.Department) Then ' Telestaff
        Try


          Dim tmpStd As List(Of Saved_Timecard_Data) = (From s In std Where s.EmployeeId = e.EmployeeId Select s).ToList
          Dim tmpTele As List(Of TelestaffTimeData) = (From t In teledata Where t.EmployeeId = e.EmployeeId Select t).ToList
          Dim tmpNotes As List(Of Note) = (From n In notes Where n.EmployeeID = e.EmployeeId Select n).ToList
          If tmpStd.Count > 0 Then
            gtc.Add(New GenericTimecard(New EPP(tmpTele, e, PayPeriodStart), tmpStd, tmpNotes, allowDataSave))
          Else
            gtc.Add(New GenericTimecard(New EPP(tmpTele, e, PayPeriodStart), allowDataSave))
          End If
        Catch ex As Exception
          Log(ex)
        End Try

      Else ' Timecard / Timestore
        Try
          Dim tmpStd As List(Of Saved_Timecard_Data) = (From s In std Where s.EmployeeId = e.EmployeeId Select s).ToList
          Dim tmpTC As List(Of TimecardTimeData) = (From t In tcdata Where t.EmployeeID = e.EmployeeId Select t).ToList
          Dim tmpCTE As Saved_TimeStore_Comp_Time_Earned = Nothing
          If tmpTC.Count = 0 Then
            tmpTC = (From t In tsdata Where t.EmployeeID = e.EmployeeId Select t).ToList
            Dim cte = (From c In tsctedata Where c.employee_id = e.EmployeeId Select c)
            If cte.Count > 0 Then tmpCTE = cte.First
          End If
          Dim tmpNotes As List(Of Note) = (From n In notes Where n.EmployeeID = e.EmployeeId Select n).ToList
          If tmpStd.Count > 0 Then
            gtc.Add(New GenericTimecard(New TC_EPP(tmpTC, e, PayPeriodStart, tmpCTE), tmpStd, tmpNotes, allowDataSave))
          Else
            gtc.Add(New GenericTimecard(New TC_EPP(tmpTC, e, PayPeriodStart), allowDataSave))
          End If
        Catch ex As Exception
          Log(ex)
        End Try
      End If

    Next

    Return gtc
  End Function

  Public Function GetCrosstabData(payPeriodStart As Date) As List(Of Crosstab)
    Dim ppEnd As Date = payPeriodStart.AddDays(13)
    Dim PublicWorks() As String = {"3701", "3709", "3711", "3712"} ' for these two departments.
    Dim employeeList As List(Of FinanceData) = (From el In GetEmployeeDataFromFinPlus()
                                                Order By el.Department, el.EmployeeLastName
                                                Where (el.TerminationDate = Date.MaxValue Or
                                                (el.TerminationDate > payPeriodStart And
                                                el.TerminationDate <= ppEnd)) And
                                                Not PublicWorks.Contains(el.Department) Select el).ToList

    Dim dbc As New Tools.DB(GetCS(ConnectionStringType.Timestore), toolsAppId, toolsDBError)
    Dim sbQ As New StringBuilder
    With sbQ
      .AppendLine("USE TimeStore; SELECT ISNULL(T1.orgn, 'Total') AS orgn, ISNULL(T1.employee_id, 0) AS employee_id,  ")
      .AppendLine("SUM(reg) AS sumReg, SUM([231]) AS [231], SUM([090]) AS [090], SUM([095]) AS [095], SUM([110]) AS [110],SUM([046]) AS [046], ")
      .AppendLine("SUM([100]) AS [100], SUM([120]) AS [120],SUM([121]) AS [121],SUM([230]) AS [230], SUM([232]) AS [232], ")
      .AppendLine("SUM([111]) AS [111],  SUM([123]) AS [123], SUM([130]) AS [130], SUM([134]) AS [134], ")
      .AppendLine("SUM([131]) AS [131],  SUM([101]) AS [101],  ")
      .AppendLine("SUM([124]) AS [124],SUM([006]) AS [006],SUM([007]) AS [007],SUM([122]) AS [122], SUM([777]) AS [777], SUM(TotalHours) AS TotalHours ")
      .AppendLine("FROM (SELECT orgn, employee_id, pay_period_ending, [002] as reg, [046], [090], [095], [100], [101],  ")
      .AppendLine("[110],[111], [121], [123], [230], [231], [232], [130], [131], [134], [120], [122], [124], [006], [007], [777], TotalHours ")
      .AppendLine("FROM (SELECT S.employee_id, S.paycode, S.hours, S.orgn, S.pay_period_ending, S2.TotalHours FROM Saved_Time S ")
      .AppendLine("INNER JOIN (SELECT employee_id, SUM(hours) AS TotalHours FROM Saved_Time ")
      .Append("	WHERE pay_period_ending='").Append(ppEnd.ToShortDateString)
      .Append("' GROUP BY employee_id) AS S2 ON S2.employee_id = S.employee_id ")
      .Append("WHERE S.pay_period_ending='").Append(ppEnd.ToShortDateString)
      .AppendLine("') AS ST ")
      .AppendLine("PIVOT (	SUM(hours) ")
      .AppendLine("	FOR paycode IN ([002], [046], [090], [095], [100], [101], [110],[111], [121], [123], [230], [231], [232],  ")
      .AppendLine("					[130], [131], [134], [120], [122], [124], [006], [007], [777]) ")
      .AppendLine("	) AS PivotTable) AS T1 ")
      .AppendLine("GROUP BY ROLLUP (T1.orgn, T1.employee_id);")
      '.AppendLine("ORDER BY CASE WHEN t1.orgn IS NULL THEN 1 ELSE 0 END, t1.orgn ASC, ")
      '.AppendLine("CASE WHEN t1.employee_id IS NULL THEN 1 ELSE 0 END, t1.employee_id ASC; ")
    End With
    Dim ds As DataSet = dbc.Get_Dataset(sbQ.ToString)
    Try
      'Dim ct As List(Of Crosstab) = (From d In ds.Tables(0).AsEnumerable Select New Crosstab With {
      '                    .EmployeeID = IsNull(d("employee_id"), ""), .orgn = IsNull(d("orgn"), ""),
      '                    .FirstName = GetFirstName(f, .EmployeeID.ToString),
      '                    .LastName = GetLastName(f, .EmployeeID.ToString),
      '                    .Regular = IsNull(d("sumReg"), 0), .Total = IsNull(d("TotalHours"), 0),
      '                    .pc007 = IsNull(d("007"), 0), .pc046 = IsNull(d("046"), 0),
      '                    .pc090 = IsNull(d("090"), 0), .pc100 = IsNull(d("100"), 0),
      '                    .pc101 = IsNull(d("101"), 0), .pc110 = IsNull(d("110"), 0),
      '                    .pc111 = IsNull(d("111"), 0), .pc120 = IsNull(d("120"), 0),
      '                    .pc121 = IsNull(d("121"), 0), .pc122 = IsNull(d("122"), 0),
      '                    .pc123 = IsNull(d("123"), 0), .pc124 = IsNull(d("124"), 0),
      '                    .pc130 = IsNull(d("130"), 0), .pc131 = IsNull(d("131"), 0),
      '                    .pc134 = IsNull(d("134"), 0), .pc230 = IsNull(d("230"), 0),
      '                    .pc231 = IsNull(d("231"), 0), .pc232 = IsNull(d("232"), 0)}).ToList
      Dim ct As New List(Of Crosstab)
      Dim datalist = (From data In ds.Tables(0).AsEnumerable Where data("orgn") = "Total" Or data("employee_id") = 0 Select data).ToList
      For Each d In datalist
        ct.Add(New Crosstab(d, payPeriodStart))
      Next
      For Each e In employeeList
        Dim d = (From dr In ds.Tables(0).AsEnumerable Where dr("employee_id") = e.EmployeeId.ToString Select dr)
        If d.Count > 0 Then
          ct.Add(New Crosstab(d.First, e, payPeriodStart))
        Else
          ct.Add(New Crosstab(e, payPeriodStart))
        End If
      Next
      Return ct
    Catch ex As Exception
      Log(ex)
      Return New List(Of Crosstab)
    End Try
  End Function

  Public Function InsertSavedTimeToFinplus(tsds As DataSet, UseProduction As Boolean) As Boolean
    Dim sbQ As New StringBuilder
    'Dim dbf As New Tools.DB(GetCS(ConnectionStringType.FinPlus), toolsAppId, toolsDBError)
    Dim dbf As New Tools.DB(GetCS(ConnectionStringType.FinPlus), toolsAppId, toolsDBError)
    ' We're going to grab all of the 002 data from Finplus's timecard table 
    ' and all of Timestore's SavedTime table
    ' loop through the finplus data to find matches. 
    ' If the paycode and pay rate match, we update the hours
    ' then we loop through the saved_time data and insert everything else.
    ' If the paycode and payrate exists in the Finplus Timecard data, we update.  Otherwise we insert.
    Dim fds As DataSet = GetRawFinplusTimecardData(UseProduction)
    If fds.Tables(0).Rows.Count > 0 Then
      Dim payrun As String = (From f In fds.Tables(0).AsEnumerable Where Not IsDBNull(f("pay_run")) Select f("pay_run")).FirstOrDefault
      With sbQ
        '.AppendLine("USE TimeStore;")
        If UseProduction Then
          .AppendLine("USE finplus50;")
        Else
          .AppendLine("USE trnfinplus50;")
        End If
        For Each ts In tsds.Tables(0).Rows
          Dim eid As Integer = ts("employee_id")
          Dim paycode As String = ts("paycode")
          Dim payrate As Decimal = ts("payrate")
          Dim rndPayrate As Decimal = Math.Round(payrate, 5)
          Dim hours As Double = ts("hours")
          Dim tmp = (From f In fds.Tables(0).AsEnumerable Where f("empl_no") = eid And
                     f("pay_code") = paycode And Math.Round(f("payrate"), 5) = rndPayrate Select f)
          If tmp.Count = 0 Then
            ' We need to insert this row.
            .Append("INSERT INTO timecard (empl_no,pay_code,hours,")
            .Append("payrate,amount,pay_run,") ' Removed orgn from insert statement.
            .Append("classify,pay_cycle,reported,")
            .Append("user_chg,date_chg,flsa_cycle,")
            .Append("flsa_flg,flsa_carry_ovr) VALUES ('")
            .Append(eid).Append("', '").Append(paycode).Append("', ").Append(hours).Append(", ")
            .Append(payrate).Append(", ").Append(GetAmount(paycode, payrate, hours)).Append(", '")
            .Append(payrun).Append("', '")
            '.Append(ts("orgn").ToString.Trim).Append("', '") ' Removed orgn from insert statement.
            .Append(ts("classify")).Append("', '1', 'N', ")
            .Append("'TimeStore', GETDATE(), 0, ")
            .Append("'N', 'N');").Append(vbCrLf)

          ElseIf tmp.Count = 1 Then
            ' we need to compare the hours and update it if it doesn't match what we've got.
            Dim st As DataRow = tmp.First
            Dim newHours As Double = st("hours")
            If newHours <> hours Then
              Dim newAmount As Decimal = GetAmount(paycode, payrate, hours)
              .Append("UPDATE timecard SET hours=").Append(hours)
              .Append(", amount=").Append(newAmount)
              .Append(", user_chg='TimeStore', date_chg=GETDATE()")
              .Append(" WHERE empl_no='").Append(eid)
              .Append("' AND pay_code='").Append(paycode)
              .Append("' AND payrate=").Append(payrate).AppendLine(";")
            End If
          Else
            ' we've got an error
            Log("Too many rows in upload process", eid.ToString, payrate.ToString, paycode)
          End If
        Next
      End With
      Try

        Dim i As Integer = dbf.ExecuteNonQuery(sbQ.ToString)
        If i = -1 Then
          Return False
        Else
          Return True
        End If
      Catch ex As Exception
        Log(ex)
        Return False
      End Try
    Else
      Log("No rows found in timecard table.", "", "", "")
      Return False
    End If
  End Function

  Private Function GetAmount(paycode As String, payrate As Double, hours As Double) As Decimal
    Dim newRate As Double = payrate
    'Select Case paycode
    '    'Case "090", "120"
    '    '    newRate = 0
    '    'Case "046"
    '    '    newRate = 3
    '    'Case "231", "131"
    '    '    newRate = payrate * 1.5
    '    'Case "232"
    '    '    newRate = payrate * 2
    '    Case Else
    '        newRate = payrate
    'End Select
    Return Math.Round(newRate * hours, 2)
  End Function

  Private Function GetPayrate(paycode As String, payrate As Double) As Decimal
    Dim newRate As Double = payrate
    Select Case paycode.Trim
      Case "090", "095", "120", "122" ' 6/23/2015 added 122 for Holiday Time Banked
        newRate = 0
      Case "046"
        newRate = 3
      Case "777" ' Disaster paycode, these will be inserted as 002 by replacement.
        newRate = payrate * 1.5
      Case "231", "131"
        newRate = payrate * 1.5
      Case "232"
        newRate = payrate * 2
      Case Else
        newRate = payrate
    End Select
    Return Math.Round(newRate, 5)
  End Function

  Public Function UpdateFinplusWithSavedTime(tsds As DataSet, UseProduction As Boolean) As Boolean
    Dim fds As DataSet = GetRawFinplusTimecardData(UseProduction)
    Dim dbf As New Tools.DB(GetCS(ConnectionStringType.FinPlus), toolsAppId, toolsDBError)
    Dim sbQ As New StringBuilder
    Dim nomatch As Integer = 0, alreadymatch As Integer = 0
    With sbQ
      '.AppendLine("USE TimeStore;")
      If UseProduction Then
        .AppendLine("USE finplus50;")
      Else
        .AppendLine("USE trnfinplus50;")
      End If
      For Each f In fds.Tables(0).Rows
        Dim eid As Integer = f("empl_no")
        Dim paycode As String = f("pay_code")
        Dim payrate As Decimal = f("payrate")
        Dim hours As Double = f("hours")
        Dim rndPayrate As Decimal = Math.Round(payrate, 5)
        Dim tmp = (From t In tsds.Tables(0).AsEnumerable
                   Where t("employee_id") = eid And
                     t("paycode") = paycode And
                     t("payrate") = rndPayrate Select t)
        If tmp.Count = 0 Then
          ' We don't have any rows with this payrate/paycode for this employee so we need to 0 out this row, if this user exists in our data.
          If (From t In tsds.Tables(0).AsEnumerable Where t("employee_id") = eid Select t).Count > 0 Then
            '.Append("UPDATE timecard SET hours=0, amount=0 WHERE empl_no='")
            '.Append(eid).Append("' AND pay_code='").Append(paycode)
            '.Append("' AND payrate=").Append(payrate).AppendLine(";")
            .Append("DELETE FROM timecard WHERE empl_no='")
            .Append(eid).Append("' AND pay_code='").Append(paycode)
            .Append("' AND payrate=").Append(payrate).AppendLine(";")
          Else
            nomatch += 1
          End If

        ElseIf tmp.Count = 1 Then
          ' we need to compare the hours and update it if it doesn't match what we've got.
          Dim st As DataRow = tmp.First
          Dim newHours As Double = st("hours")
          If newHours <> hours Then
            Dim newAmount As Decimal = GetAmount(paycode, payrate, newHours)
            .Append("UPDATE timecard SET hours=").Append(newHours)
            .Append(", amount=").Append(newAmount)
            .Append(", user_chg='TimeStore', date_chg=GETDATE()")
            .Append(" WHERE empl_no='").Append(eid)
            .Append("' AND pay_code='").Append(paycode)
            .Append("' AND payrate=").Append(payrate).AppendLine(";")
          Else
            alreadymatch += 1
          End If

        Else
          ' we've got an error
          Log("Too many rows in upload process", eid.ToString, payrate.ToString, paycode)
        End If
      Next
    End With
    Try
      Dim i As Integer = dbf.ExecuteNonQuery(sbQ.ToString)
      If i = -1 Then
        Return False
      Else
        Return True
      End If
    Catch ex As Exception
      Log(ex)
      Return False
    End Try
  End Function

  Public Function GetRawFinplusTimecardData(UseProduction As Boolean) As DataSet
    Dim dbf As New Tools.DB(GetCS(ConnectionStringType.FinPlus), toolsAppId, toolsDBError)
    Dim financeQuery As New StringBuilder
    With financeQuery
      If UseProduction Then
        .AppendLine("USE finplus50;")
      Else
        .AppendLine("USE trnfinplus50;")
      End If
      .AppendLine("SELECT TC.empl_no,pay_code,hours,payrate,amount,orgn,account,proj,pacct,classify,pay_cycle,tax_ind, ")
      .AppendLine("	pay_run,subtrack_id,reported,user_chg,date_chg,flsa_cycle,flsa_flg,flsa_carry_ovr,ret_pers_code ")
      .AppendLine("FROM timecard TC ")
      .AppendLine("INNER JOIN employee E ON TC.empl_no=E.empl_no ")
      .AppendLine("WHERE TC.pay_code IN ('002', '090', '007', '100', '101', '110', '111', '120', '121', '122',  ")
      .AppendLine("					'123', '124', '130', '131', '134', '230', '231', '232') ")
      .AppendLine("AND LTRIM(RTRIM(E.home_orgn)) NOT IN ('3701', '3709', '3711', '3712') ")
      .AppendLine("ORDER BY orgn ASC, empl_no ASC ")

    End With
    Dim ds As DataSet
    Try
      ds = dbf.Get_Dataset(financeQuery.ToString)
      Return ds
    Catch ex As Exception
      Log(ex)
      Return Nothing
    End Try
  End Function

  Public Function GetRawTimestoreSavedTimeData(payPeriodEnding As Date) As DataSet
    Dim dbts As New Tools.DB(GetCS(ConnectionStringType.Timestore), toolsAppId, toolsDBError)
    Dim f As List(Of FinanceData) = GetAllEmployeeDataFromFinPlus()
    Dim TimestoreQuery As New StringBuilder
    With TimestoreQuery
      .AppendLine("USE TimeStore;")
      ' -----------------
      ' original version
      ' -----------------

      '.AppendLine("SELECT employee_id,paycode,payrate,hours,amount,orgn,classify ")
      ' -----------------
      ' disaster calculations version
      ' -----------------
      .AppendLine("SELECT employee_id,")
      .AppendLine("CASE WHEN paycode = '777' THEN '002' ELSE paycode END AS paycode,")
      .AppendLine("CASE WHEN paycode = '777' THEN CAST(payrate * 1.5 AS DECIMAL(10, 5)) ELSE payrate END AS payrate, ")
      .AppendLine("hours,amount,orgn,classify ")
      .AppendLine("FROM Saved_Time")
      .AppendLine("WHERE pay_period_ending = @PayPeriodEnding ")

      ' paycode 800 is a paycode we use only in Timestore to determine if
      ' a firefighter / dispatch employee marked any holiday hours as 
      ' ineligible.  This paycode is invalid to finplus so we're not
      ' going to send it to them.
      ' "777"
      .AppendLine("AND paycode <> 800 ")
      .AppendLine("ORDER BY orgn ASC, employee_id ASC")
    End With
    Dim P(0) As SqlParameter
    P(0) = New SqlParameter("@PayPeriodEnding", Data.SqlDbType.Date) With {.Value = payPeriodEnding}
    Dim ds As DataSet
    Try
      ds = dbts.Get_Dataset(TimestoreQuery.ToString, P)
      For Each d In ds.Tables(0).Rows
        d("payrate") = GetPayrate(d("paycode"), d("payrate"))
        d("classify") = GetClassification(d("employee_id"), f, d("classify"))
      Next
      Return ds
    Catch ex As Exception
      Log(ex)
      Return Nothing
    End Try
  End Function

  Public Function GetClassification(EmployeeID As Integer, ByRef fdl As List(Of FinanceData), oldClassification As String) As String
    Dim e = (From f In fdl Where f.EmployeeId = EmployeeID Select f)
    If e.Count = 0 Then
      Return oldClassification
    Else
      Return e.First.Classify
    End If
  End Function

  Public Function SavedTimeToFinplusProcess(payPeriodEnding As Date, UseProduction As Boolean) As Boolean
    Dim tc As List(Of GenericTimecard) = GetTimeCards(payPeriodEnding.AddDays(-13), True)
    Dim tsds As DataSet = GetRawTimestoreSavedTimeData(payPeriodEnding)
    'If Not UpdateFinplusWithSavedTime(tsds, UseProduction) Then Return False
    UpdateFinplusWithSavedTime(tsds, UseProduction)
    Return InsertSavedTimeToFinplus(tsds, UseProduction)

  End Function

  Public Function Delete_Hours(EmployeeId As Integer,
                               PayPeriodEnding As Date,
                               PayCode As String,
                               PayRate As Double) As Boolean

    Dim dp = New DynamicParameters()
    dp.Add("@EmployeeId", EmployeeId)
    dp.Add("@PayPeriodEnding", PayPeriodEnding)
    dp.Add("@PayCode", PayCode)
    dp.Add("@PayRate", Math.Round(PayRate))

    Dim sql As String = "
      SET TRANSACTION ISOLATION LEVEL SERIALIZABLE;
      BEGIN TRANSACTION DeleteData;
        USE TimeStore;
        DELETE FROM Saved_Time 
        WHERE 
          employee_id=@EmployeeId AND
          pay_period_ending = @PayPeriodEnding AND 
          paycode = @PayCode AND 
          Payrate = @PayRate;
      COMMIT TRANSACTION DeleteData;"
    Return Exec_Query(sql, dp, ConnectionStringType.Timestore) > 0
  End Function

  Public Function Save_Hours(EmployeeID As Integer,
                             PayPeriodEnding As Date,
                             PayCode As String,
                             Hours As Double,
                             Payrate As Double,
                             Department As String,
                             Classify As String) As Boolean

    Dim deleted As Boolean = Delete_Hours(EmployeeID, PayPeriodEnding, PayCode, Payrate)
    ' Here we're going to insert their time data into the database.
    Dim dp = New DynamicParameters()
    dp.Add("@EmployeeId", EmployeeID)
    dp.Add("@PayPeriodEnding", PayPeriodEnding)
    dp.Add("@PayCode", PayCode)
    dp.Add("@Hours", Hours)
    dp.Add("@Amount", 0)
    dp.Add("@Orgn", Department)
    dp.Add("@Classify", Classify)
    dp.Add("@PayRate", Math.Round(Payrate, 5))

    Dim sql As String = "
      SET TRANSACTION ISOLATION LEVEL SERIALIZABLE;
      BEGIN TRANSACTION UpdateData;
        USE TimeStore;
        INSERT INTO Saved_Time (employee_id, pay_period_ending, PayCode, Hours,
        Payrate, amount, orgn, Classify) VALUES 
        (@EmployeeId, @PayPeriodEnding, @PayCode, @Hours, @PayRate, @Amount, @Orgn, @Classify);
      COMMIT TRANSACTION UpdateData;"
    Return Exec_Query(sql, dp, ConnectionStringType.Timestore) > 0
    ' old version of this function is below.

    'Dim dbc As New Tools.DB(GetCS(ConnectionStringType.Timestore), toolsAppId, toolsDBError)
    'Dim sbQ As New StringBuilder
    'With sbQ
    '  .AppendLine("BEGIN TRANSACTION DeleteData;")
    '  .AppendLine("USE TimeStore;")
    '  .Append("DELETE FROM Saved_Time WHERE employee_id=@EmployeeId And ")
    '  .Append("pay_period_ending = @PayPeriodEnding And ")
    '  .Append("paycode = @PayCode And ")
    '  .AppendLine("payrate = @PayRate;")
    '  .AppendLine("COMMIT TRANSACTION DeleteData;")
    '  .AppendLine("BEGIN TRANSACTION UpdateData;")
    '  .AppendLine("USE TimeStore;")
    '  .AppendLine("INSERT INTO Saved_Time (employee_id, pay_period_ending, paycode, hours, ")
    '  .AppendLine("payrate, amount, orgn, classify) VALUES ")
    '  .AppendLine("(@EmployeeId, @PayPeriodEnding, @PayCode, @Hours, @PayRate, @Amount, @Orgn, @Classify);")
    '  .AppendLine("COMMIT TRANSACTION UpdateData;")
    'End With

    'Dim P(7) As SqlParameter
    'P(0) = New SqlParameter("@EmployeeId", Data.SqlDbType.Int) With {.Value = EmployeeID}
    'P(1) = New SqlParameter("@PayPeriodEnding", Data.SqlDbType.Date) With {.Value = PayPeriodEnding}
    'P(2) = New SqlParameter("@PayCode", Data.SqlDbType.Char, 3) With {.Value = PayCode}
    'P(3) = New SqlParameter("@Hours", Data.SqlDbType.Decimal) With {.Value = Hours}
    'P(4) = New SqlParameter("@Amount", Data.SqlDbType.Decimal) With {.Value = 0}
    'P(5) = New SqlParameter("@Orgn", Data.SqlDbType.Char, 16) With {.Value = Department}
    'P(6) = New SqlParameter("@Classify", Data.SqlDbType.Char, 4) With {.Value = Classify}
    'P(7) = New SqlParameter("@PayRate", Data.SqlDbType.Decimal) With {.Value = Math.Round(Payrate, 5)}

    'Dim i As Integer = 0
    '' we update
    'Try
    '  i = dbc.ExecuteNonQuery(sql, P)
    '  Return i > 0
    'Catch ex As Exception
    '  Log(ex)
    '  Return False
    'End Try
  End Function

  Public Function Clear_HolidayHours(EmployeeID As Integer, PayPeriodEnding As Date) As Boolean
    Dim dp = New DynamicParameters()
    dp.Add("@EmployeeId", EmployeeID)
    dp.Add("@PayPeriodEnding", PayPeriodEnding)

    Dim sql As String = "
        USE TimeStore;
        DELETE 
        FROM Saved_Time 
        WHERE 
          employee_id = @EmployeeId AND
          pay_period_ending = @PayPeriodEnding"
    Return Exec_Query(sql, dp, ConnectionStringType.Timestore) > 0

    'Dim dbc As New Tools.DB(GetCS(ConnectionStringType.Timestore), toolsAppId, toolsDBError)
    'Dim sbQ As New StringBuilder

    'With sbQ
    '  .AppendLine("USE TimeStore;")
    '  .AppendLine("DELETE FROM Saved_Time WHERE employee_id = @EmployeeId And ")
    '  .AppendLine("pay_period_ending = @PayPeriodEnding")
    '  ' Removed this bit so that after the holidays are saved, no matter what, they will 
    '  ' have to reapprove their time.
    '  ' AND paycode IN ('124', '134', '122');")
    'End With

    'Dim P(1) As SqlParameter
    'P(0) = New SqlParameter("@EmployeeId", Data.SqlDbType.Int) With {.Value = EmployeeID}
    'P(1) = New SqlParameter("@PayPeriodEnding", Data.SqlDbType.Date) With {.Value = PayPeriodEnding}
    'Dim i As Integer = 0
    'Try
    '  i = dbc.ExecuteNonQuery(sbQ.ToString, P)
    '  Return i > 0
    'Catch ex As Exception
    '  Log(ex)
    '  Return False
    'End Try
  End Function

  Public Function Get_Telestaff_Profile_By_Date(EmployeeID As Integer,
                                                DateToCheck As Date) As DataRow

    Dim dbc As New Tools.DB(GetCS(ConnectionStringType.Telestaff), toolsAppId, toolsDBError)
    Dim sbQ As New StringBuilder

    With sbQ
      .AppendLine("USE Telestaff;")
      .AppendLine("SELECT R.rsc_hourwage_db, R.PayInfo_No_In FROM Resource_Tbl R")
      .AppendLine("INNER JOIN Resource_Master_Tbl RMT ON R.RscMaster_No_In = RMT.RscMaster_No_In")
      .AppendLine("WHERE RMT.RscMaster_EmployeeID_Ch=@EmployeeId")
      .AppendLine("AND R.Rsc_From_Da <= @DateToCheck AND ISNULL(R.Rsc_Thru_Da, @DateToCheck) >= @DateToCheck")
      ' Removed this bit so that after the holidays are saved, no matter what, they will 
      ' have to reapprove their time.
      ' AND paycode IN ('124', '134', '122');")
    End With

    Dim P(1) As SqlParameter
    P(0) = New SqlParameter("@EmployeeId", Data.SqlDbType.VarChar, 30) With {.Value = EmployeeID.ToString}
    P(1) = New SqlParameter("@DateToCheck", Data.SqlDbType.Date) With {.Value = DateToCheck}
    Dim ds As DataSet
    Try
      ds = dbc.Get_Dataset(sbQ.ToString, P)
      If ds.Tables(0).Rows.Count > 1 Then
        Log("Too many Telestaff profiles found on date for Employee " & EmployeeID.ToString, EmployeeID.ToString, DateToCheck.ToShortDateString, ds.Tables(0).Rows.Count.ToString)
        Return Nothing
      ElseIf ds.Tables(0).Rows.Count = 1 Then
        Return ds.Tables(0).Rows(0)
      Else
        'Log("No Telestaff profiles found on date for Employee " & EmployeeID.ToString, EmployeeID.ToString, DateToCheck.ToShortDateString, ds.Tables(0).Rows.Count.ToString)
        Return Nothing
      End If

    Catch ex As Exception
      Log(ex)
      Return Nothing
    End Try
  End Function



  Public Function Update_Holiday_Data(HolidayChoice() As String, HolidayBankHoursPaid As Double,
                                      ByRef tc As GenericTimecard, UpdateBy As String) As Boolean
    ' This function will either delete or add holiday hours to this user's saved_time

    Clear_HolidayHours(tc.employeeID, tc.payPeriodStart.AddDays(13))

    Dim tpi As New Telestaff_Profile_Info(tc.employeeID, tc.payPeriodStart.AddDays(13))
    Dim holidayPayrate As Double = tc.Payrate
    If Not tpi.ProfileError Then holidayPayrate = tpi.FieldPayrate

    Dim BankedHolidayHours As Double = (From h In HolidayChoice
                                        Where h.ToUpper = "BANK"
                                        Select h).Count * tc.holidayIncrement

    Dim PaidHolidayHours As Double = (From h In HolidayChoice
                                      Where h.ToUpper = "PAID"
                                      Select h).Count * tc.holidayIncrement

    Dim IneligibleHours As Double = (From h In HolidayChoice
                                     Where h.ToUpper = "INELIGIBLE"
                                     Select h).Count * tc.holidayIncrement

    If tc.HolidaysInPPD.Length > 0 Then

      If BankedHolidayHours > 0 Then
        If Not Save_Hours(tc.employeeID, tc.payPeriodStart.AddDays(13), "122", BankedHolidayHours, holidayPayrate, tc.departmentNumber, tc.classify) Then
          Return False
        Else
          Add_Timestore_Note(tc.employeeID, tc.payPeriodStart.AddDays(13), "Elected to add " & BankedHolidayHours.ToString & " to the Holiday Hour Bank for the holiday in this pay period.", UpdateBy)
        End If
      End If

      If PaidHolidayHours > 0 Then
        If Not Save_Hours(tc.employeeID, tc.payPeriodStart.AddDays(13), "134", PaidHolidayHours, holidayPayrate, tc.departmentNumber, tc.classify) Then
          Return False
        Else
          Add_Timestore_Note(tc.employeeID, tc.payPeriodStart.AddDays(13), "Elected to be paid for " & PaidHolidayHours.ToString & " hours at pay rate " & holidayPayrate.ToString & " for the holiday in this pay period.", UpdateBy)
        End If
      End If

      ' I designed the Ineligible systems the way listed below originally because
      ' we didn't have a paycode we could use for it.  But due to the complexity of it,
      ' we switched to a much simpler version just using a specific paycode.
      ' This paycode (800) does not mean anything to finplus / Finance, so we won't 
      ' ever send it to them, we'll just use it to denote those hours in Timestore.
      ' Comparing this code to the code below shows how ridiculous the original
      ' idea was.
      If IneligibleHours > 0 Then
        If Not Save_Hours(tc.employeeID, tc.payPeriodStart.AddDays(13), "800", IneligibleHours, holidayPayrate, tc.departmentNumber, tc.classify) Then
          Return False
        Else
          Add_Timestore_Note(tc.employeeID, tc.payPeriodStart.AddDays(13), "Marked Ineligible for " & IneligibleHours.ToString & " hours at for the holiday in this pay period. These hours will not be paid or banked.", UpdateBy)
        End If
      End If

      'If IneligibleHours > 0 Then
      '  ' Ineligible hours are the hours that we use when someone doesn't work the day 
      '  ' prior to or after a holiday.  What we're going to do to handle these hours is
      '  ' to save Banked or Paid hours (paycodes 122 / 134, see above) at 0 hours.
      '  ' The exceptions to this are the very rare occasions where there are two holidays
      '  ' and even rarer, that someone is only eligible for one of those holidays.
      '  ' As far as I know, this is only Thanksgiving and Christmas.
      '  ' So what we're going to do here is check.  If they've set any hours to bank
      '  ' then we'll store our "0 hours" as time paid.
      '  ' But if they chose Paid Hours, we'll choose Banked Hours as our 0 hours.
      '  ' For 99.99% of all holidays, it'll just be the Else condition where we
      '  ' save 0 to both.
      '  ' But basically, the 0 hours saved is a boolean that we can check to tell if
      '  ' we marked those hours as ineligible.
      '  ' Hopefully the order doesn't matter.  If it does, oh man are we in for trouble.
      '  If BankedHolidayHours > 0 Then
      '    ' They banked the other holiday so we're going to mark this holiday as Paid.
      '    ' Paid is paycode 134, so we'll use that paycode.
      '    If Not Save_Hours(tc.employeeID,
      '               tc.payPeriodStart.AddDays(13),
      '               "134",
      '               0,
      '               holidayPayrate,
      '               tc.departmentNumber,
      '               tc.classify) Then
      '      Return False
      '    Else
      '      Add_Timestore_Note(tc.employeeID, tc.payPeriodStart.AddDays(13), "Marked Ineligible for " & IneligibleHours.ToString & " hours at for the holiday in this pay period. These hours will not be paid or banked.", UpdateBy)
      '    End If

      '  ElseIf PaidHolidayHours > 0 Then
      '    ' They paid the other holiday so we're going to mark this holiday as Banked
      '    ' Banked is paycode 122, so we'll use that code.
      '    If Not Save_Hours(tc.employeeID,
      '               tc.payPeriodStart.AddDays(13),
      '               "122",
      '               0,
      '               holidayPayrate,
      '               tc.departmentNumber,
      '               tc.classify) Then
      '      Return False
      '    Else
      '      Add_Timestore_Note(tc.employeeID, tc.payPeriodStart.AddDays(13), "Marked Ineligible for " & IneligibleHours.ToString & " hours at for the holiday in this pay period. These hours will not be paid or banked.", UpdateBy)
      '    End If

      '  Else
      '    If Not Save_Hours(tc.employeeID,
      '               tc.payPeriodStart.AddDays(13),
      '               "134",
      '               0,
      '               holidayPayrate,
      '               tc.departmentNumber,
      '               tc.classify) And
      '    Save_Hours(tc.employeeID,
      '               tc.payPeriodStart.AddDays(13),
      '               "122",
      '               0,
      '               holidayPayrate,
      '               tc.departmentNumber,
      '               tc.classify) Then
      '      Return False
      '    Else
      '      Add_Timestore_Note(tc.employeeID, tc.payPeriodStart.AddDays(13), "Marked Ineligible for " & IneligibleHours.ToString & " hours at for the holiday in this pay period. These hours will not be paid or banked.", UpdateBy)
      '    End If
      '  End If
      'End If

    End If

    If HolidayBankHoursPaid > 0 Then
      If tc.bankedHoliday < HolidayBankHoursPaid Then
        Return False
      Else
        ' 8/9/2015 changed from using tc.holidayincrement to holidaybankedhourspaid
        If Not Save_Hours(tc.employeeID, tc.payPeriodStart.AddDays(13), "124",
                          HolidayBankHoursPaid, holidayPayrate, tc.departmentNumber, tc.classify) Then
          Return False
        Else
          Add_Timestore_Note(tc.employeeID, tc.payPeriodStart.AddDays(13), HolidayBankHoursPaid.ToString & " banked holiday hours to be paid at pay rate " & holidayPayrate.ToString & ".", UpdateBy)
          Return True
        End If
      End If
    End If
    Return True
  End Function

  Public Function GetGenericTimeData(StartDate As Date, EndDate As Date, Fields As List(Of String)) As List(Of GenericTimeData)
    Dim fdl As List(Of FinanceData) = (From fd In GetAllEmployeeDataFromFinPlus() Where (fd.TerminationDate = Date.MaxValue Or (fd.TerminationDate >= StartDate And fd.TerminationDate <= EndDate)) Select fd).ToList
    Dim teledl As List(Of TelestaffTimeData) = GetEmployeeDataFromTelestaff(StartDate, "", EndDate)
    Dim tcl As List(Of TimecardTimeData) = GetEmployeeDataFromTimecard(StartDate, 0, EndDate)
    Dim tsl As List(Of TimecardTimeData) = GetEmployeeDataFromTimeStore(StartDate, 0, EndDate)

    Dim gtdl As New List(Of GenericTimeData)
    For Each f In fdl
      Select Case f.Department
        Case "1703", "2103", "2102" ' public safety
          For Each t In (From tele In teledl Where tele.EmployeeId = f.EmployeeId Select tele).ToList
            gtdl.Add(New GenericTimeData(t, f))
          Next
        Case "3701", "3709", "3711", "3712" ' public works


        Case Else ' timecard / timestore
          ' Here we're going to look in the timecard system first.  If they have any
          ' data there, we're going to use that and ignore the timestore data.
          ' Only if they don't have any timecard data do we even look for the timestore data.
          Dim tc = (From timecard In tcl Where timecard.EmployeeID = f.EmployeeId Select timecard).ToList
          If tc.Count = 0 Then tc = (From timestore In tsl Where timestore.EmployeeID = f.EmployeeId Select timestore).ToList
          For Each t In tc
            gtdl.Add(New GenericTimeData(t, f))
          Next

      End Select
    Next
    'Dim f As FinanceData = fdl.First
    'For Each t In teledl
    '    If t.EmployeeId <> f.EmployeeId Then f = (From fd In fdl Where fd.EmployeeId = t.EmployeeId Select fd).First
    '    gtdl.Add(New GenericTimeData(t, f))
    'Next
    'For Each t In tcl
    '    If t.EmployeeID <> f.EmployeeId Then f = (From fd In fdl Where fd.EmployeeId = t.EmployeeID Select fd).First
    '    gtdl.Add(New GenericTimeData(t, f))
    'Next

    Return FilterGTD(gtdl, Fields)
  End Function

  Private Function FilterGTD(ByRef gtd As List(Of GenericTimeData), Fields As List(Of String)) As List(Of GenericTimeData)
    Dim sbTmp As New StringBuilder
    With sbTmp
      For Each f In Fields
        If .Length > 0 Then .Append(" OR ")
        .Append(f).Append(" > 0 ")
      Next
    End With
    Dim found = gtd.Where(sbTmp.ToString).ToList
    Return found
  End Function

  Public Function Save_Timestore_Data(T As TimecardTimeData,
                                      SavingEmployee As Timecard_Access) As Boolean
    ' here's what this process is going to do:
    'User chooses to save record
    'check If work record exists
    Dim existing As New Saved_TimeStore_Data(T.EmployeeID, T.WorkDate)
    'Dim existing = Get_Saved_Timestore_Data_by_Date(T.EmployeeID, T.WorkDate)

    If existing.employee_id = 0 Then ' record does Not exist

      Dim workID As Long = Save_Timestore_Work_Data(T, SavingEmployee) '   Insert New work record, return work record ID
      If Save_Hours_To_Approve(T, workID, SavingEmployee) Then
        ' If this user's leave doesn't require approval, let's go ahead and approve everything.
        If Not SavingEmployee.RequiresApproval Then
          existing = New Saved_TimeStore_Data(T.EmployeeID, T.WorkDate)
          For Each hta In existing.HoursToApprove
            Finalize_Leave_Request(True, hta.approval_hours_id, hta.hours_used, "", SavingEmployee)
          Next
        Else
          'let's remove any current approvals.
          Dim payperiodstart As Date = GetPayPeriodStart(T.WorkDate)
          If Clear_Saved_Timestore_Data(T.EmployeeID, payperiodstart) = -1 Then
            Add_Timestore_Note(T.EmployeeID, payperiodstart.AddDays(13), "Approval Removed, Hours or Payrate has changed.")
          End If
        End If
        Return True
      Else
        Return False
      End If


      ' 8/26/2015 Going to skip the auto approving.
      '   check person saving rows' approval level (Check Authority)
      '       If they Then have the authority To "pre-approve" the time
      '		    insert the hours into the hours To approve table Using the work record id, capturing the ID inserted.
      '           Use the returned ID to save the approval data for each hours to add the rows to approve record.

      '       If they do not have the authority to pre-approve
      '           insert the hours into the hours to approve table using the work record id

    Else ' record exists

      If Not Update_Timestore_Work_Data(T, SavingEmployee, existing) Then Return False

      Select Case Update_Hours_To_Approve(T, SavingEmployee, existing)
        Case 5 ' data was updated and everything was good.
          'let's remove any current approvals.
          Dim payperiodstart As Date = GetPayPeriodStart(T.WorkDate)
          If Clear_Saved_Timestore_Data(T.EmployeeID, payperiodstart) = -1 Then
            Add_Timestore_Note(T.EmployeeID, payperiodstart.AddDays(13), "Approval Removed, Hours or Payrate has changed. **")
          End If
        Case 2 ' data wasn't updated but it completed normally
          If Not Compare_Existing_To_Current(existing, T) Then
            Dim payperiodstart As Date = GetPayPeriodStart(T.WorkDate)
            If Clear_Saved_Timestore_Data(T.EmployeeID, payperiodstart) = -1 Then
              Add_Timestore_Note(T.EmployeeID, payperiodstart.AddDays(13), "Approval Removed, Hours or Payrate has changed. *")
            End If
          End If
        Case 1 ' data was updated but there was an error
          Return False
        Case -1 ' error
          Return False
      End Select
      If Not SavingEmployee.RequiresApproval Then
        existing = New Saved_TimeStore_Data(T.EmployeeID, T.WorkDate)
        For Each hta In existing.HoursToApprove
          If Not hta.is_approved Then Finalize_Leave_Request(True, hta.approval_hours_id, hta.hours_used, "", SavingEmployee)
        Next
      End If
      Return True

      '   Return ID Of existing data
      '       get list of hours to approve rows with that ID
      '       get list of approvals that match those IDs
      '       if the hours to approve are already approved and the new hours are greater than the hours approved
      '           then remove the approval.
      '       update work record with New data (ID stays the same), update date_last_updated with current date/time
      '       Loop through each Hours to approve row And update the used_hours And worktimes field to match the current saved value, unless it Is denied, then enforce it to zero.

      '       insert any New hours to approve that were Not present

    End If
  End Function

  Private Function Compare_Existing_To_Current(Existing As Saved_TimeStore_Data, T As TimecardTimeData) As Boolean
    If T.BreakCreditHours <> Existing.break_credit Or T.WorkHours <> Existing.work_hours _
        Or T.HolidayHours <> Existing.holiday Or T.LWOPHours <> Existing.leave_without_pay _
        Or T.Vehicle <> Existing.vehicle Or T.TotalHours <> Existing.total_hours Then
      Return False ' no match
    Else
      Return True ' they match
    End If
  End Function

  Private Function Save_Hours_To_Approve(t As TimecardTimeData, WorkID As Long,
                                         SavingEmployee As Timecard_Access) As Boolean

    If WorkID = -1 Then Return False

    Dim dbc As New Tools.DB(GetCS(ConnectionStringType.Timestore), toolsAppId, toolsDBError)
    Dim sbQ As New StringBuilder
    With sbQ
      .AppendLine("USE TimeStore;")
      For Each kvp As KeyValuePair(Of String, Timestore_Field_With_Hours) In t.CreateHoursOutput
        If kvp.Value.Field_Hours > 0 Then
          .Append("DELETE FROM Hours_To_Approve WHERE work_hours_id=@WorkID AND field_id=")
          .Append(kvp.Value.Field_ID).AppendLine(";")
          .AppendLine("INSERT INTO Hours_To_Approve (work_hours_id, field_id, hours_used, worktimes)")
          .Append("VALUES (@WorkID, ")
          .Append(kvp.Value.Field_ID).Append(", ")
          .Append(kvp.Value.Field_Hours).Append(", ")
          If kvp.Value.Field_Name = "OnCallTotalHours" Then
            .Append("@OnCallWorkTimes);")
          Else
            .Append("'');")
          End If
        End If
      Next


    End With

    Dim P() As SqlParameter
    If sbQ.ToString.Contains("@OnCallWorkTimes") Then
      P = {
          New SqlParameter("@WorkID", Data.SqlDbType.BigInt) With {.Value = WorkID},
          New SqlParameter("@OnCallWorkTimes", Data.SqlDbType.VarChar, 500) With {.Value = t.CreateTimesOutput()("OnCallWorkTimes")}
      }
    Else
      P = {
          New SqlParameter("@WorkID", Data.SqlDbType.BigInt) With {.Value = WorkID}
      }
    End If

    If sbQ.ToString.Length > 20 Then ' We don't want to run a query if there is no content.

      Dim i As Long = 0
      Try
        i = dbc.ExecuteNonQuery(sbQ.ToString, P)
        Return i > -1
      Catch ex As Exception
        Log(ex)
        Return False
      End Try

    Else
      Return True
    End If
  End Function

  Private Function Update_Hours_To_Approve(t As TimecardTimeData, SavingEmployee As Timecard_Access,
                                           ExistingData As Saved_TimeStore_Data) As Integer

    Dim dbc As New Tools.DB(GetCS(ConnectionStringType.Timestore), toolsAppId, toolsDBError)
    Dim sbQ As New StringBuilder
    ' dataUpdated is going to be our test variable, if we're changing any saved data, we're going to 
    ' set dataUpdated to true.  Then that will be used in the return to communicate that data was updated.
    Dim dataUpdated As Boolean = False
    With sbQ
      .AppendLine("USE TimeStore;")
      For Each kvp As KeyValuePair(Of String, Timestore_Field_With_Hours) In t.CreateHoursOutput
        Dim found = (From d In ExistingData.HoursToApprove Where d.field_id = kvp.Value.Field_ID Select d)
        If kvp.Value.Field_Hours > 0 Or found.Count > 0 Then
          If found.Count > 0 AndAlso found.First.approval_id > 0 Then
            ' If either of these is true, we want to remove any approvals.
            dataUpdated = True
            ' Update if different
            Dim SavedData As Saved_TimeStore_Data_To_Approve = found.First
            Dim SavedHoursUsed As Double = SavedData.hours_used
            If SavedHoursUsed > kvp.Value.Field_Hours Or SavedHoursUsed < kvp.Value.Field_Hours Or
                SavedData.worktimes <> t.CreateTimesOutput()("OnCallWorkTimes") Then



              .Append("UPDATE Hours_To_Approve SET date_added = GETDATE(), hours_used = ")
              .Append(kvp.Value.Field_Hours)

              If kvp.Value.Field_Name = "OnCallTotalHours" Then
                .Append(", worktimes=@OnCallWorkTimes ")
              End If

              .Append(" WHERE approval_hours_id = ").Append(SavedData.approval_hours_id).AppendLine(";")
              If SavedHoursUsed < kvp.Value.Field_Hours AndAlso SavedData.is_approved Then
                ' let's delete the current approval
                .Append("DELETE FROM Approval_Data WHERE approval_id=")
                .Append(SavedData.approval_id).AppendLine(";")
              End If

            End If
          Else
            'If found.Count > 0 Then
            '  .Append("DELETE FROM Hours_To_Approve WHERE approval_hours_id=")
            '  .Append(found.First.approval_hours_id).AppendLine(";")
            'End If
            .Append("DELETE FROM Hours_To_Approve WHERE work_hours_id=@WorkID AND field_id=")
            .Append(kvp.Value.Field_ID).AppendLine(";")
            .AppendLine("INSERT INTO Hours_To_Approve (work_hours_id, field_id, hours_used, worktimes)")
            .Append("VALUES (@WorkID, ")
            .Append(kvp.Value.Field_ID).Append(", ")
            .Append(kvp.Value.Field_Hours).Append(", ")
            If kvp.Value.Field_Name = "OnCallTotalHours" Then
              .Append("@OnCallWorkTimes);")
            Else
              .Append("'');")
            End If
          End If

        End If
      Next
    End With

    Dim P() As SqlParameter
    If sbQ.ToString.Contains("@OnCallWorkTimes") Then
      P = {
          New SqlParameter("@WorkID", Data.SqlDbType.BigInt) With {.Value = ExistingData.work_hours_id},
          New SqlParameter("@OnCallWorkTimes", Data.SqlDbType.VarChar, 500) With {.Value = t.CreateTimesOutput()("OnCallWorkTimes")}
      }
    Else
      P = {
          New SqlParameter("@WorkID", Data.SqlDbType.BigInt) With {.Value = ExistingData.work_hours_id}
      }
    End If
    If sbQ.ToString.Length > 20 Then

      Dim i As Long = 0
      Try
        i = dbc.ExecuteNonQuery(sbQ.ToString, P)
        If dataUpdated Then
          If i > -1 Then
            Return 5 ' data was updated and everything was good.
          Else
            Return 1 ' data was updated but there was an error
          End If
        Else
          If i > -1 Then
            Return 2 ' data wasn't updated but it completed normally
          Else
            Return -1 ' data wasn't updated and there was an error
          End If
        End If
        'Return i > -1
      Catch ex As Exception
        Log(ex)
        Return -1
      End Try

    Else
      Return 2
    End If

  End Function

  Private Function Update_Timestore_Work_Data(t As TimecardTimeData, SavingEmployee As Timecard_Access,
                                              ExistingData As Saved_TimeStore_Data) As Boolean
    Dim dbc As New Tools.DB(GetCS(ConnectionStringType.Timestore), toolsAppId, toolsDBError)
    Dim sbQ As New StringBuilder

    Dim P() As SqlParameter = {
        New SqlParameter("@DeptID", Data.SqlDbType.VarChar, 16) With {.Value = t.DepartmentNumber},
        New SqlParameter("@WorkTimes", Data.SqlDbType.VarChar, 500) With {.Value = t.WorkTimes},
        New SqlParameter("@BreakCredit", Data.SqlDbType.Float) With {.Value = t.BreakCreditHours},
        New SqlParameter("@WorkHours", Data.SqlDbType.Float) With {.Value = t.WorkHours},
        New SqlParameter("@HolidayHours", Data.SqlDbType.Float) With {.Value = t.HolidayHours},
        New SqlParameter("@DoubleTimeHours", Data.SqlDbType.Float) With {.Value = t.DoubleTimeHours},
        New SqlParameter("@LWOPHours", Data.SqlDbType.Float) With {.Value = t.LWOPHours},
        New SqlParameter("@TotalHours", Data.SqlDbType.Float) With {.Value = t.TotalHours},
        New SqlParameter("@Vehicle", Data.SqlDbType.Int) With {.Value = t.Vehicle},
        New SqlParameter("@Comment", Data.SqlDbType.VarChar, 255) With {.Value = t.Comment},
        New SqlParameter("@ByEmployeeID", Data.SqlDbType.Int) With {.Value = SavingEmployee.EmployeeID},
        New SqlParameter("@ByUsername", Data.SqlDbType.VarChar, 100) With {.Value = SavingEmployee.UserName},
        New SqlParameter("@ByMachinename", Data.SqlDbType.VarChar, 100) With {.Value = SavingEmployee.MachineName},
        New SqlParameter("@ByIpAddress", Data.SqlDbType.VarChar, 20) With {.Value = SavingEmployee.IPAddress},
        New SqlParameter("@ExistingWorkID", Data.SqlDbType.BigInt) With {.Value = ExistingData.work_hours_id}
    }
    With sbQ
      .AppendLine("USE TimeStore;")
      .AppendLine("UPDATE Work_Hours SET dept_id=@DeptID, work_times=@WorkTimes,")
      .AppendLine("break_credit=@BreakCredit,work_hours=@WorkHours,holiday=@HolidayHours,")
      .AppendLine("leave_without_pay=@LWOPHours,total_hours=@TotalHours,vehicle=@Vehicle,")
      .AppendLine("comment=@Comment,by_employeeid=@ByEmployeeID,by_username=@ByUsername,")
      .AppendLine("by_machinename=@ByMachinename, by_ip_address=@ByIpAddress, ")
      .AppendLine("doubletime_hours=@DoubleTimeHours, date_last_updated=GETDATE()")
      .AppendLine("WHERE work_hours_id=@ExistingWorkID;")
    End With
    Dim i As Long = 0
    Try
      i = dbc.ExecuteNonQuery(sbQ.ToString, P)
      Return i >= 1
    Catch ex As Exception
      Log(ex)
      Return -1
    End Try
  End Function

  Private Function Save_Timestore_Work_Data(t As TimecardTimeData, SavingEmployee As Timecard_Access) As Long
    ' Here we're going to insert their time data into the database.
    Dim dbc As New Tools.DB(GetCS(ConnectionStringType.Timestore), toolsAppId, toolsDBError)
    Dim sbQ As New StringBuilder
    Dim payPeriodEnding As Date = CType(t.PPD, Date).AddDays(13)

    Dim P() As SqlParameter = {
        New SqlParameter("@EmployeeId", Data.SqlDbType.Int) With {.Value = t.EmployeeID},
        New SqlParameter("@PayPeriodEnding", Data.SqlDbType.Date) With {.Value = payPeriodEnding},
        New SqlParameter("@DeptID", Data.SqlDbType.VarChar, 16) With {.Value = t.DepartmentNumber},
        New SqlParameter("@WorkDate", Data.SqlDbType.Date) With {.Value = t.WorkDate},
        New SqlParameter("@WorkTimes", Data.SqlDbType.VarChar, 500) With {.Value = t.WorkTimes},
        New SqlParameter("@BreakCredit", Data.SqlDbType.Float) With {.Value = t.BreakCreditHours},
        New SqlParameter("@WorkHours", Data.SqlDbType.Float) With {.Value = t.WorkHours},
        New SqlParameter("@HolidayHours", Data.SqlDbType.Float) With {.Value = t.HolidayHours},
        New SqlParameter("@DoubleTimeHours", Data.SqlDbType.Float) With {.Value = t.DoubleTimeHours},
        New SqlParameter("@LWOPHours", Data.SqlDbType.Float) With {.Value = t.LWOPHours},
        New SqlParameter("@TotalHours", Data.SqlDbType.Float) With {.Value = t.TotalHours},
        New SqlParameter("@Vehicle", Data.SqlDbType.Int) With {.Value = t.Vehicle},
        New SqlParameter("@Comment", Data.SqlDbType.VarChar, 255) With {.Value = t.Comment},
        New SqlParameter("@ByEmployeeID", Data.SqlDbType.Int) With {.Value = SavingEmployee.EmployeeID},
        New SqlParameter("@ByUsername", Data.SqlDbType.VarChar, 100) With {.Value = SavingEmployee.UserName},
        New SqlParameter("@ByMachinename", Data.SqlDbType.VarChar, 100) With {.Value = SavingEmployee.MachineName},
        New SqlParameter("@ByIpAddress", Data.SqlDbType.VarChar, 20) With {.Value = SavingEmployee.IPAddress}
    }
    With sbQ
      .AppendLine("USE TimeStore;")
      .AppendLine("IF EXISTS(SELECT employee_id, work_date FROM Work_Hours ")
      .AppendLine("WHERE employee_id=@EmployeeId AND work_date=@WorkDate)")
      .AppendLine("BEGIN")
      .AppendLine("SELECT -1")
      .AppendLine("END")
      .AppendLine("ELSE")
      .AppendLine("BEGIN")
      .AppendLine("INSERT INTO Work_Hours (employee_id,dept_id,pay_period_ending,work_date,work_times")
      .AppendLine(",break_credit,work_hours,holiday,leave_without_pay,total_hours,vehicle,comment")
      .AppendLine(",by_employeeid, by_username, by_machinename, by_ip_address, doubletime_hours) ")
      .AppendLine("OUTPUT INSERTED.work_hours_id ")
      .AppendLine("VALUES (@EmployeeId, @DeptID, @PayPeriodEnding, @WorkDate, @WorkTimes ")
      .AppendLine(", @BreakCredit, @WorkHours, @HolidayHours, @LWOPHours, @TotalHours, @Vehicle, @Comment")
      .AppendLine(", @ByEmployeeID, @ByUsername, @ByMachinename, @ByIpAddress, @DoubleTimeHours);")
      .AppendLine("END")
    End With

    Dim i As Long = 0
    Try
      i = dbc.ExecuteScalar(sbQ.ToString, P)
      Return i
    Catch ex As Exception
      Log(ex)
      Return -1
    End Try
  End Function

  Public Function Get_Hours_To_Approve(StartDate As Date, Optional EmployeeID As Integer = 0,
                                       Optional EndDate As Date? = Nothing, Optional DeptId As String = "") As List(Of TimeStore_Approve_Hours_Display)
    Dim dbc As New Tools.DB(GetCS(ConnectionStringType.Timestore), toolsAppId, toolsDBError)
    Dim sbQ As New StringBuilder
    With sbQ
      .AppendLine("USE TimeStore;")
      .AppendLine("SELECT access_type,reports_to, dept_id,employee_id,pay_period_ending,work_date,field_id")
      .AppendLine(",hours_used,worktimes,payrate,approval_hours_id,approval_id,hours_approved,is_approved")
      .AppendLine(",date_approval_added,by_employeeid,by_username,by_machinename,by_ip_address, note, comment")
      .AppendLine("FROM vwApproveHoursDisplay")
      .Append("WHERE work_date >= '").Append(StartDate.ToShortDateString).Append("' ")
      If EndDate.HasValue Then
        .Append("AND work_date <= '").Append(EndDate.Value.ToShortDateString).Append("' ")
      End If
      If EmployeeID > 0 Then
        .Append("AND employee_id = ").Append(EmployeeID)
      End If
      If DeptId.Length > 0 Then
        .Append("AND dept_id = '").Append(DeptId.Trim).Append("' ")
      End If
      .AppendLine(" ORDER BY dept_id, employee_id, work_date;")
    End With
    Dim dsTmp As DataSet
    Try
      dsTmp = dbc.Get_Dataset(sbQ.ToString)
      Dim adl As List(Of AD_EmployeeData) = GetADEmployeeData()
      Dim ad As AD_EmployeeData = adl.First
      Dim t As New List(Of TimeStore_Approve_Hours_Display)
      For Each d In dsTmp.Tables(0).Rows
        If ad.EmployeeID <> d("employee_id") Then
          Dim tmp = (From current In adl Where current.EmployeeID = d("employee_id") Select current)
          If tmp.Count > 0 Then
            ad = tmp.First
            Dim tahd As New TimeStore_Approve_Hours_Display(d, ad.Name)
            If tahd.field.Requires_Approval Then t.Add(tahd)
          Else
            Dim tahd As New TimeStore_Approve_Hours_Display(d, "")
            If tahd.field.Requires_Approval Then t.Add(tahd)
          End If
        Else
          Dim tahd As New TimeStore_Approve_Hours_Display(d, ad.Name)
          If tahd.field.Requires_Approval Then t.Add(tahd)
        End If
      Next
      Return t.OrderBy(Function(x) x.dept_id).ThenBy(Function(x) x.employee_name).ToList
    Catch ex As Exception
      Log(ex)
      Return Nothing
    End Try
  End Function

  Function Get_All_Cached_ReportsTo() As Dictionary(Of Integer, List(Of Integer))
    Return myCache.GetItem("reportsto")
  End Function

  Function Get_All_ReportsTo() As Dictionary(Of Integer, List(Of Integer))
    Dim dbc As New Tools.DB(GetCS(ConnectionStringType.Timestore), toolsAppId, toolsDBError)
    Dim query As String = "SELECT employee_id, reports_to FROM Access WHERE reports_to <> 0"
    Dim ds As DataSet = dbc.Get_Dataset(query)
    Dim reportsTo As New Dictionary(Of Integer, List(Of Integer))
    Try
      Dim tmp = (From d In ds.Tables(0).AsEnumerable
                 Select New ReportsTo With {.eId = d("employee_id"),
                  .rTo = d("reports_to")}).ToList
      For Each t In tmp
        Get_ReportsTo(t.rTo, t.rTo, tmp, reportsTo)
      Next
      Return reportsTo
    Catch ex As Exception
      Log(ex)
      Return Nothing
    End Try
  End Function

  Sub Get_ReportsTo(base_EmployeeId As Integer,
                    reportsTo_EmployeeId As Integer,
                    ByRef ReportsToList As List(Of ReportsTo),
                    ByRef ReportsTo As Dictionary(Of Integer, List(Of Integer)))
    Dim found = (From r In ReportsToList
                 Where r.rTo = reportsTo_EmployeeId
                 Select r.eId).ToList

    If found.Count > 0 Then
      If Not ReportsTo.ContainsKey(base_EmployeeId) Then
        ReportsTo(base_EmployeeId) = New List(Of Integer)
      End If
      '  ReportsTo(base_EmployeeId).AddRange(found)
      For Each f In found
        If Not ReportsTo(base_EmployeeId).Contains(f) Then ReportsTo(base_EmployeeId).Add(f)
        Get_ReportsTo(base_EmployeeId, f, ReportsToList, ReportsTo)
      Next
    End If

  End Sub

  Function Save_Comp_Time_Earned(EmployeeID As Integer, PayPeriodEnding As Date, Week1 As Double, Week2 As Double,
                                 SavingEmployee As Timecard_Access) As Boolean
    Dim dbc As New Tools.DB(GetCS(ConnectionStringType.Timestore), toolsAppId, toolsDBError)
    Dim sbQ As New StringBuilder

    Dim P() As SqlParameter = {
        New SqlParameter("@EmployeeID", Data.SqlDbType.Int) With {.Value = EmployeeID},
        New SqlParameter("@PayPeriodEnding", Data.SqlDbType.Date) With {.Value = PayPeriodEnding},
        New SqlParameter("@Week1", Data.SqlDbType.Float) With {.Value = Week1},
        New SqlParameter("@Week2", Data.SqlDbType.Float) With {.Value = Week2},
        New SqlParameter("@ByEmployeeID", Data.SqlDbType.Int) With {.Value = SavingEmployee.EmployeeID},
        New SqlParameter("@ByUsername", Data.SqlDbType.VarChar, 100) With {.Value = SavingEmployee.UserName},
        New SqlParameter("@ByMachinename", Data.SqlDbType.VarChar, 100) With {.Value = SavingEmployee.MachineName},
        New SqlParameter("@ByIpAddress", Data.SqlDbType.VarChar, 20) With {.Value = SavingEmployee.IPAddress}
    }
    With sbQ
      .AppendLine("USE TimeStore;")
      .AppendLine("DELETE FROM Comp_Time_Earned_Hours WHERE employee_id=@EmployeeID AND ")
      .AppendLine("pay_period_ending=@PayPeriodEnding;")
      .AppendLine("INSERT INTO Comp_Time_Earned_Hours (employee_id, pay_period_ending, comp_time_earned_week1, ")
      .AppendLine("comp_time_earned_week2, added_by_employeeid, added_by_username, added_by_machinename, ")
      .AppendLine("added_by_ip_address) VALUES (@EmployeeID, @PayPeriodEnding, @Week1, @Week2, @ByEmployeeID, ")
      .AppendLine("@ByUsername, @ByMachinename, @ByIpAddress);")
    End With
    Dim i As Long = 0
    Try
      i = dbc.ExecuteNonQuery(sbQ.ToString, P)
      Return i >= 1
    Catch ex As Exception
      Log(ex)
      Return -1
    End Try
  End Function

  Public Function Remove_Denied_Hours(ApprovalHoursId As Long,
                                      Hours As Double) As Integer

    ' If the row is denied, we need to reduce their hours to 0 for that row 
    ' and then insert the info.
    If Hours = 0 Then Return 0 ' no point in running this to remove no hours.
    Dim dp As New DynamicParameters()
    dp.Add("@ApprovalHoursID", ApprovalHoursId)
    dp.Add("@HoursApproved", Hours)
    Dim query As String = "
      UPDATE Work_Hours Set total_hours = total_hours - @HoursApproved 
      WHERE work_hours_id In (
        SELECT work_hours_id 
        FROM Hours_To_Approve 
        WHERE approval_hours_id=@ApprovalHoursID
      );
"
    Return Exec_Query(query, dp, ConnectionStringType.Timestore)

  End Function

  Public Function Finalize_Leave_Request(Approved As Boolean,
                                         ApprovalHoursID As Long,
                                         Hours As Double,
                                         Note As String,
                                         SavingEmployee As Timecard_Access) As Integer

    If Not Approved Then Remove_Denied_Hours(ApprovalHoursID, Hours)

    Dim dp As New DynamicParameters()
    dp.Add("@ApprovalHoursID", ApprovalHoursID)
    'dp.Add("@HoursApproved", Hours)
    dp.Add("@IsApproved", IIf(Approved, 1, 0))
    dp.Add("@ByEmployeeID", SavingEmployee.EmployeeID)
    dp.Add("@ByUsername", SavingEmployee.UserName)
    dp.Add("@ByMachinename", SavingEmployee.MachineName)
    dp.Add("@ByIpAddress", SavingEmployee.IPAddress)
    dp.Add("@Note", Note)
    Dim query As String = "
      USE TimeStore;
      INSERT INTO Approval_Data (
        approval_hours_id, 
        hours_approved, 
        is_approved, 
        by_employeeid, 
        by_username, 
        by_machinename, 
        by_ip_address, 
        note
      ) 
      SELECT 
        approval_hours_id,
        hours_used,
        @IsApproved,
        @ByEmployeeID,
        @ByUsername,
        @ByMachinename,
        @ByIpAddress,
        @Note
      FROM Hours_To_Approve
      WHERE approval_hours_id=@ApprovalHoursID
"
    Return Exec_Query(query, dp, ConnectionStringType.Timestore)

    'Dim dbc As New Tools.DB(GetCS(ConnectionStringType.Timestore), toolsAppId, toolsDBError)
    'Dim sbQ As New StringBuilder
    'With sbQ
    '  .AppendLine("USE TimeStore;")
    '  .AppendLine("IF EXISTS (SELECT approval_hours_id FROM Hours_To_Approve ")
    '  .AppendLine("   WHERE approval_hours_id=@ApprovalHoursID)")
    '  .AppendLine("BEGIN")
    '  .AppendLine("INSERT INTO Approval_Data (approval_hours_id, hours_approved, is_approved, ")
    '  .AppendLine("by_employeeid, by_username, by_machinename, by_ip_address, note) ")
    '  .AppendLine("VALUES (@ApprovalHoursID, @HoursApproved, @IsApproved, ")
    '  .AppendLine("@ByEmployeeID, @ByUsername, @ByMachinename, @ByIpAddress, @Note);")
    '  If Not Approved Then
    '    ' If the row is denied, we need to reduce their hours to 0 for that row and then insert the info.
    '    .AppendLine("UPDATE Work_Hours SET total_hours = total_hours - @HoursApproved WHERE work_hours_id IN (SELECT work_hours_id FROM Hours_To_Approve WHERE approval_hours_id=@ApprovalHoursID);")
    '  End If
    '  .AppendLine("SELECT 1;")
    '  .AppendLine("END")
    '  .AppendLine("ELSE ")
    '  .AppendLine("BEGIN")
    '  .AppendLine("SELECT -5;")
    '  .AppendLine("END")
    'End With
    'Dim ApproveValue As Integer = IIf(Approved, 1, 0)

    'Dim P() As SqlParameter
    'P = {
    '    New SqlParameter("@ApprovalHoursID", Data.SqlDbType.BigInt) With {.Value = ApprovalHoursID},
    '    New SqlParameter("@HoursApproved", Data.SqlDbType.Float) With {.Value = Hours},
    '    New SqlParameter("@IsApproved", Data.SqlDbType.TinyInt) With {.Value = ApproveValue},
    '    New SqlParameter("@Note", Data.SqlDbType.VarChar, 1024) With {.Value = Note},
    '    New SqlParameter("@ByEmployeeID", Data.SqlDbType.Int) With {.Value = SavingEmployee.EmployeeID},
    '    New SqlParameter("@ByUsername", Data.SqlDbType.VarChar, 100) With {.Value = SavingEmployee.UserName},
    '    New SqlParameter("@ByMachinename", Data.SqlDbType.VarChar, 100) With {.Value = SavingEmployee.MachineName},
    '    New SqlParameter("@ByIpAddress", Data.SqlDbType.VarChar, 20) With {.Value = SavingEmployee.IPAddress}
    '}

    'Dim i As Long = 0
    'Try
    '  i = dbc.ExecuteScalar(sbQ.ToString, P)
    '  Return i
    'Catch ex As Exception
    '  Log(ex)
    '  Return -1
    'End Try

  End Function

  Public Function Clear_Saved_Timestore_Data(employeeID As Integer, PayPeriodStart As Date, Optional IgnoreHoliday As Boolean = False) As Integer
    Dim dbc As New Tools.DB(GetCS(ConnectionStringType.Timestore), toolsAppId, toolsDBError)
    Dim PayPeriodEnd As Date = PayPeriodStart.AddDays(13)
    Dim P() As SqlParameter = New SqlParameter() _
            {
                New SqlParameter("@EmployeeId", Data.SqlDbType.Int) With {.Value = employeeID},
                New SqlParameter("@PayPeriodEnding", Data.SqlDbType.Date) With {.Value = PayPeriodEnd}
            }
    Dim sbQuery As New StringBuilder
    With sbQuery
      .AppendLine("DECLARE @Test INTEGER;")
      .AppendLine("IF EXISTS(SELECT initial_approval_employeeid FROM Saved_Time ")
      .AppendLine("WHERE employee_id=@EmployeeId AND pay_period_ending=@PayPeriodEnding ")
      .AppendLine("AND (initial_approval_employeeid IS NOT NULL OR final_approval_employeeid IS NOT NULL)) ")
      .AppendLine("BEGIN SET @Test = -1; END")
      .AppendLine("ELSE")
      .AppendLine("BEGIN SET @Test = -5; END")
      .AppendLine("UPDATE Saved_Time SET ")
      .AppendLine("initial_approval_username=NULL, initial_approval_employeeid=NULL,")
      .AppendLine("initial_approval_machine_name=NULL, initial_approval_ip_address=NULL, ")
      .AppendLine("initial_approval_date=NULL, ")
      .AppendLine("final_approval_username=NULL, final_approval_employeeid=NULL,")
      .AppendLine("final_approval_machine_name=NULL, final_approval_ip_address=NULL, ")
      .AppendLine("final_approval_date=NULL WHERE employee_id=@EmployeeId AND pay_period_ending=@PayPeriodEnding;")
      .Append("DELETE FROM Saved_Time WHERE employee_id=@EmployeeId AND pay_period_ending=@PayPeriodEnding ")
      If IgnoreHoliday Then
        .Append("AND paycode NOT IN ('134', '124', '122', '800') ")
      End If
      .AppendLine(";")
      .AppendLine("SELECT @Test;")
    End With
    Try
      Dim i As Integer = dbc.ExecuteScalar(sbQuery.ToString, P)
      Return i
    Catch ex As Exception
      Log(ex)
      Return -10
    End Try
  End Function

End Module

