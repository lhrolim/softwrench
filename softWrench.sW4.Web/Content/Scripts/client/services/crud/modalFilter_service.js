
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
            const schemaid = filter.targetSchemaId ? filter.targetSchemaId : filter.advancedFilterSchemaId;
            const tokens = schemaid.split(".");
            const modalSchemaAppName = tokens.length > 1 ? tokens[0] : schema.applicationName;
            const modalSchemaId = tokens.length > 1 ? tokens[1] : tokens[0];
            const cachedSchema = schemaCacheService.getCachedSchema(modalSchemaAppName, modalSchemaId);
            if (cachedSchema) {
                return $q.when(cachedSchema);
            }
            const parameters = {
                applicationName: modalSchemaAppName,
                targetSchemaId: modalSchemaId
            };
            const promise = restService.getPromise("Metadata", "GetSchemaDefinition", parameters);
            return promise.then(function (result) {
                schemaCacheService.addSchemaToCache(result.data);
                return result.data;
            });
        }

        //#endregion

        //#region Service Instance
        const service = {
            getModalFilterSchema: getModalFilterSchema
        };
        return service;
        //#endregion
    }

    //#region Service registration

    angular.module("sw_layout").service("modalFilterService", ["$q", "schemaCacheService", "restService", modalFilterService]);

    //#endregion

})(angular);