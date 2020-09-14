/* global moment, _ */
(function ()
{
  "use strict";
  angular
    .module("timestoreApp")
    .directive("editPayrollData", function ()
    {
      return {
        restrict: "E",
        scope: {
          employee: "<",
          finplus_payrates: "<",
          paycodes: "<",
          projectcodes: "<",
          showheader: "<",
          messageonly: "<",
          totalonly: "<",
          message: "<",
          pd: "=",
          totalhours: "<",
          totalamount: "<",
          validate: "&",
          remove:"&"
          //event: "=",
          //fulltimelist: "<",
          //eventerror: "=",
          //calc: "&"
        },
        templateUrl: "EditPayrollData.directive.tmpl.html",
        controller: "EditPayrollDataDirectiveController"
      };
    })
    .controller("EditPayrollDataDirectiveController", ["$scope", EditPayrollDataController]);



  function EditPayrollDataController($scope)
  {
    $scope.RecalculateAmount = function ()
    {
      if ($scope.pd.paycode_detail.pay_type === 'H')
      {
        $scope.pd.amount = Math.round($scope.pd.hours * $scope.pd.payrate, 2);
      }
      $scope.validate();
    }
    $scope.UpdatePaycodeDetail = function ()
    {
      let pc = $scope.paycodes.filter(function (j) { return j.pay_code === $scope.pd.paycode });
      if (pc && pc.length > 0)
      {
        $scope.pd.paycode_detail = pc[0];
        let employee_classify = $scope.employee.Classify;
        let d = $scope.pd.paycode_detail;
        switch (d.pay_type)
        {
          case 'H':
            // use their regular payrate
            var new_payrate = parseFloat(($scope.employee.Base_Payrate * d.percent_x).toFixed(5));
            $scope.pd.payrate = new_payrate;
            $scope.pd.hours = 0;
            $scope.pd.amount = 0; // amount will be hours * payrate
            $scope.pd.classify = d.default_classify.length > 0 ? d.default_classify : employee_classify;
            break;

          case 'U':
            // default hours to 1
            // default payrate to 0 
            // default amount to percent_x
            $scope.pd.payrate = 0; // locked
            $scope.pd.hours = 1; // locked
            $scope.pd.amount = d.percent_x; 
            $scope.pd.classify = d.default_classify.length > 0 ? d.default_classify : employee_classify;
            break;

          case 'P':
          case 'A': // adjustment
            $scope.pd.payrate = 0; // locked
            $scope.pd.hours = 1; // locked
            $scope.pd.amount = 0; // this will be variable 
            $scope.pd.classify = d.default_classify.length > 0 ? d.default_classify : employee_classify;
            break;

        }

      }
      $scope.validate()
    }

    $scope.DeleteData = function ()
    {
      $scope.pd.delete = true;
      $scope.remove();
    }
  }
})();
