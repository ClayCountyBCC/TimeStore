
(function ()
{
  "use strict";
  angular.module('timestoreApp')
    .controller('HeaderController', ['$scope', '$mdSidenav', '$routeParams',
      'timestoredata', 'viewOptions', 'timestoreNav', Header]);

  function Header($scope, $mdSidenav, $routeParams, timestoredata,
    viewOptions, timestoreNav)
  {
    $scope.hideToolbar = false;
    $scope.myAccess = null;
    $scope.employeelist = [];
    $scope.employeeid = $routeParams.employeeId;
    $scope.employeeSearchText = null;
    $scope.employee = null;
    $scope.isDev = true;
    $scope.initiallyApproved = [];
    if ($scope.employeeid === undefined || $scope.employeeid === null)
    {
      timestoredata.getDefaultEmployeeId()
        .then(function (eid)
        {
          $scope.employeeid = eid;
        });
    }
    console.log('routeparams employeeid', $routeParams.employeeId);
    console.log('scope employeeid', $scope.employeeid);

    $scope.$on('employeeChange'), function ()
    {
      $scope.employeeid = $routeParams.employeeId;
    }

    $scope.$on('shareShowSearch', function ()
    {
      $scope.showSearch = viewOptions.viewOptions.showSearch;
    });

    $scope.$on('hideToolbar', function ()
    {
      $scope.hideToolbar = true;
    });

    $scope.querySearch = function (query, aMap)
    {
      var results = query ? aMap.filter(createFilterFor(query)) : [];
      return results;
    };

    function createFilterFor(query)
    {
      var lowercaseQuery = angular.lowercase(query);
      return function filterFn(item)
      {
        return item.displayLower.indexOf(lowercaseQuery) >= 0;
      };
    }

    timestoredata.getEmployees().then(function (data)
    {
      $scope.employeelist = data;
      $scope.employeeMapV = $scope.employeelist.map(function (employeeMap)
      {
        return {
          value: employeeMap,
          displayLower: angular.lowercase(employeeMap.EmployeeDisplay),
          display: employeeMap.EmployeeDisplay,
          Terminated: employeeMap.Terminated,
          TerminationDateDisplay: employeeMap.TerminationDateDisplay
        };
      });
    }, function () { });


    timestoredata.getMyAccess().then(function (data)
    {
      console.log("my access", data);
      $scope.myAccess = data;

    });

    $scope.selectedEmployeeChanged = function (employee)
    {
      if (employee === undefined || employee === null)
      {
        $scope.employeeid = timestoredata.getDefaultEmployeeId();
      } else
      {
        $scope.employeeid = employee.value.EmployeeID;
        timestoreNav.goDefaultEmployee($scope.employeeid);
      }
    };

    $scope.openPasswordExpirationMenu = function ()
    {
      $mdSidenav('passwordExpiring').toggle();
    };

    $scope.openAdminMenu = function ()
    {
      $mdSidenav('adminRight').toggle();
    };

    $scope.openApprovalMenu = function ()
    {
      $mdSidenav('approvalRight').toggle();
    };

    $scope.viewIncentives = function (incentiveType)
    {
      $mdSidenav('adminRight').toggle();
      timestoreNav.goIncentives(incentiveType);
    };

    $scope.viewUnapproved = function ()
    {
      $mdSidenav('adminRight').toggle();
      timestoreNav.goUnapproved();
    };

    $scope.viewTimeclockData = function ()
    {
      $mdSidenav('approvalRight').toggle();
      timestoreNav.goTimeclockView();
    };

    $scope.viewFema = function ()
    {
      $mdSidenav('adminRight').toggle();
      timestoreNav.goFema();
    };

    $scope.viewExceptions = function ()
    {
      $mdSidenav('approvalRight').toggle();
      timestoreNav.goExceptions();
    };

    $scope.viewHome = function ()
    {
      timestoreNav.goHome();
    };

    $scope.viewCalendar = function ()
    {
      timestoreNav.goCalendar();
    };

    $scope.viewLeaveRequests = function ()
    {
      timestoreNav.goLeaveRequest();
    };

    $scope.viewPaystubs = function ()
    {
      var eid = $scope.employeeid;
      if (eid === null || eid === undefined)
      {

        timestoredata.getDefaultEmployeeId()
          .then(function (defaulteid)
          {
            $scope.employeeid = defaulteid;
            timestoreNav.goPaystub(defaulteid);
          });

      }
      else
      {
        timestoreNav.goPaystub(eid);
      }
      
    };

    $scope.viewDailyCheckoff = function ()
    {
      $mdSidenav('approvalRight').toggle();
      timestoreNav.goDailyCheckoff();
    };

    $scope.viewSignatureRequired = function ()
    {
      $mdSidenav('approvalRight').toggle();
      timestoreNav.goSignatureRequired();
    };

    $scope.ViewAccessMenu = function ()
    {
      $mdSidenav('adminRight').toggle();
      timestoreNav.goAccessChange();
    };

    $scope.viewFinanceTools = function ()
    {
      $mdSidenav('adminRight').toggle();
      timestoreNav.goFinanceTools();
    };

    $scope.viewPayrollProcess = function ()
    {
      $mdSidenav('adminRight').toggle();
      timestoreNav.goPayrollOverallProcess();
    };

    $scope.viewFinalApprovals = function ()
    {
      $mdSidenav('approvalRight').toggle();
      timestoreNav.goTimecardApprovals();
    };

    $scope.viewLeaveApproval = function ()
    {
      $mdSidenav('approvalRight').toggle();
      timestoreNav.goLeaveApprovals();
    };

    $scope.switchUser = function ()
    {
      console.log('header controller switch user');
      timestoredata.getDefaultEmployeeId()
        .then(function (data)
        {
          console.log('default employeeid', data);
          $scope.employeeid = data;
          console.log('header controller switch user inside');
          timestoreNav.goSwitchUser();
          //href = "#/switchuser"
          //timestoreNav.goDefaultEmployee(data);
        });

    }


  }


}());