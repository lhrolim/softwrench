(function (angular) {
    'use strict';


    function firstSolarPersonService(crudContextHolderService,userService) {


        function filterFacilitiesBySite(item) {
            const dm = crudContextHolderService.rootDataMap();
            const siteid = dm.fields["locationsite"];
            const availableFacilities = dm.fields["availablefacilities"] || [];
            return item.extrafields["site"] === siteid && availableFacilities.some(s => s === item.value);
        }


        const service = {
            filterFacilitiesBySite
        };
        return service;
    }

    angular
    .module('firstsolar')
    .clientfactory('personService', ["crudContextHolderService","userService", firstSolarPersonService]);


})(angular);