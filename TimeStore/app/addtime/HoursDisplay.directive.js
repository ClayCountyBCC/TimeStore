/* global moment, _ */
(function ()
{
  "use strict";
  angular
    .module("timestoreApp")
    .directive("hoursDisplay", function ()
    {
      return {
        restrict: "E",
        scope: {
          tctd: "=",
          hours: "=",
          calc: "&" //ng-change="$parent.calculateTotalHours()"
        },
        templateUrl: "HoursDisplay.directive.tmpl.html",
        controller: "HoursDisplayDirectiveController"
      };
    })
    .controller("HoursDisplayDirectiveController", ["$scope", HoursDisplay]);

  function HoursDisplay($scope) { }
})();
