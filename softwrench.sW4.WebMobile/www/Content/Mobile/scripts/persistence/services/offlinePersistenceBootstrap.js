(function (angular, persistence) {
    "use strict";

    /**
     * Service with "init" hook to initialize persistence's Database.
     * @constructor
     */
    function offlinePersitenceBootstrap($rootScope, $q, $log, offlineMigrationService) {
        //#region Utils
        const listeners = [];
        var persistenceReady = false;

        const log = $log.get("offlinePersitenceBootstrap");

        var runSql = function (query, params) {
            var deferred = $q.defer();
            persistence.transaction(function (tx) {
                tx.executeSql(query, params,
                    function (results) {
                        if (persistence.debug) {
                            console.log(results);
                        }
                        deferred.resolve(results);
                        // workaround for tests
                        if (typeof (jasmine) !== "undefined" ) {
                            $rootScope.$digest();
                        }
                    }, function (cause) {
                        var msg = "An error ocurred when executing the query '{0}'".format(query);
                        if (params && params.length > 0) msg += " with parameters {0}".format(params);
                        var error = new Error(msg);
                        error.cause = cause;
                        console.error(error);
                        deferred.reject(error);
                    });
            });
            return deferred.promise;
        };

        const sendPersistenceReady = function() {
            persistenceReady = true;
            angular.forEach(listeners, (listener) => {
                listener.persistenceReady();
            });
            listeners.splice(0, listeners.length); // avoid memory leaks
        }
        //#endregion

        //#region Public methods
        function init() {
            log.info("Initing Persistence...");
            // initializing database
            persistence.store.cordovasql.config(persistence, "offlineswdb", "1.0", "SWDB OffLine instance", 5 * 1024 * 1024, 0);
            persistence.debug = sessionStorage["logsql"] === "true" || sessionStorage["loglevel"] === "debug";
            // adding some functionalities to persistence
            persistence.runSql = runSql;
            
            log.info("Start Migrations...");
            return offlineMigrationService.migrate().then(() => {
                log.info("Persistence Ready.");
                sendPersistenceReady();
            });
        }

        function addPersistenceReadyListener(listener) {
            if (!listener || !listener.persistenceReady) {
                return;
            }

            if (persistenceReady) {
                listener.persistenceReady();
            } else {
                listeners.push(listener);
            }
        }
        //#endregion

        //#region Service Instance
        const service = {
            init,
            addPersistenceReadyListener
        };

        return service;
        //#endregion
    }
    //#region Service registration
    angular.module("persistence.offline").factory("offlinePersitenceBootstrap", ["$rootScope", "$q", "$log", "offlineMigrationService", offlinePersitenceBootstrap]);
    //#endregion


})(angular, persistence);