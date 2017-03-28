(function (angular) {
    'use strict';


    function firstSolarPersonService(crudContextHolderService, personService, restService) {
        function onSiteSelected(event) {
            personService.onSiteSelected(event);
            const datamap = event.fields;
            const personid = datamap["personid"];
            const siteid = datamap["locationsite"];
            const parameters = {
                maximoPersonId: personid,
                siteid: siteid
            }
            restService.getPromise("FirstSolarAdvancedSearch", "GetAvailableFacilities", parameters).then((result) => {
                const options = result.data;
                crudContextHolderService.updateEagerAssociationOptions("availableFacilities", options);
                const dm = crudContextHolderService.rootDataMap();
                dm["facilities"] = [];
            });
        }


        const service = {
            onSiteSelected
        };
        return service;
    }

    angular
    .module('firstsolar')
    .clientfactory('personService', ["crudContextHolderService", "personService", "restService", firstSolarPersonService]);


})(angular);