var entities = entities || {};
mobileServices.factory('swdbDAO', function (dispatcherService) {

    //creating namespace for the entities, to avoid eventaul collisions

    function getInstance(entity) {
        if (!entities[entity]) {
            throw new Error("entity {0} not found".format(entity));
        }
        return persistence.define(entity);
    }

    function createFilter(entity, queryString, queryoptions) {
        queryoptions = queryoptions || {};
        var pageNumber = queryoptions.pageNumber || 1;
        var orderProperty = queryoptions.orderby || null;
        var orderascending = queryoptions.orderbyascending;
        var pageSize = queryoptions.pagesize;
        var projectionFields = queryoptions.projectionFields || [];
        var queryToUse = queryoptions.fullquery;

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
        if (orderProperty) {
            filter=filter.order(orderProperty, orderascending);
        }
        if (queryToUse) {
            filter._querytoUse = queryToUse;
        }
        return filter;

    }

    return {

        init: function () {

            persistence.store.cordovasql.config(persistence, 'offlineswdb', '1.0', 'SWDB OffLine instance', 5 * 1024 * 1024, 0);

            entities.Settings = persistence.define('Settings', {
                localversion: "TEXT",
                serverurl: 'TEXT',
            });

            entities.User = persistence.define('User', {
                name: 'TEXT',
                orgid: 'TEXT',
                siteid: 'TEXT',
            });

            entities.Configuration = persistence.define('Configuration', {
                key: 'TEXT',
                value: 'JSON',
            });


            entities.Application = persistence.define('Application', {
                application: 'TEXT',
                association: 'BOOL',
                composition: 'BOOL',
                data: "JSON"
            });

            entities.Attachments = persistence.define('Attachment', {
                rootentryRemoteId: 'TEXT',
                rootApplication: 'TEXT',
                doclinkId: 'TEXT',
                path: 'TEXT',
            });

            entities.WhereClause = persistence.define("WhereClause", {
                ///
                /// This should get populated only if, there are multiple whereclauses present for a given association.
                /// Ex: Asset has one WC for the SR, and another one for the WO. If a intersection (fallback condition exists) it should be registered on the asset application alone, though, or via the metadata.
                /// The framework will bring all Assets that match the base whereclause (only once) and match the specific whereclauses entries locally, so that the amount of data transported is minimized.
                ///
                application: "TEXT",
                parentApplication: "TEXT",
                metadataid: "TEXT",
                //the whereclause it self
                data: "TEXT",
            });

            entities.Menu = persistence.define('Menu', {
                data: "JSON"
            });

            persistence.debug = "true" == (sessionStorage["logsql"]) || sessionStorage.loglevel=="debug" ;
            persistence.schemaSync();

        },


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

            var deferred = dispatcherService.loadBaseDeferred();


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
            var deferred = dispatcherService.loadBaseDeferred();
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
            var deferred = dispatcherService.loadBaseDeferred();
            var filter = createFilter(entity, null, options);

            filter.list(null, function (result) {
                deferred.resolve(result);
            });
            return deferred.promise;
        },

        findByQuery: function (entity, queryString, options) {
            var deferred = dispatcherService.loadBaseDeferred();
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
            var deferred = dispatcherService.loadBaseDeferred();
            var promise = deferred.promise;
            this.findAll(entity).success(function (result) {
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

            var deferred = dispatcherService.loadBaseDeferred();
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
            var deferred = dispatcherService.loadBaseDeferred();
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
            for (var i = 0; i < objArray.length; i++) {
                persistence.add(objArray[i]);
            }
            var deferred = dispatcherService.loadBaseDeferred();
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
            var deferred = dispatcherService.loadBaseDeferred();
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
            var deferred = dispatcherService.loadBaseDeferred();
            var promise = deferred.promise;
            var queries = [];
            for (var i = 0; i < queriesToExecute.length; i++) {
                var query = queriesToExecute[i];
                var queryTuple = [];
                queryTuple.push(query);
                queries.push(queryTuple);
            }
            if (!tx) {
                persistence.transaction(function (closureTx) {
                    persistence.executeQueriesSeq(closureTx, queries, function () {
                        deferred.resolve();
                    });
                });
            } else {

                persistence.executeQueriesSeq(tx, queries, function () {
                    deferred.resolve();
                });
            }
            return promise;
        },




    };

});