/* global moment, _ */
(function ()
{
  "use strict";
  angular.module('timestoreApp')
    .directive('existingLeaveRequest', function ()
    {
      return {
        restrict: 'E',
        templateUrl: 'existingLeaveRequest.directive.tmpl.html',
        scope: {
          requests: '='
        },
        controller: ['$scope', 'timestoredata', 'timestoreNav', '$routeParams', 'datelist',
          function ($scope, timestoredata, timestoreNav, $routeParams, datelist)
          {

            //$scope.dataList = [];

            $scope.refreshLeaveRequests = function ()
            {
              timestoredata.getLeaveRequestsByEmployee($routeParams.employeeId)
                .then(processData);
            };

            function processData(data)
            {
              $scope.requests = data;
            }



          }]
      };
    });

}());