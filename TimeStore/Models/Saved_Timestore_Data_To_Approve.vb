Namespace Models
  Public Class Saved_TimeStore_Data_To_Approve
    Property approval_id As Long = 0
    Property approval_hours_id As Long = 0
    Property work_hours_id As Long = 0
    Property field_id As Integer = 0
    Property worktimes As String = ""
    Property hours_used As Double = 0
    Property hours_approved As Double = 0
    Property payrate As Double?
    Property date_added As Date = Date.MaxValue
    Property is_approved As Boolean = False
    Property approved_by_employee_id As Integer = 0
    Property approved_by_username As String = ""
    Property approved_by_machinename As String = ""
    Property approved_by_ip_address As String = ""
    Property note As String = ""
    Property date_approval_added As Date = Date.MaxValue

    Public Sub New(dr As DataRow)
      Load(dr)
    End Sub

    Public Sub Load(dr As DataRow)
      Try
        approval_hours_id = dr("approval_hours_id")
        work_hours_id = dr("work_hours_id")
        field_id = dr("field_id")
        worktimes = dr("worktimes")
        hours_used = dr("hours_used")
        If Not IsDBNull(dr("payrate")) Then
          payrate = dr("payrate")
        End If
        date_added = dr("date_added")
        If Get_TimeStore_Fields_By_ID()(field_id).Requires_Approval Then
          If Not IsDBNull(dr("approval_id")) Then
            approval_id = dr("approval_id")
            hours_approved = dr("hours_approved")
            is_approved = (dr("is_approved") = 1)
            approved_by_employee_id = dr("by_employeeid")
            approved_by_ip_address = dr("by_ip_address")
            approved_by_machinename = dr("by_machinename")
            approved_by_username = dr("by_username")
            note = dr("note")
            date_approval_added = dr("date_approval_added")
          End If
        Else
          is_approved = True
        End If

      Catch ex As Exception
        Log(ex)
      End Try
    End Sub
  End Class
End Namespace

