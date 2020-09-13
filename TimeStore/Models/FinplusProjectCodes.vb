Namespace Models
  Public Class FinplusProjectCodes
    Property project_code As String
    Property name As String

    Public Sub New()
    End Sub

    Public Shared Function GetFilteredProjectCodes(PayPeriodStart As Date)
      Dim dp As New DynamicParameters
      dp.Add("@pay_period_start", PayPeriodStart)
      Dim query As String = "
        SELECT 
          LTRIM(RTRIM([key_proj])) project_code
          ,[title] name
        FROM [finplus51].[dbo].[faproject]
        WHERE 
          (start_date IS NULL OR 
          start_date <= @pay_period_start)
        AND (stop_date IS NULL OR stop_date > @pay_period_start)
        AND closed IS NULL
        ORDER BY key_proj ASC"
      Return Get_Data(Of FinplusProjectCodes)(query, dp, ConnectionStringType.FinPlus)
    End Function

    Public Shared Function GetAllProjectCodes(PayPeriodStart As Date)
      Dim dp As New DynamicParameters
      dp.Add("@pay_period_start", PayPeriodStart)
      Dim query As String = "
        SELECT 
          LTRIM(RTRIM([key_proj])) project_code
          ,[title] name
        FROM [finplus51].[dbo].[faproject]
        ORDER BY key_proj ASC"
      Return Get_Data(Of FinplusProjectCodes)(query, dp, ConnectionStringType.FinPlus)
    End Function

    Public Shared Function GetCachedFilteredProjectCodes(PayPeriodStart As Date)
      Dim key As String = "filtered_project_codes," & PayPeriodStart.ToShortDateString()
      Return myCache.GetItem(key)
    End Function

    Public Shared Function GetCachedAllProjectCodes(PayPeriodStart As Date)
      Dim key As String = "all_project_codes," & PayPeriodStart.ToShortDateString()
      Return myCache.GetItem(key)
    End Function

  End Class

End Namespace

