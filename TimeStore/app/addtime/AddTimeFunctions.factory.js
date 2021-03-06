﻿/* global moment, _ */
(function ()
{
  "use strict";
  angular.module("timestoreApp").factory("addtimeFunctions", [
    "$cacheFactory",
    "timestoredata",
    function ($cacheFactory, timestoredata)
    {
      return {
        loadSavedTCTD: loadSavedTCTD,
        resetTCTD: resetTCTD,
        getTimeList: getTimeList,
        getNormallyScheduledHours: getNormallyScheduledHours,
        getShortTimeList: getShortTimeList,
        calculateTotalHours: calculateTotalHours,
        getHourTypesList: getHourTypesList,
        getBaseTCTD: getBaseTCTD,
        getMaxShiftHours: getMaxShiftHours,
        getBankedHoursUsed: getBankedHoursUsed,
        calculateWithinFirstNinetyDays: calculateWithinFirstNinetyDays
      };

      function getBankedHoursUsed(tc, hourType, workDate)
      {
        if (workDate === undefined)
        {
          switch (hourType)
          {
            case "V":
              return _.sum(tc.RawTCTD, function (n)
              {
                return n.VacationHours;
              });
            case "S":
              return _.sum(tc.RawTCTD, function (n)
              {
                return n.SickHours;
              });
            case "C":
              return _.sum(tc.RawTCTD, function (n)
              {
                return n.CompTimeUsedHours;
              });
          }
        } else
        {
          switch (hourType)
          {
            case "V":
              return _.sum(tc.RawTCTD, function (n)
              {
                return workDate !== moment(n.WorkDate).format("M/D/YYYY")
                  ? n.VacationHours
                  : 0;
              });
            case "S":
              return _.sum(tc.RawTCTD, function (n)
              {
                return workDate !== moment(n.WorkDate).format("M/D/YYYY")
                  ? n.SickHours
                  : 0;
                //return n.SickHours;
              });
            case "C":
              return _.sum(tc.RawTCTD, function (n)
              {
                return workDate !== moment(n.WorkDate).format("M/D/YYYY")
                  ? n.CompTimeUsed
                  : 0;
                //return n.CompTimeUsedHours;
              });
          }
        }
      }

      function getMaxShiftHours()
      {
        return getCacheValue("shiftMax");
      }

      function loadSavedTCTD(tc, workDate)
      {
        var tctd = resetTCTD(tc, workDate);
        var foundRawTCTD = _.find(tc.RawTCTD, function (rawtctd)
        {
          return moment(rawtctd.WorkDate).format("M/D/YYYY") === workDate;
        });
        if (foundRawTCTD !== undefined)
        {
          processRawTCTD(foundRawTCTD, tctd);
          if (tc.exemptStatus !== "Exempt")
          {
            handleBreakCredit(tctd);
          }
        }
        LoadLunchTime(tctd);
        console.log('loadSavedTCTD tctd', tctd);
        return tctd;
      }

      function LoadLunchTime(tctd)
      {
        //console.log("LoadLunchTime");
        tctd.SelectedLunchTime = null;
        tctd.LastSelectedLunchTime = null;
        if (
          tctd.selectedTimes !== undefined &&
          tctd.selectedTimes.length >= 3 &&
          tctd.selectedTimes.length <= 4
        )
        {
          // if there are atleast 3 elements, we should check to see if the last two are only 2 apart.
          // if there are more than 4 elements, we want to just quit and leave it blank
          // that means that their time is more than just "clock in, go to lunch, come back from lunch, and clock out"
          var pubWorksDepartments = ["3701", "3711", "3712"];
          var lunchDuration = pubWorksDepartments.indexOf(tctd.DepartmentNumber) !== -1 ? 2 : 4;
          //var lunchDuration = tctd.DepartmentNumber === "3701" ? 2 : 4;
          if (tctd.selectedTimes[2] - tctd.selectedTimes[1] === lunchDuration)
          {
            tctd.SelectedLunchTime = tctd.selectedTimes[1];
            tctd.LastSelectedLunchTime = tctd.selectedTimes[1];
          }
        }
      }

      function processRawTCTD(rawtctd, tctd)
      {
        console.log('processRawTCTD', rawtctd);
        loadRawTCTDHours(rawtctd, tctd);
        tctd.Comment = rawtctd.Comment;
        //tctd.DisasterName = rawtctd.DisasterName;
        //tctd.DisasterPeriodType = rawtctd.DisasterPeriodType;
        tctd.DisasterWorkHoursList = rawtctd.DisasterWorkHoursList;
        tctd.DisasterWorkType = rawtctd.DisasterWorkType;
        tctd.DisasterNormalScheduledHours = rawtctd.DisasterNormalScheduledHours;
        tctd.OutOfClass = rawtctd.OutOfClass;
        if (rawtctd.WorkTimes.search(/(\d+):(\d+):(00) (A|P)/g) !== -1)
        {
          rawtctd.WorkTimes = rawtctd.WorkTimes
            .replace(/:00 A/gi, " A")
            .replace(/:00 P/gi, " P");
        }
        processTimecardWorkTimes(
          tctd,
          rawtctd.WorkTimes,
          rawtctd.OnCallWorkTimes
        );
      }

      function loadRawTCTDHours(rawtctd, tctd)
      {
        var hours = [
          "WorkHours",
          "DisasterWorkHours",
          "BreakCreditHours",
          "HolidayHours",
          "VacationHours",
          "SickHours",
          "SickLeavePoolHours",
          "SickFamilyLeave",
          "CompTimeUsed",
          "AdminHours",
          "AdminBereavement",
          "AdminDisaster",
          "AdminWorkersComp",
          "AdminJuryDuty",
          "AdminMilitaryLeave",
          "AdminOther",
          "LWOPHours",
          "TermHours",
          "ScheduledLWOPHours",
          "LWOPSuspensionHours",
          "DoubleTimeHours",
          "TotalHours",
          "OnCallMinimumHours",
          "OnCallWorkHours",
          "OnCallTotalHours"
        ];
        _.forEach(hours, function (t)
        {
          tctd[t].value = rawtctd[t];
          if (tctd[t].value > 0 && !tctd[t].visible)
          {
            tctd[t].visible = true;
          }
        });
        tctd.Vehicle = rawtctd.Vehicle;
        tctd.showAdminHours = checkAdmin(rawtctd) > 0;
      }

      function checkAdmin(rawtctd)
      {
        
        let newAdminDisaster = rawtctd.DisasterWorkHoursList.reduce(function (j, current) { return { DisasterAdminHours: j.DisasterAdminHours + current.DisasterAdminHours }; }, 0);
        let adminDisasterValue = newAdminDisaster.DisasterAdminHours;
        return (
          rawtctd.AdminBereavement +
          rawtctd.AdminDisaster +
          rawtctd.AdminHours +
          rawtctd.AdminJuryDuty +
          rawtctd.AdminMilitaryLeave +
          rawtctd.AdminOther +
          rawtctd.AdminWorkersComp + 
          adminDisasterValue
        );
      }

      function processTimecardWorkTimes(tctd, workTime, OnCallWorkTime)
      {
        var sT = [];
        var t = [];
        var tl = getTimeList();
        var ii = 0;
        if (workTime.length === 0)
        {
          tctd.WorkTimes = "";
          tctd.selectedTimes = [];
          tctd.selectedTimesDisplay = "";
        } else
        {
          t = workTime.split("-");
          sT = [];
          for (var i = 0; i < t.length; i++)
          {
            sT.push(tl.indexOf(t[i].trim()));
          }
          tctd.WorkTimes = sT.join(" ");
          tctd.selectedTimes = sT;
          tctd.selectedTimesDisplay = workTime;
        }

        if (OnCallWorkTime.length === 0)
        {
          tctd.OnCallWorkTimes = "";
          tctd.OnCallSelectedTimes = [];
          tctd.OnCallSelectedTimesDisplay = "";
        } else
        {
          t = OnCallWorkTime.split("-");
          sT = [];
          for (ii = 0; ii < t.length; ii++)
          {
            sT.push(tl.indexOf(t[ii].trim()));
          }
          tctd.OnCallWorkTimes = sT.join(" ");
          tctd.OnCallSelectedTimes = sT;
          tctd.OnCallSelectedTimesDisplay = OnCallWorkTime;
        }

        console.log("fix disaster work times in processTimecardWorkTimes")
        
        for (let ee = 0; ee < tctd.DisasterWorkHoursList.length; ee++)
        {
          let dwh = tctd.DisasterWorkHoursList[ee];
          if (dwh.DisasterWorkTimes.length === 0)
          {
            dwh.DisasterSelectedTimes = [];
            dwh.DisasterSelectedTimesDisplay = "";
          }
          else
          {
            t = dwh.DisasterWorkTimes.split("-");
            sT = [];
            for (ii = 0; ii < t.length; ii++)
            {
              sT.push(tl.indexOf(t[ii].trim()));
            }
            dwh.DisasterWorkTimes = sT.join(" ");
            dwh.DisasterSelectedTimes = sT;
            dwh.DisasterSelectedTimesDisplay = dwh.DisasterWorkTimes;
          }
          // load the saved data into the EventsByWorkDate
          for (let ff = 0; ff < tctd.EventsByWorkDate.length; ff++)
          {
            let ewd = tctd.EventsByWorkDate[ff];
            if (ewd.event_id === dwh.DisasterPeriodId)
            {
              let dwh_ewd = ewd.disaster_work_hours;
              dwh_ewd.DisasterSelectedTimes = dwh.DisasterSelectedTimes;
              dwh_ewd.DisasterSelectedTimesDisplay = dwh.DisasterSelectedTimesDisplay;
              dwh_ewd.DisasterWorkHours = dwh.DisasterWorkHours;
              dwh_ewd.WorkHoursId = dwh.WorkHoursId;
              dwh_ewd.DisasterWorkType = dwh.DisasterWorkType;
              dwh_ewd.DisasterAdminHours = dwh.DisasterAdminHours;
            }
          }

        }
        // Original Disaster Work Times handling logic
        // replaced on 8/21/2020
        //if (disasterWorkTime.length === 0)
        //{
        //  tctd.disasterWorkTimes = "";
        //  tctd.disasterSelectedTimes = [];
        //  tctd.disasterSelectedTimesDisplay = "";
        //} else
        //{
        //  t = disasterWorkTime.split("-");
        //  sT = [];
        //  for (ii = 0; ii < t.length; ii++)
        //  {
        //    sT.push(tl.indexOf(t[ii].trim()));
        //  }
        //  tctd.disasterWorkTimes = sT.join(" ");
        //  tctd.disasterSelectedTimes = sT;
        //  tctd.disasterSelectedTimesDisplay = OnCallWorkTime;
        //}


      }

      function calculateWithinFirstNinetyDays(hireDate, workDate)
      {

        var wd = moment(workDate, "M/D/YYYY");
        var hd = moment(hireDate);
        return wd.diff(hd, "days") > 90;

        // this function returns true if the person's hire date is more than
        // 90 days away from the work date.
      }

      function resetTCTD(tc, workDate)
      {
        console.log("reset tctd function with workdate and timecard", workDate, tc);
        // resetTCTD provides a 0'd out TCTD that conforms to the user's min/max / visibility for their classification
        var isExempt = tc.exemptStatus === "Exempt";
        populateConstants(isExempt, tc.classify);

        var HireDateCheck = calculateWithinFirstNinetyDays(
          tc.hireDate,
          workDate
        );
        let events = [];
        let disaster_work_hours_list = [];
        //var DisasterNameToUse = "";        
        var wd = moment(workDate, "M/D/YYYY");

        if (tc.Events_By_WorkDate.length > 0)
        {
          let wd_short_date = wd.format("M/D/YYYY");
          events = tc.Events_By_WorkDate.filter(
            function (j)
            {
              var test = moment(new Date(j.work_date)).format("M/D/YYYY");
              return test === wd_short_date;
            });
          for (let i = 0; i < events.length; i++)
          {
            let event = events[i];
            event.disaster_work_hours =
            {
              WorkHoursId: -1,
              DisasterPeriodId: event.event_id,
              DisasterSelectedTimes: [],
              DisasterSelectedTimesDisplay: "",
              DisasterTimesError: "",
              DisasterWorkTimes: "",
              DisasterWorkHours: 0,
              DisasterAdminHours: 0,
              DisasterWorkType: ""
            };
          }
        }

        var tctd = {
          EmployeeID: tc.employeeID,
          DepartmentNumber: tc.departmentNumber,
          WorkDate: workDate,
          WorkTimes: "",
          WorkHours: getDefaultHoursNoMax("Hours Worked"),
          DisasterWorkTimes: "",
          EventsByWorkDate: events,
          //DisasterName: DisasterNameToUse,
          DisasterNormalScheduledHours: -1,
          DisasterWorkHoursList: disaster_work_hours_list,
          //DisasterPeriodType: 0,
          DisasterWorkHours: getDefaultHoursNoMax("Disaster Hours"),
          DisasterWorkType: "",
          BreakCreditHours: getDefaultHours("Break Credit", true),
          OnCallWorkTimes: "",
          OnCallWorkHours: getDefaultHoursNoMax("On Call Hours Worked"),
          OnCallMinimumHours: getDefaultHoursNoMax("On Call Min Hours"),
          OnCallTotalHours: getDefaultHoursNoMax("Total On Call Hours"),
          OnCallSelectedTimesDisplay: "",
          OnCallSelectedTimes: [],
          OutOfClass: false,
          SelectedLunchTime: null,
          LastSelectedLunchTime: null,
          HolidayHours: getDefaultHolidayHours(tc, workDate),
          VacationHours: getDefaultHoursWithNinetyDayCheck(
            "Vacation",
            HireDateCheck
          ),
          SickHours: getDefaultHoursWithNinetyDayCheck("Sick", HireDateCheck),
          SickLeavePoolHours: getDefaultSickLeavePoolHours(
            tc,
            "Sick Leave Pool",
            HireDateCheck
          ),
          SickFamilyLeave: getDefaultHoursWithNinetyDayCheck(
            "Sick Family Leave",
            HireDateCheck
          ),
          CompTimeUsed: getDefaultCompTimeHours("Comp Time Used", !isExempt),
          CompTimeEarned: 0,
          showOnCall: getCacheValue("showOnCall"),
          showAdminHours: false,
          AdminHours: getDefaultHours("Admin - Total", true), // meant to be a total for the other Admin hours.
          AdminOther: getDefaultHours("Other Admin"),
          AdminBereavement: getDefaultHours("Bereavement"),
          AdminDisaster: getDefaultHours("Disaster"),
          AdminJuryDuty: getDefaultHours("Jury Duty"),
          AdminMilitaryLeave: getDefaultHours("Military Leave"),
          AdminWorkersComp: getDefaultHours("Worker's Comp"),
          TermHours: getDefaultHours("Term Hours"),
          LWOPHours: getDefaultHours("Leave Without Pay"),
          ScheduledLWOPHours: getDefaultHours("Scheduled LWOP"),
          LWOPSuspensionHours: getDefaultHours("Suspension - LWOP"),
          DoubleTimeHours: getDefaultDoubleTimeHours(tc, workDate),
          TotalHours: getDefaultHoursNoMax("Total Hours"),
          Comment: "",
          showVehicle: getCacheValue("showVehicle"),
          Vehicle: 0,
          errorText: "",
          selectedTimesDisplay: "",
          selectedTimes: [],
          disasterSelectedTimesDisplay: "",
          disasterSelectedTimes: []
        };
        console.log('fully reset tctd', tctd);
        return tctd;
      }

      function getBaseTCTD(tctd, tc)
      {
        console.log("getbasetctd", tctd, tc);
        return {
          EmployeeID: tc.employeeID,
          DepartmentNumber: tc.departmentNumber,
          WorkDate: tctd.WorkDate,
          WorkTimes: tctd.selectedTimesDisplay,
          WorkHours: getValue(tctd.WorkHours.value),
          DisasterWorkTimes: tctd.disasterSelectedTimesDisplay,
          DisasterWorkHours: getValue(tctd.DisasterWorkHours.value),
          DisasterWorkType: tctd.DisasterWorkType,
          EventsByWorkDate: tctd.EventsByWorkDate,
          DisasterWorkHoursList: tctd.DisasterWorkHoursList,
          //DisasterName: "",
          DisasterNormalScheduledHours: tctd.DisasterNormalScheduledHours,
          BreakCreditHours: getValue(tctd.BreakCreditHours.value),
          HolidayHours: getValue(tctd.HolidayHours.value),
          VacationHours: getValue(tctd.VacationHours.value),
          SickHours: getValue(tctd.SickHours.value),
          SickLeavePoolHours: getValue(tctd.SickLeavePoolHours.value),
          SickFamilyLeave: getValue(tctd.SickFamilyLeave.value),
          CompTimeUsed: getValue(tctd.CompTimeUsed.value),
          CompTimeEarned: 0,
          showOnCall: getCacheValue("showOnCall"),
          showAdminHours: false,
          AdminHours: getValue(tctd.AdminHours.value),
          AdminBereavement: getValue(tctd.AdminBereavement.value),
          AdminDisaster: getValue(tctd.AdminDisaster.value),
          AdminWorkersComp: getValue(tctd.AdminWorkersComp.value),
          AdminJuryDuty: getValue(tctd.AdminJuryDuty.value),
          AdminMilitaryLeave: getValue(tctd.AdminMilitaryLeave.value),
          AdminOther: getValue(tctd.AdminOther.value),
          TermHours: getValue(tctd.TermHours.value),
          LWOPHours: getValue(tctd.LWOPHours.value),
          ScheduledLWOPHours: getValue(tctd.ScheduledLWOPHours.value),
          LWOPSuspensionHours: getValue(tctd.LWOPSuspensionHours.value),
          DoubleTimeHours: getValue(tctd.DoubleTimeHours.value),
          TotalHours: getValue(tctd.TotalHours.value),
          PPD: tc.payPeriodStart,
          Comment: tctd.Comment,
          Vehicle: tctd.Vehicle,
          OnCallMinimumHours: getValue(tctd.OnCallMinimumHours.value),
          OnCallWorkHours: getValue(tctd.OnCallWorkHours.value),
          OnCallWorkTimes: tctd.OnCallSelectedTimesDisplay,
          OnCallTotalHours: getValue(tctd.OnCallTotalHours.value),
          OutOfClass: tctd.OutOfClass,
          SelectedLunchTime: null,
          LastSelectedLunchTime: null,
          TerminationDate: null
        };
      }

      function calculateTotalHours(tctd, isExempt)
      {
        calcWorkHours(tctd, isExempt);
        calcOnCallHours(tctd);
        calcDisasterWorkHours(tctd);
        var th = 0;
        th += getValue(tctd.AdminBereavement.value);
        th += getValue(tctd.AdminDisaster.value);
        for (let i = 0; i < tctd.EventsByWorkDate.length; i++)
        {
          th += getValue(tctd.EventsByWorkDate[i].disaster_work_hours.DisasterAdminHours);
        }
        th += getValue(tctd.AdminJuryDuty.value);
        th += getValue(tctd.AdminMilitaryLeave.value);
        th += getValue(tctd.AdminWorkersComp.value);
        th += getValue(tctd.AdminOther.value);
        tctd.AdminHours.value = th;
        th = tctd.WorkHours.value + tctd.BreakCreditHours.value; // reset it here now that we've calc'd the admin hours.
        th += getValue(tctd.HolidayHours.value);
        th += getValue(tctd.VacationHours.value);
        th += getValue(tctd.SickHours.value);
        th += getValue(tctd.SickLeavePoolHours.value);
        th += getValue(tctd.SickFamilyLeave.value);
        th += getValue(tctd.CompTimeUsed.value);
        th += getValue(tctd.AdminHours.value);
        th += getValue(tctd.TermHours.value);
        th += getValue(tctd.LWOPHours.value);
        th += getValue(tctd.ScheduledLWOPHours.value);
        th += getValue(tctd.LWOPSuspensionHours.value);
        //th += getValue(tctd.DoubleTimeHours.value);
        th += getValue(tctd.OnCallTotalHours.value);
        tctd.TotalHours.value = th;
      }

      function getDefaultHoursWithNinetyDayCheck(label, HireDateCheck)
      {
        return getHours(label, 0, HireDateCheck, !HireDateCheck);
      }

      function getDefaultHours(label, disabled)
      {
        return getHours(label, 0, true, disabled);
      }

      function getDefaultSickLeavePoolHours(tc, label, HireDateCheck)
      {
        var v = false;
        if (
          tc.bankedVacation - getBankedHoursUsed(tc, "V") < 0.25 &&
          tc.bankedSick - getBankedHoursUsed(tc, "S") < 0.25 &&
          tc.bankedComp - getBankedHoursUsed(tc, "C") < 0.25
        )
        {
          v = true;
        }
        if (v && !HireDateCheck)
        {
          v = false;
        }
        return getHours(label, 0, v, false);
      }

      function getDefaultCompTimeHours(label, visible)
      {
        return getHours(label, 0, visible, false);
      }

      function getDefaultWorkHoursNoMax(label, visible)
      {
        var h = getHours(label, 0, visible, true);
        h.max = 24;
        return h;
      }

      function getDefaultHoursNoMax(label)
      {
        var h = getHours(label, 0, true, true);
        h.max = 24;
        return h;
      }

      function getHours(label, value, visible, disabled)
      {
        return {
          label: label,
          type: "number",
          min: getCacheValue("shiftMin"),
          max: getCacheValue("shiftMax"),
          step: getCacheValue("shiftStep"),
          value: value,
          visible: visible,
          disabled: disabled === undefined ? false : disabled
        };
      }

      function getDefaultHolidayHours(tc, workDate)
      {
        var label = "Holiday";
        return getHours(label, 0, showHoliday(tc, workDate), false);
      }

      function getDefaultDoubleTimeHours(tc, workDate)
      {
        var dThours = getHours(
          "Double Time",
          0,
          showDoubleTime(tc, workDate),
          false
        );
        dThours.max = 24;
        dThours.min = 0;
        return dThours;
      }

      function getShortTimeList()
      {
        var timeList = [];
        var m = moment([2015, 0, 1, 6, 0, 0]); // here we just pick a date that has no time, so it should start at 12:00 AM.
        timeList.push(m.format("h:mm A")); // we output 12:00 AM
        for (var i = 0; i < 47; i++)
        {
          //71
          timeList.push(m.add(15, "m").format("h:mm A")); // now we loop through the time, outputting every 15 mins.
        }
        timeList.push("6:00 PM");
        return timeList;
      }

      function getTimeList()
      {
        var timeList = [];
        var m = moment([2015, 0, 1]); // here we just pick a date that has no time, so it should start at 12:00 AM.
        timeList.push(m.format("h:mm A")); // we output 12:00 AM
        for (var i = 0; i < 95; i++)
        {
          timeList.push(m.add(15, "m").format("h:mm A")); // now we loop through the time, outputting every 15 mins.
        }
        timeList.push("11:59:59 PM");
        return timeList;
      }

      function getNormallyScheduledHours()
      {
        var hours = [];
        for (var i = 4; i < 12.25; i += .25)
        {
          hours.push(i);
        }
        return hours;
      }

      function getValue(p)
      {
        p = p === null || p === undefined ? 0 : p;
        return p !== "" ? parseFloat(p) : 0;
      }

      function showHoliday(tc, workDate)
      {
        // This is calculated for each day. It has to appear in the HolidaysInPPD array in the Timecard
        // in order for it to be a holiday.
        // for department 2801, Animal Control, we want to allow them to enter a holiday in
        // anytime during the week of the holiday.
        if (tc.departmentNumber === "2801" || tc.employeeID === "2850" || tc.classify === "0840")
        {
          // We do this by looking at each date in tc.HolidaysInPPD and calculate the week for each one
          // using calculateWeek(payperiodstart, holiday).
          // Then we compare the calculateWeek(payperiodstart, workdate) for workdate and see if it is in
          // one of the values returned for the holidays.
          var pps = moment(tc.payPeriodStart); // start of PPD and week 1
          var wd = moment(workDate, "M/D/YYYY");
          var wdWeek = calculateWeek(pps, wd);
          var hWeeks = [];
          _.forEach(tc.HolidaysInPPD, function (n)
          {
            var h = moment(n, "M/D/YYYY");
            hWeeks.push(calculateWeek(pps, h));
          });
          return hWeeks.indexOf(wdWeek) > -1;
        } else
        {
          if (tc.HolidaysInPPD.indexOf(workDate) > -1)
          {
            return true;
          } else
          {
            return false;
          }
        }
      }

      function calculateWeek(pps, d)
      {
        // assumes pps and d are both moment objects already
        var w2 = pps.clone().add(6, "days");
        return d.isAfter(w2) ? 2 : 1;
      }

      function showDoubleTime(tc, workDate)
      {
        var m = moment(workDate, "M/D/YYYY");
        if (tc.exemptStatus !== "Exempt")
        {
          if (m.day() === 0)
          {
            return true;
          } else
          {
            return showHoliday(tc, workDate);
          }
        }
      }

      function populateConstants(isExempt, classify)
      {
        var cache = $cacheFactory.get("addTimeConstants") === undefined
          ? $cacheFactory("addTimeConstants")
          : $cacheFactory.get("addTimeConstants");

        cache.put(
          "showVehicle",
          timestoredata.classVehicle().indexOf(classify) > -1
        );
        cache.put(
          "callMin",
          _.result(
            _.find(timestoredata.classOnCall(), { class: classify }),
            "callMin",
            0
          )
        );
        cache.put("showOnCall", cache.get("callMin") > 0);
        cache.put("shiftMin", 0);
        cache.put("shiftStep", 0.25);
        cache.put("shiftMax", 10); // 10 hour shift  change from 8 hours
        var s = _.result(
          _.find(timestoredata.classShiftTen(), { class: classify }),
          "shiftLength"
        );
        if (s !== undefined)
        {
          cache.put("shiftMax", s);
        }
      }

      function getCacheValue(key)
      {
        var cache = $cacheFactory.get("addTimeConstants");
        return cache.get(key);
      }

      function calcOnCallHours(tctd)
      {
        var times = tctd.OnCallSelectedTimes;
        if (times === undefined)
        {
          return;
        }
        tctd.OnCallSelectedTimesDisplay = "";
        tctd.OnCallWorkHours.value = "";
        tctd.OnCallWorkTimes = "";
        tctd.OnCallMinimumHours.value = "";
        tctd.OnCallTotalHours.value = "";
        var callMin = getCacheValue("callMin");
        var tl = getTimeList();
        if (times.length === 0)
        {
          return;
        }
        var timeDisplay = "";
        var workHours = 0;
        var minHours = 0;
        var minHoursLen = callMin * 4;
        var minHoursStart = 0;
        var minHoursEnd = callMin * 4;
        var st = times.sort(function (a, b)
        {
          return a - b;
        });
        var length = st.length - st.length % 2;
        if (length > 1)
        {
          minHoursStart = st[0];
          minHoursEnd = minHoursStart + callMin * 4;
          for (var i = 0; i < length; i += 2)
          {
            if (st[i] > minHoursEnd)
            {
              minHoursStart = st[i];
              minHoursEnd = minHoursStart + callMin * 4;
              minHours += Math.max(minHoursLen / 4, 0);
              minHoursLen = callMin * 4;
            }
            workHours += (st[i + 1] - st[i]) / 4;
            if (timeDisplay.length > 0)
            {
              timeDisplay += " - ";
            }
            timeDisplay += tl[st[i]] + " - " + tl[st[i + 1]];
            minHoursLen -= st[i + 1] - st[i];
          }
          minHours += Math.max(minHoursLen / 4, 0);
        }
        if (st.length % 2 === 1)
        {
          if (timeDisplay.length > 0)
          {
            timeDisplay += " - ";
          }
          timeDisplay += tl[st[st.length - 1]];
        }
        tctd.OnCallWorkTimes = times.join(" ");
        tctd.OnCallSelectedTimesDisplay = timeDisplay;
        tctd.OnCallWorkHours.value = workHours;
        tctd.OnCallMinimumHours.value = minHours;
        tctd.OnCallTotalHours.value =
          tctd.OnCallWorkHours.value + tctd.OnCallMinimumHours.value;
        //calculateTotalHours();
      }

      function handleLunchTime(tctd)
      {
        if (tctd.selectedTimes.length > 4 ||
          (tctd.LastSelectedLunchTime && tctd.selectedTimes.indexOf(parseInt(tctd.LastSelectedLunchTime)) === -1))
        {
          tctd.SelectedLunchTime = null;
          tctd.LastSelectedLunchTime = null;
          return;
        }
        if (tctd.SelectedLunchTime)
        {
          var pubWorksDepartments = ["3701", "3711", "3712"];
          var lunchDuration = pubWorksDepartments.indexOf(tctd.DepartmentNumber) !== -1 ? 2 : 4;
          //var lunchDuration = tctd.DepartmentNumber === "3701" ? 2 : 4;

          if (parseInt(tctd.SelectedLunchTime) === -1)
          {
            // They selected "No lunch taken"
            // we need to look for a lunch and remove it
            if (
              tctd.LastSelectedLunchTime &&
              parseInt(tctd.LastSelectedLunchTime) !== -1
            )
            {
              removeOldLunchTime(tctd, lunchDuration);
            }
            tctd.SelectedLunchTime = null;
            tctd.LastSelectedLunchTime = null;
          } else
          {
            // A couple of things to keep in mind here
            // First, we look at the last selected lunch time to remove the old time
            // if they select a lunch and then change their mind and choose another.
            // ie: They pick 3:00 PM, it auto saves, then they pick 3:30 PM
            // we use the LastSelectedLunchTime field to remove the 3:00 PM times
            // then add the 3:30 PM times
            // We should probably only do that if the LastSelectedLunchtime
            // and the SelectedLunchtime is different.

            if (tctd.selectedTimes.length > 2 && parseInt(tctd.selectedTimes[2]) - parseInt(tctd.selectedTimes[1]) !== lunchDuration)
            {
              tctd.SelectedLunchTime = null;
              tctd.LastSelectedLunchTime = null;
            } else
            {
              if (
                tctd.LastSelectedLunchTime &&
                tctd.LastSelectedLunchTime !== -1
              )
              {
                removeOldLunchTime(tctd, lunchDuration);
              }
              if (tctd.selectedTimes.indexOf(parseInt(tctd.SelectedLunchTime)) === -1)
              {
                tctd.selectedTimes.push(parseInt(tctd.SelectedLunchTime));
              }
              if (tctd.selectedTimes.indexOf(parseInt(tctd.SelectedLunchTime) + lunchDuration) === -1)
              {
                tctd.selectedTimes.push(parseInt(tctd.SelectedLunchTime) + lunchDuration);
              }
              tctd.LastSelectedLunchTime = tctd.SelectedLunchTime;
              tctd.selectedTimes.sort(function (a, b)
              {
                return a - b;
              });
            }
          }
        }
      }

      function removeOldLunchTime(tctd, lunchDuration)
      {
        var last = tctd.selectedTimes.indexOf(tctd.LastSelectedLunchTime);
        if (last > -1)
        {
          tctd.selectedTimes.splice(last, 1); // remove the original lunch start time
          last = tctd.selectedTimes.indexOf(
            tctd.LastSelectedLunchTime + lunchDuration
          );
          if (last > -1)
          {
            // remove the end time
            tctd.selectedTimes.splice(last, 1);
          }
        }
      }

      function calcWorkHours(tctd, isExempt)
      {
        // here is where we're going to calculate the total work hours
        // and the break credit, if they get one.

        if (tctd.selectedTimes === undefined)
        {
          return;
        }
        // let's handle the SelectedLunchTime and LastSelectedLunchTime
        handleLunchTime(tctd);
        var times = tctd.selectedTimes;
        tctd.selectedTimesDisplay = "";
        tctd.WorkHours.value = 0;
        tctd.WorkTimes = "";
        tctd.BreakCreditHours.value = 0;
        var timeList = getTimeList();
        if (times.length === 0)
        {
          return;
        }
        var timeDisplay = "";
        var workHours = 0;
        var st = times.sort(function (a, b)
        {
          return a - b;
        });
        var length = st.length - st.length % 2;
        if (length > 1)
        {
          for (var i = 0; i < length; i += 2)
          {
            workHours += (st[i + 1] - st[i]) / 4;
            if (timeDisplay.length > 0)
            {
              timeDisplay += " - ";
            }
            timeDisplay += timeList[st[i]] + " - " + timeList[st[i + 1]];
          }
        }
        if (st.length % 2 === 1)
        {
          if (timeDisplay.length > 0)
          {
            timeDisplay += " - ";
          }
          timeDisplay += timeList[st[st.length - 1]];
        }
        tctd.WorkTimes = times.join(" ");
        tctd.selectedTimesDisplay = timeDisplay;
        tctd.WorkHours.value = workHours;

        // Let's handle the break credit here:
        if (!isExempt)
        {
          handleBreakCredit(tctd);
          //if (st.length === 4 && workHours >= 7.5) {
          //    if (st[2] - st[1] === 4) {
          //        tctd.BreakCreditHours.value = 0.5;
          //    }
          //}
        }
      }

      function calcDisasterWorkHours(tctd)
      {
        // Replaced on 8/20/2020
        //tctd.DisasterWorkHours.value = 0;
        //tctd.disasterSelectedTimesDisplay = "";
        //tctd.DisasterWorkTimes = "";        
        //if (tctd.disasterSelectedTimes === undefined)
        //{
        //  return;
        //}
        //var times = tctd.disasterSelectedTimes;
        //tctd.disasterSelectedTimesDisplay = "";

        //var timeList = getTimeList();
        //if (times.length === 0)
        //{
        //  return;
        //}
        //var timeDisplay = "";
        //var workHours = 0;
        //var st = times.sort(function (a, b)
        //{
        //  return a - b;
        //});
        //var length = st.length - st.length % 2;
        //if (length > 1)
        //{
        //  for (var i = 0; i < length; i += 2)
        //  {
        //    workHours += (st[i + 1] - st[i]) / 4;
        //    if (timeDisplay.length > 0)
        //    {
        //      timeDisplay += " - ";
        //    }
        //    timeDisplay += timeList[st[i]] + " - " + timeList[st[i + 1]];
        //  }
        //}
        //if (st.length % 2 === 1)
        //{
        //  if (timeDisplay.length > 0)
        //  {
        //    timeDisplay += " - ";
        //  }
        //  timeDisplay += timeList[st[st.length - 1]];
        //}
        //tctd.DisasterWorkTimes = times.join(" ");
        //tctd.DisasterWorkHours.value = workHours;
        //tctd.disasterSelectedTimesDisplay = timeDisplay;
        for (let ewd = 0; ewd < tctd.EventsByWorkDate.length; ewd++)
        {
          let dwh = tctd.EventsByWorkDate[ewd].disaster_work_hours;
          dwh.DisasterWorkHours = 0;
          dwh.DisasterSelectedTimesDisplay = "";
          dwh.DisasterWorkTimes = "";
          let timeList = getTimeList();
          let times = dwh.DisasterSelectedTimes;

          if (dwh.DisasterSelectedTimes !== undefined && timeList.length > 0 && times.length > 0)
          {
            let display = "";
            let workhours = 0;

            // we sort the times now because our calculation depends on them being in 
            // order from 0 to 79
            let sorted_times = times.sort(function (a, b) { return a - b; });

            let length = sorted_times.length - sorted_times.length % 2;
            if (length > 1)
            {
              for (let i = 0; i < length; i += 2)
              {
                workhours += (sorted_times[i + 1] - sorted_times[i]) / 4;
                if (display.length > 0)
                {
                  display += " - ";
                }
                display += timeList[sorted_times[i]] + " - " + timeList[sorted_times[i + 1]];
              }
            }
            if (sorted_times.length % 2 === 1)
            {
              if (display.length > 0)
              {
                display += " - ";
              }
              display += timeList[sorted_times[sorted_times.length - 1]];
            }
            dwh.DisasterWorkTimes = display;//times.join(" ");
            dwh.DisasterWorkHours = workhours;
            dwh.DisasterSelectedTimesDisplay = display;
          }
        }

        // now we'll copy those to the tctd.DisasterWorkHoursList array
        tctd.DisasterWorkHoursList.splice(0,tctd.DisasterWorkHoursList.length); // empty the array
        for (let ee = 0; ee < tctd.EventsByWorkDate.length; ee++)
        {
          let ewd = tctd.EventsByWorkDate[ee];
          let dwh = ewd.disaster_work_hours;
          if (dwh.DisasterWorkHours > 0 || dwh.DisasterAdminHours > 0)
          {
            let new_dwh = {
              WorkHoursId: dwh.WorkHoursId,
              DisasterPeriodId: dwh.DisasterPeriodId,
              DisasterWorkHours: dwh.DisasterWorkHours,
              DisasterWorkType: dwh.DisasterWorkType,
              DisasterWorkTimes: dwh.DisasterWorkTimes ? dwh.DisasterWorkTimes : "",
              DisasterAdminHours: dwh.DisasterAdminHours
            };
            tctd.DisasterWorkHoursList.push(new_dwh);
          }
        }

      }

      function handleBreakCredit(tctd)
      {
        var st = tctd.selectedTimes.sort(function (a, b)
        {
          return a - b;
        });

        // mirroring the break credit rules from Timecard
        // to get the break credit in the original timecard they need:
        //
        // 7.5 or more hours worked
        // to be clocked out atleast twice (atleast 2 in and out times)
        // for one of their breaks to be 1 hour
        if (tctd.WorkHours.value < 7.5 || st.length % 2 > 1)
        {
          // if they don't have atleast 7.5 hours and an even number of in / out times, they get no break credit.
          tctd.BreakCreditHours.value = 0;
          return;
        }
        for (var i = 0; i < st.length; i += 2)
        {
          if (st[i + 2] - st[i + 1] === 4)
          {
            tctd.BreakCreditHours.value = 0.5;
          }
        }

        // assuming strict break credit rules
        if (st.length === 4 && tctd.WorkHours.value >= 7.5)
        {
          if (st[2] - st[1] === 4)
          {
            tctd.BreakCreditHours.value = 0.5;
          }
        }
      }

      function getHourTypesList()
      {
        var hourTypes = [];
        hourTypes.push("BreakCreditHours");
        hourTypes.push("HolidayHours");
        hourTypes.push("VacationHours");
        hourTypes.push("SickHours");
        hourTypes.push("SickLeavePoolHours");
        hourTypes.push("CompTimeUsed");
        hourTypes.push("AdminOther");
        hourTypes.push("AdminBereavement");
        hourTypes.push("AdminDisaster");
        hourTypes.push("AdminJuryDuty");
        hourTypes.push("AdminMilitaryLeave");
        hourTypes.push("AdminWorkersComp");
        hourTypes.push("TermHours");
        hourTypes.push("LWOPHours");
        hourTypes.push("ScheduledLWOPHours");
        hourTypes.push("LWOPSuspensionHours");
        hourTypes.push("DoubleTimeHours");
        return hourTypes;
      }
    }
  ]);
})();
