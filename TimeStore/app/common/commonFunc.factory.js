/* global moment, _ */
(function ()
{
  angular.module('timestoreApp')
    .factory('commonFunctions', function ()
    {

      return {
        getGroupsByShortPayRate: getGroupsByShortPayRate,
        getTotalHours: getTotalHours
      };

      function getTotalHours(tl)
      {
        if (tl === undefined)
        {
          return 0;
        }
        var total = 0;
        for (var i = 0; i < tl.length; i++)
        {
          total += tl[i].hours;
        }
        return total;
      }

      function getGroupsByShortPayRate(tl)
      {
        var groupArray = [];

        angular.forEach(tl, function (item, idx)
        {
          if (groupArray.indexOf(parseFloat(item.shortPayRate)) === -1)
          {
            groupArray.push(parseFloat(item.shortPayRate));
          }
        });
        return groupArray;
      }

    });

})();