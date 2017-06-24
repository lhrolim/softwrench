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

        function openModalNew(item, callback, rollback) {
            if (!fsrequestService.validatePackage()) {
                return;
            }
            const wpDatamap = crudContextHolderService.rootDataMap();

            schemaCacheService.fetchSchema("_CallOut", "newdetail").then((schema) => {
                const mergedItem = compositionService.buildMergedDatamap(fsrequestService.buildDatamap(schema), item);
                mergedItem["orgid"] = wpDatamap ["#workorder_.orgid"];
                mergedItem["sendtime"] = fsrequestService.defaultSendTime();
                mergedItem["#editing"] = true;
                mergedItem["#calloutfileexplorer_"] = [];

                if (sessionStorage.mockfscallout) {

                    mergedItem["subcontractorid"] = "ATI";
                    mergedItem["email"] = "devteam@controltechnologysolutions.com";
                    mergedItem["sitename"] = "fs";
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

            const wpDatamap = crudContextHolderService.rootDataMap();
            if (item["subcontractor_.id"]) {
                item["subcontractor"] = item["subcontractor_.id"] + "";
            }
            item["orgid"] = wpDatamap["#workorder_.orgid"];
            item["#editing"] = true;
            item["email"] = item["email"].split(",");

            fsrequestService.addAttachments(item, "#calloutfileexplorer_");

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