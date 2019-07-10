Namespace Models
  Public Class Employee_Data
    Public Property Lastname As String
    Public Property Firstname As String
    Public Property EmployeeID As Integer
    Public Property DepartmentName As String
    Public Property GroupName As String = ""
    Public Property DepartmentID As String
    Public Property Terminated As Boolean
    Public Property TerminationDateDisplay As String
    Public ReadOnly Property EmployeeDisplay As String
      Get
        Return Lastname & ", " & Firstname & " - " & EmployeeID.ToString
      End Get
    End Property

    Public Sub New(f As FinanceData)
      DepartmentID = f.Department
      DepartmentName = f.DepartmentName
      EmployeeID = f.EmployeeId
      Firstname = f.EmployeeFirstName

      Dim tgDepts As New List(Of String) From {
        "1703",
        "2103"
      }

      If tgDepts.Contains(DepartmentID) Then
        GroupName = GetGroupName(f.EmployeeId)
      End If

      Lastname = f.EmployeeLastName
      TerminationDateDisplay = f.TerminationDate.ToShortDateString
      Terminated = (f.TerminationDate < Date.MaxValue)
    End Sub

    Public Shared Function GetGroupName(EmployeeID As Integer) As String
      Dim TG = GetTelestaffGroups()
      Dim s As String = ""
      If TG.TryGetValue(EmployeeID, s) Then
        Return s
      Else
        Return ""
      End If
    End Function

  End Class
End Namespace
