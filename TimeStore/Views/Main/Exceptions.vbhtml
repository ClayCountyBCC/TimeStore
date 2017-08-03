@ModelType TimeStore.Models.Timecard_Access
@code
    Dim qsCurrent As String = ""
    Dim qsPrev As String = ""
    Dim ppd As String = ""
    Dim ppEnd As String = CType(Model.PayPeriodDisplayDate, Date).AddDays(13).ToString("yyyyMMdd")
    If Not Request.QueryString("ppd") Is Nothing Then
        Dim i As Integer = Request.QueryString("ppd")
        ppd = "/ppd/" & ppEnd
        qsPrev = "/TimeStore/main/exceptions?ppd=" & i - 1
        qsCurrent = "/TimeStore/main/exceptions?ppd=" & i

    Else
        ppd = "/ppd/" & ppEnd
        qsPrev = "/TimeStore/main/exceptions?ppd=-1"
        qsCurrent = "/TimeStore/main/exceptions?ppd=0"
    End If
    If Not Request.QueryString("et") Is Nothing Then
        qsPrev &= "&et=" & Request.QueryString("et")
        qsCurrent &= "&et=" & Request.QueryString("et")
    End If


End code

<md-toolbar class="short-toolbar md-accent">
    <div class="md-toolbar-tools short-toolbar" layout="row" layout-align="center center">
        <md-button href="@qsPrev">
            <h4>Previous Payperiod</h4>
        </md-button>
        <h4 flex>
            Pay Period Starting Date: @Model.PayPeriodDisplayDate
        </h4>
        <md-button href="@qsCurrent">
            <h4>Current Payperiod</h4>
        </md-button>
    </div>
</md-toolbar>

    <table class="myTable" border="1">
        <tr>
            <th>Employee ID</th>
            <th>Employee Name</th>
            <th>Department</th>
            <th>Type</th>
            @*<th>Start Time</th>
            <th>End Time</th>
            <th>Total Hours</th>
            <th>Pay Rate</th>
            <th>Job</th>
            <th>Shift Duration</th>
            <th>Shift Type</th>
            <th>Work Type</th>*@
            <th>Message</th>
        </tr>
    
    @For Each item In Model.TimecardTimeExceptionList
        @<tr>
            <td><a href="/TimeStore/#/e/@item.EmployeeId@ppd">@item.EmployeeId</a></td>
            <td><a href="/TimeStore/#/e/@item.EmployeeId@ppd">@item.EmployeeName</a></td>
             <td>@item.Department</td>
            <td>@item.ExceptionType</td>
            @*<td>@item.StartTime.ToString</td>
            <td>@item.EndTime.ToString</td>
            <td>@item.TotalHours</td>
            <td>@item.PayRate</td>
            <td>@item.JobDescription</td>
            <td>@item.ShiftDuration</td>
            <td>@item.ShiftType</td>
            <td>@item.WorkType</td>*@
            <td>@item.Message</td>
        </tr>
    Next
    @*@code
        Dim Shifts() As String = {"A", "B", "C", "DA", "DB", "NA", "NB"}
        Dim PPD As Date = CType("3/11/2015", Date)
        
    End Code
    @For Each shift In Shifts
        @code
        Dim dList As List(Of Date) = Get_Work_Days(shift, PPD)
        End Code
        @For Each d As Date In dList
            @<tr>
                <td>@shift</td>
                <td>@d.ToShortDateString</td>
                <td></td><td></td>
            </tr>
        Next
        
    Next*@   

    
    </table>
