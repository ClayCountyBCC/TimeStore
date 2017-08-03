/* global _, moment */
(function () {
    "use strict";
    angular.module('timestoreApp')
        .controller('DailyCheckoffController', ['$scope', 'timestoredata', 'viewOptions', DailyCheckoff]);

    function DailyCheckoff($scope, timestoredata, viewOptions) {
        $scope.showProgress = false;
        viewOptions.viewOptions.showSearch = false;
        viewOptions.viewOptions.share();

        resetDisplayAndData();

        $scope.RefreshApprovalData = function () {
            $scope.showProgress = true;
            resetDisplayAndData();
            timestoredata.getInitiallyApproved($scope.ppdIndex).then(ProcessData, function () { });
        };

        $scope.RefreshApprovalData();

        function resetDisplayAndData() {
            $scope.dataView = [];
            $scope.noNote = [];
            $scope.hasNote = [];
            $scope.swapButtonText = 'View Approved Employees';
            $scope.showNoNote = true;
            $scope.ppe = timestoredata.getPayPeriodEnd();
            $scope.todayDisplay = moment().format('M/D/YYYY');
            //$scope.todayDisplay = moment().add(-3, 'd').format('M/D/YYYY'); // For testing
            if (moment().hour() < 8) {
                $scope.todayDisplay = moment().add(-1, 'd').format('M/D/YYYY');
            }
            $scope.title = 'List of all Non-approved Field personnel for ' + $scope.todayDisplay;
            $scope.startTime = moment($scope.todayDisplay + ' 8:00 AM', 'M/D/YYYY h:mm A');
            $scope.endTime = moment($scope.todayDisplay + ' 8:00 AM', 'M/D/YYYY h:mm A').add(24, 'h');
        }

        function ProcessData(data) {
            _.each(data, findNonApprovals);
            $scope.dataView = $scope.noNote;
            $scope.showProgress = false;
        }

        function findNonApprovals(tc) {
            
            if (tc.GroupName.substring(0, 2) === 'BC' || tc.GroupName === 'Dispatch') {
                var d = _.find(tc.RawTime, function (x) {
                    return x.workDateDisplay === $scope.todayDisplay;
                });
                if (d !== undefined) {
                    if (checkWorkType(d.workTime)) {
                        // Need to make sure the time is approved before we bother checking for a note.
                        if (tc.Approval_Level === 0 || !checkForNoteToday(tc.Notes, tc.lastName.toUpperCase())) {
                            addEmp(tc, d, $scope.noNote);
                        } else {
                            addEmp(tc, d, $scope.hasNote);
                        }
                    } else {
                        //console.log('worktype not found ' + tc.lastName.toUpperCase());
                    }
                } else {
                    //console.log('date not found ' + tc.lastName.toUpperCase());
                }
            } else {
                //console.log('group not found ' + tc.lastName.toUpperCase());
            }
        }

        function checkForNoteToday(n, lastName) {
            //console.log(lastName);
            if (lastName.trim().indexOf(' ') > -1) {
                // we've got someone with a space in their last name, and their username is only based on
                // the first part of their last name.  So we'll just look for part of the last name in the username.
                lastName = lastName.trim().split(' ')[0]; 
            }
            for (var i = 0; i < n.length; i++) {
                if (moment(n[i].Date_Added).isBetween($scope.startTime, $scope.endTime)) {
                    // Also need to make sure the note in question is not a system note.
                    // We also check to make sure the person's last name is in the Added_By field.
                    if (n[i].Added_By.toUpperCase().indexOf(lastName) > -1) {
                        return true;
                    }                    
                }
            }
            return false;

        }

        function addEmp(tc, d, pushTo) {
            var emp = {
                eid: 0,
                group: '',
                name: '',
                time: ''
            };
            emp.eid = tc.employeeID;
            emp.group = tc.GroupName;
            emp.name = tc.EmployeeDisplay;
            emp.time = d.workTime;
            pushTo.push(emp);
            //$scope.noNote;
        }

        function checkWorkType(wt) {
            return wt.toUpperCase().match(/(OT|SU|MW|STRAIGHT|SPD|2102K|CWPP|2102KOT|CWPPOT)/);
            //OT, SU, MW, STRAIGHT, SPD, 2102K,CWPP,2102KOT,CWPPOT
        }

        $scope.switchData = function () {
            if ($scope.showNoNote) {
                $scope.dataView = $scope.hasNote;
                $scope.swapButtonText = 'View Un-Approved Employees';
                $scope.title = 'List of all Approved Field personnel on shift for ' + $scope.todayDisplay;
            } else {
                $scope.dataView = $scope.noNote;
                $scope.swapButtonText = 'View Approved Employees';
                $scope.title = 'List of all Non-approved Field personnel on shift for ' + $scope.todayDisplay;
            }
            $scope.showNoNote = !$scope.showNoNote;
        };
    }

})();