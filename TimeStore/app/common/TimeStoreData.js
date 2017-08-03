(function ()
{
  "use strict";
  var timestoreApp = angular.module("timestoreApp");

  // Factories to load from Webservices
  timestoreApp.factory("timestoredata", [
    "$cookies",
    "$q",
    "$http",
    function ($cookies, $q, $http)
    {
      function getBirthdays()
      {
        return $http
          .post("/TimeStore/TC/GetBirthdays", {}, { cache: false })
          .then(function (response)
          {
            return response.data;
          });
      }

      function getHolidays()
      {
        return $http
          .post("/TimeStore/TC/GetHolidays", {}, { cache: false })
          .then(function (response)
          {
            return response.data;
          });
      }

      function getDeptLeaveRequests()
      {
        return $http
          .post(
          "/TimeStore/TC/Leave_Requests_By_Department",
          {},
          { cache: false }
          )
          .then(function (response)
          {
            return response.data;
          });
      }

      var finalizeLeaveRequest = function (
        eId,
        approved,
        approval_hours_id,
        note,
        hours_approved,
        workdate
      )
      {
        var lr = {
          employeeId: eId,
          approved: approved,
          hours: hours_approved,
          id: approval_hours_id,
          note: note,
          workdate: workdate
        };
        return $http
          .post("/TimeStore/TC/Update_Leave_Request", lr, {
            cache: false,
            handleError: true
          })
          .then(function (response)
          {
            return response;
          });
      };

      var getLeaveRequestsByEmployee = function (eId)
      {
        return $http
          .post(
          "/TimeStore/TC/Leave_Requests_By_Employee",
          { employeeId: eId },
          { cache: false }
          )
          .then(function (response)
          {
            return response.data;
          });
      };

      var getAllLeaveRequestsByEmployee = function (eId)
      {
        return $http
          .post(
          "/TimeStore/TC/All_Leave_Requests_By_Employee",
          { employeeId: eId },
          { cache: false }
          )
          .then(function (response)
          {
            return response.data;
          });
      };

      var getLeaveRequests = function ()
      {
        return $http
          .post("/TimeStore/TC/Leave_Requests", { cache: false })
          .then(function (response)
          {
            return response.data;
          });
      };

      var saveCompTimeEarned = function (eId, Week1, Week2, PPE)
      {
        return $http
          .post(
          "/TimeStore/TC/SaveCompTimeChoices",
          {
            EmployeeID: eId,
            Week1: Week1,
            Week2: Week2,
            PayPeriodEnding: PPE
          },
          { cache: false }
          )
          .then(function (response)
          {
            return response;
          });
      };

      function getPayPeriodIndex(d, current)
      {
        if (current === undefined || current === null)
        {
          current = moment(getPayPeriodStart(null, true), "YYYYMMDD");
        }
        return Math.floor(d.diff(current, "days") / 14);
      }

      function getDefaultEmployeeId()
      {
        var deferred = $q.defer();
        var eid = $cookies.get("employeeid");
        if (eid === undefined)
        {
          deferred.resolve(
            getMyAccess().then(function (data)
            {
              return data.EmployeeID;
            })
          );
        } else
        {
          deferred.resolve(eid);
        }
        return deferred.promise;
      }

      function getPayPeriodStart(d, real)
      {
        // This function finds the number of days since 9/25/2013
        // and mods that number by 14, which will basically give us the
        // number of days since the pay period start.
        // then we add 13 to that number and return the end of the current pay period.
        var pps = moment("9/25/2013", "M/D/YYYY").startOf("day");
        real = real === undefined || real === null ? false : real;
        var end;
        if (d === undefined || d === null)
        {
          end = moment().startOf("day");
        } else
        {
          end = moment(d, "M/D/YYYY");
        }
        var payperiodstart = end.subtract(end.diff(pps, "days") % 14, "days");
        var today = moment().startOf("day");
        if (payperiodstart.isSame(today) && moment().hour() < 11 && !real)
        {
          var s = payperiodstart.add(-14, "days").format("YYYYMMDD");
          return s;
        } else
        {
          return payperiodstart.format("YYYYMMDD");
        }
        //payperiodstart = end.subtract((end.diff(pps, 'days') % 14), 'days').format('YYYYMMDD');
      }

      function getPayPeriodEnd(d)
      {
        // This function finds the number of days since 9/25/2013
        // and mods that number by 14, which will basically give us the
        // number of days since the pay period start.
        // then we add 13 to that number and return the end of the current pay period.
        //var pps = moment('9/25/2013', 'M/D/YYYY');
        //var end;
        //if (d === undefined || d === null) {
        //    end = moment().startOf('day');
        //} else {
        //    end = moment(d, 'M/D/YYYY');
        //}
        //return end.subtract((end.diff(pps, 'days') % 14) - 13, 'days').format('YYYYMMDD');
        //return end.add(pps.diff(end, 'days') % 14, 'days').format('YYYYMMDD');
        return moment(getPayPeriodStart(d), "YYYYMMDD")
          .add(13, "days")
          .format("YYYYMMDD");
      }

      var getGenericTimeData = function (startDate, endDate, fieldsToDisplay)
      {
        var x = {
          StartDate: startDate,
          EndDate: endDate,
          Fields: fieldsToDisplay
        };
        return $http
          .post("/TimeStore/Reports/GetGenericData", x)
          .then(function (response)
          {
            return response.data;
          });
      };

      var SaveNoteToPayPeriod = function (employeeId, note, payPeriodEnding)
      {
        var tmp = {
          EmployeeID: employeeId,
          Note: note,
          PPE: payPeriodEnding
        };
        return $http
          .post("/TimeStore/TC/SaveNoteToPayPeriod", tmp)
          .then(function (response)
          {
            return response.data;
          });
      };

      var saveTCTD = function (TCTD)
      {
        return $http
          .post("/TimeStore/TC/SaveTimecardDay", TCTD)
          .then(function (response)
          {
            return response;
          });
      };

      var classShiftTen = function ()
      {
        return [
          { class: "0051", shiftLength: 10 },
          { class: "0540", shiftLength: 10.5 },
          { class: "0810", shiftLength: 10.5 },
          { class: "1261", shiftLength: 10.5 },
          { class: "1262", shiftLength: 10.5 },
          { class: "1330", shiftLength: 10.5 },
          { class: "0840", shiftLength: 10 },
          { class: "1055", shiftLength: 10 } // added Programmer to the 10 hour shift classes.
        ];
      };

      var classVehicle = function ()
      {
        return ["1221", "0055", "0606", "0605"]; // 1055 for testing
      };

      var classOnCall = function ()
      {
        // the first parameter of each array is the classification, the second is the call_min
        return [
          { class: "0051", callMin: 2 },
          { class: "0055", callMin: 2 },
          { class: "0180", callMin: 2 },
          { class: "0242", callMin: 2 },
          { class: "0550", callMin: 2 },
          { class: "0552", callMin: 2 },
          { class: "0830", callMin: 2 },
          { class: "0910", callMin: 2 },
          { class: "0920", callMin: 2 },
          { class: "0930", callMin: 2 },
          { class: "0950", callMin: 2 },
          { class: "0960", callMin: 2 },
          { class: "1100", callMin: 2 },
          { class: "1150", callMin: 2 },
          { class: "1161", callMin: 2 },
          { class: "1373", callMin: 2 },
          { class: "1429", callMin: 2 },
          { class: "1430", callMin: 2 },
          { class: "1431", callMin: 2 },
          { class: "1435", callMin: 2 },
          { class: "1460", callMin: 2 }
        ];
      };

      var financePostProcess = function (ppdIndex)
      {
        var ppd = { ppdIndex: ppdIndex };
        console.log("post process pay period index", ppd);
        return $http
          .post("/TimeStore/Main/UploadFinanceData", ppd)
          .then(function (response)
          {
            return response.data;
          });
      };

      var saveHoliday = function (HolidayRequest)
      {
        return $http
          .post("/TimeStore/TC/SaveHolidays", HolidayRequest, { cache: false })
          .then(function (response)
          {
            return response.data;
          });
      };

      var getSignatureRequired = function (ppdIndex, eid)
      {
        console.log("signature required eid", eid);
        return getUnrestrictedInitiallyApproved(ppdIndex).then(function (data)
        {
          console.log("signature data", data);
          if (eid !== undefined)
          {
            console.log("signature data by employeeid", eid.toString());
            return _.filter(data, function (x)
            {
              return x.employeeID === eid.toString();
            });
          } else
          {
            return _.filter(data, function (x)
            {
              return (
                x.Initial_Approval_EmployeeID !== parseInt(x.employeeID) ||
                x.Final_Approval_EmployeeID === 0
              );
            });
          }
        });
        // changed on 8/14/2015 removing backend call, doing on client side.
        //return $http.get('/TimeStore/TC/SignatureRequired?ppdIndex=' + ppdIndex, { cache: false })
        //.then(function (response) {
        //    return response.data;
        //});
      };

      var getUncompletedApprovals = function (ppdIndex)
      {
        return getUnrestrictedInitiallyApproved(ppdIndex).then(function (data)
        {
          return _.filter(data, function (x)
          {
            return (
              x.Final_Approval_EmployeeID === 0 ||
              x.Initial_Approval_EmployeeID === 0
            );
          });
        });
      };

      var saveIncentives = function (incentives)
      {
        return $http
          .post("/TimeStore/TC/Incentives", incentives, { cache: false })
          .then(function (response)
          {
            return response.data;
          });
      };

      var getIncentives = function (incentiveType)
      {
        return $http
          .get("/TimeStore/TC/Incentives?incentiveType=" + incentiveType, {
            cache: false
          })
          .then(function (response)
          {
            return response.data;
          });
      };

      var getUnapproved = function (ppdIndex)
      {
        var p = { ppdIndex: ppdIndex };
        return $http
          .post("/TimeStore/TC/UnApproved", p, { cache: false })
          .then(function (response)
          {
            return response.data;
          });
      };

      var getUnrestrictedInitiallyApproved = function (ppdIndex)
      {
        var p = { ppdIndex: ppdIndex };
        return $http
          .post("/TimeStore/TC/UnrestrictedInitiallyApproved", p, {
            cache: false
          })
          .then(function (response)
          {
            return response.data;
          });
      };

      var getInitiallyApproved = function (ppdIndex)
      {
        var p = { ppdIndex: ppdIndex };
        return $http
          .post("/TimeStore/TC/InitiallyApproved", p, { cache: false })
          .then(function (response)
          {
            return response.data;
          });
      };

      var approveInitial = function (ad)
      {
        return $http
          .post("/TimeStore/TC/ApproveInitial", ad, { cache: false })
          .then(function (response)
          {
            return response.data;
          });
      };

      var approveFinal = function (ad)
      {
        return $http
          .post("/TimeStore/TC/ApproveFinal", ad, { cache: false })
          .then(function (response)
          {
            return response.data;
          });
      };

      var getReportsTo = function ()
      {
        return $http
          .get("/TimeStore/TC/ReportsToList", { cache: true })
          .then(function (response)
          {
            return response.data;
          });
      };

      var getMyAccess = function ()
      {
        return $http
          .get("/TimeStore/TC/Access", { cache: true })
          .then(function (response)
          {
            return response.data;
          });
      };

      var getAccess = function (employeeid)
      {
        return $http
          .post("/TimeStore/TC/Access", { EmployeeId: employeeid })
          .then(function (response)
          {
            return response.data;
          });
      };
      var saveAccess = function (rawTCA)
      {
        return $http
          .post("/TimeStore/TC/SaveAccess", rawTCA)
          .then(function (response)
          {
            return response.data;
          });
      };

      var getDepartments = function ()
      {
        return $http
          .get("/TimeStore/TC/DepartmentList", { cache: true })
          .then(function (response)
          {
            return response.data;
          });
      };
      var getEmployees = function ()
      {
        return $http
          .get("/TimeStore/TC/EmployeeList", { cache: true })
          .then(function (response)
          {
            return response.data;
          });
      };
      //var getPayPeriods = function () {
      //    return $http.get('/TimeStore/TC/PayPeriodList', { cache: true })
      //                .then(function (response) {
      //                    return response.data;
      //                });
      //};
      var getEmployee = function (payperiod, employeeid)
      {
        if (employeeid === undefined)
        {
          console.log("employeeid not provided to getEmployee");
          return;
        }
        return $http
          .post(
          "/TimeStore/TC/Employee",
          { EmployeeId: employeeid, PayPeriod: payperiod },
          { cache: false }
          )
          .then(function (response)
          {
            return response.data;
          });
      };
      var getDefaultEmployee = function ()
      {
        return $http
          .post("/TimeStore/TC/CurrentEmployee", { cache: false })
          .then(function (response)
          {
            return response.data;
          });
      };

      return {
        finalizeLeaveRequest: finalizeLeaveRequest,
        getPayPeriodIndex: getPayPeriodIndex,
        getDefaultEmployeeId: getDefaultEmployeeId,
        getPayPeriodEnd: getPayPeriodEnd,
        getPayPeriodStart: getPayPeriodStart,
        getGenericTimeData: getGenericTimeData,
        saveNote: SaveNoteToPayPeriod,
        saveTCTD: saveTCTD,
        classVehicle: classVehicle,
        classShiftTen: classShiftTen,
        classOnCall: classOnCall,
        financePostProcess: financePostProcess,
        saveHoliday: saveHoliday,
        getSignatureRequired: getSignatureRequired,
        saveIncentives: saveIncentives,
        getIncentives: getIncentives,
        getUnapproved: getUnapproved,
        getInitiallyApproved: getInitiallyApproved,
        approveInitial: approveInitial,
        approveFinal: approveFinal,
        getReportsTo: getReportsTo,
        getDepartments: getDepartments,
        getEmployee: getEmployee,
        getEmployees: getEmployees,
        getDefaultEmployee: getDefaultEmployee,
        getMyAccess: getMyAccess,
        getAccess: getAccess,
        saveAccess: saveAccess,
        getUncompletedApprovals: getUncompletedApprovals,
        getLeaveRequestsByEmployee: getLeaveRequestsByEmployee,
        getAllLeaveRequestsByEmployee: getAllLeaveRequestsByEmployee,
        getLeaveRequests: getLeaveRequests,
        saveCompTimeEarned: saveCompTimeEarned,
        getDeptLeaveRequests: getDeptLeaveRequests,
        getHolidays: getHolidays,
        getBirthdays: getBirthdays
      };
    }
  ]);
})();
