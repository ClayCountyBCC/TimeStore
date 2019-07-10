Public Class Timeclock_Data
  Public Property my_employee_id As Integer ' this is so that we know who is requesting it and can select them on the client.
  Public Property work_date As Date
  Public Property employee_id As Integer
  Public Property employee_name As String
  Public Property department_id As String
  Public Property work_times As String
  Public Property punch_in_issue As String
  Public Property punch_out_issue As String
  Public Property reports_to As Integer
  Public Property reports_to_name As String
  Public Property non_working_hours As String

  Public Sub New()

  End Sub


  Public Shared Function View(WorkDate As Date, RequestorEmployeeId As Integer) As List(Of Timeclock_Data)
    '/*
    'Filters
    '  Late (Punched in after 7 AM)
    '	Punch not 7 AM and 3:30 PM
    '	Missing Punch
    '	No Lunch

    '/*
    '  To start, we'll pull out the good punches for the day
    '  We'll do this by rigidly looking for people who punched in 
    '  just prior to 7 AM, and people who punched in just after 3:30 PM,
    '  with our rounding window taken into consideration.
    '  early:                      hh:mm:ss
    '    They had to clock in from 06:52:30 AM to 7:00:00 AM
    '  late:
    '    They had to clock out from 03:30 PM to 3:37:30 PM

    '  Everyone that appears in the EmployeesWithGoodAMPunches CTE
    '  is not late, and everyone who appears in the EmployeesWithGoodPMPunches CTE
    '  is not early.

    '  Conversely, everyone who is not in a given list either clocked in late or clocked out early.
    '  Their punch can also be missing.
    '*/
    Dim sql As String = "
      USE TimeStore;

      WITH EmployeesWithGoodAMPunches AS (

        SELECT DISTINCT
          employee_id
        FROM Timeclock_Data
        WHERE 
          source='F'
          AND DATEDIFF(SECOND, CAST(raw_punch_date AS TIME), CAST(rounded_punch_date AS TIME)) >= -59
          AND DATEPART(HOUR, rounded_punch_date) = 7
          AND DATEPART(MINUTE, rounded_punch_date) = 0
          AND CAST(raw_punch_date AS DATE) = @Punchdate

      ), EmployeesWithGoodPMPunches AS (

        SELECT DISTINCT
          employee_id
        FROM Timeclock_Data
        WHERE 
          source='F'
          AND DATEDIFF(SECOND, CAST(raw_punch_date AS TIME), CAST(rounded_punch_date AS TIME)) <= 0
          AND DATEPART(HOUR, rounded_punch_date) = 15
          AND DATEPART(MINUTE, rounded_punch_date) = 30
          AND CAST(raw_punch_date AS DATE) = @Punchdate

      ), BadPunches AS (

        SELECT
          CAST(E.empl_no AS INT) employee_id,
          CASE WHEN GAM.employee_id IS NOT NULL 
            THEN ''
            ELSE 
              CASE WHEN DATEPART(HH, CAST(MIN(raw_punch_date) AS TIME)) < 12 
              THEN CONVERT(VARCHAR(15), CAST(MIN(raw_punch_date) AS TIME), 100)
              ELSE 'No Punch'
              END
          END earliest_raw_punch,
          CASE WHEN GPM.employee_id IS NOT NULL 
          THEN ''
          ELSE
              CASE WHEN DATEPART(HH, CAST(MAX(raw_punch_date) AS TIME)) > 12 
              THEN CONVERT(VARCHAR(15), CAST(MAX(raw_punch_date) AS TIME), 100)
              ELSE 'No Punch'
              END
          END latest_raw_punch
        FROM [SQLCLUSFINANCE\FINANCE].finplus50.dbo.employee E
        INNER JOIN [SQLCLUSFINANCE\FINANCE].finplus50.dbo.person P ON E.empl_no = P.empl_no
        LEFT OUTER JOIN Timeclock_Data T ON E.empl_no=T.employee_id
          AND CAST(T.raw_punch_date AS DATE) = @PunchDate
          AND T.source = 'F'
        LEFT OUTER JOIN EmployeesWithGoodAMPunches GAM ON CAST(E.empl_no AS INT) = GAM.employee_id
        LEFT OUTER JOIN EmployeesWithGoodPMPunches GPM ON CAST(E.empl_no AS INT) = GPM.employee_id
        WHERE 
          E.home_orgn IN ('3701', '3711', '3712')
          AND (P.term_date IS NULL OR P.term_date > @PunchDate)
          AND (GAM.employee_id IS NULL OR GPM.employee_id IS NULL)
        GROUP BY CAST(E.empl_no AS INT), GAM.employee_id, GPM.employee_id

      ), NonWorkingHours AS (

        SELECT      
          W.work_hours_id,
          TF.field_display_name,
          H.hours_used
        FROM Work_Hours W
        INNER JOIN Hours_To_Approve H ON W.work_hours_id = H.work_hours_id
        INNER JOIN Timestore_Fields TF ON H.field_id = TF.field_id
        WHERE 
          1=1    
          AND W.work_date = @PunchDate
          AND H.hours_used > 0

      )

        SELECT
          @RequestorEmployeeId my_employee_id,
          @PunchDate work_date,
          CAST(E.empl_no AS INT) employee_id,
          LTRIM(RTRIM(E.home_orgn)) department_id,
          LTRIM(RTRIM(E.f_name)) + ' ' + LTRIM(RTRIM(E.l_name)) employee_name,
          ISNULL(W.work_times, 'No punches') work_times,
          ISNULL(B.earliest_raw_punch, '') punch_in_issue,
          ISNULL(B.latest_raw_punch, '') punch_out_issue,
          ISNULL(A.reports_to, 0) reports_to,
          LTRIM(RTRIM(ISNULL(BOSS.f_name, ''))) + ' ' + LTRIM(RTRIM(ISNULL(BOSS.l_name, ''))) reports_to_name,

          ISNULL(STUFF((
            SELECT 
              CHAR(13) + CHAR(10) +         
              NWH.field_display_name + ' - '  +
              CAST(CAST(NWH.hours_used AS NUMERIC(10, 2)) AS VARCHAR(10))
            FROM NonWorkingHours NWH
            WHERE NWH.work_hours_id = W.work_hours_id
            FOR XML PATH (''), TYPE).value('(./text())[1]','nvarchar(max)')
            , 
            1, 
            2, 
            N''), '') non_working_hours

        FROM [SQLCLUSFINANCE\FINANCE].finplus50.dbo.employee E
        INNER JOIN [SQLCLUSFINANCE\FINANCE].finplus50.dbo.person P ON E.empl_no = P.empl_no
        LEFT OUTER JOIN Work_Hours W ON E.empl_no = W.employee_id AND W.work_date = @PunchDate
        LEFT OUTER JOIN BadPunches B ON E.empl_no = B.employee_id
        --LEFT OUTER JOIN BadLatePunch BLP ON E.empl_no = BLP.employee_id
        LEFT OUTER JOIN Access A ON E.empl_no = A.employee_id
        LEFT OUTER JOIN [SQLCLUSFINANCE\FINANCE].finplus50.dbo.employee BOSS ON A.reports_to = BOSS.empl_no    
        WHERE 
          E.home_orgn IN ('3701', '3711', '3712')
          AND (P.term_date IS NULL OR P.term_date > @PunchDate)
        ORDER BY E.home_orgn, E.l_name, E.f_name"

    Dim dp As New DynamicParameters()
    dp.Add("@PunchDate", WorkDate.Date)
    dp.Add("@RequestorEmployeeId", RequestorEmployeeId)
    Return Get_Data(Of Timeclock_Data)(sql, dp, ConnectionStringType.Timestore)

  End Function
End Class
