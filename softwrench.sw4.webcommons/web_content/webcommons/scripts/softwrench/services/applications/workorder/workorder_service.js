
(function (angular) {
    'use strict';



    function workorderService($log, redirectService) {



        function goToDetail(parameters) {
            var params = {
                id: parameters["workorderid"]
            };
            redirectService.goToApplication("workorder", "editdetail", params, null);
        }

        function openNewDetailModal(parentdatamap) {
            var params = {
                popupmode: "modal",
                title: "Work Order"
            };
            var jsondata = {
                assetnum: parentdatamap.fields["assetnum"],
                location: parentdatamap.fields["location"]
            };
            redirectService.goToApplication("workorder", "assetdetail", params, jsondata);
        }

        var service = {
            goToDetail: goToDetail,
            openNewDetailModal: openNewDetailModal
        };

        return service;
    }



    angular.module('sw_layout').factory('workorderService', ["$log", "redirectService", workorderService]);
}
)(angular);
