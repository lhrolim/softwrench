
(function () {
    'use strict';

    mobileServices.factory('operationService', ['schemaService', operationService]);

    function operationService(schemaService) {

        var service = {
            getCrudOperation: getCrudOperation
        };

        return service;

        function getCrudOperation(dataEntry) {
            var isNew = schemaService.getId(dataEntry.datamap, detailSchema) == null;
            return isNew ? "crud_create" : "crud_update";
        }

    }
})();
