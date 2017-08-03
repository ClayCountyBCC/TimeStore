/* global moment, _ */
(function () {
  "use strict";
  angular.module('timestoreApp')
      .directive('addCompTime', function () {
        return {
          restrict: 'E',
          scope: {
            timecard: '='
          },
          templateUrl: 'AddCompTime.directive.tmpl.html',
          controller: ['$scope', 'timestoredata', 'timestoreNav', 'viewOptions', '$routeParams', '$timeout',
          function ($scope, timestoredata, timestoreNav, viewOptions, $routeParams, $timeout) {

            $scope.responseMessage = '';
            $scope.week1OT = getBoth($scope.timecard.calculatedTimeList_Week1);
            $scope.week2OT = getBoth($scope.timecard.calculatedTimeList_Week2);
            $scope.week1CompTimeEarned = getComptime($scope.timecard.calculatedTimeList_Week1);
            $scope.week2CompTimeEarned = getComptime($scope.timecard.calculatedTimeList_Week2);

            function getBoth(week) {
              return getTime(week, '231') + getTime(week, '120');
            }

            function getOvertime(week) {
              return getTime(week, '231');
            }

            function getComptime(week) {
              return getTime(week, '120');
            }

            function getTime(week, payCode) {
              return _.result(_.find(week, { payCode: payCode }), 'hours', 0);
            }

            $scope.SaveCompTime = function () {
              var eId = $routeParams.employeeId;
              var PPE = $scope.timecard.payPeriodEndingDisplay;
              timestoredata.saveCompTimeEarned(eId, $scope.week1CompTimeEarned, $scope.week2CompTimeEarned, PPE)
                  .then(onSuccess, onError)

              //alert('Not Implemented Yet');
            }

            $scope.formatNumber = function (n) {
              return (Math.round(n * 4) / 4).toFixed(2);
            }

            function onSuccess(response) {
              viewOptions.approvalUpdated.approvalUpdated = true;
              viewOptions.approvalUpdated.share();
              showMessage('Changes saved.');
            }

            function onError(response) {
              console.log(response);
              showMessage('There was an issue saving your changes.  Please try again and contact MIS if the problem persists.');
            }
            function showMessage(message) {
              $scope.responseMessage = message;
              $timeout(function (t) {
                $scope.responseMessage = '';
              }, 5000);
            }
          }]
        }
      });
})();