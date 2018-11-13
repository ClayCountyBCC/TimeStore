/* global moment, _ */
(function ()
{
  "use strict";
  angular
    .module("timestoreApp")
    .directive("addDateAndTime", function ()
    {
      return {
        restrict: "E",
        scope: {
          timecard: "="
        },
        templateUrl: "AddTime.directive.tmpl.html",
        controller: "AddTimeDirectiveController"
      };
    })
    .controller("AddTimeDirectiveController", [
      "$scope",
      "timestoredata",
      "timestoreNav",
      "$routeParams",
      "datelist",
      "viewOptions",
      "addtimeFunctions",
      "$timeout",
      AddTimeDirective
    ]);

  function AddTimeDirective(
    $scope,
    timestoredata,
    timestoreNav,
    $routeParams,
    datelist,
    viewOptions,
    addtimeFunctions,
    $timeout
  )
  {
    // testing variables
    // end testing variables    
    timestoredata.getMyAccess()
      .then(function (data)
      {
        $scope.myAccess = data;
      });
    $scope.responseMessage = "";
    $scope.workDate = moment($routeParams.workDate, "YYYYMMDD").format(
      "M/D/YYYY"
    );
    $scope.urlWorkDate = $routeParams.workDate;
    $scope.urlEmployeeId = $routeParams.employeeId;
    $scope.timeListType = "short";
    $scope.errorList = [];
    $scope.warningList = [];
    $scope.shiftMaxHours = 0;
    $scope.forceFullTimeList = false;
    $scope.bankedCompWeek1 = 0;
    $scope.fullTimeList = [];
    $scope.shortTimeList = [];
    $scope.lunchTimeList = [];
    $scope.timeList = [];
    $scope.TCTD = addtimeFunctions.loadSavedTCTD(
      $scope.timecard,
      $scope.workDate
    ); 
    $scope.isCurrentPPD = isCurrentPayPeriod();
    $scope.disasterChoiceError = "";
    $scope.disasterTimeError = "";
    //$scope.disasterHours = $scope.TCTD.DisasterWorkHours.value > 0 ? 'true' : null;
    
    populateTimeLists();
    updateBanksUsed();
    $scope.toggleOnCall = $scope.TCTD.OnCallTotalHours.value > 0;
    $scope.DisasterHoursRelated = $scope.TCTD.DisasterWorkHours.value === 0 ? null : true;
    $scope.ShowDisasterWarning = $scope.TCTD.DisasterName.length > 0 ? $scope.TCTD.DisasterPeriodType === 1 : $scope.timecard.DisasterPeriodType_Display === 1;
    $scope.showDisaster = $scope.DisasterHoursRelated ? true : false;
    checkForErrors();

    function populateTimeLists()
    {
      var tl = addtimeFunctions.getTimeList();
      for (var i = 0; i < tl.length; i++)
      {
        $scope.fullTimeList.push({ display: tl[i], index: i });
      }
      updateLunchTimeList();

      $scope.shortTimeList = $scope.fullTimeList.slice(24, 73);
      updateTimeLists();
    }

    function updateLunchTimeList()
    {
      $scope.lunchTimeList = [];
      if ($scope.TCTD.selectedTimes.length > 0 && $scope.fullTimeList.length > 0)
      {
        $scope.lunchTimeList = $scope.fullTimeList.slice($scope.TCTD.selectedTimes[0] + 1);
      }
    }

    function updateTimeLists()
    {
      $scope.forceFullTimeList = outsideShortTimes();
      if ($scope.forceFullTimeList)
      {
        $scope.timeListType = "full";
        $scope.timeList = $scope.fullTimeList;
      } else
      {
        $scope.timeList = $scope.shortTimeList;
      }
    }

    function outsideShortTimes()
    {
      var h = moment().hour();
      if (h < 6 || h > 18) return true;
      if ($scope.TCTD.selectedTimes.length === 0) return false;
      var min = _.min($scope.TCTD.selectedTimes);
      var max = _.max($scope.TCTD.selectedTimes);
      return min < 24 || max > 72;
    }

    $scope.updateTimeList = function ()
    {
      $scope.timeList = [];
      if ($scope.timeListType === "short")
      {
        $scope.timeList = $scope.shortTimeList;
      } else
      {
        $scope.timeList = $scope.fullTimeList;
      }
    };

    function updateBanksUsed()
    {
      $scope.vacationUsed = addtimeFunctions.getBankedHoursUsed(
        $scope.timecard,
        "V",
        $scope.workDate
      );
      $scope.sickUsed = addtimeFunctions.getBankedHoursUsed(
        $scope.timecard,
        "S",
        $scope.workDate
      );
      $scope.compUsed = addtimeFunctions.getBankedHoursUsed(
        $scope.timecard,
        "C",
        $scope.workDate
      );
      if ($scope.TCTD !== undefined)
      {
        $scope.vacationUsed += $scope.TCTD.VacationHours.value;
        $scope.sickUsed += $scope.TCTD.SickHours.value;
        $scope.sickUsed += $scope.TCTD.SickFamilyLeave.value;
        $scope.compUsed += $scope.TCTD.CompTimeUsed.value;
      }
      var wd = moment($scope.workDate, "M/D/YYYY");
      var week1end = moment($scope.timecard.payPeriodStart).add(6, "days");
      $scope.bankedCompWeek1 = 0;
      if (wd.isAfter(week1end))
      {
        $scope.bankedCompWeek1 = formatNumber(
          getComptime($scope.timecard.calculatedTimeList_Week1) * 1.5
        );
      }
    }

    $scope.DisasterHoursChoice = function ()
    {
      $scope.showDisaster = $scope.DisasterHoursRelated ? true : false;
      if ($scope.DisasterHoursRelated === false)
      {
        $scope.calculateTotalHours();
      }
    };

    function validateDisasterHours()
    {
      $scope.disasterChoiceError = "";
      $scope.disasterTimeError = "";
      // we need to compare the times in DisasterWorkTimes and WorkTimes
      // to ensure that they always overlap
      if ($scope.timecard.DisasterName_Display.length > 0 && $scope.TCTD.WorkHours.value > 0)
      {
        if ($scope.DisasterHoursRelated === null && $scope.ShowDisasterWarning)
        {
          $scope.disasterChoiceError = "You must select whether or not any of the work hours entered are for the disaster indicated.";
          $scope.errorList.push("You must select whether or not any of the work hours entered are for the disaster indicated.");
          return;
        }
        var dst = $scope.TCTD.disasterSelectedTimes;
        var wst = $scope.TCTD.selectedTimes;
        var dLength = dst.length - dst.length % 2;
        var wLength = wst.length - wst.length % 2;
        for (var i = 0; i < dLength; i += 2)
        {
          var dStart = dst[i];
          var dEnd = dst[i + 1];
          var found = false;
          for (var j = 0; j < wLength; j += 2)
          {
            var wStart = wst[j];
            var wEnd = wst[j + 1];
            var dStartGood = dStart >= wStart && dStart <= wEnd;
            var dEndGood = dEnd >= wStart && dEnd <= wEnd;
            found = dStartGood && dEndGood;
            if (dStartGood || dEndGood)
            {
              break;
            }
          }
          if (!found)
          {
            $scope.disasterTimeError = "Your hours worked and your disaster hours must overlap. You cannot have any disaster hours outside of your hours worked.";
            $scope.errorList.push("Your hours worked and your disaster hours must overlap. You cannot have any disaster hours outside of your hours worked.");
            return;
          }
        }
        if ($scope.TCTD.DisasterWorkHours.value > 0) // && $scope.TCTD.Comment.length === 0
        {
          if ($scope.TCTD.DisasterWorkType === "")
          {
            $scope.disasterTimeError = "You must select the type of work you did for the Disaster.";
            $scope.errorList.push($scope.disasterTimeError);
            return;
          }
          if ($scope.TCTD.DisasterWorkType === "Not Listed" && $scope.TCTD.Comment.length === 0)
          {
            $scope.disasterTimeError = "Please enter a comment that indicates the type of work you did for the disaster.";
            $scope.errorList.push($scope.disasterTimeError);
          }
        }
      }
    }

    function checkForErrors()
    {
      $scope.errorList = [];
      $scope.warningList = [];
      $scope.shiftMaxHours = addtimeFunctions.getMaxShiftHours();
      handleExemptShiftDuration();
      checkHourValues();
      checkOnCallHours();
      updateBanksUsed();
      validateDisasterHours();
      $scope.isCurrentPPD = isCurrentPayPeriod();

      // here we're going to do as much error handling as possible.
      // Any error will make this function break, and the error message text will be updated to indicate it.
      // Errors:
      // todo:

      // Let's make sure they aren't using more hours than they have banked.
      // These should be errors.
      var hireDateCheck = addtimeFunctions.calculateWithinFirstNinetyDays(
        $scope.timecard.hireDate,
        $scope.TCTD.WorkDate
      );
      //console.log("hire date check", hireDateCheck);
      if (!hireDateCheck && ($scope.sickUsed > 0 || $scope.vacationUsed > 0))
      {
        $scope.errorList.push(
          "You cannot use Sick or Vacation hours in your first 90 days."
        );
      }

      // make sure they entered an even number of times,
      // must have a start and an end time each.
      if (
        $scope.TCTD.selectedTimes !== undefined &&
        $scope.TCTD.selectedTimes.length % 2 === 1
      )
      {
        var times =
          $scope.fullTimeList[
          $scope.TCTD.selectedTimes[$scope.TCTD.selectedTimes.length - 1]
          ];
        $scope.warningList.push(
          "You have selected a Work start time of " +
          times.display +
          ", but did not select an end time."
        );
        //$scope.warningList.push('You have selected an odd number of regular work hours. You should always select them as a start time and an end time.');
      }

      //      Not enough time entered for the day (scheduled work days only)
      if (
        $scope.TCTD.TotalHours.value < $scope.shiftMaxHours &&
        $scope.isCurrentPPD
      )
      {
        $scope.warningList.push(
          "You may not have entered enough time, you must have a minimum of " +
          $scope.shiftMaxHours +
          " hours entered if this is a scheduled work day."
        );
      }
      //      Too much non-working time entered on a scheduled work day.
      if ($scope.TCTD.TotalHours.value > $scope.shiftMaxHours)
      {
        if (
          $scope.TCTD.AdminHours.value > 0 ||
          $scope.TCTD.VacationHours.value > 0 ||
          $scope.TCTD.SickHours.value > 0 ||
          $scope.TCTD.SickLeavePoolHours.value > 0 ||
          $scope.TCTD.CompTimeUsed.value > 0 ||
          $scope.TCTD.LWOPHours.value > 0
        )
        {
          $scope.warningList.push(
            "Too many Non-Working hours entered. You should reduce your Non-Working hours until you have a total of " +
            $scope.shiftMaxHours +
            " hours."
          );
        }
      }
      // If full day of admin, make sure a comment is entered.
      // This logic will probably need to be revised, it is way too specific as entered.
      if (
        $scope.TCTD.TotalHours.value === $scope.TCTD.AdminHours.value &&
        $scope.TCTD.TotalHours.value >= $scope.shiftMaxHours &&
        $scope.TCTD.Comment.trim().length === 0
      )
      {
        $scope.errorList.push(
          "You must enter a comment when taking a full day of Admin leave. "
        );
      }

      //      Make sure they aren't using too much vacation / sick / comp time across the whole pay period.
      //      What I will do here is check each type of hours that will go into a bank. If they are using any of
      //      those hours on the day we are looking at right here, I'll then add up how much they are using
      //      the whole pay period and if they are using too much, I'll throw an error.

      if (
        $scope.vacationUsed > 0 &&
        $scope.TCTD.VacationHours.value > 0 &&
        $scope.timecard.bankedVacation - $scope.vacationUsed < 0
      )
      {
        $scope.errorList.push(
          "Too many Vacation hours used.  You are trying to use " +
          $scope.vacationUsed +
          " hours and only have " +
          $scope.timecard.bankedVacation +
          " hours banked."
        );
      }
      if (
        $scope.sickUsed > 0 &&
        $scope.TCTD.SickHours.value > 0 &&
        $scope.timecard.bankedSick - $scope.sickUsed < 0
      )
      {
        $scope.errorList.push(
          "Too many Sick hours used.  You are trying to use " +
          $scope.sickUsed +
          " hours and only have " +
          $scope.timecard.bankedSick +
          " hours banked."
        );
      }
      if (
        $scope.compUsed > 0 &&
        $scope.TCTD.CompTimeUsed.value > 0 &&
        $scope.timecard.bankedComp + $scope.bankedCompWeek1 - $scope.compUsed <
        0
      )
      {
        $scope.errorList.push(
          "Too many Comp Time hours used.  You are trying to use " +
          $scope.compUsed +
          " hours and only have " +
          $scope.timecard.bankedComp +
          " hours banked."
        );
      }

      //var t = checkBankableHours('Vacation', $scope.TCTD.VacationHours.value, $scope.timecard.bankedVacation);
      //if (t.length > 0) {
      //    $scope.errorList.push(t);
      //}
      //t = checkBankableHours('Sick', $scope.TCTD.SickHours.value, $scope.timecard.bankedSick);
      //if (t.length > 0) {
      //    $scope.errorList.push(t);
      //}
      //t = checkBankableHours('Comp Time', $scope.TCTD.CompTimeUsed.value, $scope.timecard.bankedComp);
      //if (t.length > 0) {
      //    $scope.errorList.push(t);
      //}
    }

    function checkOnCallHours()
    {
      // here we need to look to make sure we're not overlapping with the work hours.
      var times = "";
      if ($scope.TCTD.selectedTimes.length === 0)
      {
        return;
      }
      if (
        $scope.TCTD.OnCallSelectedTimes !== undefined &&
        $scope.TCTD.OnCallSelectedTimes.length % 2 === 1
      )
      {
        times =
          $scope.timeList[
          $scope.TCTD.OnCallSelectedTimes[
          $scope.TCTD.OnCallSelectedTimes.length - 1
          ]
          ];
        $scope.warningList.push(
          "You have selected an On Call start time of " +
          times +
          ", but did not select an end time."
        );
      }
      var st = $scope.TCTD.selectedTimes;
      var ocst = $scope.TCTD.OnCallSelectedTimes;
      for (var i = 0; i < st.length; i += 2)
      {
        for (var j = 0; j < ocst.length; j += 2)
        {
          if (
            (ocst[j] > st[i] && ocst[j] < st[i + 1]) ||
            (ocst[j + 1] > st[i] && ocst[j + 1] < st[i + 1])
          )
          {
            times =
              $scope.timeList[ocst[j]] + " - " + $scope.timeList[ocst[j + 1]];
            $scope.errorList.push(
              "The On Call times " +
              times +
              " are overlapping with your regular hours worked.  You must correct this to continue."
            );
          }
        }
      }
    }

    function checkHourValues()
    {
      var hourTypes = addtimeFunctions.getHourTypesList();
      for (var i = 0; i < hourTypes.length; i++)
      {
        var raw_v = $scope.TCTD[hourTypes[i]].value;

        if (raw_v === null || raw_v === undefined)
        {
          $scope.TCTD[hourTypes[i]].value = 0;
          raw_v = 0;
        }
        if (raw_v !== "" && raw_v !== null)
        {
          var v = parseFloat(raw_v);
          if (isNaN(v))
          {
            $scope.errorList.push(
              "Invalid number of hours entered for " + hourTypes[i] + ". "
            );
          }
          if (v % 0.25 > 0)
          {
            $scope.errorList.push(
              "Invalid number of hours entered for " +
              hourTypes[i] +
              ".  The hours must be rounded to the quarter hour."
            );
          }
          if (v > 24)
          {
            $scope.errorList.push(
              "Too many hours entered for " + hourTypes[i] + ". "
            );
          }
        }
      }
    }

    function checkBankableHours(hourType, dayHours, bankedHours)
    {
      if (dayHours > 0)
      {
        var totalHours = getAllHours(hourType, $scope.timecard) + dayHours;
        if (totalHours >= bankedHours)
        {
          return (
            "You have requested more " +
            hourType +
            " hours than you have banked."
          );
        }
      }
      return "";
    }

    function getAllHours(hourType, tc)
    {
      var hours = 0;
      for (var i = 0; i < tc.RawTime.length; i++)
      {
        var index = _.findIndex(tc.RawTime[i].workHoursList, {
          name: hourType
        });
        if (index !== -1)
        {
          hours += tc.RawTime[i].workHoursList[index].hours;
        }
      }
      return hours;
    }

    $scope.resetTimes = function ()
    {
      $scope.TCTD = addtimeFunctions.resetTCTD(
        $scope.timecard,
        $scope.workDate
      );
    };

    function handleExemptShiftDuration()
    {
      // This function does not handle employees that are exempt that are not on
      // 8 hour schedules.  So the one person who is exempt that is on a 12 hour a week
      // schedule and if there are any 10+ hour a day exempt employees, this function will need
      // to be updated.
      if ($scope.timecard.exemptStatus === "Exempt")
      {
        var t = $scope.TCTD;
        if (
          t.VacationHours.value > 0 ||
          t.SickHours.value > 0 ||
          t.SickLeavePoolHours.value > 0 ||
          t.AdminHours.value > 0 ||
          t.LWOPHours.value > 0
        )
        {
          $scope.shiftMaxHours = 8;
        } else
        {
          $scope.shiftMaxHours = 7.5;
        }
      }
    }

    $scope.saveTCTD = function ()
    {
      var basetctd = addtimeFunctions.getBaseTCTD($scope.TCTD, $scope.timecard);
      console.log('basetctd', basetctd);
      timestoredata.saveTCTD(basetctd).then(onTCTDSave, onError);
    };

    function onTCTDSave(response)
    {
      // Changes Saved.
      viewOptions.approvalUpdated.approvalUpdated = true;
      viewOptions.approvalUpdated.share();
      showMessage("Changes saved.");
    }

    function onError(response)
    {
      showMessage(
        "An error occurred attempting to save your data. Please contact MIS for more information."
      );
    }

    $scope.Toggle_AdminHours = function ()
    {
      $scope.TCTD.showAdminHours = !$scope.TCTD.showAdminHours;
    };

    $scope.Toggle_DisasterHours = function ()
    {
      console.log('showDisaster', $scope.showDisaster);
      $scope.showDisaster = !$scope.showDisaster;
      
    };

    $scope.Toggle_OnCallHours = function ()
    {
      $scope.toggleOnCall = !$scope.toggleOnCall;
    };

    $scope.checkDisasterWorkType = function ()
    {
      // this function is run after a disaster work type is selected.
      // the comment is only required if they chose "Not Listed" as the option.
      checkForErrors();
    };

    $scope.calculateTotalHours = function ()
    {
      $scope.forceFullTimeList = outsideShortTimes();
      updateLunchTimeList();
      addtimeFunctions.calculateTotalHours(
        $scope.TCTD,
        $scope.timecard.exemptStatus === "Exempt"
      );
      checkForErrors();
      console.log('lastcheck tctd', $scope.TCTD);
      if ($scope.errorList.length === 0)
      {
        $scope.saveTCTD();
      }
    };

    function showMessage(message)
    {
      $scope.responseMessage = message;
      $timeout(function (t)
      {
        $scope.responseMessage = "";
        updateBanksUsed();
      }, 5000);
    }

    function isCurrentPayPeriod()
    {
      var pps = moment(timestoredata.getPayPeriodStart(null, true), "YYYYMMDD");
      var ppe = pps.clone().add(13, "days");
      var wd = moment($scope.workDate, "M/D/YYYY");
      //return wd.isBetween(pps, ppe);
      return !wd.isAfter(ppe);
    }

    function isAfterCurrentPayPeriod()
    {
      var pps = moment(timestoredata.getPayPeriodStart(), "YYYYMMDD").add(
        14,
        "days"
      );
      var wd = moment($scope.workDate, "M/D/YYYY");
      return wd.isAfter(pps);
    }

    function getComptime(week)
    {
      return getTime(week, "120");
    }

    function getTime(week, payCode)
    {
      return _.result(_.find(week, { payCode: payCode }), "hours", 0);
    }

    function formatNumber(n)
    {
      return (Math.round(n * 4) / 4).toFixed(2);
    }
  }
})();
