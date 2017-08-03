/* global moment, _ */
(function () {
    "use strict";

    angular.module('timestoreApp')
        .controller('SwitchUserController', ['timestoreNav', 'timestoredata', SwitchUser]);

    function SwitchUser(timestoreNav, timestoredata) {
        // in here we will basically discover the current user's employee id
        // and take the current pay period end and then use that
        // to populate the $location
        timestoredata.getDefaultEmployeeId().then(function (data) {
            timestoreNav.goDefaultEmployee(data);
        });
        
    }

}());