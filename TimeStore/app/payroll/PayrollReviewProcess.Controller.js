(function ()
{
  "use strict";

  angular.module('timestoreApp')
    .controller('PayrollReviewController',
      ['$scope', 'viewOptions', 'timestoredata', 'timestoreNav', '$routeParams', PayrollReviewController]);


  function PayrollReviewController($scope, viewOptions, timestoredata, timestoreNav, $routeParams)
  {
    // need to get changes
    $scope.payPeriod = $routeParams.payPeriod;
    $scope.pay_period_ending = moment($routeParams.payPeriod, "YYYYMMDD").format("MM/DD/YYYY");

    $scope.loading = false;
    $scope.changes_only = true;
    $scope.base_payroll_edits = [];
    $scope.filtered_payroll_edits = [];
    $scope.filter_employee = "";
    $scope.filter_department = "";

    $scope.GetPaycode = function (c)
    {
      if (c.original === null)
      {
        return c.changed.paycode_detail.title + ' (' + c.changed.paycode + ')';
      }
      return c.original.paycode_detail.title + ' (' + c.original.paycode + ')';
      //original === null ? c.changed.paycode_detail.title + ' (' + c.changed.paycode + ')' : c.original.paycode_detail.title + ' (' + c.original.paycode + ')'
    }

    $scope.GetTotalOriginalHours = function (c)
    {
      let totalHours = c.reduce(function (j, v) { return j + (v.original ? v.original.hours : 0); }, 0);
      return totalHours.toFixed(2);      
    }
    $scope.GetTotalChangedHours = function (c)
    {
      let totalHours = c.reduce(function (j, v) { return j + (v.changed ? v.changed.hours : 0); }, 0);
      return totalHours.toFixed(2);
    }
    $scope.GetTotalOriginalAmount = function (c)
    {
      let totalHours = c.reduce(function (j, v) { return j + (v.original ? v.original.amount : 0); }, 0);
      return totalHours.toFixed(2);
    }
    $scope.GetTotalChangedAmount = function (c)
    {
      let totalHours = c.reduce(function (j, v) { return j + (v.changed ? v.changed.amount : 0); }, 0);
      return totalHours.toFixed(2);
    }

    $scope.GetTotalHours = function (paydata)
    {
      let totalHours = paydata.reduce(function (j, v) { return j + v.hours; }, 0);
      //console.log('total hours test', totalHours)
      return totalHours.toFixed(2);
    }

    $scope.GetTotalAmount = function (paydata)
    {
      let totalAmount = paydata.reduce(function (j, v) { return j + v.amount; }, 0);
      //console.log('total amount test', totalAmount)
      return totalAmount.toFixed(2);

    }

    $scope.returnToOverallProcess = function ()
    {
      timestoreNav.goPayrollOverallProcess($routeParams.payPeriod);
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
      console.log('apply filters');
      let filtered = $scope.base_payroll_edits;
      if ($scope.changes_only)
      {
        filtered = filtered.filter(function (j)
        {
          return j.comparisons.filter(function (c) { return c.has_changed }).length > 0
        });
      }
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
      console.log('filtered payroll data', filtered);
      $scope.filtered_payroll_edits = filtered;
    }

    $scope.getPayrollData();

  }

})();