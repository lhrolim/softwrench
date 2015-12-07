(function (angular) {
    "use strict";

    function localStorageService() {

        //#region Utils

        var buildEntry = function(data, options) {
            var ttl = options && options.ttl ? options.ttl : null;
            var entry = { data: data };
            if (!!ttl) {
                entry.expires = new Date().getTime() + ttl;
            }
            return JSON.stringify(entry);
        }

        var validateParam = function(name, value) {
            if (!value) {
                throw new Error("{0} cannot be null nor undefined".format(name));
            }
        };

        var doGet = function(key) {
            var now = new Date().getTime();
            var entry = localStorage.getItem(key);
            // no entry found for key
            if (!entry) {
                return null;
            }
            var obj = JSON.parse(entry);
            var expires = obj["expires"];
            // no ttl established
            if (!expires) {
                return obj["data"];
            }
            // entry expired it's ttl
            if (expires <= now) {
                return null;
            }
            // entry still within it's ttl
            return obj["data"];
        };

        //#endregion

        //#region Public Methods

        /**
         * Puts key-data pair in the localStorage.
         * The entry can be configured with the options parameter.
         * Currently the only supported option is the entry's TTL (time to live) in milliseconds. 
         * See {@link localStorageService#get} to better understand the TTL feature.
         * 
         * @param String key 
         * @param {} data 
         * @param Object options 
         *                  dictionary of options e.g. { ttl: 24 * 60 * 60 * 1000 } gives the entry a TTL of one day
         */
        var put = function (key, data, options) {
            validateParam("key", key);
            validateParam("data", data);
            var entry = buildEntry(data, options);
            localStorage.setItem(key, entry);
        };

        /**
         * Fetches the data of the entry with the corresponding key.
         * If the entry's TTL is expired will return null instead.
         * 
         * @param String key
         * @returns {} data set by {@link localStorageService#put} 
         */
        var get = function(key) {
            validateParam("key", key);
            return doGet(key);
        };

        /**
         * Removes entry with the corresponding key and return it's data.
         * If the entry's TTL is expired will return null instead.
         * 
         * @param String key
         * @returns {} data that was removed
         */
        var remove = function(key) {
            validateParam("key", key);
            var data = doGet(key);
            localStorage.removeItem(key);
            return data;
        };

        //#endregion

        //#region Service Instance

        var service = {
            put: put,
            get: get,
            remove:remove
        };

        return service;

        //#endregion

    }

    //#region Service registration
    angular.module("webcommons_services").factory("localStorageService", [localStorageService]);
    //#endregion

})(angular);
