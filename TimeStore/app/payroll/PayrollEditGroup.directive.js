/* global moment, _ */
(function ()
{
  "use strict";
  angular
    .module("timestoreApp")
    .directive("payrollEditGroup", function ()
    {
      return {
        restrict: "E",
        scope: {       
          ped: "=",
          paycodes: "<"
          //event: "=",
          //fulltimelist: "<",
          //eventerror: "=",
          //calc: "&"
        },
        templateUrl: "PayrollEditGroup.directive.tmpl.html",
        controller: "PayrollEditGroupDirectiveController"
      };
    })
    .controller("PayrollEditGroupDirectiveController", ["$scope", PayrollEditGroupController]);

  function PayrollEditGroupController($scope)
  {

    $scope.GetTotalHours = function (paydata)
    {
      let totalHours = paydata.reduce(function (j, v) { return j + v.hours;  }, 0);
      //console.log('total hours test', totalHours)
      return totalHours.toFixed(2);
    }

    $scope.GetTotalAmount = function (paydata)
    {
      let totalAmount = paydata.reduce(function (j, v) { return j + v.amount; }, 0);
      //console.log('total amount test', totalAmount)
      return totalAmount.toFixed(2);
    }

  }
})();
