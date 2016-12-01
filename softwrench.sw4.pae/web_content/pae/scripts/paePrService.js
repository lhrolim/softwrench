(function (angular) {
    "use strict";

    function paePrService($rootScope, crudContextHolderService, crudCrawlService, applicationService, schemaService, modalService, validationService) {
        //#region Utils
        function openModal(modalschemaid) {
            schemaService.getSchema("pr", modalschemaid).then((schema) => {
                modalService.show(schema, {
                    memo: ""
                }, {});
            });
        }

        function changeStatus(status, memo) {
            if (validationService.validateCurrent("#modal").length !== 0) {
                return;
            }

            const datamap = crudContextHolderService.rootDataMap();
            datamap["status"] = status;
            datamap["memo"] = memo;

            applicationService.save({
                dispatchedByModal : false
            }).then(() => {
                modalService.hide();
                crudCrawlService.forwardAction();
            }).catch(() => {
                datamap["status"] = "WAAA";
            });
        }
        //#endregion

        //#region Public methods

        function approve() {
            openModal("memomodalapprove");
        }

        function reject() {
            openModal("memomodalreject");
        }

        function modalapprove(modaldatamap) {
            changeStatus("APPR", modaldatamap["memo"]);
        }

        function modalreject(modaldatamap) {
            changeStatus("PRREJ", modaldatamap["memo"]);
        }

        function areButtonsEnabled() {
            const datamap = crudContextHolderService.rootDataMap();
            return datamap["status"] === "WAAA";
        }

        //#endregion

        //#region Service Instance
        const service = {
            approve,
            reject,
            areButtonsEnabled,
            modalapprove,
            modalreject
        };
        return service;
        //#endregion
    }

    //#region Service registration

    angular.module("sw_layout").factory("paePrService", ["$rootScope", "crudContextHolderService", "crudCrawlService", "applicationService", "schemaService", "modalService", "validationService", paePrService]);

    //#endregion

})(angular);