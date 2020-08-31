@*@Modeltype TimeStore.Models.Timecard_Access*@
@code
  Dim LatestVer As String = "V202008250900"
  Dim MaterialVer As String = "1.4" ' was 1.1
  Dim AngularVer As String = "1.5.11" ' was 1.4.2

end code
<!DOCTYPE html>
<html ng-app="timestoreApp">
<head>
  <link rel="icon" type="image/png" href="~/images/ic_alarm_black_48dp_2x.png" />
  <meta http-equiv="X-UA-Compatible" content="IE=11">
  <meta name="viewport" content="width=device-width, initial-scale=1, user-scalable=no" />
  <title>@ViewData("Title")</title>
  <link href="~/CSS/bundle.css?v=@LatestVer" rel="stylesheet" />
  <script src="//ajax.googleapis.com/ajax/libs/angularjs/@AngularVer/angular.min.js"></script>
  <base href="~/">
</head>
<body ng-cloak>

  <div ng-controller="HeaderController">
    <md-toolbar>
      <div class="md-toolbar-tools"
           layout="row"
           layout-align="center center">

        <a style="cursor: pointer;"
           class="md-display-1"
           ng-click="switchUser()">
          Timestore
          <md-tooltip md-direction="bottom">
            click here to go to the home menu
          </md-tooltip>
        </a>
        <span flex></span>
        <md-button ng-show="myAccess.PasswordExpiringSoon"
                   ng-click="openPasswordExpirationMenu()"
                   aria-label="Password Expiring Soon"
                   class="md-fab md-mini md-hue-3">
          <md-icon aria-label="collection icon"
                   md-svg-src="images/warning-24px.svg">
          </md-icon>
          <md-tooltip md-direction="bottom">
            Your network password is going to expire on {{ myAccess.PasswordExpirationDate }}.
          </md-tooltip>
        </md-button>
        <md-button aria-label="View Paystubs"
                   ng-click="viewPaystubs()"
                   class="md-fab md-mini">
          <svg class="fabWhite"
               xmlns="http://www.w3.org/2000/svg" viewBox="0 0 640 512" height="100%" width="100%" fit="" preserveAspectRatio="xMidYMid meet">
            <path d="M608 32H32C14.33 32 0 46.33 0 64v384c0 17.67 14.33 32 32 32h576c17.67 0 32-14.33 32-32V64c0-17.67-14.33-32-32-32zM176 327.88V344c0 4.42-3.58 8-8 8h-16c-4.42 0-8-3.58-8-8v-16.29c-11.29-.58-22.27-4.52-31.37-11.35-3.9-2.93-4.1-8.77-.57-12.14l11.75-11.21c2.77-2.64 6.89-2.76 10.13-.73 3.87 2.42 8.26 3.72 12.82 3.72h28.11c6.5 0 11.8-5.92 11.8-13.19 0-5.95-3.61-11.19-8.77-12.73l-45-13.5c-18.59-5.58-31.58-23.42-31.58-43.39 0-24.52 19.05-44.44 42.67-45.07V152c0-4.42 3.58-8 8-8h16c4.42 0 8 3.58 8 8v16.29c11.29.58 22.27 4.51 31.37 11.35 3.9 2.93 4.1 8.77.57 12.14l-11.75 11.21c-2.77 2.64-6.89 2.76-10.13.73-3.87-2.43-8.26-3.72-12.82-3.72h-28.11c-6.5 0-11.8 5.92-11.8 13.19 0 5.95 3.61 11.19 8.77 12.73l45 13.5c18.59 5.58 31.58 23.42 31.58 43.39 0 24.53-19.05 44.44-42.67 45.07zM416 312c0 4.42-3.58 8-8 8H296c-4.42 0-8-3.58-8-8v-16c0-4.42 3.58-8 8-8h112c4.42 0 8 3.58 8 8v16zm160 0c0 4.42-3.58 8-8 8h-80c-4.42 0-8-3.58-8-8v-16c0-4.42 3.58-8 8-8h80c4.42 0 8 3.58 8 8v16zm0-96c0 4.42-3.58 8-8 8H296c-4.42 0-8-3.58-8-8v-16c0-4.42 3.58-8 8-8h272c4.42 0 8 3.58 8 8v16z" />
          </svg>
          <md-tooltip md-direction="bottom">
            View your Pay Stubs
          </md-tooltip>
        </md-button>
        <md-button ng-show="myAccess.Data_Type==='timecard'"
                   aria-label="View Leave Requests"
                   ng-click="viewLeaveRequests()"
                   class="md-fab md-mini">
          <md-icon aria-label="collection icon"
                   md-svg-src="images/ic_collections_bookmark_24px.svg">
          </md-icon>
          <md-tooltip md-direction="bottom">
            Your Leave Requests
          </md-tooltip>
        </md-button>
        <md-button ng-show="myAccess.Data_Type==='timecard'"
                   aria-label="view Calendar"
                   ng-click="viewCalendar()"
                   class="md-fab md-mini">
          <svg class="fabWhite"
               viewBox="0 0 24 24" height="100%" width="100%" xmlns="http://www.w3.org/2000/svg" fit="" preserveAspectRatio="xMidYMid meet" style="pointer-events: none; display: block;">
            <path d="M19 3h-1V1h-2v2H8V1H6v2H5c-1.11 0-1.99.9-1.99 2L3 19c0 1.1.89 2 2 2h14c1.1 0 2-.9 2-2V5c0-1.1-.9-2-2-2zm0 16H5V8h14v11zM7 10h5v5H7z"></path>
          </svg>
          <md-tooltip md-direction="bottom">
            Leave Calendar
          </md-tooltip>
        </md-button>

        <md-autocomplete ng-show="showSearch === true && employeelist.length > 0"
                         flex="20"
                         id="employeeList"
                         md-items="employee in querySearch(employeeSearchText, employeeMapV)"
                         md-item-text="employee.display"
                         md-selected-item="employee"
                         md-search-text="employeeSearchText"
                         md-selected-item-change="selectedEmployeeChanged(employee)"
                         md-min-length="2"
                         placeholder="Search by Name or Employee Id">
          <md-item-template>
            <span ng-class="{termedEmployee: employee.Terminated}">
              <span md-highlight-text="employeeSearchText">{{ employee.display }}</span>
              <span ng-if="employee.Terminated">
                <span style="margin-left: 1em;">Termed: {{ employee.TerminationDateDisplay }}</span>
              </span>
            </span>
          </md-item-template>
          <md-not-found>
            No matches found.
          </md-not-found>
        </md-autocomplete>
        <md-button ng-click="openApprovalMenu()" ng-if="myAccess.Raw_Access_Type > 1">
          Approvals
        </md-button>
        <md-button ng-click="openAdminMenu()" ng-if="myAccess.Backend_Reports_Access">
          Admin
        </md-button>
      </div>
    </md-toolbar>

    <md-sidenav md-component-id="adminRight" class="md-sidenav-right md-whiteframe-z2">
      <section>
        <md-toolbar class="md-primary">
          <div class="md-toolbar-tools">
            Timestore Controls
          </div>
        </md-toolbar>
        <md-list layout="column">
          <md-list-item ng-if="myAccess.CanChangeAccess === true">
            <md-button ng-click="ViewAccessMenu()">
              Control Access
            </md-button>
          </md-list-item>
          <md-list-item>
            <md-button ng-click="viewFinanceTools()">
              Finance Tools
            </md-button>
          </md-list-item>
          <md-list-item>
            <md-button ng-click="viewPayrollProcess()">
              Payroll Process
            </md-button>
          </md-list-item>
          <md-list-item>
            <md-button ng-if="myAccess.Data_Type==='timecard'" href="main/crosstab">
              View Crosstab
            </md-button>
          </md-list-item>
          <md-list-item>
            <md-button href="Reports/">
              View Custom Reports
            </md-button>
          </md-list-item>
          <md-list-item>
            <md-button ng-click="viewUnapproved()">
              View Unapproved
            </md-button>
          </md-list-item>
          <md-list-item>
            <md-button ng-click="viewFema()">
              View Fema Report
            </md-button>
          </md-list-item>
        </md-list>
        <md-toolbar class="md-accent">
          <div class="md-toolbar-tools">
            Public Safety Controls
          </div>
        </md-toolbar>
        <md-list layout="column">

          <md-list-item>
            <md-button ng-click="viewIncentives(1)">
              Telestaff Incentives
            </md-button>
          </md-list-item>
        </md-list>
      </section>
    </md-sidenav>

    <md-sidenav md-component-id="approvalRight"
                class="md-sidenav-right md-whiteframe-z2">
      <section>
        <md-toolbar class="md-primary">
          <div class="md-toolbar-tools" ng-if="myAccess.Data_Type==='timecard' || myAccess.Raw_Access_Type > 2">
            Leave Requests
          </div>
        </md-toolbar>
        <md-list layout="column" ng-if="myAccess.Data_Type==='timecard' || myAccess.Raw_Access_Type > 2">
          <md-list-item>
            <md-button ng-click="viewLeaveApproval()">
              Approve Leave Requests
            </md-button>
          </md-list-item>
        </md-list>
        <md-toolbar class="md-accent">
          <div class="md-toolbar-tools">
            Approve Time
          </div>
        </md-toolbar>
        <md-list layout="column">
          <md-list-item>
            <md-button ng-click="viewExceptions()">
              View Exceptions
            </md-button>
          </md-list-item>
          <md-list-item>
            <md-button ng-click="viewFinalApprovals()">
              Handle Approvals
            </md-button>
          </md-list-item>
          <md-list-item>
            <md-button ng-click="viewSignatureRequired()">
              View Signature Required
            </md-button>
          </md-list-item>
          <md-list-item>
            <md-button ng-click="viewTimeclockData()">
              Timeclock Daily View
            </md-button>
          </md-list-item>
          <md-list-item ng-if="myAccess.Data_Type==='telestaff' || myAccess.Raw_Access_Type > 6">
            <md-button ng-click="viewDailyCheckoff()">
              Daily Checkoff
            </md-button>
          </md-list-item>
        </md-list>
      </section>
    </md-sidenav>

    <md-sidenav md-component-id="passwordExpiring" class="md-sidenav-right md-whiteframe-z2">
      <section>
        <md-toolbar class="md-primary">
          <div class="md-toolbar-tools">
            Password is Expiring
          </div>
        </md-toolbar>
        <md-list layout="column">
          <md-list-item style="margin-bottom: 1em;">
            Your Windows password is set to expire on {{ myAccess.PasswordExpirationDate }}.
          </md-list-item>
          <md-list-item style="margin-bottom: 1em;">
            <p>To change your password:</p>
          </md-list-item>
          <md-list-item style="margin-bottom: 1em;">
            <p>From a county computer, please do a control-alt-delete</p>
          </md-list-item>
          <md-list-item style="margin-bottom: 1em;">
            <p>Choose <strong>Change A Password</strong> from the menu</p>
          </md-list-item>
          <md-list-item style="margin-bottom: 1em;">
            <p>You must be logged in to a County Computer as yourself in order to change your password.</p>
          </md-list-item>
          <md-list-item style="margin-bottom: 1em;">
            <p>
              Your password's expiration date is only checked once a day. After your password has been changed, this message should no longer show up the following day.
            </p>
          </md-list-item>
        </md-list>
      </section>
    </md-sidenav>

  </div>
  @RenderBody()
  <script src="//ajax.googleapis.com/ajax/libs/angularjs/@AngularVer/angular-cookies.min.js"></script>
  <script src="//ajax.googleapis.com/ajax/libs/angularjs/@AngularVer/angular-aria.min.js"></script>
  <script src="//ajax.googleapis.com/ajax/libs/angularjs/@AngularVer/angular-animate.min.js"></script>
  <script src="//ajax.googleapis.com/ajax/libs/angularjs/@AngularVer/angular-route.min.js"></script>
  <script src="~/Scripts/bundle.js?v=@LatestVer"></script>

  @Html.Partial("~/Views/Shared/bundle.vbhtml")
</body>
    
</html>
