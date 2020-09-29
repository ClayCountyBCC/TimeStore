<script type="text/ng-template" id="Access.tmpl.html">
  <div flex="100" layout="row" layout-align="center start" layout-wrap layout-margin>
    <div flex="40" layout="row" layout-align="center start" layout-wrap layout-padding layout-margin>

      <md-autocomplete ng-if="employeeList.length > 0"
                       flex="100"
                       id="employeeList"
                       md-items="employee in querySearch(employeeSearchText, employeeMapV)"
                       md-item-text="employee.display"
                       md-selected-item="employee"
                       md-search-text="employeeSearchText"
                       md-selected-item-change="selectedEmployeeChanged(employee)"
                       md-autofocus="true"
                       placeholder="Search by Name or Employee Id to begin">
        <span md-highlight-text="employeeSearchText">{{employee.display}}</span>
      </md-autocomplete>

      <div flex="100" class="md-whiteframe-z1" layout-wrap layout="row" layout-align="start center">
        <md-input-container flex="100">
          <label>Access Level</label>
          <md-select aria-label="Select Access Level"
                     flex="90"
                     id="AccessTypes"
                     ng-disabled="employee === null"
                     ng-model="timecardAccess.Raw_Access_Type">
            <md-select-label>Employee Access Level {{ accessLevels[timecardAccess.Raw_Access_Type] }}</md-select-label>
            <md-option ng-repeat="al in accessLevels" ng-value="{{$index}}">
              {{ al }}
            </md-option>
          </md-select>
        </md-input-container>
        <md-input-container flex="100">
          <label>Data Type</label>
          <md-select aria-label="Select Data Type"
                     flex="90" id="dataTypeList"
                     ng-disabled="employee === null"
                     ng-model="timecardAccess.Data_Type">
            <md-select-label>Employee Data Location {{timecardAccess.Data_Type}}</md-select-label>
            <md-option ng-value="dt" ng-repeat="dt in dataTypes">
              {{ dt }}
            </md-option>
          </md-select>
        </md-input-container>

        <md-checkbox flex="100"
                     id="BackendReportAccess"
                     ng-disabled="employee === null"
                     ng-model="timecardAccess.Backend_Reports_Access">
          Does this user have access to the Backend Reports?
        </md-checkbox>
        <md-divider flex="100"></md-divider>
        <md-checkbox flex="100"
                     ng-disabled="employee === null"
                     id="ApprovalRequired"
                     ng-model="timecardAccess.RequiresApproval">
          Does this user require approval?
        </md-checkbox>
        <md-divider flex="100"></md-divider>
        <md-checkbox flex="100"
                     id="CanChangeAccess"
                     ng-disabled="employee === null"
                     ng-model="timecardAccess.CanChangeAccess">
          Can this user change Timestore Access?
        </md-checkbox>
        <md-divider flex="100"></md-divider>
        <md-input-container flex="100">
          <label>Reports To</label>
          <md-select aria-label="Select Reports To"
                     flex="90"
                     id="reportsToList"
                     ng-disabled="employee === null"
                     ng-model="timecardAccess.ReportsTo">
            <md-select-label>User Reports to:  {{ GetEmployeeDisplay() }}</md-select-label>
            <md-option ng-value="0">No Specific User</md-option>
            <md-option ng-value="{{emp.EmployeeID}}" ng-repeat="emp in reportsToList | orderBy:'EmployeeDisplay'">
              {{ emp.EmployeeDisplay }}
            </md-option>
          </md-select>
        </md-input-container>
        <md-input-container flex="100">
          <label>Payroll Access</label>
          <md-select flex="90"
                     aria-label="Select Payroll Access"
                     id="payrollAccessList"
                     ng-disabled="employee === null"
                     ng-model="timecardAccess.PayrollAccess">
            <md-option ng-value="$index" ng-repeat="pra in payrollAccess track by $index">
              {{ pra }}
            </md-option>
          </md-select>
        </md-input-container>


          <md-divider flex="100"></md-divider>
          <div flex="100" layout="row" layout-padding layout-align="end center">
            <span flex></span>
            <md-button ng-disabled="employee === null"
                       class="md-primary md-raised md-whiteframe-z1"
                       ng-click="saveTimecardAccess()">
              SAVE User Access
            </md-button>
          </div>
</div>

    </div>


    <div flex="50" class="md-whiteframe-z1" layout-margin layout-wrap layout="row" layout-align="start center">
      <div flex="100"
           layout="row"
           layout-align="start center"
           class="Tall">
        <md-list flex="100">
          <md-list-item>
            <span class="indent" flex="100">Department</span>
            <md-divider flex="100"></md-divider>
          </md-list-item>

          <md-list-item>

            <md-checkbox ng-change="viewApproveAllChanged()"
                         id="viewApproveAll"
                         ng-disabled="timecardAccess.viewAll || employee === null || timecardAccess.Raw_Access_Type < 2"
                         ng-model="timecardAccess.approveAll">

            </md-checkbox>
            <div>
              All Departments - View And Approve
            </div>

            <md-divider flex="100"></md-divider>
          </md-list-item>

          <md-list-item>
            <md-checkbox id="ViewAll"
                         ng-disabled="timecardAccess.approveAll || employee === null || timecardAccess.Raw_Access_Type < 2"
                         ng-model="timecardAccess.viewAll">
              
            </md-checkbox>
            <div>
              All Departments - View Only
            </div>
            <md-divider flex="100"></md-divider>
          </md-list-item>

          <md-list-item ng-repeat="dept in departmentList">

            <md-checkbox ng-disabled="timecardAccess.viewAll || employee === null || timecardAccess.Raw_Access_Type < 2 || dept.disabled"
                         ng-model="dept.selected"
                         aria-label="{{dept.DepartmentDisplay}}"
                         name="chk-{{dept.DepartmentNumber}}">

            </md-checkbox>
            <div>
              {{dept.DepartmentDisplay}}
            </div>

            <md-divider flex="100"></md-divider>
          </md-list-item>
        </md-list>
      </div>
    </div>
  </div>
</script>
<script type="text/ng-template" id="AddTimeView.tmpl.html">

    <div flex="100"
         layout="row"
         layout-align="center start"
         layout-wrap
         layout-margin>

        <div flex="90"
             layout="row"
             layout-align="center center">

            <timecard-header flex="100" 
                             shortheader="true" 
                             timecard="timecard">
            </timecard-header>

        </div>

        <div flex="90"
             class="md-whiteframe-z1"
             layout="row"
             layout-align="center center"
             layout-wrap>

            <add-date-and-time flex="100" timecard="timecard"></add-date-and-time>


            <div layout="row"
                 layout-align="end center"
                 flex="100">
                <md-button ng-click="returnToTimeStore()"
                           class="md-raised md-primary">
                    Return to TimeStore
                </md-button>
            </div>
        </div>
    </div>
</script>
<script type="text/ng-template" id="AddTime.directive.tmpl.html">
  <form id="addTimeForm">

    <div style="margin-top: .5em;"
         id="workDateContainer"
         layout="row"
         layout-align="center center"
         flex="100"
         layout-wrap>

      <date-selector title="Work Date:"
                     label="Select Work Date"
                     datetype="day"
                     flex="100">
      </date-selector>

    </div>

    <div>
      <div style="margin-top: .75em; margin-bottom: .75em;"
           class="short-toolbar my-primary"
           flex="100"
           layout="row"
           layout-align="start center">
        <span flex="5"></span>
        <h5 ng-if="isCurrentPPD">
          Enter your time worked
        </h5>
        <h5 ng-if="!isCurrentPPD">
          Enter your Leave Requests
        </h5>
      </div>

      <div layout="row"
           layout-align="space-around start"
           layout-wrap
           flex="100">

        <div ng-show="isCurrentPPD"
             layout="row"
             layout-align="start center"
             flex="40"
             layout-wrap>
          <md-input-container flex="100">
            <label>Select Work Hours</label>
            <md-select md-on-close="calculateTotalHours()"
                       flex="100"
                       multiple
                       ng-model="TCTD.selectedTimes">
              <md-option ng-repeat="t in timeList track by t.index"
                         ng-value="t.index">
                {{t.display}}
              </md-option>
            </md-select>
          </md-input-container>
          <md-radio-group ng-change="updateTimeList()"
                          flex="100"
                          layout="row"
                          layout-align="space-around center"
                          class="smaller"
                          ng-model="timeListType">
            <md-radio-button ng-model="forceFullTimeList"
                             value="short">
              Short Time List
              <md-tooltip md-direction="top">
                Times from 6 AM to 6 PM
              </md-tooltip>
            </md-radio-button>
            <md-radio-button value="full">
              Full Time List
              <md-tooltip md-direction="top">
                Times from 12 AM to 12 PM
              </md-tooltip>
            </md-radio-button>
          </md-radio-group>
        </div>
        <div layout="column"
             layout-align="center center"
             layout-gt-md="row"
             layout-align-gt-md="space-around start"
             layout-wrap
             flex="55">
          <md-input-container ng-if="timecard.isPubWorks">
            <label>Lunch Start {{ TCTD.DepartmentNumber === "3701" || TCTD.DepartmentNumber === "3711" || TCTD.DepartmentNumber === "3712"  ? "(30 Mins)" : "(1 Hour)"}}</label>
            <md-select ng-disabled="lunchTimeList.length === 0 || timecard.selectedTimes.length === 0 || timecard.selectedTimes.length > 4"
                       md-on-close="calculateTotalHours()"
                       ng-model="TCTD.SelectedLunchTime">
              <md-option value="-1">No Lunch Taken</md-option>
              <md-option ng-repeat="t in lunchTimeList track by t.index"
                         ng-value="t.index">
                {{t.display}}
              </md-option>
            </md-select>
          </md-input-container>
          <hours-display tctd="TCTD" hours="TCTD.WorkHours"></hours-display>
          <hours-display tctd="TCTD" hours="TCTD.BreakCreditHours"></hours-display>
          <hours-display tctd="TCTD" hours="TCTD.TotalHours"></hours-display>
        </div>

        <div ng-if="showDisaster && TCTD.WorkHours.value > 0 && isCurrentPPD"
             layout="row"
             layout-align="start center"
             layout-wrap
             layout-padding
             flex="100">
          <p ng-if="disasterChoiceError.length > 0"
             flex="100"
             layout="row"
             class="warn">
            {{ disasterChoiceError }}
          </p>
          <md-input-container class="md-input-has-value"
                              flex="40">
            <label>Are any of the hours worked for special events?</label>
            <md-radio-group ng-change="DisasterHoursChoice()"
                            flex="100"
                            layout="row"
                            layout-align="space-around center"
                            ng-model="$parent.DisasterHoursRelated"
                            class="smaller">
              <md-radio-button ng-value="true" aria-label="Yes">
                Yes
              </md-radio-button>
              <md-radio-button ng-value="false" aria-label="No">
                No
                <md-tooltip md-direction="top">
                  If you choose No here, any information you've entered into the special events section will be removed.
                </md-tooltip>
              </md-radio-button>
            </md-radio-group>
          </md-input-container>
          <div layout="row"
               flex="60">

          </div>

        </div>

      </div>
    </div>

    <div ng-show="isCurrentPPD && showDisaster">
      <div ng-click="Toggle_DisasterHours()"
           style="margin-top: .25em; margin-bottom: .25em; cursor: pointer;"
           flex="100"
           class="short-toolbar my-primary"
           layout="row"
           layout-align="start center">
        <span flex="5"></span>
        <h5>
          Show / Hide -- Special Event hours worked
        </h5>
      </div>

      <div ng-show="ExpandDisasterHours"
           style="margin-top: .5em;"
           layout="row"
           layout-align="center center"
           layout-wrap           
           flex="100">

        <div flex="100"
             layout="row"
             layout-align="center center"
             layout-wrap
             layout-padding>


          <md-input-container class="md-input-has-value"
                              flex="40">
            <label>Are you normally scheduled to work on this date?</label>
            <md-radio-group ng-change="NormallyScheduledChoice()"
                            flex="100"
                            layout="row"
                            layout-align="space-around center"
                            ng-model="NormallyScheduled"
                            class="smaller">
              <md-radio-button ng-value="true" aria-label="Yes">
                Yes
              </md-radio-button>
              <md-radio-button ng-value="false" aria-label="No">
                No
              </md-radio-button>
            </md-radio-group>
          </md-input-container>

          <md-input-container ng-show="ShowDisasterNormallyScheduledHours"
                              class="md-input-has-value"
                              flex="60">
            <label>Select the number of hours you normally work on this date</label>
            <md-select ng-change="NormallyScheduledHoursSelected()"
                       ng-model="TCTD.DisasterNormalScheduledHours">
              <md-option ng-value="-1">Please Select</md-option>              
              <md-option ng-repeat="h in normallyScheduledHours track by $index"
                         ng-value="h">{{ h.toFixed(2)  }} hours</md-option>
            </md-select>
          </md-input-container>
          <div ng-show="!ShowDisasterNormallyScheduledHours"
               flex="60">

          </div>
          <p ng-if="normallyScheduledHoursError.length > 0"
             flex="100"
             layout="row"
             class="warn">
            {{ normallyScheduledHoursError }}
          </p>
        </div>
        <!--event: "=",
        fulltimelist: "<",
        eventerror: "=",
        validate: "&",
        calculate: "&"-->
        <div flex="100"
             layout="row"
             layout-align="center center"
             layout-wrap>


          <disaster-hours flex="100"
                          ng-repeat="ee in TCTD.EventsByWorkDate track by ee.event_id"
                          event="ee"
                          fulltimelist="fullTimeList"                          
                          calc="calculateTotalHours()"></disaster-hours>
        </div>
        <!--<md-input-container flex="40">
    <label>Disaster Hours Worked</label>
    <md-select md-on-close="calculateTotalHours()"
               flex="100"
               multiple
               ng-model="TCTD.disasterSelectedTimes"
               placeholder="Disaster Hours Worked"
               class="bigpaddingbottom">
      <md-option ng-repeat="t in fullTimeList track by t.index"
                 ng-value="t.index">
        <span class="md-text">
          {{t.display}}
        </span>
      </md-option>
    </md-select>
  </md-input-container>
  <div layout="column"
       layout-align="center center"
       layout-gt-md="row"
       layout-align-gt-md="space-around center"
       layout-wrap
       flex="55">
    <md-button ng-click="CopyWorkHoursToDisasterWorkHours()"
               class="md-primary md-raised">
      Copy Work Hours
    </md-button>
    <hours-display tctd="TCTD" hours="TCTD.DisasterWorkHours"></hours-display>
  </div>
  <div layout-padding
       layout="row"
       layout-wrap
       flex="100">
    <p ng-if="disasterTimeError.length > 0"
       flex="100"
       layout="row"
       class="warn">
      {{ disasterTimeError }}
    </p>
    <p>The section above titled "Enter your time worked" is should be filled out normally to reflect your shift start/end times and time selected for lunch. The section titled "Disaster Hours" is used to document and track the hours you worked within those hours towards the disaster. </p>
    <md-input-container flex="40">
      <label>Disaster Work Type</label>
      <md-select md-on-close="checkDisasterWorkType()"
                 flex="100"
                 ng-model="TCTD.DisasterWorkType"
                 placeholder="Disaster Work Type"
                 class="bigpaddingbottom">
        <md-option value="Debris Pickup">
          Debris Pickup
        </md-option>
        <md-option value="Debris Monitoring">
          Debris Monitoring
        </md-option>
        <md-option value="Call Center">
          Call Center
        </md-option>
        <md-option value="Working in EOC">
          Working in EOC
        </md-option>
        <md-option value="Building Repair">
          Building Repair
        </md-option>
        <md-option value="Road Repair">
          Road Repair
        </md-option>
        <md-option value="Other Repair">
          Other Repair
        </md-option>
        <md-option value="Prep for Storm">
          Prep for Storm
        </md-option>
        <md-option value="Not Listed">
          Not Listed
        </md-option>
      </md-select>
    </md-input-container>
  </div>-->
      </div>
    </div>

    <div flex="100"
         ng-if="timecard.isPubWorks">
      <div style="margin-top: .75em; margin-bottom: .75em;"
           class="short-toolbar my-accent"
           flex="100"
           layout="row"
           layout-align="start center">
        <span flex="5"></span>
        <h5>
          Add Out of Class Information
        </h5>
      </div>
      <span layout="row"
            layout-align="center center"
            flex="25">
        <md-checkbox ng-disabled="myAccess.Raw_Access_Type  === 1 || myAccess.EmployeeID === parseInt(timecard.employeeId)"
                     ng-change="calculateTotalHours()"
                     ng-model="TCTD.OutOfClass">
          Out of Class Pay
        </md-checkbox>
      </span>
    </div>
    <div ng-show="TCTD.showOnCall"
         ng-click="Toggle_OnCallHours()"
         style="margin-top: .25em; margin-bottom: .25em; cursor: pointer;"
         flex="100"
         class="short-toolbar my-accent"
         layout="row"
         layout-align="start center">
      <span flex="5"></span>
      <h5>
        On Call Hours - Show / Hide
      </h5>
    </div>

    <div ng-show="toggleOnCall"
         style="margin-top: .5em;"
         layout="row"
         layout-align="center center"
         layout-wrap
         flex="100">

      <md-input-container ng-show="isCurrentPPD"
                          flex="40">
        <label>On Call Hours Worked</label>
        <md-select md-on-close="calculateTotalHours()"
                   flex="40"
                   multiple
                   ng-model="TCTD.OnCallSelectedTimes"
                   placeholder="On Call Hours Worked"
                   class="bigpaddingbottom">
          <md-option ng-repeat="t in fullTimeList track by t.index"
                     ng-value="t.index">
            <span class="md-text">
              {{t.display}}
            </span>
          </md-option>
        </md-select>
      </md-input-container>
      <div layout="column"
           layout-align="center center"
           layout-gt-md="row"
           layout-align-gt-md="space-around center"
           layout-wrap
           flex="55">

        <hours-display tctd="TCTD" hours="TCTD.OnCallWorkHours"></hours-display>
        <hours-display tctd="TCTD" hours="TCTD.OnCallMinimumHours"></hours-display>
        <hours-display tctd="TCTD" hours="TCTD.OnCallTotalHours"></hours-display>

      </div>

    </div>

    <div flex="100">

      <div style="margin-top: .25em; margin-bottom: .25em;"
           flex="100"
           class="short-toolbar my-accent"
           layout="row"
           layout-align="start center">
        <span flex="5"></span>
        <h5 flex
            style="text-align: left;">
          Non-Working Hours
        </h5>
        <span style="text-align: center; margin-right: 1em;">
          Banked Vacation: {{ timecard.bankedVacation }}
          <span ng-if="vacationUsed > 0">
            ( {{ vacationUsed }} hours )
          </span>
        </span>
        <span style="text-align: center; margin-right: 1em;">
          Banked Sick: {{ timecard.bankedSick }}
          <span ng-if="sickUsed > 0">
            ( {{ sickUsed }} hours )
          </span>
        </span>
        <span style="text-align: center; margin-right: 1em;"
              ng-if="timecard.exemptStatus !== 'Exempt'">
          Banked Comp:{{ timecard.bankedComp }}
          <span ng-if="bankedCompWeek1 > 0">
            + {{ bankedCompWeek1 }} hours from Week 1
          </span>
          <span ng-if="compUsed > 0">
            ( {{ compUsed }} hours )
          </span>
        </span>
      </div>

      <div style="margin: .5em .5em .5em .5em;"
           layout="row"
           layout-align="start center"
           layout-wrap
           flex="100">

        <hours-display tctd="TCTD" hours="TCTD.VacationHours" calc="calculateTotalHours()"></hours-display>
        <hours-display tctd="TCTD" hours="TCTD.SickHours" calc="calculateTotalHours()"></hours-display>
        <hours-display tctd="TCTD" hours="TCTD.SickFamilyLeave" calc="calculateTotalHours()"></hours-display>
        <hours-display tctd="TCTD" hours="TCTD.CompTimeUsed" calc="calculateTotalHours()"></hours-display>
        <hours-display tctd="TCTD" hours="TCTD.LWOPHours" calc="calculateTotalHours()"></hours-display>
        <hours-display ng-show="!timecard.isFullTime"
                       tctd="TCTD" hours="TCTD.ScheduledLWOPHours" calc="calculateTotalHours()"></hours-display>
        <hours-display tctd="TCTD" hours="TCTD.LWOPSuspensionHours" calc="calculateTotalHours()"></hours-display>
        <hours-display tctd="TCTD" hours="TCTD.HolidayHours" calc="calculateTotalHours()"></hours-display>
        <hours-display tctd="TCTD" hours="TCTD.SickLeavePoolHours" calc="calculateTotalHours()"></hours-display>
        <hours-display tctd="TCTD" hours="TCTD.TermHours" calc="calculateTotalHours()"></hours-display>
      </div>


      <div>
        <div ng-click="Toggle_AdminHours()"
             style="margin-top: .5em; margin-bottom: .5em; cursor: pointer;"
             flex="100"
             class="short-toolbar my-accent"
             layout="row"
             layout-align="start center">
          <span flex="5"></span>
          <h5>
            Administrative Leave - Show / Hide
          </h5>
        </div>

        <div style="margin-top: .5em; margin-bottom: .5em;"
             ng-show="TCTD.showAdminHours === true"
             layout="row"
             layout-align="start center"
             layout-wrap
             flex="100">

          <hours-display tctd="TCTD" hours="TCTD.AdminBereavement" calc="calculateTotalHours()"></hours-display>
          <hours-display tctd="TCTD" hours="TCTD.AdminJuryDuty" calc="calculateTotalHours()"></hours-display>
          <hours-display tctd="TCTD" hours="TCTD.AdminMilitaryLeave" calc="calculateTotalHours()"></hours-display>
          <hours-display tctd="TCTD" hours="TCTD.AdminWorkersComp" calc="calculateTotalHours()"></hours-display>
          <hours-display ng-show="TCTD.AdminDisaster.value > 0" tctd="TCTD" hours="TCTD.AdminDisaster" calc="calculateTotalHours()"></hours-display>
          <md-input-container ng-repeat="ee in TCTD.EventsByWorkDate track by ee.event_id"
                              class="shortpaddingbottom shortInput">
            <label class="longerLabel"
                   style="width: auto;">{{ee.event_name}} Admin Hours</label>
            <input ng-model="ee.disaster_work_hours.DisasterAdminHours"
                   type="number"
                   min="0"
                   max="24"
                   step=".25" 
                   ng-change="calculateTotalHours()" />
          </md-input-container>
          <hours-display tctd="TCTD" hours="TCTD.AdminOther" calc="calculateTotalHours()"></hours-display>
          <hours-display tctd="TCTD" hours="TCTD.AdminHours" calc="calculateTotalHours()"></hours-display>

        </div>
      </div>

    </div>

    <div ng-if="!timecard.isPubWorks || myAccess.Raw_Access_Type > 1"
         layout="row"
         layout-align="space-around center"
         flex="100"
         layout-wrap>

      <hours-display tctd="TCTD" hours="TCTD.DoubleTimeHours" calc="calculateTotalHours()"></hours-display>

      <span ng-show="TCTD.showVehicle === true"
            layout="row"
            layout-align="center center"
            flex="25">
        <md-checkbox ng-true-value="1"
                     ng-false-value="0"
                     ng-model="TCTD.Vehicle">
          Take Home Vehicle
        </md-checkbox>
      </span>

    </div>
    <timecard-warnings flex="100"
                       alignheader="start"
                       headerclass="my-accent"
                       dl="warningList"
                       title="Warnings"></timecard-warnings>

    <timecard-warnings flex="100"
                       alignheader="start"
                       headerclass="warn"
                       dl="errorList"
                       title="Errors"></timecard-warnings>
    <div layout="row"
         layout-align="center center"
         layout-wrap
         flex="100">
      <md-input-container class="md-whiteframe-z1 margin-left-bottom"
                          flex="50">
        <label>
          <md-icon aria-label="comment icon"
                   md-svg-src="images/ic_insert_comment_24px.svg">
          </md-icon>
          Add a Comment for this day
        </label>
        <input ng-model-options="{ debounce: 1500 }"
               ng-model="TCTD.Comment"
               name="comment"
               ng-change="calculateTotalHours()"
               md-maxlength="100" />
      </md-input-container>
      <span flex></span>
      <md-button ng-click="resetTimes()"
                 class="md-raised">
        Reset
        <md-icon aria-label="reset data icon"
                 md-svg-src="images/ic_settings_backup_restore_24px.svg">
        </md-icon>
      </md-button>
      <div>
        <md-button ng-disabled="errorList.length > 0"
                   ng-click="saveTCTD()"
                   class="md-raised md-warn">
          Save
          <md-icon aria-label="save icon"
                   md-svg-src="images/ic_save_24px.svg">
          </md-icon>
        </md-button>
        <md-tooltip ng-if="errorList.length > 0">
          Unable to Save Changes while errors are present.
        </md-tooltip>
      </div>
    </div>
    <div layout="row"
         layout-align="end center"
         layout-wrap
         flex="100">
      {{ responseMessage }}
    </div>

  </form>
</script>
<script type="text/ng-template" id="AddCompTime.directive.tmpl.html">
  <div style="margin-top: .5em;"
       layout-fill
       layout="row"
       layout-align="center center"
       layout-wrap
       flex="100">
    <div ng-show="timecard.exemptStatus !== 'Exempt'"
         flex="100">
      <div class="short-toolbar my-accent"
           flex="100"
           layout="row"
           layout-align="center center">
        <h5>Week 1</h5>
      </div>
      <time-list flex="100" tl="timecard.calculatedTimeList_Week1"></time-list>

      <div class="short-toolbar my-accent"
           flex="100"
           layout="row"
           layout-align="center center">
        <h5>Week 2</h5>
      </div>
      <time-list flex="100" tl="timecard.calculatedTimeList_Week2"></time-list>
      <div class="warn"
           flex="100"
           layout-align="center center"
           layout="row">
        You can only bank a maximum of 32 hours of comp time.
      </div>
      <div layout="row"
           layout-align="start center"
           layout-wrap
           flex="95">
        <div layout="row"
             layout-align="start center"
             flex="100">
          <span style="text-align: center;"
                flex="25">
            Week 1 - {{ week1OT }} hours eligible:
          </span>
          <md-slider aria-label="Week 1 Overtime to Comp Time"
                     flex="50"
                     ng-disabled="week1OT <= 0"
                     md-discrete
                     min="0"
                     max="{{ week1OT }}"
                     step=".25"
                     ng-model="week1CompTimeEarned">
          </md-slider>
          <span layout="row"
                layout-align="center center"
                flex="20">
            {{ formatNumber(week1CompTimeEarned) }} hours ( {{ formatNumber(week1CompTimeEarned * 1.5) }} calculated hours )
          </span>
        </div>
        <div layout="row"
             layout-align="start center"
             flex="100">
          <span style="text-align: center;"
                flex="25">
            Week 2 - {{ week2OT }} hours eligible:
          </span>
          <md-slider aria-label="Week 2 Overtime to Comp Time"
                     flex="50"
                     ng-disabled="week2OT <= 0"
                     md-discrete
                     min="0"
                     max="{{ week2OT }}"
                     step=".25"
                     ng-model="week2CompTimeEarned">
          </md-slider>
          <span layout="row"
                layout-align="center center"
                flex="20">
            {{ formatNumber(week2CompTimeEarned) }} hours ( {{ formatNumber(week2CompTimeEarned * 1.5) }} calculated hours )
          </span>
        </div>
        <div layout="row"
             layout-align="center center"
             flex="100">
          <span layout="row"
                layout-align="end center"
                flex>
            {{ responseMessage }}
          </span>
          <span flex="5"></span>
          <md-button class="md-warn md-raised"
                     ng-click="SaveCompTime()">
            Save Comp Time
            <md-icon aria-label="save icon"
                     md-svg-src="images/ic_save_24px.svg">
            </md-icon>
          </md-button>
        </div>
      </div>
    </div>
    <div layout="row"
         layout-align="center center"
         flex="100"
         ng-if="timecard.exemptStatus === 'Exempt'">
      Exempt employees are ineligible for comp time.
    </div>
  </div>
</script>
<script type="text/ng-template" id="HoursDisplay.directive.tmpl.html">
    <md-input-container ng-show="::hours.visible"
                        class="shortpaddingbottom shortInput">
        <label class="longerLabel"
               style="width: auto;"
               >{{ ::hours.label }}</label>
        <input ng-model-options="{ debounce: 1500 }"
               ng-model="hours.value"
               ng-change="calc()"
               type="{{ ::hours.type }}"
               min="{{ ::hours.min }}"
               max="{{ ::hours.max }}"
               step="{{ ::hours.step }}" 
               ng-disabled="hours.disabled"/>
    </md-input-container>
</script>
<script type="text/ng-template" id="TimeclockView.controller.tmpl.html">

  <date-selector title="Work Date:"
                 label="Select Work Date"
                 datetype="day"
                 flex="100">
  </date-selector>
  <div layout="row"
       layout-align="start center"
       layout-wrap
       flex="100">
    <div layout="row"
         layout-align="center center"
         class="short-toolbar my-accent"
         flex-gt-md="50"
         flex="100">
      <h5 style="text-align: center;">
        Supervisor:
      </h5>
    </div>
    <div flex="50"
         layout="row"
         layout-align="center center">
      <div layout="row"
           layout-align="center center"
           flex="100">

        <md-input-container flex="50">
          <label>Select A Supervisor</label>
          <md-select md-on-close="showTimeclockData()"
                     flex="100"
                     ng-model="supervisor">
            <md-option ng-repeat="s in supervisors"
                       ng-value="s">
              {{s}}
            </md-option>
          </md-select>
        </md-input-container>

      </div>
    </div>
  </div>
  <div flex-offset="5"
       flex="90"
       layout="row"
       layout-align="start center"
       layout-wrap
       ng-hide="supervisor === ''">
    <md-list layout="row"
             layout-align="start center"
             layout-wrap
             flex="100">
      <md-list-item flex="100"
                    class="md-no-proxy">
        <span flex="100"
              layout="row"
              layout-align="center center">
          <span flex="10">
            Dept
          </span>
          <span flex="20">
            Name
          </span>
          <span flex="35">
            Work Times
          </span>
          <span flex="15">
            Any Non Working Hours
          </span>
          <span class="centerme"
                flex="10">
            Start Time Issue
          </span>
          <span class="centerme"
                flex="10">
            End Time Issue
          </span>
        </span>
        <md-divider></md-divider>
      </md-list-item>
      <md-list-item flex="100"
                    class="md-no-proxy"
                    ng-repeat="f in filteredData">
        <span flex="100"
              layout="row"
              layout-align="center center">
          <span flex="10">
            {{f.department_id}}
          </span>
          <span flex="20">
            <a style="text-decoration: underline; cursor: pointer;"
               layout="row"
               layout-align="start center"
               ng-click="addTimeGo(f.employee_id)"
               flex>
              {{f.employee_name}}
            </a>

          </span>
          <span flex="35">
            {{f.work_times}}
          </span>
          <span flex="15">
            {{f.non_working_hours}}
          </span>
          <span flex="10">
            {{ f.punch_in_issue}}
          </span>
          <span flex="10">
            {{ f.punch_out_issue}}
          </span>
        </span>
        <md-divider></md-divider>
      </md-list-item>
    </md-list>


  </div>

</script>

<script type="text/ng-template" id="Approval.tmpl.html">
  <div flex="100"
       layout="row"
       layout-align="center center"
       layout-wrap>

    <md-progress-linear ng-if="showProgress === true"
                        flex="100"
                        md-mode="indeterminate">
    </md-progress-linear>

    <date-selector title="Pay Period Ending:"
                   label="Select Pay Period"
                   datetype="ppd"
                   flex="100">
    </date-selector>

    <h3 flex="100"
        layout="row"
        layout-align="center center">
      {{ Title }}
    </h3>
    <h3 ng-if="timeData.length === 0"
        flex="100"
        layout="row"
        layout-align="center center">
      {{ Message }}
    </h3>

    <md-select aria-label="View By Department or Custom Group"
               flex="50"
               id="viewGroups"
               ng-show="timeData.length > 0"
               placeholder=""
               ng-model="selectedGroup">
      <md-select-label flex="100">
        <span style="text-align: center;"
              flex="100">
          View By Department or Custom Group {{ selectedGroup }}
        </span>
      </md-select-label>
      <md-optgroup label="Departments">
        <md-option ng-repeat="dept in getDepts() | orderBy:dept track by dept"
                   ng-value="dept"
                   ng-click="groupSelected(dept)">
          {{ dept }}
        </md-option>
      </md-optgroup>

      <md-optgroup label="Custom Groups">
        <md-option ng-repeat="group in getGroups() | orderBy:group track by group"
                   ng-value="group"
                   ng-click="groupSelected(group)">
          {{ group }}
        </md-option>
      </md-optgroup>
    </md-select>

    <md-button class="md-raised md-primary"
               ng-click="RefreshApprovalData()">
      Refresh Data
    </md-button>

    <div ng-if="currentPage !== null"
         flex="100"
         layout="row"
         layout-align="space-around center"
         layout-padding>
      <md-button ng-click="loadPage(-1)"
                 ng-disabled="currentPage === 1"                 
                 class="md-raised md-primary"
                 layout="row"
                 layout-align="center center">
        Previous Page
      </md-button>
      <md-button ng-click="loadPage(1)"
                 ng-disabled="currentPage === totalPages"                 
                 class="md-raised md-primary"
                 layout="row"
                 layout-align="center center">
        Next Page
      </md-button>
    </div>

    <md-list ng-if="filteredData.length > 0"
             layout="row"
             layout-align="center center"
             flex="100"
             layout-wrap>

      <md-item ng-repeat="t in filteredData"
               flex="100"
               layout="row"
               layout-align="center center"
               layout-wrap>

        <md-item-content flex="100"
                         layout="row"
                         layout-align="center center">

          <div flex="100"
               layout="row"
               layout-align="start center"
               layout-wrap
               layout-padding>

            <div flex="100"
                 layout="row"
                 layout-align="center center"
                 layout-wrap
                 ng-class="{ myprimaryhue1: t.Approval_Level == 0, myprimary: t.Approval_Level == 1 }">
              <span flex="40">
                <md-button class="ButtonAsLink" ng-click="loadTimeCard(t.employeeID)">
                  {{ t.EmployeeDisplay }}
                </md-button>
              </span>
              <span flex="30">
                {{ getApprovalText(t.Approval_Level, t.IsLeaveApproved) }}
              </span>
              <span flex></span>
              <md-button ng-if="t.Approval_Level === 1 && t.IsLeaveApproved === true"
                         class="md-raised md-warn"
                         ng-click="approveTime(t.employeeID)">
                Approve Time
              </md-button>
              <md-checkbox ng-if="(approvalType === 'I' && t.Approval_Level === 1) || (approvalType !== 'I' && t.Approval_Level === 2)"
                           ng-disabled="true"
                           class="veryshort makeWhite"
                           ng-model="t.Approved">
                Approved
              </md-checkbox>
              <md-divider flex="100"></md-divider>
            </div>

            <timecard-detail flex="100"
                             timecard="t"
                             ng-if="t.showTimecard === true">
            </timecard-detail>
            <md-divider flex="100" ng-if="t.showTimecard === true"></md-divider>
            
            <div ng-repeat="wtl in t.approvalTimeList"
                 flex="30"
                 layout="row"
                 layout-align="center center"
                 layout-wrap>

              <span flex="80" layout="row" layout-align="end center">
                {{wtl.name}}
              </span>
              <span flex="20" layout="row" layout-align="center center">
                {{wtl.hours}}
              </span>

            </div>

            <md-divider flex="100"></md-divider>

            <div flex="100"
                 layout="row"
                 layout-align="center center"
                 layout-wrap
                 class="my-accent">
              <span flex></span>
              <span flex="30"
                    layout="row"
                    layout-align="end center">
                Total Hours
              </span>
              <span flex="10"
                    layout="row"
                    layout-align="center center">
                {{getTotalHours(t.approvalTimeList)}}
              </span>

            </div>
          </div>
        </md-item-content>

      </md-item>

    </md-list>

    <div ng-if="currentPage !== null"
         flex="100"
         layout="row"
         layout-align="space-around center"
         layout-padding>
      <md-button ng-click="loadPage(-1)"
                 ng-disabled="currentPage === 1"                
                 class="md-raised md-primary"
                 layout="row"
                 layout-align="center center">
        Previous Page
      </md-button>
      <md-button ng-click="loadPage(1)"
                 ng-disabled="currentPage === totalPages"                 
                 class="md-raised md-primary"
                 layout="row"
                 layout-align="center center">
        Next Page
      </md-button>
    </div>

  </div>
</script>
<script type="text/ng-template" id="DateSelect.tmpl.html">

  <div layout="row"
       layout-align="start center"
       layout-wrap
       flex="100">
    <div layout="row"
         layout-align="center center"
         class="short-toolbar my-accent"
         flex-gt-md="50"
         flex="100">
      <h5 style="text-align: center;">
        {{ title }}
      </h5>
    </div>
    <div flex="50"
         layout="row"
         layout-align="center center">
      <div layout="row"
           layout-align="center center"
           flex="100">


        <md-button ng-disabled="allowPrevious === false"
                   aria-label="previous day"
                   ng-click="dateChange(prevDate)"
                   class="md-fab md-mini">
          <md-tooltip>
            View Previous {{ datetypeDisplay }}
          </md-tooltip>
          <md-icon aria-label="previous day icon"
                   md-svg-src="images/ic_navigate_before_24px.svg"></md-icon>
        </md-button>
        <h4 style="margin-left: .5em; margin-right: .5em; margin-top: 0; margin-bottom: 0;"
              ng-if="showDayOfWeek">
          {{ dayOfWeek }}
        </h4>
        <md-select ng-show="datetype === 'ppd'"
                   style="margin-left: 8px !important; margin-right: 8px !important; min-width: 200px;"
                   class="min"
                   ng-model="selectedPayPeriodDate"
                   md-on-close="dateChange()"
                   placeholder="{{ selectedDate }}"
                   aria-label="Select Date">
          <md-option ng-repeat="d in dateList track by $index"
                     ng-value="d">
            <span style="text-align: center;">
              {{ d }}
            </span>
          </md-option>
        </md-select>
        <md-datepicker ng-hide="datetype === 'ppd'"
                       style="background-color: cornsilk;"
                       md-min-date="minDate"
                       md-max-date="maxDate"
                       ng-change="dateChange()"
                       ng-model="selectedWorkDate">

        </md-datepicker>
        <md-button aria-label="next day"
                   ng-click="dateChange(nextDate)"
                   class="md-fab md-mini">
          <md-tooltip>
            View Next {{ datetypeDisplay }}
          </md-tooltip>
          <md-icon aria-label="next day icon"
                   md-svg-src="images/ic_navigate_next_24px.svg"></md-icon>
        </md-button>


      </div>
    </div>
  </div>
</script>
<script type="text/ng-template" id="existingLeaveRequest.directive.tmpl.html">

    <div layout="row"
         layout-align="center center"         
         flex="100">

        <div class=""
             flex="90"
             layout="row"
             layout-align="start center"
             layout-wrap>
            <div class="my-accent padding"
                 layout="row"
                 layout-align="start center"
                 flex="100">
                <span style="text-align: center;"
                      flex="25">
                    Date
                </span>
                <span flex="25">
                    Type
                </span>
                <span style="text-align: center;"
                      flex="25">
                    Hours
                </span>
                <span flex="25">
                    Status
                </span>
            </div>
            <div class="padding"
                 ng-if="requests.length > 0"
                 ng-repeat="r in requests track by $index"
                 layout="row"
                 layout-align="center center"
                 layout-wrap
                 flex="100">
                <span style="text-align: center;"
                      flex="25">
                    {{ ::r.work_date_display }}
                </span>
                <span flex="25">
                    {{ ::r.field.Field_Display }}
                </span>
                <span style="text-align: center;"
                      flex="25">
                    {{ ::r.hours_used }}
                </span>
                <span ng-if="!r.Finalized"
                      ng-click="showDetail($index)"
                      flex="25">
                    <span class="undecidedLeave">
                        Undecided
                    </span>
                </span>
                <span ng-if="r.Finalized && r.Approved"
                      ng-click="showDetail($index)"
                      flex="25">
                    <span class="approvedLeave">
                        Approved
                        <md-icon aria-label="approved icon"
                                 md-svg-src="images/ic_done_24px.svg">
                        </md-icon>
                    </span>
                </span>
                <span ng-if="r.Finalized && !r.Approved"
                      ng-click="showDetail($index)"
                      flex="25">
                    <span class="deniedLeave">
                        Denied
                        <md-icon aria-label="denied icon"
                                 md-svg-src="images/ic_clear_24px.svg">
                        </md-icon>
                    </span>
                </span>

              <div style="margin-top: .5em;"
                   ng-if="r.comment.length > 0"
                   layout="row"
                   layout-align="start center"
                   flex="100">
                <span style="text-align: right;"
                      flex="20">
                  Comment:
                </span>
                <span flex="5"></span>
                <span style="text-align: left;"
                      flex>
                  {{ r.comment}}
                </span>
              </div>
              <div style="margin-top: .5em;"
                   ng-if="r.note.length > 0"
                   layout="row"
                   layout-align="start center"
                   flex="100">
                <span style="text-align: right;"
                      flex="20">
                  Decision Note:
                </span>
                <span flex="5"></span>
                <span style="text-align: left;"
                      flex>
                  {{ r.note}}
                </span>
              </div>
              <div style="margin-top: .5em;"
                   ng-if="r.Finalized"
                   layout="row"
                   layout-align="start center"
                   flex="100">
                <span style="text-align: right;"
                      flex="20">
                  Approved By:
                </span>
                <span flex="5"></span>
                <span flex>
                  {{ r.by_username }}
                </span>
              </div>
              <div style="margin-top: .5em;"
                   ng-if="r.Finalized"
                   layout="row"
                   layout-align="start center"
                   flex="100">
                <span style="text-align: right;"
                      flex="20">
                  Approved On:
                </span>
                <span flex="5"></span>
                <span style="text-align: left;"
                      flex>
                  {{ r.approval_date_display }}
                </span>
              </div>
                <md-divider class="padding"
                            flex="100"></md-divider>
            </div>
            <div layout="row"
                 layout-align="start start"                 
                 flex="100"
                 ng-if="requests.length === 0">
                No future leave requests found.
            </div>

        </div>
    </div>

</script>

<script type="text/ng-template" id="LeaveRequestView.controller.tmpl.html">


    <div style="margin-top: .75em;"
         layout="row"
         layout-align="center center"
         layout-wrap
         flex="100">
        <div layout="row"
             layout-align="center center"
             layout-wrap
             flex="90">

            <md-toolbar flex="100">
                <div class="md-toolbar-tools" layout="row" layout-align="center center" layout-margin>
                    <span class="md-display-1"
                          layout="row"
                          layout-align="center center"
                          flex>
                        Your Leave Requests
                    </span>

                    <div layout="row"
                         layout-align="end center"
                         flex="15">
                        <md-button ng-click="goHome()"
                                   class="md-raised md-warn">
                            TimeStore
                            <md-icon aria-label="return home icon"
                                     md-svg-src="images/ic_replay_24px.svg">
                            </md-icon>
                        </md-button>
                    </div>

                </div>
            </md-toolbar>

            <div layout="row"
                 layout-align="space-between center"
                 layout-wrap
                 flex="100">
                <md-radio-group ng-change="switchData()"
                                flex="50"
                                layout="row"
                                layout-align="space-around center"
                                class="smaller"
                                ng-model="leaveRequestListType">
                    <md-radio-button value="short">
                        Current And Future Requests
                        <md-tooltip md-direction="bottom">
                            This option shows your leave requests for this pay period and later.
                        </md-tooltip>
                    </md-radio-button>
                    <md-radio-button value="full">
                        All Leave Requests
                        <md-tooltip md-direction="bottom">
                            This option shows all of your leave requests, even those in the past.
                        </md-tooltip>
                    </md-radio-button>
                </md-radio-group>
                
                <div>
                    <label class="longerLabel"
                           layout="row"
                           layout-align="center center"
                           flex="100">
                        Pick a date to Request Leave
                    </label>
                    <div style="background-color: cornsilk;"
                         class="md-whiteframe-z1">
                        <md-datepicker style="background-color: cornsilk;"
                                       md-min-date="minDate"
                                       md-max-date="maxDate"
                                       ng-change="leaveDateSelected()"
                                       ng-model="selectedDate">

                        </md-datepicker>
                    </div>
                </div>
                
                <md-button ng-click="refreshData()"
                           class="md-primary md-raised">
                    Refresh Leave Data
                </md-button>
            </div>
        </div>
        <existing-leave-request flex="100" requests="leaveRequests">
        </existing-leave-request>
    </div>
</script>

<script type="text/ng-template" id="DailyCheckoff.tmpl.html">

    <div flex="100"
         layout="row"
         layout-align="center center"
         layout-wrap>

        <md-progress-linear ng-if="showProgress === true"
                            flex="100"
                            md-mode="indeterminate">
        </md-progress-linear>

        <md-button class="md-raised md-primary"
                   ng-click="switchData()">
            {{ swapButtonText }}
        </md-button>
        <md-button class="md-raised md-primary"
                   ng-click="RefreshApprovalData()">
            Refresh Data
        </md-button>

        <h3 flex="100"
            layout="row"
            layout-align="center center">
            {{ title }}.
        </h3>

        <md-list ng-if="dataView.length > 0"
                 layout="row"
                 layout-align="center center"
                 flex="100"
                 layout-wrap>

            <md-item ng-repeat="t in dataView | orderBy: ['group', 'name']"
                     flex="100"
                     layout="row"
                     layout-align="center center"
                     layout-wrap>

                <md-item-content flex="100"
                                 layout="row"
                                 layout-align="center center">

                    <div flex="100"
                         layout="row"
                         layout-align="start center"
                         layout-wrap
                         layout-padding>
                        <span flex="20">
                            {{ t.group }}
                        </span>
                        <span flex="20">
                            <a href="#/e/{{ t.eid }}/ppd/{{ ppe }}">
                                {{ t.name }}
                            </a>
                        </span>
                        <span flex="20">
                            {{ t.time }}
                        </span>
                    </div>
                </md-item-content>

            </md-item>

        </md-list>

    </div>
</script>
<script type="text/ng-template" id="Exceptions.tmpl.html">

    <div layout="row"
         layout-align="center center"
         layout-wrap
         flex="100">

        <md-progress-linear ng-if="showProgress === true"
                            flex="100"
                            md-mode="indeterminate">
        </md-progress-linear>

        <div style="margin-top: .5em; margin-bottom: .5em;"
             class="md-whiteframe-z1 ExceptionsHeader"
             layout="row"
             layout-align="center center"
             layout-wrap
             flex="90">

            <div style="cursor: pointer;"
                 ng-click="toggleShowOptions()"
                 flex="100"
                 class="short-toolbar my-accent"
                 layout="row"
                 layout-align="center center">
                <h5>
                    View / Hide Exception Options
                </h5>
            </div>

            <div ng-show="showOptions === true"
                 layout="row"
                 layout-wrap
                 layout-align="center center"
                 flex="100">
                
                <date-selector title="Pay Period Ending:"
                               label="Select Pay Period"
                               datetype="ppd"
                               flex="100">
                </date-selector>

                <md-select aria-label="View By Department"
                           flex="50"
                           placeholder="View By Department"
                           ng-model="selectedDept">
                    <md-select-label flex="100">
                        <span style="text-align: center;"
                              flex="100">
                            View By Department {{ selectedDept }}
                        </span>
                    </md-select-label>
                    <md-option value="">
                        ALL DEPARTMENTS
                    </md-option>
                    <md-option ng-repeat="dept in deptData | orderBy:dept.deptName"
                                ng-value="dept.deptId">
                        {{ dept.deptName }}
                    </md-option>
                </md-select>

                <md-button class="md-raised md-primary"
                           ng-click="RefreshApprovalData()">
                    Refresh Data
                </md-button>

                
                <div flex="100">
                    <md-radio-group layout="row"
                                    layout-align="center center"
                                    layout-wrap
                                    flex="100"
                                    ng-model="filterExceptions">
                        <md-radio-button value="Warning">
                            Warnings Only
                        </md-radio-button>
                        <md-radio-button value="Error">
                            Errors Only
                        </md-radio-button>
                        <md-radio-button value="both">
                            Both Errors and Warnings
                        </md-radio-button>
                    </md-radio-group>
                </div>

            </div>

        </div>
        <div ng-show="filterByType().length === 0 && viewData.length > 0"
             layout="row"
             layout-align="center center"
             flex="90">
            No records found matching that criteria.
        </div>
        <table ng-show="viewData.length > 0 && filterByType().length > 0"
               class="myTable" border="1">
          <thead>
            <tr>
              <th>Employee ID</th>
              <th>Employee Name</th>
              <th>Department</th>
              <th>Type</th>
              <th>Message</th>
            </tr>
          </thead>
            <tr ng-repeat="v in filterByType() track by $index">
                <td>
                    <a href="#/e/{{v.employeeId}}/ppd/{{ppd}}">
                        {{ v.employeeId }}
                    </a>
                </td>
                <td>
                    <a href="#/e/{{v.employeeId}}/ppd/{{ppd}}">
                        {{ v.fullname }}
                    </a>
                </td>
                <td>
                    {{ v.departmentId }}
                </td>
                <td>
                    {{ v.exceptionType }}
                </td>
                <td>
                    {{ v.exception }}
                </td>
            </tr>
        </table>
    </div>
</script>
<script type="text/ng-template" id="FinanceTools.tmpl.html">
  <div layout="row"
       layout-align="center center"
       flex="100"
       layout-wrap>
    <md-progress-linear ng-if="showProgress === true"
                        flex="100"
                        md-mode="indeterminate">
    </md-progress-linear>

    <div layout="row"
         layout-align="center center"
         layout-padding
         layout-wrap
         flex="90">
      <md-radio-group flex="100"
                      layout="row"
                      layout-align="center center"
                      class="smaller"
                      ng-model="serverType">
        <md-radio-button value="normal">
          Normal Finplus Server
        </md-radio-button>
        <md-radio-button value="training">
          Training Database
        </md-radio-button>
        <!--<md-radio-button value="specialdisaster">
        Special Disaster Payrun - Production
      </md-radio-button>
      <md-radio-button value="specialdisasterqa">
        Special Disaster Payrun - Training
      </md-radio-button>-->
      </md-radio-group>
    </div>
    <md-radio-group flex="100"
                    layout="row"
                    layout-align="center center"
                    class="smaller"
                    ng-model="projectCode">
      <md-radio-button value="">
        No Project Code
      </md-radio-button>
      <md-radio-button value="CORVIRUS">
        COVID-19
      </md-radio-button>
      <!--<md-radio-button value="specialdisaster">
      Special Disaster Payrun - Production
    </md-radio-button>
    <md-radio-button value="specialdisasterqa">
      Special Disaster Payrun - Training
    </md-radio-button>-->
    </md-radio-group>
  </div>
      <div layout="row"
           layout-align="center center"
           layout-padding
           layout-wrap
           flex="90">
        <md-select aria-label="Select Pay Period"
                   flex="40"
                   ng-model="selectedPayPeriod">
          <md-select-label>
            Select the Pay Period to Post {{ selectedPayPeriod }}
          </md-select-label>
          <md-option ng-repeat="pp in payperiodlist"
                     ng-value="pp">
            {{ pp }}
          </md-option>
        </md-select>
        <md-button ng-click="PostToFinance()"
                   class="md-warn md-raised">
          Post Timestore Data to Finplus
        </md-button>
        <div layout="row"
             layout-align="center center"
             flex="50">
          Clicking this button will post all of the data from Timestore for the selected pay period to the Finplus database.
        </div>
        <div ng-if="postResult.length > 0"
             flex="100"
             layout="row"
             layout-align="center center"
             layout-wrap
             layout-padding
             layout-margin>
          <span layout="row"
                layout-align="end center"
                flex="25">
            Post Result:
          </span>
          <span layout="row"
                layout-align="start center"
                flex="50">
            {{ postResult }}
          </span>
        </div>
      </div>
    </div>
</script>
<script type="text/ng-template" id="Incentives.tmpl.html">

    <form name="IncentiveForm">
        <div flex="100" layout="row" layout-align="center start" layout-wrap layout-margin>

            <md-list ng-if="incentives.length > 0"
                     flex="50"
                     layout="row"
                     layout-align="center center"
                     layout-wrap
                     layout-padding>
                <md-item ng-repeat="i in incentives"
                         flex="100">
                    <div flex="100"
                         layout="row"
                         layout-align="center center">
                        <span flex="30">
                            {{ i.Incentive_Name }}
                        </span>
                        <md-input-container class="short-padding">
                            <label>Incentive Amount</label>
                            <input required type="number" step="any" name="amount" min="0" ng-model="incentives[$index].Incentive_Amount" />
                        </md-input-container>
                    </div>
                    <md-divider flex="100"></md-divider>
                </md-item>
            </md-list>
            <div flex="30">
                <md-button ng-click="SaveIncentives()"
                           class="md-warn md-raised">
                    Save Changes
                </md-button>
            </div>
        </div>
    </form>
</script>
<script type="text/ng-template" id="TimeCardSignatureView.tmpl.html">
    <div style="overflow: visible !important; width: 100%;">

        <md-progress-linear ng-if="showProgress === true"
                            flex="100"
                            md-mode="indeterminate">
        </md-progress-linear>

        <date-selector title="Pay Period Ending:"
                       label="Select Pay Period"
                       datetype="ppd"
                       flex="100">
        </date-selector>

        <span>{{Message}}</span>

        <table class="addPageBreak"
               ng-if="Message === ''"
               ng-repeat="t in timeData track by $index">
            <tr>
                <td style="width: 50%; text-align: center;">
                    <h5 class="short">
                        {{ t.employeeName }} ( {{ t.employeeID }} )
                    </h5>
                </td>
                <td style="width: 50%; text-align: center; ">
                    <h5 class="short">
                        Pay Period Ending {{ t.payPeriodEndingDisplay }}
                    </h5>
                </td>
            </tr>
            <tr>
                <td colspan="2">
                    <timecard-week-table style="width: 100%;" title="Week 1" week="t.RawTime_Week1"
                                         typelist="t.Get_Types_Used"></timecard-week-table>
                </td>
            </tr>
            <tr>
                <td colspan="2">
                    <timecard-week-table style="width: 100%;" title="Week 2" week="t.RawTime_Week2"
                                         typelist="t.Get_Types_Used"></timecard-week-table>
                </td>
            </tr>
            <tr>
                <td>
                    <div style="border-bottom: 2px solid black; width: 100%; text-align: left;">
                        Signature:
                    </div>
                </td>
                <td>
                    <div style="border-bottom: 2px solid black; width: 100%;">
                        Date:
                    </div>
                </td>
            </tr>
        </table>
    </div>
</script>
<script type="text/ng-template" id="FemaView.tmpl.html">
  <div style="overflow: visible !important; width: 100%;">

    <md-progress-linear ng-if="showProgress === true"
                        flex="100"
                        md-mode="indeterminate">
    </md-progress-linear>

    <!--<div id="femaGroupSelection"
         layout="row"
         layout-align="center center"
         flex="100">
      <md-input-container>
        <label>Select Group and Pay Period</label>
        <md-select ng-change="processSelectedGroup()"
                   ng-model="selectedGroup">
          <md-option ng-value="$index"
                     ng-repeat="g in GroupsByPayperiodData">
            {{ g.ppe }} - {{ g.group }}
          </md-option>
        </md-select>
      </md-input-container>
    </div>-->
    <span>{{Message}}</span>

    <table style="width: 95%; margin-left: 1em;"
           class="addPageBreak"
           ng-if="Message === ''"
           ng-repeat="t in timeData track by $index">
      <tr>
        <td style="width: 100%;">
          <div layout-wrap
               layout-align="center center"
               layout="row">
            <span layout="row"
                  layout-align="center center"
                  flex="100">
              <span layout-align="center center"
                    layout="row"
                    flex="25">
                {{ t.employeeName }} ( {{ t.employeeID }} )
              </span>
              <span layout-align="center center"
                    layout="row"
                    flex="35">
                {{ t.title }}
              </span>
              <span layout-align="center center"
                    layout="row"
                    flex="40">
                {{ t.department }}
              </span>
            </span>
            <span layout="row"
                 flex="100">
              <span flex="50"
                    layout="row"
                    layout-align="center center">
                Initial Approval By {{ t.Initial_Approval_By}} on {{ t.Initial_Approval_DateTime }}
              </span>
              <span flex="50"
                    layout="row"
                    layout-align="center center">
                Final Approval By {{ t.Final_Approval_By }} on {{ t.Final_Approval_DateTime }}
              </span>
            </span>
          </div>


          <!--<timecard-header flex="100"
                           shortheader="true"
                           timecard="t">
          </timecard-header>-->
        </td>
      </tr>
      <tr>
        <td style="width: 100%;">

          <timecard-week-table style="width: 100%;" title="Week 1" week="t.RawTime_Week1"
                               typelist="t.Get_Types_Used"></timecard-week-table>
        </td>
      </tr>
      <tr>
        <td style="width: 100%;">
          <timecard-week-table style="width: 100%;" title="Week 2" week="t.RawTime_Week2"
                               typelist="t.Get_Types_Used"></timecard-week-table>
        </td>
      </tr>
    </table>
  </div>
</script>
<script type="text/ng-template" id="LeaveApproval.tmpl.html">

  <md-progress-linear ng-if="showProgress === true"
                      flex="100"
                      md-mode="indeterminate">
  </md-progress-linear>

  <div layout="row"
       layout-align="center center"
       layout-padding
       flex="100">

    <div layout="row"
         layout-align="center center"
         class="md-whiteframe-z1"
         layout-wrap
         flex="90">

      <div ng-show="deptList.length > 1"
           layout="row"
           layout-align="center center"
           layout-wrap
           flex="100">
        <label style="margin-right: .5em;">
          Filter By Department
        </label>
        <md-select md-on-close="updateFilter()"
                   aria-label="View By Department"
                   ng-model="filterDept">
          <md-option value="Select Dept">
            Select Dept
          </md-option>
          <md-option value="all">
            All
          </md-option>
          <md-option ng-repeat="dept in deptList | orderBy:dept"
                     ng-value="dept">
            {{ dept }}
          </md-option>
        </md-select>
      </div>

      <div layout="row"
           layout-align="center center"
           layout-wrap
           flex="100">
        <label>Filter by Status</label>
        <md-radio-group layout="row"
                        layout-align="start center"
                        ng-change="updateFilter()"
                        ng-model="filterStatus">
          <md-radio-button value="undecided">
            Undecided
          </md-radio-button>
          <md-radio-button value="approved">
            Approved
          </md-radio-button>
          <md-radio-button value="denied">
            Denied
          </md-radio-button>
          <md-radio-button value="all">
            All
          </md-radio-button>
        </md-radio-group>
        <span flex="5"></span>
        <md-button ng-click="refreshData()"
                   class="md-primary md-raised">
          Refresh Data
        </md-button>
        <span flex="5"></span>
        <md-button ng-show="filteredDataList.length > 0 && filterStatus==='undecided'"
                   ng-click="approveAll()"
                   class="md-warn md-raised">
          Approve All
        </md-button>
      </div>
      <div ng-show="filteredDataList.length > 0"
           layout="row"
           layout-align="center center"
           layout-wrap
           flex="100">

        <div style="margin-top: 8px; margin-bottom: 8px;"
             class="my-accent padding"
             layout-align="center center"
             flex="100"
             layout-wrap
             layout="row">
          <span flex="10">
            Dept
          </span>
          <span flex="20">
            Date
          </span>
          <span flex="15">
            Name
          </span>
          <span flex="20">
            Type
          </span>
          <span flex="10">
            Hours
          </span>
          <span flex="20">
            Status
          </span>
          <md-divider class="padding"
                      flex="100"></md-divider>
        </div>

        <div style="cursor: pointer;"
             class="padding"
             ng-repeat="d in filteredDataList track by $index"
             ng-class="{highlightBorder: selectedId == d.approval_hours_id}"
             layout="row"
             layout-align="center"
             layout-wrap
             flex="100">
          <span ng-click="showDetail($index)"
                flex="10">
            {{ d.dept_id }}
          </span>
          <span ng-click="showDetail($index)"
                flex="20">
            {{ d.work_date_display }}
          </span>
          <span ng-click="showDetail($index)"
                flex="15">
            {{ d.employee_name }}
          </span>
          <span ng-click="showDetail($index)"
                flex="20">
            {{ d.field.Field_Display }}
          </span>
          <span ng-click="showDetail($index)"
                flex="10">
            {{ d.hours_used }}
          </span>
          <span ng-if="!d.Finalized"
                ng-click="showDetail($index)"
                flex="20">
            <span class="undecidedLeave">
              Undecided
            </span>
          </span>
          <span ng-if="d.Finalized && d.Approved"
                ng-click="showDetail($index)"
                flex="20">
            <span class="approvedLeave">
              Approved
              <md-icon aria-label="approved icon"
                       md-svg-src="images/ic_done_24px.svg">
              </md-icon>
            </span>
          </span>
          <span ng-if="d.Finalized && !d.Approved"
                ng-click="showDetail($index)"
                flex="20">
            <span class="deniedLeave">
              Denied
              <md-icon aria-label="denied icon"
                       md-svg-src="images/ic_clear_24px.svg">
              </md-icon>
            </span>
          </span>
          <div style="margin-top: .5em;"
               ng-if="d.comment.length > 0"
               layout="row"
               layout-align="start center"
               flex="100">
            <span style="text-align: right;"
                  flex="20">
              Comment:
            </span>
            <span flex="5"></span>
            <span style="text-align: left;"
                  flex>
              {{ d.comment}}
            </span>
          </div>
          <div style="margin-top: .5em;"
               ng-if="d.note.length > 0"
               layout="row"
               layout-align="start center"
               flex="100">
            <span style="text-align: right;"
                  flex="20">
              Decision Note:
            </span>
            <span flex="5"></span>
            <span style="text-align: left;"
                  flex>
              {{ d.note}}
            </span>
          </div>
          <div style="margin-top: .5em;"
               ng-if="d.Finalized"
               layout="row"
               layout-align="start center"
               flex="100">
            <span style="text-align: right;"
                  flex="20">
              Approved By: 
            </span>
            <span flex="5"></span>
            <span flex>
              {{ d.by_username }}
            </span>
          </div>
          <div style="margin-top: .5em;"
               ng-if="d.Finalized"
               layout="row"
               layout-align="start center"
               flex="100">
            <span style="text-align: right;"
                  flex="20">
              Approved On:
            </span>
            <span flex="5"></span>
            <span style="text-align: left;"
                  flex>
              {{ d.approval_date_display }}
            </span>
          </div>
          <md-divider class="padding"
                      flex="100"></md-divider>
          <div class="padding"
               flex="100"
               layout="row"
               layout-align="center center"
               layout-wrap
               ng-show="d.showDetail">

            <div ng-show="!d.Finalized"
                 layout="row"
                 layout-align="center center"
                 flex="100">
              <md-button ng-disabled="approving"
                         ng-click="finalizeLeaveRequest(true, $index)"
                         class="md-raised md-primary">
                Approve Leave Request
              </md-button>

              <span flex="5"></span>
              <md-button ng-click="closeDetail()"
                         class="md-raised">
                Close
              </md-button>

            </div>
            <div ng-if="!d.Finalized && approving"
                 layout="row"
                 layout-align="center center"
                 flex="100">
              <md-progress-circular md-diameter="25%"
                                    md-mode="indeterminate">
              </md-progress-circular>
              <span>Processing your request, please wait...</span>
            </div>
            <div ng-show="!d.Finalized"
                 layout="row"
                 layout-align="center center"
                 flex="100">
              <md-button ng-click="finalizeLeaveRequest(false, $index)"
                         ng-disabled="d.note.length === 0 || approving"
                         class="md-raised md-warn">
                Deny Leave Request
              </md-button>
              <span flex="5"></span>
              <md-input-container flex="50">
                <label style="color: black;">
                  Reason for Denial - Required for Denials
                  <md-icon aria-label="denial reason icon"
                           md-svg-src="images/ic_mode_edit_24px.svg"></md-icon>
                </label>
                <input ng-model="d.note"
                       type="text"
                       md-max-length="1000" />
              </md-input-container>
            </div>
              <div ng-show="d.Finalized"
                   layout="row"
                   layout-align="center center"
                   flex="100">
                <span flex="80"></span>
                <md-button ng-click="closeDetail()"
                           class="md-raised">
                  Close
                </md-button>
              </div>
              <div flex="100"
                   layout-align="center center"
                   layout="row">
                {{ responseMessage }}
              </div>

              <div ng-if="filteredDataByDateAndDeptList.length > 0"
                   style="margin-top: .25em; margin-bottom: .25em; cursor: text;"
                   flex="100"
                   class="short-toolbar my-accent"
                   layout="row"
                   layout-align="start center">
                <span flex="5"></span>
                <h5>
                  Showing leave requests found on this date
                </h5>
              </div>
              <div style="margin-top: .75em; cursor: text;"
                   ng-if="filteredDataByDateAndDeptList.length > 0"
                   ng-repeat="x in filteredDataByDateAndDeptList"
                   layout="row"
                   layout-align="center center"
                   layout-wrap
                   flex="100">
                <span flex="5">
                  {{ x.dept_id }}
                </span>
                <span flex="20">
                  {{ x.work_date_display }}
                </span>
                <span flex="20">
                  {{ x.employee_name }}
                </span>
                <span flex="20">
                  {{ x.field.Field_Display }}
                </span>
                <span flex="10">
                  {{ x.hours_used }}
                </span>
                <span ng-if="!x.Finalized"
                      flex="20">
                  <span class="undecidedLeave">
                    Undecided
                  </span>
                </span>
                <span ng-if="x.Finalized && x.Approved"
                      flex="20">
                  <span class="approvedLeave">
                    Approved
                    <md-icon aria-label="approved icon"
                             md-svg-src="images/ic_done_24px.svg">
                    </md-icon>
                  </span>
                </span>
                <span ng-if="x.Finalized && !x.Approved"
                      flex="20">
                  <span class="deniedLeave">
                    Denied
                    <md-icon aria-label="denied icon"
                             md-svg-src="images/ic_clear_24px.svg">
                    </md-icon>
                  </span>
                </span>
                <md-divider class="padding"
                            flex="100"></md-divider>
              </div>
            </div>
        </div>


      </div>

      <div ng-show="filteredDataList.length === 0 && showProgress === false"
           layout="row"
           layout-align="center center"
           flex="100">

        No requests to view for the selected Department.

      </div>

      <!--
          Show a listing of all of the Leave Requests, with unapproved at the top.
          Upon selecting a request, the request expands and all of the other requests found
          on this day are shown


      -->
    </div>

  </div>

</script>
<script type="text/ng-template" id="TimeCardNotApproved.tmpl.html">

    <div flex="100"
         layout="row"
         layout-align="center center"
         layout-wrap>

        <md-progress-linear ng-if="showProgress === true"
                            flex="100"
                            md-mode="indeterminate">
        </md-progress-linear>       

        <div flex="90">
            <date-selector title="Pay Period Ending:"
                           label="Select Pay Period"
                           datetype="ppd"
                           flex="100">
            </date-selector>

            <md-input-container flex="50">
                <label flex="100">
                    <span style="text-align: center;"
                          flex="100">
                        View By Department {{ selectedDept.deptName }}
                    </span>
                </label>
                <md-select aria-label="View By Department"
                           md-on-close="filterByType()"
                           flex="50"
                           ng-model="selectedDept">
                    <md-option ng-repeat="dept in deptData | orderBy: 'deptName'"
                               ng-value="dept">
                        {{ dept.deptName }}
                    </md-option>
                </md-select>
            </md-input-container>

            <table style="margin-top: 1em;"
                   class="crosstab"
                    ng-if="viewData.length > 0">
                <thead>
                    <tr>
                        <th>
                            Department #
                        </th>
                        <th>
                            Employee name and Number
                        </th>
                        <th>
                            Initial Approval Info
                        </th>
                    </tr>
                </thead>
                <tr ng-repeat="v in filteredData | orderBy: ['department', 'fullname'] track by v.fullname">
                    <td flex="15">
                        {{ v.department }}
                    </td>
                    <td flex="50">
                        <a href="#/e/{{v.employeeId}}/ppd/{{ppd}}">
                            {{ v.fullname }}
                        </a>
                    </td>
                    <td flex="35">
                        {{ v.approvalinfo}}
                    </td>
                </tr>
            </table>

        </div>

    </div>

</script>
<script type="text/ng-template" id="TimeApproval.tmpl.html">
  <div style="margin-top: .75em;"
       ng-show="ctrl.tc.ErrorList.length > 0"
       layout="row"
       layout-align="center center"
       layout-wrap
       flex="100">
    Your time cannot be approved while there are Errors present.
    <timecard-warnings flex="100"
                       alignheader="center"
                       headerclass="warn"
                       dl="ctrl.tc.ErrorList"
                       title="Errors"></timecard-warnings>
  </div>

  <div ng-show="ctrl.tc.Approval_Level > 0 && ctrl.tc.Days_Since_PPE < 1"
       layout="row"
       layout-align="center center"
       flex="100">
    Your time has already been approved.
  </div>
  <div ng-show="ctrl.tc.Days_Since_PPE > 1"
       layout="row"
       layout-align="center center"
       flex="100">
    It is too late to approve your time for this pay period.
  </div>
  <div ng-show="ctrl.tc.timeList.length === 0 && ctrl.tc.Days_Since_PPE < 1"
       layout="row"
       layout-align="center center"
       flex="100">
    No time has been entered to approve.
  </div>
  <div ng-show="ctrl.tc.WarningList.length > 0"
       layout="row"
       layout-align="center center"
       layout-wrap
       flex="100">    
    <timecard-warnings flex="100"
                       alignheader="center"
                       headerclass="my-accent"
                       dl="ctrl.tc.WarningList"
                       title="Warnings"></timecard-warnings>
  </div>
  <md-list ng-if="ctrl.showHolidayError === true"
           flex="100">
    <md-item>
      <md-item-content>
        <div flex="100"
             layout="row"
             layout-align="center center">
          You must choose how to handle your Holiday hours before you can approve your time.
        </div>
      </md-item-content>
    </md-item>
  </md-list>
  <md-list flex="100"
           ng-if="ctrl.showApprovalButton === true">
    <md-item flex>
      <md-item-content flex
                       layout="row"
                       layout-align="center center">
        <md-button ng-click="ctrl.approve()"
                   class="md-raised md-warn ">
          Approve and Finalize
        </md-button>
      </md-item-content>
    </md-item>

  </md-list>
</script>
<script type="text/ng-template" id="TimeCardDetail.tmpl.html">
  <md-tabs class="md-primary"
           flex="100"
           md-selected="selectedWeekTab"
           md-no-pagination="false"
           md-center-tabs="false"
           md-stretch="never"
           md-dynamic-height="true">

    <md-tab label="Week 1" id="week1" layout-padding>
      <div flex="100">

        <timecard-week flex="95"
                       title="Week 1"
                       week="ctrl.timecard.RawTime_Week1"
                       typelist="ctrl.timecard.Get_Types_Used"
                       employeeid="ctrl.timecard.employeeID"
                       showaddtime="ctrl.timecard.showAddTime"
                       datatype="ctrl.timecard.Data_Type">
        </timecard-week>

      </div>
    </md-tab>

    <md-tab label="Week 2" id="week2" layout-padding>
      <div flex="100"
           layout-wrap>
        <timecard-week flex="95"
                       title="Week 2"
                       week="ctrl.timecard.RawTime_Week2"
                       typelist="ctrl.timecard.Get_Types_Used"
                       employeeid="ctrl.timecard.employeeID"
                       showaddtime="ctrl.timecard.showAddTime"
                       datatype="ctrl.timecard.Data_Type"></timecard-week>

      </div>
    </md-tab>

    <md-tab label="Time Summary & Approval"
            layout-padding
            id="calctimelist">
      <div flex="100">

        <div style="margin-top: .75em;"
             class="short-toolbar my-accent"
             flex="100"
             layout="row"
             layout-align="center center">
          <h5>Approval</h5>
        </div>
        <div>
          <time-approval flex="100" tc="::ctrl.timecard"></time-approval>
        </div>

        <div style="margin-top: .75em;"
             class="short-toolbar my-accent"
             flex="100"
             layout="row"
             layout-align="center center">
          <h5>Pay Period Summary</h5>
        </div>

        <time-list flex="100" tl="ctrl.timecard.calculatedTimeList"></time-list>


      </div>

    </md-tab>
    <md-tab ng-if="ctrl.timecard.exemptStatus !== 'Exempt' && ctrl.timecard.Data_Type === 'timecard'"
            label="Handle Comp Time">
      <add-comp-time flex="100"
                     ng-show="ctrl.timecard.exemptStatus !== 'Exempt' && ctrl.timecard.Data_Type === 'timecard'"
                     timecard="ctrl.timecard"></add-comp-time>
    </md-tab>
      

    <md-tab label="Notes"
            id="notelist">

      <div flex="100">

        <timecard-notes flex="100"
                        datatype="ctrl.timecard.Data_Type"
                        employeeid="ctrl.timecard.employeeID"
                        payperiodending="ctrl.timecard.payPeriodEndingDisplay"
                        notes="ctrl.timecard.Notes">
        </timecard-notes>

      </div>

    </md-tab>

    <md-tab ng-if="ctrl.timecard.isPubWorks"
            label="Incentives">
      <div flex="100">

      </div>
    </md-tab>

    <md-tab label="Holiday" 
            ng-if="(ctrl.timecard.HolidaysInPPD.length > 0 || ctrl.timecard.bankedHoliday > 0) && ctrl.timecard.Data_Type === 'telestaff'">
      <div style="margin-top: .5em;"
           layout-align="center center"
           layout="row"
           layout-wrap
           flex="100">
        <div ng-if="ctrl.timecard.HolidaysInPPD.length > 0"
             flex="100"
             layout="row"
             layout-align="center center"
             layout-wrap>
          <div flex="100" class="short-toolbar my-accent" layout="row" layout-align="center center">
            <h5>
              How do you want to handle this Holiday?
            </h5>
          </div>
          <md-radio-group flex="100"
                          layout="row"
                          layout-align="center center"
                          layout-wrap
                          ng-model="ctrl.timecard.HolidayHoursChoice[$index]"
                          ng-repeat="holiday in ctrl.timecard.HolidaysInPPD">
            <span flex="25">
              {{holiday}}
            </span>
            <md-radio-button ng-value="'Bank'">
              Bank this holiday
            </md-radio-button>
            <md-radio-button ng-value="'Paid'">
              Pay me for this holiday
            </md-radio-button>
            <md-radio-button ng-value="'None'">
              No choice made
            </md-radio-button>
            <md-radio-button ng-value="'Ineligible'">
              Ineligible for Holiday
              <md-tooltip>
                Select this if you are not eligible to receive this Holiday.
              </md-tooltip>
            </md-radio-button>
          </md-radio-group>
        </div>
        <div flex="100"
             layout="row"
             layout-align="center center"
             layout-wrap
             ng-if="ctrl.timecard.bankedHoliday >= ctrl.timecard.holidayIncrement">
          <div flex="100"
               class="short-toolbar my-accent"
               layout="row"
               layout-align="center center">
            <h5>
              Do you want to be paid for any of your banked Holidays?
            </h5>
          </div>
          <md-input-container>
            <label>Pay me for these Banked Holiday Hours</label>
            <input required type="number"
                   step="{{ctrl.timecard.holidayIncrement}}"
                   name="amount"
                   min="0"
                   max="{{ctrl.timecard.bankedHoliday - ctrl.timecard.HolidayHoursUsed}}"
                   ng-model="ctrl.timecard.BankedHoursPaid" />
          </md-input-container>
          <span flex="50">You have {{ ctrl.timecard.bankedHoliday }} hours banked, and have currently marked {{ timecard.HolidayHoursUsed + timecard.BankedHoursPaid }} hours for use so far this pay period.  You can elect to be paid for them in groups of {{ timecard.holidayIncrement }} hours.</span>
        </div>
        <div layout="row"
             layout-align="center center"
             flex="100"
             ng-if="ctrl.timecard.bankedHoliday < ctrl.timecard.holidayIncrement">
          You must have {{ ctrl.timecard.holidayIncrement }} hours of banked holiday time to request a pay out.
        </div>
        <div flex="90"
             layout-align="center center"
             layout="row">
          <ul>
            <li>
              If you are interested in changing your tax withholding for this pay period, please access the form here (<a href="images/2020%20W4.pdf">2020 W4.pdf</a>).
            </li>
            <li>
              You will be required to complete a second form to revert to the original tax withholding status.
            </li>
            <li>
              This form must be completed and returned to HR before {{TaxWitholdingCutoff}} for it to be effective this pay period.
            </li>
            <li>
              Questions can be directed to Human Resources at <a href="mailto:bcchr@claycountygov.com">bcchr@claycountygov.com</a>.
            </li>
          </ul>            
        </div>

        <div layout="row"
             layout-align="center center"
             flex="100">
          <md-button ng-click="ctrl.SaveHolidays()"
                     class="md-warn md-raised">
            Save
          </md-button>
        </div>
      </div>

    </md-tab>

    <!--<md-tab ng-if="timecard.Data_Type === 'timecard'">
        <md-tab-label>
            Leave Requests
        </md-tab-label>
        <md-tab-body>
            <div flex="100">

                <new-leave-request></new-leave-request>

                <existing-leave-request flex="100" requests="leaveRequests">
                </existing-leave-request>

            </div>
        </md-tab-body>
    </md-tab>-->



  </md-tabs>
</script>
<script type="text/ng-template" id="TimeCardHeader.tmpl.html">

    <div class="md-whiteframe-z1" ng-cloak>
      <md-grid-list ng-cloak
                    md-cols="3"
                    md-cols-md="2"
                    md-cols-sm="1"
                    md-gutter="8px"
                    md-row-height="28px">

        <md-grid-tile md-rowspan="2"
                      md-colspan="1"
                      md-rowspan-md="4"
                      md-rowspan-sm="1"
                      md-colspan-md="1"
                      md-colspan-sm="1">

          <div layout="row"
               layout-align="center center"
               layout-wrap>
            <h4>
              {{ ::timecard.employeeName }} ( {{ ::timecard.employeeID }} )
            </h4>
          </div>
        </md-grid-tile>

        <md-grid-tile md-rowspan="1"
                      md-colspan="1">
          <div layout="row"
               flex="100"
               layout-align="center center"
               layout-wrap>
            {{ ::timecard.title }} ( {{ ::timecard.classify }} )
          </div>
        </md-grid-tile>
        <md-grid-tile md-rowspan="1"
                      md-colspan="1">
          <div layout="row"
               flex="100"
               layout-align="center center"
               layout-wrap>
            {{ ::timecard.department }} ( {{ ::timecard.departmentNumber }} )
          </div>
        </md-grid-tile>

        <md-grid-tile md-rowspan="1"
                      md-colspan="1">
          <div layout="row"
               flex="100"
               layout-align="center center"
               layout-wrap>
            <md-button class="md-primary"
                       style="text-decoration: underline;"
                       ng-click="showPayrate = !showPayrate;">
              {{ showPayrate ? 'Hide' : 'Show' }} Payrate
            </md-button>
            <span style="margin-right: .5em;"
                  ng-show="showPayrate">
              Payrate {{ ::timecard.shortPayrate }}
              <md-tooltip md-direction="right">
                ${{  ::timecard.Payrate }}
              </md-tooltip>
            </span>
            <span>
              {{ ::timecard.exemptStatus }}
            </span>
          </div>
        </md-grid-tile>
        <md-grid-tile md-rowspan="1"
                      md-colspan="1">
          <div layout="row"
               flex="100"
               layout-align="center center"
               layout-wrap>
            {{ timecard.fullTimeStatus }} {{ ' Scheduled Hours ' + timecard.scheduledHours }}
          </div>
        </md-grid-tile>

      </md-grid-list>

        <md-grid-list ng-if="shortheader === false"
                      md-cols="4"
                      md-cols-md="2"
                      md-cols-sm="1"
                      md-gutter="8px"
                      md-row-height="28px">

            <md-grid-tile md-rowspan="1"
                          md-colspan="4"
                          md-colspan-md="2"
                          md-colspan-sm="1">

                <div flex="100"
                     class="short-toolbar my-accent"
                     layout="row"
                     layout-align="center center">
                    <h5>
                        Current Banked Hours
                    </h5>
                </div>
            </md-grid-tile>

            <md-grid-tile layout="row"
                          layout-align="center center"
                          md-rowspan="1"
                          md-colspan="1">

                {{ 'Vacation ' + timecard.bankedVacation }}

            </md-grid-tile>
            <md-grid-tile layout="row"
                          layout-align="center center"
                          md-rowspan="1"
                          md-colspan="1">
                {{ 'Sick ' + timecard.bankedSick }}
            </md-grid-tile>
            <md-grid-tile ng-if="timecard.exemptStatus === 'Non Exempt' && !timecard.isHolidayTimeBankable"
                          layout="row"
                          layout-align="center center"
                          md-rowspan="1"
                          md-colspan="1">
                {{ 'Comp ' + timecard.bankedComp }}
            </md-grid-tile>
            <md-grid-tile ng-if="timecard.isHolidayTimeBankable || timecard.bankedHoliday > 0"
                          layout="row"
                          layout-align="center center"
                          md-rowspan="1"
                          md-colspan="1">
                {{ 'Holiday ' + timecard.bankedHoliday }}
            </md-grid-tile>
        </md-grid-list>

        <md-grid-list ng-if="timecard.Approval_Level > 0 && shortheader === false"
                      md-cols="2"
                      md-cols-md="1"
                      md-cols-sm="1"
                      md-gutter="8px"
                      md-row-height="28px">

            <md-grid-tile md-rowspan="1"
                          md-colspan="2"
                          md-colspan-md="1"
                          md-colspan-sm="1">

                <div flex="100"
                     class="short-toolbar my-accent"
                     layout="row"
                     layout-align="center center">
                    <h5>
                        Approval Information
                    </h5>
                </div>
            </md-grid-tile>

            <md-grid-tile layout="row"
                          layout-align="center center">
                Initial Approval By {{ ::timecard.Initial_Approval_By}} on {{ ::timecard.Initial_Approval_DateTime }}
            </md-grid-tile>
            <md-grid-tile ng-if="timecard.Approval_Level > 1"
                          layout="row"
                          layout-align="center center">
                Final Approval By {{ ::timecard.Final_Approval_By }} on {{ ::timecard.Final_Approval_DateTime }}
            </md-grid-tile>
        </md-grid-list>

    </div>
</script>
<script type="text/ng-template" id="TimecardNotes.tmpl.html">
  <div style="padding-top: .5em;"
       layout="row"
       layout-align="center center"
       layout-wrap
       flex="100">
    <div layout="row"
         layout-align="center center"
         flex="75">
      <md-input-container flex="90"
                          class="md-whiteframe-z1">
        <label>{{ datatype === "timecard" ? "Add a note" : "Add your Manpower note" }}</label>
        <input ng-model="noteText" />
      </md-input-container>
    </div>
    <md-button ng-click="saveNote()"
               class="md-raised md-warn">
      Save
    </md-button>
  </div>  
  <md-list>
    <md-list-item>
      <div class="short-toolbar my-accent"
           flex="100"
           layout="row"
           layout-align="center center">
        <h5>Notes</h5>
      </div>
      <md-divider flex="100"></md-divider>
    </md-list-item>    
    <md-list-item>
      <div flex="100"
           layout="row"
           layout-align="start center">
        <span flex="25">Note Date</span>
        <span flex="25">Note By</span>
        <span flex="45">Note</span>
      </div>
      <md-divider flex="100"></md-divider>
    </md-list-item>

    <md-list-item ng-if="notes.length === 0">
      <div flex="100"
           layout="row"
           layout-align="start center">
        <span flex="25">n/a</span>
        <span flex="25">n/a</span>
        <span flex="45">No notes have been added yet.</span>
      </div>
      <md-divider flex="100"></md-divider>
    </md-list-item>
    <md-list-item ng-if="notes.length > 0"
                  ng-repeat="note in notes track by $index">
      <div flex="100"
           layout="row"
           layout-align="start center">
        <span flex="25">{{note.Date_Added_Display}}</span>
        <span flex="25">{{note.Added_By}}</span>
        <span flex="45">{{note.Note}}</span>
      </div>
      <md-divider flex="100"></md-divider>
    </md-list-item>
  </md-list>  
</script>
<script type="text/ng-template" id="TimeCardView.tmpl.html">
    <div flex="100"
         layout="row"
         layout-align="center start"
         layout-wrap>

        <div style="margin-top: 8px; margin-bottom: 8px;"
             flex="90">

            <timecard-header flex="100"
                             shortheader="false"
                             timecard="timecard">
            </timecard-header>

        </div>

        <div flex="90"
             class="md-whiteframe-z1">
            <date-selector title="Pay Period Ending:"
                           label="Select Pay Period"
                           datetype="ppd"
                           flex="100">
            </date-selector>
            <timecard-detail flex="100" timecard="::timecard"></timecard-detail>

        </div>
    </div>
</script>
<script type="text/ng-template" id="TimecardWarnings.tmpl.html">

    <md-list ng-if="dl.length > 0"
             flex="100"
             layout="row"
             layout-align="center center"
             layout-wrap>

        <md-item flex="100"
                 layout="row"
                 layout-align="center center"
                 layout-wrap>
            <div class="short-toolbar"
                 ng-class="headerclass"
                 flex="100"
                 layout="row"
                 layout-wrap
                 layout-align="start center">
                <span flex="5"> </span>
                <h5>
                    {{::title}}
                </h5>
            </div>
            <md-divider flex="100"></md-divider>
        </md-item>
        <md-item style="margin-top: .75em; margin-left: 1em;"
                 flex="100"
                 layout="row"
                 layout-align="center center"
                 ng-repeat="wl in dl track by $index"
                 layout-wrap>
            <div flex="100"
                 layout="row"
                 layout-align="start center">
                <span flex="100">{{::wl}}</span>
            </div>
            <md-divider flex="100"></md-divider>
        </md-item>
    </md-list>
</script>
<script type="text/ng-template" id="TimeCardWeek-table.tmpl.html">

    <table style="width: 100%;">
        <tr>
            <th class="width15">
                Day / Date
            </th>
            <th class="autoWidth">
                Work Times
            </th>
            <th class="width10">
                Comment
            </th>
            <th class="width10" ng-repeat="gtu in typelist">
                {{gtu}}
            </th>
            <th class="width5">
                Total
            </th>
        </tr>
        <tr ng-repeat="r in week">
            <td class="width15">
                {{r.workDateDayOfWeek}} {{r.workDateDisplay}}
            </td>
            <td class="autoWidth">
                <div style="display: block; width: 100%;">
                    <span style="padding-right: .5em;"
                          ng-repeat="work in r.workTimeList">
                        {{work.WorkTime}}
                    </span>
                </div>
            </td>
            <td class="width10">
                {{r.comment}}
            </td>
            <td class="width10"
                ng-repeat="gtu in typelist">
                {{ getHours(gtu, r.workHoursList) }}
            </td>
            <td class="width5">
                {{ r.totalWorkHours}}
            </td>
        </tr>
        <tr>
            <td colspan="3"
                style="">
                Total for {{title}}
            </td>
            <td class="width10"
                ng-repeat="gtu in typelist">
                {{ getTotalHours(gtu)}}
            </td>
            <td class="width5">
                {{getTotalHours(null)}}
            </td>
        </tr>
    </table>
</script>
<script type="text/ng-template" id="TimeCardWeek.tmpl.html">
  <md-list ng-cloak>
    <md-list-item class="md-no-proxy">
      <span flex="100"
           layout="row"
           layout-align="center center">
        <span layout="row"
              layout-align="center center"
              flex="15">
          <md-button style="width: 95%; min-width: 130px;"
                     ng-if="showaddtime === true"
                     ng-click="addTimeGo()"
                     class="md-accent md-raised">
            Time Entry
            <md-icon aria-label="add time icon"
                     class="iconColor"
                     md-svg-src="images/ic_alarm_add_24px.svg">
            </md-icon>
          </md-button>
          <span ng-if="showaddtime === false">
            Day / Date
          </span>
        </span>
        <span layout="row"
             layout-align="center center"
             flex>
          Work Times
        </span>
        <span layout="row"
              layout-align="center center"
              flex="15">
          Comment
        </span>
        <span class="centerme"
              layout="row"
              layout-align="center center"
              flex="10"
              ng-repeat="gtu in typelist">
          {{gtu}}
        </span>
        <span style="text-align: center;"
              flex="10">
          Total
        </span>
      </span>
      <md-divider></md-divider>
    </md-list-item>
    <md-list-item ng-repeat="r in week">

      <span flex="100"
           layout="row"
           layout-align="center center">
        <span layout="row"
              layout-align="center center"
              flex="15">
          <a style="text-decoration: underline; cursor: pointer;"
             layout="row"
             layout-align="center center"
             ng-click="addTimeGo(r.workDateDisplay)"
             ng-if="showaddtime === true"
             flex>
            {{r.workDateDayOfWeek}} {{r.workDateDisplay}}
          </a>
          <span ng-if="showaddtime === false">
            {{r.workDateDayOfWeek}} {{r.workDateDisplay}}
          </span>
        </span>
        <span style="display: inline-block;"
             flex>
          <md-list flex="100">
            <md-item style="display: inline-block; padding-right: 1em;"
                     ng-repeat="work in r.workTimeList">
              {{work.WorkTime}}
              <md-tooltip md-direction="right"
                          ng-if="work.Description.length > 0">
                {{work.Description}}
              </md-tooltip>
            </md-item>
          </md-list>
        </span>
        <span layout="row"
              layout-align="center center"
              flex="15">
          {{r.comment}}
        </span>
        <span layout="row"
              layout-align="center center"
              flex="10"
              ng-repeat="gtu in typelist">
          {{ getHours(gtu, r.workHoursList) }}
        </span>
        <span layout="row"
              layout-align="center center"
              flex="10">
          {{ r.totalWorkHours}}
        </span>
      </span>
      <md-divider></md-divider>

    </md-list-item>
    <md-list-item>
      <span flex="100"
           layout="row"
           layout-align="center center">
        <b layout="row"
            layout-align="center end"
            class="short" flex>
          Total for {{title}}
        </b>
        <b layout="row"
            layout-align="center center"
            class="short"
            flex="10"
            ng-repeat="gtu in typelist">
          {{ getTotalHours(gtu)}}
        </b>
        <b layout="row"
            layout-align="center center"
            class="short"
            flex="10">
          {{getTotalHours(null)}}
        </b>
      </span>
    </md-list-item>
  </md-list>
</script>
<script type="text/ng-template" id="TimeList.tmpl.html">

  <md-list class="smallList"
           ng-if="getGroups().length > 1"
           ng-repeat="group in getGroups()"
           flex="100"
           layout="row"
           layout-align="end center"
           layout-wrap>

    <md-list-item flex="100">
      <div flex="100"
           class="short-toolbar my-primary"
           layout="row"
           layout-align="center center">
        <h5>Your hours at base Payrate: {{group}}</h5>
      </div>
    </md-list-item>

    <md-list-item style="min-height: 0;"
                  ng-repeat="t in tl | groupPayrateBy:group"
                  flex="33"
                  flex-md="50"
                  flex-sm="100">
      <div flex="100"
           layout="row"
           layout-align="center center"
           ng-class="t.hours > 0 ? 'my-accent' : '' ">
        <span flex="70" layout="row" layout-align="end center">
          {{t.name}}
        </span>
        <span flex="20" layout="row" layout-align="center center">
          {{t.hours}}
        </span>
      </div>
    </md-list-item>
  </md-list>


  <md-list class="smallList"
           flex="100"
           ng-if="getGroups().length == 1"
           layout-wrap
           layout="row"
           layout-align="end center">
    <md-list-item style="min-height: 0;"
                  ng-repeat="t in tl"
                  flex="33"
                  flex-md="50"
                  flex-sm="100">
      <div flex="100"
           layout="row"
           layout-align="center center"
           ng-class="t.hours > 0 ? 'my-accent' : '' ">
        <span flex="70" layout="row" layout-align="end center">
          {{t.name}}
        </span>
        <span flex="20" layout="row" layout-align="center center">
          {{t.hours}}
        </span>

      </div>
    </md-list-item>

  </md-list>

  <md-list class="smallList"
           flex="100"
           layout="row"
           layout-align="end center">
    <md-list-item style="min-height: 0;"
                  flex="33"
                  flex-md="50"
                  flex-sm="100">
      <div flex="100"
           layout="row"
           layout-align="center center"
           ng-class="getTotalHours() > 0 ? 'my-primary' : '' ">
        <span flex="70" layout="row" layout-align="end center">
          Total Hours
        </span>
        <span flex="20" layout="row" layout-align="center center">
          {{getTotalHours()}}
        </span>
      </div>
    </md-list-item>

  </md-list>
</script>
<script type="text/ng-template" id="CalendarView.controller.tmpl.html">
  <div layout-align="center center"
       layout="row"
       flex="100">
    <div style="margin-top: 1em;"
         layout="row"
         layout-align="start center"
         flex="90">
      <leave-calendar flex="100"
                      holidaylist="holidays"
                      birthdaylist="birthdays"
                      leavedata="leaveRequests">

      </leave-calendar>
    </div>
  </div>
</script>
<script type="text/ng-template" id="LeaveCalendar.directive.tmpl.html">

    <div class="leaveCalendar" flex>

        <md-toolbar flex="100">
            <div class="md-toolbar-tools" layout="row" layout-align="center center" layout-margin>
                <div flex="30">
                    <md-button aria-label="view Calendar"
                               ng-click="previousMonth()"
                               class="md-fab md-mini">
                        <md-icon aria-label="previous month icon"
                                 md-svg-src="images/ic_navigate_before_24px.svg"></md-icon>
                    </md-button>
                    <md-button aria-label="view Calendar"
                               ng-click="nextMonth()"
                               class="md-fab md-mini">
                        <md-icon aria-label="next month icon"
                                 md-svg-src="images/ic_navigate_next_24px.svg"></md-icon>
                    </md-button>
                </div>
                <span class="md-display-1"
                      style="text-align: center;"
                      flex>
                    {{ monthTitle }}
                </span>

                <div ng-show="deptList.length > 1"
                     layout="row"
                     layout-align="center center"
                     flex="15">
                    <label style="margin-right: .25em;">
                        Dept
                    </label>
                    <md-select class="deptSelect"
                                flex="60"                                   
                                md-on-close="deptChanged()"
                                aria-label="View By Department"
                                ng-model="selectedDept">
                        <md-option ng-repeat="dept in deptList | orderBy:dept"
                                    ng-value="dept">
                            {{ dept }}
                        </md-option>
                    </md-select>
                </div>
                <div layout="row"
                     layout-align="end center"
                     flex="15">
                    <md-button ng-click="goHome()"
                               class="md-raised md-warn">
                        TimeStore
                        <md-icon aria-label="return home icon"
                                 md-svg-src="images/ic_replay_24px.svg">
                        </md-icon>
                    </md-button>
                </div>

            </div>
        </md-toolbar>


        <md-grid-list style="margin-top: 1em;"
                      md-cols="7"
                      md-gutter="0"
                      md-row-height="28px">
            <md-grid-tile class="Framed"
                          ng-repeat="d in days"
                          md-rowspan="1"
                          md-colspan="1">
                <h3 layout-align="center center"
                    layout="row">
                    {{ d }}
                </h3>
            </md-grid-tile>
        </md-grid-list>
        <md-grid-list class="myCalendar"
                      md-cols="7"
                      md-gutter="0"
                      md-row-height="140px">
            <md-grid-tile class="noFocus"
                          ng-class="{'calendarHoliday': c.isHoliday}"
                          ng-click="showCalendarDayDetail($index)"
                          ng-repeat="c in calendarDays track by $index"
                          md-rowspan="{{ c.span }}"
                          md-colspan="1">

                <md-grid-tile-header flex="100"
                                     layout-align="end center"
                                     layout="row">
                    <h3 flex="100" ng-class="{'currentMonth': c.isCurrent, 'notCurrentMonth': !c.isCurrent }">
                        <md-icon aria-label="birthday cake icon"
                                 ng-if="c.birthdayList.length > 0"
                                 class="md-icon-button" 
                                 md-svg-src="images/ic_cake_24px.svg">
                        </md-icon>
                        {{ c.isHoliday ? 'Holiday' : '' }}
                        {{ c.dayOfWeek }}
                        {{ c.day }}
                    </h3>
                </md-grid-tile-header>
                <div class="noFocus gridTileHeight"
                     flex="100"
                     layout="row"
                     layout-align="start start"
                     layout-wrap>
                    <div ng-show="c.showDetail === false"
                         style="margin-top: 24px;"
                         layout-align="start start"
                         layout="row"
                         layout-wrap
                         flex="100"
                         class="smaller">
                        <div layout-align="start start"
                             layout="row"
                             layout-wrap
                             flex="100"
                             ng-if="c.birthdayList.length > 0 && c.shortList.length + c.birthdayList.length < 4">
                            <div ng-repeat="b in c.birthdayList"
                                 flex="100"
                                 layout-align="center center"
                                 layout="row"
                                 layout-padding
                                 layout-wrap>
                                <span style="text-align: center;"
                                      flex="75">
                                    {{ b.toProperCase() }}
                                </span>
                                <span layout="row"
                                      layout-align="center center"
                                      flex="25">
                                    <md-icon aria-label="birthday cake icon"
                                             class="md-icon-button" md-svg-src="images/ic_cake_24px.svg">
                                    </md-icon>
                                </span>
                            </div>
                        </div>
                        <span layout="row"
                              layout-align="start center"
                              style="padding-left: 1em; margin-bottom: .25em;"
                              ng-repeat="sl in c.shortList"
                              flex="100">
                            {{ sl.toProperCase() }}
                        </span>

                    </div>

                    <div class="smaller"
                         ng-show="c.birthdayList.length > 0 && c.showDetail === true"
                         style="margin-top: 24px;"
                         layout-align="start start"
                         layout="row"
                         layout-wrap
                         layout-padding
                         flex="100">
                        <div ng-repeat="b in c.birthdayList"
                             flex="100"
                             layout-align="center center"
                             layout="row"
                             layout-padding
                             layout-wrap>
                            <span style="text-align: center;"
                                  flex="75">
                                {{ b.toProperCase() }}
                            </span>
                            <span layout="row"
                                  layout-align="center center"
                                  flex="25">
                                <md-icon aria-label="birthday cake icon"
                                         class="md-icon-button" md-svg-src="images/ic_cake_24px.svg">
                                </md-icon>
                            </span>
                        </div>

                    </div>

                    <div class="smaller"
                         ng-show="c.showDetail === true && c.detailedList.length > 0"
                         style="margin-top: 24px; padding-top: .5em;"
                         layout-align="start start"
                         layout="row"
                         layout-wrap
                         flex="100">
                        <div ng-repeat="dl in c.detailedList"
                             flex="100"
                             layout-align="center center"
                             layout="row"
                             layout-padding
                             layout-wrap>
                            <span style="text-align: center;"
                                  flex="75">
                                {{ dl.employee_name.toProperCase() }}
                            </span>
                            <span style="text-align: center;"
                                  flex="25">
                                {{ dl.hours_used }}
                            </span>
                            <md-tooltip md-direction="right"
                                        ng-if="dl.comment.length > 0">
                                {{dl.comment}}
                            </md-tooltip>
                        </div>

                    </div>
                </div>
            </md-grid-tile>
        </md-grid-list>

    </div>



</script>
<script type="text/ng-template" id="PaystubView.controller.tmpl.html">

 <link rel="stylesheet" href="css/paystubPrint.css" />

  <div flex="100"
       layout="row"
       layout-align="center start"
       layout-wrap
       layout-margin>

    <div flex="90"
         layout="row"
         id="paystubSelector"
         ng-show="paystubList.length > 0"
         layout-align="center center">

      <md-input-container flex="25">
        <label>Select a Check</label>
        <md-select ng-model="checkNumber"
                   multiple="false"
                   md-on-close="selectCheck()"
                   aria-label="Select A Check">
          <md-option ng-repeat="p in filtered_paystub_list track by $index"
                     ng-value="p.check_number">
            <span style="text-align: center;">
              {{ FormatDate(p.check_date) }} - {{ p.check_number }} {{ p.is_voided ? '(VOIDED)' : ''}}
            </span>
          </md-option>
        </md-select>
      </md-input-container>

      <md-input-container flex="25">
        <label>Filter Pay Stubs by Year</label>
        <md-select ng-model="filter_year"
                   multiple="false"
                   md-on-close="selectYear()"
                   aria-label="Select A Year">
          <md-option value="">
            Show All
          </md-option>
          <md-option ng-repeat="p in paystub_years track by $index"
                     ng-value="p">
            <span style="text-align: center;">
              {{ p }}
            </span>
          </md-option>
        </md-select>
      </md-input-container>

      <div layout="row"
           layout-align="end center"
           flex="100">
        <md-button aria-label="Print Paystub"
                   onclick="window.print();"                   
                   class="md-button md-accent md-raised">
          Print
          <md-icon>
            <svg class="fabWhite"
                 xmlns="http://www.w3.org/2000/svg" height="24" viewBox="0 0 24 24" width="24"><path d="M0 0h24v24H0z" fill="none" /><path d="M19 8H5c-1.66 0-3 1.34-3 3v6h4v4h12v-4h4v-6c0-1.66-1.34-3-3-3zm-3 11H8v-5h8v5zm3-7c-.55 0-1-.45-1-1s.45-1 1-1 1 .45 1 1-.45 1-1 1zm-1-9H6v4h12V3z" /></svg>
          </md-icon>

          <md-tooltip md-direction="bottom">
            Print the paystub being viewed
          </md-tooltip>
        </md-button>
        <md-button ng-click="returnToTimeStore()"
                   class="md-raised md-primary">
          Return to TimeStore
        </md-button>
      </div>
    </div>

    <div id="paystubDetailedView"
         flex="90"
         layout="row"
         ng-show="currentPaystub !== null"
         layout-align="center center"
         layout-wrap>


      <table class="paystub_table md-whiteframe-z1">
        <thead>
          <tr>
            <th style="width: 50%; text-align: left; padding-left: 1em;">
              Employee Name
            </th>
            <th style="width: 50%; text-align: left; padding-left: 1em;">
              Department
            </th>
          </tr>
        </thead>
        <tbody>
          <tr>
            <td style="text-align: left; padding-left: 1em;">
              {{currentPaystub.employee_name}}
            </td>
            <td style="text-align: left; padding-left: 1em;">
              {{currentPaystub.department}}
            </td>
          </tr>
        </tbody>
      </table>

      <div style="margin-top: 1em;"
           layout="row"
           layout-align="center center"
           class="short-toolbar my-accent"
           flex="100">
        Paystub Information
      </div>
      <table class="paystub_table md-whiteframe-z1">
        <thead>
          <tr>
            <th>YTD Gross</th>
            <th>Current Earnings</th>
            <th>Pay Period Ending</th>
            <th>Pay Date</th>
            <th>Stub No</th>
          </tr>
        </thead>
        <tbody>
          <tr>
            <td>{{currentPaystub.year_to_date_gross.toFixed(2).toString()}}</td>
            <td>{{currentPaystub.total_earnings_amount}}</td>
            <td>{{currentPaystub.formatted_pay_period_ending}}</td>
            <td>{{currentPaystub.formatted_pay_date}}</td>
            <td>{{currentPaystub.check_number}}</td>
          </tr>
          <tr ng-show="currentPaystub.is_voided">
            <td colspan="5" style="text-align: center; font-weight: bold; color: red; font-size: x-large;">
              CHECK IS VOIDED
            </td>
          </tr>
        </tbody>
      </table>

      <div style="margin-top: 1em;"
           layout="row"
           layout-align="center center"
           class="short-toolbar my-accent"
           flex="100">
        Earnings
      </div>
      <table class="paystub_table md-whiteframe-z1">
        <thead>
          <tr>
            <th>Earnings</th>
            <th>Hours</th>
            <th>Payrate</th>
            <th>Amount</th>
          </tr>
        </thead>
        <tbody>
          <tr ng-repeat="e in currentPaystub.earnings">
            <td>{{e.pay_code_name}}</td>
            <td>{{e.hours.toFixed(2).toString()}}</td>
            <td>{{e.payrate}}</td>
            <td>{{e.amount}}</td>
          </tr>
        </tbody>
        <tfoot>
          <tr>
            <td></td>
            <td>
              {{ currentPaystub.total_earnings_hours }}
            </td>
            <td></td>
            <td>
              {{ currentPaystub.total_earnings_amount }}
            </td>
          </tr>
        </tfoot>
      </table>

      <div style="margin-top: 1em;"
           layout="row"
           layout-align="center center"
           class="short-toolbar my-accent"
           flex="100">
        Leave
      </div>
      <table class="paystub_table md-whiteframe-z1">
        <thead>
          <tr>
            <th>Leave</th>
            <th>Balance</th>
            <th>Taken YTD</th>
            <th>Earned YTD</th>
            <th>Accrual Rate</th>
            <th>Bank Maximum</th>
          </tr>
        </thead>
        <tbody>
          <tr ng-repeat="l in currentPaystub.leave">
            <td>{{l.leave_code_name}}</td>
            <td>{{l.leave_balance.toFixed(2).toString()}}</td>
            <td>{{l.leave_taken.toFixed(2).toString()}}</td>
            <td>{{l.leave_earned.toFixed(2).toString()}}</td>
            <td>{{l.calculated_accrual_rate.toFixed(2)}} hours/ppd</td>
            <td>{{l.bank_maximum > 9000 ? '' : l.bank_maximum.toString()}}</td>
          </tr>
        </tbody>

      </table>

      <div style="margin-top: 1em;"
           layout="row"
           layout-align="center center"
           class="short-toolbar my-accent"
           flex="100">
        Deductions
      </div>
      <table class="paystub_table md-whiteframe-z1">
        <thead>
          <tr>
            <th>Deductions</th>
            <th>Amount</th>
            <th>YTD Deduct</th>
            <th>Contribution</th>
          </tr>
        </thead>
        <tbody>
          <tr ng-repeat="d in currentPaystub.deductions">
            <td>{{d.ded_code_full_name}}</td>
            <td>{{d.amount.toFixed(2).toString()}}</td>
            <td>{{d.year_to_date_deductions.toFixed(2).toString()}}</td>
            <td>{{d.contributions.toFixed(2).toString()}}</td>
          </tr>
        </tbody>
        <tfoot>
          <tr>
            <td></td>
            <td>
              {{currentPaystub.total_deductions_amount}}
            </td>
            <td>
              {{currentPaystub.total_deductions_year_to_date}}
            </td>
            <td>
              {{currentPaystub.total_contributions}}
            </td>
          </tr>
        </tfoot>
      </table>



    </div>

    <div flex="90"
         layout="row"
         ng-show="currentPaystub === null"
         layout-align="center center"
         layout-wrap>
      No check data was found, or an error was encountered.  Please try again, and contact MIS if the problem continues.
    </div>

    <div id="paystubPrintView"
         flex="100">
      <table style="width: 100%;">
        <tbody>
          <tr>
            <td>
              <table style="width: 100%;">
                <tbody>
                  <tr>
                    <td style="width: 40%;">
                      <p style="padding-left: 2em; text-align: left;">
                        <strong>BOARD OF COUNTY COMMISSIONERS</strong><br />
                        STATE OF FLORIDA - CLAY COUNTY<br />
                        P.O. BOX 988<br />
                        GREEN COVE SPRINGS, FLORIDA 32043-0988<br />
                        PAYROLL CHECK
                      </p>
                    </td>
                    <td style="width: 20%; padding: 0 0 0 0; margin: 0;">
                      <div style="height: 100%; max-height: 128px; max-width: 128px;">
                        <img style="object-fit: contain; object-position: 50% 50%;  width: 100%; height: 100%;"
                             src="images/ClayCountySeal-258b.png" />
                      </div>
                    </td>
                    <td style="width: 40%;">
                      <table id="paystubHeader_right"
                             style="width: 100%;">
                        <thead>
                          <tr>
                            <td style="width: 20%;"></td>
                            <th style="width: 30%; border-bottom: none; text-align: center;">STUB DATE</th>
                            <th style="width: 30%; border-bottom: none; text-align: center;">STUB NO.</th>
                            <td style="width: 20%;"></td>
                          </tr>
                        </thead>
                        <tbody>
                          <tr>
                            <td></td>
                            <td>{{currentPaystub.formatted_pay_date}}</td>
                            <td>{{currentPaystub.check_number}}</td>
                            <td></td>
                          </tr>
                        </tbody>
                      </table>
                    </td>
                  </tr>
                  <tr>
                    <td style="padding-left: 4em; text-align: left;"
                        colspan="3">
                      <p>
                        {{currentPaystub.department}}<br />
                        {{currentPaystub.employee_name}}<br />
                        {{currentPaystub.address_line_1}}<br />
                        {{currentPaystub.address_line_2 }}<br ng-show="currentPaystub.address_line_2.length > 0" />
                        {{currentPaystub.address_line_3}}<br />
                      </p>
                    </td>
                  </tr>
                </tbody>
              </table>
            </td>
          </tr>
          <tr>
            <td></td>
          </tr>
          <tr>
            <td>
              <table style="width: 100%;">
                <tbody>
                  <tr ng-show="currentPaystub.is_voided">
                    <td colspan="2" style="text-align: center; font-weight: bold; font-size: x-large;">
                      CHECK IS VOIDED
                    </td>
                  </tr>
                  <tr>
                    <td style="text-align: left;">
                      CLAY COUNTY BOARD OF COMMISSIONERS
                    </td>
                    <td style="text-align: right;">
                      {{currentPaystub.employee_name}}
                    </td>
                  </tr>
                  <tr>
                    <td style="text-align: center; font-size: smaller;"
                        colspan="2">
                      STATEMENT OF EARNINGS AND DEDUCTIONS
                    </td>
                  </tr>
                </tbody>
              </table>
            </td>
          </tr>
          <tr>
            <td>
              <table style="width: 100%; border: 2px solid black;">
                <tbody>
                  <tr>
                    <td style="width: 30%; vertical-align: top; padding-right: 0;">
                      <table style="width: 100%; border-spacing: 0;">
                        <thead>
                          <tr>
                            <th style="width: 40%;">
                              Earnings
                            </th>
                            <th style="width: 20%;">
                              Hours
                            </th>
                            <th style="width: 40%;">
                              Amount
                            </th>
                          </tr>
                        </thead>
                        <tbody>
                          <tr ng-repeat="e in currentPaystub.earnings">
                            <td>{{e.pay_code_short_name}}</td>
                            <td>{{e.hours.toFixed(2).toString()}}</td>
                            <td>{{e.amount}}</td>
                          </tr>
                          <tr>
                            <td style="height: 22px;"></td>
                            <td style="height: 22px;"></td>
                            <td style="height: 22px;"></td>
                          </tr>
                        </tbody>
                        <tfoot>
                          <tr>
                            <td></td>
                            <td>
                              {{ currentPaystub.total_earnings_hours }}
                            </td>
                            <td>
                              {{ currentPaystub.total_earnings_amount }}
                            </td>
                          </tr>
                        </tfoot>
                      </table>
                      <table style="width: 100%; margin-top: 1em;">
                        <thead>
                          <tr>
                            <th style="width: 40%;">
                              Leave
                            </th>
                            <th style="width: 20%;">
                              Balance
                            </th>
                            <th style="width: 40%;">
                              Taken YTD
                            </th>
                          </tr>
                        </thead>
                        <tbody>
                          <tr ng-repeat="l in currentPaystub.leave">
                            <td>{{l.leave_code_short_name}}</td>
                            <td>{{l.leave_balance.toFixed(2).toString()}}</td>
                            <td>{{l.leave_taken.toFixed(2).toString()}}</td>
                          </tr>
                        </tbody>
                      </table>

                    </td>
                    <td style="width: 50%; vertical-align: top; border-left: 1px solid black; padding-right: 0;">

                      <table style="width: 100%; border-spacing: 0;">
                        <thead>
                          <tr>
                            <th>Deductions</th>
                            <th>Amount</th>
                            <th>YTD Deduct</th>
                            <th>Contribution</th>
                          </tr>
                        </thead>
                        <tbody>
                          <tr ng-repeat="d in currentPaystub.deductions">
                            <td>{{d.ded_code_short_name}}</td>
                            <td>{{d.amount.toFixed(2).toString()}}</td>
                            <td>{{d.year_to_date_deductions.toFixed(2).toString()}}</td>
                            <td>{{d.contributions.toFixed(2).toString()}}</td>
                          </tr>
                          <tr>
                            <td style="height: 22px;"></td>
                            <td style="height: 22px;"></td>
                            <td style="height: 22px;"></td>
                            <td style="height: 22px;"></td>
                          </tr>
                        </tbody>
                        <tfoot>
                          <tr>
                            <td>
                              Total
                            </td>
                            <td>
                              {{currentPaystub.total_deductions_amount}}
                            </td>
                            <td>
                              {{currentPaystub.total_deductions_year_to_date}}
                            </td>
                            <td>
                              {{currentPaystub.total_contributions}}
                            </td>
                          </tr>
                        </tfoot>
                      </table>


                    </td>
                    <td style="width: 20%; vertical-align: top; border-left: 1px solid black;">
                      <table style="width: 100%;">
                        <tbody>
                          <tr>
                            <th>YTD Gross</th>
                          </tr>
                          <tr>
                            <td>{{currentPaystub.year_to_date_gross.toFixed(2).toString()}}</td>
                          </tr>
                          <tr>
                            <th>Current Earnings</th>
                          </tr>
                          <tr>
                            <td>{{currentPaystub.total_earnings_amount}}</td>
                          </tr>
                          <tr>
                            <th>Pay Period Ending</th>
                          </tr>
                          <tr>
                            <td>{{currentPaystub.formatted_pay_period_ending}}</td>
                          </tr>
                          <tr>
                            <th>Pay Date</th>
                          </tr>
                          <tr>
                            <td>{{currentPaystub.formatted_pay_date}}</td>
                          </tr>
                          <tr>
                            <th>Stub No</th>
                          </tr>
                          <tr>
                            <td>{{currentPaystub.check_number}}</td>
                          </tr>
                        </tbody>
                      </table>

                    </td>
                  </tr>
                  <tr>
                    <td>

                    </td>
                  </tr>
                </tbody>
              </table>


            </td>
          </tr>
        </tbody>
      </table>
    </div>

  </div>
</script>

<script type="text/ng-template" id="PayrollOverall.tmpl.html">


  <div flex="100"
       layout="row"
       layout-align="center start"
       layout-wrap
       layout-margin>

    <div flex="90"
         class="md-whiteframe-z1">
      <date-selector title="Pay Period Ending:"
                     label="Select Pay Period"
                     datetype="ppd"
                     flex="100">
      </date-selector>
    </div>

    <div style="margin-top: 3em;"
         layout="row"
         layout-align="start center"
         class="md-whiteframe-z1"
         layout-wrap
         flex="90">
      <div layout="row"
           layout-align="center center"
           class="short-toolbar my-accent"
           flex-gt-md="50"
           flex="100">
        <h5 style="text-align: center;">
          Set up Payroll Process
        </h5>
      </div>
      <div flex="50"
           layout="row"
           layout-align="space-around center">
        <md-input-container flex="35">
          <label>Target Database</label>
          <md-select ng-disabled="!currentStatus.can_start"
                     ng-model="currentStatus.target_db"
                     multiple="false"
                     aria-label="Select the target Database">
            <md-option value="0">
              Production (finplus51)
            </md-option>
            <md-option value="1">
              Training (trnfinplus51)
            </md-option>
          </md-select>
        </md-input-container>
        <md-input-container flex="40"
                            class="md-input-has-value"
                            layout="row"
                            layout-align="space-around center"
                            layout-wrap>
          <label>
            Include Pay Benefits (pay_group S)
          </label>
          <md-radio-group ng-model="currentStatus.include_benefits"
                          ng-disabled="!currentStatus.can_start"
                          layout="row"
                          layout-align="center center"
                          layout-wrap>
            <md-radio-button ng-value="true">
              Yes (Default)
            </md-radio-button>
            <md-radio-button ng-value="false">
              No
              <md-tooltip>
                This should probably be answered no if this is the third paycheck that will be paid this month.
              </md-tooltip>
            </md-radio-button>
          </md-radio-group>
        </md-input-container>
        <md-button class="md-primary md-raised"
                   ng-click="StartPayroll()"
                   ng-show="currentStatus.can_start && !StartOrResetInProgress">
          Start
        </md-button>
        <md-button class="md-primary md-raised"
                   ng-click="ResetPayroll()"
                   ng-show="currentStatus.can_reset && !StartOrResetInProgress">
          Reset Data
          <md-tooltip>
            This button should only be used when you know the data in Finplus has been changed since this process was originally started.
          </md-tooltip>
        </md-button>
        <md-progress-circular ng-if="StartOrResetInProgress"
                              md-mode="indeterminate">
        </md-progress-circular>
      </div>
      <div flex="100">
        <p style="padding-left: 1em;"
           ng-if="currentStatus.started_by.length > 0">
          Started by {{ currentStatus.started_by }} on {{ currentStatus.started_on_display }}
        </p>
      </div>
    </div>

    <div layout="row"
         layout-align="start center"
         layout-wrap
         class="md-whiteframe-z1"
         flex="90">
      <div layout="row"
           layout-align="center center"
           class="short-toolbar my-accent"
           flex-gt-md="50"
           flex="100">
        <h5 style="text-align: center;">
          View / Edit Timestore / Finplus Data
        </h5>
      </div>
      <div flex="50"
           layout="row"
           layout-align="center center">
        <md-button ng-click="ViewEdits()"
                   ng-disabled="currentStatus.started_by.length === 0"
                   class="md-primary md-raised">
          View Edits
        </md-button>
        <md-button ng-disabled="!currentStatus.can_edit || currentStatus.edits_completed_by.length > 0"
                   ng-click="EditsCompleted()"
                   class="md-warn md-raised">
          Mark Edits Completed
        </md-button>
        <md-button ng-show="currentStatus.edits_completed_by.length > 0 && currentStatus.edits_approved_by.length === 0"
                   ng-click="MarkEditsIncomplete()"
                   class="md-warn md-raised">
          Mark Edits Incomplete
        </md-button>
      </div>
      <div flex="100">
        <p ng-if="currentStatus.edits_completed_by.length > 0"
           style="padding-left: 1em;">
          Edits Completed by {{ currentStatus.edits_completed_by }} on {{ currentStatus.edits_completed_on_display }}
        </p>
      </div>
    </div>

    <div layout="row"
         layout-align="start center"
         layout-wrap
         class="md-whiteframe-z1"
         flex="90">
      <div layout="row"
           layout-align="center center"
           class="short-toolbar my-accent"
           flex-gt-md="50"
           flex="100">
        <h5 style="text-align: center;">
          Approve Timestore Payroll changes
        </h5>
      </div>
      <div flex="50"
           layout="row"
           layout-align="center center">
        <md-button ng-disabled="currentStatus.started_by.length === 0"
                   ng-click="ViewChanges()"
                   class="md-primary md-raised">
          View Changes
        </md-button>
        <md-button ng-click="ChangesApproved()"
                   ng-disabled="currentStatus.edits_completed_by.length === 0 || currentStatus.edits_approved_by.length > 0"
                   class="md-warn md-raised">
          Approve All Changes
        </md-button>
        <md-button ng-show="currentStatus.edits_approved_by.length > 0"
                   ng-click="CancelApproval()"
                   class="md-warn md-raised">
          Cancel Approval
        </md-button>
      </div>
      <div flex="100">
        <p ng-if="currentStatus.edits_approved_by.length > 0"
           style="padding-left: 1em;">
          Edits Approved by {{ currentStatus.edits_approved_by }} on {{ currentStatus.edits_approved_on_display }}
        </p>
        <!--<ol style="padding-right: 1em;">
          <li>
            This action can only be performed after the completion of the View / Edit Timestore / Finplus data step.
          </li>
          <li>
            This is done by the Finance director or their designee
          </li>
          <li>
            This is the last step to be performed by the BCC.  When this is completed, the data is now available to be sent to Finplus.
          </li>
        </ol>-->
      </div>
    </div>

    <div layout="row"
         layout-align="start center"
         layout-wrap
         class="md-whiteframe-z1"
         flex="90">
      <div layout="row"
           layout-align="center center"
           class="short-toolbar my-accent"
           flex-gt-md="50"
           flex="100">
        <h5 style="text-align: center;">
          Post Timestore Data / Changes to Finplus
        </h5>
      </div>
      <div flex="50"
           layout="row"
           layout-wrap
           layout-align="center center">

        <md-input-container flex="55">
          <label>Select Pay Run</label>
          <!--ng-disabled="!currentStatus.can_update_finplus"-->
          <md-select ng-model="postPayrun"
                     multiple="false"
                     md-on-close="selectPayRun()"
                     aria-label="Select the target payrun">
            <md-option ng-value="">
              No payrun
            </md-option>
            <md-option ng-repeat="p in payruns track by $index"
                       ng-value="p">
              {{p}}
            </md-option>
          </md-select>
        </md-input-container>
        <md-button ng-click="GetPayruns()"
                   class="md-warn md-raised">
          Refresh Pay Runs
        </md-button>
        <md-button ng-click="PostTimestoreData()"
                   ng-disabled="!currentStatus.can_update_finplus || !postPayrun"
                   class="md-primary md-raised">
          Post to Finplus
        </md-button>
      </div>
      <div flex="100">
        <p ng-if="currentStatus.finplus_updated_by.length > 0"
           style="padding-left: 1em;">
          Data Posted to Finplus by {{ currentStatus.finplus_updated_by }} on {{ currentStatus.finplus_updated_on_display }}
        </p>
        <p ng-if="payruns.length === 0 && currentStatus.can_update_finplus">
          No pay runs were found in Finplus.  It doesn't look like the pay roll has been started there.
        </p>
      </div>
    </div>

  </div>


</script>
<script type="text/ng-template" id="PayrollEdit.tmpl.html">
  
    <md-progress-linear ng-if="loading"
                        flex="100"
                        md-mode="indeterminate">
    </md-progress-linear>
    <fieldset style="border: 1px solid #cccccc; background-color: #efefef;"
              layout="row"
              layout-align="start center"
              flex="100">
      <legend style="padding-left: 1em; padding-right: 1em;">Filters</legend>
      <md-input-container style="margin-bottom: -1em; margin-top: .5em;"
                          flex="30">
        <label>
          Department Name / Number
        </label>
        <input type="text"
               ng-model="filter_department"
               ng-model-options="{ debounce: 300}"
               ng-change="applyFilters()"
               class="md-input short" />
      </md-input-container>
      <md-input-container style="margin-bottom: -1em; margin-top: .5em;"
                          flex="30">
        <label>
          Employee Name / Number
        </label>
        <input type="text"
               class="md-input short"
               ng-model-options="{ debounce: 300}"
               ng-model="filter_employee"
               ng-change="applyFilters()" />
      </md-input-container>
      <md-input-container flex="20">
        <label>
          Filter By Error
        </label>
        <md-select ng-change="applyFilters()"
                   aria-label="Filter by Error"
                   ng-model="filter_error_message">
          <md-option ng-value="''">
            None
          </md-option>
          <md-option ng-repeat="m in error_messages"
                     ng-value="m">
            {{m}}
          </md-option>
        </md-select>
      </md-input-container>
      <span flex></span>
      <md-button ng-click="returnToOverallProcess()"
                 class="md-button md-raised md-primary">Return to Process</md-button>
    </fieldset>
    <!--<div>
      Sort by Department, employee name
      Group rows by Employee name
      Filter by has errors, has specific error,
      has unmarked error (you can mark an error to show that it does not require fixing.)

    </div>-->


    <payroll-edit-group ng-repeat="pd in filtered_payroll_edits track by pd.employee.EmployeeId"
                        ped="pd"
                        paycodes="paycodeslist"
                        projectcodes="project_codes"
                        flex="100">
    </payroll-edit-group>

</script>

<script type="text/ng-template" id="PayrollReview.tmpl.html">
  <md-progress-linear ng-if="loading"
                      flex="100"
                      md-mode="indeterminate">
  </md-progress-linear>
  <fieldset style="border: 1px solid #cccccc; background-color: #efefef;"
            layout="row"
            layout-align="start center"
            class="hide-print"
            flex="100">
    <legend style="padding-left: 1em; padding-right: 1em;">Filters</legend>
    <md-input-container style="margin-bottom: -1em; margin-top: .5em;"
                        flex="30">
      <label>
        Department Name / Number
      </label>
      <input type="text"
             ng-model="filter_department"
             ng-model-options="{ debounce: 300}"
             ng-change="applyFilters()"
             class="md-input short" />
    </md-input-container>
    <md-input-container style="margin-bottom: -1em; margin-top: .5em;"
                        flex="30">
      <label>
        Employee Name / Number
      </label>
      <input type="text"
             class="md-input short"
             ng-model-options="{ debounce: 300}"
             ng-model="filter_employee"
             ng-change="applyFilters()" />
    </md-input-container>
    <md-checkbox ng-change="applyFilters()"
                 ng-model="changes_only" 
                 aria-label="Show Changes Only?">
      Show Changes Only?
    </md-checkbox>
    <span flex></span>
    <md-button ng-click="returnToOverallProcess()"
               class="md-button md-raised md-primary">Return to Process</md-button>
  </fieldset>

  <div ng-repeat="ped in filtered_payroll_edits track by ped.employee.EmployeeId"
       id="reviewgroup{{ped.employee.EmployeeId}}"
       layout="row"
       layout-wrap
       style="background-color: #efefef;"
       flex="100">
    <div style="padding-left: 1em; padding-right: 1em; height: 2em;"
         class="my-accent"
         layout="row"
         layout-align="start center"
         layout-wrap
         flex="100">

      <span flex="30">{{ped.employee.DepartmentName}} ({{ped.employee.Department}}) </span>
      <a flex="20"
         style="padding-left: 1em; color: white;"
         target="_blank"
         rel="nofollow noopener"
         href="#/e/{{ped.employee.EmployeeId}}/ppd/{{payPeriod}}">
        {{ped.employee.EmployeeName}} ({{ped.employee.EmployeeId}})<!--</span><span flex="20" style="padding-left: 1em;">-->
      </a>
      <span flex="10">{{ ped.employee.isFulltime ? 'Full time' : 'Part time';}}</span>
      <span flex="10">{{ ped.employee.IsExempt ? 'Exempt' : 'Non Exempt';}}</span>
    </div>
    <div style="padding-top: 1em; padding-bottom: 1em;"
         flex="100">
      <table style="width: 100%; border-collapse: collapse;">
        <thead>
          <tr>
            <th colspan=2 style="width: 30%;"></th>
            <th colspan=2 style="width: 10%; text-align: center;">Hours</th>
            <th colspan=2 style="width: 20%; text-align: center;">Pay Rate</th>
            <th colspan=2 style="width: 10%; text-align: center;">Amount</th>
            <th colspan=2 style="width: 10%; text-align: center;">Classify</th>
            <th colspan=2 style="width: 20%; text-align: center;">Project Code</th>
          </tr>
          <tr>
            <th style="width: 25%;">Pay Code</th>
            <th style="width: 5%; text-align: right;">Status</th>
            <th style="width: 5%; text-align: right;">Before</th>
            <th style="width: 5%; text-align: right;">After</th>
            <th style="width: 10%; text-align: right;">Before</th>
            <th style="width: 10%; text-align: right;">After</th>
            <th style="width: 5%; text-align: right;">Before</th>
            <th style="width: 5%; text-align: right;">After</th>
            <th style="width: 5%; text-align: right;">Before</th>
            <th style="width: 5%; text-align: right;">After</th>
            <th style="width: 10%; text-align: right;">Before</th>
            <th style="width: 10%; text-align: right; padding-right: .5em;">After</th>
          </tr>
        </thead>
        <tbody>
          <tr ng-repeat="c in ped.comparisons track by $index"
              style="background-color: {{c.status === 'Same' ? 'White' : '#ffffE0'}};">
            <td style="padding-left: .5em;">
              {{ GetPaycode(c) }}
            </td>
            <td style="text-align: right;">{{c.status}}</td>
            <td style="text-align: right;">{{c.original ? c.original.hours.toFixed(2) : 0}}</td>
            <td style="text-align: right;">{{c.changed ? c.changed.hours.toFixed(2) : 0}}</td>
            <td style="text-align: right;">{{c.original ? c.original.payrate : 0}}</td>
            <td style="text-align: right;">{{c.changed ? c.changed.payrate : 0}}</td>
            <td style="text-align: right;">{{c.original ? c.original.amount.toFixed(2) : 0}}</td>
            <td style="text-align: right;">{{c.changed ? c.changed.amount.toFixed(2) : 0}}</td>
            <td style="text-align: right;">{{c.original ? c.original.classify : ''}}</td>
            <td style="text-align: right;">{{c.changed ? c.changed.classify : ''}}</td>
            <td style="text-align: right;">{{c.original ? c.original.project_code : ''}}</td>
            <td style="text-align: right; padding-right: .5em;">{{c.changed ? c.changed.project_code : ''}}</td>
          </tr>

        </tbody>
        <tfoot style="border-top: 2px solid black;">
          <tr>
            <td style="text-align: left; padding-left: .5em;" colspan="2">Totals</td>
            <td style="text-align: right;">{{GetTotalOriginalHours(ped.comparisons)}}</td>
            <td style="text-align: right;">{{GetTotalChangedHours(ped.comparisons)}}</td>
            <td colspan="2"></td>
            <td style="text-align: right;">{{GetTotalOriginalAmount(ped.comparisons)}}</td>
            <td style="text-align: right;">{{GetTotalChangedAmount(ped.comparisons)}}</td>
            <td colspan="4"></td>
          </tr>
        </tfoot>
      </table>
      <table ng-if="ped.justifications.length > 0"
             style="width: 100%; border-collapse: collapse; margin-top: 1em;">
        <thead>
          <tr>
            <th style="text-align: center;">Justifications</th>
          </tr>
        </thead>
          <tbody>
            <tr ng-repeat="j in ped.justifications">
              <td>
                <div style="width: 100%; white-space: pre-wrap; border: 1px solid black;">
                  {{j.justification}}
                </div>
              </td>
            </tr>
          </tbody>
      </table>

    </div>

  </div>


</script>


<script type="text/ng-template" id="PaystubView.controller.tmpl.html">

 <link rel="stylesheet" href="css/paystubPrint.css" />

  <div flex="100"
       layout="row"
       layout-align="center start"
       layout-wrap
       layout-margin>

    <div flex="90"
         layout="row"
         id="paystubSelector"
         ng-show="paystubList.length > 0"
         layout-align="center center">

      <md-input-container flex="25">
        <label>Select a Check</label>
        <md-select ng-model="checkNumber"
                   multiple="false"
                   md-on-close="selectCheck()"
                   aria-label="Select A Check">
          <md-option ng-repeat="p in filtered_paystub_list track by $index"
                     ng-value="p.check_number">
            <span style="text-align: center;">
              {{ FormatDate(p.check_date) }} - {{ p.check_number }} {{ p.is_voided ? '(VOIDED)' : ''}}
            </span>
          </md-option>
        </md-select>
      </md-input-container>

      <md-input-container flex="25">
        <label>Filter Pay Stubs by Year</label>
        <md-select ng-model="filter_year"
                   multiple="false"
                   md-on-close="selectYear()"
                   aria-label="Select A Year">
          <md-option value="">
            Show All
          </md-option>
          <md-option ng-repeat="p in paystub_years track by $index"
                     ng-value="p">
            <span style="text-align: center;">
              {{ p }}
            </span>
          </md-option>
        </md-select>
      </md-input-container>

      <div layout="row"
           layout-align="end center"
           flex="100">
        <md-button aria-label="Print Paystub"
                   onclick="window.print();"                   
                   class="md-button md-accent md-raised">
          Print
          <md-icon>
            <svg class="fabWhite"
                 xmlns="http://www.w3.org/2000/svg" height="24" viewBox="0 0 24 24" width="24"><path d="M0 0h24v24H0z" fill="none" /><path d="M19 8H5c-1.66 0-3 1.34-3 3v6h4v4h12v-4h4v-6c0-1.66-1.34-3-3-3zm-3 11H8v-5h8v5zm3-7c-.55 0-1-.45-1-1s.45-1 1-1 1 .45 1 1-.45 1-1 1zm-1-9H6v4h12V3z" /></svg>
          </md-icon>

          <md-tooltip md-direction="bottom">
            Print the paystub being viewed
          </md-tooltip>
        </md-button>
        <md-button ng-click="returnToTimeStore()"
                   class="md-raised md-primary">
          Return to TimeStore
        </md-button>
      </div>
    </div>

    <div id="paystubDetailedView"
         flex="90"
         layout="row"
         ng-show="currentPaystub !== null"
         layout-align="center center"
         layout-wrap>


      <table class="paystub_table md-whiteframe-z1">
        <thead>
          <tr>
            <th style="width: 50%; text-align: left; padding-left: 1em;">
              Employee Name
            </th>
            <th style="width: 50%; text-align: left; padding-left: 1em;">
              Department
            </th>
          </tr>
        </thead>
        <tbody>
          <tr>
            <td style="text-align: left; padding-left: 1em;">
              {{currentPaystub.employee_name}}
            </td>
            <td style="text-align: left; padding-left: 1em;">
              {{currentPaystub.department}}
            </td>
          </tr>
        </tbody>
      </table>

      <div style="margin-top: 1em;"
           layout="row"
           layout-align="center center"
           class="short-toolbar my-accent"
           flex="100">
        Paystub Information
      </div>
      <table class="paystub_table md-whiteframe-z1">
        <thead>
          <tr>
            <th>YTD Gross</th>
            <th>Current Earnings</th>
            <th>Pay Period Ending</th>
            <th>Pay Date</th>
            <th>Stub No</th>
          </tr>
        </thead>
        <tbody>
          <tr>
            <td>{{currentPaystub.year_to_date_gross.toFixed(2).toString()}}</td>
            <td>{{currentPaystub.total_earnings_amount}}</td>
            <td>{{currentPaystub.formatted_pay_period_ending}}</td>
            <td>{{currentPaystub.formatted_pay_date}}</td>
            <td>{{currentPaystub.check_number}}</td>
          </tr>
          <tr ng-show="currentPaystub.is_voided">
            <td colspan="5" style="text-align: center; font-weight: bold; color: red; font-size: x-large;">
              CHECK IS VOIDED
            </td>
          </tr>
        </tbody>
      </table>

      <div style="margin-top: 1em;"
           layout="row"
           layout-align="center center"
           class="short-toolbar my-accent"
           flex="100">
        Earnings
      </div>
      <table class="paystub_table md-whiteframe-z1">
        <thead>
          <tr>
            <th>Earnings</th>
            <th>Hours</th>
            <th>Payrate</th>
            <th>Amount</th>
          </tr>
        </thead>
        <tbody>
          <tr ng-repeat="e in currentPaystub.earnings">
            <td>{{e.pay_code_name}}</td>
            <td>{{e.hours.toFixed(2).toString()}}</td>
            <td>{{e.payrate}}</td>
            <td>{{e.amount}}</td>
          </tr>
        </tbody>
        <tfoot>
          <tr>
            <td></td>
            <td>
              {{ currentPaystub.total_earnings_hours }}
            </td>
            <td></td>
            <td>
              {{ currentPaystub.total_earnings_amount }}
            </td>
          </tr>
        </tfoot>
      </table>

      <div style="margin-top: 1em;"
           layout="row"
           layout-align="center center"
           class="short-toolbar my-accent"
           flex="100">
        Leave
      </div>
      <table class="paystub_table md-whiteframe-z1">
        <thead>
          <tr>
            <th>Leave</th>
            <th>Balance</th>
            <th>Taken YTD</th>
            <th>Earned YTD</th>
            <th>Accrual Rate</th>
            <th>Bank Maximum</th>
          </tr>
        </thead>
        <tbody>
          <tr ng-repeat="l in currentPaystub.leave">
            <td>{{l.leave_code_name}}</td>
            <td>{{l.leave_balance.toFixed(2).toString()}}</td>
            <td>{{l.leave_taken.toFixed(2).toString()}}</td>
            <td>{{l.leave_earned.toFixed(2).toString()}}</td>
            <td>{{l.calculated_accrual_rate.toFixed(2)}} hours/ppd</td>
            <td>{{l.bank_maximum > 9000 ? '' : l.bank_maximum.toString()}}</td>
          </tr>
        </tbody>

      </table>

      <div style="margin-top: 1em;"
           layout="row"
           layout-align="center center"
           class="short-toolbar my-accent"
           flex="100">
        Deductions
      </div>
      <table class="paystub_table md-whiteframe-z1">
        <thead>
          <tr>
            <th>Deductions</th>
            <th>Amount</th>
            <th>YTD Deduct</th>
            <th>Contribution</th>
          </tr>
        </thead>
        <tbody>
          <tr ng-repeat="d in currentPaystub.deductions">
            <td>{{d.ded_code_full_name}}</td>
            <td>{{d.amount.toFixed(2).toString()}}</td>
            <td>{{d.year_to_date_deductions.toFixed(2).toString()}}</td>
            <td>{{d.contributions.toFixed(2).toString()}}</td>
          </tr>
        </tbody>
        <tfoot>
          <tr>
            <td></td>
            <td>
              {{currentPaystub.total_deductions_amount}}
            </td>
            <td>
              {{currentPaystub.total_deductions_year_to_date}}
            </td>
            <td>
              {{currentPaystub.total_contributions}}
            </td>
          </tr>
        </tfoot>
      </table>



    </div>

    <div flex="90"
         layout="row"
         ng-show="currentPaystub === null"
         layout-align="center center"
         layout-wrap>
      No check data was found, or an error was encountered.  Please try again, and contact MIS if the problem continues.
    </div>

    <div id="paystubPrintView"
         flex="100">
      <table style="width: 100%;">
        <tbody>
          <tr>
            <td>
              <table style="width: 100%;">
                <tbody>
                  <tr>
                    <td style="width: 40%;">
                      <p style="padding-left: 2em; text-align: left;">
                        <strong>BOARD OF COUNTY COMMISSIONERS</strong><br />
                        STATE OF FLORIDA - CLAY COUNTY<br />
                        P.O. BOX 988<br />
                        GREEN COVE SPRINGS, FLORIDA 32043-0988<br />
                        PAYROLL CHECK
                      </p>
                    </td>
                    <td style="width: 20%; padding: 0 0 0 0; margin: 0;">
                      <div style="height: 100%; max-height: 128px; max-width: 128px;">
                        <img style="object-fit: contain; object-position: 50% 50%;  width: 100%; height: 100%;"
                             src="images/ClayCountySeal-258b.png" />
                      </div>
                    </td>
                    <td style="width: 40%;">
                      <table id="paystubHeader_right"
                             style="width: 100%;">
                        <thead>
                          <tr>
                            <td style="width: 20%;"></td>
                            <th style="width: 30%; border-bottom: none; text-align: center;">STUB DATE</th>
                            <th style="width: 30%; border-bottom: none; text-align: center;">STUB NO.</th>
                            <td style="width: 20%;"></td>
                          </tr>
                        </thead>
                        <tbody>
                          <tr>
                            <td></td>
                            <td>{{currentPaystub.formatted_pay_date}}</td>
                            <td>{{currentPaystub.check_number}}</td>
                            <td></td>
                          </tr>
                        </tbody>
                      </table>
                    </td>
                  </tr>
                  <tr>
                    <td style="padding-left: 4em; text-align: left;"
                        colspan="3">
                      <p>
                        {{currentPaystub.department}}<br />
                        {{currentPaystub.employee_name}}<br />
                        {{currentPaystub.address_line_1}}<br />
                        {{currentPaystub.address_line_2 }}<br ng-show="currentPaystub.address_line_2.length > 0" />
                        {{currentPaystub.address_line_3}}<br />
                      </p>
                    </td>
                  </tr>
                </tbody>
              </table>
            </td>
          </tr>
          <tr>
            <td></td>
          </tr>
          <tr>
            <td>
              <table style="width: 100%;">
                <tbody>
                  <tr ng-show="currentPaystub.is_voided">
                    <td colspan="2" style="text-align: center; font-weight: bold; font-size: x-large;">
                      CHECK IS VOIDED
                    </td>
                  </tr>
                  <tr>
                    <td style="text-align: left;">
                      CLAY COUNTY BOARD OF COMMISSIONERS
                    </td>
                    <td style="text-align: right;">
                      {{currentPaystub.employee_name}}
                    </td>
                  </tr>
                  <tr>
                    <td style="text-align: center; font-size: smaller;"
                        colspan="2">
                      STATEMENT OF EARNINGS AND DEDUCTIONS
                    </td>
                  </tr>
                </tbody>
              </table>
            </td>
          </tr>
          <tr>
            <td>
              <table style="width: 100%; border: 2px solid black;">
                <tbody>
                  <tr>
                    <td style="width: 30%; vertical-align: top; padding-right: 0;">
                      <table style="width: 100%; border-spacing: 0;">
                        <thead>
                          <tr>
                            <th style="width: 40%;">
                              Earnings
                            </th>
                            <th style="width: 20%;">
                              Hours
                            </th>
                            <th style="width: 40%;">
                              Amount
                            </th>
                          </tr>
                        </thead>
                        <tbody>
                          <tr ng-repeat="e in currentPaystub.earnings">
                            <td>{{e.pay_code_short_name}}</td>
                            <td>{{e.hours.toFixed(2).toString()}}</td>
                            <td>{{e.amount}}</td>
                          </tr>
                          <tr>
                            <td style="height: 22px;"></td>
                            <td style="height: 22px;"></td>
                            <td style="height: 22px;"></td>
                          </tr>
                        </tbody>
                        <tfoot>
                          <tr>
                            <td></td>
                            <td>
                              {{ currentPaystub.total_earnings_hours }}
                            </td>
                            <td>
                              {{ currentPaystub.total_earnings_amount }}
                            </td>
                          </tr>
                        </tfoot>
                      </table>
                      <table style="width: 100%; margin-top: 1em;">
                        <thead>
                          <tr>
                            <th style="width: 40%;">
                              Leave
                            </th>
                            <th style="width: 20%;">
                              Balance
                            </th>
                            <th style="width: 40%;">
                              Taken YTD
                            </th>
                          </tr>
                        </thead>
                        <tbody>
                          <tr ng-repeat="l in currentPaystub.leave">
                            <td>{{l.leave_code_short_name}}</td>
                            <td>{{l.leave_balance.toFixed(2).toString()}}</td>
                            <td>{{l.leave_taken.toFixed(2).toString()}}</td>
                          </tr>
                        </tbody>
                      </table>

                    </td>
                    <td style="width: 50%; vertical-align: top; border-left: 1px solid black; padding-right: 0;">

                      <table style="width: 100%; border-spacing: 0;">
                        <thead>
                          <tr>
                            <th>Deductions</th>
                            <th>Amount</th>
                            <th>YTD Deduct</th>
                            <th>Contribution</th>
                          </tr>
                        </thead>
                        <tbody>
                          <tr ng-repeat="d in currentPaystub.deductions">
                            <td>{{d.ded_code_short_name}}</td>
                            <td>{{d.amount.toFixed(2).toString()}}</td>
                            <td>{{d.year_to_date_deductions.toFixed(2).toString()}}</td>
                            <td>{{d.contributions.toFixed(2).toString()}}</td>
                          </tr>
                          <tr>
                            <td style="height: 22px;"></td>
                            <td style="height: 22px;"></td>
                            <td style="height: 22px;"></td>
                            <td style="height: 22px;"></td>
                          </tr>
                        </tbody>
                        <tfoot>
                          <tr>
                            <td>
                              Total
                            </td>
                            <td>
                              {{currentPaystub.total_deductions_amount}}
                            </td>
                            <td>
                              {{currentPaystub.total_deductions_year_to_date}}
                            </td>
                            <td>
                              {{currentPaystub.total_contributions}}
                            </td>
                          </tr>
                        </tfoot>
                      </table>


                    </td>
                    <td style="width: 20%; vertical-align: top; border-left: 1px solid black;">
                      <table style="width: 100%;">
                        <tbody>
                          <tr>
                            <th>YTD Gross</th>
                          </tr>
                          <tr>
                            <td>{{currentPaystub.year_to_date_gross.toFixed(2).toString()}}</td>
                          </tr>
                          <tr>
                            <th>Current Earnings</th>
                          </tr>
                          <tr>
                            <td>{{currentPaystub.total_earnings_amount}}</td>
                          </tr>
                          <tr>
                            <th>Pay Period Ending</th>
                          </tr>
                          <tr>
                            <td>{{currentPaystub.formatted_pay_period_ending}}</td>
                          </tr>
                          <tr>
                            <th>Pay Date</th>
                          </tr>
                          <tr>
                            <td>{{currentPaystub.formatted_pay_date}}</td>
                          </tr>
                          <tr>
                            <th>Stub No</th>
                          </tr>
                          <tr>
                            <td>{{currentPaystub.check_number}}</td>
                          </tr>
                        </tbody>
                      </table>

                    </td>
                  </tr>
                  <tr>
                    <td>

                    </td>
                  </tr>
                </tbody>
              </table>


            </td>
          </tr>
        </tbody>
      </table>
    </div>

  </div>
</script>

<script type="text/ng-template" id="DisasterHours.directive.tmpl.html">


  <div layout="row"
       layout-align="center center"
       layout-wrap
       flex="100">
    <div style="margin-top: .75em; margin-bottom: .75em;"
         class="short-toolbar my-primary"
         flex="100"
         layout="row"
         layout-align="center center">
      <span flex="5"></span>
      <h5>
        Work Hours For {{event.event_name}}
      </h5>
    </div>
    <div layout="row"
         flex="35"
         layout-padding
         layout-align="center center">
      <md-input-container flex="100">
        <label>Hours Worked on {{event.event_name}}</label>
        <md-select md-on-close="calc()"
                   flex="80"
                   multiple
                   ng-model="event.disaster_work_hours.DisasterSelectedTimes"
                   placeholder="Hours Worked on {{event.event_name}}"
                   class="bigpaddingbottom">
          <md-option ng-repeat="t in fulltimelist track by t.index"
                     ng-value="t.index">
            <span class="md-text">
              {{t.display}}
            </span>
          </md-option>
        </md-select>
      </md-input-container>
    </div>

    <div layout="row"
         layout-align="center center"
         layout-gt-md="row"
         layout-align-gt-md="space-around center"
         layout-wrap
         flex="65">
      <!--<md-button ng-click="CopyWorkHoursToDisasterWorkHours()"
                 class="md-primary md-raised">
        Copy Work Hours
      </md-button>-->
      <md-input-container class="shortpaddingbottom shortInput">
        <label class="longerLabel"
               style="width: auto;">Hours Worked</label>
        <input ng-model="event.disaster_work_hours.DisasterWorkHours"
               type="number"
               ng-disabled="true" />
      </md-input-container>
      <div layout="row"
           layout-align="center center"
           layout-padding
           flex="40">
        <md-input-container flex="100">
          <label>Disaster Work Type</label>
          <md-select md-on-close="calc()"
                     flex="100"
                     ng-model="event.disaster_work_hours.DisasterWorkType"
                     placeholder="Work Type for {{event.event_name}}"
                     class="bigpaddingbottom">
            <md-option value="">
              Select Work Type
            </md-option>
            <md-option value="Debris Pickup">
              Debris Pickup
            </md-option>
            <md-option value="Debris Monitoring">
              Debris Monitoring
            </md-option>
            <md-option value="Call Center">
              Call Center
            </md-option>
            <md-option value="Working in EOC">
              Working in EOC
            </md-option>
            <md-option value="Building Repair">
              Building Repair
            </md-option>
            <md-option value="Road Repair">
              Road Repair
            </md-option>
            <md-option value="Other Repair">
              Other Repair
            </md-option>
            <md-option value="Prep for Storm">
              Prep for Storm
            </md-option>
            <md-option value="Not Listed">
              Not Listed
            </md-option>
          </md-select>
        </md-input-container>
      </div>
    </div>
    <div ng-show="event.disaster_work_hours.DisasterTimesError.length > 0"
         layout-padding
         layout="row"
         layout-wrap
         flex="100">
      <p style="margin-top: .5em;"
         flex="100"
         layout="row"
         class="warn">
        {{ event.disaster_work_hours.DisasterTimesError }}
      </p>     
      
    </div>
  </div>

</script>
<script type="text/ng-template" id="PayrollData.directive.tmpl.html">

  <div ng-if="showheader"
       layout="row"
       layout-align="start center"
       flex="100">
    <span style="text-align: left;"
          flex="25">
      Pay Code
    </span>
    <span style="text-align: right;"
          flex="10">
      Hours
    </span>
    <span style="text-align: right;"
          flex="15">
      Pay Rate
    </span>
    <span style="text-align: right;"
          flex="15">
      Amount
    </span>
    <span style="text-align: center;"
          flex="15">
      Classify
    </span>
    <span style="text-align: center;"
          flex="20">
      Project Code
    </span>
  </div>
  <hr ng-if="showheader"
      flex="100" />
  <div ng-if="!showheader && !messageonly && !totalonly"
       layout="row"
       layout-align="start center"
       flex="100">

    <span style="text-align: left;"
          flex="25">
      {{pd.paycode_detail.title}} ({{pd.paycode}})
    </span>
    <span style="text-align: right;"
          flex="10">
      {{pd.hours.toFixed(2)}}
    </span>
    <span style="text-align: right;"
          flex="15">
      {{pd.payrate.toFixed(5)}}
    </span>
    <span style="text-align: right;"
          flex="15">
      {{pd.amount.toFixed(2)}}
    </span>
    <span style="text-align: center;"
          flex="15">
      {{pd.classify}}
    </span>
    <span style="text-align: center;"
          flex="20">
      {{pd.project_code}}
    </span>
  </div>
  <div style="border-top: 1px dotted #040404; margin-top: .25em;"
       ng-if="totalonly"
       layout="row"
       layout-align="start center"
       flex="100">

    <span style="text-align: left;"
          flex="25">
      TOTAL
    </span>
    <span style="text-align: right;"
          flex="10">
      {{totalhours}}
    </span>
    <span style="text-align: right;"
          flex="15">
      
    </span>
    <span style="text-align: right;"
          flex="15">
      {{totalamount}}
    </span>
    <span style="text-align: center;"
          flex="35">
      
    </span>
  </div>
  <div ng-if="!showheader && !messageonly"
       ng-repeat="m in pd.messages track by $index"
       flex="100"
       layout="row"
       layout-align="start center">
    <div style="font-size: smaller; text-align: left; background-color: #ffffE0; padding-left: 1em;"
         flex-offset="25"
         flex="75">
      {{ m }}
    </div>
  </div>
  <div ng-if="messageonly"
       flex="100"
       layout="row"
       layout-align="start center">
    <div style="font-size: smaller; text-align: left; background-color: #ffffE0; padding-left: 1em;"
         flex-offset="25"
         flex="75">
      {{ message }}
    </div>
  </div>

</script>
<script type="text/ng-template" id="EditPayrollData.directive.tmpl.html">

  <div ng-if="showheader"
       layout="row"
       layout-align="space-between center"
       flex="100">
    <span flex="5">

    </span>
    <span style="text-align: left;"
          flex="30">
      Pay Code
    </span>
    <span style="text-align: right;"
          flex="10">
      Hours
    </span>
    <span style="text-align: right;"
          flex="10">
      Pay Rate
    </span>
    <span style="text-align: right;"
          flex="10">
      Amount
    </span>
    <span style="text-align: center;"
          flex="10">
      Classify
    </span>
    <span style="text-align: center;"
          flex="20">
      Project Code
    </span>
  </div>

  <hr ng-if="showheader"
      flex="100" />

  <div ng-if="!showheader && !messageonly && !totalonly"
       layout="row"
       layout-align="start start"
       flex="100">

    <div layout="row"
         layout-align="start center"
         layout-wrap
         flex="10">
      <md-button ng-click="DeleteData()"
                 class="md-button md-primary md-raised">
        DELETE
      </md-button>
    </div>

    <md-input-container flex="25">
      <label>Pay Code</label>
      <md-select class=""
                 ng-change="UpdatePaycodeDetail()"
                 ng-model="pd.paycode">
        <md-option ng-repeat="pc in paycodes track by pc.pay_code"
                   ng-value="pc.pay_code">
          {{pc.title}} ({{pc.pay_code}})
        </md-option>
      </md-select>
    </md-input-container>

    <md-input-container flex="10">
      <label>Hours</label>
      <input ng-disabled="disablehours"
             style="text-align: right;"
             ng-model="pd.hours"
             ng-change="RecalculateAmount()"
             type="number"
             step=".25" />
    </md-input-container>

    <md-input-container flex="10">
      <label>Payrate</label>
      <input ng-disabled="disablepayrate"
             style="text-align: right;"
             ng-model="pd.payrate"
             ng-change="RecalculateAmount()"
             type="number"
             step="any" />
    </md-input-container>

    <md-input-container flex="10">
      <label>Amount</label>
      <input ng-disabled="disableamount"
             style="text-align: right;"
             ng-model="pd.amount"
             type="number"
             step="any" />
    </md-input-container>

    <md-input-container flex="10">
      <label>Classify</label>
      <input style="text-align: right;"
             ng-model="pd.classify"
             maxlength="4"
             type="text" />
    </md-input-container>

    <md-input-container flex="15">
      <label>Project Code</label>
      <md-select ng-change="validate()"
                 ng-model="pd.project_code">
        <md-option ng-value="''">None</md-option>
        <md-option ng-repeat="pc in projectcodes track by pc.project_code"
                   ng-value="pc.project_code">{{pc.project_code}}</md-option>
      </md-select>

    </md-input-container>

    <!--<md-input-container flex="10">
    <label>Project Code</label>
    <input style="text-align: right;"
           ng-model="pd.project_code"
           maxlength="8"
           type="text" />
  </md-input-container>-->

  </div>
  <div style="border-top: 1px dotted #040404; margin-top: .25em;"
       ng-if="totalonly"
       layout="row"
       layout-align="start start"
       layout-wrap
       flex="100">
    <span flex="10">

    </span>
    <span style="text-align: right;"
          flex="25">
      TOTAL
    </span>
    <span style="text-align: right;"
          flex="10">
      {{totalhours}}
    </span>
    <span style="text-align: right;"
          flex="10">
      
    </span>
    <span style="text-align: right;"
          flex="10">
      {{totalamount}}
    </span>
    <span style="text-align: right;"
          flex="15">
    </span>
  </div>
  <div ng-if="!showheader && !messageonly"
       ng-repeat="m in pd.messages track by $index"
       flex="100"
       layout="row"
       layout-align="start center">
    <div style="font-size: smaller; text-align: left; background-color: #ffffE0; padding-left: 1em;"
         flex-offset="25"
         flex="75">
      {{ m }}
    </div>
  </div>
  <div ng-if="messageonly"
       flex="100"
       layout="row"
       layout-align="start center">
    <div style="font-size: smaller; text-align: left; background-color: #ffffE0; padding-left: 1em;"
         flex-offset="25"
         flex="75">
      {{ message }}
    </div>
  </div>

</script>
<script type="text/ng-template" id="PayrollEditGroup.directive.tmpl.html">

  <div id="editgroup{{ped.employee.EmployeeId}}"
       layout="row"
       layout-wrap
       style="background-color: #efefef;"
       flex="100">
    <div style="padding-left: 1em; padding-right: 1em;"
         class="my-accent"
         layout="row"
         layout-align="start center"
         layout-wrap
         flex="100">

      <span flex="30">{{ped.employee.DepartmentName}} ({{ped.employee.Department}}) </span>
      <a flex="20"
         style="padding-left: 1em; color: white;"
         target="_blank"
         rel="nofollow noopener"
         href="#/e/{{ped.employee.EmployeeId}}/ppd/{{payPeriod}}">
        {{ped.employee.EmployeeName}} ({{ped.employee.EmployeeId}})<!--</span><span flex="20" style="padding-left: 1em;">-->
      </a>
      <span flex="10">{{ ped.employee.isFulltime ? 'Full time' : 'Part time';}}</span>
      <span flex="10">{{ ped.employee.IsExempt ? 'Exempt' : 'Non Exempt';}}</span>

      <md-button ng-show="allowEdit"
                 ng-click="ShowEdit($event)"
                 class="md-primary md-raised hide-print">
        Edit
      </md-button>
    </div>
    <div style="padding: 1em 1em 1em 1em;"
         flex="100">
      <payroll-data showheader="true"></payroll-data>
      <payroll-data ng-repeat="pcd in ped.payroll_change_data track by $index"
                    pd="pcd"
                    flex="100">
      </payroll-data>
      <payroll-data ng-repeat="m in ped.messages track by $index"
                    messageonly="true"
                    message="m"
                    flex="100">
      </payroll-data>
      <payroll-data flex="100"
                    totalonly="true"
                    totalhours="GetTotalHours(ped.payroll_change_data)"
                    totalamount="GetTotalAmount(ped.payroll_change_data)">

      </payroll-data>
    </div>

  </div>
  
</script>
<script type="text/ng-template" id="PayrollEditDialog.tmpl.html">
  <md-dialog aria-label="Edit Time">
    <md-toolbar>
      <div class="md-toolbar-tools">
        <h2 flex="30">{{edit_data.employee.DepartmentName}} ({{edit_data.employee.Department}}) </h2>
        <h2 flex="20" style="padding-left: 1em;">{{edit_data.employee.EmployeeName}} ({{edit_data.employee.EmployeeId}})</h2>
        <h2 flex="10">{{ edit_data.employee.isFulltime ? 'Full time' : 'Part time';}}</h2>
        <h2 flex="10">{{ edit_data.employee.IsExempt ? 'Exempt' : 'Non Exempt';}}</h2>
        <span flex></span>
        <md-button class="md-icon-button" ng-click="cancel()">
          <md-icon md-svg-src="images/ic_close_24px.svg" aria-label="Close dialog"></md-icon>
        </md-button>
      </div>

    </md-toolbar>
    <md-dialog-content>
      <md-tabs md-dynamic-height
               md-border-bottom>
        <md-tab label="Payroll Changes">
          <div layout="row"
               layout-wrap
               style="padding: .5em .5em .5em .5em;"
               flex="100">
            <!-- Show tabs for Current Payroll Data and Base Data  -->
            <div layout="row"
                 layout-wrap
                 flex="100">

              <edit-payroll-data ng-repeat="pcd in edit_data.payroll_change_data track by $index"
                                 pd="pcd"
                                 employee="edit_data.employee"
                                 payrates="edit_data.finplus_payrates"
                                 paycodes="paycodes"
                                 projectcodes="project_codes"
                                 remove="RemoveDeleted()"
                                 validate="ValidateChanges()"
                                 flex="100">
              </edit-payroll-data>
              <edit-payroll-data ng-repeat="m in edit_data.messages track by $index"
                                 messageonly="true"
                                 message="m"
                                 flex="100">
              </edit-payroll-data>
              <div layout="row"
                   layout-align="start center"
                   flex="100">
                <md-button ng-click="AddPayrollChange()"
                           class="md-warn md-raised">
                  Add
                </md-button>
                <md-button ng-click="RevertAllChanges()"
                           class="md-raised">
                  Revert All Changes
                </md-button>
              </div>
              <edit-payroll-data flex="100"
                                 totalonly="true"
                                 totalhours="GetTotalHours(edit_data.payroll_change_data)"
                                 totalamount="GetTotalAmount(edit_data.payroll_change_data)">

              </edit-payroll-data>
            </div>

            <div ng-if="validation_errors.length > 0"
                 class="ErrorText"
                 style="margin-top: 1em; margin-bottom: 1em;"
                 layout="row"
                 layout-align="start center"
                 flex="100">
              Error: {{ validation_errors }}
            </div>

            <div style="margin-top: .5em;"
                 layout-align="center start"
                 layout="row"
                 layout-wrap
                 flex="100">

              <div layout="row"
                   flex="100">
                <div layout="row"
                     layout-align="start center"
                     flex="10">
                  <md-button ng-click="AddJustification()"
                             class="md-primary md-raised">
                    Add Justification
                  </md-button>
                </div>
                <div layout="row"
                     layout-align="start center"
                     flex="20">
                  <md-button ng-click="SaveJustifications();"
                             ng-show="edit_data.justifications.length > 0"
                             class="md-primary md-raised">
                    Save Justifications
                  </md-button>
                </div>
                <div style="padding-left: 1em; padding-right: 1em;"
                     layout="row"
                     layout-align="start center"
                     flex="70">
                  {{edit_data.justifications.length === 0 ? "No Justifications have been added." : "Justifications"}}
                </div>
              </div>
              <div ng-repeat="j in edit_data.justifications track by $index"
                   layout="row"
                   flex="100">
                <div layout="row"
                     layout-align="start center"
                     flex="10">
                  <md-button ng-click="DeleteJustification(j.id);"
                             class="md-warn md-raised">
                    Delete
                  </md-button>

                </div>
                <div layout="row"
                     layout-align="start center"
                     flex="90">
                  <textarea rows="6"
                            flex="100"
                            ng-model="j.justification"></textarea>
                </div>
              </div>
            </div>



          </div>
        </md-tab>
        <md-tab label="Base Timestore Data">
          <div style="padding: 1em 1em 1em 1em;"
               flex="100">
            <payroll-data showheader="true"></payroll-data>
            <payroll-data ng-repeat="pcd in edit_data.base_payroll_data track by $index"
                          pd="pcd"
                          flex="100">
            </payroll-data>
            <payroll-data flex="100"
                          totalonly="true"
                          totalhours="GetTotalHours(edit_data.base_payroll_data)"
                          totalamount="GetTotalAmount(edit_data.base_payroll_data)">

            </payroll-data>
          </div>
        </md-tab>
        <md-tab label="Default / Past Pay">
          <div layout="row"
               layout-align="start center"
               flex="100"
               layout-wrap
               style="padding: 1em 1em 1em 1em;">


            <md-input-container flex="60">
              <label>Show Default Info</label>
              <md-select ng-model="defaultview"
                         ng-change="UpdateView()">
                <md-option value="default">
                  Default Pay
                </md-option>
                <md-option ng-repeat="ps in edit_data.paystub_data"
                           ng-value="ps.check_number">
                  Check # {{ps.check_number}}
                </md-option>
              </md-select>
            </md-input-container>
            <div layout="row"
                 layout-align="center center"
                 flex="100">
              <span style="text-align: right;"
                    flex="20">
                Pay Code
              </span>
              <span style="text-align: right;"
                    flex="20">
                Hours
              </span>
              <span style="text-align: right;"
                    flex="20">
                Payrate
              </span>
              <span style="text-align: right;"
                    flex="20">
                Amount
              </span>
              <span style="text-align: right;"
                    flex="20">
                Classify
              </span>
            </div>
            <div ng-repeat="d in default_display track by $index"
                 layout="row"
                 layout-align="center center"
                 layout-wrap
                 flex="100">
              <span style="text-align: right;"
                    flex="20">
                {{d.paycode}}
              </span>
              <span style="text-align: right;"
                    flex="20">
                {{d.hours}}
              </span>
              <span style="text-align: right;"
                    flex="20">
                {{d.payrate}}
              </span>
              <span style="text-align: right;"
                    flex="20">
                {{d.amount}}
              </span>
              <span style="text-align: right;"
                    flex="20">
                {{d.classify}}
              </span>
            </div>

          </div>
          
        </md-tab>
      </md-tabs>
    </md-dialog-content>
    <md-dialog-actions layout="row">
      <span flex></span>
      <md-button ng-click="cancel()"
                 class="md-raised">
        Cancel
      </md-button>
      <md-button ng-disabled="validation_errors.length > 0"
                 ng-click="hide()"
                 class="md-primary md-raised">
        Save
      </md-button>
    </md-dialog-actions>
  </md-dialog>
  </script>