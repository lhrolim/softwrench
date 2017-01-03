(function (angular) {
    "use strict";

    function localStorageService(compressionService) {

        //#region Utils
        function buildEntry(data, options) {
            const ttl = options && options.ttl ? options.ttl : null;
            const compress = options && options.compress ? options.compress : false;
            const entry = { data: data };
            if (!!ttl) {
                entry.expires = new Date().getTime() + ttl;
            }
            if (compress) {
                entry.compressed = true;
                entry.parse = !angular.isString(data);
                // data as a compressed string
                const dataString = entry.parse ? JSON.stringify(data) : data;
                entry.data = compressionService.compress(dataString);
            }
            return JSON.stringify(entry);
        }

        function validateParam(name, value) {
            if (value !== 0 && !value) throw new Error(`${name} cannot be null nor undefined`);
        };

        function doGet(key) {
            const now = new Date().getTime();
            const entryString = localStorage.getItem(key);
            // no entry found for key
            if (!entryString) return null;

            const entry = JSON.parse(entryString);
            const expires = entry["expires"];

            // entry expired it's ttl
            if (!!expires && expires <= now) {
                localStorage.removeItem(key);
                return null;
            }
            // entry still within it's ttl or no ttl stablished
            const raw = entry["data"];
            if (entry.compressed) {
                const decompressed = compressionService.decompress(raw);
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
        function put(key, data, options) {
            validateParam("key", key);
            validateParam("data", data);
            const entry = buildEntry(data, options);
            localStorage.setItem(key, entry);
        };

        /**
         * Fetches the data of the entry with the corresponding key.
         * If the entry's TTL is expired will return null instead.
         * 
         * @param String key
         * @returns {} data set by {@link localStorageService#put} 
         */
        function get(key) {
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
        function remove(key) {
            validateParam("key", key);
            const data = doGet(key);
            localStorage.removeItem(key);
            return data;
        };
        //#endregion

        //#region Service Instance
        const service = {
            put,
            get: get,
            remove
        };
        return service;
        //#endregion
    }

    //#region Service registration
    angular.module("webcommons_services").service("localStorageService", ["compressionService", localStorageService]);
    //#endregion

})(angular);
