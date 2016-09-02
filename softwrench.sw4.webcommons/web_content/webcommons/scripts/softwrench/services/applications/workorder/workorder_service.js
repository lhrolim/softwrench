
(function (angular) {
    'use strict';


    function workorderService($log, redirectService, crudContextHolderService, applicationService, restService, compositionService, alertService, $http, $rootScope, submitService) {

        function setLocationFromAsset() {
            var dm = crudContextHolderService.rootDataMap();
            if (dm.location == null && dm.extrafields.asset_.location != null) {
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

            var assetData = crudContextHolderService.rootDataMap();

            var schema = modalschemaId ? modalschemaId : "workordercreationmodal";

            var jsondata = {
                assetnum: assetData["assetnum"],
                location: assetData["location"],
                classstructureid: assetData["classstructureid"],
                siteid: assetData["siteid"],
                orgid: assetData["orgid"]
            };
            return redirectService.goToApplication("workorder", schema, params, jsondata);
        }

        function createRelatedSr(schema, datamap) {
            var composition = 'relatedrecord_';
            // Make related record for workorder to SR and add to datamap, setting #createSr to true so related record handler will make the SR before the related record
            var relatedRecord = {
                '#createSr': true,
                'relatedrecsiteid': datamap['siteid'],
                'relatedrecorgid': datamap['orgid'],
                'relatedrecclass': 'SR',
                '#isDirty': true
            };
            if (datamap[composition] == null) {
                datamap[composition] = [];
            }
            datamap[composition].push(relatedRecord);

            applicationService.save().then(function (res) {
                res.successMessage = null;
                // remove `#isDirty` and `#createSr` flags from the related records composition items
                datamap[composition] = datamap[composition]
                        .map(function (c) {
                            delete c["#isDirty"];
                            delete c["#createSr"];
                            return c;
                        });
                // Need to update the composition data to incude the new related record
                compositionService.populateWithCompositionData(schema, datamap).then(function (res) {
                    // Alert the user of the new SR
                    var newSr = Math.max.apply(Math, res["relatedrecord_"].list.map(function (o) { return o.relatedreckey; }));
                    var successMessage = "Related Service Request " + newSr + " was created";
                    alertService.notifymessage("success", successMessage);
                });
            });
        }

        var service = {
            goToDetail: goToDetail,
            openNewDetailModal: openNewDetailModal,
            setLocationFromAsset: setLocationFromAsset,
            createRelatedSr: createRelatedSr
        };

        return service;
    }



    angular.module('sw_layout').factory('workorderService', ["$log", "redirectService", 'crudContextHolderService', 'applicationService', 'restService', 'compositionService', 'alertService', '$http', '$rootScope', 'submitService', workorderService]);
}
)(angular);
