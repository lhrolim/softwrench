
(function () {
    'use strict';

    angular.module('sw_layout').service('assetService', ["$log", "redirectService", assetService]);

    function assetService($log, redirectService) {

        var service = {
            goToDetail: goToDetail
        };

        return service;

        function goToDetail(parameters) {
            var params = {
                id: parameters["assetuid"],
                saveHistoryReturn: true
            };
            redirectService.goToApplication("asset", "detail", params, null);
        }
    }
})();
