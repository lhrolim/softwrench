(function (angular) {
    "use strict";


    class swgasNoLoginSubmitService {

        constructor(submitService, crudContextHolderService, restService, spinService) {
            this.submitService = submitService;
            this.crudContextHolderService = crudContextHolderService;
            this.restService = restService;
            this.spinService = spinService;
        }

        submit() {
            const parameters = {
                originaldatamap: this.crudContextHolderService.originalDatamap(),
                customurl: this.restService.getActionUrl("SwgasRequestApi", "Submit")
            };
            const schemaToSave = this.crudContextHolderService.currentSchema();
            const datamap = this.crudContextHolderService.rootDataMap();
            this.spinService.start();
            return this.submitService.submit(schemaToSave, datamap, parameters).finally((result) => {
                this.spinService.stop();
                return result;
            }).then((result) => {
                window.location = url("/swgasrequestsuccess") + `?id=${result.id}`;
            });
        }
    }

    swgasNoLoginSubmitService.$inject = ["submitService", "crudContextHolderService", "restService", "spinService"];

    angular.module("swgas").clientfactory("swgasNoLoginSubmitService", swgasNoLoginSubmitService);

})(angular);