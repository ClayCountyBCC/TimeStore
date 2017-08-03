@*@ModelType IEnumerable(Of TimeStore.Models.GenericTimecard)*@
@ModelType IEnumerable(Of TimeStore.Models.Crosstab)

@code
    Dim qsCurrent As String = ""
    Dim qsPrev As String = ""
    Dim i As Integer = 0
    If Not Request.QueryString("ppd") Is Nothing Then
        i = Request.QueryString("ppd")
        qsPrev = "/TimeStore/main/crosstab?ppd=" & i - 1
        qsCurrent = "/TimeStore/main/crosstab?ppd=" & i
    Else
        qsPrev = "/TimeStore/main/crosstab?ppd=-1"
        qsCurrent = "/TimeStore/main/crosstab?ppd=0"
    End If
    If Not Request.QueryString("et") Is Nothing Then
        qsPrev &= "&et=" & Request.QueryString("et")
        qsCurrent &= "&et=" & Request.QueryString("et")
    End If

    Dim ppdStart As Date = Model.First.PayPeriodStart
    'GetPayPeriodStart(Today.AddDays(i * 14))
    Dim sbTmp As New StringBuilder
    With sbTmp
        .Append("<tr>")
        For a As Integer = 0 To 23
            .Append("<td>&nbsp;</td>")
        Next
        .Append("</tr>")
    End With
    Dim BlankLine As String = sbTmp.ToString
End code

  <md-toolbar class="short-toolbar md-accent">
    <div class="md-toolbar-tools short-toolbar" layout="row" layout-align="center center">
      <md-button href="@qsPrev">
        <h4>Previous Payperiod</h4>
      </md-button>
      <h4 flex>
        Pay Period Starting Date: @ppdStart
      </h4>
      <md-button href="@qsCurrent">
        <h4>Current Payperiod</h4>
      </md-button>
    </div>
  </md-toolbar>
  <table class="myTable shading crosstab">
    <thead>
      <tr>
        <th>
          orgn
        </th>
        <th>
          emplno
        </th>
        <th>
          lastname
        </th>
        <th>
          firstname
        </th>
        <th>
          reg
        </th>
        <th>
          006/7
        </th>
        <th>
          046
        </th>
        <th>
          090
        </th>
        <th>
          095
        </th>
        <th>
          100
        </th>
        <th>
          101
        </th>
        <th>
          110
        </th>
        <th>
          111
        </th>
        <th>
          120
        </th>
        <th>
          121
        </th>
        <th>
          122
        </th>
        <th>
          123
        </th>
        <th>
          124
        </th>
        <th>
          130
        </th>
        <th>
          131
        </th>
        <th>
          134
        </th>
        <th>
          230
        </th>
        <th>
          231
        </th>
        <th>
          232
        </th>
        <th>
          Total
        </th>
      </tr>
    </thead>
    @For Each item In Model
      @<tr>
        <td>
          @item.Orgn_d
        </td>
        <td>
          @If item.EmployeeID = "" Then
            @item.EmployeeID_d
          Else
            @item.EmployeeID_d
            @*@<a href="/TimeStore/#/e/@item.EmployeeID">
                  @item.EmployeeID_d
              </a>*@
          End If

        </td>
        <td>
          @Html.DisplayFor(Function(modelItem) item.LastName)
          @*<a href="/TimeStore/#/e/@item.EmployeeID">
                @Html.DisplayFor(Function(modelItem) item.LastName)
            </a>*@
        </td>
        <td>
          @Html.DisplayFor(Function(modelItem) item.FirstName)
          @*<a href="/TimeStore/#/e/@item.EmployeeID">
                @Html.DisplayFor(Function(modelItem) item.FirstName)
            </a>*@
        </td>
        <td>
          @item.Regular_d
        </td>
        <td>
          @If item.pc006_d = "" Then
            @item.pc007_d
          Else
            @item.pc006_d
          End If          
        </td>
        <td>
          @item.pc046_d
        </td>
        <td>
          @item.pc090_d
        </td>
         <td>
           @item.pc095_d
         </td>
        <td>
          @item.pc100_d
        </td>
        <td>
          @item.pc101_d
        </td>
        <td>
          @item.pc110_d
        </td>
        <td>
          @item.pc111_d
        </td>
        <td>
          @item.pc120_d
        </td>
        <td>
          @item.pc121_d
        </td>
        <td>
          @item.pc122_d
        </td>
        <td>
          @item.pc123_d
        </td>
        <td>
          @item.pc124_d
        </td>
        <td>
          @item.pc130_d
        </td>
        <td>
          @item.pc131_d
        </td>
        <td>
          @item.pc134_d
        </td>
        <td>
          @item.pc230_d
        </td>
        <td>
          @item.pc231_d
        </td>
        <td>
          @item.pc232_d
        </td>
        <td>
          @item.Total_d
        </td>
      </tr>
            @If item.EmployeeID = "" Then
              @Html.Raw(BlankLine)
            End If
    Next

  </table>








