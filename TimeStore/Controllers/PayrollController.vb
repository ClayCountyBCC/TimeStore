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
    <Route("EditsCompleted")>
    Public Function EditsCompleted(PayPeriodEnding As Date) As PayrollStatus
      Dim current = GetCurrentStatus(PayPeriodEnding)
      If current.can_edit Then
        PayrollStatus.MarkEditsComplete(PayPeriodEnding, current.my_access, ConnectionStringType.Timestore)
        Return GetCurrentStatus(PayPeriodEnding)
      Else
        Return Nothing
      End If
    End Function

    <HttpGet>
    <Route("ChangesApproved")>
    Public Function ChangesApproved(PayPeriodEnding As Date) As PayrollStatus
      Dim current = GetCurrentStatus(PayPeriodEnding)
      If current.can_approve_edits Then
        PayrollStatus.ApproveEdits(PayPeriodEnding, current.my_access, ConnectionStringType.Timestore)
        Return GetCurrentStatus(PayPeriodEnding)
      Else
        Return Nothing
      End If
    End Function

    <HttpGet>
    <Route("CancelApproval")>
    Public Function CancelApproval(PayPeriodEnding As Date) As PayrollStatus
      Dim current = GetCurrentStatus(PayPeriodEnding)
      If current.my_access.PayrollAccess > 1 Then
        PayrollStatus.CancelApproval(PayPeriodEnding, current.my_access, ConnectionStringType.Timestore)
        Return GetCurrentStatus(PayPeriodEnding)
      Else
        Return Nothing
      End If
    End Function

    <HttpGet>
    <Route("EditsInComplete")>
    Public Function EditsInComplete(PayPeriodEnding As Date) As PayrollStatus
      Dim current = GetCurrentStatus(PayPeriodEnding)
      If current.my_access.PayrollAccess > 0 Then
        PayrollStatus.MarkEditsInComplete(PayPeriodEnding, current.my_access, ConnectionStringType.Timestore)
        Return GetCurrentStatus(PayPeriodEnding)
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
    <Route("PayrollEditsByEmployee")>
    Public Function GetEdits(PayPeriodEnding As Date, EmployeeId As Integer) As PayrollEditData
      Dim current = GetCurrentStatus(PayPeriodEnding)
      If current.my_access.PayrollAccess > 0 Then
        Return PayrollEditData.GetPayrollEditsByEmployee(PayPeriodEnding, current, EmployeeId)
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

    <HttpGet>
    <Route("GetCheck")>
    Public Function GetCheckInfo(EmployeeId As Integer, CheckNumber As String) As List(Of PayrollData)
      Dim tca As Timecard_Access = GetTimeCardAccess(User.Identity.Name)
      If tca.PayrollAccess < 1 Then Return New List(Of PayrollData)
      Return PayrollData.GetCheckPayInformation(EmployeeId, CheckNumber)
    End Function

    <HttpPost>
    <Route("SaveChanges")>
    Public Function SaveChanges(PayPeriodEnding As Date, EmployeeId As Integer, PayrollChanges As List(Of PayrollData)) As PayrollEditData
      Dim current = GetCurrentStatus(PayPeriodEnding)
      If current.can_edit Then
        Dim b = PayrollData.SavePayrollChanges(PayPeriodEnding, EmployeeId, current.my_access.UserName, PayrollChanges)
      Else
        Return Nothing
      End If
      Return PayrollEditData.GetPayrollEditsByEmployee(PayPeriodEnding, current, EmployeeId)
    End Function

    <HttpPost>
    <Route("SaveJustifications")>
    Public Function SaveJustifications(PayPeriodEnding As Date, EmployeeId As Integer, Justifications As List(Of PayrollChangeJustification)) As List(Of PayrollChangeJustification)
      Dim current = GetCurrentStatus(PayPeriodEnding)
      If current.can_edit Then
        Return PayrollChangeJustification.SaveJustifications(PayPeriodEnding, EmployeeId, current.my_access.UserName, Justifications)
      Else
        Return Nothing
      End If
    End Function

    <HttpGet>
    <Route("DeleteJustification")>
    Public Function DeleteJustification(PayPeriodEnding As Date, id As Integer) As Boolean
      Dim current = GetCurrentStatus(PayPeriodEnding)
      If current.can_edit Then
        Return PayrollChangeJustification.DeleteJustification(id)
      Else
        Return Nothing
      End If
    End Function

    <HttpGet>
    <Route("GetProjectCodes")>
    Public Function GetProjectCodes(PayPeriodEnding As Date) As List(Of FinplusProjectCodes)
      Return FinplusProjectCodes.GetCachedFilteredProjectCodes(PayPeriodEnding.AddDays(-13))
    End Function

    <HttpGet>
    <Route("GetPayruns")>
    Public Function GetPayruns(PayPeriodEnding As Date) As List(Of String)
      Dim current = GetCurrentStatus(PayPeriodEnding)
      Return PayrollStatus.GetPayruns(current)
    End Function

    <HttpGet>
    <Route("PostTimestoreDataToFinplus")>
    Public Function PostTimestoreDataToFinplus(PayPeriodEnding As Date, Payrun As String) As PayrollStatus
      Dim current = GetCurrentStatus(PayPeriodEnding)
      If current.can_update_finplus Then
        Return PayrollStatus.PostToFinplus(PayPeriodEnding, Payrun, current)
      Else
        Return Nothing
      End If
    End Function

  End Class
End Namespace
