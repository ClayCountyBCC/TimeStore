/* global moment, _ */
(function ()
{
  "use strict";
  angular.module('timestoreApp')
    .directive('newLeaveRequest', function ()
    {
      return {
        restrict: 'E',
        templateUrl: 'newLeaveRequest.directive.tmpl.html',
        scope: {
          requests: '='
        },
        controller: ['$scope', 'timestoredata', 'timestoreNav', 'viewOptions', '$routeParams', 'addtimeFunctions', '$timeout',
          function ($scope, timestoredata, timestoreNav, viewOptions, $routeParams, addtimeFunctions, $timeout)
          {
            $scope.responseMessage = '';
            $scope.TCTD = {};
            $scope.timecard = {};
            $scope.showSelection = false;
            $scope.allowPrevious = false;
            $scope.allowNext = false;
            $scope.prevDay = null;
            $scope.nextDay = null;
            $scope.showProgress = false;
            var m = moment();//moment(timestoredata.getPayPeriodStart(), "YYYYMMDD").add(14, 'days');
            $scope.selectedDate = m.clone().startOf('day').toDate();
            m = moment(timestoredata.getPayPeriodStart(), "YYYYMMDD").add(14, 'days');
            $scope.minDate = m.clone().toDate();
            $scope.maxDate = m.clone().add(1, 'years').toDate();
            updateDays();

            $scope.leaveDateSelected = function ()
            {
              $scope.showProgress = true;
              loadTimeCard(moment($scope.selectedDate));
            };

            function loadTimeCard(mWorkDate)
            {
              var ppi = timestoredata.getPayPeriodIndex(mWorkDate);
              var eid = $routeParams.employeeId;
              return timestoredata.getEmployee(ppi, eid)
                .then(processTimeCardData);
            }

            function processTimeCardData(data)
            {
              $scope.timecard = data;
              $scope.TCTD = addtimeFunctions.loadSavedTCTD($scope.timecard, moment($scope.selectedDate).format('M/D/YYYY'));
              updateDays();
              $scope.showProgress = false;
            }

            function updateDays()
            {
              var m = moment($scope.selectedDate);
              var p = m.clone().add(-1, 'days');
              var n = m.clone().add(1, 'days');
              var max = moment($scope.maxDate).add(1, 'days');
              var min = moment($scope.minDate).add(-1, 'days');
              $scope.allowNext = n.isBefore(max) && n.isAfter(min);
              $scope.allowPrevious = p.isAfter(min) && p.isBefore(max);
              $scope.showSelection = $scope.allowNext || $scope.allowPrevious;
              $scope.prevDay = p.format('dddd M/D/YYYY');
              $scope.nextDay = n.format('dddd M/D/YYYY');
            }

            $scope.refreshLeaveRequests = function ()
            {
              timestoredata.getLeaveRequestsByEmployee($routeParams.employeeId)
                .then(processData);
            };

            $scope.goPrevious = function ()
            {
              moveDay(-1);
            };

            $scope.goNext = function ()
            {
              moveDay(1);
            };

            function moveDay(n)
            {
              $scope.selectedDate = moment($scope.selectedDate).add(n, 'days').toDate();
              $scope.leaveDateSelected();
              //updateDays();
            }

            function processData(data)
            {
              $scope.requests = data;
            }

            function checkHourValues()
            {
              var hourTypes = ["VacationHours", "SickHours", "ScheduledLWOPHours", "SickFamilyLeave"];
              for (var i = 0; i < hourTypes.length; i++)
              {
                var raw_v = $scope.TCTD[hourTypes[i]].value;
                if (raw_v !== '')
                {
                  var v = parseFloat(raw_v);
                  if (isNaN(v))
                  {
                    return 'Invalid number of hours entered for ' + hourTypes[i] + '. ';
                  }
                  if (v % .25 > 0)
                  {
                    return 'Invalid number of hours entered for ' + hourTypes[i] + '.  The hours must be rounded to the quarter hour.';
                  }
                  if (v > 11)
                  {
                    return 'Invalid number of hours entered for ' + hourTypes[i] + '. ';
                  }
                }
              }
              return '';
            }

            $scope.saveTCTD = function ()
            {
              if (checkForErrors())
              {
                addtimeFunctions.calculateTotalHours($scope.TCTD, $scope.timecard.isExempt);
                var basetctd = addtimeFunctions.getBaseTCTD($scope.TCTD, $scope.timecard);
                timestoredata.saveTCTD(basetctd)
                  .then(onTCTDSave, onError);
              }

            };

            function checkForErrors()
            {
              var m = checkHourValues();
              if (m.length > 0)
              {
                showMessage(m);
                return false;
              }
              return true;
            }

            function onTCTDSave(response)
            {
              // Changes Saved.
              viewOptions.leaveRequestUpdated.share();
              //viewOptions.approvalUpdated.approvalUpdated = true;
              //viewOptions.approvalUpdated.share();
              showMessage('Changes saved.');
            }


            function onError(response)
            {
              //'An error occurred while attempting to save your request. Please try again.  If this error continues, please contact MIS.'
              showMessage('There was an error saving your data. Please contact MIS to resolve.');
            }

            function showMessage(message)
            {
              $scope.responseMessage = message;
              $timeout(function (t)
              {
                $scope.responseMessage = '';
              }, 5000);
            }

            $scope.resetSelection = function ()
            {
              $scope.selectedDate = moment().startOf('day').toDate();
              $scope.TCTD = {};
              $scope.timecard = {};
              updateDays();
            };

            $scope.calculateTotalHours = function ()
            {
              addtimeFunctions.calculateTotalHours($scope.TCTD, $scope.timecard.isExempt);
              checkForErrors();
            };


          }]
      };
    });

}());