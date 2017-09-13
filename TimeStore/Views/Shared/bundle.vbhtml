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
        <md-select aria-label="Select Access Level"
                   flex="90" id="AccessTypes"
                   ng-disabled="employee === null"
                   ng-model="timecardAccess.Raw_Access_Type">
          <md-select-label>Employee Access Level {{ accessLevels[timecardAccess.Raw_Access_Type] }}</md-select-label>
          <md-option ng-repeat="al in accessLevels" ng-value="{{$index}}">
            {{ al }}
          </md-option>
        </md-select>
        <md-select aria-label="Select Data Type"
                   flex="90" id="dataTypeList"
                   ng-disabled="employee === null"
                   ng-model="timecardAccess.Data_Type">
          <md-select-label>Employee Data Location {{timecardAccess.Data_Type}}</md-select-label>
          <md-option ng-value="dt" ng-repeat="dt in dataTypes">
            {{ dt }}
          </md-option>
        </md-select>

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
             layout-align="center center"
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
            <label>Lunch Start {{ TCTD.DepartmentNumber === "3701" ? "(30 Mins)" : "(1 Hour)"}}</label>
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
          <div ng-if="timecard.DisasterName_Display.length > 0 && TCTD.WorkHours.value > 0 && isCurrentPPD"
               layout="row"
               layout-wrap
               layout-padding
               flex="100">
            <md-input-container class="md-input-has-value"
                                flex="100">
              <label>Are any of the hours worked related to {{ timecard.DisasterName_Display }}?</label>
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
                </md-radio-button>
              </md-radio-group>
            </md-input-container>
            <p ng-if="disasterChoiceError.length > 0"
               flex="100"
               layout="row"               
               class="warn">
              {{ disasterChoiceError }}
            </p>
          </div>
        </div>

      </div>
    </div>

    <div ng-show="isCurrentPPD">
      <div ng-click="Toggle_DisasterHours()"
           style="margin-top: .25em; margin-bottom: .25em; cursor: pointer;"
           flex="100"
           class="short-toolbar my-primary"
           layout="row"
           layout-align="start center">
        <span flex="5"></span>
        <h5>
          Show / Hide -- Disaster hours worked related to {{ timecard.DisasterName_Display }}
        </h5>
      </div>

      <div ng-show="showDisaster"
           style="margin-top: .5em;"
           layout="row"
           layout-align="center center"
           layout-wrap
           flex="100">

        <md-input-container flex="40">
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
          <p>You must enter a comment indicating your duties relating to the disaster.</p>
        </div>
      </div>
    </div>

    <div flex="100"
         ng-if="timecard.isPubWorks">
      <div style="margin-top: .75em; margin-bottom: .75em;"
           class="short-toolbar my-primary"
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
        <md-checkbox ng-true-value="1"
                     ng-false-value="0"
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
          <hours-display tctd="TCTD" hours="TCTD.ScheduledLWOPHours" calc="calculateTotalHours()"></hours-display>
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
            <hours-display tctd="TCTD" hours="TCTD.AdminDisaster" calc="calculateTotalHours()"></hours-display>
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
        <input ng-model="hours.value"
               ng-change="calc()"
               type="{{ ::hours.type }}"
               min="{{ ::hours.min }}"
               max="{{ ::hours.max }}"
               step="{{ ::hours.step }}" 
               ng-disabled="hours.disabled"/>
    </md-input-container>
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
                <span ng-if="r.approval_id === 0"
                      ng-click="showDetail($index)"
                      flex="25">
                    <span class="undecidedLeave">
                        Undecided
                    </span>
                </span>
                <span ng-if="r.approval_id !== 0 && r.Approved"
                      ng-click="showDetail($index)"
                      flex="25">
                    <span class="approvedLeave">
                        Approved
                        <md-icon aria-label="approved icon"
                                 md-svg-src="images/ic_done_24px.svg">
                        </md-icon>
                    </span>
                </span>
                <span ng-if="r.approval_id !== 0 && !r.Approved"
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
                   ng-if="r.approval_id !== 0"
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
                   ng-if="r.approval_id !== 0"
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
          <span ng-if="d.approval_id === 0"
                ng-click="showDetail($index)"
                flex="20">
            <span class="undecidedLeave">
              Undecided
            </span>
          </span>
          <span ng-if="d.approval_id !== 0 && d.Approved"
                ng-click="showDetail($index)"
                flex="20">
            <span class="approvedLeave">
              Approved
              <md-icon aria-label="approved icon"
                       md-svg-src="images/ic_done_24px.svg">
              </md-icon>
            </span>
          </span>
          <span ng-if="d.approval_id !== 0 && !d.Approved"
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
               ng-if="d.approval_id !== 0"
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
               ng-if="d.approval_id !== 0"
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

            <div ng-show="d.approval_id === 0"
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
            <div ng-if="d.approval_id === 0 && approving"
                 layout="row"
                 layout-align="center center"
                 flex="100">
              <md-progress-circular md-diameter="25%"
                                    md-mode="indeterminate">
              </md-progress-circular>
              <span>Processing your request, please wait...</span>
            </div>
            <div ng-show="d.approval_id === 0"
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
              <div ng-show="d.approval_id !== 0"
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
                <span ng-if="x.approval_id === 0"
                      flex="20">
                  <span class="undecidedLeave">
                    Undecided
                  </span>
                </span>
                <span ng-if="x.approval_id !== 0 && x.Approved"
                      flex="20">
                  <span class="approvedLeave">
                    Approved
                    <md-icon aria-label="approved icon"
                             md-svg-src="images/ic_done_24px.svg">
                    </md-icon>
                  </span>
                </span>
                <span ng-if="x.approval_id !== 0 && !x.Approved"
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
    <md-list flex="100">
        <md-item>
            <md-item-content>
                <div flex="100"
                     layout="row"
                     layout-align="center center"
                     layout-wrap>
                    <span flex="20" layout="row" layout-align="end center">
                        Type of Hours
                    </span>
                    <span flex="10" layout="row" layout-align="center center">
                        Hours
                    </span>
                    <span flex>
                        Check your hours
                    </span>
                    <md-divider flex="100"></md-divider>
                </div>
            </md-item-content>
        </md-item>
    </md-list>
    <md-list ng-if="getGroups().length > 1"
             ng-repeat="group in getGroups()"
             flex="100">

        <md-item flex="100">
            <md-item-content flex="100">
                <div flex="100"
                     class="short-toolbar my-accent"
                     layout="row"
                     layout-align="center center">
                    <h5>Your hours at base Payrate: {{group}}</h5>
                </div>
            </md-item-content>
        </md-item>
        <md-item ng-repeat="t in tl | groupPayrateBy:group"
                 flex="100">
            <md-item-content>
                <div flex="100"
                     layout="row"
                     layout-align="center center"
                     layout-wrap>
                    <span flex="20" layout="row" layout-align="end center">
                        {{t.name}}
                    </span>
                    <span flex="10" layout="row" layout-align="center center">
                        {{t.hours}}
                    </span>
                    <md-checkbox style="margin-left: 0; margin-right: 0;"
                                 flex
                                 class="green"
                                 ng-change="checkApproved()"
                                 ng-model="t.approved" 
                                 aria-label="These hours are correct">
                        These hours are correct
                    </md-checkbox>
                    <!--<md-radio-group flex
                                    layout="row"
                                    ng-model="t.approved"
                                    layout-align="start center"
                                    layout-wrap
                                    ng-change="checkApproved()">
                        <md-radio-button ng-value="true">
                            These hours are correct.
                        </md-radio-button>
                        <md-radio-button ng-value="false">
                            These hours are wrong.
                        </md-radio-button>
                    </md-radio-group>-->
                    <md-divider flex="100"></md-divider>
                </div>
            </md-item-content>

        </md-item>
    </md-list>

    <md-list flex="100"
             ng-if="getGroups().length == 1">
        <md-item ng-repeat="t in tl">
            <md-item-content>
                <div flex="100"
                     layout="row"
                     layout-align="center center"
                     layout-wrap>
                    <span flex="20" layout="row" layout-align="end center">
                        {{t.name}}
                    </span>
                    <span flex="10" layout="row" layout-align="center center">
                        {{t.hours}}
                    </span>
                    <md-checkbox style="margin-left: 0; margin-right: 0;"
                                 flex
                                 class="green"
                                 ng-change="checkApproved()"
                                 ng-model="t.approved" 
                                 aria-label="These hours are correct">
                        These hours are correct
                    </md-checkbox>
                    <!--<md-radio-group flex
                                    layout="row"
                                    layout-align="start center"
                                    ng-model="t.approved"
                                    layout-wrap
                                    ng-change="checkApproved()">
                        <md-radio-button ng-value="true">
                            These hours are correct.
                        </md-radio-button>
                        <md-radio-button ng-value="false">
                            These hours are wrong.
                        </md-radio-button>
                    </md-radio-group>-->
                    <md-divider flex="100"></md-divider>
                </div>
            </md-item-content>

        </md-item>

    </md-list>
    <md-list ng-if="showHolidayError === true"
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
    <md-list flex
             ng-if="showApprovalButton === true">
        <md-item flex>
            <md-item-content flex
                             layout="row"
                             layout-align="center center"
                             layout-padding>
                <md-button ng-click="approve()"
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
                       week="timecard.RawTime_Week1"
                       typelist="timecard.Get_Types_Used"
                       employeeid="timecard.employeeID"
                       showaddtime="timecard.showAddTime"
                       datatype="timecard.Data_Type">
        </timecard-week>

      </div>
    </md-tab>

    <md-tab label="Week 2" id="week2" layout-padding>
      <div flex="100"
           layout-wrap>
        <timecard-week flex="95"
                       title="Week 2"
                       week="timecard.RawTime_Week2"
                       typelist="timecard.Get_Types_Used"
                       employeeid="timecard.employeeID"
                       showaddtime="timecard.showAddTime"
                       datatype="timecard.Data_Type"></timecard-week>

      </div>
    </md-tab>

    <md-tab label="{{ (timecard.exemptStatus !== 'Exempt' && timecard.Data_Type === 'timecard'? 'Comp / ': '') }} Time Summary"
            layout-padding
            id="calctimelist">
      <div flex="100">

        <div style="margin-top: .75em;"
             class="short-toolbar my-accent"
             flex="100"
             layout="row"
             layout-align="center center">
          <h5>Pay Period Summary</h5>
        </div>

        <time-list flex="100" tl="timecard.calculatedTimeList"></time-list>

        <add-comp-time flex="100"
                       ng-show="timecard.exemptStatus !== 'Exempt' && timecard.Data_Type === 'timecard'"
                       timecard="timecard"></add-comp-time>

      </div>

    </md-tab>

    <md-tab label="Approve"
            id="approveyourhours">

      <div ng-show="timecard.ErrorList.length === 0 && timecard.Approval_Level === 0 && timecard.Days_Since_PPE <= 1 && timecard.timeList.length > 0"
           flex="100"
           layout-padding>

        <time-approval flex="95" tl="timecard.timeList" tc="timecard"></time-approval>

      </div>
      <div ng-show="timecard.ErrorList.length > 0"
           layout="row"
           layout-align="center center"
           layout-padding
           layout-wrap
           flex="100">
        Your time cannot be approved while there are Errors present.
        <timecard-warnings flex="100"
                           alignheader="center"
                           headerclass="warn"
                           dl="timecard.ErrorList"
                           title="Errors"></timecard-warnings>
      </div>
      <div ng-show="timecard.Approval_Level > 0 && timecard.Days_Since_PPE < 1"
           layout="row"
           layout-align="center center"
           layout-padding
           flex="100">
        Your time has already been approved.
      </div>
      <div ng-show="timecard.Days_Since_PPE > 1"
           layout="row"
           layout-align="center center"
           layout-padding
           flex="100">
        It is too late to approve your time for this pay period.
      </div>
      <div ng-show="timecard.timeList.length === 0 && timecard.Days_Since_PPE < 1"
           layout="row"
           layout-align="center center"
           layout-padding
           flex="100">
        No time has been entered to approve.  You can enter time by going to the Week 1 or Week 2 tabs and clicking the Time Entry button.
      </div>
      <div ng-show="timecard.WarningList.length > 0"
           layout="row"
           layout-align="center center"
           layout-padding
           layout-wrap
           flex="100">
        These warnings should be considered before you approve your time.
        <timecard-warnings flex="100"
                           alignheader="center"
                           headerclass="my-accent"
                           dl="timecard.WarningList"
                           title="Warnings"></timecard-warnings>
      </div>
    </md-tab>

    <md-tab label="Notes"
            id="notelist">

      <div flex="100">

        <timecard-notes flex="100"
                        datatype="timecard.Data_Type"
                        employeeid="timecard.employeeID"
                        payperiodending="timecard.payPeriodEndingDisplay"
                        notes="timecard.Notes">
        </timecard-notes>

      </div>

    </md-tab>

    <md-tab ng-if="timecard.isPubWorks"
            label="Incentives">
      <div flex="100">

      </div>
    </md-tab>

    <md-tab label="Holiday" 
            ng-if="(timecard.HolidaysInPPD.length > 0 || timecard.bankedHoliday > 0) && timecard.Data_Type === 'telestaff'">
      <div style="margin-top: .5em;"
           flex="100">
        <div ng-if="timecard.HolidaysInPPD.length > 0"
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
                          ng-model="timecard.HolidayHoursChoice[$index]"
                          ng-repeat="holiday in timecard.HolidaysInPPD">
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
             ng-if="timecard.bankedHoliday >= timecard.holidayIncrement">
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
                   step="{{timecard.holidayIncrement}}"
                   name="amount"
                   min="0"
                   max="{{timecard.bankedHoliday - timecard.HolidayHoursUsed}}"
                   ng-model="timecard.BankedHoursPaid" />
          </md-input-container>
          <span flex="50">You have {{ timecard.bankedHoliday }} hours banked, and have currently marked {{ timecard.HolidayHoursUsed + timecard.BankedHoursPaid }} hours for use so far this pay period.  You can elect to be paid for them in groups of {{ timecard.holidayIncrement }} hours.</span>
        </div>
        <div layout="row"
             layout-align="center center"
             flex="100"
             ng-if="timecard.bankedHoliday < timecard.holidayIncrement">
          You must have {{ timecard.holidayIncrement }} hours of banked holiday time to request a pay out.
        </div>
        <div layout="row"
             layout-align="center center"
             flex="100">
          <md-button ng-click="SaveHolidays()"
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
                    <span>
                        Payrate {{ ::timecard.shortPayrate}} {{ ::timecard.exemptStatus }}
                        <md-tooltip md-direction="right">
                            {{ ::timecard.Payrate }}
                        </md-tooltip>
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
            <timecard-detail flex="100" timecard="timecard"></timecard-detail>

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
        <md-item flex="100"
                 layout="row"
                 layout-align="center center"
                 ng-repeat="wl in dl"
                 layout-wrap
                 layout-padding>
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

    <md-list-item>
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