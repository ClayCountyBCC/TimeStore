/* global _, moment */
(function ()
{
  "use strict";
  angular
    .module("timestoreApp")
    .controller("TimeclockViewController", [
      "$scope",      
      "viewOptions",
      "timestoredata",
      "timestoreNav",
      "timeclockdata",
      TimeclockView
    ]);

  function TimeclockView(
    $scope,    
    viewOptions,
    timestoredata,
    timestoreNav,
    timeclockdata
    
  )
  {
    $scope.timeclockdata = timeclockdata;
    $scope.filteredData = [];
    $scope.supervisors = [];
    $scope.supervisor = "SHOW ALL";
    $scope.workdate = "";
    $scope.timeclockdata.length > 0 ? moment($scope.timeclockdata[0].work_date).format('MM/DD/YYYY') : "";

    $scope.showTimeclockData = function ()
    {
      if ($scope.supervisor === "SHOW ALL")
      {
        $scope.filteredData = $scope.timeclockdata;
      }
      else
      {
        $scope.filteredData = $scope.timeclockdata.filter(
          function (j)
          {
            return j.reports_to_name === $scope.supervisor;
          });
      }
    };

    function filterSupervisors()
    {
      $scope.supervisors = [];
      for (var i = 0; i < $scope.timeclockdata.length; i++)
      {
        if ($scope.supervisors.indexOf($scope.timeclockdata[i].reports_to_name) === -1 && $scope.timeclockdata[i].reports_to_name.trim().length > 0)
        {
          $scope.supervisors.push($scope.timeclockdata[i].reports_to_name);
          if ($scope.timeclockdata[i].my_employee_id === $scope.timeclockdata[i].reports_to)
          {
            $scope.supervisor = $scope.timeclockdata[i].reports_to_name;
          }
        }
      }
      $scope.supervisors.sort();
      $scope.supervisors.splice(0, 0, "SHOW ALL");
      $scope.showTimeclockData();
    }

    filterSupervisors();




    $scope.addTimeGo = function (employeeId)
    {
      timestoreNav.goAddTime(employeeId, $scope.workdate);
    };

  }
})();
