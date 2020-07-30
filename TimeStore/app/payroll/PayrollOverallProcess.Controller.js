(function ()
{
  "use strict";

  angular.module('timestoreApp')
    .controller('PayrollOverallController',
      ['$scope', 'viewOptions', 'timestoredata', 'timestoreNav', '$routeParams', PayrollOverallController]);


  function PayrollOverallController($scope, viewOptions, timestoredata, timestoreNav, $routeParams)
  {
    console.log('payroll overall process');
    $scope.showSetUpDetails = false;
    $scope.setUpDetails = "";
    $scope.setUpCompleted = false;    

    $scope.showEditDetails = false;
    $scope.editDetails = "";
    $scope.editCompleted = false;

    $scope.showChangeDetails = false;
    $scope.changeDetails = "";
    $scope.changeCompleted = false;

    $scope.showPostDetails = false;
    $scope.postDetails = "";
    $scope.postCompleted = false;

    $scope.postDatabase = "finplus51";

    $scope.foundPayRuns = [];


    $scope.selectDatabase = function ()
    {
      console.log('selectDatabase', $scope.postDatabase);
    }

    $scope.selectPayRun = function ()
    {
      console.log('selectPayRun');
    }

  }

})();