(function (angular) {
    "use strict";

    function localStorageService() {

        //#region Utils
        var buildEntry = function(data, options) {
            var ttl = options && options.ttl ? options.ttl : null;
            var compress = options && options.compress ? options.compress : false;
            var entry = { data: data };
            if (!!ttl) {
                entry.expires = new Date().getTime() + ttl;
            }
            if (compress) {
                entry.compressed = true;
                entry.parse = !angular.isString(data);
                // data as a compressed string
                var dataString = entry.parse ? JSON.stringify(data) : data;
                entry.data = window.LZString.compressToUTF16(dataString);
            }
            return JSON.stringify(entry);
        }

        var validateParam = function(name, value) {
            if (!value) throw new Error("{0} cannot be null nor undefined".format(name));
        };

        var doGet = function(key) {
            var now = new Date().getTime();
            var entryString = localStorage.getItem(key);
            // no entry found for key
            if (!entryString) return null;

            var entry = JSON.parse(entryString);
            var expires = entry["expires"];

            // entry expired it's ttl
            if (!!expires && expires <= now) {
                localStorage.removeItem(key);
                return null;
            }
            // entry still within it's ttl or no ttl stablished
            var raw = entry["data"];
            if (entry.compressed) {
                var decompressed = window.LZString.decompressFromUTF16(raw);
                return !!entry.parse ? JSON.parse(decompressed) : decompressed;
            }
            return raw;
        };
        //#endregion

        //#region Public Methods

        /**
         * Puts key-data pair in the localStorage.
         * The entry can be configured with the options parameter.
         * See {@link localStorageService#get} to better understand the TTL feature.
         * 
         * @param String key 
         * @param {} data 
         * @param {} options dictionary of options: 
         *          {
         *              ttl: Number //entry's time-to-live in the cache in milliseconds, defaults to null (infinite TTL)
         *              compress: Boolean // indicates whether or not the data should be compressed, defaults to false (don't compress)
         *          } 
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
