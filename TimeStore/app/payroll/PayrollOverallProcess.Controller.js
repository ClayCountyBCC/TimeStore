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
    $scope.StartOrResetInProgress = false;
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

    $scope.foundPayRuns = [];

    $scope.ResetPayroll = function ()
    {
      $scope.StartOrResetInProgress = true;
      timestoredata.resetPayroll($scope.pay_period_ending)
        .then(function (data)
        {
          HandleCurrentStatus(data);
          $scope.StartOrResetInProgress = false;
        });
    }

    $scope.EditsCompleted = function ()
    {
      timestoredata.editsCompleted($scope.pay_period_ending)
        .then(function (data)
        {
          HandleCurrentStatus(data);
        });
    }

    $scope.ChangesApproved = function ()
    {
      timestoredata.changesApproved($scope.pay_period_ending)
        .then(function (data)
        {
          HandleCurrentStatus(data);
        });
    }

    $scope.CancelApproval = function ()
    {
      timestoredata.cancelApproval($scope.pay_period_ending)
        .then(function (data)
        {
          HandleCurrentStatus(data);
        });
    }

    $scope.MarkEditsIncomplete = function ()
    {
      timestoredata.editsInComplete($scope.pay_period_ending)
        .then(function (data)
        {
          HandleCurrentStatus(data);
        });
    }

    $scope.StartPayroll = function ()
    {
      $scope.StartOrResetInProgress = true;
      var target = $scope.currentStatus.target_db;
      if (target !== "1" && target !== "0")
      {
        alert("Missing Target Database selection.");
        $scope.StartOrResetInProgress = false;
        return false;
      }
      timestoredata.startPayroll($scope.pay_period_ending, $scope.currentStatus.include_benefits, $scope.currentStatus.target_db)
        .then(function (data)
        {
          HandleCurrentStatus(data);
          $scope.StartOrResetInProgress = false;
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