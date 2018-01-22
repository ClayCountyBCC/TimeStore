Imports System.Web.Mvc
Imports System.Security.Cryptography
Imports TimeStore.Models
Imports System.Runtime.Caching

Namespace Controllers
  Public Class ReportsController
    Inherits Controller

    Private cache As myCache
    Private defaultCIP As New CacheItemPolicy()

    ' GET: Reports
    Function Index() As ActionResult
      Dim tca As Timecard_Access = Timecard_Access.GetTimeCardAccess(Request.LogonUserIdentity.Name)
      If Not tca.Backend_Reports_Access Then Return New HttpUnauthorizedResult
      Return View()
    End Function

    <HttpPost>
    Public Function GetGenericData(StartDate As Date, EndDate As Date, Fields As List(Of String)) As JsonNetResult
      Dim jnr As New JsonNetResult
      Dim tca As Timecard_Access = Timecard_Access.GetTimeCardAccess(Request.LogonUserIdentity.Name)
      If Not tca.Backend_Reports_Access Then
        jnr.Data = "Error: Unauthorized"
      Else
        jnr.Data = GetGenericTimeData(StartDate, EndDate, Fields)
      End If
      Return jnr
    End Function

    '<HttpPost>
    'Public Function GetGenericDataNoDates() As JsonNetResult
    '    ' The purpose of htis function is to get some initial data populated.
    '    ' It will automatically calculate and use the previous pay period's dates.
    '    Dim jnr As New JsonNetResult
    '    Dim tca As Timecard_Access = GetTimeCardAccess(Request.LogonUserIdentity.Name)
    '    If Not tca.Backend_Reports_Access Then
    '        jnr.Data = "Error: Unauthorized"
    '    Else
    '        Dim endDate As Date = GetPayPeriodStart(Today).AddDays(-1)
    '        Dim startDate As Date = GetPayPeriodStart(endDate)
    '        jnr.Data = GetGenericTimeData(startDate, endDate)
    '    End If
    '    Return jnr
    'End Function

    'Private Function GetTimeCardAccess(UserName As String) As Timecard_Access
    '  Return GetTimeCardAccess(AD_EmployeeData.GetEmployeeIDFromAD(UserName))
    'End Function

    'Private Function GetTimeCardAccess(EmployeeId As Integer) As Timecard_Access
    '  Dim key As String = "tca," & EmployeeId
    '  defaultCIP.AbsoluteExpiration = Now.AddHours(12)
    '  Return myCache.GetItem(key, defaultCIP)
    'End Function

  End Class
End Namespace
