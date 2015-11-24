
(function () {
    "use strict";



    function schemaCacheService($log,contextService) {

        //#region Utils

        var schemaCache = {

        }

        function restore() {
            var log = $log.get("schemaCacheService#restore", ["performance"]);
            var t0=performance.now();
            var urlContext = url("");
            var schemaCacheJson = localStorage[urlContext + ":schemaCache"];
            if (schemaCacheJson) {
                schemaCache = JSON.parse(schemaCacheJson);
            }
            var t1 = performance.now();
            log.debug("schema restore took ",t1-t0);

        }

        restore();

        //#endregion

        //#region Public methods


        function getSchemaCacheKeys() {
            if (sessionStorage.ignoreSchemaCache === "true") {
                return ";";
            }

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
                var log = $log.get("schemaCacheService#getSchemaFromResult",["performance"]);
                log.info("schema {0}.{1} retrieved from cache".format(result.applicationName, result.cachedSchemaId));
                var cachedSchema = this.getCachedSchema(result.applicationName, result.cachedSchemaId);
                log.info("finish retrieving from cache".format(result.applicationName, result.cachedSchemaId));
                return cachedSchema;
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
                var log = $log.get("schemaCacheService#addSchemaToCache",["performance"]);
                log.info("adding schema {0} retrieved to cache".format(schemaKey));
                schemaCache[schemaKey] = schema;
                schemaCache.systeminitMillis = systeminitMillis;
                var urlContext = url("");
                localStorage[urlContext + ":schemaCache"] = JSON.stringify(schemaCache);
                log.info("finishing adding schema {0} retrieved to cache".format(schemaKey));
            }
        }

        function wipeSchemaCacheIfNeeded(forceClean) {
            var systeminitMillis = contextService.getFromContext("systeminittime");
            if (forceClean || (schemaCache && schemaCache.systeminitMillis !== systeminitMillis)) {
                $log.get("schemaCacheService#wipeSchemaCacheIfNeeded").info("wiping out schema cache");
                delete localStorage["schemaCache"];
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

    //#region Service registration

    angular.module("sw_layout").factory("schemaCacheService", ["$log", "contextService", schemaCacheService]);

    //#endregion

})();
