
(function () {
    'use strict';

    angular.module('sw_layout').factory('assetService', ["$log", "redirectService", assetService]);

    function assetService($log, redirectService) {

        var service = {
            goToDetail: goToDetail
        };

        return service;

        function goToDetail(parameters) {
            var params = {
                id: parameters["assetid"],
                saveHistoryReturn: true
            };
            redirectService.goToApplication("asset", "detail", params, null);
        }
    }
})();
