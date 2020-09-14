/* global moment, _ */
(function ()
{
  "use strict";
  angular.module('timestoreApp')
    .directive('timeApproval', ['viewOptions', function (viewOptions)
    {
      return {
        bindToController: true,
        restrict: 'E',
        templateUrl: 'TimeApproval.tmpl.html', //'app/timecard/TimeApproval.tmpl.html',
        scope: {
          tc: '='
          //tl: '=',
          //showApproval: '='

        },
        controllerAs: 'ctrl',
        controller: ['$scope', '$mdToast', 'timestoredata', 'commonFunctions', 
          function ($scope, $mdToast, timestoredata, commonFunctions)
          {
            var ctrl = this;
            $scope.$on("shareTimecardReloaded", function ()
            {
              ctrl.checkApproved();
              
            });

            ctrl.showHolidayError = false;
            ctrl.showApprovalButton = false;

            ctrl.checkApproved = function ()
            {
              ctrl.showApprovalButton = false;
              if (ctrl.tc.ErrorList.length > 0)
              {
                return false;
              }
              if (ctrl.tc.Approval_Level !== 0)
              {
                return false;
              }
              if (ctrl.tc.Days_Since_PPE > 1)
              {
                return false;
              }
              if (ctrl.tc.timeList.length === 0)
              {
                return false;
              }
              //if (ctrl.tl === undefined)
              //{
              //  return false;
              //}
              //var ctl = ctrl.tl;
              //if (ctl.length === 0)
              //{
              //  return false;
              //}
              //for (var i = 0; i < ctl.length; i++)
              //{
              //  if (ctl[i].approved === false)
              //  {
              //    return false;
              //  }
              //}
              // Now let's check that the holidays are handled
              if (ctrl.tc.isHolidayTimeBankable === true && ctrl.tc.HolidaysInPPD.length > 0)
              {
                // If they don't have any hours in 134 or 122 then we need to stop.                        
                var ctl = ctrl.tc.timeList;
                ctrl.showHolidayError = true;
                for (var j = 0; j < ctl.length; j++)
                {
                  if (ctl[j].payCode === '122' || ctl[j].payCode === '800' ||  ctl[j].payCode === '134')
                  {
                    ctrl.showHolidayError = false;
                  }
                }
                if (ctrl.showHolidayError === true)
                {
                  return false;
                }
              }
              ctrl.showApprovalButton = true;
              return true;
            };

            ctrl.approve = function ()
            {
              var timecard = ctrl.tc;
              var ad = {
                EmployeeID: timecard.employeeID,
                PayPeriodStart: timecard.payPeriodStart,
                WorkTypeList: timecard.calculatedTimeList,
                Initial_Approval_By_EmployeeID: timecard.Initial_Approval_EmployeeID
              };
              timestoredata.approveInitial(ad)
                .then(onApproval, onError);
            };

            var onApproval = function (data)
            {
              ctrl.showApprovalButton = false;
              showToast(data);              
              viewOptions.approvalUpdated.approvalUpdated = true;
              viewOptions.approvalUpdated.share();
            };

            var onError = function (data)
            {
              alert(data + '  Your approval was not saved!');
            };

            //ctrl.getGroups = function ()
            //{
            //  return commonFunctions.getGroupsByShortPayRate(ctrl.tl);

            //  //var groupArray = [];

            //  //angular.forEach(ctrl.tl, function (item, idx) {
            //  //    if (groupArray.indexOf(parseFloat(item.shortPayRate)) === -1) {
            //  //        groupArray.push(parseFloat(item.shortPayRate));
            //  //    }
            //  //});
            //  //return groupArray;
            //};

            ctrl.toastPosition = {
              bottom: true,
              top: false,
              left: false,
              right: true
            };

            //ctrl.getTotalHours = function ()
            //{
            //  return commonFunctions.getTotalHours(ctrl.tl);
            //  //if (ctrl.tl === undefined) {
            //  //    return 0;
            //  //}
            //  //var tl = ctrl.tl;
            //  //var total = 0;
            //  //for (var i = 0; i < tl.length; i++) {
            //  //    total += tl[i].hours;
            //  //}
            //  //return total;
            //};

            function showToast(Message)
            {
              $mdToast.show(
                $mdToast.simple()
                  .content(Message)
                  .position(ctrl.getToastPosition())
                  .hideDelay(3000)
              );
            }

            ctrl.getToastPosition = function ()
            {
              return Object.keys(ctrl.toastPosition)
                .filter(function (pos) { return ctrl.toastPosition[pos]; })
                .join(' ');
            };

            ctrl.checkApproved();
          }]
      };
    }]);
}());