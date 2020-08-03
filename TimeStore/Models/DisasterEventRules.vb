Imports Dapper
Imports System.Runtime.Caching

Namespace Models

  Public Class DisasterEventRules

    Public Property id As Integer
    Public Property Name As String = ""
    Public Property StartDate As DateTime
    Public Property EndDate As DateTime
    Public Property force_disaster_prompt As Boolean = False
    Public Property pay_rule As Integer
    Public Property period_id As Integer
    Public Property period_name As String = ""
    Public Property telestaff_staffing_detail = ""
    Public Property finplus_project_code = ""

    Public Sub New()

    End Sub

    Public Shared Function Get_Disaster_Rules(pay_period_ending As Date) As List(Of DisasterEventRules)

      Dim dp As New DynamicParameters()
      dp.Add("@ppe", pay_period_ending)

      Dim query As String = "
        DECLARE @pps DATE = DATEADD(dd, -13, @ppe);

        SELECT
          DE.id
          ,DE.Name
          ,DE.StartDate
          ,DE.EndDate
          ,DE.force_disaster_prompt
          ,DE.pay_rule
          ,DP.id period_id
          ,DP.Name period_name
          ,DP.TelestaffStaffingDetail telestaff_staffing_detail
          ,DP.FinplusProjectCode finplus_project_code
        FROM Disaster_Events DE
        INNER JOIN Disaster_Event_Period_Lookup L ON DE.id = L.disaster_event_id
        INNER JOIN Disaster_Period DP ON L.disaster_period_id = DP.id
        WHERE 
          CAST(DE.StartDate AS DATE) <= @ppe
          AND CAST(DE.EndDate AS DATE) >= @pps
        ORDER BY DP.StartDate"
      Dim rules = Get_Data(Of DisasterEventRules)(query, dp, ConnectionStringType.Timestore)

      For Each rule In rules
        If rule.EndDate.Second = 59 Then rule.EndDate = rule.EndDate.AddSeconds(1)
      Next
      Return rules

    End Function

    Public Shared Function Get_Cached_Disaster_Rules(pay_period_ending As Date) As List(Of DisasterEventRules)
      Dim key As String = "disaster_rules," & pay_period_ending.ToShortDateString()
      Dim CIP As New CacheItemPolicy With {
        .AbsoluteExpiration = Now.AddHours(12)
      }
      Return myCache.GetItem(key, CIP)
    End Function

  End Class

End Namespace

