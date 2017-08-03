/* global moment, _ */
(function () {
  "use strict";
  angular.module('timestoreApp')
      .directive('timeList', function () {
        return {
          restrict: 'E',
          templateUrl: 'TimeList.tmpl.html',
          scope: {
            tl: '='
          },
          controller: ['$scope', 'commonFunctions', function ($scope, commonFunctions) {

            $scope.getGroups = function () {
              return commonFunctions.getGroupsByShortPayRate($scope.tl);
            };

            $scope.getTotalHours = function () {
              return commonFunctions.getTotalHours($scope.tl);

            };
          }]
        };
      });
}());