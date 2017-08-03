/* global _, moment */
(function () {
    "use strict";
    angular.module('timestoreApp')
        .controller('ExceptionsController', ['$scope', 'timestoredata', 'viewOptions', '$routeParams', 'timestoreNav', ExceptionsController]);

    function ExceptionsController($scope, timestoredata, viewOptions, $routeParams, timestoreNav) {

        $scope.showProgress = false;
        $scope.showOptions = true;
        $scope.filterExceptions = 'both';
        $scope.ppdIndex = timestoredata.getPayPeriodIndex(moment($routeParams.payPeriod, 'YYYYMMDD'));
        $scope.ppd = $routeParams.payPeriod;
        $scope.viewData = [];
        $scope.deptData = [];
        $scope.selectedDept = '';
        viewOptions.viewOptions.showSearch = false;
        viewOptions.viewOptions.share();

        
        $scope.RefreshApprovalData = function () {
            $scope.showProgress = true;
            $scope.viewData = [];
            timestoredata.getInitiallyApproved($scope.ppdIndex).then(ProcessData, function () { });
        };

        $scope.RefreshApprovalData();


        $scope.filterByType = function () {
            return returnByType($scope.viewData, $scope.filterExceptions, $scope.selectedDept);
        }

        function returnByType(data, type, dept) {
            return _.filter(data, function (d) {
                if (type !== 'both' && dept.length > 0) {
                    return d.exceptionType === type && d.departmentId === dept;
                } else if (type !== 'both') {
                    return d.exceptionType === type;
                } else if (dept.length > 0) {
                    return d.departmentId === dept;
                } else {
                    return true;
                }
            });
        }

        function ProcessData(data) {
            console.log('exception data', data);
            _.each(data, findExceptions);
            sortData();
            $scope.showProgress = false;
        }
        function sortData() {
            $scope.viewData = _.sortByAll($scope.viewData, ['departmentId', 'fullname']);
        }

        function findExceptions(tc) {            
            var eid = tc.employeeID;
            var did = tc.departmentNumber;
            var fn = tc.EmployeeDisplay;
            var dept = tc.DepartmentDisplay;
            var eType = '';
            _.each(tc.WarningList, function (message) {
                eType = 'Warning';
                addDept(dept, did);
                addException(eid, did, fn, eType, message);
            });
            _.each(tc.ErrorList, function (message) {
                eType = 'Error';
                addDept(dept, did);
                addException(eid, did, fn, eType, message);
            });
        }

        function addException(eId, dId, fn, eType, message) {
            var e = {
                employeeId: eId,
                fullname: fn,
                departmentId: dId,
                exceptionType: eType,
                exception: message
            };
            $scope.viewData.push(e);
        }

        $scope.viewTimecard = function (eid, ppd) {
            timestoreNav.goEmployeeByPPD(eid, ppd);
        };


        $scope.toggleShowOptions = function () {
            $scope.showOptions = !$scope.showOptions;
        };

        function addDept(dept, dId) {
            var x = _.findIndex($scope.deptData, function(d) {
                return d.deptId === dId;
            });
            if (x === -1) {
                $scope.deptData.push({deptName: dept, deptId: dId});
            }
        };

    }

})();