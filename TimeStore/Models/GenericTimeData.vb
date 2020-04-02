Namespace Models
  Public Class GenericTimeData
    Property EmployeeID As Integer = 0
    Property FullName As String = ""
    Property LastName As String = ""
    Property FirstName As String = ""
    Property Classify As String = ""
    Property EmployeeType As String = ""
    ReadOnly Property IsExempt As Boolean
      Get
        Return EmployeeType = "E"
      End Get
    End Property
    Property IsFullTime As Boolean = True
    Property JobTitle As String = ""
    Property HoursNeededForOvertime As Double = 0
    Property BasePayrate As Double = 0
    Property TelestaffPayRate As Double = 0
    Property Comment As String = ""
    Property HireDate As Date = Date.MinValue
    Property WorkDate As Date = Date.MinValue
    Property WorkTimes As String = ""
    Property DepartmentName As String = ""
    Property DepartmentID As String = ""
    Property BankedVacation As Double = 0
    Property BankedSick As Double = 0
    Property BankedCompTimeEarned As Double = 0
    Property BankedHoliday As Double = 0
    Property RegularWork As Double = 0
    Property Vacation As Double = 0
    Property Holiday As Double = 0
    Property Sick As Double = 0
    Property SickFamilyLeave As Double = 0
    Property CompTimeEarned As Double = 0
    Property CompTimeUsed As Double = 0
    Property Admin As Double = 0
    Property AdminBereavement As Double = 0
    Property AdminDisaster As Double = 0
    Property AdminWorkersComp As Double = 0
    Property AdminJuryDuty As Double = 0
    Property AdminMilitaryLeave As Double = 0
    Property AdminOther As Double = 0
    Property WorkersComp As Double = 0
    Property SickLeavePool As Double = 0
    Property AdminEducation As Double = 0
    Property Swap As Double = 0
    Property MWI As Double = 0
    Property StepUp As Double = 0
    Property ShiftTrade As Double = 0
    'Property AdminNonWorking As Double = 0
    Property HonorGuard As Double = 0
    Property LeaveWithoutPay As Double = 0
    Property LWOPSuspension As Double = 0
    Property LWOPScheduled As Double = 0
    Property SickLeaveWithoutPay As Double = 0
    Property BreakCredit As Double = 0
    Property DoubleTime As Double = 0
    Property CallMin As Double = 0
    Property Vehicle As Double = 0
    'Property CallAdjust As Double = 0
    Property OnCallMinimumHours As Double = 0
    Property OnCallWorkHours As Double = 0
    Property OnCallTotalHours As Double = 0
    Property TerminationDate As Date = Date.MinValue
    Property UnionTimePool As Double = 0

    Public Sub New(TCTD As TimecardTimeData, f As FinanceData)
      Load_EmployeeData(f)
      Load_Data(TCTD)
    End Sub

    Public Sub New(TTD As TelestaffTimeData, f As FinanceData)
      Load_EmployeeData(f)
      Load_Data(TTD)
    End Sub

    Private Sub Load_Data(tctd As TimecardTimeData)
      WorkDate = tctd.WorkDate
      WorkTimes = tctd.WorkTimes
      Comment = tctd.Comment
      Vehicle = tctd.Vehicle

      Admin = tctd.AdminHours ' This needs to be broken out into the ones below:
      AdminBereavement = tctd.AdminBereavement
      AdminDisaster = tctd.AdminDisaster
      AdminJuryDuty = tctd.AdminJuryDuty
      AdminMilitaryLeave = tctd.AdminMilitaryLeave
      AdminWorkersComp = tctd.AdminWorkersComp
      AdminEducation = 0
      AdminOther = tctd.AdminOther
      WorkersComp = 0
      ' Now the rest
      Vacation = tctd.VacationHours
      SickLeavePool = tctd.SickLeavePoolHours
      Sick = tctd.SickHours
      SickFamilyLeave = tctd.SickFamilyLeave
      CompTimeEarned = tctd.CompTimeEarned
      CompTimeUsed = tctd.CompTimeUsed
      Holiday = tctd.HolidayHours
      LeaveWithoutPay = tctd.LWOPHours
      DoubleTime = tctd.DoubleTimeHours
      BreakCredit = tctd.BreakCreditHours
      'CallAdjust = tctd.CallAdjustHours
      OnCallMinimumHours = tctd.OnCallMinimumHours
      OnCallTotalHours = tctd.OnCallTotalHours
      OnCallWorkHours = tctd.OnCallWorkHours
      RegularWork = tctd.WorkHours
      LWOPScheduled = tctd.ScheduledLWOPHours
      LWOPSuspension = tctd.LWOPSuspensionHours
    End Sub

    Private Sub Load_Data(ttd As TelestaffTimeData)
      TelestaffPayRate = ttd.PayRate
      WorkDate = ttd.WorkDate
      WorkTimes = ttd.StartTime.ToString & " - " & ttd.EndTime.ToString
      Comment = ttd.Comment

      Select Case ttd.WorkTypeAbrv.ToUpper
        Case "MWI", "DWMI"
          MWI = ttd.WorkHours
        Case "SWAP", "DSWAP" ' Also counts as regular hours
          Swap = ttd.WorkHours
        Case "V", "VS", "DL", "VBC" ' Dispatch Vacation is listed as Dispatch Leave
          Vacation = ttd.WorkHours
        Case "H", "DH", "HS"
          Holiday = ttd.WorkHours
        Case "S", "SS", "DSL" ' Dispatch Sick Leave
          Sick = ttd.WorkHours
        Case "AA", "ADM", "ADMNSWAP", "ADWG", "ADMG" ' Also counts as regular hours
          Admin = ttd.WorkHours
        Case "EL"
          AdminEducation = ttd.WorkHours
        Case "CTE"
          CompTimeEarned = ttd.WorkHours
        Case "CTU"
          CompTimeUsed = ttd.WorkHours
        Case "LWP"
          LeaveWithoutPay = ttd.WorkHours
        Case "ML"
          AdminMilitaryLeave = ttd.WorkHours ' Also counts as regular hours
        Case "OJI" ' This is admin leave while injured.
          AdminWorkersComp = ttd.WorkHours

        Case "SU12", "OT12", "OTLC12", "OTLR12", "OTM12",
             "OTLR10", "OTM10", "SU10", "OT10", "OTLC10",
             "OTLR10", "SUE", "OTSUE", "OTMSUE", "OTLCSUE",
             "OTLRSUE", "OTSUED", "SUEG", "SUO", "OTSUO", "OTMSUO",
             "OTLRSUO", "OTLCSUO", "OTSUOD", "SUOG", "SUBC",
             "OTSUBC", "OTMSUBC", "OLTRSUBC", "OTLCSUBC",
             "OTSUBCD", "SUBCG", "SUED"
          StepUp = ttd.WorkHours
        Case "ST10", "ST12", "STE", "STO", "STBC"
          ShiftTrade = ttd.WorkHours

        Case "SDOT"
          DoubleTime = ttd.WorkHours
        Case "SLWP"
          SickLeaveWithoutPay = ttd.WorkHours
        Case "UTP"
          UnionTimePool = ttd.WorkHours
        Case "WC" ' This is used when Worker's Comp is paying the employee.
          WorkersComp = ttd.WorkHours
        Case "OTHGE", "OTHGT" ' Also counts as regular hours
          HonorGuard = ttd.WorkHours
        Case "BL" ' Also counts as regular hours
          AdminBereavement = ttd.WorkHours
        Case "JD" ' Also counts as regular hours
          AdminJuryDuty = ttd.WorkHours
        Case Else ' missing "SLOT"
          RegularWork = ttd.WorkHours
      End Select
    End Sub

    Private Sub Load_EmployeeData(f As FinanceData)
      EmployeeID = f.EmployeeId
      FirstName = f.EmployeeFirstName
      LastName = f.EmployeeLastName
      FullName = f.EmployeeName
      EmployeeType = f.EmployeeType
      BankedCompTimeEarned = f.Banked_Comp_Hours
      BankedHoliday = f.Banked_Holiday_Hours
      BankedSick = f.Banked_Sick_Hours
      BankedVacation = f.Banked_Vacation_Hours
      BasePayrate = f.Base_Payrate
      Classify = f.Classify
      DepartmentName = f.DepartmentName
      DepartmentID = f.Department
      HireDate = f.HireDate
      HoursNeededForOvertime = f.HoursNeededForOvertime
      IsFullTime = f.isFulltime
      TerminationDate = f.TerminationDate
      JobTitle = f.JobTitle
    End Sub

  End Class
End Namespace