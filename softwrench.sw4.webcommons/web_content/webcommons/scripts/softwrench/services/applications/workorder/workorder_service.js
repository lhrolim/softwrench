
(function () {
    'use strict';

    angular.module('sw_layout').factory('workorderService', ["$log", "redirectService", workorderService]);

    function workorderService($log, redirectService) {

        var service = {
            goToDetail: goToDetail,
            openNewDetailModal: openNewDetailModal
        };

        return service;

        function goToDetail(parameters) {
            var params = {
                id: parameters["workorderid"]
            };
            redirectService.goToApplication("workorder", "editdetail", params, null);
        }

        function openNewDetailModal(parentdatamap) {
            var params = {
                popupmode: "modal"
            };
            var jsondata = {
                assetnum: parentdatamap.fields["assetnum"],
                location: parentdatamap.fields["location"]
            };
            redirectService.goToApplication("workorder", "newdetail", params, jsondata);
        }
    }
})();
