var entities = {};
mobileServices.factory('swdbDAO', function (dispatcherService) {

    //creating namespace for the entities, to avoid eventaul collisions



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


            entities.Schema = persistence.define('Schema', {
                application: 'TEXT',
                key: 'TEXT',
                schema: "JSON"
            });


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

            entities.DataEntry.index(['application', 'remoteid'], { unique: true });
            entities.DataEntry.index(['application', 'parentId']);

            entities.Menu = persistence.define('Menu', {
                data: "JSON"
            });

            entities.SyncStatus = persistence.define('SyncStatus', {
                lastsynced: "DATE"
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
        instantiate: function (entity, memoryObject) {

            if (!entities[entity]) {
                throw new Error("entity {0} not found".format(entity));
            }

            var deferred = dispatcherService.loadBaseDeferred();


            var ob = entities[entity];
            if (memoryObject.id == null) {
                var transientEntity = new ob();
                deferred.resolve(mergeObjects(memoryObject, transientEntity));
                return deferred.promise;
            }

            if (memoryObject.id) {
                //since it has an id, there´s a chance it´s present on session cache
                ob.load(memoryObject.id, function (loadedObject) {
                    if (!loadedObject) {
                        //if not found in cache, let´s instantiate a new one
                        loadedObject = new ob();
                    }
                    deferred.resolve(mergeObjects(memoryObject, loadedObject));
                });
            }
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

        findAll: function (entity, cbk) {
            if (!entities[entity]) {
                throw new Error("entity {0} not found".format(entity));
            }
            var filter = entities[entity].all();
            filter.list(null, function (result) {
                //single result expected
                cbk(result);
            });
        },

        save: function (obj, cbk, tx) {
            persistence.add(obj);
            if (!cbk) {
                persistence.flush();
                return;
            }
            if (tx) {
                persistence.flush(tx, function () {
                    cbk();
                });
            } else {
                persistence.flush(function () {
                    cbk();
                });
            }

        }

    };

});