Namespace Models
  Public Class TimeStore_Hours_To_Approve
    Property Approval_Hours_ID As Integer
    Property FieldName As String = ""
    Property FieldDisplay As String = ""
    Property FieldID As Integer = 0
    Property WorkTimes As String = ""
    Property Hours_Used As Double = 0
    Property Payrate As Double?
    Property Hours_Approved As Double?
    Property Is_Approved As Integer?
    Public ReadOnly Property Approved As Boolean
      Get
        If Is_Approved.HasValue AndAlso Is_Approved.Value = 1 Then Return True Else Return False
      End Get
    End Property
    Public ReadOnly Property Denied As Boolean
      Get
        If Is_Approved.HasValue AndAlso Is_Approved.Value = 0 Then Return True Else Return False
      End Get
    End Property
    Property DateAdded As Date = Date.MinValue
    Property Handled_By_EmployeeID As Integer?
    Property Handled_By_Username As String = ""
    Property Handled_By_Machinename As String = ""
    Property Handled_By_IP_Address As String = ""
    Property Note As String = ""
  End Class
End Namespace