/* global _ */
(function () {
  "use strict";
  angular.module('timestoreApp')
      .controller('TimeCardViewController', ['$scope', '$routeParams', 'timestoredata',
                                              'viewOptions', 'timecard', TimeCardViewController]);

  function TimeCardViewController($scope, $routeParams, timestoredata, viewOptions, timecard) {

    $scope.timecard = timecard;
    updateCalculatedTimeList();
    viewOptions.viewOptions.showSearch = true;
    viewOptions.viewOptions.share();

    function updateCalculatedTimeList() {
      var i = 0;
      for (i = 0; i < $scope.timecard.calculatedTimeList.length; i++) {
        $scope.timecard.calculatedTimeList[i].approved = false;
      }
      for (i = 0; i < $scope.timecard.timeList.length; i++) {
        $scope.timecard.timeList[i].approved = false;
      }

    }

    $scope.$on('shareApprovalUpdated', function () {
      if (viewOptions.approvalUpdated.approvalUpdated) {
        console.log('shareapprovalupdated');
        var ppi = timestoredata.getPayPeriodIndex(moment($routeParams.payPeriod, 'YYYYMMDD'));
        timestoredata.getEmployee(ppi, $routeParams.employeeId)
          .then(onEmployee, onError);
      }
    });

    var onError = function (reason) {
      console.log('Error:' + reason);
    };

    var onEmployee = function (data) {
      console.log('updating employee after approval updated', data);
      $scope.timecard = data;
      updateCalculatedTimeList();
      console.log('timecard data', $scope.timecard);
      viewOptions.timecardReloaded.share()
    };


  }
}());
