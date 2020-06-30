/* global moment, _ */
(function () {
  "use strict";

  angular.module('timestoreApp').factory('datelist', ['timestoredata', function (timestoredata) {

    return {
      getShortPayPeriodList: getShortPayPeriodList,
      getPayPeriodList: getPayPeriodList,
      getWorkDayList: getWorkDayList
    }

    function getShortPayPeriodList() {
      var end = moment().add(1, 'months')
      var current = moment(timestoredata.getPayPeriodEnd(moment().format('M/D/YYYY')), 'YYYYMMDD');
      var start = moment(timestoredata.getPayPeriodEnd(moment().subtract(1, 'months').format('M/D/YYYY')), 'YYYYMMDD');
      var ppl = [start.format('M/D/YYYY')];
      while (start.isBefore(end)) {
        var s = start.add(14, 'days');
        var p = s.format('M/D/YYYY');
        ppl.push(p);
      }
      return ppl;
    }

    function getPayPeriodList() {
      var end = moment().add(1, 'years')
      var current = moment(timestoredata.getPayPeriodEnd(moment().format('M/D/YYYY')), 'YYYYMMDD');
      var start = moment(timestoredata.getPayPeriodEnd(moment().subtract(2, 'years').format('M/D/YYYY')), 'YYYYMMDD');
      var ppl = [];
      while (start.isBefore(end)) {
        var s = start.add(14, 'days');
        var p = s.format('M/D/YYYY');
        ppl.push(p);
      }
      return ppl;
    }

    function getWorkDayList() {
      var payPeriodStart = timestoredata.getPayPeriodStart();
      var start = moment(payPeriodStart, 'YYYYMMDD').subtract(1, 'days');
      var workdays = [];
      workdays = _.times(42, function () {
        return start.add(1, 'days').format('dddd M/D/YYYY');
      });
      return workdays;
    }

  }]);



})();