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
    $scope.disablehours = true;
    $scope.disablepayrate = true;
    $scope.disableamount = true;

    $scope.RecalculateAmount = function ()
    {
      if ($scope.pd.paycode_detail.pay_type === 'H')
      {
        $scope.pd.amount = parseFloat(($scope.pd.hours * $scope.pd.payrate).toFixed(2));
      }
      $scope.validate();
    }

    $scope.UpdatePaycodeDetail = function ()
    {
       
      let pc = $scope.paycodes.filter(function (j) { return j.pay_code === $scope.pd.paycode });
      if (pc && pc.length > 0)
      {
        $scope.pd.paycode_detail = pc[0];
        let d = $scope.pd.paycode_detail;
        console.log('update paycode detail', d);
        UpdateDefaults();
      }
      $scope.validate()
    }

    function UpdateDefaults()
    {
      var d = $scope.pd.paycode_detail;
      let employee_classify = $scope.employee.Classify;
      $scope.pd.classify = d.default_classify.length > 0 ? d.default_classify : employee_classify;
      
      switch (d.time_type)
      {
        case 'A':
          $scope.pd.payrate = 0; // locked
          $scope.pd.hours = 1; // locked
          $scope.pd.amount = 0; // this will be variable 

          break;

        default:
          switch (d.pay_type)
          {
            case 'A':
              $scope.pd.payrate = 0; // locked
              $scope.pd.hours = 0; // locked
              $scope.pd.amount = 0; // this will be variable 
              break;

            case 'P':
              $scope.pd.payrate = 0; // locked
              $scope.pd.hours = 1; // locked
              $scope.pd.amount = 0; // this will be variable 
              break;

            case 'H':
              // use their regular payrate
              var new_payrate = parseFloat(($scope.employee.Base_Payrate * d.percent_x).toFixed(5));
              $scope.pd.payrate = new_payrate;
              if (!$scope.pd.hours)
              {
                $scope.pd.hours = 0;
              }
              if (!$scope.pd.hours || !$scope.pd.payrate)
              {
                $scope.pd.amount = 0; // amount will be hours * payrate
              }
              else
              {
                $scope.pd.amount = parseFloat(($scope.pd.payrate * $scope.pd.hours).toFixed(2)); // amount will be hours * payrate
              }

              break;

            case 'U':
              // default hours to 1
              // default payrate to 0 
              // default amount to percent_x
              $scope.pd.payrate = 0; // locked
              $scope.pd.hours = 1; // locked
              $scope.pd.amount = d.percent_x;
              // all fields except the amount should be locked.
              break;
          }
      }
      UpdatePayFields();

    }

    function UpdatePayFields()
    {
      $scope.disablehours = true;
      $scope.disablepayrate = true;
      $scope.disableamount = true;
     
      if ($scope.pd && $scope.pd.paycode_detail)
      {
        switch ($scope.pd.paycode_detail.time_type)
        {
          case 'A':
            $scope.disableamount = false;
            break;

          default:
            switch ($scope.pd.paycode_detail.pay_type)
            {
              case 'A':
              case 'P':
                $scope.disableamount = false;
                break;

              case 'H':
                $scope.disableamount = false;
                $scope.disablehours = false;
                $scope.disablepayrate = false;

                break;

              case 'U':
                $scope.disableamount = false;
                break;
            }
        }
      }

    }

    $scope.DeleteData = function ()
    {
      $scope.pd.delete = true;
      $scope.remove();
    }

    UpdatePayFields();
  }
})();
