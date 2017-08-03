/* global moment, _ */

(function () {
  "use strict";

    angular.module('timestoreApp')
      .controller('CalendarViewController', ['$scope', 'viewOptions', 'deptleavedata', 'holidays', 'birthdays', CalendarViewController]);
  function CalendarViewController($scope, viewOptions, deptleavedata, holidays, birthdays) {
    viewOptions.viewOptions.showSearch = false;
    viewOptions.viewOptions.share();
    $scope.leaveRequests = deptleavedata;
    $scope.holidays = holidays;
    $scope.birthdays = birthdays;

  }

})();