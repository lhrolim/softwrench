(function (angular) {
    "use strict";

    function chicagoformsService($q, $window, modalService, configurationService, schemaCacheService, restService, historyService) {

        const formsInfo = {
            app : "form",
            schemaid : "list",
            cached: false
        }

        //#region Utils
        function getFormSchema(info) {
            if (info.cached) {
                return $q.when(schemaCacheService.getCachedSchema(info.app, info.schemaid));
            }

            const parameters = {
                applicationName: info.app,
                targetSchemaId: info.schemaid
            };
            const promise = restService.getPromise("Metadata", "GetSchemaDefinition", parameters);
            return promise.then(function (result) {
                info.cached = true;
                schemaCacheService.addSchemaToCache(result.data);
                return result.data;
            });
        }
        //#endregion

        //#region Public methods
        function openFormsModal() {
            getFormSchema(formsInfo).then((schema) => {
                modalService.show(schema, null, {cancelOnClickOutside : true});
            });
        }
        
        function downloadForm({name,isibm}, column) {
            historyService.getRouteInfo().then(info => {
                if (column.attribute === "download") {
                    window.location = info.contextPath + `/ChicagoForms/GetPdfForm?formName=${name}&isIbm=${isibm}`;
                } else {
                    $window.open(info.contextPath + `/ChicagoForms/Index?formName=${name}&isIbm=${isibm}`, "_blank");
                }
            });
    }
    //#endregion

    //#region Service Instance
    const service = {
        openFormsModal,
        downloadForm
    };
    return service;
    //#endregion
}

    //#region Service registration

    angular.module("chicago").clientfactory("chicagoformsService", ["$q", "$window", "modalService", "configurationService", "schemaCacheService", "restService", "historyService", chicagoformsService]);

    //#endregion

})(angular);