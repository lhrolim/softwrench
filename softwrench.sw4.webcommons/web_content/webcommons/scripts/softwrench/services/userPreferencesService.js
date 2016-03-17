
(function (angular) {
    "use strict";

    function userPreferencesService(localStorageService, contextService, crudContextHolderService) {
        //#region Utils
        function getInnerKey(key) {
            var innerKey = "userpref." + contextService.getUserData().dbId + "." + key;
            return innerKey.toLowerCase();
        }
        //#endregion

        //#region Public methods

        function syncPreference(container, attr, key) {
            if (typeof container[attr] !== "undefined" && typeof getPreference(key) === "undefined") {
                setPreference(key, container[attr]);
            }

            Object.defineProperty(container, attr, {
                get: function() {
                    return getPreference(key);
                },
                set: function (value) {
                    setPreference(key, value);
                }
            });
        }

        function syncSchemaPreference(container, attr, baseKey, applicationName, schemaId, panelid) {
            var key = buildSchemaKey(baseKey, applicationName, schemaId, panelid);
            syncPreference(container, attr, key);
        }

        function syncCrudPreference(container, attr, baseKey, panelid) {
            var key = buildCrudKey(baseKey, panelid);
            syncPreference(container, attr, key);
        }

        function syncApplicationPreference(container, attr, baseKey, applicationName) {
            var key = buildApplicationKey(baseKey, applicationName);
            syncPreference(container, attr, key);
        }

        function getPreference(key) {
            var innerKey = getInnerKey(key);
            if (typeof localStorage[innerKey] === "undefined") {
                return undefined;
            }
            return JSON.parse(localStorage[innerKey]);
        }

        function getSchemaPreference(baseKey, applicationName, schemaId, panelid) {
            var key = buildSchemaKey(baseKey, applicationName, schemaId, panelid);
            return getPreference(key);
        }

        function getCrudPreference(baseKey, panelid) {
            var key = buildCrudKey(baseKey, panelid);
            return getPreference(key);
        }

        function getApplicationPreference(baseKey, applicationName) {
            var key = buildApplicationKey(baseKey, applicationName);
            return getPreference(key);
        }

        function setPreference(key, value) {
            var innerKey = getInnerKey(key);
            if (typeof value === "undefined") {
                localStorage[innerKey] = null;
                return;
            }
            localStorage[innerKey] = JSON.stringify(value);
        }

        function setSchemaPreference(baseKey, value, applicationName, schemaId, panelid) {
            var key = buildSchemaKey(baseKey, applicationName, schemaId, panelid);
            setPreference(key, value);
        }

        function setCrudPreference(baseKey, value, panelid) {
            var key = buildCrudKey(baseKey, panelid);
            setPreference(key, value);
        }

        function setApplicationPreference(baseKey, value, applicationName) {
            var key = buildApplicationKey(baseKey, applicationName);
            setPreference(key, value);
        }

        function buildSchemaKey(baseKey, applicationName, schemaId, panelid) {
            var key = applicationName;
            if (schemaId) {
                key += "." + schemaId;
            }
            if (panelid) {
                key += "." + panelid;
            }
            key += "." + baseKey;
            return key;
        }

        function buildCrudKey(baseKey, panelid) {
            var schema = crudContextHolderService.currentSchema(panelid);
            return buildSchemaKey(baseKey, schema.applicationName, schema.schemaId, panelid);
        }

        function buildApplicationKey(baseKey, applicationName) {
            return buildSchemaKey(baseKey, applicationName);
        }

        //#endregion

        //#region Service Instance
        var service = {
            syncPreference: syncPreference,
            syncSchemaPreference: syncSchemaPreference,
            syncCrudPreference: syncCrudPreference,
            syncApplicationPreference: syncApplicationPreference,
            getPreference: getPreference,
            getSchemaPreference: getSchemaPreference,
            getCrudPreference: getCrudPreference,
            getApplicationPreference: getApplicationPreference,
            setPreference: setPreference,
            setSchemaPreference: setSchemaPreference,
            setCrudPreference: setCrudPreference,
            setApplicationPreference: setApplicationPreference
        };
        return service;
        //#endregion
    }

    //#region Service registration

    angular.module("webcommons_services").factory("userPreferencesService", ["localStorageService", "contextService", "crudContextHolderService", userPreferencesService]);

    //#endregion

})(angular);