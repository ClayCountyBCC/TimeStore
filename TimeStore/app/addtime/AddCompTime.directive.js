/* global moment, _ */
(function ()
{
  "use strict";
  angular.module('timestoreApp')
    .directive('addCompTime', function ()
    {
      return {
        restrict: 'E',
        scope: {
          timecard: '='
        },
        templateUrl: 'AddCompTime.directive.tmpl.html',
        controller: ['$scope', 'timestoredata', 'timestoreNav', 'viewOptions', '$routeParams', '$timeout',
          function ($scope, timestoredata, timestoreNav, viewOptions, $routeParams, $timeout)
          {
            $scope.compTimeMax = 32;
            $scope.responseMessage = '';
            $scope.formatNumber = function (n)
            {
              return (Math.round(n * 4) / 4).toFixed(2);
            }
            updateEligibleOT();
            // They can only have a maximum of 32 hours of comp time banked
            // In order to enforce this maximum, we need to know how many
            // comp time hours they already have banked
            // $scope.timecard.bankedComp is how many hours they have banked
            // Also important to realize is that the hours they are
            // banking are multipled by 1.5, so we'll need to do that in order
            // to figure out our maximum
            // formula for (Week1/2)CompTimeEarned:
            // ($scope.compTimeMax - timecard.bankedComp - comptimeused) / 1.5

            function updateEligibleOT()
            {
              var compTimeUsed = getComptimeUsed();
              var ct = ($scope.compTimeMax - $scope.timecard.bankedComp + compTimeUsed);
              var maxCompTimeHours = $scope.formatNumber(ct / 1.5);
              var maxcth = parseFloat(maxCompTimeHours);
              if (maxcth * 1.5 > ct)
              {
                maxCompTimeHours = $scope.formatNumber((ct - .25) / 1.5);
              }
              var week1OT = getBoth($scope.timecard.calculatedTimeList_Week1);
              var week2OT = getBoth($scope.timecard.calculatedTimeList_Week2);
              var totalEligibleHours = Math.min(week1OT + week2OT, maxCompTimeHours);
              console.log(week1OT, week2OT, totalEligibleHours, ct, maxCompTimeHours);
              $scope.week1OT = Math.min(week1OT, totalEligibleHours);
              $scope.week2OT = Math.min(week2OT, totalEligibleHours - $scope.week1OT);
              $scope.week1CompTimeEarned = getComptime($scope.timecard.calculatedTimeList_Week1);
              $scope.week2CompTimeEarned = getComptime($scope.timecard.calculatedTimeList_Week2);
            }

            function getTotalCompTimeBanked()
            {
              $scope.totalCompTimeBanked = getComptime($scope.timecard.calculatedTimeList_Week1) + getComptime($scope.timecard.calculatedTimeList_Week2);
            }

            function getBoth(week)
            {
              var ot = getOvertime(week);
              var cttest = ($scope.compTimeMax - $scope.timecard.bankedComp);
              var maxct = $scope.formatNumber(cttest / 1.5);
              ot = Math.min(ot, maxct);
              var ct = getComptime(week);
              return ot + ct;
            }

            function getOvertime(week)
            {
              return getTime(week, '231');
            }

            function getComptime(week)
            {
              return getTime(week, '120');
            }

            function getComptimeUsed()
            {
              return getTime($scope.timecard.calculatedTimeList_Week1, '121') +
                getTime($scope.timecard.calculatedTimeList_Week2, '121');
            }

            function getTime(week, payCode)
            {
              return _.result(_.find(week, { payCode: payCode }), 'hours', 0);
            }

            $scope.SaveCompTime = function ()
            {
              var eId = $routeParams.employeeId;
              var PPE = $scope.timecard.payPeriodEndingDisplay;
              timestoredata.saveCompTimeEarned(eId, $scope.week1CompTimeEarned, $scope.week2CompTimeEarned, PPE)
                .then(onSuccess, onError)

              //alert('Not Implemented Yet');
            }



            function onSuccess(response)
            {
              viewOptions.approvalUpdated.approvalUpdated = true;
              viewOptions.approvalUpdated.share();
              showMessage('Changes saved.');
            }

            function onError(response)
            {
              console.log(response);
              showMessage('There was an issue saving your changes.  Please try again and contact MIS if the problem persists.');
            }
            function showMessage(message)
            {
              $scope.responseMessage = message;
              $timeout(function (t)
              {
                $scope.responseMessage = '';
              }, 5000);
            }
          }]
      }
    });
})();