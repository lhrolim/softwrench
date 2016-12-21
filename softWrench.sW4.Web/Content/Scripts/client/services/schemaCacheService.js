(function () {
    "use strict";

    function schemaCacheService($log, contextService, localStorageService) {
        //#region Utils
        // schema's first-level cache
        var schemaCache = {};

        var keyRoot = url("") + ":schemaCache:";
        var systemInitTimeKey = keyRoot + "systeminitMillis";

        function restore() {
            delete localStorage[url("") + ":schemaCache"]; // deleting 'deprecated' cache model


            //wipe first-level cache
            schemaCache = {};
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
            var username = contextService.getUserData().login;
            return keyRoot + username + ":" + applicationName + "." + schemaId;
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
                var log = $log.get("schemaCacheService#getSchemaFromResult", ["performance"]);
                log.info("schema {0}.{1} retrieved from cache".format(result.applicationName, result.cachedSchemaId));
                var cachedSchema = getCachedSchema(result.applicationName, result.cachedSchemaId);
                log.info("finish retrieving from cache".format(result.applicationName, result.cachedSchemaId));
                return cachedSchema;
            }
            return result.schema;
        }

        function getCachedSchema(applicationName, schemaId) {
            var cacheKey = applicationName + "." + schemaId;
            var schema = schemaCache[cacheKey];
            if (!schema) {
                var storageKey = schemaStorageKey(applicationName, schemaId);
                schema = localStorageService.get(storageKey);
                schemaCache[cacheKey] = schema;
            }
            return schema;
        }

        function addSchemaToCache(schema) {
            if (!schema || !!schema.ignoreCache) return;

            var log = $log.get("schemaCacheService#addSchemaToCache", ["performance"]);

            var schemaKey = schema.applicationName + "." + schema.schemaId;
            if (!!schemaCache[schemaKey]) return; // already in the cache

            log.info("adding schema {0} retrieved to cache".format(schemaKey));
            var systeminitMillis = contextService.getFromContext("systeminittime");
            // let´s force a wipe before we update the systeminitMillis time
            this.wipeSchemaCacheIfNeeded();
            // in-memory first-level cache
            schemaCache[schemaKey] = schema;
            schemaCache.systeminitMillis = systeminitMillis;

            var storageKey = schemaStorageKey(schema.applicationName, schema.schemaId);
            try {
                // localStorage as second-level cache
                localStorage.setItem(systemInitTimeKey, systeminitMillis); // plain localStorage for performance and simpler data-structure
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

                delete localStorage[url("") + ":schemaCache"]; // deleting 'deprecated' cache model

                Object.keys(localStorage)
                    .filter(function (key) {
                        return key.startsWith(keyRoot);
                    })
                    .forEach(function (schemakey) {
                        delete localStorage[schemakey];
                    });
                //wipe first-level cache
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
    angular.module("sw_layout").service("schemaCacheService", ["$log", "contextService", "localStorageService", schemaCacheService]);
    //#endregion

})();