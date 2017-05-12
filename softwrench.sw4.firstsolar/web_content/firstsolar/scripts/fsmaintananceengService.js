(function (angular) {
    'use strict';
    
    function fsmaintananceengService($rootScope, $timeout, modalService, schemaCacheService, alertService, crudContextHolderService, applicationService, compositionService) {

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

        function verifyEdit(item) {
            const status = item["status"];
            if ([Status.Submited, Status.Accepted, Status.Rejected].some((invalidStatus) => invalidStatus === status)) {
                alertService.alert(`Is not possible edit maintenance engineering request with status "${status}".`);
                return false;
            }
            return true;
        }

        function verifyDelete(item) {
            const status = item["status"];
            if ([Status.Submited, Status.Accepted, Status.Rejected].some((invalidStatus) => invalidStatus === status)) {
                alertService.alert(`Is not possible delete maintenance engineering request with status "${status}".`);
                return false;
            }
            return true;
        }

        function postSave(saveDatamap) {
            if (saveDatamap["engineer"]) {
                const option = crudContextHolderService.fetchLazyAssociationOption("#workorder_.fakelabor_", saveDatamap["engineer"].toLocaleLowerCase(), "#modal");
                saveDatamap["#engineername"] = option ? option.label : saveDatamap["engineer"];
            }
            if (saveDatamap["submitaftersave"]) {
                saveDatamap["status"] = Status.SubmitAfterSave;
                saveDatamap["sendtime"] = null;
            } else {
                saveDatamap["status"] = Status.Pending;
            }
        }

        function openModalNew(item, callback) {
            schemaCacheService.fetchSchema("_MaintenanceEngineering", "newdetail").then((schema) => {
                const mergedItem = compositionService.buildMergedDatamap(buildDatamap(schema), item);
                var date = new Date();
                date.setHours(23, 59, 59, 999);
                mergedItem["sendtime"] = date;
                modalService.show(schema, mergedItem, { cssclass: 'extra-height-modal' }, (saveDatamap) => {
                    postSave(saveDatamap);
                    callback(saveDatamap);
                    modalService.hide();
                });
            });
        }

        function openModalEdit(item, callback) {
            if (!verifyEdit(item)) {
                return;
            }
            schemaCacheService.fetchSchema("_MaintenanceEngineering", "detail").then((schema) => {
                modalService.show(schema, item, { cssclass: 'extra-height-modal' }, (saveDatamap) => {
                    postSave(saveDatamap);
                    callback(saveDatamap);
                    modalService.hide();
                });
            });
        }

        function baseSaveMaintananceEng(submit) {
            const item = crudContextHolderService.rootDataMap("#modal");
            item["submitaftersave"] = !!submit;
            applicationService.save();
        }

        function saveMaintananceEng() {
            baseSaveMaintananceEng(false);
        }

        function saveAndSubmitMaintananceEng() {
            baseSaveMaintananceEng(true);
        }

        function deleteRow(item, callback) {
            if (!verifyDelete(item)) {
                return;
            }
            alertService.confirm("Are you sure you want to delete this maintenance engineering?").then(() => {
                callback();
            });
        }

        const service = {
            openModalNew,
            openModalEdit,
            deleteRow,
            saveMaintananceEng,
            saveAndSubmitMaintananceEng
        };
        return service;
    }

    angular
    .module("firstsolar")
        .clientfactory("fsmaintananceengService", ["$rootScope", "$timeout", "modalService", "schemaCacheService", "alertService", "crudContextHolderService", "applicationService", "compositionService", fsmaintananceengService]);
    
    class Status {
        static get Pending() {
            return "Pending";
        }
        static get SubmitAfterSave() {
            return "Submit After Save";
        }
        static get Submited() {
            return "Submited";
        }
        static get Accepted() {
            return "Accepted";
        }
        static get Rejected() {
            return "Rejected";
        }
    }
})(angular);