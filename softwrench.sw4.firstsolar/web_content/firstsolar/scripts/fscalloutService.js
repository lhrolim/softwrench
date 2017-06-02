(function (angular) {
    'use strict';
    
    function fscalloutService($rootScope, $timeout, modalService, schemaCacheService, alertService, crudContextHolderService, applicationService, compositionService, validationService) {

        function buildDatamap(schema) {
            const datamap = {};
            schema.displayables.forEach(d => {
                if (d.isHidden) {
                    return;
                }
                if (d.target) {
                    datamap[d.target] = null;
                } else {
                    datamap[d.attribute] = null;
                }
            });
            return datamap;
        }

        function postSave(saveDatamap, callback, rollback) {
            saveDatamap["subcontractor_.id"] = saveDatamap["subcontractor"];
            saveDatamap["status"] = Status.Scheduled;
            if (saveDatamap["submitaftersave"]) {
                saveDatamap["sendnow"] = true;
                saveDatamap["sendtime"] = null;
            }
            callback(saveDatamap);
            
            return applicationService.save({
                dispatchedByModal: false
            }).then(() => {
                modalService.hide();
            }).catch(() => {
                rollback();
            });
        }

        function validatePackage() {
            return validationService.validateCurrent().length <= 0;
        }

        function openModalNew(item, callback, rollback) {
            if (!validatePackage()) {
                return;
            }
            const wpDatamap = crudContextHolderService.rootDataMap();

            schemaCacheService.fetchSchema("_CallOut", "newdetail").then((schema) => {
                const mergedItem = compositionService.buildMergedDatamap(buildDatamap(schema), item);
                mergedItem["orgid"] = wpDatamap ["#workorder_.orgid"];
                var date = new Date();

                var toNextDay = (date.getUTCHours() <= 7);

                var currentOffSet = date.getTimezoneOffset();
                var diff = (420 - currentOffSet) / 60; // AZ timezone = -7
                date.setHours(17, 0, 0, 0);
                date.addHours(diff);
                
                if (toNextDay) {
                    date.setDate(date.getDate() + 1);
                }

                mergedItem["sendtime"] = date;
                mergedItem["#editing"] = true;
                modalService.show(schema, mergedItem, { cssclass: 'extra-height-modal' }, (saveDatamap) => {
                    postSave(saveDatamap, callback, rollback);
                });
            });
        }

        function openModalEdit(item, callback, rollback) {
            if (!validatePackage()) {
                return;
            }
            const wpDatamap = crudContextHolderService.rootDataMap();
            if (item["status"] !== Status.Scheduled) {
                alertService.alert("Is not possible edit a sent callout.");
                return;
            }
            if (item["subcontractor_.id"]) {
                item["subcontractor"] = item["subcontractor_.id"] + "";
            }
            if (!item["sendtime"]) {
                const date = new Date();
                date.setHours(23, 59, 59, 999);
                item["sendtime"] = date;
            }
            item["orgid"] = wpDatamap["#workorder_.orgid"];
            item["#editing"] = true;

            const parentDataMap = crudContextHolderService.rootDataMap();
            const attachs = parentDataMap["#calloutfileexplorer_"];
            if (attachs && !item["#calloutfileexplorer_"]) {
                item["#calloutfileexplorer_"] = [];
                angular.forEach(attachs, (attach) => {
                    const id = attach["docinfo_.urlparam1"].substr(9);
                    if (id === item["id"] + "") {
                        item["#calloutfileexplorer_"].push(attach);
                    }
                });
            }

            schemaCacheService.fetchSchema("_CallOut", "detail").then((schema) => {
                modalService.show(schema, item, { cssclass: 'extra-height-modal' }, (saveDatamap) => {
                    postSave(saveDatamap, callback, rollback);
                });
            });
        }


        function saveCallOut() {
            applicationService.save();
        }

    

        function deleteRow(item, callback, rollback) {
            if (item["status"] !== Status.Scheduled) {
                alertService.alert("Is not possible delete a sent callout.");
                return;
            }

            alertService.confirm("Are you sure you want to delete this subcontractor call out?").then(() => {
                callback();
                return applicationService.save({
                    dispatchedByModal: false
                }).catch(() => {
                    rollback();
                });
            });
        }

        const service = {
            openModalNew,
            openModalEdit,
            deleteRow,
            saveCallOut
        };
        return service;
    }

    angular
    .module("firstsolar")
        .clientfactory("fscalloutService", ["$rootScope", "$timeout", "modalService", "schemaCacheService", "alertService", "crudContextHolderService", "applicationService", "compositionService", "validationService", fscalloutService]);
    
    class Status {

        static get Open() {
            return "Open";
        }

        static get Scheduled() {
            return "Scheduled";
        }

        static get Submited() {
            return "Sent";
        }
    }
})(angular);