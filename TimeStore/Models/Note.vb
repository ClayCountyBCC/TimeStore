Namespace Models
  Public Class Note
    Property EmployeeID As Integer
    Property PayPeriodEnding As Date
    Property Note As String
    Property Date_Added As Date
    ReadOnly Property Date_Added_Display As String
      Get
        Return Date_Added.ToString
      End Get
    End Property
    Property Added_By As String = ""
  End Class
End Namespace