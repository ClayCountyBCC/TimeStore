/* global moment, _ */
(function () {
  "use strict";
  angular.module('timestoreApp')
      .directive('timecardNotes', function () {
        return {
          restrict: 'E',
          templateUrl: 'TimecardNotes.tmpl.html',//'app/timecard/TimecardNotes.tmpl.html',
          scope: {
            notes: '=',
            employeeid: '=',
            payperiodending: '=',
            datatype: '='
          },
          controller: ['$scope', 'timestoredata', 'viewOptions', function ($scope, timestoredata, viewOptions) {
            $scope.noteText = '';

            $scope.saveNote = function () {
              timestoredata.saveNote($scope.employeeid, $scope.noteText, $scope.payperiodending)
                        .then(onSuccess, onError);

            };

            function onSuccess(data) {
              $scope.noteText = '';
              viewOptions.approvalUpdated.approvalUpdated = true;
              viewOptions.approvalUpdated.share();
            }

            function onError(data) {
              console.log('note error');
              console.log(data);
            }

          }]
        };
      });
}());