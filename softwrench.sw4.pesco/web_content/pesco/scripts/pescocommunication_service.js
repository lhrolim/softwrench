
(function (angular) {
    'use strict';

  
    function pescoCommunicationService(crudContextHolderService) {
       

        function getSubjectDefaultExpression (datamap, schema, displayable) {

            var parentData = crudContextHolderService.rootDataMap();
            if (datamap['ownertable'].equalIc("SR")) {
                return "SR ##" + parentData['ticketid'];
            }
            if (datamap['ownertable'].equalIc("WORKORDER")) {
                return "WO ##" + parentData['wonum'];
            }
            return "";
        };

        const service = {
            getSubjectDefaultExpression: getSubjectDefaultExpression
        };

        return service;
    }

    angular
    .module('pesco')
    .clientfactory('pescoCommunicationService', ['crudContextHolderService', pescoCommunicationService]);


})(angular);
