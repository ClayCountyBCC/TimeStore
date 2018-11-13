/* global _ */
(function ()
{
  "use strict";

  angular.module('timestoreApp')
    .controller('FinanceToolsController', ['$scope', 'timestoredata', 'viewOptions', 'datelist', financeTools]);

  function financeTools($scope, timestoredata, viewOptions, datelist)
  {

    $scope.payperiodlist = datelist.getShortPayPeriodList();
    $scope.showProgress = false;
    $scope.postResult = '';
    $scope.selectedPayPeriod = '';
    viewOptions.viewOptions.showSearch = false;
    viewOptions.viewOptions.share();
    $scope.serverType = 'normal';
    $scope.PostToFinance = function ()
    {
      console.log('server Type', $scope.serverType);
      if ($scope.selectedPayPeriod === '')
      {
        alert('You must select a pay period');
      } else
      {
        $scope.showProgress = true;
        var m = moment($scope.selectedPayPeriod, 'M/D/YYYY');
        timestoredata.financePostProcess(timestoredata.getPayPeriodIndex(m), $scope.serverType)
          .then(onComplete);
      }
    };

    function onComplete(data)
    {
      $scope.showProgress = false;
      if (data.toUpperCase() === 'SUCCESS')
      {
        $scope.postResult = 'The Post to Finplus completed Successfully.';
      } else
      {
        $scope.postResult = data;
      }
    }

    //timestoredata.getPayPeriods().then(function (data) {
    //    $scope.payperiodlist = data;
    //}, function () {
    //});
  }

}());