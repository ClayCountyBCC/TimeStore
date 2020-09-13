Namespace Models

  Public Class Paycode
    Property pay_code As String
    Property title As String
    Property percent_x As Double
    Property lv_add As String
    Property lv_sub As String
    Property pay_type As String
    Property time_type As String
    Property default_classify As String = ""

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
        WITH DefaultClassify AS (

          SELECT DISTINCT
            C.class_cd classify
            ,PT.pay_code
          FROM paytable PT
          INNER JOIN clstable C ON C.pay_cd = PT.pay_code AND C.pay_cd != '002'
          WHERE
            C.class_cd IN (SELECT DISTINCT classify FROM payrate)

        ), DefaultClassifyNoDupes AS (

          SELECT
            pay_code
            ,COUNT(*) CNT
          FROM DefaultClassify
          GROUP BY pay_code
          HAVING COUNT(*) = 1

        ), DefaultClassifyFinal AS (

          SELECT
            DC.classify
            ,DC.pay_code
          FROM DefaultClassify DC
          INNER JOIN DefaultClassifyNoDupes N ON N.pay_code = DC.pay_code

        )
        SELECT
          P.pay_code
          ,LTRIM(RTRIM(P.title)) title
          ,P.percent_x
          ,ISNULL(LTRIM(RTRIM(P.lv_add)), '') lv_add
          ,ISNULL(LTRIM(RTRIM(P.lv_sub)), '') lv_sub
          ,LTRIM(RTRIM(P.time_type)) time_type
          ,LTRIM(RTRIM(P.pay_type)) pay_type
          ,ISNULL(LTRIM(RTRIM(C.classify)), '') default_classify
        FROM paytable P
        LEFT OUTER JOIN DefaultClassifyFinal C ON P.pay_code = C.pay_code
        ORDER BY P.pay_code;"
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