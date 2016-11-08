(function (angular) {
    "use strict";

    function paePrService($rootScope, crudContextHolderService, crudCrawlService, applicationService) {
        //#region Utils
        function changeStatus(status, datamap) {
            datamap["status"] = status;

            applicationService.save().then(crudCrawlService.forwardAction)
                .catch(() => {
                    datamap["status"] = "WAPPR";
                });

        }
        //#endregion

        //#region Public methods

        function approve(datamap) {
            changeStatus("APPR", datamap);
        }

        function reject(datamap) {
            changeStatus("PRREJ", datamap);
        }

        function areButtonsEnabled() {
            const datamap = crudContextHolderService.rootDataMap();
            return datamap["status"] === "WAPPR";
        }

        //#endregion

        //#region Service Instance
        const service = {
            approve,
            reject,
            areButtonsEnabled
        };
        return service;
        //#endregion
    }

    //#region Service registration

    angular.module("sw_layout").factory("paePrService", ["$rootScope", "crudContextHolderService", "crudCrawlService", paePrService]);

    //#endregion

})(angular);