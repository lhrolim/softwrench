
(function () {
    "use strict";

    //#region Service registration

    angular.module("sw_layout").factory("schemaCacheService", [schemaCacheService]);

    //#endregion

    function schemaCacheService() {

        //#region Utils

        var schemaCache = {

        }

        function restore() {
            var schemaCacheJson = sessionStorage["schemaCache"];
            if (schemaCacheJson) {
                schemaCache = JSON.parse(schemaCacheJson);
            }
        }

        restore();

        //#endregion

        //#region Public methods


        function getSchemaCacheKeys() {
            var result = ";";
            for (var key in schemaCache) {
                if (!schemaCache.hasOwnProperty(key)) {
                    continue;
                }
                result += key + ";";
            }
            return result;
        }

        function getSchemaFromResult(result) {
            if (result.cachedSchemaId) {
                return this.getCachedSchema(result.applicationName, result.cachedSchemaId);
            }
            return result.schema;

        }

        function getCachedSchema(applicationName, schemaId) {
            return schemaCache[applicationName + "." + schemaId];
        }

        function addSchemaToCache(schema) {
            if (schema == null) {
                return;
            }
            schemaCache = schemaCache || {};
            var schemaKey = schema.applicationName + "." + schema.schemaId;
            if (!schemaCache[schemaKey]) {
                schemaCache[schemaKey] = schema;
                sessionStorage["schemaCache"] = JSON.stringify(schemaCache);
            }
        }

        function wipeSchemaCache() {
            schemaCache = {};
            delete sessionStorage["schemaCache"];
        }

        //#endregion

        //#region Service Instance

        var service = {
            getSchemaCacheKeys: getSchemaCacheKeys,
            addSchemaToCache: addSchemaToCache,
            getCachedSchema: getCachedSchema,
            getSchemaFromResult: getSchemaFromResult,
            wipeSchemaCache: wipeSchemaCache
        };

        return service;

        //#endregion
    }

 

})();
