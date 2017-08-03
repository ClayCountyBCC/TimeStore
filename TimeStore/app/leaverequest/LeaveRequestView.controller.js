
(function () {
  "use strict";

  angular.module('timestoreApp')
      .controller('leaveRequestViewController', ['$scope', 'viewOptions', 'timestoredata', 'timestoreNav', LeaveRequestViewController]);

  function LeaveRequestViewController($scope, viewOptions, timestoredata, timestoreNav) {
    viewOptions.viewOptions.showSearch = false;
    viewOptions.viewOptions.share();
    $scope.leaveRequestListType = 'short';
    $scope.leaveRequests = [];
    $scope.futureLeaveRequests = [];
    $scope.allLeaveRequests = [];
    $scope.selectedDate = null;
    var m = moment(timestoredata.getPayPeriodStart(), "YYYYMMDD");//.add(14, 'days');
    $scope.minDate = m.clone().toDate();
    $scope.maxDate = m.clone().add(1, 'years').toDate();
    updateLeaveRequests();

    $scope.goHome = function () {
      timestoreNav.goHome();
    }

    $scope.refreshData = function () {
      updateLeaveRequests();
    }

    $scope.leaveDateSelected = function () {
      var d = moment($scope.selectedDate).format('M/D/YYYY');
      timestoreNav.goAddTime($scope.employeeId, d);
    }

    $scope.switchData = function () {
      if ($scope.leaveRequestListType === 'full') {
        $scope.leaveRequests = $scope.allLeaveRequests;
      } else {
        $scope.leaveRequests = $scope.futureLeaveRequests;
      }
    }

    function updateLeaveRequests() {
      timestoredata.getDefaultEmployeeId().then(function (data) {
        $scope.employeeId = data;

        timestoredata.getLeaveRequestsByEmployee($scope.employeeId)
            .then(function (data) {
              console.log('current leave data', data);
              _.forEach(data.leaveData, function (l) {
                l.work_date_display = moment(l.work_date).format('M/D/YYYY');
                l.approval_date_display = moment(l.date_approval_added).format('M/D/YYYY hh:mm A');
              });
              $scope.futureLeaveRequests = data.leaveData;
              $scope.leaveRequests = $scope.futureLeaveRequests;
            });
        timestoredata.getAllLeaveRequestsByEmployee($scope.employeeId)
            .then(function (data) {
              _.forEach(data.leaveData, function (l) {
                l.work_date_display = moment(l.work_date).format('M/D/YYYY');
              });
              $scope.allLeaveRequests = data.leaveData;
            });
      });

    }

  }


})();