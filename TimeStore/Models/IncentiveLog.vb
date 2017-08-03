

Namespace Models

  Public Class IncentiveLog
    Public Property employee_id As Integer
    Public Property old_incentive_id As Integer?
    Public Property new_incentive_id As Integer?
    Public Property old_multipler As Integer?
    Public Property new_multipler As Integer?
    Public Property by_employeeid As Integer
    Public Property by_machinename As String
    Public Property by_username As String
    Public Property by_ip_address As String

    Public Sub IncentiveLog()

    End Sub

    Public Sub Save()

    End Sub


  End Class

End Namespace