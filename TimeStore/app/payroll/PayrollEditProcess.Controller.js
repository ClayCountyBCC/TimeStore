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
    $scope.filter_error_message = "";
    $scope.error_messages = [];
    
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
          getMessages();
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

      if ($scope.filter_error_message.length > 0)
      {
        let m = $scope.filter_error_message.toLowerCase().trim();
        filtered = filtered.filter(function (j)
        {
          return LookForErrorMessage(j, m);
        });
      }

      $scope.filtered_payroll_edits = filtered;
    }

    function LookForErrorMessage(d, m)
    {
      //console.log('look for message data', d, m);
      for (var i = 0; i < d.messages.length; i++)
      {
        var em = d.messages[i];
        if (em.toLowerCase().indexOf(m) > -1) return true;
      }
      var pcd = d.payroll_change_data;
      for (var i = 0; i < pcd.length; i++)
      {
        var pcm = pcd[i].messages
        for (var j = 0; j < pcm.length; j++)
        {

          if (pcm[j].toLowerCase().indexOf(m) > -1) return true;
        }
      }
      return false;
    }

    function getMessages()
    {
      $scope.fuzzy_messages = [];
      $scope.error_messages = [];
      // add some default basic error messages
      $scope.error_messages.push("Timestore Error: Too many Sick hours used.");
      $scope.error_messages.push("Timestore Error: Using too many Sick hours");
      $scope.error_messages.push("Timestore Error: Too many Vacation hours used.");
      $scope.error_messages.push("Timestore Error: Using too many Vacation hours");
      $scope.error_messages.push("Timestore Error: Using too many Comp Time Used hours");
      $scope.error_messages.push("*** TIMECARD RATE NOT = CURRENT RATE");
      $scope.error_messages.push("Timestore Error: Too many Holiday hours used.")

      $scope.fuzzy_messages.push("Timestore Error: Too many Sick hours used.");
      $scope.fuzzy_messages.push("Timestore Error: Too many Vacation hours used.");
      $scope.fuzzy_messages.push("Timestore Error: Using too many Sick hours");
      $scope.fuzzy_messages.push("Timestore Error: Using too many Vacation hours");
      $scope.fuzzy_messages.push("Timestore Error: Using too many Comp Time Used hours");
      $scope.fuzzy_messages.push("*** TIMECARD RATE");
      $scope.fuzzy_messages.push("Timestore Error: Too many Holiday hours used.")
      $scope.fuzzy_messages.push("Sick hours banked")
      
      for (var i = 0; i < $scope.base_payroll_edits.length; i++)
      {
        var pe = $scope.base_payroll_edits[i];
        for (var j = 0; j < pe.payroll_change_data.length; j++)
        {
          var ped = pe.payroll_change_data[j];
          if (ped.messages.length > 0)
          {
            for (var k = 0; k < ped.messages.length; k++)
            {
              var m = ped.messages[k];

              var is_fuzzy = false;
              for (var l = 0; l < $scope.fuzzy_messages.length; l++)
              {
                if (m.toLowerCase().indexOf($scope.fuzzy_messages[l].toLowerCase()) > -1)
                {
                  is_fuzzy = true;
                }
              }
              if ($scope.error_messages.indexOf(m) === -1 && !is_fuzzy)
              {
                $scope.error_messages.push(m);
              }


              //if ($scope.error_messages.indexOf(m) === -1) $scope.error_messages.push(m);
            }
          }
        }
        for (var j = 0; j < pe.messages.length; j++)
        {
          var m = pe.messages[j];
          var is_fuzzy = false;
          for (var l = 0; l < $scope.fuzzy_messages.length; l++)
          {
            if (m.toLowerCase().indexOf($scope.fuzzy_messages[l].toLowerCase()) > -1)
            {
              is_fuzzy = true;
            }
          }
          if ($scope.error_messages.indexOf(m) === -1 && !is_fuzzy)
          {
            $scope.error_messages.push(m);
          }
        }
      }
      $scope.error_messages.sort();//function (a, b) { return a - b; });
      console.log('error messages list', $scope.error_messages);
    }

    $scope.getPayrollData();
    getPaycodes();
    GetPayrollStatus();
    GetProjectCodes();
  }

})();