
@Code
    
End Code

<div layout-wrap
     layout="row"
     layout-align="center center"
     flex="100" 
     ng-controller="ReportController">


        <md-progress-linear ng-if="showProgress === true"
                            flex="100"
                            md-mode="indeterminate">
        </md-progress-linear>


        <div flex="90">

            <div style="margin-top: .5em;"                 
                 layout="row"
                 layout-wrap
                 layout-align="start center"
                 flex="100">
                <div flex="40"
                     class="short-toolbar my-accent"
                     layout="row"
                     layout-align="center center">
                    <h5>
                        Search Fields
                    </h5>
                </div>
                <span flex="5"></span>
                <md-select aria-label="Select the fields to search for"
                           flex="25"
                           md-on-close="updateFieldSelections()"
                           ng-model="selectedFields"
                           multiple>
                    <md-option ng-value="f"
                               ng-repeat="f in fieldList | orderBy: f  track by f">
                        {{ f }}
                    </md-option>
                </md-select>
                @*<div flex="100"
                     layout="row"
                     layout-align="start start"
                     layout-wrap
                     ng-if="showFields === true">
                    <md-checkbox ng-repeat="f in fieldList | orderBy: f"
                                 ng-disabled="fieldsToDisplay.length > 3 && fieldsToDisplay.indexOf(f) === -1"
                                 ng-click="addDisplay(f)"
                                 flex="15">
                        {{ f }}
                    </md-checkbox>

                </div>*@
            </div>
            <div layout="row"
                 layout-align="start center"
                 layout-wrap
                 flex="100">
                <md-chips>
                    <md-chip ng-repeat="s in selectedFields">
                        {{ s}}
                    </md-chip>
                </md-chips>
            </div>

            <div class="md-whiteframe-z1"
                 style="margin-top: .5em; margin-bottom: .5em;"
                 layout="row"
                 layout-align="center center"
                 layout-wrap
                 flex="100">
                <div flex="100"
                     class="short-toolbar my-accent"
                     layout="row"
                     layout-align="center center">
                    <h5>
                        Search Criteria
                    </h5>
                </div>
                <div flex="100"
                     layout="row"
                     layout-align="center center">
                    Choose Your Date Range:
                    <md-datepicker md-min-date="minDate" 
                                   md-max-date="maxDate"
                                   ng-model="dateFrom">
                    </md-datepicker>
                    <md-datepicker md-min-date="minDate"
                                   md-max-date="maxDate"
                                   ng-model="dateTo">
                    </md-datepicker>
                    <md-button class="md-raised"
                               ng-click="Search()">
                        Search
                    </md-button>
                    <span flex="5"></span>
                    <md-button download="{{csvFilename}}"
                               ng-href="{{csvUrl}}"
                               ng-show="timeData.length > 0"
                               class="md-primary md-fab md-raised">
                        <md-icon md-svg-src="/TimeStore/images/ic_file_download_24px.svg">
                        </md-icon>
                    </md-button>

                </div>
            </div>
        </div>
    <div ng-if="selectedFields.length > 0 && message.length === 0"
         layout="row"
         layout-align="center start"
         layout-wrap
         flex="90">
        <div layout="row"
             layout-align="center start"
             flex="100">
            <span flex="20">
                Name
            </span>
            <span flex="5">
                Dept
            </span>
            <span flex="15"
                  ng-repeat="f in selectedFields">
                {{ f }}
            </span>
            <span flex="15">
                Total
            </span>
        </div>
        <div ng-repeat="d in dataToView | orderBy: ['dept', 'displayName'] track by d.eid"
             layout="row"
             layout-align="center start"
             flex="100">
            <span flex="20">
                {{ d.displayName }}
            </span>
            <span flex="5">
                {{ d.dept }}
            </span>
            <span flex="15"
                  ng-repeat="v in d.values track by $index">
                {{ v }}
            </span>
        </div>
    </div>
    <div layout="row"
         layout-align="center center"
         ng-if="message.length > 0">
        {{ message }}
    </div>
</div>
