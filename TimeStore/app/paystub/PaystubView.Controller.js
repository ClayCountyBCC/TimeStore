(function ()
{
  "use strict";

  angular.module('timestoreApp')
    .controller('PaystubViewController',
      ['$scope', 'viewOptions', 'timestoredata', 'timestoreNav', 'paystub_list', '$routeParams', PaystubViewController]);


  function PaystubViewController($scope, viewOptions, timestoredata, timestoreNav, paystub_list, $routeParams)
  {
    $scope.checkNumber = "";
    $scope.filter_year = "";
    $scope.currentPaystub = null;
    function FilterPaystubList()
    {
      //console.log('paystub year', $scope.filter_year);
      if ($scope.filter_year.length === 0) return $scope.paystubList;
      
      var list = $scope.paystubList.filter(function (j) { return j.pay_stub_year.toString() === $scope.filter_year });
      return list;
    }
    //console.log('paystub list', paystub_list);
    $scope.employeeId = $routeParams.employeeId;

    $scope.paystubList = paystub_list;
    $scope.filtered_paystub_list = FilterPaystubList();
    
    $scope.paystub_years = [];
    $scope.paystubList.map(function (j)
    {
      if ($scope.paystub_years.indexOf(j.pay_stub_year.toString()) === -1)
      {
        $scope.paystub_years.push(j.pay_stub_year.toString());
      }
    });

    $scope.returnToTimeStore = function ()
    {
      timestoreNav.goDefaultEmployee($routeParams.employeeId);
    };

    if ($routeParams.checkNumber !== undefined && $routeParams.checkNumber !== null)
    {
      $scope.checkNumber = $routeParams.checkNumber;
    }
    else
    {
      $scope.checkNumber = $scope.paystubList[0].check_number;
    }

    if ($scope.checkNumber.length > 0)
    {
      LoadCheck();
    }
    else
    {
      // Work out no checks here
    }
              
    $scope.FormatDate = function (date)
    {
      if (date instanceof Date)
      {
        return date.toLocaleDateString('en-us');
      }
      var d = new Date(date);
      return d.toLocaleDateString('en-US');
    }

    $scope.selectYear = function ()
    {
      $scope.filtered_paystub_list = FilterPaystubList();
    }

    $scope.selectCheck = function ()
    {
      LoadCheck();
    }

    function LoadCheck()
    {
      timestoredata.getPayStubByEmployee($scope.employeeId, $scope.checkNumber)
        .then(function (paystub)
        {
          paystub.formatted_pay_period_ending = new Date(paystub.pay_period_ending).toLocaleDateString('en-us');
          paystub.formatted_pay_date = new Date(paystub.pay_date).toLocaleDateString('en-us');
          paystub.total_earnings_hours = paystub.earnings.reduce(function (a, b) { return a + b.hours; }, 0).toFixed(2).toString();
          paystub.total_earnings_amount = paystub.earnings.reduce(function (a, b) { return a + b.amount; }, 0).toFixed(2).toString();
          paystub.total_deductions_amount = paystub.deductions.reduce(function (a, b) { return a + b.amount }, 0).toFixed(2).toString();
          paystub.total_deductions_year_to_date = paystub.deductions.reduce(function (a, b) { return a + b.year_to_date_deductions }, 0).toFixed(2).toString();
          paystub.total_contributions = paystub.deductions.reduce(function (a, b) { return a + b.contributions }, 0).toFixed(2).toString();
          //console.log('paystub', paystub);
          $scope.currentPaystub = paystub;

        });
    }

  }

})();