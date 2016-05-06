
(function (angular) {
    'use strict';



    function workorderService($log, redirectService, crudContextHolderService) {

        function setLocationFromAsset(parameters) {
            var dm = crudContextHolderService.rootDataMap().fields;
            if (dm.location == null && dm.extrafields.asset_.location!=null) {
                dm.location = dm.extrafields.asset_.location;
            }
        }

        function goToDetail(parameters) {
            var params = {
                id: parameters["workorderid"],
                saveHistoryReturn: true
            };
            redirectService.goToApplication("workorder", "editdetail", params, null);
        }

        function openNewDetailModal(modalschemaId) {
            var params = {
                popupmode: "modal",
//                title: "Work Order"
            };

            var datamap = crudContextHolderService.rootDataMap();

            var assetData = datamap.fields;

            var schema = modalschemaId ? modalschemaId : "workordercreationmodal";

            var jsondata = {
                assetnum: assetData["assetnum"],
                location: assetData["location"],
                classstructureid: assetData["classstructureid"],
                siteid: assetData["siteid"]
            };
            return redirectService.goToApplication("workorder", schema, params, jsondata);
        }

        var service = {
            goToDetail: goToDetail,
            openNewDetailModal: openNewDetailModal,
            setLocationFromAsset: setLocationFromAsset
        };

        return service;
    }



    angular.module('sw_layout').factory('workorderService', ["$log", "redirectService", 'crudContextHolderService', workorderService]);
}
)(angular);
