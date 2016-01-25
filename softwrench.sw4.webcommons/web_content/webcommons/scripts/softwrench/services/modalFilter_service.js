
(function (angular) {
    "use strict";

    function modalFilterService($q, schemaCacheService, restService) {
        //#region Utils

        //#endregion

        //#region Public methods

        /**
         * Gets a promise with the modal filter schema.
         * 
         * @param {} filter
         * @param {} schema The current schema (not the modal one).
         * @returns {} A promise with the modal filter schema.
         */
        function getModalFilterSchema(filter, schema) {
            if (!filter || (!filter.targetSchemaId && !filter.advancedFilterSchemaId)) {
                return $q.when(null);
            }
            var schemaid = filter.targetSchemaId ? filter.targetSchemaId : filter.advancedFilterSchemaId;
            var tokens = schemaid.split(".");
            var modalSchemaAppName = tokens.length > 1 ? tokens[0] : schema.applicationName;
            var modalSchemaId = tokens.length > 1 ? tokens[1] : tokens[0];

            var cachedSchema = schemaCacheService.getCachedSchema(modalSchemaAppName, modalSchemaId);
            if (cachedSchema) {
                return $q.when(cachedSchema);
            }

            var parameters = {
                applicationName: modalSchemaAppName,
                targetSchemaId: modalSchemaId
            }
            var promise = restService.getPromise("Metadata", "GetSchemaDefinition", parameters);
            return promise.then(function (result) {
                schemaCacheService.addSchemaToCache(result.data);
                return result.data;
            });
        }

        //#endregion

        //#region Service Instance
        var service = {
            getModalFilterSchema: getModalFilterSchema
        };
        return service;
        //#endregion
    }

    //#region Service registration

    angular.module("sw_layout").factory("modalFilterService", ["$q", "schemaCacheService", "restService", modalFilterService]);

    //#endregion

})(angular);