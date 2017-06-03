(function (angular) {
    'use strict';
    
    function fsmaintananceengService(modalService, schemaCacheService, alertService, crudContextHolderService, applicationService, compositionService, fsrequestService) {

        function verifyEdit(item) {
            const status = item["status"];
            if ([Status.Submited, Status.Accepted, Status.Rejected].some((invalidStatus) => invalidStatus === status)) {
                alertService.alert(`Is not possible edit a maintenance engineering request with status "${status}".`);
                return false;
            }
            return true;
        }

        function verifyDelete(item) {
            const status = item["status"];
            if ([Status.Submited, Status.Accepted, Status.Rejected].some((invalidStatus) => invalidStatus === status)) {
                alertService.alert(`Is not possible delete a maintenance engineering request with status "${status}".`);
                return false;
            }
            return true;
        }

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
                mergedItem["email"] = "";
                mergedItem["#maintenanceengineeringfileexplorer_"] = [];
                modalService.show(schema, mergedItem, { cssclass: 'extra-height-modal' }, (saveDatamap) => {
                    postSave(saveDatamap, callback, rollback);
                });
            });
        }

        function openModalEdit(item, callback, rollback) {
            if (!verifyEdit(item)) {
                return;
            }
            if (!fsrequestService.validatePackage()) {
                return;
            }
            
            fsrequestService.addAttachments(item, "#maintenanceengineeringfileexplorer_");

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
            if (!verifyDelete(item)) {
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