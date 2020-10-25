﻿/* global moment, _ */

(function ()
{
  "use strict";

  angular.module('timestoreApp')
    .controller('PayrollOverallController',
      ['$scope', 'viewOptions', 'timestoredata', 'timestoreNav', '$routeParams', PayrollOverallController]);


  function PayrollOverallController($scope, viewOptions, timestoredata, timestoreNav, $routeParams)
  {
    
    console.log('payroll overall process');
    $scope.payruns = [];
    $scope.postPayrun = "";
    $scope.pay_period_ending = moment($routeParams.payPeriod, "YYYYMMDD").format("MM/DD/YYYY");    
    $scope.StartOrResetInProgress = false;
    $scope.UpdateLockInProgress = false;
    $scope.payroll_lock = {};

    $scope.PostPayrollLock = function ()
    {
      if ($scope.payroll_lock === null) return;
      $scope.UpdateLockInProgress = true;
      $scope.payroll_lock.lock_date = $scope.payroll_lock.lock_date_display;
      timestoredata.postPayrollLock($scope.payroll_lock)
        .then(function ()
        {
          $scope.GetPayrollLock();
          $scope.UpdateLockInProgress = false;
        }, function ()
        {
            console.log('error saving payroll lock');
            $scope.UpdateLockInProgress = false;
        });
    }

    $scope.GetPayrollLock = function ()
    {
      timestoredata.getPayrollLock($scope.pay_period_ending)
        .then(function (data)
        {
          data.created_on_display = formatDatetime(data.created_on)
          data.default_lock_date_display = formatDate(data.default_lock_date);
          data.lock_date_display = formatDate(data.lock_date);
          console.log("payroll lock", data);          
          $scope.payroll_lock = data;          
        }, function ()
        {
            console.log('error getting payroll lock');
        });
    }

    $scope.PostTimestoreData = function ()
    {
      $scope.StartOrResetInProgress = true;
      timestoredata.postTimestoreData($scope.pay_period_ending, $scope.postPayrun)
        .then(function (data)
        {
          HandleCurrentStatus(data);
          $scope.StartOrResetInProgress = false;
        }, function ()
        {
          $scope.StartOrResetInProgress = false;
        });
    };

    $scope.ResetPayroll = function ()
    {
      $scope.StartOrResetInProgress = true;
      timestoredata.resetPayroll($scope.pay_period_ending)
        .then(function (data)
        {
          HandleCurrentStatus(data);
          $scope.StartOrResetInProgress = false;
        }, function ()
        {
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

    $scope.GetPayruns = function()
    {
      timestoredata.getPayruns($scope.pay_period_ending)
        .then(function (data)
        {
          $scope.payruns = data;
        })
    }

    $scope.GetPayrollLock();
    $scope.GetPayruns();

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

    function formatDate(d)
    {
      return d ? new Date(d).toLocaleDateString('en-us') : "";
    }

    function formatDatetime(d)
    {
      return d ? new Date(d).toLocaleString('en-us') : "";
    }
  }

})();