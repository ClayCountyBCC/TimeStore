Imports System.Runtime.Caching

Namespace Models
  Public Class EventsByWorkDate
    Property event_id As Integer
    Property work_date As Date
    Property event_name As String

    Public Sub New()
    End Sub

    Public Shared Function Get_By_PayPeriod(PayPeriodStart As Date) As List(Of EventsByWorkDate)
      Dim dp As New DynamicParameters()
      dp.Add("@Start", PayPeriodStart)
      dp.Add("@End", PayPeriodStart.AddDays(13))

      Dim query As String = "
        SELECT DISTINCT
          P.id event_id
          ,P.Name event_name
          ,D.calendar_date work_date
        FROM Disaster_Period P
        INNER JOIN Disaster_Event_Period_Lookup L ON P.id = L.disaster_period_id
        INNER JOIN Disaster_Events E ON E.id = L.disaster_event_id
        LEFT OUTER JOIN Calendar.dbo.Dates D ON D.calendar_date BETWEEN @Start AND @End 
          AND D.calendar_date BETWEEN E.StartDate AND E.EndDate
        WHERE
          E.StartDate <= @End
          AND E.EndDate >= @Start
        ORDER BY work_date"
      Return Get_Data(Of EventsByWorkDate)(query, dp, ConnectionStringType.Timestore)
    End Function

    Public Shared Function Get_By_PayPeriod_Excluding_Pay_Rule_Zero(PayPeriodStart As Date) As List(Of EventsByWorkDate)
      Dim dp As New DynamicParameters()
      dp.Add("@Start", PayPeriodStart)
      dp.Add("@End", PayPeriodStart.AddDays(13))

      Dim query As String = "
        SELECT DISTINCT
          P.id event_id
          ,P.Name event_name
          ,D.calendar_date work_date
        FROM Disaster_Period P
        INNER JOIN Disaster_Event_Period_Lookup L ON P.id = L.disaster_period_id
        INNER JOIN Disaster_Events E ON E.id = L.disaster_event_id AND E.pay_rule NOT IN (0)
        LEFT OUTER JOIN Calendar.dbo.Dates D ON D.calendar_date BETWEEN @Start AND @End 
          AND D.calendar_date BETWEEN E.StartDate AND E.EndDate
        WHERE
          E.StartDate <= @End
          AND E.EndDate >= @Start
        ORDER BY work_date"
      Return Get_Data(Of EventsByWorkDate)(query, dp, ConnectionStringType.Timestore)
    End Function

    Public Shared Function Get_By_PayPeriod_Cached(PayPeriodStart As Date) As List(Of EventsByWorkDate)
      Dim key As String = "eventsbyworkdate," & PayPeriodStart.ToShortDateString()
      Dim CIP As New CacheItemPolicy With {
        .AbsoluteExpiration = Now.AddHours(1)
      }
      Return myCache.GetItem(key, CIP)
    End Function

    Public Shared Function Get_By_PayPeriod_Excluding_Pay_Rule_Zero_Cached(PayPeriodStart As Date) As List(Of EventsByWorkDate)
      Dim key As String = "events_by_workdate_excluding_payrule_zero," & PayPeriodStart.ToShortDateString()
      Dim CIP As New CacheItemPolicy With {
        .AbsoluteExpiration = Now.AddHours(1)
      }
      Return myCache.GetItem(key, CIP)
    End Function

  End Class
End Namespace

