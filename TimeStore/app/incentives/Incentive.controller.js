(function () {
    "use strict";
    angular.module('timestoreApp')
        .controller('IncentiveController', ['$scope', 'timestoredata', 'viewOptions', '$routeParams', '$mdToast', Incentives]);

    function Incentives($scope, timestoredata, viewOptions, $routeParams, $mdToast) {
        viewOptions.viewOptions.showSearch = false;
        viewOptions.viewOptions.share();
        $scope.incentives = [];
        $scope.incentiveType = 0;
        if ($routeParams.incentiveType !== undefined) {
            $scope.incentiveType = parseInt($routeParams.incentiveType);
        }

        timestoredata.getIncentives($scope.incentiveType).then(ProcessData, function () { });

        function ProcessData(data) {
            $scope.incentives = data;
        }
        
        function showToast(Message) {
            $mdToast.show(
              $mdToast.simple()
                .content(Message)
                .position($scope.getToastPosition())
                .hideDelay(3000)
            );
        }

        $scope.getToastPosition = function () {
            return Object.keys($scope.toastPosition)
              .filter(function (pos) { return $scope.toastPosition[pos]; })
              .join(' ');
        };
        $scope.SaveIncentives = function () {
            timestoredata.saveIncentives($scope.incentives).then(CheckData, function () { });
        };
        function CheckData(data) {
            if (data.toUpperCase() === 'SUCCESS') {
                showToast('Your changes have been saved.');
            } else {
                showToast(data);
            }
        }

        $scope.toastPosition = {
            bottom: true,
            top: false,
            left: false,
            right: true
        };

    }

})();