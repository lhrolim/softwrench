(function (mobileServices) {
    "use strict";

    function settingsService(localStorageService, dao, $q) {
        //#region Utils
        var config = {
            storageNamespace: "settings:"
        }
        function key(str) {
            return config.storageNamespace + str;
        }
        function cacheSettings(settings) {
            if (!settings) {
                return settings;
            }
            localStorageService.put(key("settings"), settings);
            if(!!settings.serverurl) localStorageService.put(key("serverurl"), settings.serverurl);
            return settings;
        }
        //#endregion

        //#region Public methods
        /**
         * Finds the saved Settings entity
         * (uses localStorage as cache for performance).
         * 
         * @returns Promise resolved with the saved Settings entity
         */
        function getSettings() {
            const settings = localStorageService.get(key("settings"));
            return !!settings
                // resolve immediately on cache hit
                ? $q.when(settings)
                // fetch from DB on cache miss and cache result
                : dao.findSingleByQuery("Settings", null).then(cacheSettings); 
        }

        function getServerUrl() {
            return getSettings().then(settings => settings.serverurl);
        }
        /**
         * Saves the settings as Settings entity to local database 
         * (uses localStorage as cache for performance).
         * 
         * @param {} settings 
         * @returns Promise resolved with the saved Settings entity 
         */
        function saveSettings(settings) {
            return dao.instantiate("Settings", settings)
                .then(settingsInstance => dao.save(settingsInstance))
                .then(cacheSettings);
        }

        /**
         * Checks the database for a saved Settings entry.
         * If there are none save an empty entry and resolve with it.
         * Otherwise resolve with the entry found.
         * 
         * @returns Promise resolved with saved Settings entity
         */
        function initializeSettings() {
            return dao.findAll("Settings").then(function(settings) {
                return settings.length <= 0
                    ? saveSettings(null) // save 'empty' Settings to DB and cache it
                    : cacheSettings(settings[0]); // already has an entry -> just cache it
            });
        }
        //#endregion

        //#region Service Instance
        const service = {
            getSettings,
            getServerUrl,
            saveSettings,
            initializeSettings
        };
        return service;
        //#endregion
    }

    //#region Service registration
    mobileServices.factory("settingsService", ["localStorageService", "swdbDAO", "$q", settingsService]);
    //#endregion

})(mobileServices);
