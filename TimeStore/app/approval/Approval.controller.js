/* global _ */
(function () {
  "use strict";
  angular.module('timestoreApp')
      .controller('ApprovalController', ['$scope', 'timestoredata', '$routeParams',
          'commonFunctions', 'viewOptions', '$mdToast', Approval]);

  function Approval($scope, timestoredata, $routeParams, commonFunctions, viewOptions, $mdToast) {
    $scope.Title = '';
    $scope.timeData = [];
    $scope.filteredData = [];
    $scope.pagedData = [];
    $scope.myAccess = null;
    $scope.selectedGroup = null;
    $scope.previousSelection = null;
    $scope.currentPage = null;
    $scope.pageSize = 5;
    $scope.totalPages = null;
    $scope.showProgress = false;
    viewOptions.viewOptions.showSearch = false;
    viewOptions.viewOptions.share();
    $scope.ppdIndex = timestoredata.getPayPeriodIndex(moment($routeParams.payPeriod, 'YYYYMMDD'));
    //$scope.approvalType = $routeParams.approvalType.toUpperCase();
    //if ($routeParams.ppdIndex !== undefined) {
    //    $scope.ppdIndex = $routeParams.ppdIndex;
    //}
    timestoredata.getMyAccess().then(function (data) {
      $scope.myAccess = data;
    });

    $scope.RefreshApprovalData = function () {
      $scope.showProgress = true;
      $scope.Message = '';
      $scope.selectedGroup = null;
      $scope.filteredData = [];
      $scope.timeData = [];
      $scope.pagedData = [];
      $scope.Title = 'These Employees\' hours require Approval.';
      timestoredata.getInitiallyApproved($scope.ppdIndex).then(ProcessData, function () { });
      //timestoredata.getUncompletedApprovals($scope.ppdIndex).then(ProcessData, function () { });
    };

    function updateCalculatedTimeList(tc) {
      var j = 0;
      for (j = 0; j < tc.calculatedTimeList.length; j++) {
        tc.calculatedTimeList[j].approved = false;
      }
      for (j = 0; j < tc.timeList.length; j++) {
        tc.timeList[j].approved = false;
      }

    }

    $scope.RefreshApprovalData();

    $scope.toastPosition = {
      bottom: true,
      top: false,
      left: false,
      right: true
    };

    function ProcessData(data) {
      console.log('original approval data', data);
      data = _.filter(data, function (x) {
        return (x.Final_Approval_EmployeeID === 0 || x.Initial_Approval_EmployeeID === 0);
      });
      console.log('filtered approval data', data);
      $scope.timeData = data;
      for (var i = 0; i < $scope.timeData.length; i++) {
        updateCalculatedTimeList($scope.timeData[i]);
      }
      if (data.length === 0) {
        $scope.Message = 'Nothing to approve at this time.';
      }
      if ($scope.previousSelection !== null) {
        $scope.selectedGroup = $scope.previousSelection;
        $scope.groupSelected($scope.selectedGroup);
      }
      $scope.showProgress = false;
    }

    $scope.getGroups = function () {
      var groupArray = [' My Approvals'];

      angular.forEach($scope.timeData, function (item, idx) {
        if (item.GroupName.length > 0) {
          if (groupArray.indexOf(item.GroupName) === -1) {
            groupArray.push(item.GroupName);
          }
        }
      });
      return groupArray;
    };

    $scope.getDepts = function () {
      var groupArray = [];

      angular.forEach($scope.timeData, function (item, idx) {
        if (groupArray.indexOf(item.DepartmentDisplay) === -1) {
          groupArray.push(item.DepartmentDisplay);
        }
      });
      return groupArray;
    };

    $scope.loadTimeCard = function (eid) {
      var idx = getIndex($scope.filteredData, eid);
      var ppdIndex = $scope.ppdIndex;
      // Here we're going to check to see if we're on the first day of the new pay period.
      // If we are, we want to show them the data from the lay pay period.
      // We may choose to add in a time condition to this later.
      if ($scope.filteredData[idx].Days_Since_PPE === 1) {
        ppdIndex = -1;
      }
      if ($scope.filteredData[idx].showTimecard === false) {
        timestoredata.getEmployee(ppdIndex, $scope.filteredData[idx].employeeID)
            .then(function (data) {
              updateCalculatedTimeList(data);
              data.showTimecard = true;
              //if ($scope.approvalType === 'I') {
              //    data.Approved = (data.Approval_Level === 1);
              //} else {
              //    data.Approved = (data.Approval_Level === 2);
              //}
              data.Approved = (data.Approval_Level === 2);
              $scope.filteredData[idx] = data;
              $scope.timeData[getIndex($scope.timeData, data.employeeID)] = data; // update the main array with the new data.
              if ($scope.currentPage !== null) { // If we're paging the data, we want to update the main pagedData.
                $scope.pagedData[getIndex($scope.pagedData, data.employeeID)] = data;
              }
            }, function () { });
      }

      $scope.filteredData[idx].showTimecard = !$scope.filteredData[idx].showTimecard;
    };

    $scope.getApprovalText = function (al, l) {
      //t.Approval_Level < 1 ? 'Unapproved' : 'Needs Final Approval';
      if (!l) {
        return 'Leave Requires Approval';
      } else {
        return al < 1 ? 'Unapproved' : 'Needs Final Approval';
      }
    };

    $scope.groupSelected = function (group) {
      $scope.previousSelection = group;
      var groupArray = [];
      if (group === ' My Approvals') {
        var myAL = $scope.myAccess.Raw_Access_Type - 1; // We only want to see things that are one level below ours.
        angular.forEach($scope.timeData, function (item, idx) {
          if (item.Access_Type === myAL || item.Initial_Approval_EmployeeID_Access_Type === myAL || item.Reports_To === $scope.myAccess.EmployeeID) {
            item.showTimecard = false;
            item.Approved = false;
            groupArray.push(item);
          }
        });
      } else {
        angular.forEach($scope.timeData, function (item, idx) {
          if (item.DepartmentDisplay === group || item.GroupName === group) {
            item.showTimecard = false;
            item.Approved = false;
            groupArray.push(item);
          }
        });
      }
      groupArray = _.sortBy(groupArray, 'EmployeeDisplay');
      console.log('group array', groupArray);
      if (groupArray.length > $scope.pageSize) {
        $scope.currentPage = 1;
        $scope.totalPages = parseInt(groupArray.length / $scope.pageSize);
        if ((groupArray.length % $scope.pageSize) > 0) {
          $scope.totalPages++;
        }
        $scope.pagedData = groupArray;
        $scope.loadPage(0);
      } else {
        $scope.currentPage = null;
        $scope.totalPages = null;
        $scope.filteredData = groupArray;
      }

    };

    $scope.loadPage = function (pageIncrement) {
      $scope.currentPage += pageIncrement;
      if ($scope.currentPage > $scope.totalPages) {
        $scope.currentPage = $scope.totalPages;
      }

      var groupArray = [];
      var startIndex = ($scope.currentPage - 1) * $scope.pageSize;
      var tmp = $scope.pageSize;
      if (tmp + startIndex > $scope.pagedData.length) {
        tmp = $scope.pagedData.length - startIndex;
      }
      for (var i = 0; i < tmp; i++) {
        groupArray.push($scope.pagedData[i + (startIndex)]);
      }
      $scope.filteredData = groupArray;
    };


    $scope.CountByDeptAndGroup = function (dept, group) {
      var i = 0;
      angular.forEach($scope.timeData, function (item, idx) {
        if (item.GroupName === group && item.DepartmentName === dept) {
          i++;
        }
      });
      return i;
    };

    $scope.getTotalHours = function (wtl) {
      if ($scope.timeData === undefined) {
        return 0;
      } else {
        return commonFunctions.getTotalHours(wtl);
      }
      //var tl = wtl;
      //var total = 0;
      //for (var i = 0; i < tl.length; i++) {
      //    total += tl[i].hours;
      //}
      //return total;
    };

    $scope.approveTime = function (eid) {
      var idx = getIndex($scope.filteredData, eid);
      var tc = $scope.filteredData[idx];
      if (tc.ErrorList.length === 0) {
        var ad = {
          EmployeeID: tc.employeeID,
          PayPeriodStart: tc.payPeriodStart,
          WorkTypeList: tc.calculatedTimeList,
          Initial_Approval_By_EmployeeID: tc.Initial_Approval_EmployeeID
        };
        console.log('ad', ad);
        console.log('tc', tc);
        //if ($scope.approvalType === 'I') {
        //    timestoredata.approveInitial(ad)
        //        .then(function (data) {
        //            onApproval(data, idx);
        //        });
        //} else {
        //    timestoredata.approveFinal(ad)
        //        .then(function (data) {
        //            onApproval(data, idx);                        
        //        });
        //} 
        // removed 8/14/2015, they only use this function if they are approving through this
        // interface, and this interface will only let them approve if it is a final approval.
        timestoredata.approveFinal(ad)
            .then(function (data) {
              onApproval(data, idx);
            });
      } else {
        showToast('Unable to approve this Employee\'s time.  They have errors that must be resolved prior to approval.');
      }
    };

    function getIndex(dArray, employeeId) {
      for (var i = 0; i <= dArray.length; i++) {
        if (dArray[i].employeeID === employeeId) {
          return i;
        }
      }
      return;
    }

    function changeToApproved(idx) { // this function will set the approval level on the filterData object and then on the timeData object for that person.
      var origIdx = getIndex($scope.timeData, $scope.filteredData[idx].employeeID);
      var pageIdx = null;
      if ($scope.currentPage !== null) {
        pageIdx = getIndex($scope.pagedData, $scope.filteredData[idx].employeeID);
      }
      $scope.filteredData[idx].Approved = true;
      $scope.timeData[origIdx].Approved = true;
      //if ($scope.approvalType === 'I') {
      //    $scope.filteredData[idx].Approval_Level = 1;
      //    $scope.timeData[origIdx].Approval_Level = 1;
      //    if (pageIdx !== null) {
      //        $scope.pagedData[pageIdx].Approval_Level = 1;
      //    }
      //} else {                
      //    $scope.filteredData[idx].Approval_Level = 2;
      //    $scope.timeData[origIdx].Approval_Level = 2;
      //    if (pageIdx !== null) {
      //        $scope.pagedData[pageIdx].Approval_Level = 2;
      //    }                
      //}
      $scope.filteredData[idx].Approval_Level = 2;
      $scope.timeData[origIdx].Approval_Level = 2;
      if (pageIdx !== null) {
        $scope.pagedData[pageIdx].Approval_Level = 2;
      }
    }

    function onApproval(data, idx) {
      if (data.toUpperCase() === 'SUCCESS') {
        showToast('Successfully approved hours for ' + $scope.filteredData[idx].EmployeeDisplay);
        changeToApproved(idx);
      } else {
        var e = 'Unable to approve hours for ' + $scope.filteredData[idx].EmployeeDisplay + '. ';
        if (data === '"Error: Already approved."') {
          alert(e + ' This person has already been approved.');
          changeToApproved(idx);
        } else {
          alert(e + data);
        }
      }
    }

    function showToast(Message) {
      $mdToast.show(
        $mdToast.simple()
          .content(Message)
          .position($scope.getToastPosition())
          .hideDelay(6000)
      );
    }

    $scope.getToastPosition = function () {
      return Object.keys($scope.toastPosition)
        .filter(function (pos) { return $scope.toastPosition[pos]; })
        .join(' ');
    };
  }

})();