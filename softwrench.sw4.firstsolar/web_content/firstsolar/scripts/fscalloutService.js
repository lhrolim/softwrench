(function (angular) {
    'use strict';
    
    function fscalloutService(modalService, schemaCacheService, alertService, crudContextHolderService, applicationService, compositionService, fsrequestService) {

        const i18N = "callout";

        function postSave(saveDatamap, callback, rollback) {
            saveDatamap["subcontractor_.id"] = saveDatamap["subcontractor"];
            saveDatamap["status"] = Status.Scheduled;
            if (saveDatamap["sendnow"]) {
                saveDatamap["sendtime"] = null;
            }
            return fsrequestService.postSave(saveDatamap, callback, rollback);
        }

        function beforeOpenModal(item) {
            const wpDatamap = crudContextHolderService.rootDataMap();
            item["orgid"] = wpDatamap["#workorder_.orgid"];
            item["#editing"] = true;
            if (!item["sitename"]) {
                item["sitename"] = wpDatamap["#workorder_.site_.description"];
            }
        }

        function openModalNew(item, callback, rollback) {
            if (!fsrequestService.validatePackage()) {
                return;
            }

            schemaCacheService.fetchSchema("_CallOut", "newdetail").then((schema) => {
                const mergedItem = compositionService.buildMergedDatamap(fsrequestService.buildDatamap(schema), item);
                beforeOpenModal(mergedItem);
                mergedItem["sendtime"] = fsrequestService.defaultSendTime();
                mergedItem["#calloutfileexplorer_"] = [];

                if (sessionStorage.mockfscallout) {
                    mergedItem["subcontractorid"] = "ATI";
                    mergedItem["email"] = "devteam@controltechnologysolutions.com";
                    mergedItem["tonumber"] = "mockedto";
                    mergedItem["nottoexceedamount"] = 1;
                    mergedItem["scopeofwork"] = "mocked scope of work";
                    mergedItem["plantcontacts"] = "mocked plant contacts";
                    mergedItem["otherinfo"] = "mocked other info";
                    var futureDate = new Date();
                    futureDate.addDays(2);
                    mergedItem["expirationdate"] = futureDate;
                    mergedItem["contractorstartdate"] = futureDate;
                }

                modalService.show(schema, mergedItem, fsrequestService.requestModalProps(), (saveDatamap) => {
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
            
            if (item["subcontractor_.id"]) {
                item["subcontractor"] = item["subcontractor_.id"] + "";
            }
            beforeOpenModal(item);

            if (typeof item["email"] === "string") {
                item["email"] = item["email"].split(",");
            }

            fsrequestService.addAttachments(item, "#calloutfileexplorer_");

            schemaCacheService.fetchSchema("_CallOut", "detail").then((schema) => {
                modalService.show(schema, item, fsrequestService.requestModalProps(), (saveDatamap) => {
                    postSave(saveDatamap, callback, rollback);
                });
            });
        }


        function saveCallOut() {
            applicationService.save();
        }

        function deleteRow(item, callback, rollback) {
            if (!fsrequestService.verifyDelete(item, i18N)) {
                return;
            }
            if (!fsrequestService.validatePackage()) {
                return;
            }
            alertService.confirm("Are you sure you want to delete this subcontractor callout?").then(() => {
                return fsrequestService.postDelete(callback, rollback);
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
        .clientfactory("fscalloutService", ["modalService", "schemaCacheService", "alertService", "crudContextHolderService", "applicationService", "compositionService", "firstsolar.fsrequestService", fscalloutService]);
    
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