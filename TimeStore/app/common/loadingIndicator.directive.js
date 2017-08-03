(function () {

  angular.module('timestoreApp')
  .directive('routeLoadingIndicator', function () {
    return {
      restrict: 'E',
      template: "<md-progress-linear flex='100' ng-show='isRouteLoading === true' md-mode='indeterminate'></md-progress-linear>",
      controller: ['$scope', '$rootScope', function ($scope, $rootScope) {
        $scope.isRouteLoading = false;

        $rootScope.$on('$routeChangeStart', function (event, next, current) {
          $scope.isRouteLoading = true;
        });

        $rootScope.$on('$routeChangeSuccess', function (event, next, current) {
          $scope.isRouteLoading = false;
        });
      }]
    };
  });

})();