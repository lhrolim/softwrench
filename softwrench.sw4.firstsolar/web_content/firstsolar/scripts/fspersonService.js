(function (angular) {
    'use strict';


    function firstSolarPersonService(crudContextHolderService,userService) {


        function filterFacilitiesBySite(item) {
            const dm = crudContextHolderService.rootDataMap();
            const siteid = dm.fields["locationsite"];
            const groups = dm.fields["persongroups"] || [];
            return item.extrafields["site"] === siteid && groups.some(s => s === item.extrafields["group"]);
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