Imports Dapper
Imports System.Runtime.Caching

Namespace Models

  Public Class DisasterEventRules

    Public Property id As Integer
    Public Property Name As String = ""
    Public Property StartDateTime As DateTime
    Public Property EndDateTime As DateTime
    Public Property StartDate As DateTime
    Public Property EndDate As DateTime
    Public Property pay_rule As Integer = 0
    Public Property telestaff_staffing_detail As String
    Public Property finplus_project_code As String
    Public Property period_id As Integer

    Public Sub New()

    End Sub

    Public Shared Function Get_Disaster_Rules(pay_period_ending As Date) As List(Of DisasterEventRules)

      Dim dp As New DynamicParameters()
      dp.Add("@ppe", pay_period_ending)

      Dim query As String = "
        DECLARE @pps DATE = DATEADD(dd, -13, @ppe);

        SELECT
          E.id
          ,E.Name
          ,E.StartDateTime
          ,E.EndDateTime
          ,E.StartDate
          ,E.EndDate
          ,E.pay_rule
          ,P.TelestaffStaffingDetail telestaff_staffing_detail
          ,P.FinplusProjectCode finplus_project_code
          ,P.id period_id
        FROM Disaster_Events E
        INNER JOIN Disaster_Event_Period_Lookup L ON E.id = L.disaster_event_id
        INNER JOIN Disaster_Period P ON L.disaster_period_id = P.id
        WHERE 
          CAST(E.StartDate AS DATE) <= @ppe
          AND CAST(E.EndDate AS DATE) >= @pps
        ORDER BY StartDate"
      Dim rules = Get_Data(Of DisasterEventRules)(query, dp, ConnectionStringType.Timestore)

      For Each rule In rules
        If rule.EndDateTime.Second = 59 Then rule.EndDateTime = rule.EndDateTime.AddSeconds(1)
      Next
      Return rules

    End Function

    Public Shared Function Get_Cached_Disaster_Rules(pay_period_ending As Date) As List(Of DisasterEventRules)
      Dim key As String = "disaster_rules," & pay_period_ending.ToShortDateString()
      Dim CIP As New CacheItemPolicy With {
        .AbsoluteExpiration = Now.AddHours(1)
      }
      Return myCache.GetItem(key, CIP)
    End Function

    Public Shared Function Get_Telestaff_Staffing_Details() As List(Of String)
      ' This function is not cached as it will only be used when setting up a new Disaster Event 
      Dim query As String = "
        SELECT DISTINCT
          LTRIM(RTRIM(UPPER(CAST(Staffing_Detail_Ch AS VARCHAR(50))))) Staffing_Detail
        FROM
          CLAYBCCMSCSQL.WorkForceTelestaff.dbo.Staffing_Tbl
        WHERE
          1 = 1
          AND staffing_calendar_da > '1/1/2018'
          AND Staffing_Detail_Ch IS NOT NULL
          AND LTRIM(RTRIM(Staffing_Detail_Ch)) NOT IN ( '' ) 
        ORDER BY 
          Staffing_Detail"
      Return Get_Data(Of String)(query, ConnectionStringType.Telestaff)
    End Function

    Public Shared Function Get_Finplus_Project_Codes() As List(Of String)
      ' This function is not cached it will only be used when setting up a new Disaster Event 
      Dim query As String = "
        SELECT DISTINCT
          code
        FROM finplus51.dbo.proj_title
        ORDER BY code"
      Return Get_Data(Of String)(query, ConnectionStringType.FinPlus)
    End Function

  End Class

End Namespace

