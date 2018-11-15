Namespace Models
  Public Class Namedday
    Property Dept As String = ""
    Property Name As String = ""
    Property NamedDate As Date = Date.MinValue

    Public Sub New(EmployeeName As String, EmployeeDate As Date, EmployeeDept As String)
      Name = EmployeeName
      NamedDate = EmployeeDate
      Dept = EmployeeDept
    End Sub

    Private Function GetCurrentBirthdays(Birthday As Date) As Date()
      Dim bd(2) As Date
      Try
        Dim m As Integer = Birthday.Month
        Dim d As Integer = Birthday.Day
        If m = 2 And d = 29 Then d = 28
        bd(0) = New Date(Now.Year - 1, m, d)
        bd(1) = New Date(Now.Year, m, d)
        bd(2) = New Date(Now.Year + 1, m, d)
      Catch ex As Exception
        Dim e As New ErrorLog(ex, "")
        Return Nothing
      End Try

      Return bd
    End Function

    Public Function ToList() As List(Of Namedday)
      Dim bd() As Date = GetCurrentBirthdays(NamedDate)
      Dim bl As New List(Of Namedday)
      For a As Integer = bd.GetLowerBound(0) To bd.GetUpperBound(0)
        bl.Add(New Namedday(Name, bd(a), Dept))
      Next
      Return bl
    End Function

    Public Shared Function GetAllCachedBirthdays() As List(Of Namedday)
      Dim cip As New Runtime.Caching.CacheItemPolicy
      cip.AbsoluteExpiration = DateTime.Now.AddHours(12)
      Return myCache.GetItem("birthdays", cip)
    End Function

    Public Shared Function GetAllBirthdays() As List(Of Namedday)

      ' This section of code is for finding active employees that are in Finplus
      ' but aren't in Active Directory.
      Try

        Dim aded As Dictionary(Of Integer, AD_EmployeeData) = GetADEmployeeData()
        Dim fl = GetCachedEmployeeDataFromFinplus()
        Dim flad = (From f In fl
                    Where Not f.IsTerminated And
                      f.BirthDate <> Date.MinValue And
                      aded.ContainsKey(f.EmployeeId)
                    Select f.BirthDate, f.Department, f.EmployeeId)
        Dim bdayList As New List(Of Namedday)
        For Each f In flad
          Dim m As Integer = f.BirthDate.Month
          Dim d As Integer = f.BirthDate.Day
          If m = 2 And d = 29 Then d = 28
          Dim name As String = aded(f.EmployeeId).Name
          bdayList.Add(New Namedday(name, New Date(Now.Year - 1, m, d), f.Department))
          bdayList.Add(New Namedday(name, New Date(Now.Year, m, d), f.Department))
          bdayList.Add(New Namedday(name, New Date(Now.Year + 1, m, d), f.Department))
        Next

        Return bdayList

      Catch ex As Exception
        Dim e As New ErrorLog(ex, "")
        Return New List(Of Namedday)
      End Try
    End Function



  End Class
End Namespace