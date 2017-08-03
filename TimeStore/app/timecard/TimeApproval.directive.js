/* global moment, _ */
(function () {
    "use strict";
    angular.module('timestoreApp')
        .directive('timeApproval', ['viewOptions', function (viewOptions) {
            return {
                restrict: 'E',
                templateUrl: 'TimeApproval.tmpl.html', //'app/timecard/TimeApproval.tmpl.html',
                scope: {
                    tc: '=',
                    tl: '=',
                    showApproval: '='

                },
                controller: ['$scope', '$mdToast', 'timestoredata', 'commonFunctions',
                function ($scope, $mdToast, timestoredata, commonFunctions) {

                    $scope.showHolidayError = false;
                    $scope.showApprovalButton = false;

                    $scope.checkApproved = function () {
                        $scope.showApprovalButton = false;
                        if ($scope.tl === undefined) {
                            return false;
                        }
                        var ctl = $scope.tl;
                        if (ctl.length === 0) {
                            return false;
                        }
                        for (var i = 0; i < ctl.length; i++) {
                            if (ctl[i].approved === false) {
                                return false;
                            }
                        }
                        // Now let's check that the holidays are handled
                        if ($scope.tc.isHolidayTimeBankable === true && $scope.tc.HolidaysInPPD.length > 0) {
                            // If they don't have any hours in 134 or 122 then we need to stop.                        
                            $scope.showHolidayError = true;
                            for (var j = 0; j < ctl.length; j++) {
                                if (ctl[j].payCode === '122' || ctl[j].payCode === '134') {
                                    $scope.showHolidayError = false;
                                }
                            }
                            if ($scope.showHolidayError === true) {
                                return false;
                            }
                        }
                        $scope.showApprovalButton = true;
                        return true;
                    };

                    $scope.approve = function () {
                        var timecard = $scope.tc;
                        var ad = {
                            EmployeeID: timecard.employeeID,
                            PayPeriodStart: timecard.payPeriodStart,
                            WorkTypeList: timecard.calculatedTimeList,
                            Initial_Approval_By_EmployeeID: timecard.Initial_Approval_EmployeeID
                        };
                        timestoredata.approveInitial(ad)
                                            .then(onApproval, onError);
                    };

                    var onApproval = function (data) {
                        showToast(data);
                        viewOptions.approvalUpdated.approvalUpdated = true;
                        viewOptions.approvalUpdated.share();
                    };

                    var onError = function (data) {
                        alert(data + '  Your approval was not saved!');
                    };

                    $scope.getGroups = function () {
                        return commonFunctions.getGroupsByShortPayRate($scope.tl);

                        //var groupArray = [];

                        //angular.forEach($scope.tl, function (item, idx) {
                        //    if (groupArray.indexOf(parseFloat(item.shortPayRate)) === -1) {
                        //        groupArray.push(parseFloat(item.shortPayRate));
                        //    }
                        //});
                        //return groupArray;
                    };

                    $scope.toastPosition = {
                        bottom: true,
                        top: false,
                        left: false,
                        right: true
                    };

                    $scope.getTotalHours = function () {
                        return commonFunctions.getTotalHours($scope.tl);
                        //if ($scope.tl === undefined) {
                        //    return 0;
                        //}
                        //var tl = $scope.tl;
                        //var total = 0;
                        //for (var i = 0; i < tl.length; i++) {
                        //    total += tl[i].hours;
                        //}
                        //return total;
                    };

                    function showToast(Message) {
                        $mdToast.show(
                          $mdToast.simple()
                            .content(Message)
                            .position($scope.getToastPosition())
                            .hideDelay(3000)
                        );
                    }

                    $scope.getToastPosition = function () {
                        return Object.keys($scope.toastPosition)
                          .filter(function (pos) { return $scope.toastPosition[pos]; })
                          .join(' ');
                    };
                }]
            };
        }]);
}());