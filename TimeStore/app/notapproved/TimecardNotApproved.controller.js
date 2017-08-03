/* global _ */
(function () {
    "use strict";
    angular.module('timestoreApp')
        .controller('TimecardNotApprovedController',
        ['$scope', 'viewOptions', 'timestoredata', '$routeParams', TimecardNotApproved]);

    function TimecardNotApproved($scope, viewOptions, timestoredata, $routeParams) {

        $scope.showProgress = false;
        $scope.ppd = $routeParams.payPeriod;
        resetDisplayAndData();
        viewOptions.viewOptions.showSearch = false;
        viewOptions.viewOptions.share();
        $scope.ppdIndex = timestoredata.getPayPeriodIndex(moment($routeParams.payPeriod, 'YYYYMMDD'));

        function resetDisplayAndData() {
            $scope.deptData = [{ deptName: '*** ALL DEPARTMENTS', deptId: '' }];
            $scope.viewData = [];
            $scope.filteredData = [];
            $scope.selectedDept = $scope.deptData[0];
        }

        $scope.RefreshApprovalData = function () {
            $scope.showProgress = true;            
            resetDisplayAndData();
            timestoredata.getUncompletedApprovals($scope.ppdIndex)
                .then(ProcessData, function () { });
        };

        $scope.RefreshApprovalData();
        
        function ProcessData(data) {
            _.each(data, collectNames);
            $scope.showProgress = false;
        }

        function collectNames(tc) {
            addDept(tc.DepartmentDisplay, tc.departmentNumber);
            $scope.viewData.push(unApproved(tc));
            $scope.filterByType();
        }
        
        $scope.filterByType = function () {
            $scope.filteredData = returnByType($scope.viewData, $scope.selectedDept.deptId);
        }

        function returnByType(data, dept) {
            return _.filter(data, function (d) {
                if (dept.length > 0) {
                    return d.department === dept;
                } else {
                    return true;
                }
            });
        }

        function unApproved(tc) {
            return {
                employeeId: tc.employeeID,
                fullname: tc.EmployeeDisplay,
                department: tc.departmentNumber,
                approvalinfo: tc.Initial_Approval_By.length > 0 ? 'Initial Approval By: ' + tc.Initial_Approval_By : ''
            }
        }

        function addDept(dept, dId) {
            var x = _.findIndex($scope.deptData, function (d) {
                return d.deptId === dId;
            });
            if (x === -1) {
                $scope.deptData.push({ deptName: dept, deptId: dId });
            }
        };


    }


})();