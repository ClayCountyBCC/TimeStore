/* global _ */
(function ()
{
  "use strict";
  angular
    .module("timestoreApp")
    .controller("ReportController", [
      "$scope",
      "timestoredata",
      "viewOptions",
      "$mdToast",
      MainReport
    ]);

  function MainReport($scope, timestoredata, viewOptions, $mdToast)
  {
    $scope.message = "";
    $scope.showProgress = false;
    $scope.timeData = [];
    $scope.dataToView = [];
    $scope.departmentList = [];
    $scope.showSettings = true;
    $scope.showFilters = false;
    $scope.showFields = true;
    $scope.searchText = "";
    $scope.selectedFields = [];
    $scope.selectedItem = null;
    $scope.minDate = moment("1/1/2015", "M/D/YYYY").toDate();
    $scope.maxDate = moment().add(1, "years").toDate();
    $scope.dateFrom = null;
    $scope.dateTo = null;
    $scope.csvUrl = "";
    $scope.csvFilename = "CustomReport.csv";

    $scope.fieldList = [
      "Vacation",
      "Holiday",
      "Sick",
      "CompTimeEarned",
      "CompTimeUsed",
      "Admin",
      "AdminBereavement",
      "AdminDisaster",
      "AdminWorkersComp",
      "AdminJuryDuty",
      "AdminMilitaryLeave",
      "AdminOther",
      "SickFamilyLeave",
      "SickLeavePool",
      "AdminEducation",
      "Swap",
      "MWI",
      "StepUp",
      "HonorGuard",
      "LWOPSuspension",
      "LWOPScheduled",
      "LeaveWithoutPay",
      "SickLeaveWithoutPay",
      "BreakCredit",
      "DoubleTime",
      "CallMin",
      "Vehicle",
      "WorkersComp",
      "OnCallTotalHours",
      "OnCallWorkHours",
      "OnCallMinimumHours",
      "UnionTimePool"
    ];

    //timestoredata.getGenericTimeDataDateless().then(ProcessData, function () { });
    //timestoredata.getDepartments().then(function (data) {
    //    $scope.departmentList = data;
    //}, function () { });

    //$scope.querySearch = function (query) {
    //    var results = query ? $scope.fieldList.filter(createFilterFor(query)) : [];
    //    return results;
    //}

    //function createFilterFor(query) {
    //    var lowercaseQuery = angular.lowercase(query);
    //    return function filterFn(field) {
    //        return (field.toLowerCase().indexOf(lowercaseQuery) === 0);
    //    };
    //}

    function ProcessData(data)
    {
      $scope.timeData = data;
      convertToCSV(data);
      console.log("timedata", $scope.timeData);
      if (data.length === 0)
      {
        $scope.message = "No records were found for that criteria.";
      } else
      {
        updateReport();
      }
      $scope.showProgress = false;
    }

    $scope.Search = function ()
    {
      // Will need to check to make sure that both dates are selected.
      $scope.message = "";
      if ($scope.dateFrom === null || $scope.dateTo === null)
      {
        $scope.message =
          "You must enter a Start Date and an End Date to search.";
        return;
      }
      $scope.showProgress = true;
      timestoredata
        .getGenericTimeData(
        $scope.dateFrom,
        $scope.dateTo,
        $scope.selectedFields
        )
        .then(ProcessData, function () { });
    };

    $scope.addDisplay = function (d)
    {
      $scope.dataToView = [];
      var i = $scope.selectedFields.indexOf(d);
      if (i > -1)
      {
        $scope.selectedFields.splice(i, 1);
      } else
      {
        $scope.selectedFields.push(d);
      }
    };

    $scope.updateFieldSelections = function ()
    {
      $scope.timeData = [];
      $scope.dataToView = [];
      $scope.csvUrl = null;
      $scope.csvFilename = "";
      //console.log('selected fields', $scope.selectedFields);
    };

    function updateReport()
    {
      //$scope.timeData = [];
      $scope.dataToView = [];
      $scope.csvUrl = null;
      $scope.csvFilename = "";
      // Go through each item in the timeData array
      // if the object has a property that's in fieldsToDisplay that's got a value > 0
      // then convert it into the data object below and add it to the dataToView array.
      // Or check if one already exists for that person and the find that and add the values.
      // set up the ng-repeat with track by eid
      _.each($scope.timeData, function (t)
      {
        processReportData(t);
      });
      addTotals();
    }

    function processReportData(d)
    {
      var fl = $scope.selectedFields;
      for (var i = 0; i < fl.length; i++)
      {
        if (d[fl[i]] > 0)
        {
          addData(d);
          return;
        }
      }
    }

    function addData(d)
    {
      var fl = $scope.selectedFields;
      var index = _.findIndex($scope.dataToView, function (b)
      {
        return b.eid === d.EmployeeID;
      });
      var data;
      if (index === -1)
      {
        data = {
          eid: d.EmployeeID,
          dept: d.DepartmentID,
          displayName: d.FullName,
          values: []
        };
        data.values = _.fill(Array(fl.length), 0);
      } else
      {
        data = $scope.dataToView[index];
      }
      for (var i = 0; i < fl.length; i++)
      {
        if (d[fl[i]] > 0)
        {
          data.values[i] += d[fl[i]];
        }
      }
      if (index === -1)
      {
        $scope.dataToView.push(data);
      }
    }

    function addTotals()
    {
      _.each($scope.dataToView, function (n)
      {
        n.values.push(
          _.reduce(
            n.values,
            function (total, i)
            {
              return total + i;
            },
            0
          )
        );
      });
    }

    //$scope.toggleSettingsDisplay = function () {
    //    $scope.showSettings = !$scope.showSettings;
    //};
    //$scope.toggleFilterDisplay = function () {
    //    $scope.showFilters = !$scope.showFilters;
    //};
    //$scope.toggleFieldDisplay = function () {
    //    $scope.showFields = !$scope.showFields;
    //};

    function convertToCSV(data)
    {
      console.log("converttocsv function called");
      if (data.length === 0) return;
      var fields = Object.keys(data[0]);

      //var csv = fields.join(',') + '\r\n'; // header row

      var csv = data.map(function (row)
      {
        var m = fields.map(function (fieldName)
        {
          switch (fieldName)
          {
            case "WorkDate":
            case "TerminationDate":
            case "HireDate":
              return moment(row[fieldName]).format("M/D/YYYY HH:mm A");
            //break;
            default:
              return row[fieldName] || "";
          }
        });
        m = m.slice(0, -1).join(",");
        //console.log("row", m);
        return m;
      });
      csv.unshift(fields.join(","));
      //console.log('csv', csv);
      var blob = new Blob([csv.join("\r\n")], { type: "text/csv" });
      var url = window.URL || window.webkitURL;
      var link = document.getElementById("download");
      link.href = url.createObjectURL(blob);

      //$scope.csvUrl = 

      //console.log("csv url", $scope.csvUrl);
      //$scope.csvFilename =
      //  "Custom Report - " +
      //  $scope.selectedFields.join(" ") +
      //  " " +
      //  moment().format("M-D-YYYY HHmmss") +
      //  ".csv";
      //console.log("filename", $scope.csvFilename);
    }
  }
})();
