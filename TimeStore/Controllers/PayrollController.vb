Imports System.Net
Imports System.Web.Http
Imports TimeStore.Models

Namespace Controllers
  <RoutePrefix("API/Payroll")>
  Public Class PayrollController
    Inherits ApiController

    Private Function GetTimeCardAccess(UserName As String) As Timecard_Access
#If DEBUG Then
      'UserName = "wardj"
#End If
      Dim EID As Integer = AD_EmployeeData.GetEmployeeIDFromAD(UserName)
      Dim tca = Timecard_Access.GetTimeCardAccess(EID)
      tca.UserName = UserName
      Return tca
    End Function

    <HttpGet>
    <Route("Start")>
    Public Function StartPayroll(PayPeriodEnding As Date, IncludeBenefits As Boolean, TargetDB As Integer) As PayrollStatus
      StartPayrollProcess(PayPeriodEnding,
                          IncludeBenefits,
                          GetCurrentStatus(PayPeriodEnding),
                          TargetDB)
      Return GetCurrentStatus(PayPeriodEnding)
    End Function

    <HttpGet>
    <Route("Reset")>
    Public Function ResetPayroll(PayPeriodEnding As Date) As PayrollStatus
      Dim current = GetCurrentStatus(PayPeriodEnding)
      If current.can_reset Then
        Return PayrollStatus.ResetPayroll(PayPeriodEnding, current.my_access)
      Else
        Return Nothing
      End If
    End Function

    <HttpGet>
    <Route("PayrollEdits")>
    Public Function GetEdits(PayPeriodEnding As Date) As List(Of PayrollEditData)
      Dim current = GetCurrentStatus(PayPeriodEnding)
      If current.my_access.PayrollAccess > 0 Then
        Return PayrollEditData.GetPayrollEdits(PayPeriodEnding, current)
      Else
        Return Nothing
      End If
    End Function

    <HttpGet>
    <Route("Paycodes")>
    Public Function GetPaycodes(PayPeriodEnding As Date) As Dictionary(Of String, Paycode)
      Dim current = GetCurrentStatus(PayPeriodEnding)
      If current.target_db = PayrollStatus.DatabaseTarget.Finplus_Production Then
        Return Paycode.GetCachedFromProduction()
      Else
        If current.target_db = PayrollStatus.DatabaseTarget.Finplus_Training Then
          Return Paycode.GetCachedFromTraining()
        Else
          Return Nothing
        End If
      End If
    End Function

    <HttpGet>
    <Route("GetStatus")>
    Public Function GetStatus(PayPeriodEnding As Date) As PayrollStatus
      Return GetCurrentStatus(PayPeriodEnding)
    End Function

    Private Function GetCurrentStatus(PayPeriodEnding As Date) As PayrollStatus
      Dim tca As Timecard_Access = GetTimeCardAccess(User.Identity.Name)
      Return PayrollStatus.GetPayrollStatus(PayPeriodEnding, tca)
    End Function

  End Class
End Namespace
