/* global _ */
(function () {
  'use strict';

  angular.module('timestoreApp', ['ngMaterial', 'ngRoute', 'ngCookies']) //'ngSanitize', 

    // ngRoute mechanism
        .config(['$compileProvider', function ($compileProvider) {
          $compileProvider.aHrefSanitizationWhitelist(/^\s*(https?|blob|data|ftp|mailto|tel|file):/);
          $compileProvider.debugInfoEnabled(false); // disable while debugging.
        }])
        .config(['$routeProvider', function ($routeProvider) {

          $routeProvider
              .when('/', {
                controller: 'SwitchUserController',
                template: ''
              })
              .when('/e/:employeeId/ppd/:payPeriod', {
                controller: 'TimeCardViewController',
                templateUrl: 'TimeCardView.tmpl.html',
                resolve: {
                  timecard: ['timestoredata', '$route', function (timestoredata, $route) {
                    var ppi = timestoredata.getPayPeriodIndex(moment($route.current.params.payPeriod, 'YYYYMMDD'));
                    var eid = $route.current.params.employeeId;
                    return timestoredata.getEmployee(ppi, eid)
                        .then(function (data) {
                          console.log('timecard data', data);
                          return data;
                        });
                  }]
                }
              })
              .when('/e/:employeeId/addtime/day/:workDate', {
                controller: 'AddTimeViewController',
                templateUrl: 'AddTimeView.tmpl.html',
                resolve: {
                  timecard: ['timestoredata', '$route', function (timestoredata, $route) {
                    var wd = moment($route.current.params.workDate, 'YYYYMMDD');
                    var ppd = moment(timestoredata.getPayPeriodStart(wd.format('M/D/YYYY'), true), 'YYYYMMDD');
                    var ppi = timestoredata.getPayPeriodIndex(ppd);
                    var eid = $route.current.params.employeeId;
                    return timestoredata.getEmployee(ppi, eid)
                        .then(function (data) {
                          console.log('timecard data, add time', data);
                          return data;
                        });
                  }]
                }
              })
              .when('/signature/ppd/:payPeriod', {
                controller: 'SignatureViewController',
                templateUrl: 'TimeCardSignatureView.tmpl.html' //'app/signatureview/TimeCardSignatureView.tmpl.html',
              })
              .when('/signature/e/:employeeId/ppd/:payPeriod', {
                controller: 'SignatureViewController',
                templateUrl: 'TimeCardSignatureView.tmpl.html' //'app/signatureview/TimeCardSignatureView.tmpl.html',
              })
              .when('/LeaveApproval/', {
                controller: 'leaveApprovalController',
                templateUrl: 'LeaveApproval.tmpl.html'
              })
              .when('/LeaveRequest/', {
                controller: 'leaveRequestViewController',
                templateUrl: 'LeaveRequestView.controller.tmpl.html'
              })
              .when('/a/', {
                controller: 'AccessController',
                templateUrl: 'Access.tmpl.html' // 'app/access/Access.tmpl.html',
              })
              .when('/incentives/:incentiveType', {
                controller: 'IncentiveController',
                templateUrl: 'Incentives.tmpl.html' // 'app/incentives/Incentives.tmpl.html',
              })
              .when('/approval/ppd/:payPeriod', {
                controller: 'ApprovalController',
                templateUrl: 'Approval.tmpl.html' //'app/approval/Approval.tmpl.html',
              })
              //.when('/approval/:approvalType', {
              //    controller: 'ApprovalController',
              //    templateUrl: 'Approval.tmpl.html' //'app/approval/Approval.tmpl.html',
              //})
              .when('/approval/:approvalType/:ppdIndex', {
                controller: 'ApprovalController',
                templateUrl: 'Approval.tmpl.html' //'app/approval/Approval.tmpl.html',
              })
              .when('/dailycheckoff', {
                controller: 'DailyCheckoffController',
                templateUrl: 'DailyCheckoff.tmpl.html' // 'app/dailycheckoff/DailyCheckoff.tmpl.html',
              })
              .when('/exceptions/ppd/:payPeriod', {
                controller: 'ExceptionsController',
                templateUrl: 'Exceptions.tmpl.html'
              })
              .when('/unapproved/ppd/:payPeriod', {
                controller: 'TimecardNotApprovedController',
                templateUrl: 'TimeCardNotApproved.tmpl.html'
              })
              .when('/FinanceTools', {
                controller: 'FinanceToolsController',
                templateUrl: 'FinanceTools.tmpl.html' //'app/financetools/FinanceTools.tmpl.html',
              })
              .when('/LeaveCalendar/', {
                controller: 'CalendarViewController',
                templateUrl: 'CalendarView.controller.tmpl.html',
                resolve: {
                  deptleavedata: ['timestoredata', function (timestoredata) {
                    return timestoredata.getDeptLeaveRequests()
                        .then(function (data) {
                          _.remove(data.leaveData, function (ld) {
                            return ld.Approved === false && ld.approval_id !== 0;
                          });
                          var group = [];
                          group.push({ work_date: null, employee_name: 'my_dept', hours_used: 0, comment: '', dept: data.MyDept });
                          _.forEach(data.leaveData, function (d) {
                            var i = _.findIndex(group, function (n) {
                              return n.work_date === d.work_date && n.employee_name === d.employee_name;
                            });
                            if (i === -1) {
                              group.push({ work_date: d.work_date, employee_name: d.employee_name, hours_used: d.hours_used, comment: d.comment, dept: d.dept_id });
                            } else {
                              group[i].hours_used += d.hours_used;
                            }
                          });
                          return group;
                        });
                  }],
                  holidays: ['timestoredata', function (timestoredata) {
                    return timestoredata.getHolidays()
                        .then(function (data) {
                          return data;
                        });
                  }],
                  birthdays: ['timestoredata', function (timestoredata) {
                    return timestoredata.getBirthdays()
                        .then(function (data) {
                          return data;
                        });
                  }],
                }
              })
              .otherwise({ redirectTo: '/' });

          //$locationProvider.html5Mode(false).hashPrefix('!');
        }])
            .filter('groupPayrateBy', function () {
              return function (items, group) {
                return items.filter(function (element, index, array) {
                  return parseFloat(element.shortPayRate) === group;
                });
              };
            });

}());