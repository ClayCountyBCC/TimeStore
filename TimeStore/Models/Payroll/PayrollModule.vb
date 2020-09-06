Imports TimeStore.Models

Module PayrollModule

  Public Function StartPayrollProcess(PayPeriodEnding As Date,
                                      IncludeBenefits As Boolean,
                                      current As PayrollStatus,
                                      Target As PayrollStatus.DatabaseTarget) As PayrollStatus
    If current.can_start Then
      ' we'll need to force a recalculate of all of the timestore data
      GetTimeCards(PayPeriodEnding.AddDays(-13), True) ' the true passed to this function forces a recalculate
      ' then we'll do the payroll start process.
      Return PayrollStatus.StartPayroll(PayPeriodEnding,
                                        IncludeBenefits,
                                        Target,
                                        current.my_access)
    End If

    Return Nothing
  End Function

End Module
