(function (angular) {
    "use strict";

    function dynComponentService($rootScope, $q, restService, alertService, schemaCacheService, modalService, validationService, crudContextHolderService, crudextraService, redirectService,applicationService) {
        const app = "_dynamic";
        const reloadContainerModalSchemaId = "reloadcontainermodal";
        const deleteComponentModalSchemaId = "deletecomponentmodal";

        //#region Utils
        function getModalSchema(modalSchemaId) {
            const cachedSchema = schemaCacheService.getCachedSchema(app, modalSchemaId);
            if (cachedSchema) {
                return $q.when(cachedSchema);
            }

            const parameters = {
                applicationName: app,
                targetSchemaId: modalSchemaId
            }
            const promise = restService.getPromise("Metadata", "GetSchemaDefinition", parameters);
            return promise.then(function (result) {
                schemaCacheService.addSchemaToCache(result.data);
                return result.data;
            });
        }

        function openModal(modalSchemaId) {
            getModalSchema(modalSchemaId).then((schema) => {
                modalService.show(schema, {
                    dummy: "dummy" // to force datamap load
                });
            });
        }

        function sameScript(scriptA, scriptB) {
            if (!scriptA && !scriptB) {
                return true;
            }
            if ((scriptA && !scriptB) || (!scriptA && scriptB)) {
                return false;
            }
            const spacelessScriptA = scriptA.replace(/\s/g, "");
            const spacelessScriptB = scriptB.replace(/\s/g, "");
            return spacelessScriptA === spacelessScriptB;
        }

        function shouldBeOnContainer(datamap) {
            const systemVersion = datamap["systemversion"];
            const appliesToVersion = datamap["appliestoversion"];
            const deploy = datamap["deploy"];
            return deploy && systemVersion === appliesToVersion;
        }

        function hasImportantChanges(datamap) {
            const originalDatamap = crudContextHolderService.originalDatamap();
            if (datamap["target"] !== originalDatamap["target"]) {
                return true;
            }
            if (datamap["appliestoversion"] !== originalDatamap["appliestoversion"]) {
                return true;
            }
            if (datamap["deploy"] !== originalDatamap["deploy"]) {
                return true;
            }
            return !sameScript(datamap["script"], originalDatamap["script"]);
        }
        //#endregion

        //#region Public methods

        function reloadContainer() {
            openModal(reloadContainerModalSchemaId);
        }

        function reloadContainerConfirm(datamap) {
            const validation = validationService.validateCurrent("#modal");
            if (validation && validation.length > 0) {
                return;
            }

            modalService.hide();
            const json = angular.toJson({
                comment: datamap.comment,
                username: datamap.username
            });
            restService.invokePost("Scripts", "ReloadContainer", null, json, () => redirectService.goToApplication("_dynamic", "list"));
        }

        function deleteComponent() {
            openModal(deleteComponentModalSchemaId);
        }

        function deleteComponentConfirm(datamap) {
            const validation = validationService.validateCurrent("#modal");
            if (validation && validation.length > 0) {
                return;
            }

            modalService.hide();
            const detailSchema = crudContextHolderService.currentSchema();
            const detailDatamap = crudContextHolderService.rootDataMap();
            const extraParameters = {
                comment: datamap.comment,
                username: datamap.username
            }
            crudextraService.deletefn(detailSchema, detailDatamap, extraParameters, true);
        }

        function showSaveAndDeploy(datamap) {
            return shouldBeOnContainer(datamap) && hasImportantChanges(datamap);
        }

        function showSaveAndUndeploy(datamap) {
            return !shouldBeOnContainer(datamap) && hasImportantChanges(datamap);
        }

        function saveAndReload(datamap) {
            datamap["saveandreload"] = true;
            applicationService.save();
        }

        function showUsernameAndComment(datamap) {
            return hasImportantChanges(datamap);
        }

        //#endregion

        //#region Service Instance
        const service = {
            reloadContainer,
            reloadContainerConfirm,
            deleteComponent,
            deleteComponentConfirm,
            showSaveAndDeploy,
            showSaveAndUndeploy,
            saveAndReload,
            showUsernameAndComment
        };
        return service;
        //#endregion
    }

    //#region Service registration

    angular.module("sw_layout").factory("dynComponentService", ["$rootScope", "$q", "restService", "alertService", "schemaCacheService", "modalService", "validationService", "crudContextHolderService", "crudextraService", "redirectService", "applicationService", dynComponentService]);

    //#endregion

})(angular);