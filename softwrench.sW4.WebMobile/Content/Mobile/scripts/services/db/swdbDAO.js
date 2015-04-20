var entities = {};
mobileServices.factory('swdbDAO', function (dispatcherService) {

    //creating namespace for the entities, to avoid eventaul collisions

    function createFilter(entity, queryString, queryoptions) {
        queryoptions = queryoptions || {};
        var pageNumber = queryoptions.pageNumber || 1;
        var pageSize = queryoptions.pagesize;

        if (!entities[entity]) {
            throw new Error("entity {0} not found".format(entity));
        }
        var filter = entities[entity].all();
        if (pageSize) {
            filter = filter.limit(pageSize);
            filter = filter.skip((pageSize * (pageNumber - 1)));
        }
        if (queryString) {
            filter._additionalWhereSqls.push(queryString);
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


            entities.Application = persistence.define('Application', {
                application: 'TEXT',
                data: "JSON"
            });

            ///
            /// Holds both Parent entities and compositions data.
            /// This Entries needs to be reevaluated on every sync, since chances are that they do not needed to be present on client side after execution.
            ///
            entities.DataEntry = persistence.define('DataEntry', {
                application: 'TEXT',
                datamap: 'JSON',
                //used for composition matching, when data comes from the server, it will be on a tabular format (e.g all worklogs); 
                //composite applications (root), however, will have this as null
                parentId: 'TEXT',
                //The id of this entry in maximo, it will be null when it´s created locally
                remoteId: 'TEXT',
                //if this flag is true, it will indicate that some change has been made to this entry locally, and it will appear on the pending sync dashboard
                isDirty: 'BOOL',
                rowstamp: 'DATE',
                problem: 'BOOL',
                problemReason: "TEXT"
            });

            //this entity stores the Data which will be used as support of the main entities.
            //It cannot be created locally. Hence Synchronization should use rowstamp caching capabilities by default
            entities.AssociationData = persistence.define('AssociationData', {
                application: 'TEXT',
                label: 'TEXT',
                value: 'TEXT',
                projectionfields: 'JSON',
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


            entities.AssociationCache = persistence.define('AssociationCache', {
                ///Holds an object that for each application will have properties that will help the framework to understaand how to proceed upon synchronization:
                /// <ul>
                ///     <li>maximorowstamp --> the rowstamp of the last data fetched from maximo of this application</li>
                ///     <li>whereclausehash--> the md5 hash of the latest applied whereclause used to pick this data, or null, if none present
                ///             The framework will compare to see if the wc has changed, invalidating the entire cache in that case.</li>
                ///     <li>syncschemahash --> the hash of the sync schema used for last synchronization. It could have changed due to a presence of a new projection field, for instance</li>
                /// </ul>
                ///ex:
                /// {
                ///    location:{
                ///       maximorowstamp: 1000
                ///       whereclausehash:'abasacvsava'
                ///       syncschemahash:'asadfasdfasdfasdfasdfadsfasdfasdfamlmlmlsdafsadfassfdafa'
                ///    }
                /// 
                ///

                ///    asset:{
                ///       maximorowstamp: 12500
                ///       whereclausehash:'hashoflatestappliedwhereclause'
                ///       syncschemahash:baadfasdfasdfa
                ///    }
                //        .
                //        .
                //        .
                // }
                data: "JSON"
            });

            entities.DataEntry.index(['application', 'remoteid'], { unique: true });
            entities.DataEntry.index(['application', 'parentId']);

            entities.AssociationData.index(['application', 'value']);

            entities.Menu = persistence.define('Menu', {
                data: "JSON"
            });

            entities.SyncStatus = persistence.define('SyncStatus', {
                lastsynced: "DATE",
                lastsyncServerVersion: "TEXT"
            });



            persistence.debug = true;
            persistence.schemaSync();

        },


        /// <summary>
        /// 
        /// </summary>
        /// <param name="entity">the name of the entity to load</param>
        /// <param name="memoryObject">the object to take as a parameter, so that if it contains an id, that will be used to try to load the persistent instance from cache,
        ///  otherwise a fresh new copy will be used, with all its properties merged into the persistent instance.</param>
        /// <returns type="promise">returns a promise that will pass the loaded instance to the chain</returns>
        instantiate: function (entity, memoryObject,mergingFunction) {



            if (!entities[entity]) {
                throw new Error("entity {0} not found".format(entity));
            }

            memoryObject = memoryObject || {};

            var deferred = dispatcherService.loadBaseDeferred();


            var ob = entities[entity];
            if (memoryObject.id == null) {
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

        findById: function (entity, id, cbk) {
            if (!entities[entity]) {
                throw new Error("entity {0} not found".format(entity));
            }
            var filter = entities[entity].all().filter("id", '=', id);
            filter.list(null, function (result) {
                //single result expected
                cbk(result[0]);
            });
        },

        /// <summary>
        /// 
        /// </summary>
        /// <param name="entity"></param>
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

            filter.list(null, function (result) {
                deferred.resolve(result);
            });
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
                persistence.flush(tx, function () {
                    deferred.resolve();
                });
            } else {
                persistence.flush(function () {
                    deferred.resolve();
                });
            }
            return promise;

        },


        bulkSave: function (objArray) {
            for (var i = 0; i < objArray.length; i++) {
                persistence.add(objArray[i]);
            }
            var deferred = dispatcherService.loadBaseDeferred();
            var promise = deferred.promise;
            persistence.flush(function () {
                deferred.resolve();
            });
            return promise;

        },

        bulkDelete: function (objArray) {
            var deferred = dispatcherService.loadBaseDeferred();
            var promise = deferred.promise;
            if (!objArray || objArray.length == 0) {
                deferred.resolve();
                return promise;
            }
            for (var i = 0; i < objArray.length; i++) {
                persistence.remove(objArray[i]);
            }
            persistence.flush(function () {
                deferred.resolve();
            });
            return promise;

        },

        executeQuery: function (query) {
            var deferred = dispatcherService.loadBaseDeferred();
            var promise = deferred.promise;
            persistence.transaction(function (tx) {
                var queries = [];
                var queryTuple = [];
                queryTuple.push(query);
                queries.push(queryTuple);
                persistence.executeQueriesSeq(tx, queries, function () {
                    deferred.resolve();
                });
            });
            return promise;
        }



    };

});