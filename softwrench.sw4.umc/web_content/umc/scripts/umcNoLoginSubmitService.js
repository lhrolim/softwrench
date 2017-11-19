(function (angular) {
    "use strict";


    class umcNoLoginSubmitService {

        constructor(submitService, crudContextHolderService, restService, spinService) {
            this.submitService = submitService;
            this.crudContextHolderService = crudContextHolderService;
            this.restService = restService;
            this.spinService = spinService;
        }

        submit() {
            const parameters = {
                originaldatamap: this.crudContextHolderService.originalDatamap(),
                customurl: this.restService.getActionUrl("UmcRequestApi", "Submit")
            };
            const schemaToSave = this.crudContextHolderService.currentSchema();
            const datamap = this.crudContextHolderService.rootDataMap();
            this.spinService.start();
            return this.submitService.submit(schemaToSave, datamap, parameters).finally(() => this.spinService.stop()).then(() => {
                window.location.pathname = url("/umcrequestsuccess");
            });
        }
    }

    umcNoLoginSubmitService.$inject = ["submitService", "crudContextHolderService", "restService", "spinService"];

    angular.module("umc").clientfactory("umcNoLoginSubmitService", umcNoLoginSubmitService);

})(angular);