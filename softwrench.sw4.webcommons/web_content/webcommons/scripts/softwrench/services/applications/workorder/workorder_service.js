
(function () {
    'use strict';

    angular.module('sw_layout').factory('workorderService', ["$log", "redirectService", workorderService]);

    function workorderService($log, redirectService) {

        var service = {
            goToDetail: goToDetail
        };

        return service;

        function goToDetail(parameters) {
            var params = {
                id: parameters["workorderid"]
            };
            redirectService.goToApplication("workorder", "editdetail", params, null);
        }
    }
})();
