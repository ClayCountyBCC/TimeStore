/* global _, moment */
(function ()
{
  "use strict";
  angular
    .module("timestoreApp")
    .controller("AddTimeViewController", [
      "$scope",
      "viewOptions",
      "timestoredata",
      "$routeParams",
      "timecard",
      "timestoreNav",
      AddTimeView
    ]);

  function AddTimeView(
    $scope,
    viewOptions,
    timestoredata,
    $routeParams,
    timecard,
    timestoreNav
  )
  {
    $scope.timecard = timecard;
    $scope.leaveRequests = [];
    console.log("addtimechoice timecard", $scope.timecard);

    $scope.returnToTimeStore = function ()
    {
      timestoreNav.goDefaultEmployee($routeParams.employeeId);
    };
    $scope.updateLeaveRequests = function ()
    {
      updateLeaveRequests();
    };

    function updateLeaveRequests()
    {
      timestoredata
        .getLeaveRequestsByEmployee($routeParams.employeeId)
        .then(function (data)
        {
          //_.forEach(data.leaveData, function (l)
          //{
          //  l.work_date_display = moment(l.work_date).format("M/D/YYYY");
          //});
          $scope.leaveRequests = data.leaveData;
        });
    }
    //$scope.employeeId = $routeParams.employeeId;
    //$scope.workDate = $routeParams.workDate

    //if ($scope.workDate === undefined) {
    //    $scope.workDate = moment().format('YYYYMMDD');
    //}
    //console.log($scope.workDate);
    // We should change this line to consider the day their requesting,
    // ie: if they are in next pay period, switch to it.
    //timestoredata.getEmployee(timestoredata.getPayPeriodIndex(moment($scope.workDate, 'YYYYMMDD')), $scope.employeeId)
    //    .then(onEmployee, onError);
    $scope.$on("shareApprovalUpdated", function ()
    {
      if (viewOptions.approvalUpdated.approvalUpdated)
      {
        var ppi = timestoredata.getPayPeriodIndex(
          moment($routeParams.payPeriod, "YYYYMMDD")
        );
        var eid = $routeParams.employeeId;
        console.log("ppi", ppi, "eid", eid);
        timestoredata.getEmployee(ppi, eid).then(onEmployee, onError);
        //updateLeaveRequests();
        //.then(viewOptions.timecardReloaded.share());
      }
    });

    $scope.$on("leaveRequestUpdated", function ()
    {
      updateLeaveRequests();
    });

    function onEmployee(data)
    {
      $scope.timecard = data;
      //$scope.TCTD.DepartmentNumber = $scope.timecard.departmentNumber;
      //populateDateList($scope.timecard.payPeriodStart);
      //handleClassifications($scope.timecard);
      //handleCompTime($scope.timecard);
      console.log("timecard data", $scope.timecard);
      //$scope.requestingData = false;
    }

    function onError(reason)
    {
      console.log("Error:" + reason);
      //$scope.requestingData = false;
    }
  }
})();
