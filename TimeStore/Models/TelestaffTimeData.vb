Imports System.Data.SqlClient

Namespace Models

  Public Class TelestaffTimeData
    Public Property EmployeeId As Integer ' The employeeID from Finplus, seems to be standardized across all databases.
    Public Property WorkDate As Date ' The date this work was performed
    Public Property StartTime As Date ' the time this particular chunk of time started
    Public Property EndTime As Date ' the time this chunk ended.
    Public Property DisasterRule As Integer = -1
    Public Property WorkHours As Double ' The number of hours worked on this date
    Public Property PayRate As Double ' The hourly rate paid for these hours
    Public Property WorkCode As String ' The workcode that defines the kind of work this is, ie: straight pay, OT, vacation, holiday, etc.
    Public Property Comment As String = ""
    Public Property Job As String = ""
    Public Property FLSAHoursRequirement As Double ' This field will contain the number of hours needed by the Job above to qualify for OT.
    Public Property WorkType As String = ""
    Public Property WorkTypeAbrv As String = ""
    Public Property ConstantShift As Boolean = False
    Public Property ShiftType As String = ""
    Public Property ShiftDuration As Double = 0 ' The number of hours for this type of employee.  either 80 for dispatch/office or 106 (field)
    Public Property ProfileType As TelestaffProfileType ' dispatch / field / office
    Public Property StratName As String = ""
    Public Property ProfileID As Integer ' the specific profile ID in telestaff.
    'Property ProfileDesc As String ' Their profile's title string.
    Public Property RequiresApproval As Boolean = False ' Indicates if the hours have been approved or not.
    Public Property IsPaidTime As Boolean = False
    Public Property IsWorkingTime As Boolean = False
    Public Property CountsTowardsOvertime As Boolean = False ' This is whether or not the hours count towards your total hours for overtime.
    Public Property Specialties As String = ""
    Public Property ProfileSpecialties As String = "" ' If this is different from the Specialties, it means that the staffing entry was created before their specialties changed, and will need to be fixed.
    Public Property ProfileStartDate As Date
    Public Property ProfileEndDate As Date
    Public Property Staffing_Detail As String = ""
    Public Property Finplus_Project_Code As String = ""

    Public Function Clone() As TelestaffTimeData
      Dim x As New TelestaffTimeData
      x.Comment = Comment
      x.ProfileID = ProfileID
      'x.ProfileDesc = ProfileDesc
      x.RequiresApproval = RequiresApproval
      'x.ShiftDuration = ShiftDuration
      x.ConstantShift = ConstantShift
      x.ShiftType = ShiftType
      x.ProfileType = ProfileType
      x.WorkTypeAbrv = WorkTypeAbrv
      x.WorkType = WorkType
      x.WorkHours = WorkHours
      x.WorkDate = WorkDate
      x.WorkCode = WorkCode
      x.StartTime = StartTime
      x.StratName = StratName
      x.PayRate = PayRate
      x.Specialties = Specialties
      x.ProfileSpecialties = ProfileSpecialties
      x.Job = Job
      x.FLSAHoursRequirement = FLSAHoursRequirement
      x.EndTime = EndTime
      x.EmployeeId = EmployeeId
      x.IsPaidTime = IsPaidTime
      x.IsWorkingTime = IsWorkingTime
      x.CountsTowardsOvertime = CountsTowardsOvertime
      x.ProfileStartDate = ProfileStartDate
      x.ProfileEndDate = ProfileEndDate
      x.Staffing_Detail = Staffing_Detail
      x.Finplus_Project_Code = Finplus_Project_Code
      Return x
    End Function


    Public Shared Function GetEmployeeDataFromTelestaff(StartDate As Date, Optional ByVal EmployeeID As String = "", Optional ByVal EndDate? As Date = Nothing) As List(Of TelestaffTimeData)
      Dim I As List(Of Incentive) = myCache.GetItem("incentive")
      Dim dbc As New Tools.DB(GetCS(ConnectionStringType.Telestaff), toolsAppId, toolsDBError)
      Dim P(2) As SqlParameter
      P(0) = New SqlParameter("@Start", Data.SqlDbType.Date) With {.Value = StartDate}
      P(1) = New SqlParameter("@End", Data.SqlDbType.Date) With {.Value = IIf(EndDate Is Nothing, StartDate.AddDays(13), EndDate)}
      P(2) = New SqlParameter("@EmployeeID", Data.SqlDbType.VarChar) With {.Value = EmployeeID}

      Dim TimestoreServer As String = ""
      Dim testCS As Boolean = GetCS(ConnectionStringType.Timestore).ToUpper().Contains("CLAYBCCDV10")
      If testCS Then
        TimestoreServer = "CLAYBCCDV10."
      End If

      Dim query As String = $"
USE WorkForceTelestaff;

WITH BaseSpecialtyData AS (
SELECT
  RMT.RscMaster_No_In
  ,R.Rsc_From_Da
  ,R.Rsc_Thru_Da
  ,R.rsc_no_in
  ,STUFF((SELECT
            '*/' + S.Spec_Abrv_Ch
          FROM
            WorkForceTelestaff.dbo.Rsc_Splty_X_Tbl SL
            INNER JOIN WorkForceTelestaff.dbo.Specialty_Tbl S ON SL.spec_no_in = S.spec_no_in
          WHERE
           1 = 1
           AND R.rsc_no_in = SL.rsc_no_in
          ORDER  BY
           S.Spec_Abrv_Ch
          FOR XML PATH (''), TYPE).value('(./text())[1]', 'nvarchar(max)')
         ,1
         ,2
         ,N'') Specialties
FROM
  Resource_Tbl R
  INNER JOIN Resource_Master_Tbl RMT ON R.RscMaster_No_In = RMT.RscMaster_No_In
GROUP  BY
  RMT.RscMaster_No_In
  ,R.Rsc_No_In
  ,R.Rsc_From_Da
  ,R.Rsc_Thru_Da 
)

SELECT
  RMT.RscMaster_Name_Ch
  ,RMT.RscMaster_EmployeeID_Ch
  ,ST.rsc_no_in
  ,ST.staffing_calendar_da
  ,ST.staffing_start_dt
  ,ST.staffing_end_dt
  ,ISNULL(ST.Staffing_Detail_Ch, '') staffing_detail
  ,( CAST(DATEDIFF(minute
                   ,ST.Staffing_Start_Dt
                   ,ST.Staffing_End_Dt) AS DECIMAL(10, 2)) / 60 ) Staffing_Hours
  ,CASE
     WHEN ST.staffing_request_state = 1
     THEN ISNULL(ST.Staffing_Note_VC, '')
          + ' *** Unapproved in Telestaff'
     ELSE ISNULL(ST.Staffing_Note_VC
                 ,'')
   END Comment
  ,CASE
     WHEN ST.staffing_request_state = 1
     THEN 'Y'
     ELSE 'N'
   END RequiresApproval
  ,CASE
     WHEN LTRIM(RTRIM(W.Wstat_Abrv_Ch)) = ''
     THEN 'Straight'
     ELSE UPPER(LTRIM(RTRIM(W.Wstat_Abrv_Ch)))
   END WstatAbrv
  ,W.Wstat_Name_Ch
  ,W.Wstat_Payroll_ch
  ,W.Wstat_FLSA_Si
  ,WT.wstat_type_desc_ch
  ,W.Wstat_WageFactor_In
  ,J.Job_Abrv_Ch
  ,ISNULL(PAY.PayInfo_FlsaHours_In, 0) PayInfo_FlsaHours_In
  ,SH.shift_abrv_ch
  ,SH.Shift_TimeDuration_Ch
  ,SH.shift_type_no_in
  ,R.payinfo_no_in
  ,ISNULL(BSDR.Specialties,'') Specialties
  ,ISNULL(SPEC.Specialties
          ,'') ProfileSpecialties
  ,WAGE.rscmaster_wage_db Rsc_Hourwage_db
  ,'' StratName
  ,SH.shift_no_in
  ,R.region_no_in
  ,R.Rsc_From_Da
  ,R.Rsc_Thru_Da
  ,CASE WHEN TFWC.wstat_abrv_ch IS NOT NULL 
    THEN 1
    ELSE
      CASE WHEN WT.Wstat_Type_desc_ch != 'NON WORKING' 
      THEN 1
      ELSE 0
      END
    END IsWorkingTime    
  
  --,CASE
  --   WHEN FIX.region_no_in IS NULL
  --   THEN 0
  --   ELSE 1
  -- END is_fixed
FROM
  Staffing_Tbl ST
  LEFT OUTER JOIN Resource_Tbl R ON ST.rsc_no_in = R.rsc_no_in
  LEFT OUTER JOIN Resource_Master_Tbl RMT ON R.RscMaster_No_In = RMT.RscMaster_No_In
  LEFT OUTER JOIN wstat_cde_tbl W ON ST.wstat_no_in = W.wstat_no_in
  LEFT OUTER JOIN wstat_type_tbl WT ON W.wstat_type_no_in = WT.wstat_type_no_in
  LEFT OUTER JOIN Position_Tbl P ON ST.pos_no_in = P.pos_no_in
  LEFT OUTER JOIN Shift_Tbl SH ON ST.shift_no_in = SH.shift_no_in
  LEFT OUTER JOIN Job_Title_Tbl J ON J.Job_No_In = R.Job_No_In
  LEFT OUTER JOIN Pay_Information_Tbl PAY ON PAY.PayInfo_No_In = R.PayInfo_No_In
  LEFT OUTER JOIN BaseSpecialtyData SPEC ON R.rsc_no_in = SPEC.rsc_no_in
  LEFT OUTER JOIN BaseSpecialtyData BSDR ON RMT.RscMaster_No_In = BSDR.RscMaster_No_In
    AND ST.staffing_calendar_da >= BSDR.Rsc_From_Da
    AND (BSDR.Rsc_Thru_Da IS NULL OR ST.staffing_calendar_da <= BSDR.Rsc_Thru_Da)
  --LEFT OUTER JOIN Specialities SPEC ON R.rsc_no_in = SPEC.rsc_no_in
  LEFT OUTER JOIN resource_master_wage_tbl WAGE ON RMT.rscmaster_no_in = WAGE.rscmaster_no_in
                                                   AND CAST(WAGE.rscmaster_wage_effective_da AS DATE) <=
                                                       ST.staffing_calendar_da
                                                   AND ISNULL(WAGE.rscmaster_wage_expiration_da
                                                              ,ST.staffing_calendar_da) >= ST.staffing_calendar_da
  LEFT OUTER JOIN {TimestoreServer}Timestore.dbo.Telestaff_Forced_Working_Codes TFWC ON W.wstat_abrv_ch = TFWC.wstat_abrv_ch
  --LEFT OUTER JOIN stamp_log_tbl FIX ON ST.staffing_calendar_da = FIX.stamp_calendar_da
  --                                     AND FIX.region_no_in = R.region_no_in
  --                                     AND FIX.shift_no_in = ST.shift_no_in
WHERE
  ST.staffing_calendar_da BETWEEN @Start AND @End
  --AND RMT.RscMaster_Login_Disable_Si = 'N'
  AND W.Wstat_Abrv_Ch NOT IN ( 'OTR', 'OTRR', 'ORD', 'ORRD',
                               'NO', 'DPRN' )  
  AND RMT.RscMaster_EmployeeID_Ch = COALESCE(NULLIF(@EmployeeID, ''), RMT.RscMaster_EmployeeID_Ch)
  AND ST.staffing_request_state <> 20 -- 20 is the state for a denied leave request.
ORDER  BY
  RMT.RscMaster_Name_Ch
  ,ST.staffing_start_dt"
      'With sbQuery ' This humongous sql query was pulled from the telestaff report system and then tweaked.
      '  .AppendLine("USE Telestaff;")
      '  .Append("DECLARE @Start DATETIME = '").Append(StartDate.ToShortDateString).Append("';")
      '  If EndDate.HasValue Then
      '    .Append("DECLARE @End DATETIME = '").Append(EndDate.Value.ToShortDateString).Append("';")
      '  Else
      '    .AppendLine("DECLARE @End DATETIME = DATEADD(dd, 13, @Start);")
      '  End If

      '  '.AppendLine("SELECT ISNULL(DRP.rule_applied, 0) disaster_rule, ")
      '  .AppendLine("SELECT 0 disaster_rule, ")
      '  .AppendLine("R.payinfo_no_in, R.rsc_no_in, R.rsc_desc_ch, R.Rsc_Hourwage_db, R.Rsc_From_Da, R.Rsc_Thru_Da, AAA.* FROM (SELECT Resource_Master_Tbl.RscMaster_No_In, Staffing_Tbl.Staffing_Calendar_Da, ")
      '  .AppendLine("SUM(DATEDIFF(minute,Staffing_Tbl.Staffing_Start_Dt,Staffing_Tbl.Staffing_End_Dt))/60.00 as StaffingHours, Wstat_Cde_Tbl.Wstat_FLSA_Si, Wstat_Cde_Tbl.Wstat_WageFactor_In,")
      '  .AppendLine("Staffing_Tbl.Staffing_Benign_Si AS RequiresApproval, Resource_Master_Tbl.RscMaster_Name_Ch,Resource_Master_Tbl.RscMaster_EmployeeID_Ch, ")
      '  .AppendLine("Staffing_Tbl.Staffing_Start_Dt,Staffing_Tbl.Staffing_End_Dt,Job_Title_Tbl.Job_Abrv_Ch,Wstat_Type_Tbl.Wstat_Type_Desc_Ch,")
      '  .AppendLine("(CASE WHEN LTRIM(RTRIM(Wstat_Cde_Tbl.Wstat_Abrv_Ch)) = '' THEN 'Straight' ELSE UPPER(LTRIM(RTRIM(Wstat_Cde_Tbl.Wstat_Abrv_Ch))) END) AS WstatAbrv,")
      '  .AppendLine("Wstat_Cde_Tbl.Wstat_Name_Ch AS WstatName,Wstat_Payroll_ch,Pay_Information_Tbl.PayInfo_FlsaHours_In,")
      '  .AppendLine("Shift_tbl.shift_abrv_ch, shift_tbl.Shift_TimeDuration_Ch, shift_tbl.shift_type_no_in, ")
      '  .AppendLine("CASE WHEN Staffing_Tbl.Staffing_Benign_Si = 'Y' THEN ISNULL(Staffing_tbl.Staffing_Note_Vc, '') + ")
      '  .AppendLine("' *** Unapproved in Telestaff' ELSE ISNULL(Staffing_tbl.Staffing_Note_Vc, '') END AS Comment, ")
      '  .AppendLine("ISNULL(dbo.GetRscSpecialties(Resource_Tbl.Rsc_No_In,1), '') as Specialties, ISNULL(STRAT.strat_name_ch, '') AS StratName ")
      '  .AppendLine("FROM Staffing_Tbl ")
      '  .AppendLine("LEFT OUTER JOIN strategy_tbl STRAT ON Staffing_tbl.strat_no_in=STRAT.strat_no_in ")
      '  .AppendLine("JOIN Resource_Tbl ON Resource_Tbl.Rsc_No_In=Staffing_Tbl.Rsc_No_In ")
      '  .AppendLine("JOIN Wstat_Cde_Tbl ON Wstat_Cde_Tbl.Wstat_No_In=Staffing_Tbl.Wstat_No_In ")
      '  .AppendLine("JOIN Shift_Tbl ON Shift_Tbl.Shift_No_In=Staffing_Tbl.Shift_No_In ")
      '  .AppendLine("JOIN Wstat_Type_Tbl ON Wstat_Type_Tbl.Wstat_Type_No_In=Wstat_Cde_Tbl.Wstat_Type_No_In ")
      '  .AppendLine("JOIN Job_Title_Tbl ON Job_Title_Tbl.Job_No_In=Resource_Tbl.Job_No_In ")
      '  .AppendLine("LEFT OUTER JOIN Pay_Information_Tbl ON Pay_Information_Tbl.PayInfo_No_In=Resource_Tbl.PayInfo_No_In ")
      '  .AppendLine("JOIN Resource_Master_Tbl ON Resource_Master_Tbl.RscMaster_No_In=Resource_Tbl.RscMaster_No_In ")
      '  .AppendLine("JOIN Position_Tbl ON Position_Tbl.Pos_No_In=Staffing_Tbl.Pos_No_In ")
      '  .AppendLine("JOIN Unit_Tbl ON Unit_Tbl.Unit_No_In=Position_Tbl.Unit_No_In ")
      '  .AppendLine("JOIN Station_Tbl ON Station_Tbl.Station_No_In=Unit_Tbl.Station_No_In ")
      '  .AppendLine("WHERE Staffing_Tbl.Staffing_Calendar_Da BETWEEN @Start AND @End AND Station_Tbl.Region_No_In IN (4,2,5,6) ")
      '  ' Excluding the following work codes:
      '  ' OTR -  Overtime Reject, field personnel were offered OT by Telestaff and didn't take it.
      '  ' ORD - same as above but for Dispatch
      '  ' OTRR - reject for rapid hire.  Same as above
      '  ' DMWI - Dispatch shift trade, working.  This lets you know that the dispatcher traded shifts with someone and now they are working the shift in repayment.
      '  ' MWI - Same as above just for field personnel instead of dispatch
      '  ' SLOT - Sick leave on OT, person accepted OT but was sick.  Just used by Telestaff to fill the vacancy.
      '  ' BR - Break for staff employees, this is used to accurately calculate their schedules, rather than using the automatically calculated break.
      '  ' OJ - OJI on OT, these hours are not paid.
      '  ' ADMNSWAP - Admin leave on Swap time, so it's not paid time.
      '  ' OR - Off roster
      '  ' 8/1/2014, these were moved to the NonPaid array and are now manually excluded.
      '  ' The reason for the change is because we will need to see all of the time to correctly render a user's timesheet.
      '  '.AppendLine("AND Wstat_Cde_Tbl.Wstat_Abrv_Ch NOT IN ('ADMNSWAP', 'OTR', 'OTRR', 'DMWI', 'MWI', 'ORD', 'SLOT', 'BR', 'OJ', 'OR') ")
      '  ' 8/4/2014, we're going to keep the exclusions that are just used by telestaff to fill a spot on the roster.
      '  If EmployeeID.Length > 0 Then .Append("AND Resource_Master_Tbl.RscMaster_EmployeeID_Ch = '").Append(EmployeeID).AppendLine("'")

      '  'If Not EmployeeList Is Nothing Then
      '  '    .Append("AND Resource_Master_Tbl.RscMaster_EmployeeID_Ch IN (")
      '  '    For a As Integer = 0 To EmployeeList.Count - 1
      '  '        .Append("'").Append(EmployeeList(a)).Append("'")
      '  '        If a < EmployeeList.Count - 1 Then .Append(",")
      '  '    Next
      '  '    .AppendLine(") ")
      '  'End If
      '  If StartDate > CType("10/16/2018", Date) Then
      '    .Append("AND Resource_Master_Tbl.RscMaster_EmployeeID_Ch <> '2744' ") ' Jesse Hellard Exclusion while he works Animal Control and Public Safety until 8/4
      '  End If
      '  If StartDate < CType("8/11/2015", Date) Or StartDate > CType("8/25/2015", Date) Then .Append("AND Resource_Master_Tbl.RscMaster_EmployeeID_Ch <> '2201' ") ' Jesse Hellard Exclusion while he works Animal Control and Public Safety until 8/4
      '  .AppendLine("AND Wstat_Cde_Tbl.Wstat_Abrv_Ch NOT IN ('OTR', 'OTRR', 'ORD', 'ORRD', 'NO', 'DPRN') ")
      '  '.AppendLine("AND Resource_Tbl.rsc_no_in <> 1795 ")
      '  ' Old version, this was determined to be incorrect when we have an end date entered that was after our start/end date
      '  '.AppendLine("AND (Resource_Master_Tbl.RscMaster_Thru_Da IS NULL OR Resource_Master_Tbl.RscMaster_Thru_Da BETWEEN @Start AND @End) ")
      '  .AppendLine("AND (Resource_Master_Tbl.RscMaster_Thru_Da IS NULL OR Resource_Master_Tbl.RscMaster_Thru_Da >= @Start) ")
      '  .AppendLine("GROUP BY Staffing_Tbl.Staffing_Calendar_Da,Shift_Tbl.Shift_Type_No_In,Staffing_Tbl.Rsc_No_In,Resource_Master_Tbl.RscMaster_No_In,")
      '  .AppendLine("Resource_Master_Tbl.RscMaster_Name_Ch,Resource_Master_Tbl.RscMaster_EmployeeID_Ch,Resource_Master_Tbl.RscMaster_PayrollID_Ch,")
      '  .AppendLine("Resource_Master_Tbl.RscMaster_Contact1_Ch,Resource_Master_Tbl.RscMaster_Contact2_Ch,Resource_Tbl.Rsc_Job_Level_Ch,")
      '  .AppendLine("Resource_Tbl.Rsc_No_In,Job_Title_Tbl.Job_Abrv_Ch,Wstat_Type_Tbl.Wstat_Type_No_In,Wstat_Type_Tbl.Wstat_Type_desc_Ch,")
      '  .AppendLine("Wstat_Cde_Tbl.Wstat_No_In,Wstat_Cde_Tbl.Wstat_Name_Ch,Wstat_Cde_Tbl.Wstat_Abrv_Ch,Wstat_Cde_Tbl.Wstat_Payroll_Ch,Staffing_tbl.Staffing_Note_Vc,")
      '  .AppendLine("Pay_Information_Tbl.PayInfo_FlsaHours_In,shift_tbl.shift_abrv_ch, shift_tbl.Shift_TimeDuration_Ch,Staffing_Tbl.Staffing_Start_Dt,")
      '  .AppendLine("Staffing_Tbl.Staffing_End_Dt, Staffing_Tbl.Staffing_Benign_Si,Wstat_Cde_Tbl.Wstat_FLSA_Si, Wstat_Cde_Tbl.Wstat_WageFactor_In,STRAT.strat_name_ch) AS AAA ")
      '  .AppendLine("LEFT OUTER JOIN Resource_Tbl R ON AAA.RscMaster_No_In = R.RscMaster_No_In ")
      '  '.AppendLine("LEFT OUTER JOIN TimeStore.dbo.Disaster_Pay_Rules DRP ON AAA.Staffing_Calendar_Da = DRP.disaster_date ")
      '  .AppendLine("WHERE staffing_calendar_da BETWEEN ISNULL(R.Rsc_From_Da, @Start) AND ISNULL(R.Rsc_Thru_Da, @End) ")
      '  .AppendLine("ORDER BY AAA.RscMaster_Name_Ch ASC, AAA.staffing_calendar_da ASC, AAA.staffing_start_dt ASC") ' AND R.rsc_disable_si='N' ' Removed 8/20/2014
      'End With
      Try
        Dim ds As DataSet = dbc.Get_Dataset(query, P)
        Dim tmp As New List(Of TelestaffTimeData)(
          From dbRow In ds.Tables(0).AsEnumerable()
          Select New TelestaffTimeData With {
            .DisasterRule = -1, 'dbRow("disaster_rule"),
            .EmployeeId = dbRow("RscMaster_EmployeeID_Ch"),
            .RequiresApproval = (dbRow("RequiresApproval") = "Y"),
            .WorkCode = IsNull(dbRow("Wstat_Payroll_Ch"), "000"),
            .WorkDate = dbRow("Staffing_Calendar_Da"),
            .WorkHours = IsNull(dbRow("Staffing_Hours"), "0"),
            .ShiftType = dbRow("shift_abrv_ch"),
            .Comment = dbRow("Comment"),
            .Job = dbRow("Job_Abrv_Ch"),
            .Staffing_Detail = dbRow("staffing_detail"),
            .ConstantShift = (dbRow("shift_type_no_in") = 0),
            .ProfileType = dbRow("payinfo_no_in"),
            .ProfileID = dbRow("rsc_no_in"),
            .StratName = "", 'dbRow("StratName"),
            .FLSAHoursRequirement = IsNull(dbRow("PayInfo_FlsaHours_In"), "0"),
            .WorkType = dbRow("Wstat_Name_Ch"),
            .WorkTypeAbrv = dbRow("WstatAbrv"),
            .StartTime = dbRow("Staffing_Start_Dt"),
            .EndTime = dbRow("Staffing_End_Dt"),
            .IsWorkingTime = (dbRow("IsWorkingTime") = 1),'.IsWorkingTime = (dbRow("Wstat_Type_desc_Ch").ToString.Trim.ToUpper <> "NON WORKING"),
            .IsPaidTime = Not IsDBNull(dbRow("Wstat_WageFactor_In")),
            .CountsTowardsOvertime = (dbRow("Wstat_FLSA_Si").ToString.Trim.ToUpper = "Y"),
            .Specialties = dbRow("Specialties"),
            .ProfileSpecialties = dbRow("ProfileSpecialties"),
            .ProfileStartDate = IsNull(dbRow("Rsc_From_Da"), Date.MinValue),
            .ProfileEndDate = IsNull(dbRow("Rsc_Thru_Da"), Date.MaxValue),
            .PayRate = Calculate_PayRate_With_Incentives(IsNull(dbRow("Rsc_Hourwage_db"), 0), .Specialties, .Job, .WorkTypeAbrv, .ProfileType, I)})


        '.ShiftDuration = dbRow("Shift_TimeDuration_Ch").ToString.Split(",")(1),
        Return TelestaffTimeData.UpdateOfficeTimeForDisaster(StartDate, tmp)
      Catch ex As Exception
        Log(ex)
        Return Nothing
      End Try
    End Function

    Private Shared Function UpdateOfficeTimeForDisaster(Startdate As Date,
                                                        tl As List(Of TelestaffTimeData)) As List(Of TelestaffTimeData)

      Dim DisasterPayRules As List(Of DisasterEventRules) = DisasterEventRules.Get_Cached_Disaster_Rules(Startdate.AddDays(13))

      Dim employee_dict As Dictionary(Of Integer, FinanceData) = GetCachedEmployeeDataFromFinplusAsDictionary()
      Dim f As FinanceData
      If DisasterPayRules.Count() = 0 Then Return tl ' no disaster, no need to do anything.

      Dim newTimelist As New List(Of TelestaffTimeData)

      Dim disasterWorkPayCodes As New List(Of String) From {
        "299",
        "301",
        "302",
        "303"
      }

      Dim disastertimelist = (From t In tl
                              Where
                                t.ProfileType = TelestaffProfileType.Office And
                                t.IsWorkingTime And
                                disasterWorkPayCodes.Contains(t.WorkCode)
                              Select t).ToList()


      For Each ttd In disastertimelist

        If employee_dict.ContainsKey(ttd.EmployeeId) Then
          f = employee_dict(ttd.EmployeeId)
        Else
          f = Nothing
        End If
        If ttd.Staffing_Detail.Length > 0 Then

          Dim tmpPayRules = (From d In DisasterPayRules
                             Where ttd.Staffing_Detail = d.telestaff_staffing_detail
                             Select d).ToList()

          For Each dpr In tmpPayRules

            If ttd.WorkDate.Date >= dpr.StartDate.Date And ttd.WorkDate.Date <= dpr.EndDate.Date Then

              If ttd.StartTime < dpr.EndDateTime And ttd.EndTime > dpr.StartDateTime Then

                ttd.DisasterRule = dpr.pay_rule
                ttd.Finplus_Project_Code = dpr.finplus_project_code
                If ttd.DisasterRule = 0 Then
                  Select Case ttd.WorkCode
                    Case "299"
                      ttd.WorkCode = "002"
                    Case "301"
                      ttd.WorkCode = "230"
                    Case "302"
                      ttd.WorkCode = "231"
                    Case "303"
                      ttd.WorkCode = "232"

                  End Select


                Else
                  If ttd.DisasterRule > 0 Then ttd.WorkCode = "299"
                End If

                If ttd.StartTime < dpr.StartDateTime Then
                  Dim earlyStart = ttd.Clone()
                  earlyStart.EndTime = dpr.StartDateTime
                  earlyStart.UpdateHoursWorked()
                  earlyStart.UpdatePayCode(dpr, f)
                  earlyStart.DisasterRule = 0

                  newTimelist.Add(earlyStart)
                  ttd.StartTime = dpr.StartDateTime
                  ttd.UpdateHoursWorked()

                End If

                If ttd.EndTime > dpr.EndDateTime Then
                  Dim lateEnd = ttd.Clone()
                  lateEnd.StartTime = dpr.EndDateTime
                  lateEnd.UpdateHoursWorked()
                  lateEnd.UpdatePayCode(dpr, f)
                  lateEnd.DisasterRule = 0
                  newTimelist.Add(lateEnd)

                  ttd.EndTime = dpr.EndDateTime
                  ttd.UpdateHoursWorked()

                End If
                ttd.UpdatePayCode(dpr, f)

              End If

            End If
          Next
        End If

      Next

      For Each ttd In newTimelist
        Dim tmpPayRules = (From d In DisasterPayRules
                           Where ttd.Staffing_Detail = d.telestaff_staffing_detail
                           Select d).ToList()
        For Each dpr In tmpPayRules
          If ttd.WorkDate.Date >= dpr.StartDate.Date And ttd.WorkDate.Date <= dpr.EndDate.Date Then

            If ttd.StartTime < dpr.EndDateTime And ttd.EndTime > dpr.StartDateTime Then
              ttd.DisasterRule = dpr.pay_rule
              ttd.Finplus_Project_Code = dpr.finplus_project_code
              If ttd.DisasterRule = 0 Then
                Select Case ttd.WorkCode
                  Case "299"
                    ttd.WorkCode = "002"
                  Case "301"
                    ttd.WorkCode = "230"
                  Case "302"
                    ttd.WorkCode = "231"
                  Case "303"
                    ttd.WorkCode = "232"
                End Select
              Else
                If ttd.DisasterRule > 0 Then ttd.WorkCode = "299"
              End If
              If employee_dict.ContainsKey(ttd.EmployeeId) Then
                f = employee_dict(ttd.EmployeeId)
              Else
                f = Nothing
              End If
              ttd.UpdatePayCode(dpr, f)
            End If
          End If

        Next
      Next


      tl.AddRange(newTimelist)

      'Dim pr2 = (From t In tl Where t.DisasterRule = 2 Select t).ToList()
      'Dim pr1 = (From t In tl Where t.DisasterRule = 1 Select t).ToList()

      Return tl.OrderBy(Function(t) t.EmployeeId).ThenBy(Function(j) j.StartTime).ToList()
    End Function

    Private Sub UpdateHoursWorked()
      WorkHours = EndTime.Subtract(StartTime).TotalHours
    End Sub

    Private Sub UpdatePayCode(dpr As DisasterEventRules, f As FinanceData)

      If f Is Nothing Then Exit Sub

      If dpr.pay_rule = 2 Then
        If StartTime <= dpr.EndDateTime And EndTime >= dpr.StartDateTime And IsWorkingTime Then
          If f.IsExempt Then
            WorkCode = "301"
          Else
            WorkCode = "303" ' 303 is the disaster code for double time.
          End If
        End If
      End If
      If dpr.pay_rule = 1 Then

        If f.IsExempt Then
          If (WorkDate.DayOfWeek = DayOfWeek.Sunday Or WorkDate.DayOfWeek = DayOfWeek.Saturday) And IsWorkingTime Then
            WorkCode = "301"
            ' None of the office staff are scheduled to work on Saturday or Sunday
          End If
        End If
        If Not f.IsExempt Then
          If WorkDate.DayOfWeek = DayOfWeek.Saturday And IsWorkingTime Then
            WorkCode = "302" ' 302 is the disaster code for overtime
          End If
          If WorkDate.DayOfWeek = DayOfWeek.Sunday And IsWorkingTime Then
            WorkCode = "303" ' 303 is the disaster code for double time.
          End If
        End If

      End If
      If dpr.pay_rule = 0 Then
        If WorkDate.DayOfWeek = DayOfWeek.Sunday And Not f.IsExempt And IsWorkingTime Then
          WorkCode = "303" ' 303 is the disaster code for double time.
        End If
      End If


    End Sub

    'Private Function BalanceDisasterOfficeHoursToTheDay(StartDate As Date,
    '                                                    timelist As List(Of TelestaffTimeData),
    '                                                    employeeData As Dictionary(Of Integer, FinanceData),
    '                                                    DisasterPayRules As List(Of DisasterEventRules))
    '  Dim employees = (From t In timelist
    '                   Where t.ProfileType = TelestaffProfileType.Office
    '                   Select t.EmployeeId).Distinct().ToList()

    '  Dim disasterWorkDates As List(Of Date) = EPP.PopulateDisasterWorkDates(StartDate, DisasterPayRules)

    '  Dim disasterWorkPayCodes As New List(Of String) From {
    '    "299",
    '    "301",
    '    "302",
    '    "303"
    '  }

    '  For Each e In employees
    '    For Each d In disasterWorkDates
    '      For Each dpr In DisasterPayRules
    '        Dim worktimes = (From t In timelist
    '                         Where t.EmployeeId = e And
    '                          t.WorkDate = d And
    '                          t.IsWorkingTime And
    '                           disasterWorkPayCodes.Contains(t.WorkCode)
    '                         Select t).ToList()


    '        Dim NormallyScheduledWorkHours As Integer = 8
    '        If e = 1158 Then NormallyScheduledWorkHours = 10
    '        If d.DayOfWeek = DayOfWeek.Saturday Or d.DayOfWeek = DayOfWeek.Sunday Then
    '          NormallyScheduledWorkHours = 0
    '        End If

    '        Dim totalhours = (From w In worktimes Select w.WorkHours).Sum

    '        Dim difference As Double = 0
    '        If totalhours > NormallyScheduledWorkHours Then
    '          difference = totalhours - NormallyScheduledWorkHours
    '        Else

    '        End If

    '        For Each ttd In worktimes
    '          If ttd.WorkDate.Date >= dpr.StartDate.Date And ttd.WorkDate.Date <= dpr.EndDate.Date Then

    '            If ttd.StartTime < dpr.EndDate And ttd.EndTime > dpr.StartDate Then

    '            End If
    '          End If
    '        Next

    '      Next
    '    Next
    '  Next

    'End Function

  End Class

End Namespace

