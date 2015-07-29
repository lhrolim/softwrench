﻿//#region Provider: offlineEntities
(function(angular, persistence) {
    /**
     * entities: exposed as provider propertiy so it can be configured by the modules that use them.
     * Usage in config:
     * angular.module("myApp", ["persistence.offline"]).config(["offlineEntitiesProvider", function(entitiesProvider){
     *     entitiesProvider.entities.MyTable = persistence.define("MyTable", { ... }); 
     * }]);
     * Usage in run, service, controller, etc:
     * app.controller("MyCtrl", ["$scope", "offlineEntities", function($scope, entities){
     *      $scope.newMyTable = function(){
     *          var record = new entities.MyTable();
     *          // ...
     *      };
     * }])
     */
    angular.module("persistence.offline").provider("offlineEntities", [function () {
        // service instance
        var entities = {};

        var provider = {
            // access to the service instance in config time
            entities: entities,
            // alias to persistence.define
            define: function (name, options) {
                entities[name] = persistence.define(name, options);
            },
            // service constructor
            $get: [function () {
                return entities;
            }]
        };
        return provider;
    }]);

})(angular, persistence);


//#endregion

//#region offlinePersitenceBootstrap
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
//#endregion

//#region swdbDAO
(function (angular, persistence) {
    "use strict";

    angular.module("persistence.offline").factory("swdbDAO", ["$q", "offlineEntities", function ($q, entities) {

        //creating namespace for the entities, to avoid collisions

        function getInstance(entity) {
            if (!entities[entity]) {
                throw new Error("entity {0} not found".format(entity));
            }
            //return persistence.define(entity);
            return entities[entity];
        }

        function createFilter(entity, queryString, queryoptions) {
            queryoptions = queryoptions || {};
            var pageNumber = queryoptions.pageNumber || 1;
            var orderProperty = queryoptions.orderby || null;
            var orderascending = queryoptions.orderbyascending;
            var pageSize = queryoptions.pagesize;
            var projectionFields = queryoptions.projectionFields || [];
            var queryToUse = queryoptions.fullquery;
            var prefetch = queryoptions.prefetch;

            var filter = getInstance(entity).all();


            if (pageSize) {
                filter = filter.limit(pageSize);
                filter = filter.skip((pageSize * (pageNumber - 1)));
            }

            filter._additionalWhereSqls = [];
            filter._projectionFields = [];
            filter._querytoUse = null;

            if (queryString) {
                filter._additionalWhereSqls.push(queryString);
            }
            if (projectionFields.length > 0) {
                filter._projectionFields = projectionFields;
            }
            if (prefetch) {
                filter = filter.prefetch(prefetch);
            }
            if (orderProperty) {
                filter = filter.order(orderProperty, orderascending);
            }
            if (queryToUse) {
                filter._querytoUse = queryToUse;
            }
            return filter;

        }

        return {

            /// <summary>
            /// 
            /// </summary>
            /// <param name="entity">the name of the entity to load</param>
            /// <param name="memoryObject">the object to take as a parameter, so that if it contains an id, that will be used to try to load the persistent instance from cache,
            ///  otherwise a fresh new copy will be used, with all its properties merged into the persistent instance.</param>
            /// <returns type="promise">returns a promise that will pass the loaded instance to the chain</returns>
            instantiate: function (entity, memoryObject, mergingFunction) {
                if (!entities[entity]) {
                    throw new Error("entity {0} not found".format(entity));
                }

                memoryObject = memoryObject || {};

                var deferred = $q.defer();


                var ob = entities[entity];
                if (memoryObject.id == null || (memoryObject._type && memoryObject._type != entity)) {
                    //if the memory object doesn´t contain an id, then we don´t need to check on persistence cache, 
                    //just instantiate a new one
                    var transientEntity = new ob();
                    if (mergingFunction) {
                        deferred.resolve(mergingFunction(memoryObject, transientEntity));
                    } else {
                        deferred.resolve(mergeObjects(memoryObject, transientEntity));
                    }
                    return deferred.promise;
                }


                //since it has an id, there´s a chance it´s present on session cache
                ob.load(memoryObject.id, function (loadedObject) {
                    if (!loadedObject) {
                        //if not found in cache, let´s instantiate a new one anyway
                        loadedObject = new ob();
                    }
                    if (mergingFunction) {
                        deferred.resolve(mergingFunction(memoryObject, loadedObject));
                    } else {
                        deferred.resolve(mergeObjects(memoryObject, loadedObject));
                    }
                });

                return deferred.promise;;
            },

            findById: function (entity, id) {
                var deferred = $q.defer();
                var dbEntity = entities[entity];
                if (!dbEntity) {
                    deferred.reject(new Error("entity {0} not found".format(entity)));
                }
                dbEntity.findBy("id", id, function (result) {
                    //single result expected
                    if (!result || result.length <= 0) {
                        deferred.reject(new Error("no {0} found with 'id' = {1}".format(entity, id)));
                    } else {
                        deferred.resolve(result);
                    }
                });
                return deferred.promise;
            },

            /// <summary>
            /// 
            /// </summary>
            /// <param name="entity">The name of the table to lookup</param>
            /// <param name="options">
            /// 
            ///  pagesize: number of items per page. if undefined, it will bring all the results
            ///  pagenumber: page to fetch. if undefined, no limit will 
            /// 
            /// </param>
            /// <returns type=""></returns>
            findAll: function (entity, options) {
                var deferred = $q.defer();
                var filter = createFilter(entity, null, options);

                filter.list(null, function (result) {
                    deferred.resolve(result);
                });
                return deferred.promise;
            },

            findByQuery: function (entity, queryString, options) {
                var deferred = $q.defer();
                var filter = createFilter(entity, queryString, options);

                try {
                    filter.list(null, function (result) {
                        deferred.resolve(result);
                    });
                } catch (err) {
                    deferred.reject(err);
                }
                return deferred.promise;
            },

            findUnique: function (entity) {
                var deferred = $q.defer();
                var promise = deferred.promise;
                this.findAll(entity).then(function (result) {
                    if (result.length == 0) {
                        deferred.resolve(null);
                    } else {
                        deferred.resolve(result[0]);
                    }
                });
                return promise;
            },

            save: function (obj, tx) {
                persistence.add(obj);

                var deferred = $q.defer();
                var promise = deferred.promise;
                if (tx) {
                    //flush has to be called from the outside
                    deferred.resolve();
                    return promise;
                } else {
                    persistence.flush(function () {
                        deferred.resolve(obj);
                    });
                }
                return promise;

            },


            createTx: function (args) {
                var deferred = $q.defer();
                persistence.transaction(function (tx) {
                    if (args) {
                        deferred.resolve([tx, args]);
                    } else {
                        deferred.resolve([tx]);
                    }
                });
                return deferred.promise;
            },

            bulkSave: function (objArray, tx) {
                if (objArray == null) {
                    return $q.when();
                }

                for (var i = 0; i < objArray.length; i++) {
                    persistence.add(objArray[i]);
                }
                var deferred = $q.defer();
                var promise = deferred.promise;
                if (tx) {
                    //flush has to be called from the outside
                    deferred.resolve(objArray);
                    return promise;
                }
                persistence.flush(function () {
                    deferred.resolve(objArray);
                });
                return promise;

            },

            bulkDelete: function (objArray, tx) {
                var deferred = $q.defer();
                var promise = deferred.promise;
                if (!objArray || objArray.length == 0) {
                    deferred.resolve();
                    return promise;
                }
                for (var i = 0; i < objArray.length; i++) {
                    persistence.remove(objArray[i]);
                }
                if (tx) {
                    //flush has to be called from the outside
                    deferred.resolve(objArray);
                    return promise;
                }
                persistence.flush(function () {
                    deferred.resolve();
                });
                return promise;

            },

            executeQuery: function (query, tx) {
                return this.executeQueries([query], tx);
            },

            executeQueries: function (queriesToExecute, tx) {
                var deferred = $q.defer();
                var promise = deferred.promise;

                var queries = queriesToExecute.map(function (query) {
                    if (angular.isString(query)) {
                        // using a formatted query String: tuple as [formatted query String, undefined]
                        // TODO: deprecate it
                        return [query];
                    }
                    // using "prepared statement": tuple as [statement, query arguments]
                    return [query.query, query.args];
                });

                if (!tx) {
                    persistence.transaction(function (closureTx) {
                        persistence.executeQueriesSeq(closureTx, queries, function (res, err) {
                            if (err) {
                                deferred.reject(err);
                            } else {
                                deferred.resolve(res);
                            }
                        });
                    });
                } else {
                    persistence.executeQueriesSeq(tx, queries, function (res, err) {
                        if (err) {
                            deferred.reject(err);
                        } else {
                            deferred.resolve(res);
                        }
                    });
                }
                return promise;
            },

            findSingleByQuery: function (entity, query, options) {
                var optionsToUse = angular.copy(options);
                optionsToUse.pagesize = 1;
                optionsToUse.pageNumber = 1;
                return this.findByQuery(entity, query, options)
                    .then(function (results) {
                        if (!results || results.length <= 0) {
                            return null;
                        }
                        return results[0];
                    });
            },

            /**
             * Counts the number of results the querry would return.
             * (it uses a count statement, it doesn't actually perform the query to measure the array length).
             * 
             * @param String entity 
             * @param String query 
             * @returns Promise resolved with the count value 
             */
            countByQuery: function (entity, query) {
                var deferred = $q.defer();
                var filter = createFilter(entity, query);
                console.log("before:");
                console.log(query);
                console.log(filter._additionalWhereSqls);
                filter.count(function (count) {
                    console.log("after:");
                    console.log(query);
                    console.log(filter._additionalWhereSqls);
                    deferred.resolve(count);
                });
                return deferred.promise;
            },

            /**
             * Deletes all entries for every entity in the database (every entity registered in window.entities).
             * 
             * @returns Promise 
             */
            resetDataBase: function () {
                var queries = [];
                for (var entity in entities) {
                    if (!entities.hasOwnProperty(entity)) continue;
                    queries.push("delete from {0}".format(entity));
                }
                return this.executeQueries(queries);
            },

            /**
             * Deletes all entries of the entity in the database.
             * 
             * @param String entity 
             * @returns Promise 
             */
            deleteTable: function (entity) {
                if (!entities[entity]) {
                    throw new Error("entity {0} not found".format(entity));
                }
                return this.executeQuery("delete from {0}".format(entity));
            },

            /**
             * Drops all entities in the database (every entity registered in window.entities).
             * 
             * @returns Promise 
             */
            dropDataBase: function () {
                var queries = [];
                for (var entity in entities) {
                    if (!entities.hasOwnProperty(entity)) continue;
                    queries.push("drop table if exists `{0}`".format(entity));
                }
                return this.executeQueries(queries);
            },

            /**
             * Drops and recreates all entities in the database.
             * 
             * @returns Promise 
             */
            recreateDataBase: function () {
                var deferred = $q.defer();
                persistence.reset(null, function (res, err) {
                    if (err) {
                        deferred.reject(err);
                    } else {
                        deferred.resolve(res);
                    }
                });
                return deferred.promise;
            },

            /**
             * Executes a sql statement.
             * 
             * @param String statement 
             * @param [] args 
             * @returns Promise: resolved with result, rejected with database error 
             */
            executeStatement: function(statement, args) {
                return persistence.runSql(statement, args);
            }

        };

    }]);

})(angular, persistence);
//#endregion