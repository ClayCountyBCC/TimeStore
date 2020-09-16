(function ()
{
  "use strict";

  angular.module('timestoreApp')
    .controller('PayrollEditController',
      ['$scope', 'viewOptions', 'timestoredata', 'timestoreNav', '$routeParams', '$mdDialog', PayrollEditController]);


  function PayrollEditController($scope, viewOptions, timestoredata, timestoreNav, $routeParams, $mdDialog)
  {
    $scope.pay_period_ending = moment($routeParams.payPeriod, "YYYYMMDD").format("MM/DD/YYYY");    
    $scope.loading = false;
    $scope.project_codes = [];
    $scope.base_payroll_edits = [];
    $scope.filtered_payroll_edits = [];
    $scope.filter_employee = "";
    $scope.filter_department = "";
    
    $scope.paycodes = {};
    $scope.paycodeslist = [];
    $scope.currentStatus = {};
    $scope.edit_target = [];
    // need to get data for this pay period
    function HandleCurrentStatus(data)
    {
      if (!data)
      {
        console.log('payroll status is invalid');
        return;
      }
      $scope.currentStatus = UpdateDisplays(data);
      console.log('payroll status', $scope.currentStatus);
    }

    $scope.returnToOverallProcess = function ()
    {
      timestoreNav.goPayrollOverallProcess($routeParams.payPeriod);
    }

    function UpdateDisplays(data)
    {
      data.started_on_display = formatDatetime(data.started_on);
      data.edits_completed_on_display = formatDatetime(data.edits_completed_on);
      data.edits_approved_on_display = formatDatetime(data.edits_approved_on);
      data.finplus_updated_on_display = formatDatetime(data.finplus_updated_on);
      return data;
    }

    function GetPayrollStatus()
    {
      timestoredata.getPayrollStatus($scope.pay_period_ending)
        .then(function (data)
        {
          HandleCurrentStatus(data);
        });
    }


    function GetProjectCodes()
    {
      timestoredata.getProjectCodes($scope.pay_period_ending)
        .then(function (data)
        {
          console.log('project codes', data);
          $scope.project_codes = data;
        });
    }

    function formatDatetime(d)
    {
      return d ? new Date(d).toLocaleString('en-us') : "";
    }

    function getPaycodes()
    {
      timestoredata.getPaycodes($scope.pay_period_ending)
        .then(function (data)
        {
          console.log('paycode data', data);
          $scope.paycodes = data;
          $scope.paycodeslist = ConvertPaycodeDictionaryToArray(data);
        });
    }

    function ConvertPaycodeDictionaryToArray(data)
    {
      var list = [];
      Object.keys(data).forEach(function (k, i)
      {
        list.push(data[k]);
      })
      //for (const [key, value] of Object.entries(data))
      //{
      //  list.push(value);
      //}
      list.sort(function (a, b) { return a.pay_code - b.pay_code; })
      return list;
    }

    $scope.getPayrollData = function ()
    {
      $scope.loading = true;
      timestoredata.getPayrollEdits($scope.pay_period_ending)
        .then(function (data)
        {
          console.log('payroll data', data);
          $scope.base_payroll_edits = data;
          $scope.applyFilters();
          $scope.loading = false;
        });
    }


    $scope.applyFilters = function ()
    {
      let filtered = $scope.base_payroll_edits;
      if ($scope.filter_employee.length > 0)
      {
        filtered = filtered.filter(function (j)
        {
          return j.employee.EmployeeId.toString().indexOf($scope.filter_employee) > -1 ||
            j.employee.EmployeeName.toUpperCase().indexOf($scope.filter_employee.toUpperCase()) > -1;
        });
      }

      if ($scope.filter_department.length > 0)
      {
        filtered = filtered.filter(function (j)
        {
          return j.employee.Department.toString().indexOf($scope.filter_department) > -1 ||
            j.employee.DepartmentName.toUpperCase().indexOf($scope.filter_department.toUpperCase()) > -1;
        });

      }

      $scope.filtered_payroll_edits = filtered;
    }

    $scope.getPayrollData();
    getPaycodes();
    GetPayrollStatus();
    GetProjectCodes();
  }

})();