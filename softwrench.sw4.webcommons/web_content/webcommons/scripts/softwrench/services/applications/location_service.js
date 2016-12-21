
(function (angular) {
    'use strict';
    function locationService($log, redirectService) {

        function dispatchWO(schema, datamap) {
            redirectService.goToApplication("workorder", "newdetail", null, { "location": datamap["location"], "siteid": datamap["siteid"] });
        }
        var service = {
            dispatchWO: dispatchWO
        };
        return service;
    }
    angular
    .module('sw_layout')
    .service('locationService', ['$log', 'redirectService', locationService]);
})(angular);
