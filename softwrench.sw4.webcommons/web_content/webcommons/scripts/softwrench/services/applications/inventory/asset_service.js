
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
                popupmode: "modal",
            };
            redirectService.goToApplication("asset", "detail", params, null);
        }
    }
})();
