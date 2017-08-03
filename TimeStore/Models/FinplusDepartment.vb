Namespace Models
  Public Class FinplusDepartment
    Public Property Department As String
    Public Property DepartmentNumber As String
    Public ReadOnly Property DepartmentDisplay
      Get
        Return Department & " (" & DepartmentNumber & ")"
      End Get
    End Property
  End Class
End Namespace