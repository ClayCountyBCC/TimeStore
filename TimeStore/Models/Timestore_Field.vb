Namespace Models
  Public Class Timestore_Field
    Property Field_ID As Integer
    Property Field_Name As String = ""
    Property Field_Display As String = ""
    Property Requires_Approval As Boolean = True

    Public Sub New(id As Integer, fieldName As String, displayName As String, requiresApproval As Boolean)
      Field_ID = id
      Field_Name = fieldName
      Field_Display = displayName
      Requires_Approval = requiresApproval
    End Sub
  End Class
End Namespace