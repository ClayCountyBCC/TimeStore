/* global _ */
(function ()
{
  "use strict";
  angular.module('timestoreApp')
    .controller('FemaViewController', ['$scope', 'viewOptions', 'timestoredata', '$routeParams', FemaTimecard]);

  function FemaTimecard($scope, viewOptions, timestoredata, $routeParams)
  {

    //$scope.showProgress = true;
    $scope.filtered = [];
    $scope.Message = '';
    $scope.timeData = [];
    $scope.rawData = [];
    $scope.selectedGroup = -1;
    $scope.selectedPPI = 0;
    $scope.GroupsByPayperiodData = GroupsByPayPeriod();

    //$scope.processSelectedGroup = function ()
    //{
    //  if ($scope.selectedGroup === -1) return;
    //  var g = $scope.GroupsByPayperiodData[$scope.selectedGroup];
    //  var ppi = timestoredata.getPayPeriodIndex(moment(g.ppe, 'M/D/YYYY'));
    //  if (ppi !== $scope.selectedPPI)
    //  {
    //    $scope.showProgress = true;
    //    $scope.Message = '';
    //    timestoredata.getFemaData(ppi).then(function (data)
    //    {
    //      $scope.rawData = data;
    //      $scope.selectedPPI = ppi;
    //      $scope.timeData = FilterRawData($scope.rawData, g.employees);
    //      if (data.length === 0)
    //      {
    //        $scope.Message = 'No timecards found.';
    //      }
    //      $scope.showProgress = false;
    //    });
    //  }
    //  else
    //  {
    //    $scope.timeData = FilterRawData($scope.rawData, g.employees);
    //  }
    //};

    $scope.processAllGroups = function ()
    {
      $scope.showProgress = true;
      //var g = $scope.GroupsByPayperiodData[$scope.selectedGroup];
      var ppis = [];
      var es = [];
      var c = {};
      var raw = [], filtered = [];
      var ppi, currentPPI;
      var g = $scope.GroupsByPayperiodData;
      for (var i = 0; i < g.length; i++)
      {

        ppi = timestoredata.getPayPeriodIndex(moment(g[i].ppe, 'M/D/YYYY'));
        //console.log(g[i].ppe, g[i].employees, ppi);
        var x = ppis.indexOf(ppi);
        if (x === -1)
        {
          ppis.push(ppi);
          es.push(g[i].employees);
        }
        else
        {
          //for (var j = 0; j < g[i].employees.length; j++)
          //{
          //  if (es[x].indexOf(g[i].employees[j]) > -1)
          //  {
          //    console.log('duplicate employee', g[i].employees[j], es[x]);
          //  }
          //  else
          //  {
          //    es[x].push(g[i].employees[j]);
          //  }
          //}
          es[x].push.apply(es[x], g[i].employees);
        }
      }
      //console.log('ppi', ppis, 'employees', es);
      getData(ppis.pop(), es.pop(), ppis, es);
      //console.log('all filtered', filtered);
      $scope.showProgress = false;
    };

    function getData(ppi, employees, ppis, es)
    {
      if (!ppi)
      {

        $scope.timeData = $scope.filtered.sort(function (a, b)
        {
          var e1 = parseInt(a['employeeID']);
          var e2 = parseInt(b['employeeID']);
          var d1 = new Date(a['payPeriodStart']);
          var d2 = new Date(b['payPeriodStart']);
          return e1 === e2 ? d1 - d2 : e1 - e2;
        });
        console.log('sorted', $scope.timeData);
        return;
      }
      timestoredata.getFemaData(ppi).then(function (data)
      {
        //raw = data;
        //currentPPI = ppi;        
        var fd = FilterRawData(data, employees);
        $scope.filtered.push.apply($scope.filtered, fd);
        getData(ppis.pop(), es.pop(), ppis, es);

      });
    }


    $scope.processAllGroups();

    viewOptions.viewOptions.showSearch = false;
    viewOptions.viewOptions.share();
    //$scope.ppdIndex = timestoredata.getPayPeriodIndex(moment($routeParams.payPeriod, 'YYYYMMDD'));


    //console.log('pay period data', $scope.GroupsByPayperiodData);
    //timestoredata.getFemaData($scope.ppdIndex).then(ProcessData, function () { });

    function FilterRawData(raw, employees)
    {
      return _.filter(raw, function (x)
      {
        return employees.indexOf(parseInt(x.employeeID)) !== -1;
      });
      //console.log('timedata', $scope.timeData);
    }

    function GroupsByPayPeriod()
    {
      return [{ "ppe": "10/18/2016", "employees": [1019, 1020, 1065, 1073, 1078, 1081, 1181, 1223, 1233, 1255, 1266, 1303, 1335, 1344, 1347, 1349, 1356, 1357, 1359, 1374, 1483, 1499, 1501, 1521, 1523, 1567, 1729, 1822, 1887, 1889, 1969, 1987, 1989, 2060, 2081, 2082, 2097, 2192, 2231, 2375, 2377, 2399, 2427, 2428, 2445, 2457, 2459, 2464, 2474, 2489, 2490, 2493, 2500, 2511, 2513, 2541, 2542, 2574, 2604, 2605, 2614, 2615, 2630, 2633, 2644, 2648, 2650, 2652, 2685, 2704, 2710, 2711, 2742, 2746, 2753, 2759, 2783] }];
    }
    // Debris - Call Center - 30 day
    // [{"ppe": "10/18/2016", "employees": [1303,2377,2541]}, {"ppe": "11/1/2016", "employees": [1002,1083,1184,1303,1335,1349,1417,1433,1483,1523,1830,1848,1901,2029,2037,2192,2439,2489,2530,2634,2650,2659,2704,2750,2758,2759]}];
    // Debris - Call Center - 90 day
    // [{"ppe": "11/15/2016", "employees": [1083,1303,1326,1417,1433,1483,1808,1822,1837,1969,2007,2048,2097,2258,2439,2497,2650,2659,2758,2769]}, {"ppe": "11/29/2016", "employees": [1181,1347,1830,2048,2489,2650,2759]}, {"ppe": "12/13/2016", "employees": [2650]}]
    // Animal Control 
    // [{"ppe": "10/18/2016", "employees": [1175,1266,1896,2191,2229,2563,2726,2745,2766,2777,2778]}]
    // Public Safety Cat B
    // [{ "ppe": "10/18/2016", "employees": [1042, 1091, 1092, 1101, 1109, 1130, 1145, 1158, 1169, 1172, 1173, 1199, 1258, 1259, 1262, 1269, 1272, 1280, 1289, 1298, 1318, 1365, 1379, 1380, 1383, 1422, 1425, 1437, 1456, 1472, 1478, 1530, 1546, 1552, 1562, 1576, 1579, 1584, 1674, 1678, 1679, 1684, 1713, 1740, 1758, 1853, 1882, 1885, 1971, 1973, 1993, 1994, 1999, 2000, 2039, 2046, 2051, 2052, 2054, 2111, 2112, 2118, 2123, 2235, 2237, 2238, 2242, 2243, 2245, 2246, 2249, 2251, 2254, 2260, 2347, 2389, 2398, 2400, 2414, 2436, 2438, 2481, 2484, 2485, 2487, 2505, 2509, 2523, 2524, 2546, 2552, 2565, 2567, 2579, 2580, 2586, 2590, 2591, 2592, 2593, 2596, 2597, 2609, 2610, 2611, 2626, 2628, 2655, 2662, 2663, 2665, 2666, 2667, 2669, 2670, 2705, 2715, 2716, 2717, 2719, 2720, 2724, 2725, 2728, 2733, 2734, 2744, 2752, 2765, 2770, 2775, 2776, 2779, 2781, 2785] }];
    // pub works Grading cat b - not run yet.
    // [{"ppe": "10/18/2016", "employees": [1012,1028,1074,1079,1159,1167]}]
    // pub works Misc
    // [{"ppe": "10/4/2016", "employees": [1569]}, {"ppe": "10/18/2016", "employees": [1030,1063,1075,1080,1097,1117,1121,1124,1125,1140,1168,1191,1212,1245,1328,1337,1389,1414,1445,1460,1480,1569,1759,1892,1978,2018,2022,2057,2127,2216,2220,2515,2516,2551,2619,2677,2681,2703,2714,2735,2737,2747,2754,2791]}, {"ppe": "11/1/2016", "employees": [1245,1369,1863,2266,2515,2551,2604,2641,2703,2761]}, {"ppe": "11/15/2016", "employees": [ 2604 ]}]
    // pub works signs 
    // [{"ppe": "10/18/2016", "employees": [1066,1306,1449,1707,1759,2010,2262,2674]}]
    // Cat B Preliminary Timestore data
    // [{"ppe": "10/18/2016", "employees": [1019,1020,1065,1073,1078,1081,1181,1223,1233,1255,1266,1303,1335,1344,1347,1349,1356,1357,1359,1374,1483,1499,1501,1521,1523,1567,1729,1822,1887,1889,1969,1987,1989,2060,2081,2082,2097,2192,2231,2375,2377,2399,2427,2428,2445,2457,2459,2464,2474,2489,2490,2493,2500,2511,2513,2541,2542,2574,2604,2605,2614,2615,2630,2633,2644,2648,2650,2652,2685,2704,2710,2711,2742,2746,2753,2759,2783]}]



  }

})();