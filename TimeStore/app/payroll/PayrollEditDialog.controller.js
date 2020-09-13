(function ()
{
  "use strict";

  angular.module('timestoreApp')
    .controller('PayrollEditDialogController',
      ['$scope', 'viewOptions', 'timestoredata', 'timestoreNav', '$routeParams', '$mdDialog', 'edit_data', 'paycodes', 'projectcodes', PayrollEditDialogController]);


  function PayrollEditDialogController($scope,
    viewOptions,
    timestoredata,
    timestoreNav,
    $routeParams,
    $mdDialog,
    edit_data,
    paycodes,
    projectcodes)
  {
    $scope.pay_period_ending = moment($routeParams.payPeriod, "YYYYMMDD").format("MM/DD/YYYY");    
    $scope.validation_errors = "";
    $scope.edit_data = edit_data;
    $scope.paycodes = paycodes;
    $scope.project_codes = projectcodes;
    $scope.defaultview = "default";
    $scope.default_display = [];
    UpdateDefaultDisplay($scope.edit_data.finplus_payrates);
    console.log('payroll edit dialog edit data', $scope.edit_data);
    $scope.hide = function ()
    {
      // this is the save function
      $scope.ValidateChanges();
      $mdDialog.hide();
    }

    $scope.cancel = function ()
    {
      $mdDialog.cancel();
    }

    $scope.ValidateChanges = function()
    {
      let data_changed = $scope.edit_data.payroll_change_data.length !== $scope.edit_data.base_payroll_data.length;
      $scope.validation_errors = "";
      // Some rules:
      // Paycode, Payrate, ProjectCode combinations must be unique to each item in the payroll_changes array
      for (let i = 0; i < $scope.edit_data.payroll_change_data.length; i++)
      {
        let pcdi = $scope.edit_data.payroll_change_data[i];
        for (let j = 0; j < $scope.edit_data.payroll_change_data.length; j++)
        {
          if (i != j)
          {
            let pcdj = $scope.edit_data.payroll_change_data[j];
            if (pcdi.paycode === pcdj.paycode &&
              pcdi.payrate === pcdj.payrate &&
              pcdi.project_code === pcdj.project_code)
            {
              // houston we have a problem
              $scope.validation_errors = "You cannot add a row that is has the same paycode, payrate, and project code as another row.  Instead, increase the hours or change the hours of the original row accordingly";
              return;
            }
          }
        }
      }

      if (!CheckForSavedJustifications() && !data_changed)
      {
        for (let a = 0; a < $scope.edit_data.base_payroll_data.length; a++)
        {
          let match_found = false;
          let pcda = $scope.edit_data.base_payroll_data[a];
          for (let i = 0; i < $scope.edit_data.payroll_change_data.length; i++)
          {
            let pcdi = $scope.edit_data.payroll_change_data[i];
            if (pcda.paycode === pcdi.paycode &&
              pcda.hours === pcdi.hours &&
              pcda.payrate === pcdi.payrate &&
              pcda.amount === pcdi.amount &&
              pcda.classify === pcdi.classify &&
              pcda.project_code === pcdi.project_code)
            {
              match_found = true;
              break;
            }
          }
          if (!match_found)
          {
            data_changed = true;
            break;
          }
        }

      }

      if (!CheckForSavedJustifications() && data_changed)
      {
        $scope.validation_errors = "If any changes are made, a justification must be added.";
      }
    }

    function CheckForSavedJustifications()
    {
      if ($scope.edit_data.justifications.length === 0) return false;
      return $scope.edit_data.justifications.filter(function (j) { return j.id > -1 }).length > 0;
    }

    $scope.RevertAllChanges = function ()
    {
      $scope.edit_data.payroll_change_data.splice(0, $scope.edit_data.payroll_change_data.length);
      for (let e of $scope.edit_data.base_payroll_data)
      {
        $scope.edit_data.payroll_change_data.push(Object.assign({}, e));
      }
      $scope.ValidateChanges()
    }

    $scope.UpdateView = function ()
    {
      if ($scope.defaultview === "default")
      {
        UpdateDefaultDisplay($scope.edit_data.finplus_payrates);
      }
      else
      {
        getCheckData($scope.edit_data.employee.EmployeeId, $scope.defaultview);
      }
    }

    $scope.AddPayrollChange = function ()
    {
      
      let e = $scope.edit_data.employee;

      let data = {
        amount: 0,
        changed_by: "",
        changed_on: null,
        classify: e.Classify,
        employee_id: e.EmployeeId,
        hours: 0,
        messages: [],
        orgn: e.Department,
        paycode: "",
        paycode_detail: {},
        payrate: e.Base_Payrate,
        project_code: ""
      }
      $scope.edit_data.payroll_change_data.push(data);
      $scope.ValidateChanges();
    }

    function UpdateDefaultDisplay(data)
    {
      $scope.default_display.splice(0, $scope.default_display.length);
      for (let d of data)
      {
        $scope.default_display.push({
          paycode: d.paycode,
          hours: d.hours,
          payrate: d.payrate,
          amount: d.amount,
          classify: d.classify
        });
      }
    }

    function getCheckData(employee_id, check_number)
    {
      timestoredata.getCheckPay(employee_id, check_number)
        .then(function (data)
        {
          UpdateDefaultDisplay(data);
        });
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

    $scope.RemoveDeleted = function ()
    {
      let d = $scope.edit_data.payroll_change_data.filter(function (j) { return !j.delete; });
      $scope.edit_data.payroll_change_data = d;
      $scope.ValidateChanges();
    }

    $scope.AddJustification = function ()
    {
      var j = {
        id: ($scope.edit_data.justifications.length + 1) * -1,
        pay_period_ending: $scope.edit_data.pay_period_ending,
        employee_id: $scope.edit_data.employee.EmployeeId,
        justification: "",
        added_on: new Date(),
        added_by: ""
      }
      console.log('added justification', j);
      $scope.edit_data.justifications.push(j);
    }

    $scope.SaveJustifications = function ()
    {
      timestoredata.saveJustifications($scope.pay_period_ending, $scope.edit_data.employee.EmployeeId, $scope.edit_data.justifications)
        .then(function (data)
        {
          if (!data) return;
          $scope.edit_data.justifications = data;
        })
      $scope.ValidateChanges();
    }

    $scope.DeleteJustification = function (id)
    {
      if (id < 0)
      {
        $scope.edit_data.justifications = $scope.edit_data.justifications.filter(function (j) { return j.id !== id });
      }
      else
      {
        timestoredata.deleteJustification($scope.pay_period_ending, id)
          .then(function (data)
          {
            if (data)
            {
              // lets remove it from the list
              $scope.edit_data.justifications = $scope.edit_data.justifications.filter(function (j) { return j.id !== id });
            }
          })
      }
      $scope.ValidateChanges();
    }

  }

})();