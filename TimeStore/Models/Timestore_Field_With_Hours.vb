Namespace Models
  Public Class Timestore_Field_With_Hours
    Inherits Timestore_Field

    Public Property Field_Hours As Double

    Public Sub New(tsf As Timestore_Field, Hours As Double)
      MyBase.New(tsf.Field_ID, tsf.Field_Name, tsf.Field_Display, tsf.Requires_Approval)
      Field_Hours = Hours
    End Sub
  End Class
End Namespace