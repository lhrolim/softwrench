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

        //#afterchange
        loadCities() {
            const datamap = this.crudContextHolderService.rootDataMap();
            const division = datamap["division"];
            this.restService.get("SwgasRequestApi", "LoadCities", { division }).then(result => {
                const data = result.data;
                this.crudContextHolderService.updateEagerAssociationOptions("#city", data);
            });
        }

        //#afterchange
        loadBuildings() {
            const datamap = this.crudContextHolderService.rootDataMap();
            const city = datamap["city"];
            const division = datamap["division"];
            this.restService.get("SwgasRequestApi", "LoadBuildings", { division, city }).then(result => {
                const data = result.data;
                this.crudContextHolderService.updateEagerAssociationOptions("#locbuilding", data);
            });
        }
    }

    swgasNoLoginSubmitService.$inject = ["submitService", "crudContextHolderService", "restService", "spinService"];

    angular.module("swgas").clientfactory("swgasNoLoginSubmitService", swgasNoLoginSubmitService);

})(angular);