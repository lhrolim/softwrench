﻿
(function (angular) {
    'use strict';

  
    function kongsbergCommunicationService(crudContextHolderService) {
       

        function getSubjectDefaultExpression (datamap, schema, displayable) {

            var parentData = crudContextHolderService.rootDataMap();
            if (datamap['ownertable'].equalIc("SR")) {
                return "##" + parentData['ticketid'] + '## ' + parentData['description'];
            }
            return "";
        };

        var service = {
            getSubjectDefaultExpression: getSubjectDefaultExpression
        };

        return service;
    }

    angular
    .module('kongsberg')
    .clientfactory('kongsbergCommunicationService', ['crudContextHolderService', kongsbergCommunicationService]);


})(angular);
