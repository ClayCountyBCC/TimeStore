/* global moment, _ */
(function ()
{
  "use strict";
  angular.module('timestoreApp')
    .controller('leaveApprovalController', ['$scope', 'timestoredata',
      'viewOptions', LeaveApprovalController]);

  function LeaveApprovalController($scope, timestoredata, viewOptions)
  {
    $scope.filteredDataList = [];
    $scope.filteredDataByDateAndDeptList = [];
    $scope.dataList = [];
    $scope.deptList = [];
    $scope.filterStatus = 'undecided';
    $scope.filterDept = 'Select Dept';
    $scope.selectedIndex = -1;
    $scope.selectedId = -1;
    $scope.showProgress = false;
    $scope.approving = false;

    refresh();

    function filterData(data)
    {
      if ($scope.filterDept !== 'all')
      {
        data = _.filter(data, function (x)
        {
          return x.dept_id === $scope.filterDept;
        });
      }
      if ($scope.filterStatus !== 'all')
      {
        data = _.filter(data, function (x)
        {
          switch ($scope.filterStatus)
          {
            case 'undecided':
              return !x.Finalized;
            case 'approved':
              return x.Finalized && x.Approved;
            case 'denied':
              return x.Finalized && !x.Approved;
            default:
              return false;
          }
        });
      }
      return data;
    }

    function filterDataByDateAndDept(workdate, dept, current_id)
    {
      return _.filter($scope.dataList, function (x)
      {
        return x.work_date_display === workdate && x.dept_id === dept && x.approval_hours_id !== current_id;
      });
    }

    function refresh()
    {
      $scope.showProgress = true;
      timestoredata.getLeaveRequests()
        .then(processLeaveRequestData);
    }

    function resetDetail()
    {
      $scope.selectedIndex = -1;
      $scope.selectedId = -1;
      $scope.filteredDataByDateAndDeptList = [];
    }

    function processLeaveRequestData(data)
    {
      console.log('leave data', data);
      _.forEach(data.leaveData, function (l)
      {
        //l.work_date_display = moment(l.work_date).format('M/D/YYYY');
        l.approval_date_display = moment(l.date_approval_added).format('M/D/YYYY hh:mm A');
        l.showDetail = false;
        if ($scope.deptList.indexOf(l.dept_id) === -1)
        {
          $scope.deptList.push(l.dept_id);
        }
      });
      if ($scope.deptList.length === 1)
      {
        $scope.filterDept = 'all';
      }
      $scope.dataList = data.leaveData;
      $scope.filteredDataList = filterData($scope.dataList);
      $scope.showProgress = false;
    }

    $scope.finalizeLeaveRequest = function (approved, i)
    {
      $scope.approving = true;
      var n = $scope.filteredDataList[i];
      console.log('approved', approved, 'a id', n.approval_hours_id, 'note', n.note);

      timestoredata.finalizeLeaveRequest(n.employee_id, approved, n.approval_hours_id, n.note, n.hours_used, n.work_date_display)
        .then(onFinalizeSuccess, onFinalizeError);
    };

    function onFinalizeSuccess(response)
    {
      $scope.closeDetail();
      refresh();
      $scope.approving = false;
    }

    function onFinalizeError(response)
    {
      var m = '';
      switch (response.status)
      {
        case 403:
          m = 'You do not have access to approve this person\'s leave.';
          break;
        case 500:
          m = 'There was a problem saving your request.  Please try again and contact MIS if the problem persists.';
          break;
        case 501:
          m = 'The hours you are trying to approve have changed.  Please refresh the data and try again.';
          break;
      }
      $scope.approving = false;
      alert(m);
    }

    $scope.refreshData = function ()
    {
      refresh();
    };

    $scope.showDetail = function (i)
    {
      resetDetail();
      _.forEach($scope.filteredDataList, function (x)
      {
        x.showDetail = false;
      });
      $scope.filteredDataList[i].showDetail = true;
      console.log('leave data', $scope.filteredDataList[i]);
      if ($scope.filteredDataList[i].showDetail)
      {
        $scope.selectedIndex = i;
        var n = $scope.filteredDataList[i];
        $scope.selectedId = n.approval_hours_id;
        $scope.filteredDataByDateAndDeptList = filterDataByDateAndDept(n.work_date_display, n.dept_id, n.approval_hours_id);
      }
    };

    $scope.closeDetail = function ()
    {
      resetDetail();
      _.forEach($scope.filteredDataList, function (x)
      {
        x.showDetail = false;
      });
    };

    $scope.updateFilter = function ()
    {
      $scope.showProgress = true;
      //console.log('filter status', $scope.filterStatus);
      $scope.filteredDataList = filterData($scope.dataList);
      $scope.showProgress = false;
    };
  }

})();