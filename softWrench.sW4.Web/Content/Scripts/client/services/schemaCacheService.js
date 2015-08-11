﻿
(function () {
    "use strict";

    //#region Service registration

    angular.module("sw_layout").factory("schemaCacheService", ["$log","contextService", schemaCacheService]);

    //#endregion

    function schemaCacheService($log,contextService) {

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
            $log.get("schemaCacheService#getSchemaCacheKeys").debug("schema keys in cache {0}".format(result));
            return result;
        }

        function getSchemaFromResult(result) {
            if (result.cachedSchemaId) {
                $log.get("schemaCacheService#getSchemaFromResult").info("schema {0}.{1} retrieved from cache".format(result.applicationName, result.cachedSchemaId));
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
                var systeminitMillis = contextService.getFromContext("systeminittime");
                $log.get("schemaCacheService#addSchemaToCache").info("adding schema {0} retrieved to cache".format(schemaKey));
                schemaCache[schemaKey] = schema;
                schemaCache.systeminitMillis = systeminitMillis;
                sessionStorage["schemaCache"] = JSON.stringify(schemaCache);
            }
        }

        function wipeSchemaCacheIfNeeded(forceClean) {
            var systeminitMillis = contextService.getFromContext("systeminittime");
            if (forceClean || (schemaCache && schemaCache.systeminitMillis !== systeminitMillis)) {
                $log.get("schemaCacheService#wipeSchemaCacheIfNeeded").info("wiping out schema cache");
                delete sessionStorage["schemaCache"];
                schemaCache = {
                    systeminitMillis: systeminitMillis
                };
            }
        }

        //#endregion

        //#region Service Instance

        var service = {
            getSchemaCacheKeys: getSchemaCacheKeys,
            addSchemaToCache: addSchemaToCache,
            getCachedSchema: getCachedSchema,
            getSchemaFromResult: getSchemaFromResult,
            wipeSchemaCacheIfNeeded: wipeSchemaCacheIfNeeded
        };

        return service;

        //#endregion
    }



})();
