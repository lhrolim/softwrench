(function (angular) {
    'use strict';
    
    function fsmaintananceengService(modalService, schemaCacheService, alertService, crudContextHolderService, applicationService, compositionService, fsrequestService) {

        const i18N = "maintenance engineering";

        function postSave(saveDatamap, callback, rollback) {
            if (saveDatamap["engineer"]) {
                const option = crudContextHolderService.fetchLazyAssociationOption("#workorder_.fakelabor_", saveDatamap["engineer"].toLocaleLowerCase(), "#modal");
                saveDatamap["#engineername"] = option ? option.label : saveDatamap["engineer"];
            }
            saveDatamap["status"] = Status.Scheduled;
            if (saveDatamap["sendnow"]) {
                saveDatamap["sendtime"] = null;
            }
            return fsrequestService.postSave(saveDatamap, callback, rollback);
        }

        function openModalNew(item, callback, rollback) {
            if (!fsrequestService.validatePackage()) {
                return;
            }
            schemaCacheService.fetchSchema("_MaintenanceEngineering", "newdetail").then((schema) => {
                const mergedItem = compositionService.buildMergedDatamap(fsrequestService.buildDatamap(schema), item);
                mergedItem["sendtime"] = fsrequestService.defaultSendTime();
                mergedItem["email"] = crudContextHolderService.rootDataMap()["defaultmetoemail"];
                mergedItem["#maintenanceengineeringfileexplorer_"] = [];
                modalService.show(schema, mergedItem, { cssclass: 'extra-height-modal' }, (saveDatamap) => {
                    postSave(saveDatamap, callback, rollback);
                });
            });
        }

        function openModalEdit(item, callback, rollback) {
            if (!fsrequestService.verifyEdit(item, i18N)) {
                return;
            }
            if (!fsrequestService.validatePackage()) {
                return;
            }
            
            fsrequestService.addAttachments(item, "#maintenanceengineeringfileexplorer_");
            if (typeof item["cc"] === "string") {
                item["cc"] = item["cc"].split(",");
            }

            schemaCacheService.fetchSchema("_MaintenanceEngineering", "detail").then((schema) => {
                item["email"] = item["email"] || "";
                modalService.show(schema, item, { cssclass: 'extra-height-modal' }, (saveDatamap) => {
                    postSave(saveDatamap, callback, rollback);
                });
            });
        }

        function saveMaintananceEng() {
            applicationService.save();
        }


        function deleteRow(item, callback, rollback) {
            if (!fsrequestService.verifyDelete(item, i18N)) {
                return;
            }
            if (!fsrequestService.validatePackage()) {
                return;
            }
            alertService.confirm("Are you sure you want to delete this maintenance engineering?").then(() => {
                return fsrequestService.postDelete(callback, rollback);
            });
        }

        const service = {
            openModalNew,
            openModalEdit,
            deleteRow,
            saveMaintananceEng
        };
        return service;
    }

    angular
    .module("firstsolar")
        .clientfactory("fsmaintananceengService", ["modalService", "schemaCacheService", "alertService", "crudContextHolderService", "applicationService", "compositionService", "firstsolar.fsrequestService", fsmaintananceengService]);
    
    class Status {
        static get Pending() {
            return "Pending";
        }
        static get Scheduled() {
            return "Scheduled";
        }
        static get Submited() {
            return "Sent";
        }
        static get Accepted() {
            return "Accepted";
        }
        static get Rejected() {
            return "Rejected";
        }
    }
})(angular);