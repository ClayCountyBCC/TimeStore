Namespace Models

  Public Class Paycode
    Property pay_code As String
    Property title As String
    Property percent_x As Double
    Property lv_add As String
    Property lv_sub As String
    Property pay_type As String
    Property time_type As String

    Public Sub New()
    End Sub

    Public Shared Function GetFromProduction() As Dictionary(Of String, Paycode)
      Return GetData(ConnectionStringType.FinPlus)
    End Function

    Public Shared Function GetFromTraining() As Dictionary(Of String, Paycode)
      Return GetData(ConnectionStringType.FinplusTraining)
    End Function

    Public Shared Function GetData(cs As ConnectionStringType) As Dictionary(Of String, Paycode)
      Dim query As String = "
      SELECT
        pay_code
        ,LTRIM(RTRIM(title)) title
        ,percent_x
        ,ISNULL(LTRIM(RTRIM(lv_add)), '') lv_add
        ,ISNULL(LTRIM(RTRIM(lv_sub)), '') lv_sub
        ,LTRIM(RTRIM(time_type)) time_type
        ,LTRIM(RTRIM(pay_type)) pay_type
      FROM paytable
      ORDER BY pay_code;"
      Dim data = Get_Data(Of Paycode)(query, cs)
      Dim dict As Dictionary(Of String, Paycode) = data.ToDictionary(Function(p) p.pay_code, Function(p) p)
      Return dict
    End Function

    Public Shared Function GetCachedFromProduction() As Dictionary(Of String, Paycode)
      Dim paycodes As Dictionary(Of String, Paycode) = myCache.GetItem("paycode_production")
      Return paycodes
    End Function

    Public Shared Function GetCachedFromTraining() As Dictionary(Of String, Paycode)
      Dim paycodes As Dictionary(Of String, Paycode) = myCache.GetItem("paycode_training")
      Return paycodes
    End Function

  End Class
End Namespace