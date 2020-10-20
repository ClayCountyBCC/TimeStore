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

    $scope.normallyScheduledHours = addtimeFunctions.getNormallyScheduledHours();

    $scope.timeList = [];
    $scope.TCTD = addtimeFunctions.loadSavedTCTD(
      $scope.timecard,
      $scope.workDate
    ); 
    $scope.isCurrentPPD = isCurrentPayPeriod();
    $scope.disasterChoiceError = "";
    $scope.disasterTimeError = "";
    $scope.normallyScheduledHoursError = "";
    //$scope.disasterHours = $scope.TCTD.DisasterWorkHours.value > 0 ? 'true' : null;
    
    populateTimeLists();
    updateBanksUsed();
    //$scope.DisasterHoursRelated    
    $scope.toggleOnCall = $scope.TCTD.OnCallTotalHours.value > 0;
    var forceDisasterShow = $scope.TCTD.EventsByWorkDate.filter(function (j) { return j.disaster_work_hours.DisasterWorkHours > 0; }).length > 0;
    console.log('force disaster show', forceDisasterShow);
    $scope.DisasterHoursRelated = forceDisasterShow ? true : null;    
    $scope.showDisaster = $scope.TCTD.EventsByWorkDate.length > 0;
    $scope.ExpandDisasterHours = $scope.showDisaster && $scope.TCTD.WorkHours.value > 0 && $scope.DisasterHoursRelated;
    
    // changed on 8/31/2019
    //$scope.ShowDisasterWarning = $scope.TCTD.DisasterName.length > 0 ? $scope.TCTD.DisasterPeriodType === 1 : $scope.timecard.DisasterPeriodType_Display === 1;
    //$scope.ShowDisasterWarning = $scope.TCTD.DisasterName.length > 0; // ? $scope.TCTD.DisasterPeriodType === 1 : $scope.timecard.DisasterPeriodType_Display === 1;
    //$scope.showDisaster = $scope.DisasterHoursRelated ? true : false;
    //$scope.showDisaster = $scope.TCTD.DisasterName.length > 0;
    //$scope.TCTD.DisasterNormalScheduledHours values are as follows:
    // -1 if the value has not yet been saved
    // 0 if they are not normally scheduled on this day
    // > 0 if they are normally scheduled
    $scope.NormallyScheduled = null;
    
    if ($scope.TCTD.DisasterNormalScheduledHours === 0)
    {
      $scope.NormallyScheduled = false;
    }
    if ($scope.TCTD.DisasterNormalScheduledHours > 0)
    {
      $scope.NormallyScheduled = true;
    }

    //$scope.NormallyScheduled= $scope.TCTD.DisasterNormalScheduledHours === -1 ? null : $scope.TCTD.DisasterNormalScheduledHours > 0;
    $scope.ShowDisasterNormallyScheduledHours = $scope.NormallyScheduled === true;

      //$scope.TCTD.DisasterNormalScheduledHours.toString() !== "-1";
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
      console.log("Don't forget to clear the disaster hours entered if No is chosen.");
      $scope.ExpandDisasterHours = $scope.DisasterHoursRelated;
      if (!$scope.DisasterHoursRelated)
      {
        // let's clear any saved disaster hours information
        $scope.TCTD.DisasterWorkHoursList.splice(0, $scope.TCTD.DisasterWorkHoursList.length); // empty the array
        $scope.TCTD.DisasterNormalScheduledHours = null;
        for (let ee = 0; ee < $scope.TCTD.EventsByWorkDate.length; ee++)
        {
          let dwh = $scope.TCTD.EventsByWorkDate[ee].disaster_work_hours;          
          dwh.DisasterSelectedTimes.splice(0, dwh.DisasterSelectedTimes.length)
          dwh.DisasterselectedTimesDisplay = "";
          dwh.DisasterWorkHours = 0;
          dwh.DisasterWorkTimes = "";
          dwh.DisasterWorkType = "";
        }
      }
      checkForErrors();
      if (!$scope.DisasterHoursRelated && $scope.errorList.length === 0)
      {

        $scope.calculateTotalHours();
      }
    };
    
    $scope.NormallyScheduledChoice = function ()
    {
      $scope.ShowDisasterNormallyScheduledHours = $scope.NormallyScheduled;
      $scope.TCTD.DisasterNormalScheduledHours = $scope.NormallyScheduled ? null : 0;
      checkForErrors();
      $scope.calculateTotalHours();
    };

    $scope.NormallyScheduledHoursSelected = function ()
    {
      checkForErrors();
      $scope.calculateTotalHours();
    };

    $scope.CopyWorkHoursToDisasterWorkHours = function ()
    {
      $scope.TCTD.disasterSelectedTimes = $scope.TCTD.selectedTimes;
      $scope.TCTD.disasterSelectedTimesDisplay = $scope.TCTD.selectedTimesDisplay;
      $scope.calculateTotalHours();
    };

    function validateDisasterHours()
    {
      $scope.disasterChoiceError = "";
      $scope.disasterTimeError = "";
      $scope.normallyScheduledHoursError = "";
      // we need to compare the times in DisasterWorkTimes and WorkTimes
      // to ensure that they always overlap
      //if ($scope.timecard.DisasterName_Display.length > 0 && $scope.TCTD.WorkHours.value > 0)
      if ($scope.TCTD.WorkHours.value === 0) return;

      if (!$scope.showDisaster) return;

      if ($scope.DisasterHoursRelated === null)
      {
        $scope.disasterChoiceError = "You must select whether any of the work hours entered are for any special events. Your time has not yet been saved.";
        $scope.errorList.push("You must select whether any of the work hours entered are for any special events. Your time has not yet been saved.");
        return;
      }

      if (!$scope.DisasterHoursRelated) return;

      if ($scope.NormallyScheduled === null)
      {
        $scope.normallyScheduledHoursError = "You must select if you are normally scheduled to work on this date.";
        $scope.errorList.push("You must select if you are normally scheduled to work on this date.");
        return;
      }

      if ($scope.NormallyScheduled && ($scope.TCTD.DisasterNormalScheduledHours === null || $scope.TCTD.DisasterNormalScheduledHours.toString() === "-1"))
      {
        $scope.normallyScheduledHoursError = "You must select how many hours you normally work on this date.";
        $scope.errorList.push("You must select how many hours you normally work on this date.");
        return;
      }

      let ewd = $scope.TCTD.EventsByWorkDate;
      let total_disaster_work_hours = 0;

      for (let ee = 0; ee < ewd.length; ee++)
      {
        let special_event = ewd[ee];
        let dwh = special_event.disaster_work_hours;

        // reset error message
        dwh.DisasterTimesError = "";

        total_disaster_work_hours += dwh.DisasterWorkHours;

        if (dwh.DisasterWorkHours === 0 && total_disaster_work_hours === 0 && ee === ewd.length - 1)
        {
          $scope.normallyScheduledHoursError = "You must add your hours worked for any special events to the section below.";
          $scope.errorList.push("You must add your hours worked for any special events to the specific event section.");
          return;
        }

        // let's make sure these disaster hours don't overlap the regular hours worked
        if (!HoursWorkedMustOverlap($scope.TCTD.selectedTimes, dwh.DisasterSelectedTimes))
        {
          dwh.DisasterTimesError = "Yours hours worked on " + special_event.event_name + " must overlap your regular hours worked.";
          $scope.errorList.push(dwh.DisasterTimesError);
          return;
        }

        // now we need to make sure that the disaster hours don't overlap other disaster hours
        if (ewd.length > 1)
        {
          for (let ff = 0; ff < ewd.length; ff++)
          {
            if (ff !== ee)
            {
              let special_event_ff = ewd[ff];
              let dwh_ff = special_event_ff.disaster_work_hours;
              if (!HoursWorkedMustNotOverlap(dwh.DisasterSelectedTimes, dwh_ff.DisasterSelectedTimes))
              {
                dwh.DisasterTimesError = "Yours hours worked on " + special_event.event_name + " are overlapping the hours you entered for " + special_event_ff.event_name + ". The same time range cannot be allocated more than once.";
                $scope.errorList.push(dwh.DisasterTimesError);
                return;
              }
              if (!HoursWorkedMustNotOverlap(dwh_ff.DisasterSelectedTimes, dwh.DisasterSelectedTimes))
              {
                dwh.DisasterTimesError = "Yours hours worked on " + special_event.event_name + " are overlapping the hours you entered for " + special_event_ff.event_name + ". The same time range cannot be allocated more than once.";
                $scope.errorList.push(dwh.DisasterTimesError);
                return;
              }
            }
          }
        }

        // Check that a disaster work type was selected
        if (dwh.DisasterWorkHours > 0)
        {
          if (dwh.DisasterWorkType.length === 0)
          {
            dwh.DisasterTimesError = "You must select the type of work you did for " + special_event.event_name + ".";
            $scope.errorList.push(dwh.DisasterTimesError);
            return;
          }
          if (dwh.DisasterWorkType === "Not Listed" && $scope.TCTD.Comment.length === 0)
          {
            dwh.DisasterTimesError = "Please enter a comment that indicates the type of work you did for " + special_event.event_name + ".";
            $scope.errorList.push(dwh.DisasterTimesError);
            return;
          }
        }
        else
        {
          dwh.DisasterWorkType = "";
        }
      }

    }


    function HoursWorkedMustOverlap(a, b)
    {
      // this function will return true if all array elements overlap, and false if any don't
      let bLength = b.length - b.length % 2; // this ensures we're only working with a mod 2 array length
      let aLength = a.length - a.length % 2;
      for (let i = 0; i < bLength; i += 2)
      {
        let bStart = b[i];
        let bEnd = b[i + 1];
        let found = false;
        for (let j = 0; j < aLength; j += 2)
        {
          let aStart = a[j];
          let aEnd = a[j + 1];
          let bStartGood = bStart >= aStart && bStart <= aEnd;
          let bEndGood = bEnd >= aStart && bEnd <= aEnd;
          found = bStartGood && bEndGood;
          if (bStartGood || bEndGood)
          {
            break;
          }
        }
        if (!found)
        {
          return false;
        }
      }
      return true;
    }

    function HoursWorkedMustNotOverlap(a, b)
    {
      // this function will return true if any array elements do NOT overlap, and false if they do
      let bLength = b.length - b.length % 2; // this ensures we're only working with a mod 2 array length
      let aLength = a.length - a.length % 2;
      for (let i = 0; i < bLength; i += 2)
      {
        let bStart = b[i];
        let bEnd = b[i + 1];
        for (let j = 0; j < aLength; j += 2)
        {
          let aStart = a[j];
          let aEnd = a[j + 1];
          let bStartGood = bStart >= aStart && bStart < aEnd;
          let bEndGood = bEnd > aStart && bEnd <= aEnd;
          if (bStartGood || bEndGood)
          {
            return false;
          }
        }
      }
      return true;
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
        $scope.isCurrentPPD && $scope.timecard.exemptStatus !== "Exempt"
      )
      {
        var warningmessage = "";
        if ($scope.TCTD.TotalHours.value < 8 || ($scope.TCTD.TotalHours.value > 8 && $scope.TCTD.TotalHours.value < 10))
        {
          warningmessage = "If this is a normally scheduled work day, you have not entered enough hours. You must have 8 total hours or 10 total hours, depending on your work shift. ";
          $scope.warningList.push(warningmessage);
        }        
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
          if ($scope.timecard.exemptStatus !== "Exempt")
          {
            $scope.warningList.push(
              "Too many Non-Working hours entered. You should reduce your Non-Working hours until you have a total of " +
              $scope.shiftMaxHours +
              " hours."
            );
          }
          else
          {
            $scope.warningList.push(
              "Please check your leave hours entered. You will only need a maximum of 8 hours or 10 Total hours, depending on what shift you are working, when you are including leave."
            );
          }
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
      $scope.NormallyScheduled = null;
      $scope.ExpandDisasterHours = false;
      $scope.warningList = [];
      $scope.errorList = [];
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
      console.log("saving tctd", $scope.TCTD);

      var basetctd = addtimeFunctions.getBaseTCTD($scope.TCTD, $scope.timecard);
      //console.log('basetctd after save', basetctd);
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
      $scope.ExpandDisasterHours = !$scope.ExpandDisasterHours;
    };

    $scope.Toggle_OnCallHours = function ()
    {
      $scope.toggleOnCall = !$scope.toggleOnCall;
    };

    //$scope.checkDisasterWorkType = function ()
    //{
    //  // this function is run after a disaster work type is selected.
    //  // the comment is only required if they chose "Not Listed" as the option.
    //  checkForErrors();
    //  if ($scope.errorList.length === 0)
    //  {
    //    $scope.saveTCTD();
    //  }
    //};

    $scope.calculateTotalHours = function ()
    {
      if ($scope.showDisaster &&
        !$scope.ExpandDisasterHours &&
        $scope.TCTD.WorkHours.value > 0 &&
        $scope.DisasterHoursRelated)
      {
        $scope.ExpandDisasterHours = true;
      }
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
