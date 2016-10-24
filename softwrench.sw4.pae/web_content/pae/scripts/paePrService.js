(function (angular) {
    "use strict";

    function paePrService($rootScope, crudContextHolderService, crudCrawlService) {
        //#region Utils
        function changeStatus(status, datamap) {
            datamap["status"] = status;
            const parameters = {
                successCbk: function () {
                    crudCrawlService.forwardAction();
                },
                failureCbk: function() {
                    datamap["status"] = "WAPPR";
                }
            }
            $rootScope.$broadcast("sw_submitdata", parameters);
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