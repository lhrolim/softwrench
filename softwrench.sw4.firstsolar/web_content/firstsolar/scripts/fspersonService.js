(function (angular) {
    'use strict';


    function firstSolarPersonService(crudContextHolderService) {


        function filterFacilitiesBySite(item) {
            const dm = crudContextHolderService.rootDataMap();
            const siteid = dm.fields["locationsite"];
            return item.extrafields["site"] === siteid;
        }


        const service = {
            filterFacilitiesBySite
        };
        return service;
    }

    angular
    .module('firstsolar')
    .clientfactory('personService', ["crudContextHolderService", firstSolarPersonService]);


})(angular);