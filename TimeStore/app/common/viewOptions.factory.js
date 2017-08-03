(function ()
{
  "use strict";
  angular.module("timestoreApp").factory("viewOptions", [
    "$rootScope",
    function ($rootScope)
    {
      var viewOptions = {
        showSearch: false,
        share: function ()
        {
          $rootScope.$broadcast("shareShowSearch");
        }
      };
      var hideToolbar = {
        share: function ()
        {
          $rootScope.$broadcast("hideToolbar");
        }
      };
      var approvalUpdated = {
        approvalUpdated: false,
        share: function ()
        {
          $rootScope.$broadcast("shareApprovalUpdated");
        }
      };
      var leaveRequestUpdated = {
        share: function ()
        {
          $rootScope.$broadcast("leaveRequestUpdated");
        }
      };
      var timecardReloaded = {
        share: function ()
        {
          $rootScope.$broadcast("shareTimecardReloaded");
        }
      };
      return {
        hideToolbar: hideToolbar,
        timecardReloaded: timecardReloaded,
        viewOptions: viewOptions,
        approvalUpdated: approvalUpdated,
        leaveRequestUpdated: leaveRequestUpdated
      };
    }
  ]);
})();
