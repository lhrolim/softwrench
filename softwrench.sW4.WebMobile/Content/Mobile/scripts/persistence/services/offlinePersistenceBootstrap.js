(function (angular, persistence) {
    "use strict";

    /**
     * Service with "init" hook to initialize persistence's Database.
     * @constructor
     */
    function offlinePersitenceBootstrap($q) {
        //#region Utils
        var runSql = function (query, params) {
            var deferred = $q.defer();
            persistence.transaction(function (tx) {
                tx.executeSql(query, params,
                    function (results) {
                        if (persistence.debug) {
                            console.log(results);
                        }
                        deferred.resolve(results);
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
        //#endregion

        //#region Public methods
        function init() {
            // initializing database
            persistence.store.cordovasql.config(persistence, "offlineswdb", "1.0", "SWDB OffLine instance", 5 * 1024 * 1024, 0);
            persistence.debug = sessionStorage["logsql"] === "true" || sessionStorage["loglevel"] === "debug";
            persistence.schemaSync();
            // adding some functionalities to persistence
            persistence.runSql = runSql;
        }
        //#endregion

        //#region Service Instance
        var service = {
            init: init
        };

        return service;
        //#endregion
    }
    //#region Service registration
    angular.module("persistence.offline").factory("offlinePersitenceBootstrap", ["$q", offlinePersitenceBootstrap]);
    //#endregion


})(angular, persistence);