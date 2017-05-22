
(function (angular) {
    'use strict';


    function workorderService($log, redirectService, crudContextHolderService, applicationService, restService, compositionService, alertService, $http, $rootScope, submitService) {

        function setLocationFromAsset() {
            const dm = crudContextHolderService.rootDataMap();
            if (dm.location == null && dm.extrafields.asset_.location != null) {
                dm.location = dm.extrafields.asset_.location;
            }
        }

        function goToDetail(parameters) {
            const params = {
                id: parameters["workorderid"],
                saveHistoryReturn: true
            };
            redirectService.goToApplication("workorder", "editdetail", params, null);
        }

        function openNewDetailModal(modalschemaId) {
            const params = {
                popupmode: "modal",
//                title: "Work Order"
            };

            const assetData = crudContextHolderService.rootDataMap();

            const schema = modalschemaId ? modalschemaId : "workordercreationmodal";

            const jsondata = {
                assetnum: assetData["assetnum"],
                location: assetData["location"],
                classstructureid: assetData["classstructureid"],
                siteid: assetData["siteid"],
                orgid: assetData["orgid"]
            };
            return redirectService.goToApplication("workorder", schema, params, jsondata);
        }

        function createWorkPackage(schema, datamap) {
            const workorderid = datamap["workorderid"];
            const wonum = datamap["wonum"];
            return redirectService.goToApplication("_workpackage", "newdetail", {}, { workorderid, wonum });
        }

        function createRelatedSr(schema, datamap) {
            var composition = 'relatedrecord_';
            // Make related record for workorder to SR and add to datamap, setting #createSr to true so related record handler will make the SR before the related record
            const relatedRecord = {
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
                    const newSr = Math.max.apply(Math, res["relatedrecord_"].list.map(function (o) { return o.relatedreckey; }));
                    const successMessage = "Related Service Request " + newSr + " was created";
                    alertService.notifymessage("success", successMessage);
                });
            });
        }

        const service = {
            goToDetail,
            openNewDetailModal,
            setLocationFromAsset,
            createRelatedSr,
            createWorkPackage
        };

        return service;
    }



    angular.module('sw_layout').service('workorderService', ["$log", "redirectService", 'crudContextHolderService', 'applicationService', 'restService', 'compositionService', 'alertService', '$http', '$rootScope', 'submitService', workorderService]);
}
)(angular);
