/* global moment, _ */
(function ()
{
  "use strict";
  angular
    .module("timestoreApp")
    .directive("disasterHours", function ()
    {
      return {
        restrict: "E",
        scope: {
          tctd: "=",
          hours: "=",
          calc: "&" //ng-change="$parent.calculateTotalHours()"
        },
        templateUrl: "DisasterHours.directive.tmpl.html",
        controller: "DisasterHoursDirectiveController"
      };
    })
    .controller("DisasterHoursDirectiveController", ["$scope", DisasterHours]);

  function DisasterHours($scope) { }
})();
