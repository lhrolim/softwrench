(function (angular) {
    'use strict';
    
    function fsdailyoutagemeetingService(modalService, schemaCacheService, alertService, applicationService, compositionService, fsrequestService) {
        function openModalNew(item, callback, rollback) {
            if (!fsrequestService.validatePackage()) {
                return;
            }
            schemaCacheService.fetchSchema("_DailyOutageMeeting", "newdetail").then((schema) => {
                const mergedItem = compositionService.buildMergedDatamap(fsrequestService.buildDatamap(schema), item);
                mergedItem["mwhlostyesterday"] = 0;
                mergedItem["#domfileexplorer_"] = [];
                modalService.show(schema, mergedItem, fsrequestService.requestModalProps(), (saveDatamap) => {
                    fsrequestService.postSave(saveDatamap, callback, rollback);
                });
            });
        }

        function openModalEdit(item, callback, rollback) {
            if (!fsrequestService.validatePackage()) {
                return;
            }

            fsrequestService.addAttachments(item, "#domfileexplorer_");

            schemaCacheService.fetchSchema("_DailyOutageMeeting", "detail").then((schema) => {
                modalService.show(schema, item, fsrequestService.requestModalProps(), (saveDatamap) => {
                    fsrequestService.postSave(saveDatamap, callback, rollback).then(r => {
                        return r;
                    });
                });
            });
        }

        function saveDailyOutageMeeting() {
            applicationService.save();
        }


        function deleteRow(item, callback, rollback) {
            if (!fsrequestService.validatePackage()) {
                return;
            }
            alertService.confirm("Are you sure you want to delete this daily outge meeting?").then(() => {
                return fsrequestService.postDelete(callback, rollback);
            });
        }

        const service = {
            openModalNew,
            openModalEdit,
            deleteRow,
            saveDailyOutageMeeting
        };
        return service;
    }

    angular
    .module("firstsolar")
        .clientfactory("fsdailyoutagemeetingService", ["modalService", "schemaCacheService", "alertService", "applicationService", "compositionService", "firstsolar.fsrequestService", fsdailyoutagemeetingService]);
})(angular);