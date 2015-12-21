
(function (angular) {
    'use strict';
    function locationService($log, redirectService) {
        function dispatchWO(schema, datamap) {
            redirectService.goToApplication("workorder", "newdetail", null, { "location": datamap.fields["location"] });
        }
        var service = {
            dispatchWO: dispatchWO
        };
        return service;
    }
    angular
    .module('sw_layout')
    .factory('locationService', ['$log', 'redirectService', locationService]);
})(angular);
