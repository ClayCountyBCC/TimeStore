/* global moment, _ */
(function ()
{
  "use strict";
  angular.module('timestoreApp')
    .directive('timecardHeader', function ()
    {
      return {
        restrict: 'E',
        templateUrl: 'TimeCardHeader.tmpl.html', //'app/timecard/TimeCardHeader.tmpl.html',
        scope: {
          timecard: '=',
          shortheader: '='
        },
        controller: function ($scope)
        {
          $scope.showPayrate = false;
        }
      };
    });
}());