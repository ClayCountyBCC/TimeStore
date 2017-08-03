@ModelType TimeStore.Models.Timecard_Access

@code
    Dim Header = "<tr><th>1703</th><th>Regular (002)</th><th>Overtime (131)</th><th>Absent No Pay (090)</th><th>Vac (101)</th><th>Sick (111)</th><th>ROT (130)</th><th>HOL (134)</th><th>Unsched Reg OT (230)</th><th>Unsched OT 50% (231)</th><th>Nonsched DOT (232)</th><th>Holiday Time Bank (122)</th><th>Holiday Time Used (123)</th></tr>"
    Dim a As Integer = 0
    Dim CurrentDept As String = "1703"
End Code
<md-toolbar class="md-accent short-toolbar">
    <div class="md-toolbar-tools  short-toolbar" layout="row" layout-align="center center">
        <md-button href="/TimeStore/main/index?ppd=-1">
            <h4>Previous Payperiod</h4>
        </md-button>
        <h4 flex>
            Pay Period Starting Date: @Model.PayPeriodDisplayDate
        </h4>
        <md-button href="/TimeStore/main/exceptions?ppd=0">
            <h4>Current Payperiod</h4>
        </md-button>
    </div>
</md-toolbar>
    <div> 
        <table>
            <thead>
                <tr>
                    @Html.Raw(Header)
                </tr>
            </thead>
            <tbody>
                @For Each p In Model.EmployeeOutputList
                    @code
                    If CurrentDept <> p.Department Then
                        a = 1
                        Header = Header.Replace(CurrentDept, p.Department)
                        Select Case p.Department
                            Case "2102", "2103"
                                ' Need to also replace the other codes as needed, vacation, sick
                                Header = Header.Replace("Sick (111)", "Sick (110)")
                                Header = Header.Replace("Vac (101)", "Vac (100)")
                                Header = Header.Replace("Holiday Time Bank (122)", "COMP Time Accrued (120)")
                                Header = Header.Replace("Holiday Time Used (123)", "COMP Used (121)")
                        End Select
                        CurrentDept = p.Department
                        @Html.Raw(Header)
                    Else
                        @If a > 7 Then
                            @Code
                            a = 1
                            End Code
                            @Html.Raw(Header)
                        Else
                            a = a + 1
                        End If
                    End If
                    End Code
                    @<tr>
                         <td>
                             <table class="employeeinfo">
                                 <tr>
                                     <td class="employeenumber_header">Emp Number</td>
                                     <td colspan="3" class="employeenumber">@p.EmployeeId</td>
                                     <td class="vacation_banked_header">Vac</td>
                                     <td class="vacation_banked">@p.Banked_Vacation.ToString</td>
                                 </tr>
                                 <tr>
                                     <td colspan="4" class="employeename">@p.EmployeeName</td>
                                     <td class="sick_header">Sick</td>
                                     <td class="sick">@p.Banked_Sick</td>
                                 </tr>
                                 <tr>
                                     <td class="hiredate_header">Hire Date</td>
                                     <td colspan="3" class="hiredate">@p.HireDate.ToShortDateString</td>
                                     <td class="holiday_header">@IIf(p.StaffEmployee = True, "Comp", "Holiday")</td>
                                     <td class="holiday">@IIf(p.StaffEmployee = True, p.Banked_Comp, p.Banked_Holiday)</td>
                                 </tr>
                                 <tr>
                                     <td class="scheduledhours_header">Sch Hours</td>
                                     <td class="scheduledhours">@p.HoursNeededForOvertime</td>
                                     <td class="payrate_header">Fin-Rate</td>
                                     <td class="payrate">@p.FinplusPayrate</td>
                                     <td class="payrate_header">T-Rate</td>
                                     <td class="payrate">@p.TelestaffPayrate</td>
                                </tr>
                            </table>
                        </td>
                        <td>@p.Regular</td>
                        <td>@IIf(p.Scheduled_Overtime > 0, p.Scheduled_Overtime, "")</td>
                        <td>@IIf(p.Absent_Without_Pay > 0, p.Absent_Without_Pay, "")</td>
                        <td>@IIf(p.Vacation > 0, p.Vacation, "") </td>
                        <td>@IIf(p.Sick > 0, p.Sick, "") </td>
                        <td>@IIf(p.Scheduled_Regular_Overtime > 0, p.Scheduled_Regular_Overtime, "") </td>
                        <td>@IIf(p.Holiday > 0, p.Holiday, "")</td>
                        <td>@IIf(p.Unscheduled_Regular_Overtime > 0, p.Unscheduled_Regular_Overtime, "") </td>
                        <td>@IIf(p.Unscheduled_Overtime > 0, p.Unscheduled_Overtime, "")</td>
                        <td>@IIf(p.Unscheduled_Double_Overtime > 0, p.Unscheduled_Double_Overtime, "")</td>
                        <td>@IIf(p.StaffEmployee = True, IIf(p.Comp_Time_Banked > 0, p.Comp_Time_Banked, ""), IIf(p.Holiday_Time_Banked > 0, p.Holiday_Time_Banked, ""))</td>
                        <td>@IIf(p.StaffEmployee = True, IIf(p.Comp_Time_Used > 0, p.Comp_Time_Used, ""), IIf(p.Holiday_Time_Used > 0, p.Holiday_Time_Used, ""))</td>
                    </tr>
                Next
            </tbody>
        </table>
    </div>
    <style>
        body {
            font-size: larger;
        }

        table {
            border-collapse: collapse;
        }

        table, th, td {
            border-collapse: collapse;
            border: 1px solid black;
            text-align: center;
        }

        td {
            height: 2em;
        }

        tr:nth-of-type(odd) {
            background-color: lightyellow;
        }

        table.employeeinfo {
            width: 100%;
        }

        .name {
            font-size: larger;
            text-align: left;
        }

        .employeenumber_header, .hiredate_header, .vacation_banked_header, .sick_header, .scheduledhours_header, .payrate_header, .holiday_header {
            font-size: x-small;
            text-align: left;
        }

        .employeenumber, .hiredate, .vacation_banked, .sick, .scheduledhours, .payrate, .holiday {
            font-size: x-small;
            text-align: left;
        }
        /*th:nth-of-type(even), td:nth-of-type(even){
                /*background-color: #eee;
            }*/
        /*.sot {
                background-color: yellow;
            }
            .usot{
                background-color: orange;
            }
            .absent{
                background-color: lightblue;
            }
            .vacation{
                background-color: lightgreen;
            }
            .sick {
                background-color: lightsalmon;
            }
            .holiday{
                background-color: cyan;
            }*/
        tr:hover {
            background-color: lightblue;
        }

        tr.error {
            background-color: lightcoral;
        }

            tr.error:nth-last-of-type(even) {
                background-color: #eee;
            }
    </style>

