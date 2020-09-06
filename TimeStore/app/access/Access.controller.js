/* global _ */
(function ()
{
  "use strict";

  angular.module('timestoreApp')
    .controller('AccessController', ['$scope', '$mdToast', 'timestoredata', 'viewOptions', ControlAccess]);

  function ControlAccess($scope, $mdToast, timestoredata, viewOptions)
  {
    viewOptions.viewOptions.showSearch = false;
    viewOptions.viewOptions.share();
    $scope.employeeList = [];
    $scope.departmentList = [];
    $scope.reportsToList = [];
    $scope.employee = null;
    $scope.employeeMapV = null;
    $scope.timecardAccess = null;
    $scope.dataTypes = ["timecard", "telestaff"];

    $scope.accessLevels = ["None", "User Only", "Dept Level 1", "Dept Level 2",
      "Dept Level 3", "Dept Level 4", "Dept Level 5", "All"];

    $scope.payrollAccess = ["None", "Can Edit", "Can Approve"];


    $scope.toastPosition = {
      bottom: true,
      top: false,
      left: true,
      right: false
    };

    $scope.querySearch = function (query, aMap)
    {
      var results = query ? aMap.filter(createFilterFor(query)) : [];
      return results;
    };

    function createFilterFor(query)
    {
      var lowercaseQuery = angular.lowercase(query);
      return function filterFn(item)
      {
        return (item.displayLower.indexOf(lowercaseQuery) >= 0);
      };
    }

    timestoredata.getEmployees().then(function (data)
    {
      $scope.employeeList = data;
      $scope.employeeMapV = $scope.employeeList.map(function (employeeMap)
      {
        return {
          value: employeeMap,
          displayLower: angular.lowercase(employeeMap.EmployeeDisplay),
          display: employeeMap.EmployeeDisplay
        };
      });
    }, function () { });

    $scope.viewApproveAllChanged = function ()
    {
      approveAllChanged();
    }

    function approveAllChanged()
    {
      $scope.departmentList[0].disabled = $scope.timecardAccess.approveAll;
      $scope.departmentList[1].disabled = $scope.timecardAccess.approveAll;
    }

    timestoredata.getDepartments().then(function (data)
    {
      $scope.departmentList = data;
      console.log('department list', data);
      $scope.departmentList.unshift(
        {
          Department: 'View Only - Add Departments or Reports To',
          DepartmentNumber: 'VIEW',
          DepartmentDisplay: 'View Only - Add Departments or Reports To'
        },
        {
          Department: 'Approve Leave Only - Add Departments or Reports To',
          DepartmentNumber: 'LEAVE',
          DepartmentDisplay: 'Approve Leave Only - Add Departments or Reports To'
        });
      console.log('updated department list', $scope.departmentList);
    }, function () { });

    timestoredata.getReportsTo().then(function (data)
    {
      $scope.reportsToList = data;
    }, function () { });

    $scope.selectedEmployeeChanged = function (employee)
    {
      if (employee === undefined)
      {
        $scope.employee = null;
        $scope.timecardAccess = null;
      } else
      {
        $scope.employee = employee;
        timestoredata.getAccess(employee.value.EmployeeID).then(function (data)
        {
          $scope.timecardAccess = data;
          updateDepartmentList();
        });
        // Load Access for this user.
      }
    };

    function updateDepartmentList()
    {
      var tca = $scope.timecardAccess;
      tca.viewAll = (tca.DepartmentsToApprove.indexOf('VIEW') > -1);
      tca.approveAll = (tca.DepartmentsToApprove.indexOf('ALL') > -1);
      for (var i = 0; i < $scope.departmentList.length; i++)
      {
        var dept = $scope.departmentList[i];
        dept.selected = false;
        if (tca.DepartmentsToApprove.indexOf(dept.DepartmentNumber) > -1)
        {
          dept.selected = true;
        }
      }
      approveAllChanged();
    }

    $scope.GetEmployeeDisplay = function ()
    {
      var rtl = $scope.reportsToList;
      if ($scope.timecardAccess === null || $scope.timecardAccess.ReportsTo === 0 || $scope.reportsToList.length === 0)
      {
        return '';
      }
      var index = _.findIndex(rtl, { 'EmployeeID': $scope.timecardAccess.ReportsTo });
      if (index === -1)
      {
        return '';
      } else
      {
        return rtl[index].EmployeeDisplay;
      }
    };

    $scope.saveTimecardAccess = function ()
    {
      var rawTCA = {
        Access_Type: $scope.timecardAccess.Raw_Access_Type,
        EmployeeId: $scope.timecardAccess.EmployeeID,
        ReportsTo: $scope.timecardAccess.ReportsTo,
        RequiresApproval: $scope.timecardAccess.RequiresApproval,
        BackendReportsAccess: $scope.timecardAccess.Backend_Reports_Access,
        DepartmentsToApprove: getSelectedDepartmentList(),
        DataType: $scope.timecardAccess.Data_Type,
        CanChangeAccess: $scope.timecardAccess.CanChangeAccess,
        PayrollAccess: $scope.timecardAccess.PayrollAccess
      };
      console.log('raw Timecard Access', rawTCA);
      timestoredata.saveAccess(rawTCA).then(function (data) {
          if (data === 'Success') {
              showToast('Access Saved...');
          } else {
              showToast('Error attempting to save access, please contact MIS.');
          }
      });
    };

    function getSelectedDepartmentList()
    {
      var selDepts = [];
      if ($scope.timecardAccess.Raw_Access_Type < 2)
      { // They don't have any dept level access if they only 1 or 0.
        return selDepts;

      } else if ($scope.timecardAccess.viewAll)
      {
        selDepts.push('VIEW');
        return selDepts;

      } else if ($scope.timecardAccess.approveAll)
      {
        selDepts.push('ALL');
        //return selDepts;

      } //else {
      for (var i = 0; i < $scope.departmentList.length; i++)
      {
        var dl = $scope.departmentList[i];
        if (dl.selected)
        {
          selDepts.push(dl.DepartmentNumber);
        }
      }
      console.log('selected departments', selDepts);
      return selDepts;
      //}

    }

    function showToast(Message)
    {
      $mdToast.show(
        $mdToast.simple()
          .content(Message)
          .position($scope.getToastPosition())
          .hideDelay(3000)
      );
    }

    $scope.getToastPosition = function ()
    {
      return Object.keys($scope.toastPosition)
        .filter(function (pos) { return $scope.toastPosition[pos]; })
        .join(' ');
    };

  };

}());