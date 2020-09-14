Namespace Models

  Public Class PayrollEditData
    Property employee As FinanceData
    Property pay_period_ending As Date
    Property justifications As New List(Of PayrollChangeJustification)
    Property base_payroll_data As New List(Of PayrollData)
    Property payroll_change_data As New List(Of PayrollData)
    Property finplus_payrates As New List(Of PayrollData)
    Property paystub_data As New List(Of Paystub.PaystubList)
    Property leave_balance As Dictionary(Of String, Decimal)
    Property comparisons As New List(Of PayrollChangeComparison)
    Property messages As New List(Of String)
    Public Sub New()

    End Sub

    Public Sub New(e As FinanceData,
                   ppe As Date,
                   ByRef base_data As List(Of PayrollData),
                   ByRef payroll_changes As List(Of PayrollData),
                   ByRef justifications As List(Of PayrollChangeJustification),
                   ByRef payrates As List(Of PayrollData),
                   ByRef paystubs As List(Of Paystub.PaystubList),
                   leave_balance As Dictionary(Of String, Decimal),
                   ByRef timestoreErrors As List(Of Timestore_Error),
                   ByRef payroll_changes_comparison As List(Of PayrollData),
                   ByRef base_data_comparison As List(Of PayrollData))
      employee = e
      If leave_balance IsNot Nothing Then
        Me.leave_balance = leave_balance
      End If
      pay_period_ending = ppe
      Me.justifications.AddRange((From j In justifications
                                  Where j.employee_id = employee.EmployeeId AndAlso
                                   j.pay_period_ending = pay_period_ending
                                  Select j).ToList())

      base_payroll_data.AddRange((From b In base_data
                                  Where b.employee_id = employee.EmployeeId AndAlso
                                    b.pay_period_ending = pay_period_ending
                                  Order By b.paycode Ascending, b.payrate Ascending
                                  Select b).ToList)

      payroll_change_data.AddRange((From c In payroll_changes
                                    Where c.employee_id = employee.EmployeeId AndAlso
                                      c.pay_period_ending = pay_period_ending
                                    Order By c.paycode Ascending, c.payrate Ascending
                                    Select c).ToList)

      finplus_payrates.AddRange((From p In payrates
                                 Where p.employee_id = employee.EmployeeId AndAlso
                                     p.pay_period_ending = pay_period_ending
                                 Order By p.paycode Ascending
                                 Select p).ToList)

      paystub_data.AddRange((From p In paystubs
                             Where p.employee_id = employee.EmployeeId
                             Order By p.check_date Descending
                             Select p).ToList)

      Dim b_comparison = (From b In base_data
                          Where b.employee_id = employee.EmployeeId AndAlso
                                    b.pay_period_ending = pay_period_ending
                          Order By b.paycode Ascending, b.payrate Ascending,
                            b.hours Ascending, b.amount Ascending,
                            b.project_code Ascending, b.classify Ascending
                          Select b).ToList

      Dim pc_comparison = (From c In payroll_changes
                           Where c.employee_id = employee.EmployeeId AndAlso
                                      c.pay_period_ending = pay_period_ending
                           Order By c.paycode Ascending, c.payrate Ascending,
                            c.hours Ascending, c.amount Ascending,
                            c.project_code Ascending, c.classify Ascending
                           Select c).ToList

      comparisons.AddRange(PayrollChangeComparison.CreateComparisons(b_comparison, pc_comparison))

      For Each tse In (From t In timestoreErrors
                       Where t.employee_id = e.EmployeeId
                       Select t).ToList
        messages.Add(tse.error_text)
      Next
      GenerateMessages()
    End Sub

    Public Shared Function GetPayrollEdits(PayPeriodEnding As Date, current As PayrollStatus) As List(Of PayrollEditData)
      Dim employee_data As List(Of FinanceData)
      Dim paycodes As Dictionary(Of String, Paycode)
      Select Case current.target_db
        Case PayrollStatus.DatabaseTarget.Finplus_Production
          employee_data = (From d In GetCachedEmployeeDataFromFinplus()
                           Where d.IsTerminated = False
                           Order By d.DepartmentName, d.EmployeeLastName, d.EmployeeFirstName
                           Select d).ToList
          paycodes = Paycode.GetCachedFromProduction()
        Case PayrollStatus.DatabaseTarget.Finplus_Training
          employee_data = (From d In GetCachedEmployeeDataFromFinplusTraining()
                           Where d.IsTerminated = False
                           Order By d.DepartmentName, d.EmployeeLastName, d.EmployeeFirstName
                           Select d).ToList
          paycodes = Paycode.GetCachedFromTraining
        Case Else
          Return Nothing
      End Select
      Dim timestore_errors As List(Of Timestore_Error) = Timestore_Error.GetErrors(PayPeriodEnding)
      Dim leave_balances = LeaveBalance.GetLeaveBalances(PayPeriodEnding)
      Dim edit_data As New List(Of PayrollEditData)
      Dim finplus_payrates = PayrollData.GetAllFinplusPayrates(PayPeriodEnding, paycodes)
      Dim justifications = PayrollChangeJustification.GetJustificationsByPayPeriod(PayPeriodEnding)
      Dim base_data = PayrollData.GetAllBasePayrollData(PayPeriodEnding, paycodes)
      Dim base_data_comparison = PayrollData.GetAllBasePayrollData(PayPeriodEnding, paycodes)
      Dim payroll_changes = PayrollData.GetAllPayrollChanges(PayPeriodEnding, paycodes)
      Dim payroll_changes_comparison = PayrollData.GetAllPayrollChanges(PayPeriodEnding, paycodes)
      Dim paystubs = Paystub.PaystubList.Get_All_Recent_Paystubs_Cached()

      For Each ed In employee_data
        Dim lb As Dictionary(Of String, Decimal)
        If leave_balances.ContainsKey(ed.EmployeeId) Then
          lb = leave_balances(ed.EmployeeId)
        Else
          lb = Nothing
        End If
        edit_data.Add(New PayrollEditData(ed,
                                          PayPeriodEnding,
                                          base_data,
                                          payroll_changes,
                                          justifications,
                                          finplus_payrates,
                                          paystubs,
                                          lb,
                                          timestore_errors,
                                          base_data_comparison,
                                          payroll_changes_comparison))
      Next
      Return (From e In edit_data
              Order By e.employee.Department Ascending, e.employee.EmployeeId Ascending
              Select e).ToList
    End Function

    Public Sub GenerateMessages()
      ' Non-Overtime hours entered less than normal
      ' compare some summation of hours to the payhours for 002
      If payroll_change_data.Count() = 0 Then
        messages.Add("*** NO HOURS ENTERED")
      Else
        Dim amount_check_time_types As New List(Of String)
        amount_check_time_types.Add("C")
        amount_check_time_types.Add("A")
        amount_check_time_types.Add("N")
        For Each pcd In payroll_change_data
          If amount_check_time_types.Contains(pcd.paycode_detail.time_type) = False Then
            If pcd.payrate = 0 Then
              pcd.messages.Add("*** WARNING - TIMECARD AMOUNT = 0")
            Else
              Dim calc_payrate = Math.Round((employee.Base_Payrate * pcd.paycode_detail.percent_x), 5)

              If Math.Round(pcd.payrate, 5) <> calc_payrate Then
                ' This message will need to be added based on the the percent_x value on the paycode
                ' so that we're comparing payrates equally, regardless of the multiplier
                pcd.messages.Add($"*** TIMECARD RATE {pcd.payrate} NOT = CURRENT RATE {calc_payrate}")
              End If
            End If
          End If

          ' Check leave balances against the right paycode
          Dim leave_code As String = pcd.paycode_detail.lv_sub
          If leave_code.Length > 0 Then
            If leave_balance.ContainsKey(leave_code) Then
              If leave_balance(leave_code) < pcd.hours Then
                pcd.messages.Add($"*** WARNING - LEAVE CODE {leave_code} CURRENT BALANCE IS { leave_balance(leave_code) }")
              End If
            Else
              pcd.messages.Add($"*** WARNING - LEAVE CODE {leave_code} CURRENT BALANCE IS 0.0000")
            End If
          End If


        Next
        ' Overtime checks
        ' You can only have so many hours (hours needed for overtime) before you must have some overtime hours
        ' Conversely, you must have at least (hours needed for overtmie) 
        If employee.HoursNeededForOvertime > 0 Then


          Dim total_regular = (From pcd In payroll_change_data
                               Where pcd.paycode_detail.pay_type = "H" AndAlso
                                 pcd.paycode_detail.time_type = "R"
                               Select pcd.hours).Sum
          If total_regular < employee.HoursNeededForOvertime Then
            messages.Add($"NON-OVERTIME HOURS ENTERED  LESS  THAN NORMAL ({employee.HoursNeededForOvertime}) HOURS")
          Else
            If total_regular > employee.HoursNeededForOvertime Then
              messages.Add($"NON-OVERTIME HOURS ENTERED  MORE  THAN NORMAL ({employee.HoursNeededForOvertime}) HOURS")
            End If
          End If
        End If
      End If



    End Sub

  End Class

End Namespace

