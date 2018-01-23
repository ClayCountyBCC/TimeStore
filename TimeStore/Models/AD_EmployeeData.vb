﻿Namespace Models
  Public Class AD_EmployeeData
    Property EmployeeID As Integer = 0
    Property Name As String = ""
    Property EmailAddress As String = ""
    Property Username As String = ""
    Property DatePasswordChanged As DateTime = DateTime.MaxValue
    Public ReadOnly Property PasswordExpiring As Boolean
      Get
        If DatePasswordChanged = DateTime.MaxValue Then Return False
        Dim expiration As Date = DatePasswordChanged.AddDays(180)
        Return expiration.Subtract(DateTime.Today).TotalDays < 16
      End Get
    End Property


    Public Sub New(EID As Integer,
                   EmployeeName As String,
                   Email As String,
                   User As String,
                   PasswordDate As DateTime)

      EmployeeID = EID
      Name = EmployeeName
      EmailAddress = Email
      Username = User
      DatePasswordChanged = PasswordDate
    End Sub

    Public Shared Function GetEmployeeDataFromAD() As Dictionary(Of Integer, AD_EmployeeData)
      Dim aded As New Dictionary(Of Integer, AD_EmployeeData)
      GetEmployeeDataFromAD("LDAP://OU=DomainUsers,DC=CLAYBCC,DC=local", aded)
      GetEmployeeDataFromAD("LDAP://OU=IFASUF,OU=ExtDepartments,DC=CLAYBCC,DC=local", aded)
      Return aded
    End Function

    Public Shared Function GetEmployeeLookupData() As Dictionary(Of String, Integer)
      ' this function returns a dictionary to return employee id by username
      Dim adld As New Dictionary(Of String, Integer)
      Dim aded As Dictionary(Of Integer, AD_EmployeeData) = GetEmployeeDataFromAD()
      For Each key In aded.Keys
        adld(aded(key).Username.ToLower) = key
      Next
      Return adld
    End Function

    'Public Function GetEmployeeDataFromAD() As List(Of AD_EmployeeData)
    '  Dim adel As New List(Of AD_EmployeeData)
    '  Try
    '    adel.AddRange(GetEmployeeDataFromAD("LDAP://OU=DomainUsers,DC=CLAYBCC,DC=local"))
    '    adel.AddRange(GetEmployeeDataFromAD("LDAP://OU=IFASUF,OU=ExtDepartments,DC=CLAYBCC,DC=local"))
    '    Return adel
    '  Catch ex As Exception
    '    Log(ex)
    '    Return Nothing
    '  End Try
    'End Function

    Private Shared Sub GetEmployeeDataFromAD(Path As String, ByRef aded As Dictionary(Of Integer, AD_EmployeeData))
      Try
        Dim de As New DirectoryEntry
        de.AuthenticationType = AuthenticationTypes.Secure
        de.Path = Path
        Dim ds As New DirectorySearcher(de)
        ds.Filter = "(&(objectClass=user)(employeeID=*))"
        Dim src As SearchResultCollection = ds.FindAll()
        For Each s As SearchResult In src
          Dim eid As Integer = GetADProperty(s, "employeeID")
          If eid > 0 Then
            Dim name As String = GetADProperty_String(s, "displayName")
            Dim mail As String = GetADProperty_String(s, "mail")
            Dim user As String = GetADProperty_String(s, "sAMAccountName")
            Dim pwLastChangedDate = DateTime.MaxValue
            Try
              Dim rawvalue As String = GetADProperty_String(s, "pwdLastSet")
              If rawvalue.Length > 0 Then
                pwLastChangedDate = DateTime.FromFileTimeUtc(rawvalue)
              End If
            Catch ex As Exception
              Log(ex)
            End Try
            aded(eid) = New AD_EmployeeData(eid, name, mail, user.ToLower, pwLastChangedDate)
          End If
        Next
      Catch ex As Exception
        Log(ex)
      End Try
    End Sub

    'Public Function GetEmployeeDataFromAD(Path As String) As List(Of AD_EmployeeData)
    '  Dim adel As New List(Of AD_EmployeeData)
    '  Try
    '    Dim de As New DirectoryEntry
    '    de.AuthenticationType = AuthenticationTypes.Secure
    '    de.Path = Path
    '    Dim ds As New DirectorySearcher(de)
    '    ds.Filter = "(&(objectClass=user)(employeeID=*))"
    '    Dim src As SearchResultCollection = ds.FindAll()
    '    For Each s As SearchResult In src
    '      Dim eid As Integer = GetADProperty(s, "employeeID")
    '      If eid > 0 Then
    '        Dim name As String = GetADProperty_String(s, "displayName")
    '        Dim mail As String = GetADProperty_String(s, "mail")
    '        Dim user As String = GetADProperty_String(s, "sAMAccountName")
    '        Dim pwLastChangedDate = DateTime.MaxValue
    '        Try
    '          Dim rawvalue As String = GetADProperty_String(s, "pwdLastSet")
    '          If rawvalue.Length > 0 Then
    '            pwLastChangedDate = DateTime.FromFileTimeUtc(rawvalue)
    '          End If
    '        Catch ex As Exception
    '          Log(ex)
    '        End Try
    '        adel.Add(New AD_EmployeeData(eid, name, mail, user.ToLower, pwLastChangedDate))
    '      End If
    '    Next
    '    Return adel
    '  Catch ex As Exception
    '    Log(ex)
    '    Return Nothing
    '  End Try
    'End Function

    Public Shared Function GetEmployeeIDFromAD(UserName As String) As Integer
      If UserName.Contains("\") Then UserName = UserName.Split("\")(1).ToLower
      Try
        Dim adld As Dictionary(Of String, Integer) = myCache.GetItem("employee_lookup_data")
        Return adld(UserName)
      Catch ex As Exception
        Dim e As New ErrorLog(ex, UserName)
        Return -1
      End Try

    End Function

    Private Shared Function GetADProperty(ByRef sr As SearchResult, propertyName As String) As Integer
      If sr Is Nothing Then Return 0
      If sr.Properties(propertyName).Count > 0 Then Return sr.Properties(propertyName)(0).ToString.Trim Else Return 0
    End Function

    Private Shared Function GetADProperty_String(ByRef sr As SearchResult, propertyName As String) As String
      If sr Is Nothing Then Return ""
      If sr.Properties(propertyName).Count > 0 Then Return sr.Properties(propertyName)(0).ToString.Trim Else Return ""
    End Function


  End Class
End Namespace