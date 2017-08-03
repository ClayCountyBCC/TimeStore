Namespace Models
  Public Class PayPeriodList
    Public Property PayPeriodStart As Date
    Public Property Index As Integer
    Public ReadOnly Property PayPeriodEndDisplayShort As String
      Get
        Return PayPeriodStart.AddDays(13).ToString("yyyyMMdd")
      End Get
    End Property
    Public ReadOnly Property PayPeriodStartDisplay As String
      Get
        Return PayPeriodStart.ToShortDateString
      End Get
    End Property
    Public ReadOnly Property PayPeriodEndDisplay As String
      Get
        Return PayPeriodStart.AddDays(13).ToShortDateString
      End Get
    End Property
  End Class
End Namespace