/* global moment, _ */
(function ()
{
  "use strict";
  angular.module('timestoreApp')
    .factory('timestoreNav', ['$route', '$http', '$location', 'timestoredata', '$window',
      function ($route, $http, $location, timestoredata, $window)
      {
        return {
          goSwitchUser: goSwitchUser,
          goPaystub: goPaystub,
          goTimeclockView: goTimeclockView,
          goHome: goHome,
          goDefaultEmployee: goDefaultEmployee,
          goEmployeeByPPD: goEmployeeByPPD,
          goDailyCheckoff: goDailyCheckoff,
          goIncentives: goIncentives,
          goAccessChange: goAccessChange,
          goFinanceTools: goFinanceTools,
          goTimecardApprovals: goTimecardApprovals,
          goLeaveApprovals: goLeaveApprovals,
          goAddTime: goAddTime,
          goExceptions: goExceptions,
          changePayPeriod: changePayPeriod,
          changeWorkDate: changeWorkDate,
          goSignatureRequired: goSignatureRequired,
          goUnapproved: goUnapproved,
          goFema: goFema,
          goCalendar: goCalendar,
          goLeaveRequest: goLeaveRequest
        };

        function goSwitchUser()
        {
          // switch back to the default view
          console.log('goswitchuser');
          go('/switchuser');
        }

        function goCalendar()
        {
          // going to use moment to navigate to the current year and month
          go('/LeaveCalendar/');
        }

        function goTimeclockView()
        {
          go('/timeclockview/day/' + moment().format("YYYYMMDD"));
        }

        function goLeaveRequest()
        {
          // going to use moment to navigate to the current year and month
          go('/LeaveRequest/');
        }

        function goPaystub(eid)
        {
          go('/e/' + eid + '/paystub');
        }

        function goSignatureRequired()
        {
          go('/signature/ppd/' + timestoredata.getPayPeriodEnd());
        }

        function goUnapproved()
        {
          go('/unapproved/ppd/' + timestoredata.getPayPeriodEnd());
        }
        function goFema()
        {
          go('/fema/');
        }

        function goAddTime(eid, d)
        {
          var dateToUse = '';
          if (d !== undefined && d.length > 0)
          {
            dateToUse = moment(d, "M/D/YYYY").format("YYYYMMDD");
          } else
          {
            dateToUse = moment().format("YYYYMMDD");
          }
          go('/e/' + eid + '/addtime/day/' + dateToUse);
          //$location.path('/e/' + eid + '/addtime/' + dateToUse);
        }

        function goLeaveApprovals()
        {
          go('/LeaveApproval/');
          //$window.location.href = '#/LeaveApproval';
        }

        function goTimecardApprovals()
        {
          go('approval/ppd/' + timestoredata.getPayPeriodEnd());
          //$window.location.href = '#/approval/F';
        }

        function goFinanceTools()
        {
          go('/FinanceTools/');
          //$window.location.href = '#/FinanceTools';
        }

        function goAccessChange()
        {
          go('/a/');
          //$window.location.href = '#/a/';
        }

        function goExceptions()
        {
          go('/exceptions/ppd/' + timestoredata.getPayPeriodEnd());
        }

        function changePayPeriod(ppd)
        {
          $route.updateParams({ payPeriod: ppd });
        }

        function changeWorkDate(wd)
        {
          $route.updateParams({ workDate: wd });
        }

        function goHome()
        {
          goWindow('/');
          //$window.location.href = '';
        }

        function goDefaultEmployee(eid)
        {
          go('/e/' + eid + '/ppd/' + timestoredata.getPayPeriodEnd());
          //$location.path('/e/' + eid + '/ppd/' + timestoredata.getPayPeriodEnd());
        }

        function goEmployeeByPPD(eid, ppd)
        {
          go('/e/' + eid + '/ppd/' + ppd);
          //$location.path('/e/' + eid + '/ppd/' + ppd);
        }

        function goDailyCheckoff()
        {
          go('/dailycheckoff/');
          //$window.location.href = '#/dailycheckoff/';
        }

        function goIncentives(incentiveType)
        {
          go('/incentives/' + incentiveType);
          //$window.location.href = '#/incentives/' + incentiveType;
        }

        function go(url)
        {
          // if the location is an empty path, we are going to use the $window object, 
          // otherwise we'll use $location.path
          if ($location.path().length === 0 || !checkUrl())
          {
            goWindow(url);
          } else
          {
            goLocation(url);
          }
        }

        function checkUrl()
        {
          var url = $location.absUrl().toUpperCase();
          return url.indexOf('#') > -1; // || url.indexOf('#') > -1;
        }

        function goLocation(url)
        {
          $location.path(url);
        }
        function goWindow(url)
        {
          $window.location.href = '#' + url;
        }
        
      }]);

})();