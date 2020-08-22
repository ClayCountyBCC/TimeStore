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
          event: "=",
          fulltimelist: "<",
          eventerror: "=",
          calc: "&"
        },
        templateUrl: "DisasterHours.directive.tmpl.html",
        controller: "DisasterHoursDirectiveController"
      };
    })
    .controller("DisasterHoursDirectiveController", ["$scope", DisasterHours]);

  function DisasterHours($scope)
  {
    $scope.CheckDisasterWorkType = function ()
    {
      console.log('finish checkDisasterWorkType');
    }

  }
})();
