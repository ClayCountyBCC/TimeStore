/* global moment, _ */
(function ()
{
  "use strict";
  angular
    .module("timestoreApp")
    .directive("payrollEditGroup", function ()
    {
      return {
        restrict: "E",
        scope: {       
          ped: "=",
          paycodes: "<",
          projectcodes: "<"
          //event: "=",
          //fulltimelist: "<",
          //eventerror: "=",
          //calc: "&"
        },
        templateUrl: "PayrollEditGroup.directive.tmpl.html",
        controller: "PayrollEditGroupDirectiveController"
      };
    })
    .controller("PayrollEditGroupDirectiveController", ["$scope", "$routeParams", "timestoredata", "$mdDialog", '$anchorScroll', PayrollEditGroupController]);

  function PayrollEditGroupController($scope, $routeParams, timestoredata, $mdDialog, $anchorScroll)
  {
    $scope.payPeriod = $routeParams.payPeriod;
    $scope.pay_period_ending = moment($routeParams.payPeriod, "YYYYMMDD").format("MM/DD/YYYY");    
    $scope.storeYOffset = 0;


    $scope.GetTotalHours = function (paydata)
    {
      let totalHours = paydata.reduce(function (j, v) { return j + v.hours;  }, 0);
      //console.log('total hours test', totalHours)
      return totalHours.toFixed(2);
    }

    $scope.GetTotalAmount = function (paydata)
    {
      let totalAmount = paydata.reduce(function (j, v) { return j + v.amount; }, 0);
      //console.log('total amount test', totalAmount)
      return totalAmount.toFixed(2);
    }

    $scope.ShowEdit = function (ev)
    {
      $scope.storeYOffset = $anchorScroll.yOffset;
      $anchorScroll.yOffset = 0;
      $anchorScroll();
      $mdDialog.show({
        locals: {
          edit_data: $scope.ped,
          paycodes: $scope.paycodes,
          projectcodes: $scope.projectcodes
        },
        parent: angular.element(document.body),
        targetEvent: ev,
        fullscreen: true,
        clickOutsideToClose: false,
        templateUrl: 'PayrollEditDialog.tmpl.html',
        controller: 'PayrollEditDialogController'
      }).then(function ()
      {
        console.log('after save clicked ped', $scope.ped);
        timestoredata.postPayrollChanges($scope.pay_period_ending, $scope.ped.payroll_change_data)
          .then(function (data)
          {
            console.log('postpayrollchange data', data);
          });
        document.getElementById("editgroup" + $scope.ped.employee.EmployeeId.toString()).scrollIntoView();
      }, function ()
      {
        document.getElementById("editgroup" + $scope.ped.employee.EmployeeId.toString()).scrollIntoView();
      });
    }

  }
})();
