/* global moment, _ */
(function () {
  "use strict";
  angular.module('timestoreApp')
      .directive('dateSelector', function () {
        return {
          restrict: 'E',
          templateUrl: 'DateSelect.tmpl.html',
          scope: {
            title: '@',
            label: '@',
            datetype: '@' // ppd = pay period, day = work date
          },
          controller: ['$scope', 'timestoredata', 'timestoreNav', '$routeParams', 'datelist',
          function ($scope, timestoredata, timestoreNav, $routeParams, datelist) {
            $scope.showDayOfWeek = false;
            $scope.dayOfWeek = '';
            $scope.currentPayPeriodStart = timestoredata.getPayPeriodStart();
            $scope.datetypeDisplay = '';
            $scope.allowPrevious = true;
            var m = moment(timestoredata.getPayPeriodStart(), "YYYYMMDD");
            $scope.minDate = m.clone().toDate();
            $scope.maxDate = m.clone().add(1, 'years').toDate();

            if ($scope.datetype === 'ppd') {
              initPPD();
            } else {
              initWorkDay();
            }

            function initPPD() {
              $scope.datetypeDisplay = 'Pay Period';
              if (!checkCurrentPayPeriod()) {
                return;
              }
              $scope.selectedPayPeriodDate = setDate($routeParams.payPeriod);
              $scope.dateList = datelist.getPayPeriodList();
              $scope.prevDate = getDateValue(-14);
              $scope.nextDate = getDateValue(14);
            }

            function initWorkDay() {
              $scope.showDayOfWeek = true;
              $scope.datetypeDisplay = 'Date';
              //if (!checkCurrentPayPeriod()) {
              //    return;
              //}
              $scope.selectedWorkDate = setWorkDate($routeParams.workDate);
              $scope.dayOfWeek = setWorkDay($routeParams.workDate);
              $scope.dateList = datelist.getWorkDayList();
              $scope.prevDate = getDateValue(-1);
              $scope.nextDate = getDateValue(1);
              if (moment().format('YYYYMMDD') === $scope.currentPayPeriodStart && moment().hour() < 10) {
                $scope.allowPrevious = true;
                var m = moment(timestoredata.getPayPeriodStart(), "YYYYMMDD").add(-14, 'days');
                $scope.minDate = m.clone().toDate();
              } else {
                $scope.allowPrevious = (moment($scope.currentPayPeriodStart, 'YYYYMMDD').subtract(1, 'days').isBefore(moment($scope.prevDate, 'M/D/YYYY')));
              }

            }

            function setWorkDate(d) {
              return moment(d, 'YYYYMMDD').toDate();
            }

            function setWorkDay(d) {
              return moment(d, 'YYYYMMDD').format('dddd');
            }

            function setDate(d) {
              return moment(d, 'YYYYMMDD').format('M/D/YYYY');
            }

            $scope.dateChange = function (d) {
              if (d === undefined || d === null) { // let's use the selectedPayPeriod
                if ($scope.datetype === 'ppd') {
                  d = moment($scope.selectedPayPeriodDate, 'M/D/YYYY').format('YYYYMMDD');
                } else {
                  d = moment($scope.selectedWorkDate).format('YYYYMMDD');
                }

              } else {
                d = moment(d, 'M/D/YYYY').format('YYYYMMDD');
              }
              if ($scope.datetype === 'ppd') {
                timestoreNav.changePayPeriod(d);
              } else {
                timestoreNav.changeWorkDate(d);
              }

            };

            function checkCurrentPayPeriod() {
              // this function checks the date passed and 
              // converts it into a valid pay period ending date if is not.
              var d = moment($scope.currentPayPeriod, 'YYYYMMDD');
              var ppe = timestoredata.getPayPeriodEnd(d.format('M/D/YYYY'));
              // First let's check to make sure they passed in a valid date, if they didn't let's 
              // change it to the pay period for that date.
              if (d.format('YYYYMMDD') !== ppe) {
                // someone put in the wrong date, let's fix it.
                timestoreNav.goEmployeeByPPD($scope.employeeId, ppe);
                return false;
              } else { // the selected pay period is a valid pay period ending date.
                return true;
              }
            }

            function getDateValue(days) {
              if ($scope.datetype === 'ppd') {
                return moment($scope.selectedPayPeriodDate, 'M/D/YYYY').add(days, 'days').format('M/D/YYYY');
              } else {
                return moment($scope.selectedWorkDate).add(days, 'days').format('M/D/YYYY');
              }

            }

          }]
        };
      });

}());