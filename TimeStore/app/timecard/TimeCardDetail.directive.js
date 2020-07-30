/* global moment, _ */
(function ()
{
  "use strict";
  angular
    .module("timestoreApp")
    .directive("timecardDetail", function ()
    {
      return {
        bindToController: true,
        restrict: "E",
        templateUrl: "TimeCardDetail.tmpl.html", //'app/timecard/TimeCardDetail.tmpl.html',
        controller: "TimeCardDetailController",
        controllerAs: 'ctrl',
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
      "timestoreNav",
      TimeCardDetail
    ]);

  function TimeCardDetail(
    $scope,
    timestoredata,
    $mdToast,
    viewOptions,
    $routeParams,
    timestoreNav
  )
  {
    var ppd = $routeParams.payPeriod;
    $scope.TaxWitholdingCutoff = moment(ppd, "YYYYMMDD").format("M/D/YYYY") + " 5:00 PM";
    var ctrl = this;
    ctrl.selectedWeekTab = 0;
    //updateLeaveRequests();

    //ctrl.$on('leaveRequestUpdated', function () {
    //    updateLeaveRequests();
    //});
    //$scope.$watch('timecard', function (newValue, oldValue, scope)
    //{
    //  $scope.timecard = newValue;
    //  console.log('$scope.watch fired', newValue, oldValue);
    //});
    $scope.$on("shareTimecardReloaded", function ()
    {
      ctrl.selectedWeekTab = 0;
    });

    ctrl.SaveHolidays = function ()
    {
      if (ctrl.timecard.isHolidayInPPD === false)
      {
        ctrl.timecard.HolidayHoursChoice = [];
      } else
      {
        for (var i = 0; i < ctrl.timecard.HolidayHoursChoice.length; i++)
        {
          if (ctrl.timecard.HolidayHoursChoice[i].toUpperCase() === "NONE")
          {
            showToast(
              "You must choose whether to bank or be paid for the holiday(s) in order to save."
            );
            return;
          }
        }
      }
      if (ctrl.BankedHolidaysPaid > ctrl.timecard.bankedHoliday)
      {
        alert(
          "You have chosen to be paid for too many Holiday hours.  You have " +
          ctrl.timecard.bankedHoliday +
          " hours banked, and are trying to be paid for " +
          ctrl.BankedHoursPaid +
          ".  Please correct this and try again."
        );
        return;
      }
      switch (ctrl.timecard.TelestaffProfileType)
      {
        case 1:
          if (ctrl.timecard.BankedHoursPaid % 24 > 0)
          {
            alert(
              "Your Banked Holiday hours must be allocated in groups of 24 hours."
            );
            return;
          }
          break;
        case 4:
          if (ctrl.timecard.BankedHoursPaid % 12 > 0)
          {
            alert(
              "Your Banked Holiday hours must be allocated in groups of 12 hours."
            );
            return;
          }
          break;
      }
      var hr = {
        EmployeeID: ctrl.timecard.employeeID,
        PayPeriodStart: ctrl.timecard.payPeriodStart,
        CurrentHolidayChoice: ctrl.timecard.HolidayHoursChoice,
        BankedHolidaysPaid: ctrl.timecard.BankedHoursPaid
      };
      timestoredata.saveHoliday(hr).then(function (data)
      {
        onApproval(data);
      });
    };

    function onApproval(data)
    {
      showToast(data);
      timestoreNav.goHome();
    }

    ctrl.toastPosition = {
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
          .position(ctrl.getToastPosition())
          .hideDelay(3000)
      );
    }

    ctrl.getToastPosition = function ()
    {
      return Object.keys(ctrl.toastPosition)
        .filter(function (pos)
        {
          return ctrl.toastPosition[pos];
        })
        .join(" ");
    };

  }
})();
