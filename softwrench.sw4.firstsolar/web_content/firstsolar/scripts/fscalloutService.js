(function (angular) {
    'use strict';
    
    function fscalloutService($rootScope,$timeout,modalService, schemaCacheService, alertService, crudContextHolderService, applicationService, compositionService) {

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

        function postSave(saveDatamap) {
            saveDatamap["subcontractor_.id"] = saveDatamap["subcontractor"];
            if (saveDatamap["submitaftersave"]) {
                saveDatamap["status"] = Status.SubmitAfterSave;
                saveDatamap["sendtime"] = null;
            } else {
                saveDatamap["status"] = Status.Open;
            }
        }

        function openModalNew(item, callback) {
            schemaCacheService.fetchSchema("_CallOut", "newdetail").then((schema) => {
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
            if (item["status"] === Status.Submited) {
                alertService.alert("Is not possible edit a submited callout.");
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

            schemaCacheService.fetchSchema("_CallOut", "detail").then((schema) => {
                modalService.show(schema, item, { cssclass: 'extra-height-modal' }, (saveDatamap) => {
                    postSave(saveDatamap);
                    callback(saveDatamap);
                    modalService.hide();
                });
            });
        }

        function baseSaveCallOut(submit) {
            const item = crudContextHolderService.rootDataMap("#modal");
            item["submitaftersave"] = !!submit;
            applicationService.save();
        }

        function saveCallOut() {
            baseSaveCallOut(false);
        }

        function saveAndSubmitCallOut() {
            baseSaveCallOut(true);
        }

        function deleteRow(item, callback) {
            if (item["status"] === "Submited") {
                alertService.alert("Is not possible delete a submited callout.");
                return;
            }

            alertService.confirm("Are you sure you want to delete this subcontractor call out?").then(() => {
                callback();
            });
        }

        const service = {
            openModalNew,
            openModalEdit,
            deleteRow,
            saveCallOut,
            saveAndSubmitCallOut
        };
        return service;
    }

    angular
    .module("firstsolar")
        .clientfactory("fscalloutService", ["$rootScope", "$timeout","modalService", "schemaCacheService", "alertService", "crudContextHolderService", "applicationService", "compositionService", fscalloutService]);
    
    class Status {
        static get Open() {
            return "Open";
        }
        static get SubmitAfterSave() {
            return "Submit After Save";
        }
        static get Submited() {
            return "Submited";
        }
    }
})(angular);