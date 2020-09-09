Imports System.DirectoryServices
Imports TimeStore.Models
Imports System.IO
Imports System.Data
Imports System.Data.SqlClient
Imports System.Runtime.Caching

Public Module ModuleMain

  Public Const PayPeriodEndingCutoff As Integer = 10 ' This is the hour we cutoff the ability to save changes for the previous pay period.
  Public Const toolsAppId As Integer = 20006
  Public Const toolsDBError As Tools.DB.DB_Error_Handling_Method = Tools.DB.DB_Error_Handling_Method.Send_Errors_To_Log_Only

  Public Enum ConnectionStringType
    Timestore = 0
    FinPlus = 1
    Telestaff = 2
    Timecard = 3
    Log = 4
    FinplusTraining = 5
    SpecialDisasterPayroll = 6
  End Enum

  Public Function IsItPastCutoffDate(WorkDate As Date) As Boolean
    ' This function returns true if the current date is greater than the 
    ' cutoff for the date in the pay period that was passed.  
    ' Ugh, that doesn't mean anything.  Let me try again.
    ' The cutoff for changes to the pay period is 10 AM the day after the end of
    ' the pay period.  So if the pay period were to end on 9/6/2016,
    ' users would have until 9/7/2016 10:00 AM EDT to make changes.
    ' If the current date/time is after that for a given pay period ending date, 
    ' we return true.
    'Dim specialDisasterPayPeriodStart As Date = Date.Parse("8/21/2019")
    'Dim specialDisasterPayPeriodEnd As Date = specialDisasterPayPeriodStart.AddDays(13)
    'If (WorkDate >= specialDisasterPayPeriodStart And WorkDate <= specialDisasterPayPeriodEnd) Then
    '  Return Now > GetPayPeriodStart(WorkDate).AddDays(16).AddHours(17)
    'Else
    '  Return Now > GetPayPeriodStart(WorkDate).AddDays(14).AddHours(PayPeriodEndingCutoff)
    'End If
    Return Now > GetPayperiodCutOffDateTime(WorkDate)
  End Function

  Public Function GetPayperiodCutOffDateTime(WorkDate As Date) As Date
    Dim PayPeriodStart = GetPayPeriodStart(WorkDate)
    Select Case PayPeriodStart
      Case "11/13/2019"
        Return PayPeriodStart.AddDays(12).AddHours(17).AddMinutes(30)
      Case "12/11/2019"
        Return PayPeriodStart.AddDays(12).AddHours(16)
      Case Else
        Return PayPeriodStart.AddDays(14).AddHours(PayPeriodEndingCutoff)
    End Select




  End Function

  Public Sub Log(e As Exception, Query As String)
    Dim el As New ErrorLog(e, Query)
  End Sub

  Public Sub Log(e As Exception)
    Dim el As New ErrorLog(e, "")
  End Sub

  Public Sub Log(ErrorText As String, ErrorMessage As String, ErrorStacktrace As String,
                  ErrorSource As String)
    Dim el As New ErrorLog(ErrorText, ErrorMessage, ErrorStacktrace, ErrorSource, "")
  End Sub

  Public Function IsNull(Of T)(ByVal Value As T, ByVal DefaultValue As T) As T
    If IsDBNull(Value) OrElse Value Is Nothing Then
      Return DefaultValue
    Else
      Return Value
    End If
  End Function

  Public Function getHolidayList(ByVal vYear As Integer, Optional MoveDays As Boolean = True) As List(Of Date)
    ' This function is used to get a list of holidays for a given year.

    Dim FirstWeek As Integer = 1
    Dim SecondWeek As Integer = 2
    Dim ThirdWeek As Integer = 3
    Dim FourthWeek As Integer = 4
    Dim LastWeek As Integer = 5

    Dim HolidayList As New List(Of Date)

    '   http://www.usa.gov/citizens/holidays.shtml      
    '   http://archive.opm.gov/operating_status_schedules/fedhol/2013.asp

    ' New Year's Day            Jan 1
    HolidayList.Add(DateSerial(vYear, 1, 1))

    ' Martin Luther King, Jr. third Mon in Jan
    HolidayList.Add(GetNthDayOfNthWeek(DateSerial(vYear, 1, 1), DayOfWeek.Monday, ThirdWeek))

    ' Washington's Birthday third Mon in Feb
    HolidayList.Add(GetNthDayOfNthWeek(DateSerial(vYear, 2, 1), DayOfWeek.Monday, ThirdWeek))

    ' Memorial Day          last Mon in May
    HolidayList.Add(GetNthDayOfNthWeek(DateSerial(vYear, 5, 1), DayOfWeek.Monday, LastWeek))

    ' Independence Day      July 4
    HolidayList.Add(DateSerial(vYear, 7, 4))

    ' Labor Day             first Mon in Sept
    HolidayList.Add(GetNthDayOfNthWeek(DateSerial(vYear, 9, 1), DayOfWeek.Monday, FirstWeek))

    ' Columbus Day          second Mon in Oct
    'HolidayList.Add(GetNthDayOfNthWeek(DateSerial(vYear, 10, 1), DayOfWeek.Monday, SecondWeek))

    ' Veterans Day          Nov 11
    HolidayList.Add(DateSerial(vYear, 11, 11))

    ' Thanksgiving Day      fourth Thur in Nov
    Dim ThanksGiving As Date = GetNthDayOfNthWeek(DateSerial(vYear, 11, 1), DayOfWeek.Thursday, FourthWeek)
    HolidayList.Add(ThanksGiving)
    HolidayList.Add(ThanksGiving.AddDays(+1))
    Select Case vYear
      Case 2014, 2017
        ' Christmas Eve         Dec 24
        If MoveDays Then
          HolidayList.Add(DateSerial(vYear, 12, 26)) ' for 2014 & 2017, the holidays are set to 12/25 and 12/26
        Else ' for the union employees.
          HolidayList.Add(DateSerial(vYear, 12, 24)) ' for 2014 & 2017, the holidays are set to 12/25 and 12/26
        End If

      Case Else
        ' Christmas Eve         Dec 24
        HolidayList.Add(DateSerial(vYear, 12, 24))
    End Select
    Select Case vYear
      Case 2018
        HolidayList.Add(DateSerial(2018, 12, 31))
    End Select

    ' Christmas Day         Dec 25
    HolidayList.Add(DateSerial(vYear, 12, 25))

    'saturday holidays are moved to Fri; Sun to Mon
    If MoveDays Then
      For i As Integer = 0 To HolidayList.Count - 1
        Dim dt As Date = HolidayList(i)
        If dt.DayOfWeek = DayOfWeek.Saturday Then
          HolidayList(i) = dt.AddDays(-1)
        End If
        If dt.DayOfWeek = DayOfWeek.Sunday Then
          HolidayList(i) = dt.AddDays(1)
        End If
      Next
    End If
    Return HolidayList

  End Function

  Private Function GetNthDayOfNthWeek(ByVal dt As Date, ByVal DayofWeek As Integer, ByVal WhichWeek As Integer) As Date
    'specify which day of which week of a month and this function will get the date
    'this function uses the month and year of the date provided

    'get first day of the given date
    Dim dtFirst As Date = DateSerial(dt.Year, dt.Month, 1)

    'get first DayOfWeek of the month
    Dim dtRet As Date = dtFirst.AddDays(6 - dtFirst.AddDays(-(DayofWeek + 1)).DayOfWeek)

    'get which week
    dtRet = dtRet.AddDays((WhichWeek - 1) * 7)

    'if day is past end of month then adjust backwards a week
    If dtRet >= dtFirst.AddMonths(1) Then
      dtRet = dtRet.AddDays(-7)
    End If

    'return
    Return dtRet

  End Function

  Public Function Get_Holidays_By_Payperiod(PayPeriodStart As Date, Optional MoveDates As Boolean = False) As List(Of Date)
    Dim holidays As List(Of Date) = getHolidayList(PayPeriodStart.Year, MoveDates)
    If PayPeriodStart.Year <> PayPeriodStart.AddDays(13).Year Then
      holidays.AddRange(getHolidayList(PayPeriodStart.AddDays(13).Year, MoveDates))
    End If
    holidays = (From holiday In holidays Where holiday >= PayPeriodStart And
                                     holiday < PayPeriodStart.AddDays(14)
                Select holiday).ToList
    Return holidays
  End Function

  Public Function GetPayPeriodStart(dateToCheck As Date) As Date
    ' This function takes a date and returns the pay period start date for that date.
    '9/23/2015 is the first ppd of 2016
    '9/24/2014 is the first ppd of 2015
    '9/25/2013 is the first ppd of 2014
    Return dateToCheck.AddDays(-(dateToCheck.Subtract("9/25/2013").TotalDays Mod 14))
  End Function

  Public Function GetPayPeriodStartList() As List(Of PayPeriodList)
    'Dim d As Date = GetPayPeriodStart(Today.AddYears(-1))
    'Dim endD As Date = GetPayPeriodStart(Today.AddYears(1))
    Dim d As Date = GetPayPeriodStart(Today)
    Dim ppList As New List(Of PayPeriodList)
    'For a As Integer = -30 To 30 Step 1
    For a As Integer = -6 To 1 Step 1
      Dim x As New PayPeriodList
      x.PayPeriodStart = d.AddDays(a * 14)
      x.Index = a
      ppList.Add(x)
    Next

    Return ppList
  End Function

  Public Enum Data_Type As Integer
    Timecard = 0
    Telestaff = 1
  End Enum

  Public Function Get_Data_Type(employee_id As Integer, department_id As String, pay_period_start As Date) As Data_Type
    ' this function was added to facilitate switching an employee from Telestaff to timestore, 
    ' and knowing when they switched.
    Select Case department_id
      Case "1703"
        Return Data_Type.Telestaff

      Case "2103"
        Return Data_Type.Telestaff


      Case "2102"
        If pay_period_start < CType("3/23/2016", Date) Then
          Return Data_Type.Telestaff
        Else
          Return Data_Type.Timecard
        End If

      Case "1709"
        Select Case employee_id
          Case 1109 ' Anthony Roseberry and Chip Earls moved from Telestaff to Timestore on this date.
            If pay_period_start < CType("4/30/2019", Date) Then
              Return Data_Type.Telestaff
            Else
              Return Data_Type.Timecard
            End If

          Case 2546
            If pay_period_start < CType("5/15/2019", Date) Then
              Return Data_Type.Telestaff
            Else
              Return Data_Type.Timecard
            End If

          Case Else
            Return Data_Type.Timecard
        End Select
      Case Else
        ' Jesse Hellard moved from Animal Control to Public Safety Dispatch for about 4 weeks.  Only one pay period
        ' was in telestaff, all of his other data was in Timestore / timecard.
        If employee_id = 2201 AndAlso pay_period_start = CType("8/10/2015", Date) Then Return Data_Type.Telestaff

        Return Data_Type.Timecard

    End Select

  End Function

  Public Function Is_Stepup(WorkType As String) As Boolean
    Select Case WorkType
      Case "SU12", "OT12", "OTLC12", "OTLR12", "OTM12", "SU10", "OT10", "OTLC10",
           "OTLR10", "OTM10", "SU10", "OT10", "OTLC10", "OTLR10", "SUE", "OTSUE",
           "OTMSUE", "OTLCSUE", "OTLRSUE", "OTSUED", "SUEG", "SUO", "OTSUO", "OTMSUO",
           "OTLRSUO", "OTLCSUO", "OTSUOD", "SUOG", "SUBC", "OTSUBC", "OTMSUBC",
           "OLTRSUBC", "OTLCSUBC", "OTSUBCD", "SUBCG", "DOTSUBC", "DOTSUO", "DOTSUE",
           "ST10", "ST12", "STE", "STO", "STBC", "SUED"
        Return True
      Case Else
        Return False
    End Select
  End Function


  Public Function Calculate_PayRate_With_Incentives(PR As Double,
                                                    Specialties As String,
                                                    Job As String,
                                                    WorkType As String,
                                                    ProfileType As TelestaffProfileType,
                                                    ByRef Incentives As List(Of Incentive)) As Double

    If PR = 0 Then
      Return PR
    Else
      Dim HoursByYear As Double = 0
      Select Case ProfileType
        Case TelestaffProfileType.Office
          HoursByYear = 2080
        Case TelestaffProfileType.Dispatch
          HoursByYear = 2080
        Case TelestaffProfileType.Field
          HoursByYear = 2912
      End Select
      Dim TotalIncentive As Double = Calculate_Telestaff_Incentives(Job, Specialties, Incentives)

      Select Case WorkType
        Case "SU12", "OT12", "OTLC12", "OTLR12", "OTM12" ' for older work codes
          Return Calculate_Stepup_Rate(PR, TotalIncentive, 1.12, HoursByYear)

        Case "SU10", "OT10", "OTLC10", "OTLR10", "OTM10",' for older work codes, these shouldn't be used any longer.
             "SU10", "OT10", "OTLC10", "OTLR10"
          Return Calculate_Stepup_Rate(PR, TotalIncentive, 1.1, HoursByYear)

        Case "SUE", "SUED", "OTSUE", "OTMSUE", "OTLCSUE", "OTLRSUE", "OTSUED", "SUEG"  ' Step up engineer 
          Return Calculate_Stepup_Rate(PR, TotalIncentive, 1.1, HoursByYear)

        Case "SUO", "OTSUO", "OTMSUO", "OTLRSUO", "OTLCSUO", "OTSUOD", "SUOG" ' Step up Officer
          Return Calculate_Stepup_Rate(PR, TotalIncentive, 1.12, HoursByYear)

        Case "SUBC", "OTSUBC", "OTMSUBC", "OLTRSUBC", "OTLCSUBC", "OTSUBCD", "SUBCG" ' Step up BC
          Return Calculate_Stepup_Rate(PR, TotalIncentive, 1.12, HoursByYear)

        Case "DOTSUBC" ' Step Up Doubletime BC ' For those rare occasions when someone steps up as a BC when they're on office duty, so they are eligible for double time.
          Return Calculate_Stepup_Rate(PR, TotalIncentive, 2.12, HoursByYear)

        Case "DOTSUO" ' Step up Doubletime Officer ' Same as above
          Return Calculate_Stepup_Rate(PR, TotalIncentive, 2.12, HoursByYear)

        Case "DOTSUE" ' Step up Doubletime Engineer ' Same as above.
          Return Calculate_Stepup_Rate(PR, TotalIncentive, 2.1, HoursByYear)

          ' Shift trades from here down
        Case "ST10" ' older work codes, not used anymore
          Dim StepupRate As Double = Calculate_Stepup_Rate(PR, TotalIncentive, 1.1, HoursByYear)
          Return Math.Round(StepupRate - PR, 5)

        Case "ST12" ' older work codes, not used anymore
          Dim StepupRate As Double = Calculate_Stepup_Rate(PR, TotalIncentive, 1.12, HoursByYear)
          Return Math.Round(StepupRate - PR, 5)

        Case "STE" ' Shift trade Engineer
          Dim StepupRate As Double = Calculate_Stepup_Rate(PR, TotalIncentive, 1.1, HoursByYear)
          Return Math.Round(StepupRate - PR, 5)

        Case "STO"
          Dim StepupRate As Double = Calculate_Stepup_Rate(PR, TotalIncentive, 1.12, HoursByYear)
          Return Math.Round(StepupRate - PR, 5)

        Case "STBC" ' Shift Trade
          Dim StepupRate As Double = Calculate_Stepup_Rate(PR, TotalIncentive, 1.12, HoursByYear)
          Return Math.Round(StepupRate - PR, 5)

        Case Else
          Return PR
      End Select
    End If
  End Function

  Public Function Calculate_Telestaff_Incentives(Job As String, Specialties As String, ByRef Incentives As List(Of Incentive)) As Double
    Dim s() As String = Specialties.Replace("*", "").Split(New String() {"/"}, StringSplitOptions.RemoveEmptyEntries)
    Dim TotalIncentive As Double = (From i In Incentives Where s.Contains(i.Incentive_Abrv) Select i.Incentive_Amount).Sum
    Select Case Job 'EMT/*L/PM/*SF/*SPOPS
      Case "E", "FF"
      Case Else
        ' Certain job types have the paramedic incentive baked in to the base rate and it is not subject to those calculations, 
        ' so we need to make sure we remove it from our calculations for those job types.
        If s.Contains("PM") Then
          TotalIncentive = TotalIncentive - (From i In Incentives Where i.Incentive_Abrv = "PM" Select i.Incentive_Amount).Sum
        End If
    End Select
    Return TotalIncentive
  End Function

  Public Function Calculate_Stepup_Rate(RegularPayrate As Double, TotalIncentive As Double, Wagefactor As Double, HoursByYear As Double) As Double
    Dim newPayrate As Double = 0
    Dim basePR As Double = ((RegularPayrate * HoursByYear) - TotalIncentive) / HoursByYear
    Return Math.Round(((basePR * Wagefactor * HoursByYear) + TotalIncentive) / HoursByYear, 5)
  End Function

  Public Function Calculate_Telestaff_Office_Payrate(Payrate As Double) As Double
    ' What we do here is take their regular payrate including incentives and multiply it 
    ' by their normal hours, including 2/3rds of their holidays.  This is 3088 total hours.
    ' Then we divide that by 2080, the number of hours an office employee works in a year.
    Return (Payrate * 3088) / 2080
  End Function

  Public Function Calculate_Reverse_Telestaff_Office_Payrate(OfficePayRate As Double) As Double
    ' This function will take a field employee's office rate and reverse it to get their field rate.
    ' You can compare it directly to the Calculate_Telestaff_Office_Payrate.
    Return (OfficePayRate * 2080) / 3088
  End Function

  Public Function GenericTimeCard_To_Output(tcList As List(Of GenericTimecard)) As List(Of EmployeeOutput)
    Dim eo As New List(Of EmployeeOutput)
    Dim vac() As String = {"100", "101"}
    Dim sick() As String = {"110", "111"}
    For Each e In tcList
      ' Here we create entries for each different payrate
      Dim payrates As List(Of Double) = (From c In e.calculatedTimeList Select c.payRate Distinct).ToList
      For Each p In payrates
        Dim neweo As New EmployeeOutput
        With neweo
          .EmployeeId = e.employeeID
          .EmployeeName = e.employeeName
          .Department = e.departmentNumber
          .HoursNeededForOvertime = e.hoursForOvertime
          .HireDate = e.hireDate
          .Banked_Comp = e.bankedComp
          .Banked_Vacation = e.bankedVacation
          .Banked_Holiday = e.bankedHoliday
          .ProfileType = e.TelestaffProfileType
          .Banked_Sick = e.bankedSick
          .ScheduledHours = e.scheduledHours
          .FinPlusPayrate = e.Payrate
          .TelestaffPayrate = p
          .StaffEmployee = e.TelestaffProfileType = TelestaffProfileType.Office
          .Vacation = GetHours(e.calculatedTimeList, vac, p)
          .Sick = GetHours(e.calculatedTimeList, sick, p)
          .Holiday = GetHours(e.calculatedTimeList, "134", p)
          If e.isExempt Then
            .Regular = 80 - .Vacation - .Sick
            .Scheduled_Regular_Overtime = 0
            .Scheduled_Overtime = 0
            .Absent_Without_Pay = 0
            .Unscheduled_Regular_Overtime = 0
            .Unscheduled_Overtime = 0
            .Unscheduled_Double_Overtime = 0
            .Holiday_Time_Banked = 0
            .Holiday_Time_Used = 0
            .Comp_Time_Banked = 0
            .Comp_Time_Used = 0
          Else
            .Regular = GetHours(e.calculatedTimeList, "002", p)
            .Scheduled_Regular_Overtime = GetHours(e.calculatedTimeList, "130", p)
            .Scheduled_Overtime = GetHours(e.calculatedTimeList, "131", p)
            .Absent_Without_Pay = GetHours(e.calculatedTimeList, "090", p)
            .Term_Hours = GetHours(e.calculatedTimeList, "095", p)
            .Unscheduled_Regular_Overtime = GetHours(e.calculatedTimeList, "230", p)
            .Unscheduled_Overtime = GetHours(e.calculatedTimeList, "231", p)
            .Unscheduled_Double_Overtime = GetHours(e.calculatedTimeList, "232", p)
            .Holiday_Time_Banked = GetHours(e.calculatedTimeList, "122", p)
            .Holiday_Time_Used = GetHours(e.calculatedTimeList, "123", p)
            .Comp_Time_Banked = GetHours(e.calculatedTimeList, "120", p)
            .Comp_Time_Used = GetHours(e.calculatedTimeList, "121", p)
            .Comp_Time_Banked += GetHours(e.calculatedTimeList, "118", p)
            .Comp_Time_Used += GetHours(e.calculatedTimeList, "119", p)
          End If
        End With
        eo.Add(neweo)
      Next
    Next
    Return eo


  End Function

  Public Function EPP_To_Output(eppList As List(Of EPP)) As List(Of EmployeeOutput)
    Dim eo As New List(Of EmployeeOutput)
    For Each e In eppList
      ' Here we create entries for each different payrate
      For Each p In e.Payrates
        Dim neweo As New EmployeeOutput
        With neweo
          .EmployeeId = e.EmployeeData.EmployeeId
          .EmployeeName = e.EmployeeData.EmployeeName
          .ProfileType = e.TelestaffProfileType
          .Department = e.EmployeeData.Department
          .HoursNeededForOvertime = e.TelestaffHoursNeededForOvertime
          .HireDate = e.EmployeeData.HireDate
          .Banked_Comp = e.EmployeeData.Banked_Comp_Hours
          .Banked_Vacation = e.EmployeeData.Banked_Vacation_Hours
          .Banked_Holiday = e.EmployeeData.Banked_Holiday_Hours

          'Select Case e.TelestaffProfileType
          '    Case TelestaffProfileType.Dispatch, TelestaffProfileType.Field
          '        ' The purpose of this bit of code is to handle holidays as they occur during the pay period.
          '        ' The Field staff can use the holiday as soon as they get it, so even though they might
          '        ' have 0 hours banked, they accrue one during the pay period and use it later in the pay period.
          '        Dim modifier As Integer = 24 ' field is the default
          '        If e.TelestaffProfileType = TelestaffProfileType.Dispatch Then modifier = 12

          '        Dim i As Integer = (From h In getHolidayList(e.EmployeeData.PayPeriodStart.Year) Where h >= e.EmployeeData.PayPeriodStart _
          '                                And h < e.EmployeeData.PayPeriodStart.AddDays(14) Select h).Count
          '        .Banked_Holiday -= (i * modifier) ' Remove the holidays that were added.
          'End Select

          .Banked_Sick = e.EmployeeData.Banked_Sick_Hours
          .ScheduledHours = e.Scheduled_Hours
          .FinPlusPayrate = e.EmployeeData.Base_Payrate
          .TelestaffPayrate = p
          .StaffEmployee = e.IsOvertime_Calculated_Weekly
          .Vacation = e.Vacation.TotalHours(p)
          .Sick = e.Sick.TotalHours(p)
          .Holiday = e.Holiday_Paid.TotalHours(p)
          If e.IsExempt Then
            .Regular = 80 - e.Vacation.TotalHours - e.Sick.TotalHours
            .Scheduled_Regular_Overtime = 0
            .Scheduled_Overtime = 0
            .Absent_Without_Pay = 0
            .Unscheduled_Regular_Overtime = 0
            .Unscheduled_Overtime = 0
            .Unscheduled_Double_Overtime = 0
            .Holiday_Time_Banked = 0
            .Holiday_Time_Used = 0
            .Comp_Time_Banked = 0
            .Comp_Time_Used = 0
          Else
            .Regular = e.Regular.TotalHours(p)
            .Scheduled_Regular_Overtime = e.Scheduled_Regular_Overtime.TotalHours(p)
            .Scheduled_Overtime = e.Scheduled_Overtime.TotalHours(p)
            .Absent_Without_Pay = e.Leave_Without_Pay.TotalHours(p)
            .Unscheduled_Regular_Overtime = e.Unscheduled_Regular_Overtime.TotalHours(p)
            .Unscheduled_Overtime = e.Unscheduled_Overtime.TotalHours(p)
            .Unscheduled_Double_Overtime = e.Unscheduled_Double_Overtime.TotalHours(p)
            .Holiday_Time_Banked = e.Holiday_Time_Banked.TotalHours(p)
            .Holiday_Time_Used = e.Holiday_Time_Used.TotalHours(p)
            .Comp_Time_Banked = e.Comp_Time_Banked.TotalHours(p)
            .Comp_Time_Used = e.Comp_Time_Used.TotalHours(p)
            .Comp_Time_Banked += e.BC_Comp_Time_Banked.TotalHours(p)
            .Comp_Time_Used += e.BC_Comp_Time_Used.TotalHours(p)
          End If
          ' Handling Thanksgiving Holidays on 12/3/2014
          'Select Case e.TelestaffProfileType
          '    Case TelestaffProfileType.Dispatch
          '        .Holiday += 24
          '    Case TelestaffProfileType.Field
          '        .Holiday += 48
          'End Select
          'If e.EmployeeData.Banked_Holiday_Hours > 0 Then
          '    Select Case e.EmployeeData.EmployeeId
          '        Case 1093
          '            .Holiday = e.EmployeeData.Banked_Holiday_Hours - e.Holiday_Time_Used.TotalHours - 48
          '        Case 1112, 1472, 2247
          '            .Holiday = e.EmployeeData.Banked_Holiday_Hours - e.Holiday_Time_Used.TotalHours - 24
          '        Case Else
          '            .Holiday = e.EmployeeData.Banked_Holiday_Hours - e.Holiday_Time_Used.TotalHours
          '    End Select
          'Else
          'End If
        End With
        eo.Add(neweo)
      Next
    Next
    Return eo
  End Function

  Public Function Get_All_Timecard_Exceptions(payperiodstart As Date) As List(Of TimecardTimeException)
    ' Get Telestaff exceptions first
    Dim TCEList As New List(Of TimecardTimeException)
    'Dim epplist As List(Of EPP) = GetPublicSafetyEmployeeData_EPP(startDate)
    'For Each e In epplist

    '    For Each w In e.WarningList
    '        TCEList.Add(New TimecardTimeException(e.EmployeeData, "Warning", w))
    '    Next

    '    For Each el In e.ErrorList
    '        TCEList.Add(New TimecardTimeException(e.EmployeeData, "Error", el))
    '    Next

    'Next
    Dim CIP As New CacheItemPolicy
    CIP.AbsoluteExpiration = Now.AddSeconds(30)
    Dim key As String = "timecardsbyppd," & payperiodstart.ToShortDateString
    Dim tmpTC As List(Of GenericTimecard) = GetTimeCards(payperiodstart)
    For Each t In tmpTC

      For Each w In t.WarningList
        TCEList.Add(New TimecardTimeException(t.employeeID, t.employeeName, "Warning", w, t.departmentNumber))
      Next

      For Each w In t.ErrorList
        TCEList.Add(New TimecardTimeException(t.employeeID, t.employeeName, "Error", w, t.departmentNumber))
      Next

    Next
    Return TCEList
  End Function

  Public Function Get_Work_Days(ShiftType As String, PayPeriodStart As Date) As List(Of Date)
    'CASE WHEN DATEDIFF(mi, '9/6/2006 8:00:00 AM', calltime) % 4320 BETWEEN 0 AND 1439 ")
    '.AppendLine("THEN LEFT(U.station,2) + ' Shift A' ")
    '.AppendLine("WHEN DATEDIFF(mi, '9/6/2006 8:00:00 AM', calltime) % 4320 BETWEEN 1440 AND 2879 ")
    '.AppendLine("THEN LEFT(U.station,2) + ' Shift B' ")
    '.AppendLine("WHEN DATEDIFF(mi, '9/6/2006 8:00:00 AM', calltime) % 4320 BETWEEN 2880 AND 4319 
    Dim dList As New List(Of Date)

    Dim StartDate As Date = "9/6/2006"

    Select Case ShiftType.ToUpper
      Case "A", "B", "C"
        For a As Integer = 0 To 13
          Select Case PayPeriodStart.AddDays(a).Subtract(StartDate).TotalDays Mod 3
            Case 0
              If ShiftType = "A" Then dList.Add(PayPeriodStart.AddDays(a))
            Case 1
              If ShiftType = "B" Then dList.Add(PayPeriodStart.AddDays(a))
            Case 2
              If ShiftType = "C" Then dList.Add(PayPeriodStart.AddDays(a))
          End Select
        Next

      Case "DA", "NA"
        Dim workDays() As String = {"1", "1", "0", "0", "0", "1", "1", "0", "0", "1", "1", "1", "0", "0"}
        For a As Integer = 0 To 13
          If workDays(PayPeriodStart.AddDays(a).Subtract(StartDate).TotalDays Mod 14) = "1" Then
            dList.Add(PayPeriodStart.AddDays(a))
          End If
        Next

      Case "DB", "NB"
        Dim workDays() As String = {"1", "1", "1", "0", "0", "1", "1", "0", "0", "0", "1", "1", "0", "0"}
        StartDate = CType("9/8/2006", Date)
        For a As Integer = 0 To 13
          If workDays(PayPeriodStart.AddDays(a).Subtract(StartDate).TotalDays Mod 14) = "1" Then
            dList.Add(PayPeriodStart.AddDays(a))
          End If
        Next


      Case Else
        ' Assume office. Return all week days until pay period end.
        For a As Integer = 0 To 13
          Select Case PayPeriodStart.AddDays(a).DayOfWeek
            Case DayOfWeek.Saturday, DayOfWeek.Sunday
            Case Else
              dList.Add(PayPeriodStart.AddDays(a))
          End Select
        Next

    End Select

    Return dList
  End Function

  Public Function Get_Cached_Timestore_Fields_By_ID() As Dictionary(Of Integer, Timestore_Field)
    Dim CIP As New CacheItemPolicy
    CIP.AbsoluteExpiration = Now.AddHours(12)
    Return myCache.GetItem("timestorefields_by_id", CIP)
  End Function

  Public Function Get_TimeStore_Fields_By_ID() As Dictionary(Of Integer, Timestore_Field)
    Dim tflist As New Dictionary(Of Integer, Timestore_Field)
    For Each kvp As KeyValuePair(Of String, Timestore_Field) In Get_Cached_Timestore_Fields_By_Name()
      tflist.Add(kvp.Value.Field_ID, kvp.Value)
    Next
    Return tflist
  End Function

  Public Function Get_Cached_Timestore_Fields_By_Name() As Dictionary(Of String, Timestore_Field)
    Dim CIP As New CacheItemPolicy
    CIP.AbsoluteExpiration = Now.AddHours(12)
    Return myCache.GetItem("timestorefields_by_name", CIP)
  End Function

  Public Function Get_TimeStore_Fields_By_Name() As Dictionary(Of String, Timestore_Field)
    Dim tflist As New Dictionary(Of String, Timestore_Field)
    With tflist
      .Add("WorkHours", New Timestore_Field(1, "WorkHours", "Work Hours", True))
      .Add("VacationHours", New Timestore_Field(2, "VacationHours", "Vacation", True))
      .Add("SickHours", New Timestore_Field(3, "SickHours", "Sick", True))
      .Add("SickLeavePoolHours", New Timestore_Field(14, "SickLeavePoolHours", "Sick Leave Pool", True))
      .Add("CompTimeUsed", New Timestore_Field(4, "CompTimeUsed", "Comp Time Used", True))
      .Add("AdminBereavement", New Timestore_Field(5, "AdminBereavement", "Admin - Bereavement Leave", True))
      .Add("AdminDisaster", New Timestore_Field(19, "AdminDisaster", "Admin - Disaster", True))
      .Add("AdminJuryDuty", New Timestore_Field(6, "AdminJuryDuty", "Admin - Jury Duty", True))
      .Add("AdminMilitaryLeave", New Timestore_Field(7, "AdminMilitaryLeave", "Admin - Military Leave", True))
      .Add("AdminWorkersComp", New Timestore_Field(8, "AdminWorkersComp", "Admin - Worker's Comp", True))
      .Add("AdminOther", New Timestore_Field(9, "AdminOther", "Admin - Other", True))
      .Add("OnCallMinimumHours", New Timestore_Field(10, "OnCallMinimumHours", "On Call - Minimum Hours", False))
      .Add("OnCallWorkHours", New Timestore_Field(11, "OnCallWorkHours", "On Call - Work Hours", False))
      .Add("OnCallWorkTimes", New Timestore_Field(12, "OnCallWorkTimes", "On Call - Work Times", False))
      .Add("OnCallTotalHours", New Timestore_Field(13, "OnCallTotalHours", "On Call - Total Hours by Date", True))
      .Add("LWOPSuspension", New Timestore_Field(15, "LWOPSuspension", "LWOP - Suspension", True))
      .Add("ScheduledLWOP", New Timestore_Field(16, "ScheduledLWOP", "Scheduled LWOP", True))
      .Add("SickFamilyLeave", New Timestore_Field(17, "SickFamilyLeave", "Family Sick Leave", True))
      .Add("TermHours", New Timestore_Field(18, "TermHours", "Term Hours", False))
    End With
    Return tflist
  End Function

  Public Function PopulateltrOrder() As Dictionary(Of String, Integer)
    Dim ltrOrder As New Dictionary(Of String, Integer)
    With ltrOrder
      '.Clear()
      .Add("002", 0) ' straight time aka regular
      .Add("006", 13)
      .Add("007", 13) ' Union Time Pool
      .Add("046", 12)
      .Add("090", 2) ' Leave without pay
      .Add("095", 2)
      .Add("100", 3) ' vacation
      .Add("101", 3)
      .Add("110", 4)
      .Add("111", 4) ' sick 
      .Add("118", 10) ' Comp Time accrued for BCs
      .Add("119", 11) ' Comp Time Used for BCs
      .Add("120", 10) ' Comp time accrued
      .Add("121", 11) ' Banked comp time used
      .Add("122", 10) ' Holiday Time Bank
      .Add("123", 11) ' Holiday time bank Hours Requested to be used
      .Add("130", 5) ' Scheduled Regular overtime
      .Add("131", 1) ' Scheduled OT
      .Add("134", 6) ' paid holiday
      .Add("230", 7) ' Unscheduled Regular OT
      .Add("231", 8) ' Unscheduled OT
      .Add("232", 9) ' Unscheduled Double OT
      .Add("300", 14)
      .Add("301", 15)
      .Add("302", 16)
      .Add("303", 17)
      .Add("777", 11) ' Disaster 1.5
    End With
    Return ltrOrder
  End Function

  Public Function PopulatePayCodes() As Dictionary(Of String, String)
    Dim p As New Dictionary(Of String, String)
    With p
      .Add("002", "Regular") ' straight time aka regular
      .Add("006", "Sick Leave Pool")
      .Add("007", "Union Time Pool") ' Union Time Pool
      .Add("046", "Vehicle")
      .Add("090", "LWOP") ' Leave without pay
      .Add("095", "Term Hours") ' Term hours, when someone is termed in the middle of the pay period
      .Add("100", "Vacation") ' vacation
      .Add("101", "Vacation")
      .Add("110", "Sick")
      .Add("111", "Sick") ' sick 
      .Add("118", "Comp Time Banked") ' Comp time accrued for BCs
      .Add("119", "Comp Time Used") ' for BCs
      .Add("120", "Comp Time Banked") ' Comp time accrued
      .Add("121", "Comp Time Used") ' Banked comp time used
      .Add("122", "Holiday Time Banked") ' Holiday Time Bank
      .Add("123", "Holiday Time Used") ' Holiday time bank Hours Requested to be used
      .Add("124", "Banked Holidays Paid")
      .Add("130", "Scheduled OT 1.0") ' Scheduled Regular overtime
      .Add("131", "Scheduled OT 1.5") ' Scheduled OT
      .Add("134", "Paid Holiday") ' paid holiday
      .Add("230", "Unscheduled OT 1.0") ' Unscheduled Regular OT
      .Add("231", "Unscheduled OT 1.5") ' Unscheduled OT
      .Add("232", "Unscheduled OT 2.0") ' Unscheduled Double OT
      '.Add("777", "Disaster 1.5")
      .Add("300", "Disaster Admin")
      .Add("301", "Disaster 1.0")
      .Add("302", "Disaster 1.5")
      .Add("303", "Disaster 2.0")
      .Add("800", "Ineligible Holiday")
    End With
    Return p
  End Function

  Function GetHours(ByRef cl As List(Of GenericTimecard.WorkType), Paycode As String) As String
    ' Needs to return a number with a decimal and 2 places, unless it's not found, then return nothing.
    If Paycode.Length = 0 Then
      Return String.Format("{0: N2}", (From c In cl Select c.hours).Sum)
    End If
    Dim tmp = (From c In cl Where c.payCode = Paycode Select c)
    If tmp.Count > 0 Then
      Dim h As Double = tmp.First.hours
      If h = 0 Then Return "" Else Return String.Format("{0:N2}", h)
    Else
      Return ""
    End If
  End Function

  Function GetHours(ByRef cl As List(Of GenericTimecard.WorkType), Paycode As String, Payrate As Double) As Double
    ' Needs to return a number with a decimal and 2 places, unless it's not found, then return nothing.
    If Paycode.Length = 0 Then
      Return (From c In cl Where c.payRate = Payrate Select c.hours).Sum
    End If
    Dim tmp = (From c In cl Where c.payCode = Paycode And c.payRate = Payrate Select c)
    If tmp.Count > 0 Then
      Return tmp.First.hours
    Else
      Return 0
    End If
  End Function

  Function GetHours(ByRef cl As List(Of GenericTimecard.WorkType), Paycode() As String, Payrate As Double) As Double
    ' Needs to return a number with a decimal and 2 places, unless it's not found, then return nothing.
    Dim tmp = (From c In cl Where Paycode.Contains(c.payCode) And c.payRate = Payrate Select c)
    If tmp.Count > 0 Then
      Return tmp.First.hours
    Else
      Return 0
    End If
  End Function

  Public Function GetFirstName(ByRef fd As List(Of FinanceData), eid As String) As String
    If eid = "" Then Return ""
    Dim tmp = (From f In fd Where f.EmployeeId = eid Select f)
    If tmp.Count > 0 Then
      Return tmp.First.EmployeeFirstName
    Else
      Return ""
    End If
  End Function

  Public Function GetLastName(ByRef fd As List(Of FinanceData), eid As String) As String
    If eid = "" Then Return ""
    Dim tmp = (From f In fd Where f.EmployeeId = eid Select f)
    If tmp.Count > 0 Then
      Return tmp.First.EmployeeLastName
    Else
      Return ""
    End If
  End Function

  Public Function Compare_Holiday_Request_To_Timecard(ByRef hr As HolidayRequest,
                                                      ByRef tc As GenericTimecard) As Boolean
    ' This function is going to return true if the holiday request matches
    ' what is currently saved on the timecard, and false if there are any changes.

    Dim checkList As New List(Of Boolean)
    Dim choiceTypes As String() = {"Paid", "Bank", "Ineligible"}

    checkList.Add(hr.BankedHolidaysPaid = tc.BankedHoursPaid)

    For Each choice In choiceTypes
      Dim tct = (From t In tc.HolidayHoursChoice
                 Where t = choice
                 Select t).Count
      Dim hrt = (From h In hr.CurrentHolidayChoice
                 Where h = choice
                 Select h).Count
      checkList.Add(tct = hrt)
    Next
    Return Not checkList.Contains(False)

    'Dim tc_PaidCount = (From t In tc.HolidayHoursChoice
    '                    Where t = "Paid"
    '                    Select t).Count

    'Dim tc_BankCount = (From t In tc.HolidayHoursChoice
    '                    Where t = "Bank"
    '                    Select t).Count

    'Dim hr_PaidCount = (From h In hr.CurrentHolidayChoice
    '                    Where h = "Paid"
    '                    Select h).Count



    'Dim hr_BankCount = (From h In hr.CurrentHolidayChoice
    '                    Where h = "Bank"
    '                    Select h).Count

    'Return ((tc_PaidCount = hr_PaidCount) And
    '  (tc_BankCount = hr_BankCount) And
    '  (hr.BankedHolidaysPaid = tc.BankedHoursPaid))

  End Function




End Module

