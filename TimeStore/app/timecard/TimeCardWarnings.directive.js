/* global moment, _ */
(function () {
    "use strict";
    angular.module('timestoreApp')
        .directive('timecardWarnings', function () {
            return {
                restrict: 'E',
                templateUrl: 'TimecardWarnings.tmpl.html', //'app/timecard/TimecardWarnings.tmpl.html',
                scope: {
                    dl: '=',
                    title: '@',
                    headerclass: '@',
                    alignheader: '@'
                }
            };
        });
}());