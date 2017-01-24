
(function (angular) {
    'use strict';


    function kongsbergCommunicationService(crudContextHolderService, userService) {


        //SWWEB-2910
        function getFromDefaultExpression(dm, schema) {

            const datamap = crudContextHolderService.rootDataMap();

            if ("rigmanager".equalIc(datamap.siteid)) {
                return "rigmanager@kdisupport24.com";
            }
            if ("kspice".equalIc(datamap.siteid) || "k-spice".equalIc(datamap.siteid)) {
                return "kspice@kdisupport24.com";
            }
            if ("ledaflow".equalIc(datamap.siteid)) {
                return "ledaflow@kdisupport24.com";
            }
            if ("support24".equalIc(datamap.siteid)) {
                return "support@kdisupport24.com";
            }
            return "support@kdisupport24.com";
        }


        function getSubjectDefaultExpression(datamap, schema, displayable) {
            const parentData = crudContextHolderService.rootDataMap();
            if (datamap['ownertable'].equalIc("SR")) {
                return "##" + parentData['ticketid'] + '## ' + parentData['description'];
            }
            return "";
        };

        const service = {
            getSubjectDefaultExpression,
            getFromDefaultExpression
        };
        return service;
    }

    angular
    .module('kongsberg')
    .clientfactory('kongsbergCommunicationService', ['crudContextHolderService', 'userService', kongsbergCommunicationService]);


})(angular);
