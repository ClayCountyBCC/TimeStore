Imports System.Runtime.Caching

Namespace Models

  Public NotInheritable Class myCache
    Private Shared _cache As New MemoryCache("myCache")

    Public Shared Function GetItem(key As String, Optional CIP As CacheItemPolicy = Nothing) As Object
      Dim tmpCIP As New CacheItemPolicy
      If Not CIP Is Nothing Then
        tmpCIP = CIP
      Else
        tmpCIP.AbsoluteExpiration = Now.AddHours(1)

      End If
      Return GetOrAddExisting(key, tmpCIP, Function() InitItem(key))
    End Function

    Private Shared Function GetOrAddExisting(Of T)(key As String, CIP As CacheItemPolicy, valueFactory As Func(Of T)) As T
      Dim newValue = New Lazy(Of T)(valueFactory)
      Dim oldValue = TryCast(_cache.AddOrGetExisting(key, newValue, CIP), Lazy(Of T))
      Try
        Return (If(oldValue, newValue)).Value
      Catch
        ' Handle cached lazy exception by evicting from cache. Thanks to Denis Borovnev for pointing this out!
        _cache.Remove(key)
        Throw
      End Try
    End Function

    Public Shared Function Add(key As String, value As Object, CIP As CacheItemPolicy) As Boolean
      If _cache.Contains(key) Then _cache.Remove(key)
      Return _cache.Add(key, value, CIP)
    End Function

    Public Shared Function GetObject(key As String) As Object
      Return _cache.Get(key)
    End Function

    Private Shared Function InitItem(key As String) As Object
      ' Do something expensive to initialize item
      Dim s() As String = key.Split(",")
      Select Case s(0).ToLower
        'Case "approval"
        '    Dim eid As Integer = s(1), ppd As String = s(2)
        '    Dim dtmp As Date = GetPayPeriodStart(Today.AddDays(ppd * 14))
        '    Return New GenericTimecard(dtmp, eid)
        Case "reportsto"
          Return Get_All_ReportsTo()
        Case "incentive"
          Return Incentive.Get_All_Incentive_Data()
        Case "employee_ad_data"
          Return AD_EmployeeData.GetEmployeeDataFromAD()
        Case "employee_lookup_data"
          Return AD_EmployeeData.GetEmployeeLookupData()
        Case "employeedata"
          'Dim payperiodstart As Date = s(1)
          Return GetAllEmployeeDataFromFinPlus()
        Case "tca"
          Dim eid As Integer = s(1)
          Return New Timecard_Access(eid, Nothing)
        Case "employeelist"
          Return GetEmployeeListFromFinPlus()
        Case "departmentlist"
          Return GetDepartmentListFromFinPlus()
        Case "telestaffgroupingdata"
          Return GetTelestaffGroupingData()
        Case "timecardsbyppd"
          Dim ppd As Date = s(1)
          Return GetTimeCards(ppd)
        'Case "initial", "final"
        '    Dim ppd As Date = s(1)
        '    Return PopulateApprovals((s(0) = "final"), ppd)
        'Case "timecarddata"
        '    Dim ppd As Date = s(1)
        '    Return GetEmployeeDataFromTimecard(ppd)
        Case "notes"
          Dim ppd As Date = s(1)
          Return Get_All_Notes(ppd)
        Case "telestaffdata"
          Dim ppd As Date = s(1)
          Return GetEmployeeDataFromTelestaff(ppd)
        Case "approvaldata"
          Dim ppd As Date = s(1)
          Return Get_All_Saved_Timecard_Data(ppd)
        Case "timestorefields_by_id"
          Return Get_TimeStore_Fields_By_ID()
        Case "timestorefields_by_name"
          Return Get_TimeStore_Fields_By_Name()
        Case "paycodes"
          Return PopulatePayCodes()
        Case "ltrorder"
          Return PopulateltrOrder()
        Case "disasterdates"
          Return Disaster.GetDisasters()
        Case Else
          Return Nothing
      End Select
    End Function
  End Class
End Namespace
