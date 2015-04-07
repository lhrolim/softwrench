mobileServices.factory('swdbProxy', function () {

    //creating namespace for the entities, to avoid eventaul collisions
    var entities = {};
  

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
                key:'TEXT',
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

            entities.DataEntry.index(['application', 'remoteid'],{unique:true});
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

        findById:function(entity, id) {
            if (!entities[entity]) {
                throw new Error("entity {0} not found".format(entity));
            }
            var filter = entities[entity].all().filter("id", '=', id);
            var resultItem;
            filter.list(null,function(result) {
                resultItem = result;
            });
            return resultItem;
        }

    };

});