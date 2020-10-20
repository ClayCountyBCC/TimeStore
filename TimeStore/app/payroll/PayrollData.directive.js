/* global moment, _ */
(function ()
{
  "use strict";
  angular
    .module("timestoreApp")
    .directive("payrollData", function ()
    {
      return {
        restrict: "E",
        scope: {
          showheader: "<",
          messageonly: "<",
          totalonly: "<",
          message: "<",
          pd: "=",
          totalhours: "<",
          totalamount: "<"
          //event: "=",
          //fulltimelist: "<",
          //eventerror: "=",
          //calc: "&"
        },
        templateUrl: "PayrollData.directive.tmpl.html",
        controller: "PayrollDataDirectiveController"
      };
    })
    .controller("PayrollDataDirectiveController", ["$scope", PayrollDataController]);

  function PayrollDataController($scope)
  {

  }
})();
