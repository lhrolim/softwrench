(function () {
    "use strict";

    function schemaCacheService($log, contextService, localStorageService) {
        //#region Utils
        var schemaCache = {};
        var keyRoot = url("") + ":schemaCache:";
        var systemInitTimeKey = keyRoot + "systeminitMillis";

        function restore() {
            delete localStorageService[url("") + ":schemaCache"]; // deleting previous version cache
            // lazy schema fetch strategy: only restore the systeminitmillis
            schemaCache.systeminitMillis = localStorage.getItem(systemInitTimeKey);

            //var log = $log.get("schemaCacheService#restore", ["performance"]);
            //log.debug("starting schema restore process");
            //var urlContext = url("");
            //var schemaCacheJson = localStorage[urlContext + ":schemaCache"];
            //if (schemaCacheJson) {
            //    schemaCache = JSON.parse(schemaCacheJson);
            //}
            //log.debug("finished schema restore took ");
        }

        function schemaStorageKey(applicationName, schemaId) {
            return keyRoot + applicationName + "." + schemaId;
        }
        //#endregion

        //#region Public methods
        function getSchemaCacheKeys() {
            if (sessionStorage.ignoreSchemaCache === "true") {
                return ";";
            }
            var result = ";" + Object.keys(schemaCache).join(";") + ";";
            $log.get("schemaCacheService#getSchemaCacheKeys").debug("schema keys in cache {0}".format(result));
            return result;
        }

        function getSchemaFromResult(result) {
            if (result.cachedSchemaId) {
                var log = $log.get("schemaCacheService#getSchemaFromResult",["performance"]);
                log.info("schema {0}.{1} retrieved from cache".format(result.applicationName, result.cachedSchemaId));
                var cachedSchema = getCachedSchema(result.applicationName, result.cachedSchemaId);
                log.info("finish retrieving from cache".format(result.applicationName, result.cachedSchemaId));
                return cachedSchema;
            }
            return result.schema;
        }

        function getCachedSchema(applicationName, schemaId) {
            var schema = schemaCache[applicationName + "." + schemaId];
            if (!schema) {
                var storageKey = schemaStorageKey(applicationName, schemaId);
                schema = localStorageService.get(storageKey);
            }
            return schema;
        }

        function addSchemaToCache(schema) {
            if (!schema) return;

            var log = $log.get("schemaCacheService#addSchemaToCache", ["performance"]);

            var schemaKey = schema.applicationName + "." + schema.schemaId;
            if (!!schemaCache[schemaKey]) return; // already in the cache

            log.info("adding schema {0} retrieved to cache".format(schemaKey));
            var systeminitMillis = contextService.getFromContext("systeminittime");
            schemaCache[schemaKey] = schema;
            schemaCache.systeminitMillis = systeminitMillis;

            var storageKey = schemaStorageKey(schema.applicationName, schema.schemaId);
            try {
                localStorage.setItem(systemInitTimeKey, systeminitMillis); // plain localStorage for performance
                localStorageService.put(storageKey, schema, { compress: true });
                log.info("finishing adding schema {0} retrieved to cache".format(schemaKey));
            } catch (e) {
                log.warn("localStorage is full... avoiding cache");
            }
        }

        function wipeSchemaCacheIfNeeded(forceClean) {
            var systeminitMillis = contextService.getFromContext("systeminittime");
            if (forceClean || (schemaCache && schemaCache.systeminitMillis !== systeminitMillis)) {
                $log.get("schemaCacheService#wipeSchemaCacheIfNeeded").info("wiping out schema cache");

                delete localStorageService[url("") + ":schemaCache"]; // deleting previous version cache

                Object.keys(localStorage)
                    .filter(function(key) {
                        key.startsWith(keyRoot);
                    })
                    .forEach(function (schemakey) {
                        delete localStorage[schemakey];
                    });


                schemaCache = { systeminitMillis: systeminitMillis };
            }
        }
        //#endregion

        //#region Service Instance
        restore();
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
    angular.module("sw_layout").factory("schemaCacheService", ["$log", "contextService", "localStorageService", schemaCacheService]);
    //#endregion

})();