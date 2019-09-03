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

    Public Sub New()

    End Sub

    Public Shared Function Get_Disaster_Rules(pay_period_ending As Date) As List(Of DisasterEventRules)

      Dim dp As New DynamicParameters()
      dp.Add("@ppe", pay_period_ending)

      Dim query As String = "
        DECLARE @pps DATE = DATEADD(dd, -13, @ppe);

        SELECT
          Name
          ,StartDate
          ,EndDate
          ,force_disaster_prompt
          ,pay_rule
        FROM Disaster_Events
        WHERE 
          CAST(StartDate AS DATE) <= @ppe
          AND CAST(EndDate AS DATE) >= @pps
        ORDER BY StartDate"

      Return Get_Data(Of DisasterEventRules)(query, dp, ConnectionStringType.Timestore)
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

