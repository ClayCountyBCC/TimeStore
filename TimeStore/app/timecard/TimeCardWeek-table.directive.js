/* global moment, _ */
(function () {
    "use strict";
    angular.module('timestoreApp')
        .directive('timecardWeekTable', function () {
            return {
                restrict: 'E',
                templateUrl: 'TimeCardWeek-table.tmpl.html', //'app/timecard/TimeCardWeek-table.tmpl.html',
                scope: {
                    week: '=',
                    title: '@',
                    typelist: '='
                },
                controller: ['$scope', function ($scope) {

                    $scope.getHours = function (listval, workHoursList) {
                        var index = _.findIndex(workHoursList, { 'name': listval });
                        if (index === -1) {
                            return 0;
                        } else {
                            return workHoursList[index].hours;
                        }
                    };

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
                }]
            };
        });
}());