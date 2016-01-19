
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
//                title: "Work Order"
            };

            var assetData = parentdatamap.fields;

            var jsondata = {
                assetnum: assetData["assetnum"],
                location: assetData["location"],
                classstructureid: assetData["classstructureid"]
            };
            return redirectService.goToApplication("workorder", "workordercreationmodal", params, jsondata);
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
