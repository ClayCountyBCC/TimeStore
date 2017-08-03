/* global moment, _ */

(function () {
  "use strict";

  angular.module('timestoreApp')
      .directive('leaveCalendar', function () {
        return {
          restrict: 'E',
          scope: {
            leavedata: '=',
            holidaylist: '=',
            birthdaylist: '='
          },
          templateUrl: 'LeaveCalendar.directive.tmpl.html',
          controller: 'LeaveCalendarDirectiveController'
        }
      })
      .controller('LeaveCalendarDirectiveController', ['$scope', 'timestoreNav', LeaveCalendarDirectiveController]);

  function LeaveCalendarDirectiveController($scope, timestoreNav) {
    $scope.days = ['Sun', 'Mon', 'Tues', 'Wed', 'Thu', 'Fri', 'Sat'];
    $scope.minMonth = moment().month() + 1;
    $scope.minYear = moment().year();
    $scope.currentMonth = moment().month() + 1;
    $scope.currentYear = moment().year();
    $scope.allowPrevious = false;
    $scope.monthTitle = moment().format('MMMM YYYY');
    $scope.calendarDays = [];
    $scope.deptList = [];
    $scope.selectedDept = '';
    updateDeptList();
    updateCalendarDays();

    $scope.showCalendarDayDetail = function (i) {
      _.times(42, function (n) {
        $scope.calendarDays[n].span = 1;
        $scope.calendarDays[n].dayOfWeek = '';
      });
      var cd = $scope.calendarDays[i];
      if (cd.showDetail === false && (cd.detailedList.length > 0 || cd.birthdayList.length > 0)) {
        var i = cd.detailedList.length + cd.birthdayList.length;
        cd.span = Math.max(Math.ceil(i / 3), 1);
        cd.dayOfWeek = moment(cd.currentDate).format('dddd');
        cd.showDetail = true;
      } else {
        cd.span = 1;
        cd.showDetail = false;
        cd.dayOfWeek = '';
      }
    }

    $scope.goHome = function () {
      timestoreNav.goHome();
    }

    $scope.previousMonth = function () {
      $scope.currentMonth -= 1;
      if ($scope.currentMonth <= 0) {
        $scope.currentMonth = 12;
        $scope.currentYear -= 1;
      }
      updateCalendarDays();
    };

    $scope.nextMonth = function () {
      $scope.currentMonth += 1;
      if ($scope.currentMonth > 12) {
        $scope.currentMonth = 1;
        $scope.currentYear += 1;
      }
      updateCalendarDays();
    };

    function updateDeptList() {
      $scope.deptList = _.pluck(_.uniq($scope.leavedata, 'dept'), 'dept');
      if ($scope.leavedata.length > 0) {
        $scope.selectedDept = $scope.leavedata[0].dept;
        if ($scope.leavedata[0].employee_name === 'my_dept') {
          $scope.leavedata.splice(0, 1); // remove the element at index 0.
        }
      }
    }

    $scope.deptChanged = function () {
      updateCalendarDays();
    }

    function updateCalendarDays() {
      // we're going to set a hard number of days here, 42.  That will be 6 weeks.
      $scope.calendarDays = [];
      var m = moment().year($scope.currentYear).month($scope.currentMonth - 1);
      var firstdayofmonth = m.clone().startOf('day').date(1).day();
      var startDate = m.clone().startOf('day').date(1).add(-firstdayofmonth, 'days');
      $scope.monthTitle = m.clone().format('MMMM YYYY');

      _.times(42, function () {
        var thisDay = {
          day: startDate.date(),
          currentDate: startDate.clone().toDate(),
          isCurrent: (startDate.month() === $scope.currentMonth - 1),
          showDetail: false,
          shortList: getShortList(startDate.clone(), $scope.selectedDept),
          detailedList: getDetailedList(startDate.clone(), $scope.selectedDept),
          birthdayList: getBirthdayList(startDate.clone(), $scope.selectedDept),
          isHoliday: checkHoliday(startDate.clone()),
          dayOfWeek: '',
          span: 1
        }
        $scope.calendarDays.push(thisDay);
        startDate.add(1, 'days');
      });
    }

    function getShortList(d, selDept) {
      var filtered = _.pluck(_.filter($scope.leavedata, function (ld) {
        return moment(ld.work_date).isSame(d) && ld.dept === selDept;
      }), 'employee_name');
      if (filtered.length > 3) {
        var f = filtered.slice(0, 2);
        f.push('click for more ...');
        return f
      } else {
        return filtered;
      }
    }

    function getDetailedList(d, selDept) {
      return _.filter($scope.leavedata, function (ld) {
        return moment(ld.work_date).isSame(d) && ld.dept === selDept;
      }, ['employee_name', 'hours_used']);
    }

    function getBirthdayList(d, selDept) {
      return _.pluck(_.filter($scope.birthdaylist, function (bd) {
        return moment(bd.NamedDate).isSame(d) && bd.Dept === selDept;
      }), 'Name');
    }

    function checkHoliday(d) {
      var i = _.findIndex($scope.holidaylist, function (n) {
        return moment(n).isSame(d);
      });
      return i > -1;

    }

    String.prototype.toProperCase = function () {
      return this.replace(/\w\S*/g, function (txt) { return txt.charAt(0).toUpperCase() + txt.substr(1).toLowerCase(); });
    };
  }

})();