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

  Public Function Get_Data(Of T)(query As String, cst As ConnectionStringType) As List(Of T)
    Try
      Using db As IDbConnection = New SqlConnection(GetCS(cst))
        Return db.Query(Of T)(query)
      End Using
    Catch ex As Exception
      Dim e As New ErrorLog(ex, query)
      Return Nothing
    End Try
  End Function

  Public Function Get_Data(Of T)(query As String, dbA As DynamicParameters, cst As ConnectionStringType) As List(Of T)
    Try
      Using db As IDbConnection = New SqlConnection(GetCS(cst))
        Return db.Query(Of T)(query, dbA)
      End Using
    Catch ex As Exception
      Dim e As New ErrorLog(ex, query)
      Return Nothing
    End Try
  End Function

  Public Function GetCS(cst As ConnectionStringType) As String
    ' This function will return a specific connectionstring based on the machine it's currently running on
    ' MSIL03, CLAYBCCDV10 = Development / Testing
    ' CLAYBCCIIS01 = Production
    Select Case Environment.MachineName.ToUpper
      Case "CLAYBCCDV10" ', "MISSL01" ' QA
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
          Case ConnectionStringType.FinplusTraining
            Return ConfigurationManager.ConnectionStrings("FinplusQA").ConnectionString
          Case ConnectionStringType.Log
            Return ConfigurationManager.ConnectionStrings("Log").ConnectionString

          Case Else
            Return ""
        End Select

      Case "CLAYBCCIIS01", "MISSL01" ' Production
        Select Case cst
          Case ConnectionStringType.Telestaff
            Return ConfigurationManager.ConnectionStrings("TimestoreProduction").ConnectionString
          Case ConnectionStringType.Timecard
            Return ConfigurationManager.ConnectionStrings("TimecardProduction").ConnectionString

          Case ConnectionStringType.Timestore
            Return ConfigurationManager.ConnectionStrings("TimestoreProduction").ConnectionString

          Case ConnectionStringType.FinPlus
            Return ConfigurationManager.ConnectionStrings("FinplusProduction").ConnectionString

          Case ConnectionStringType.FinplusTraining
            Return ConfigurationManager.ConnectionStrings("FinplusQA").ConnectionString

          Case ConnectionStringType.Log
            Return ConfigurationManager.ConnectionStrings("Log").ConnectionString

            'Case ConnectionStringType.FinplusNew
            '  Return ConfigurationManager.ConnectionStrings("FinplusNew").ConnectionString

          Case Else
            Return ""
        End Select

      Case Else
        Return ""

    End Select
  End Function

  Public Function GetADEmployeeData() As Dictionary(Of Integer, AD_EmployeeData)
    Dim CIP As New CacheItemPolicy With {
      .AbsoluteExpiration = Now.AddHours(12)
    }
    Dim key As String = "employee_ad_data"
    Dim aded As Dictionary(Of Integer, AD_EmployeeData) = myCache.GetItem(key, CIP)
    Return aded
  End Function

  'Public Function GetADEmployeeData() As List(Of AD_EmployeeData)
  '  Dim key As String = "employee_ad_data"
  '  Dim adl As List(Of AD_EmployeeData) = myCache.GetItem(key)
  '  Return adl
  'End Function

  Public Function GetCachedEmployeeDataFromFinplus() As List(Of FinanceData)
    Dim CIP As New CacheItemPolicy With {
      .AbsoluteExpiration = Now.AddHours(12)
    }
    Dim key As String = "employeedata" ' & PayPeriodStart.ToShortDateString
    Dim fdl As List(Of FinanceData) = myCache.GetItem(key, CIP)
    Return fdl
  End Function

  Public Function GetCachedEmployeeDataFromFinplusAsDictionary() As Dictionary(Of Integer, FinanceData)
    Dim CIP As New CacheItemPolicy With {
      .AbsoluteExpiration = Now.AddHours(12)
    }
    Dim key As String = "employeedata_dict" ' & PayPeriodStart.ToShortDateString
    Dim fdl As Dictionary(Of Integer, FinanceData) = myCache.GetItem(key, CIP)
    Return fdl
  End Function

  Public Function GetEmployeeDataFromFinplusAsDictionary() As Dictionary(Of Integer, FinanceData)
    Dim fdl As List(Of FinanceData) = GetCachedEmployeeDataFromFinplus()
    Dim dict As New Dictionary(Of Integer, FinanceData)
    For Each f In fdl
      dict(f.EmployeeId) = f
    Next
    Return dict
  End Function

  Public Function GetEmployeeDataFromFinPlus(DepartmentList As List(Of String)) As List(Of FinanceData)
    Dim CIP As New CacheItemPolicy With {
      .AbsoluteExpiration = Now.AddHours(12)
    }
    Dim key As String = "employeedata" ' & PayPeriodStart.ToShortDateString
    Dim fdl As List(Of FinanceData) = myCache.GetItem(key, CIP)
    Return (From f In fdl
            Where DepartmentList.Contains(f.Department)
            Select f).ToList
  End Function

  Public Function GetEmployeeDataFromFinPlus(EmployeeID As Integer) As List(Of FinanceData)
    Dim CIP As New CacheItemPolicy With {
      .AbsoluteExpiration = Now.AddHours(12)
    }
    Dim key As String = "employeedata" ' & PayPeriodStart.ToShortDateString
    Dim fdl As List(Of FinanceData) = myCache.GetItem(key, CIP)
    Return (From f In fdl
            Where f.EmployeeId = EmployeeID
            Select f).ToList
  End Function

  Public Function GetEmployeeDataFromFinPlus(DepartmentList As List(Of String), EmployeeID As Integer) As List(Of FinanceData)
    Dim CIP As New CacheItemPolicy With {
      .AbsoluteExpiration = Now.AddHours(12)
    }
    Dim key As String = "employeedata" '& PayPeriodStart.ToShortDateString
    Dim fdl As List(Of FinanceData) = myCache.GetItem(key, CIP)
    Return (From f In fdl
            Where DepartmentList.Contains(f.Department) And
              f.EmployeeId = EmployeeID
            Select f).ToList
  End Function

  Public Function GetAllEmployeeDataFromFinPlus() As List(Of FinanceData)
    ' This pulls the employee data from Pentamation for the list of departments 
    'Dim sbQuery As New StringBuilder, 
    Dim dbc As New Tools.DB(GetCS(ConnectionStringType.FinPlus), toolsAppId, toolsDBError)
    Dim query As String = "
      USE finplus51;
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
      ORDER BY E.l_name ASC, E.f_name ASC"
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
      .AppendLine("USE finplus51;")
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
    Dim tctdl As List(Of TimecardTimeData) =
      GetEmployeeDataFromTimeStore(Startdate, Startdate.AddDays(13), EmployeeID)
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

  Public Function GetEmployeeDataFromTimeStore(StartDate As Date, EndDate As Date, Optional EmployeeID As Integer = 0) As List(Of TimecardTimeData)
    Dim STDL As List(Of Saved_TimeStore_Data)
    If EmployeeID = 0 Then
      STDL = Saved_TimeStore_Data.GetAllByDateRange(StartDate, EndDate)
    Else
      STDL = Saved_TimeStore_Data.GetByEmployeeAndDateRange(StartDate, EndDate, EmployeeID)
    End If
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
    Dim query As String = "
USE TimeStore; 
SELECT 
  employee_id 
FROM Access 
WHERE reports_to=" & employeeId
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
    Dim query As String = "
USE TimeStore; 
SELECT 
  employee_id 
FROM Access 
WHERE access_type >= " & accessType
    Try
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
    Dim key As String = "employeedata" ' & GetPayPeriodStart(Today).ToShortDateString
    Dim fdl As List(Of FinanceData) = myCache.GetItem(key)
    Return (From f In fdl
            Where f.TerminationDate = Date.MaxValue Or
              f.TerminationDate > Date.Parse("1/1/2015")
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

  Public Function Approve_Payperiod(ByRef Req As HttpRequestBase,
                                    EmployeeId As Integer,
                                    PayPeriodEnding As Date,
                                    ApprovalType As String) As Boolean

    Dim Machinename As String = Req.UserHostName
    Dim Username As String = Req.LogonUserIdentity.Name
    Dim IPAddress As String = Req.UserHostAddress
    Dim MyEID As Integer = AD_EmployeeData.GetEmployeeIDFromAD(Username)
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
    'Dim PublicWorks() As String = {} '{"3701", "3709", "3711", "3712"} ' for these two departments.

    Dim PayPeriodEnding As Date = PayPeriodStart.AddDays(13)
    Dim gtc As New List(Of GenericTimecard)
    Dim std As List(Of Saved_Timecard_Data) = Get_All_Saved_Timecard_Data(PayPeriodEnding)
    Dim teledata As List(Of TelestaffTimeData) = TelestaffTimeData.GetEmployeeDataFromTelestaff(PayPeriodStart)
    Dim tcdata As List(Of TimecardTimeData) = GetEmployeeDataFromTimecard(PayPeriodStart)
    Dim tsdata As List(Of TimecardTimeData) = GetEmployeeDataFromTimeStore(PayPeriodStart, PayPeriodEnding)
    Dim tsctedata As List(Of Saved_TimeStore_Comp_Time_Earned) = GetCompTimeEarnedDataFromTimeStore(PayPeriodStart)
    Dim notes As List(Of Note) = Get_All_Notes(PayPeriodEnding)

    ' Let's pare down the employee list to just those that are not terminated, 
    ' or those that were terminated in the pay period.
    Dim employeeList As List(Of FinanceData) = (From el In GetCachedEmployeeDataFromFinplus()
                                                Order By el.Department, el.EmployeeLastName
                                                Where el.TerminationDate > PayPeriodStart
                                                Select el).ToList
    ' And Not PublicWorks.Contains(el.Department)
    ' public works test And Not PublicWorks.Contains(el.Department)
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
    'Dim PublicWorks() As String = {"3701", "3709", "3711", "3712"} ' for these two departments.
    Dim employeeList As List(Of FinanceData) = (From el In GetCachedEmployeeDataFromFinplus()
                                                Order By el.Department, el.EmployeeLastName
                                                Where (el.TerminationDate = Date.MaxValue Or
                                                (el.TerminationDate > payPeriodStart And
                                                el.TerminationDate <= ppEnd))
                                                Select el).ToList
    'And Not PublicWorks.Contains(el.Department)
    ' public works test
    ' And Not PublicWorks.Contains(el.Department
    Dim dbc As New Tools.DB(GetCS(ConnectionStringType.Timestore), toolsAppId, toolsDBError)
    Dim query As String = $"
USE TimeStore; 
SELECT 
  ISNULL(T1.orgn, 'Total') AS orgn, 
  ISNULL(T1.employee_id, 0) AS employee_id,  
  SUM(reg) AS sumReg, 
  SUM([231]) AS [231], 
  SUM([090]) AS [090], 
  SUM([095]) AS [095], 
  SUM([110]) AS [110],
  SUM([046]) AS [046],
  SUM([100]) AS [100],
  SUM([120]) AS [120],
  SUM([121]) AS [121],
  SUM([230]) AS [230],
  SUM([232]) AS [232],
  SUM([111]) AS [111],
  SUM([123]) AS [123],
  SUM([130]) AS [130],
  SUM([134]) AS [134],
  SUM([131]) AS [131],
  SUM([101]) AS [101],
  SUM([124]) AS [124],
  SUM([006]) AS [006],
  SUM([007]) AS [007],
  SUM([122]) AS [122],
  SUM([777]) AS [777],
  SUM([299]) AS [299],
  SUM([300]) AS [300],
  SUM([301]) AS [301],
  SUM([302]) AS [302],
  SUM([303]) AS [303],
  SUM(TotalHours) AS TotalHours 
FROM (
  SELECT 
    orgn,
    employee_id,
    pay_period_ending,
    [002] as reg,
    [046],
    [090],
    [095],
    [100],
    [101],
    [110],
    [111],
    [121],
    [123],
    [230],
    [231],
    [232],
    [130],
    [131],
    [134],
    [120],
    [122],
    [124],
    [006],
    [007],
    [777],
    [299],
    [300],
    [301],
    [302],
    [303],
    TotalHours 
  FROM (
    SELECT 
      S.employee_id,
      S.paycode,
      S.hours,
      S.orgn,
      S.pay_period_ending,
      S2.TotalHours 
    FROM Saved_Time S 
    LEFT OUTER JOIN CLAYBCCFINDB.finplus51.dbo.person P ON S.employee_id = P.empl_no
    INNER JOIN (
      SELECT 
        employee_id, 
        SUM(hours) AS TotalHours 
      FROM Saved_Time 
      WHERE pay_period_ending='{ppEnd.ToShortDateString}'
      GROUP BY employee_id
    ) AS S2 ON S2.employee_id = S.employee_id 
    WHERE 
      1 = 1
      AND (P.term_date > DATEADD(DAY, -13, CAST('{ppEnd.ToShortDateString}' AS DATE)) 
      OR P.term_date IS NULL)
      AND S.pay_period_ending='{ppEnd.ToShortDateString}'
  ) AS ST 
PIVOT (	SUM(hours) 
  FOR paycode IN ([002], [046], [090], [095], [100], [101], [110],[111], [121], [123], [230], [231], [232],  
      		[130], [131], [134], [120], [122], [124], [006], [007], [777], [299], [300], [301], [302], [303]) 
  ) AS PivotTable) AS T1 
GROUP BY ROLLUP (T1.orgn, T1.employee_id);"
    'Dim sbQ As New StringBuilder
    'With sbQ
    '  .AppendLine("USE TimeStore; SELECT ISNULL(T1.orgn, 'Total') AS orgn, ISNULL(T1.employee_id, 0) AS employee_id,  ")
    '  .AppendLine("SUM(reg) AS sumReg, SUM([231]) AS [231], SUM([090]) AS [090], SUM([095]) AS [095], SUM([110]) AS [110],SUM([046]) AS [046], ")
    '  .AppendLine("SUM([100]) AS [100], SUM([120]) AS [120],SUM([121]) AS [121],SUM([230]) AS [230], SUM([232]) AS [232], ")
    '  .AppendLine("SUM([111]) AS [111], SUM([123]) AS [123], SUM([130]) AS [130], SUM([134]) AS [134], ")
    '  .AppendLine("SUM([131]) AS [131], SUM([101]) AS [101],  ")
    '  .AppendLine("SUM([124]) AS [124], SUM([006]) AS [006], SUM([007]) AS [007], SUM([122]) AS [122], SUM([777]) AS [777], ")
    '  .AppendLine("SUM([299]) AS [299], SUM([300]) AS [300],  SUM([301]) AS [301], SUM([302]) AS [302], SUM([303]) AS [303], SUM(TotalHours) AS TotalHours ")
    '  .AppendLine("FROM (SELECT orgn, employee_id, pay_period_ending, [002] as reg, [046], [090], [095], [100], [101],  ")
    '  .AppendLine("[110],[111], [121], [123], [230], [231], [232], [130], [131], [134], [120], [122], [124], [006], [007], [777], [299], [300], [301], [302], [303], TotalHours ")
    '  .AppendLine("FROM (SELECT S.employee_id, S.paycode, S.hours, S.orgn, S.pay_period_ending, S2.TotalHours FROM Saved_Time S ")
    '  .AppendLine("INNER JOIN (SELECT employee_id, SUM(hours) AS TotalHours FROM Saved_Time ")
    '  .Append("	WHERE pay_period_ending='").Append(ppEnd.ToShortDateString)
    '  .Append("' GROUP BY employee_id) AS S2 ON S2.employee_id = S.employee_id ")
    '  .Append("WHERE S.pay_period_ending='").Append(ppEnd.ToShortDateString)
    '  .AppendLine("') AS ST ")
    '  .AppendLine("PIVOT (	SUM(hours) ")
    '  .AppendLine("	FOR paycode IN ([002], [046], [090], [095], [100], [101], [110],[111], [121], [123], [230], [231], [232],  ")
    '  .AppendLine("					[130], [131], [134], [120], [122], [124], [006], [007], [777], [299], [300], [301], [302], [303]) ")
    '  .AppendLine("	) AS PivotTable) AS T1 ")
    '  .AppendLine("GROUP BY ROLLUP (T1.orgn, T1.employee_id);")
    '  '.AppendLine("ORDER BY CASE WHEN t1.orgn IS NULL THEN 1 ELSE 0 END, t1.orgn ASC, ")
    '  '.AppendLine("CASE WHEN t1.employee_id IS NULL THEN 1 ELSE 0 END, t1.employee_id ASC; ")
    'End With
    Dim ds As DataSet = dbc.Get_Dataset(query)
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

  Public Function InsertSavedTimeToFinplus(tsds As DataSet, cst As ConnectionStringType) As Boolean
    Dim sbQ As New StringBuilder
    'Dim dbf As New Tools.DB(GetCS(ConnectionStringType.FinPlus), toolsAppId, toolsDBError)
    'Dim cs As ConnectionStringType
    'If UseProduction Then
    '  cs = ConnectionStringType.FinPlus
    'Else
    '  cs = ConnectionStringType.FinplusTraining
    'End If
    Dim dbf As New Tools.DB(GetCS(cst), toolsAppId, toolsDBError)
    ' We're going to grab all of the 002 data from Finplus's timecard table 
    ' and all of Timestore's SavedTime table
    ' loop through the finplus data to find matches. 
    ' If the paycode and pay rate match, we update the hours
    ' then we loop through the saved_time data and insert everything else.
    ' If the paycode and payrate exists in the Finplus Timecard data, we update.  Otherwise we insert.
    Dim employeeData As List(Of FinanceData) = GetCachedEmployeeDataFromFinplus()
    Dim fds As DataSet = GetRawFinplusTimecardData(cst)
    If fds.Tables(0).Rows.Count > 0 Then
      Dim payrun As String = (From f In fds.Tables(0).AsEnumerable Where Not IsDBNull(f("pay_run")) Select f("pay_run")).FirstOrDefault
      With sbQ
        '.AppendLine("USE TimeStore;")
        Select Case cst
          Case ConnectionStringType.FinPlus
            .AppendLine("USE finplus51;")
          Case ConnectionStringType.FinplusTraining
            .AppendLine("USE trnfinplus51;")
          Case Else
            .AppendLine("USE finplus51;")
        End Select
        For Each ts In tsds.Tables(0).Rows
          Dim eid As Integer = ts("employee_id")
          Dim employee As FinanceData = (From e In employeeData
                                         Where e.EmployeeId = eid
                                         Select e).First

          Dim paycode As String = ts("paycode")
          Dim payrate As Decimal = ts("payrate")
          Dim rndPayrate As Decimal = Math.Round(payrate, 5)
          Dim hours As Double = ts("hours")
          Dim tmp = (From f In fds.Tables(0).AsEnumerable
                     Where f("empl_no") = eid And
                       f("pay_code") = paycode And
                       Math.Round(f("payrate"), 5) = rndPayrate
                     Select f)
          If Not employee.IsTerminated Then
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
          End If
        Next
        ' at the end here, we need to add a row updating the project code for
        ' any disaster hours.
        ' HI-09/17 Irma project code
        '.AppendLine("UPDATE timecard SET proj='HI-09/17' WHERE pay_code IN ('299', '300', '301', '302', '303');")
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
      Case "231", "131", "302"
        newRate = payrate * 1.5
      Case "232", "303"
        newRate = payrate * 2
      Case Else
        newRate = payrate
    End Select
    Return Math.Round(newRate, 5)
  End Function

  Public Function UpdateFinplusWithSavedTime(tsds As DataSet, cst As ConnectionStringType) As Boolean

    Dim fds As DataSet = GetRawFinplusTimecardData(cst)
    'Dim cs As ConnectionStringType
    'If UseProduction Then
    '  cs = ConnectionStringType.FinPlus
    'Else
    '  cs = ConnectionStringType.FinplusTraining
    'End If
    Dim dbf As New Tools.DB(GetCS(cst), toolsAppId, toolsDBError)
    Dim sbQ As New StringBuilder
    Dim nomatch As Integer = 0, alreadymatch As Integer = 0
    With sbQ
      '.AppendLine("USE TimeStore;")
      Select Case cst
        Case ConnectionStringType.FinPlus
          .AppendLine("USE finplus51;")

        Case ConnectionStringType.FinplusTraining
          .AppendLine("USE trnfinplus51;")
        Case Else
          .AppendLine("USE finplus51;")
      End Select
      'If UseProduction Then
      '  .AppendLine("USE finplus50;")
      'Else
      '  .AppendLine("USE trnfinplus50;")
      'End If
      ' Here we're looping through each employee in pentamation.
      ' this helps us reconcile because  if we don't have any hours for them
      ' in timestore, we can still do something with that information.
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
        If tmp.Count = 0 Then ' No rows found for this employee at this payrate.

          ' Here, if the user has no hours in Timestore for this employee at the payrate we found in pentamation
          ' We make a couple of choices.


          ' Now before, we were matching the employee and the payrate to the information we have in timestore.
          ' With this IF statement, we are asking "Ok, so this employee doesn't have any hours at that payrate,
          ' do they have any hours at all at any payrate?" 
          If (From t In tsds.Tables(0).AsEnumerable Where t("employee_id") = eid Select t).Count > 0 Then
            ' If we end up here, they do have hours in Timestore at a payrate other than what we expect in 
            ' Pentamation.  This happens for Public Safety employees regularly because Telestaff is kept
            ' up to date more than pentamation is, as far as payrates go.
            ' So if we do have data for them, we get rid of the hours found in the pentamation timecard at the 
            ' different payrate.  We will insert the hours we do have at the payrate we have in a later step.

            '.Append("UPDATE timecard SET hours=0, amount=0 WHERE empl_no='")
            '.Append(eid).Append("' AND pay_code='").Append(paycode)
            '.Append("' AND payrate=").Append(payrate).AppendLine(";")
            .Append("DELETE FROM timecard WHERE empl_no='")
            .Append(eid).Append("' AND pay_code='").Append(paycode)
            .Append("' AND payrate=").Append(rndPayrate).AppendLine(";")
          Else
            ' So this person doesn't have any time at all in timestore, for this pay period.
            ' As of today 5/14/2018, this process will leave the time in pentamation alone, so their default hours will remain.

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

  Public Function GetRawFinplusTimecardData(cst As ConnectionStringType) As DataSet
    'Dim cs As ConnectionStringType
    'If UseProduction Then
    '  cs = ConnectionStringType.FinPlus
    'Else
    '  cs = ConnectionStringType.FinplusTraining
    'End If
    Dim dbf As New Tools.DB(GetCS(cst), toolsAppId, toolsDBError)
    Dim financeQuery As New StringBuilder
    With financeQuery
      Select Case cst
        Case ConnectionStringType.FinPlus
          .AppendLine("USE finplus51;")
        Case ConnectionStringType.FinplusTraining
          .AppendLine("USE trnfinplus51;")
        Case Else
          .AppendLine("USE finplus51;")
      End Select
      .AppendLine("SELECT TC.empl_no,pay_code,hours,payrate,amount,orgn,account,proj,pacct,classify,pay_cycle,tax_ind, ")
      .AppendLine("	pay_run,subtrack_id,reported,user_chg,date_chg,flsa_cycle,flsa_flg,flsa_carry_ovr,ret_pers_code ")
      .AppendLine("FROM timecard TC ")
      .AppendLine("INNER JOIN employee E ON TC.empl_no=E.empl_no ")
      .AppendLine("WHERE TC.pay_code IN ('002', '090', '007', '100', '101', '110', '111', '120', '121', '122',  ")
      .AppendLine("					'123', '124', '130', '131', '134', '230', '231', '232') ")
      ' public works test
      ' removed 11/13/2018
      '.AppendLine("AND LTRIM(RTRIM(E.home_orgn)) NOT IN ('3701', '3709', '3711', '3712') ")
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

  Public Function GetRawTimestoreSavedTimeDataForDisaster(payPeriodEnding As Date) As DataSet
    Dim dbts As New Tools.DB(GetCS(ConnectionStringType.Timestore), toolsAppId, toolsDBError)
    Dim f As List(Of FinanceData) = GetAllEmployeeDataFromFinPlus()
    '    Dim query As String = "
    'DECLARE @pay_period_ending DATE = '9/3/2019';

    'WITH TotalHours AS (
    '  SELECT 
    '    employee_id
    '    ,SUM(hours) total_hours
    '  FROM Saved_Time
    '  WHERE orgn NOT IN ('1703', '2103')
    '  AND paycode NOT IN ('230', '231', '232', '299', '300', '301', '302', '303', '046', '120')
    '  AND pay_period_ending = @pay_period_ending
    '  GROUP BY employee_id
    ')
    'SELECT
    '  S.employee_id
    '  ,S.paycode
    '  ,S.payrate
    '  ,CASE WHEN paycode= '002' AND T.total_hours < 80 
    '    THEN 80 - (T.total_hours - S.hours)
    '    ELSE S.hours
    '    END hours
    '  ,S.amount
    '  ,S.orgn
    '  ,S.classify
    'FROM Saved_Time S
    'LEFT OUTER JOIN TotalHours T ON S.employee_id = T.employee_id
    'WHERE 
    '  pay_period_ending= @pay_period_ending
    '  AND paycode <> 800
    '  AND ((orgn NOT IN ('1703', '2103')
    '  AND paycode NOT IN ('230', '231', '232', '299', '300', '301', '302', '303', '046', '120'))
    '  OR orgn IN ('1703', '2103'))
    'ORDER BY S.orgn ASC, S.employee_id ASC"

    Dim query As String = "
SELECT
  empl_no employee_id
  ,paycode
  ,payrate
  ,[Hours To Be Added] hours
  ,0 amount
  ,home_orgn orgn
  ,classify
FROM vw_CombinedPayroll
ORDER BY home_orgn, empl_no

"
    'Dim TimestoreQuery As New StringBuilder
    'With TimestoreQuery
    '  .AppendLine("USE TimeStore;")
    '  ' -----------------
    '  ' original version
    '  ' -----------------

    '  '.AppendLine("SELECT employee_id,paycode,payrate,hours,amount,orgn,classify ")
    '  ' -----------------
    '  ' disaster calculations version
    '  ' -----------------
    '  .AppendLine("SELECT employee_id,")
    '  .AppendLine("CASE WHEN paycode = '777' THEN '002' ELSE paycode END AS paycode,")
    '  .AppendLine("CASE WHEN paycode = '777' THEN CAST(payrate * 1.5 AS DECIMAL(10, 5)) ELSE payrate END AS payrate, ")
    '  .AppendLine("hours,amount,orgn,classify ")
    '  .AppendLine("FROM Saved_Time")
    '  .AppendLine("WHERE pay_period_ending = @PayPeriodEnding ")

    '  ' paycode 800 is a paycode we use only in Timestore to determine if
    '  ' a firefighter / dispatch employee marked any holiday hours as 
    '  ' ineligible.  This paycode is invalid to finplus so we're not
    '  ' going to send it to them.
    '  ' "777"
    '  .AppendLine("AND paycode <> 800 ")
    '  .AppendLine("AND ((orgn NOT IN ('1703', '2103')")
    '  .AppendLine("AND paycode NOT IN ('002', '230', '231', '232', '299', '300', '301', '302', '303', '046', '120'))")
    '  .AppendLine("OR orgn IN ('1703', '2103'))")
    '  .AppendLine("ORDER BY orgn ASC, employee_id ASC")
    'End With
    Dim P(0) As SqlParameter
    P(0) = New SqlParameter("@PayPeriodEnding", Data.SqlDbType.Date) With {.Value = payPeriodEnding}
    Dim ds As DataSet
    Try
      ds = dbts.Get_Dataset(query, P)
      For Each d In ds.Tables(0).Rows
        'd("payrate") = GetPayrate(d("paycode"), d("payrate"))
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

  Public Function SavedTimeToFinplusProcess(payPeriodEnding As Date, cst As ConnectionStringType) As Boolean
    Dim tc As List(Of GenericTimecard) = GetTimeCards(payPeriodEnding.AddDays(-13), True)
    Dim tsds As DataSet = GetRawTimestoreSavedTimeData(payPeriodEnding)
    'If Not UpdateFinplusWithSavedTime(tsds, UseProduction) Then Return False
    UpdateFinplusWithSavedTime(tsds, cst)
    Return InsertSavedTimeToFinplus(tsds, cst)
  End Function

  Public Function SpecialDisasterSavedTimeToFinplusProcess(payPeriodEnding As Date, cst As ConnectionStringType) As Boolean
    Dim tc As List(Of GenericTimecard) = GetTimeCards(payPeriodEnding.AddDays(-13), True)
    Dim tsds As DataSet = GetRawTimestoreSavedTimeDataForDisaster(payPeriodEnding)
    'If Not UpdateFinplusWithSavedTime(tsds, UseProduction) Then Return False
    UpdateFinplusWithSavedTime(tsds, cst)
    Return InsertSavedTimeToFinplus(tsds, cst)
  End Function

  Public Function Save_Hours(EmployeeID As Integer,
                             PayPeriodEnding As Date,
                             PayCode As String,
                             Hours As Double,
                             Payrate As Double,
                             Department As String,
                             Classify As String) As Boolean

    'Dim deleted As Boolean = Delete_Hours(EmployeeID, PayPeriodEnding, PayCode, Payrate)
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

        DELETE FROM Saved_Time 
        WHERE 
          employee_id=@EmployeeId AND
          pay_period_ending = @PayPeriodEnding AND 
          paycode = @PayCode AND 
          Payrate = @PayRate;

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
    Dim fdl As List(Of FinanceData) = (From fd In GetAllEmployeeDataFromFinPlus()
                                       Where (fd.TerminationDate = Date.MaxValue Or
                                         (fd.TerminationDate >= StartDate And
                                         fd.TerminationDate <= EndDate))
                                       Select fd).ToList
    Dim teledl As List(Of TelestaffTimeData) = TelestaffTimeData.GetEmployeeDataFromTelestaff(StartDate, "", EndDate)
    Dim tcl As List(Of TimecardTimeData) = GetEmployeeDataFromTimecard(StartDate, 0, EndDate)
    Dim tsl As List(Of TimecardTimeData) = GetEmployeeDataFromTimeStore(StartDate, EndDate)

    Dim gtdl As New List(Of GenericTimeData)
    For Each f In fdl
      Select Case f.Department
        Case "1703", "2103" ' public safety ' removed 2102, they are apart of timestore now.
          For Each t In (From tele In teledl
                         Where tele.EmployeeId = f.EmployeeId
                         Select tele).ToList
            gtdl.Add(New GenericTimeData(t, f))
          Next
          ' public works test
          ' Removed 11/13/2018
          'Case "3701", "3709", "3711", "3712" ' public works


        Case Else ' timecard / timestore
          ' Here we're going to look in the timecard system first.  If they have any
          ' data there, we're going to use that and ignore the timestore data.
          ' Only if they don't have any timecard data do we even look for the timestore data.
          Dim tc = (From timecard In tcl
                    Where timecard.EmployeeID = f.EmployeeId
                    Select timecard).ToList
          If tc.Count = 0 Then
            tc = (From timestore In tsl
                  Where timestore.EmployeeID = f.EmployeeId
                  Select timestore).ToList
          End If
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

  Public Function Finalize_Leave_Request(Approved As Boolean,
                                         ApprovalHoursID As Long,
                                         Hours As Double,
                                         Note As String,
                                         SavingEmployee As Timecard_Access) As Integer

    Dim dp As New DynamicParameters()
    dp.Add("@ApprovalHoursId", ApprovalHoursID)
    dp.Add("@IsApproved", If(Approved, 1, 0))
    dp.Add("@ByEmployeeID", SavingEmployee.EmployeeID)
    dp.Add("@ByUsername", SavingEmployee.UserName)
    dp.Add("@ByMachinename", SavingEmployee.MachineName)
    dp.Add("@ByIpAddress", SavingEmployee.IPAddress)
    dp.Add("@Note", Note)
    Dim query As String = "
      UPDATE Hours_To_Approve
      SET 
        by_employeeid = @ByEmployeeID,
        by_username = @ByUsername,
        by_ip_address = @ByIpAddress,
        by_machinename = @ByMachinename,
        date_approval_added = GETDATE(),
        is_approved=@IsApproved,
        note = @Note
      WHERE 
        approval_hours_id=@ApprovalHoursId;

      IF(@IsApproved = 0) 
      BEGIN
        DECLARE @work_hours_id BIGINT = (
          SELECT 
            work_hours_id 
          FROM Hours_To_Approve
          WHERE approval_hours_id = @ApprovalHoursId
          );

        DECLARE @TotalHours FLOAT = dbo.GetTotalHours(@work_hours_id);

        UPDATE Work_Hours
        SET total_hours = @TotalHours
        WHERE work_hours_id=@work_hours_id;
      END"
    Try
      Return Exec_Query(query, dp, ConnectionStringType.Timestore)
    Catch ex As Exception
      Dim e As New ErrorLog(ex, query)
      Return -1
    End Try
  End Function

  Public Function Get_Leave_Request_EmployeeIds(Ids As List(Of Long)) As List(Of Integer)
    Dim dp As New DynamicParameters
    dp.Add("@Ids", Ids)
    Dim query As String = "
      SELECT DISTINCT
        W.employee_id
      FROM Hours_To_Approve H
      INNER JOIN Work_Hours W ON W.work_hours_id = H.work_hours_id
      INNER JOIN Timestore_Fields F ON H.field_id = F.field_id AND F.requires_approval = 1
      WHERE H.approval_hours_id IN @Ids
      AND H.by_employeeid IS NULL
      AND H.hours_used > 0
      AND is_approved = 1
      AND DATEADD(HOUR, 10, DATEADD(dd, 1, CAST(W.pay_period_ending AS DATETIME))) > GETDATE()
      " ' this is how we handle the pay period cutoff.
    ' any leave requests after the pay period ends will be ignored.
    Return Get_Data(Of Integer)(query, dp, ConnectionStringType.Timestore)
  End Function


  Public Function Bulk_Approve_Leave_Requests(Ids As List(Of Long),
                                              SavingEmployee As Timecard_Access) As Integer

    Dim dp As New DynamicParameters()
    dp.Add("@ApprovalHoursIds", Ids)
    dp.Add("@ByEmployeeID", SavingEmployee.EmployeeID)
    dp.Add("@ByUsername", SavingEmployee.UserName)
    dp.Add("@ByMachinename", SavingEmployee.MachineName)
    dp.Add("@ByIpAddress", SavingEmployee.IPAddress)
    ' Disaster Change
    Dim query As String = "
      UPDATE H
      SET 
        by_employeeid = @ByEmployeeID,
        by_username = @ByUsername,
        by_ip_address = @ByIpAddress,
        by_machinename = @ByMachinename,
        date_approval_added = GETDATE()
      FROM Hours_To_Approve H
      INNER JOIN Work_Hours W ON W.work_hours_id = H.work_hours_id
      INNER JOIN Timestore_Fields F ON H.field_id = F.field_id 
        AND F.requires_approval = 1
      WHERE 
        H.approval_hours_id IN @ApprovalHoursIds
        AND H.by_employeeid IS NULL
        AND H.hours_used > 0
        AND H.is_approved = 1
        AND DATEADD(HOUR, 10, DATEADD(dd, 1, CAST(W.pay_period_ending AS DATETIME))) > GETDATE()
        ;" ' 
    Try
      Return Exec_Query(query, dp, ConnectionStringType.Timestore)
    Catch ex As Exception
      Dim e As New ErrorLog(ex, query)
      Return -1
    End Try
  End Function

  Public Sub Clear_Saved_Timestore_Data(employeeID As Integer,
                                        PayPeriodStart As Date,
                                        Optional IgnoreHoliday As Boolean = False)
    Dim dp As New DynamicParameters
    dp.Add("@EmployeeId", employeeID)
    dp.Add("@PayPeriodEnding", PayPeriodStart.AddDays(13))
    Dim query As String = $"
      IF EXISTS(
        SELECT * FROM Saved_Time
        WHERE employee_id=@EmployeeId
        AND pay_period_ending=@PayPeriodEnding
        AND initial_approval_employeeid IS NOT NULL
      )
      BEGIN 
        INSERT INTO notes (employee_id, pay_period_ending, note) 
        VALUES (@EmployeeId, @PayPeriodEnding, 'Approval Removed, Hours or Payrate has changed.');
      END

        DELETE 
        FROM Saved_Time
        WHERE employee_id=@EmployeeId
        AND pay_period_ending=@PayPeriodEnding
        { If(IgnoreHoliday, "AND paycode NOT IN ('134', '124', '122', '800')", "") }

        UPDATE Saved_Time 
        SET 
          initial_approval_username=NULL, 
          initial_approval_employeeid=NULL,
          initial_approval_machine_name=NULL, 
          initial_approval_ip_address=NULL, 
          initial_approval_date=NULL, 
          final_approval_username=NULL, 
          final_approval_employeeid=NULL,
          final_approval_machine_name=NULL, 
          final_approval_ip_address=NULL, 
          final_approval_date=NULL 
        WHERE employee_id=@EmployeeId 
        AND pay_period_ending=@PayPeriodEnding;"
    Try
      Exec_Query(query, dp, ConnectionStringType.Timestore)
    Catch ex As Exception
      Dim e As New ErrorLog(ex, query)
    End Try
  End Sub

End Module

