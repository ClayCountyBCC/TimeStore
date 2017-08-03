/* global moment, _ */
(function () {
    "use strict";
    angular.module('timestoreApp')
        .directive('timecardWeek', function () {
            return {
                restrict: 'E',
                templateUrl: 'TimeCardWeek.tmpl.html', //'app/timecard/TimeCardWeek.tmpl.html',
                scope: {
                    employeeid: '=',
                    showaddtime: '=',
                    datatype: '=',
                    week: '=',
                    title: '@',
                    typelist: '='
                },
                controller: ['$scope', 'timestoreNav', function ($scope, timestoreNav) {

                    $scope.getHours = function (listval, workHoursList) {
                        var index = _.findIndex(workHoursList, { 'name': listval });
                        if (index === -1) {
                            return 0;
                        } else {
                            return workHoursList[index].hours;
                        }
                    };

                    $scope.addTimeGo = function (d) {
                        timestoreNav.goAddTime($scope.employeeid, d);
                        //ng-href="#/e/{{employeeid}}/addtime/{{formatURLDate(r.workDateDisplay)}}"
                        //$location.path('/e/' + $scope.employeeid + '/addtime/' + $scope.formatURLDate(d));
                        //$location.path()
                    };

                    //$scope.formatURLDate = function (d) {
                    //    if (!d) {
                    //        return '';
                    //    } else {
                    //        return moment(d, "M/D/YYYY").format("YYYYMMDD")
                    //    }
                    //};

                    $scope.getTotalHours = function (listval) {
                        var total = 0;
                        if ($scope.week !== undefined) {
                            var week = $scope.week;
                            for (var i = 0; i < week.length; i++) {
                                if (listval === null) {
                                    total += week[i].totalWorkHours;
                                } else {
                                    total += $scope.getHours(listval, week[i].workHoursList);
                                }
                            }
                        }
                        return total;
                    };

                    //$scope.showAddTime = function (ev, employeeId, workDate) {
                    //    showAddTimeDialog(ev, employeeId, workDate, $mdDialog);
                    //};
                }]
            };
        });
}());