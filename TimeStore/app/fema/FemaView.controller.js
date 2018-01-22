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
      return [{ "ppe": "12/13/2016", "employees": [1383, 1883] },
        { "ppe": "10/18/2016", "employees": [1190] },
        { "ppe": "12/27/2016", "employees": [1169, 1182] },
        { "ppe": "11/29/2016", "employees": [1289, 1134] },
        { "ppe": "1/10/2017", "employees": [1830] },
        { "ppe": "1/24/2017", "employees": [1241] },
      ];
    }
    // Hurricane Irma Data

    // BCC Staff through 09-19
    // [{"ppe": "9/19/2017", "employees": [1012,1019,1028,1030,1031,1043,1063,1064,1065,1066,1067,1073,1074,1075,1076,1078,1079,1080,1114,1117,1121,1124,1125,1127,1140,1151,1157,1159,1167,1168,1181,1182,1184,1187,1191,1204,1212,1216,1217,1222,1229,1235,1241,1245,1255,1266,1267,1277,1281,1291,1303,1304,1306,1308,1313,1324,1326,1327,1328,1335,1337,1340,1341,1344,1347,1349,1361,1369,1377,1384,1387,1389,1397,1398,1408,1414,1420,1433,1436,1445,1451,1452,1459,1460,1461,1474,1480,1492,1501,1521,1549,1563,1567,1569,1707,1719,1721,1729,1759,1789,1808,1824,1848,1849,1863,1887,1889,1892,1896,1963,1969,1978,1987,1989,2008,2009,2018,2019,2022,2033,2037,2057,2070,2081,2082,2097,2127,2163,2191,2192,2196,2206,2216,2220,2225,2229,2258,2262,2264,2266,2273,2375,2377,2399,2410,2427,2439,2445,2457,2459,2469,2471,2474,2493,2497,2500,2512,2516,2530,2532,2535,2541,2542,2549,2551,2553,2554,2555,2558,2559,2563,2570,2572,2576,2605,2617,2619,2633,2634,2641,2644,2648,2650,2652,2659,2675,2679,2681,2684,2695,2696,2701,2704,2710,2711,2714,2722,2736,2737,2739,2745,2747,2753,2754,2755,2756,2761,2768,2769,2772,2774,2782,2783,2784,2786,2791,2793,2794,2797,2808,2810,2811,2813,2814,2815,2816,2819,2826,2832,2837,2838,2839,2846,2847,2849,2850,2851,2857,2858,2860,2861]}];
    // CAT B Data
    // Telestaff Fema Data PPE 09-19-2017
    // [{"ppe": "9/19/2017", "employees": [1042,1091,1093,1095,1101,1109,1130,1145,1158,1162,1169,1172,1173,1190,1199,1202,1258,1259,1262,1272,1280,1289,1298,1318,1365,1379,1383,1404,1422,1425,1456,1472,1530,1546,1552,1562,1579,1584,1674,1677,1678,1679,1684,1713,1839,1853,1883,1885,1970,1971,1973,1993,1994,1996,1999,2039,2046,2051,2052,2111,2112,2116,2235,2237,2238,2241,2245,2249,2251,2260,2347,2386,2389,2400,2435,2436,2438,2481,2484,2485,2487,2505,2509,2523,2525,2526,2527,2533,2539,2552,2567,2568,2579,2580,2583,2584,2586,2590,2593,2594,2596,2597,2598,2609,2628,2636,2655,2662,2664,2666,2667,2670,2707,2715,2716,2718,2719,2720,2728,2744,2748,2752,2765,2781,2785,2796,2798,2799,2803,2806,2821,2822,2824,2825,2827,2828,2830,2831,2835,2842,2848,2854,2862,2863,2864,2865,2866,2867,2868,2869,2870,2871,2872,2873,2874]}];
    // Telestaff Fema Data PPE 10-03-2017
    // [{"ppe": "10/3/2017", "employees": [1589,2112,2116,2481,2526,2579,2583,2592,2609,2627,2636,2669,2707,2720,2752,2785,2803,2822,2830,2863]}];
    // Telestaff Fema Data PPE 10-17-2017
    // [{"ppe": "10/17/2017", "employees": [1237,1589,1679,2481,2526,2584,2590,2592,2597,2628,2666,2707,2715,2748,2749,2765,2806,2821,2822,2827,2830]}];
    // Telestaff Fema Data PPE 10-31-2017
    // [{"ppe": "10/31/2017", "employees": [1425,2112,2254,2414,2481,2592,2715,2796,2821,2827,2869,2870,2874]}];
    // Timestore Fema Data PPE 09-19-2017

    // Timestore Fema Data PPE 10-03-2017
    // [{"ppe": "10/3/2017", "employees": [1030,1075,1090,1125,1204,1222,1251,1277,1304,1377,1508,1719,1879,1887,1987,2019,2041,2070,2081,2258,2273,2282,2410,2464,2493,2634,2704,2711,2742,2772,2813,2815,2818,2819,2849,2850,2851]}];
    // Timestore Fema Data PPE 10-17-2017
    // [{"ppe": "10/17/2017", "employees": [1030,1075,1125,1204,1222,1304,1508,1553,1719,1816,1887,1889,1989,2041,2081,2196,2197,2264,2273,2282,2385,2410,2464,2474,2493,2549,2701,2704,2710,2711,2742,2753,2818,2819,2849,2858]}];
    // Timestore Fema Data PPE 10-31-2017
    // [{"ppe": "10/31/2017", "employees": [1030,1125,1222,1241,1508,1719,1879,1889,2282,2410,2474,2493,2553,2644,2704,2753,2819]}];

    // CAT A Data
    // CAT A \Debris 10_03_2017
    // [{"ppe": "10/3/2017", "employees": [1202,1259,1269,1404,1562,1584,1713,1856,1997,2241,2670,2716]}]
    // CAT A \Debris 10-17-2017
    // [{"ppe": "10/17/2017", "employees": [1259,1269,1404,1579,1584,1713,1997,2039,2508,2670]}];
    // CAT A \Debris 10-31-2017
    // [{"ppe": "10/31/2017", "employees": [1130,1202,1237,1259,1269,1383,1404,1552,1579,1584,1713,1993,1997,2052,2241,2508,2670,2716]}];



    // Hurricane Matthew Data
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