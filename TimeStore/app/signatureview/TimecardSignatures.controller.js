/* global _ */
(function () {
  "use strict";
  angular.module('timestoreApp')
      .controller('SignatureViewController', ['$scope', 'viewOptions', 'timestoredata', '$routeParams', TimecardSignature]);

  function TimecardSignature($scope, viewOptions, timestoredata, $routeParams) {

    $scope.showProgress = true;
    $scope.Message = '';
    $scope.timeData = [];
    viewOptions.viewOptions.showSearch = false;
    viewOptions.viewOptions.share();
    //viewOptions.hideToolbar.share();
    $scope.ppdIndex = timestoredata.getPayPeriodIndex(moment($routeParams.payPeriod, 'YYYYMMDD'));
    //if ($routeParams.ppdIndex !== undefined) {
    //    $scope.ppdIndex = $routeParams.ppdIndex;
    //}    

    timestoredata.getSignatureRequired($scope.ppdIndex, $routeParams.employeeId).then(ProcessData, function () { });

    function ProcessData(data) {
      console.log('number downloaded', data.length);
      $scope.timeData = data;
      if (data.length === 0) {
        $scope.Message = 'Nothing to print at this time.';
      }
      //findDupes(data);
      $scope.showProgress = false;
    }
    //function findDupes(data) {
    //    for (var i = 0; i < data.length; i++) {
    //        var eid = data[i].employeeID;
    //        for (var j = 0; j < data.length; j++) {
    //            if (i !== j && eid === data[j].employeeID) {
    //                console.log(data[j].employeeID + ' found at ' + i + ' and ' + j);
    //            }
    //        }
    //    }
    //}



  }

})();