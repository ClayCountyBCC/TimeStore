/* global moment, _ */

(function ()
{
  "use strict";

  angular.module('timestoreApp')
    .controller('PayrollOverallController',
      ['$scope', 'viewOptions', 'timestoredata', 'timestoreNav', '$routeParams', PayrollOverallController]);


  function PayrollOverallController($scope, viewOptions, timestoredata, timestoreNav, $routeParams)
  {
    
    console.log('payroll overall process');
    $scope.pay_period_ending = moment($routeParams.payPeriod, "YYYYMMDD").format("MM/DD/YYYY");
    $scope.includeBenefits = true;
    //$scope.showSetUpDetails = false;
    //$scope.setUpDetails = "";
    //$scope.setUpCompleted = false;    

    //$scope.showEditDetails = false;
    //$scope.editDetails = "";
    //$scope.editCompleted = false;

    //$scope.showChangeDetails = false;
    //$scope.changeDetails = "";
    //$scope.changeCompleted = false;

    //$scope.showPostDetails = false;
    //$scope.postDetails = "";
    //$scope.postCompleted = false;

    $scope.postDatabase = "";

    $scope.foundPayRuns = [];

    $scope.ResetPayroll = function ()
    {
      console.log('include benefits', $scope.includeBenefits);
      timestoredata.resetPayroll($scope.pay_period_ending, $scope.includeBenefits)
        .then(function (data)
        {
          HandleCurrentStatus(data);
        });
    }

    $scope.StartPayroll = function ()
    {
      timestoredata.startPayroll($scope.pay_period_ending, $scope.includeBenefits)
        .then(function (data)
        {
          HandleCurrentStatus(data);
        });
    }

    $scope.ViewEdits = function ()
    {
      timestoreNav.goPayrollEditProcess($routeParams.payPeriod);
    }

    $scope.ViewChanges = function ()
    {
      timestoreNav.goPayrollReviewProcess($routeParams.payPeriod);
    }

    $scope.PostData = function ()
    {

    }

    GetPayrollStatus()

    $scope.selectDatabase = function ()
    {
      console.log('selectDatabase', $scope.postDatabase);
    }

    $scope.selectPayRun = function ()
    {
      console.log('selectPayRun');
    }

    function GetPayrollStatus()
    {
      timestoredata.getPayrollStatus($scope.pay_period_ending)
        .then(function (data)
        {
          HandleCurrentStatus(data);          
        });
    }

    function HandleCurrentStatus(data)
    {
      if (!data)
      {
        console.log('payroll status is invalid');
        return;
      }
      $scope.currentStatus = UpdateDisplays(data);
      console.log('payroll status', $scope.currentStatus);
    }    

    function UpdateDisplays(data)
    {
      data.started_on_display = formatDatetime(data.started_on);
      data.edits_completed_on_display = formatDatetime(data.edits_completed_on);
      data.edits_approved_on_display = formatDatetime(data.edits_approved_on);
      data.finplus_updated_on_display = formatDatetime(data.finplus_updated_on);
      return data;
    }

    function formatDatetime(d)
    {
      return d ? new Date(d).toLocaleString('en-us') : "";
    }
  }

})();