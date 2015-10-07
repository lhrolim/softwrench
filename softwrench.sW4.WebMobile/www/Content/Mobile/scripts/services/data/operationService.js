(function (mobileServices) {
    "use strict";

    mobileServices.factory("operationService", ["schemaService", "crudConstants", operationService]);

    function operationService(schemaService, crudConstants) {

        var service = {
            getCrudOperation: getCrudOperation
        };

        return service;

        function getCrudOperation(dataEntry, detailSchema) {
            var isNew = schemaService.getId(dataEntry.datamap, detailSchema) == null;
            return isNew ? crudConstants.operation.create : crudConstants.operation.update;
        }

    }
})(mobileServices);
