(function (mobileServices, angular) {
    "use strict";

    function applicationStateService(swdbDAO, $q, entities, configurationService, $cordovaAppVersion) {

        //#region Utils

        var countAll = function (app) {
            var deferred = $q.defer();

            entities["DataEntry"]
                .all()
                .filter("application", "=", app)
                .count(function (count) {
                    deferred.resolve(count);
                });

            return deferred.promise;
        };

        var countPending = function (app) {
            var deferred = $q.defer();

            entities["DataEntry"]
                .all()
                .filter("application", "=", app)
                .filter("pending", "=", true)
                .count(function (count) {
                    deferred.resolve(count);
                });

            return deferred.promise;
        };

        var dirtyAndProblematicCount = function (app) {
            return swdbDAO.findByQuery("DataEntry", "application='{0}' and isDirty=1".format(app))
                // all dirty entries
                .then(function (entries) {
                    // no dirty entries means no problematic entries
                    if (!entries || entries.length <= 0) {
                        return { dirty: 0, problematic: 0 };
                    }

                    var ids = entries.map(function (entry) {
                        return "'{0}'".format(entry.id);
                    });
                    return swdbDAO.countByQuery("BatchItem", "dataentry in ({0}) and problem is not null".format(ids))
                        // count of problematic batches related to the entries
                        .then(function(problematic) {
                            var justDirty = entries.length - problematic;
                            return { dirty: justDirty, problematic: problematic };
                        });
                });
        }

        //#endregion

        //#region Public methods

        /**
         * Fetches the status of each 'viewable' application (not association nor composition) in the system.
         * Each status is a dictionary: 
         * { 
         *  application: String, // application's name 
         *  all: Integer, // count of all entities
         *  pending: Integer, // count of pending entities 
         *  dirty: Integer, // count of all dirty entities
         *  problematic: Integer, // count of problematic entities 
         * }
         * 
         * @returns Promised resolved with array of each application's states 
         */
        function currentState() {
            return swdbDAO.findByQuery("Application", "composition=0 and association=0")
                .then(function(results) {
                    var apps = results.map(function(app) {
                        return app.application;
                    });

                    var promises = apps.map(function(app) {
                        var countPromises = [];

                        //TODO: verify (then fix) why using swdbdao made the first 2 queries be overriden by "application='<app>' and isDirty=1"
                        //var basequery = "application='{0}'".format(app);
                        //countPromises.push(swdbDAO.countByQuery("DataEntry", basequery)); // all
                        //countPromises.push(swdbDAO.countByQuery("DataEntry", basequery + " and isPending=1")); // pending

                        countPromises.push(countAll(app)); // all
                        countPromises.push(countPending(app)); // pending
                        countPromises.push(dirtyAndProblematicCount(app)); // dirty + problematic -> because problematic=dirty+batchitem.problem!==null

                        return $q.all(countPromises)
                            .then(function(counts) {
                                return {
                                    application: app,
                                    all: counts[0],
                                    pending: counts[1],
                                    dirty: counts[2].dirty,
                                    problematic: counts[2].problematic
                                };
                            });
                    });

                    return $q.all(promises);
                });

        }
        
        /**
         * Fetches the app's configuration (server and client info).
         * 
         * @returns Promise resolved with dictionary containing 'server' and 'client' configuration (both are dictionaries) 
         */
        function getAppConfig() {
            var serverConfigPromise = configurationService.getConfig("serverconfig");
            var clientVersionPromise =  isRippleEmulator() ? $q.when("Ripple") : $cordovaAppVersion.getAppVersion();
            return $q.all([serverConfigPromise, clientVersionPromise]).then(function (results) {
                var serverConfig = results[0];
                var appVersion = results[1];
                var config = {
                    'server': serverConfig,
                    'client': {
                        'version': appVersion
                    }
                };
                return config;
            });
        }

        //#endregion

        //#region Service Instance

        var service = {
            currentState: currentState,
            getAppConfig: getAppConfig
        };

        return service;

        //#endregion
    }

    //#region Service registration

    mobileServices.factory("applicationStateService", ["swdbDAO", "$q", "offlineEntities", "configurationService", "$cordovaAppVersion", applicationStateService]);

    //#endregion

})(mobileServices, angular);
