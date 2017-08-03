/* global moment, _ */
(function ()
{
  "use strict";
  angular
    .module("timestoreApp")
    .directive("timecardDetail", function ()
    {
      return {
        restrict: "E",
        templateUrl: "TimeCardDetail.tmpl.html", //'/TimeStore/app/timecard/TimeCardDetail.tmpl.html',
        controller: "TimeCardDetailController",
        scope: {
          timecard: "="
        }
      };
    })
    .controller("TimeCardDetailController", [
      "$scope",
      "timestoredata",
      "$mdToast",
      "viewOptions",
      "$routeParams",
      TimeCardDetail
    ]);

  function TimeCardDetail(
    $scope,
    timestoredata,
    $mdToast,
    viewOptions,
    $routeParams
  )
  {
    $scope.selectedWeekTab = 0;
    //updateLeaveRequests();

    //$scope.$on('leaveRequestUpdated', function () {
    //    updateLeaveRequests();
    //});

    $scope.$on("shareTimecardReloaded", function ()
    {
      $scope.selectedWeekTab = 0;
    });

    $scope.SaveHolidays = function ()
    {
      if ($scope.timecard.isHolidayInPPD === false)
      {
        $scope.timecard.HolidayHoursChoice = [];
      } else
      {
        for (var i = 0; i < $scope.timecard.HolidayHoursChoice.length; i++)
        {
          if ($scope.timecard.HolidayHoursChoice[i].toUpperCase() === "NONE")
          {
            showToast(
              "You must choose whether to bank or be paid for the holiday(s) in order to save."
            );
            return;
          }
        }
      }
      if ($scope.BankedHolidaysPaid > $scope.timecard.bankedHoliday)
      {
        alert(
          "You have chosen to be paid for too many Holiday hours.  You have " +
          $scope.timecard.bankedHoliday +
          " hours banked, and are trying to be paid for " +
          $scope.BankedHoursPaid +
          ".  Please correct this and try again."
        );
        return;
      }
      switch ($scope.timecard.TelestaffProfileType)
      {
        case 1:
          if ($scope.timecard.BankedHoursPaid % 24 > 0)
          {
            alert(
              "Your Banked Holiday hours must be allocated in groups of 24 hours."
            );
            return;
          }
          break;
        case 4:
          if ($scope.timecard.BankedHoursPaid % 12 > 0)
          {
            alert(
              "Your Banked Holiday hours must be allocated in groups of 12 hours."
            );
            return;
          }
          break;
      }
      var hr = {
        EmployeeID: $scope.timecard.employeeID,
        PayPeriodStart: $scope.timecard.payPeriodStart,
        CurrentHolidayChoice: $scope.timecard.HolidayHoursChoice,
        BankedHolidaysPaid: $scope.timecard.BankedHoursPaid
      };
      timestoredata.saveHoliday(hr).then(function (data)
      {
        onApproval(data);
      });
    };

    function onApproval(data)
    {
      showToast(data);
      viewOptions.approvalUpdated.approvalUpdated = true;
      viewOptions.approvalUpdated.share();
    }

    $scope.toastPosition = {
      bottom: true,
      top: false,
      left: true,
      right: false
    };
    function showToast(Message)
    {
      $mdToast.show(
        $mdToast
          .simple()
          .content(Message)
          .position($scope.getToastPosition())
          .hideDelay(3000)
      );
    }
    $scope.getToastPosition = function ()
    {
      return Object.keys($scope.toastPosition)
        .filter(function (pos)
        {
          return $scope.toastPosition[pos];
        })
        .join(" ");
    };

    //function updateLeaveRequests() {
    //    timestoredata.getLeaveRequestsByEmployee($routeParams.employeeId)
    //        .then(function (data) {
    //            _.forEach(data.leaveData, function (l) {
    //                l.work_date_display = moment(l.work_date).format('M/D/YYYY');
    //            });
    //            $scope.leaveRequests = data.leaveData;
    //        });
    //}
  }
})();
