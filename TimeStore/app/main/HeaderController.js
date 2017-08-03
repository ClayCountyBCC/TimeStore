﻿
(function () {
  "use strict";
  angular.module('timestoreApp')
      .controller('HeaderController', ['$scope', '$mdSidenav', '$routeParams',
                                      'timestoredata', 'viewOptions', 'timestoreNav', Header]);

  function Header($scope, $mdSidenav, $routeParams, timestoredata,
                  viewOptions, timestoreNav) {

    $scope.hideToolbar = false;
    $scope.myAccess = null;
    $scope.employeelist = [];
    $scope.employeeid = $routeParams.employeeId;
    $scope.employeeSearchText = null;
    $scope.employee = null;
    $scope.isDev = true;
    $scope.initiallyApproved = [];

    $scope.$on('shareShowSearch', function () {
      $scope.showSearch = viewOptions.viewOptions.showSearch;
    });

    $scope.$on('hideToolbar', function () {
      $scope.hideToolbar = true;
    });

    $scope.querySearch = function (query, aMap) {
      var results = query ? aMap.filter(createFilterFor(query)) : [];
      return results;
    };

    function createFilterFor(query) {
      var lowercaseQuery = angular.lowercase(query);
      return function filterFn(item) {
        return (item.displayLower.indexOf(lowercaseQuery) >= 0);
      };
    }

    timestoredata.getEmployees().then(function (data) {
      $scope.employeelist = data;
      $scope.employeeMapV = $scope.employeelist.map(function (employeeMap) {
        return {
          value: employeeMap,
          displayLower: angular.lowercase(employeeMap.EmployeeDisplay),
          display: employeeMap.EmployeeDisplay,
          Terminated: employeeMap.Terminated,
          TerminationDateDisplay: employeeMap.TerminationDateDisplay
        };
      });
    }, function () { });


    timestoredata.getMyAccess().then(function (data) {
      $scope.myAccess = data;

    });

    $scope.selectedEmployeeChanged = function (employee) {
      if (employee === undefined || employee === null) {
        $scope.employeeid = timestoredata.getDefaultEmployeeId();
      } else {
        $scope.employeeid = employee.value.EmployeeID;
        timestoreNav.goDefaultEmployee($scope.employeeid);
      }
    };

    $scope.openAdminMenu = function () {
      $mdSidenav('adminRight').toggle();
    };

    $scope.openApprovalMenu = function () {
      $mdSidenav('approvalRight').toggle();
    };

    $scope.viewIncentives = function (incentiveType) {
      $mdSidenav('adminRight').toggle();
      timestoreNav.goIncentives(incentiveType);
    };

    $scope.viewUnapproved = function () {
      $mdSidenav('adminRight').toggle();
      timestoreNav.goUnapproved();
    }

    $scope.viewExceptions = function () {
      $mdSidenav('approvalRight').toggle();
      timestoreNav.goExceptions();
    };

    $scope.viewHome = function () {
      timestoreNav.goHome();
    };

    $scope.viewCalendar = function () {
      timestoreNav.goCalendar();
    }

    $scope.viewLeaveRequests = function () {
      timestoreNav.goLeaveRequest();
    }

    $scope.viewDailyCheckoff = function () {
      $mdSidenav('approvalRight').toggle();
      timestoreNav.goDailyCheckoff();
    };

    $scope.viewSignatureRequired = function () {
      $mdSidenav('approvalRight').toggle();
      timestoreNav.goSignatureRequired();
    }

    $scope.ViewAccessMenu = function () {
      $mdSidenav('adminRight').toggle();
      timestoreNav.goAccessChange();
    };

    $scope.viewFinanceTools = function () {
      $mdSidenav('adminRight').toggle();
      timestoreNav.goFinanceTools();
    };

    $scope.viewFinalApprovals = function () {
      $mdSidenav('approvalRight').toggle();
      timestoreNav.goTimecardApprovals();
    };

    $scope.viewLeaveApproval = function () {
      $mdSidenav('approvalRight').toggle();
      timestoreNav.goLeaveApprovals();
    };


  };


}());