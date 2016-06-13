(function(angular, persistence) {
    "use strict";
    angular.module("persistence.offline").config(["offlineEntitiesProvider", function (offlineEntitiesProvider) {

        var entities = offlineEntitiesProvider.entities;

        //#region AssociationData
        ///
        /// Holds top level application data.
        /// This Entries needs to be reevaluated on every sync, since chances are that they do not needed to be present on client side after execution.
        ///
        //this entity stores the Data which will be used as support of the main entities.
        //It cannot be created locally. Hence Synchronization should use rowstamp caching capabilities by default
        entities.AssociationData = persistence.define('AssociationData', {
            application: 'TEXT',
            datamap: 'JSON',
            remoteId: 'TEXT',
            rowstamp: 'INT'
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

        ///
        /// Inserts or updates associationData based upon the uniqueness of the entries
        ///
        entities.AssociationData.InsertionPattern = "INSERT OR REPLACE INTO AssociationData (application,datamap,remoteId,rowstamp,id) values (?,?,?,?,?)";

        entities.AssociationData.maxRowstampQueries = "select max(rowstamp) as rowstamp,application,id from AssociationData group by application";

        entities.AssociationData.maxRowstampQueryByApp = "select max(rowstamp) as rowstamp,application,id from AssociationData where application = '{0}' group by application";

        entities.AssociationData.index(['application', 'remoteId'], { unique: true });

        //#endregion

        //#region Batch
        entities.Batch = persistence.define('Batch', {
            ///
            /// A batch gets created when the user has made any local changes to items (operations), and hits the synchronize button.
            /// 
            /// The batch will be sent to the server where it will be handled asynchronously, using a Queue implementation for load distribution.
            /// 
            /// Each operation made locally will incur in a BatchItem entry being generated and linked to the Batch Entry.
            /// 
            /// After submitted, the batch status will be checked periodically (on a configurable setting basis). 
            /// MeanWhile, all the affected DataEntries will be locked locally, so that the user is not able to perform any operation (but viewing) on them before the batch response.
            ///
            application: 'TEXT',
            sentDate: 'DATE',
            completionDate: 'DATE',
            lastChecked: 'DATE',
            //this is the id of the Batch on the server, it will be used for checking the status of it periodically
            remoteId: 'TEXT',
            //either pending, or completed
            status: 'TEXT',
        });

        entities.BatchItem = persistence.define('BatchItem', {
            //how this batch item presents on the screen
            label: 'TEXT',
            //either pending, or completed
            status: 'TEXT',
            // marks the CRUD operation being executed: update, create, etc
            crudoperation: "TEXT"
        });

        entities.Batch.hasMany('items', entities.BatchItem, 'batch');
        //#endregion

        //#region CompositionDataEntry
        ///
        /// Holds both Parent entities and compositions data.
        /// This Entries needs to be reevaluated on every sync, since chances are that they do not needed to be present on client side after execution.
        ///
        entities.CompositionDataEntry = persistence.define('CompositionDataEntry', {
            application: 'TEXT',
            datamap: 'JSON',
            //used to match the parent application for the items that were created locally (since they won´t have a remote id, the match needs to be performed differently)
            parentlocalId: 'TEXT',
            //The id of this entry in maximo, it will be null when it´s created locally
            remoteId: 'TEXT',
            //if this flag is true, it will indicate that some change has been made to this entry locally, and it will appear on the pending sync dashboard
            isDirty: 'BOOL',
            rowstamp: 'INT',
        });

        entities.CompositionDataEntry.insertionQueryPattern = "insert into CompositionDataEntry (application,datamap,isDirty,remoteId,rowstamp,id) values (?,?,0,?,?,?)";
        entities.CompositionDataEntry.updateQueryPattern = "update CompositionDataEntry set datamap='{0}' rowstamp={1} where remoteId='{2}' and applicaton='{3}'";
        entities.CompositionDataEntry.syncdeletionQuery = "delete from CompositionDataEntry where remoteId in (?)";


        entities.CompositionDataEntry.maxRowstampQueries = "select max(rowstamp) as rowstamp,application,id from CompositionDataEntry  group by application";
        entities.CompositionDataEntry.selectCompositions = "select max(rowstamp) as rowstamp,application,id from CompositionDataEntry  group by application";
        //#endregion

        //#region DataEntry
        ///
        /// Holds top level application data.
        /// This Entries needs to be reevaluated on every sync, since chances are that they do not needed to be present on client side after execution.
        ///
        entities.DataEntry = persistence.define('DataEntry', {
            application: 'TEXT',
            //this is a datamap that was retrieved from the latest maximo synchronization, will be used to gather the diff upon synchronization
            originaldatamap: 'JSON',
            //this is the current datamap of the entity, after n different operations had succeed on it. It´s the current entity state, that will be shown on screen.
            datamap: 'JSON',
            //whether this item is pending for a synchronization response, becoming read-only at this point
            pending: 'BOOL',
            //The id of this entry in maximo, it will be null when it has been created locally
            remoteId: 'TEXT',
            //if this flag is true, it will indicate that some change has been made to this entry locally, and it will appear on the pending sync dashboard
            isDirty: 'BOOL',
            rowstamp: 'INT',

        });


        entities.Operation = persistence.define('Operation', {
            //the json object representing everything needed to perform the operation on the server side
            datamap: 'JSON',
            operation: 'TEXT',
            //used to execute operations on the right order
            creationDate: 'DATE',
        });

        entities.DataEntry.hasMany('operations', entities.Operation, 'entry');

        entities.DataEntry.index(['application', 'remoteid'], { unique: true });

        entities.DataEntry.maxRowstampByAppQuery = "select max(rowstamp) as rowstamp,application,id from DataEntry where application = '{0}'";

        //query to be performed after synchronization has occurred, for new items
        entities.DataEntry.insertionQueryPattern = "insert into DataEntry ('application','originaldatamap','datamap','pending','isDirty','remoteId','rowstamp','id') values (:p0,:p1,:p1,0,0,:p2,:p3,:p4)";
        //query to be performed after synchronization has occurred, for existing items
        entities.DataEntry.updateQueryPattern = "update DataEntry set 'originaldatamap'=:p0,'datamap'=:p0,'pending'=0,'rowstamp'=:p1 where remoteId=:p2 and application=:p3";

        entities.DataEntry.insertOrReplacePattern = "INSERT OR REPLACE INTO DataEntry (application,originaldatamap,datamap,pending,isDirty,remoteId,rowstamp,id) values (?,?,?,0,0,?,?,?)";

        entities.DataEntry.deleteQueryPattern = "delete from DataEntry where 'remoteId' in(?) and 'application'=?";
        entities.DataEntry.deleteInIdsStatement = "delete from DataEntry where id in(?) and application=?";

        entities.DataEntry.updateLocalPattern = "update DataEntry set 'datamap'=?,'isDirty'=1 where id =?";
        entities.DataEntry.insertLocalPattern = "insert into DataEntry ('application','datamap','isDirty','pending','remoteId','rowstamp','id') values (?,?,1,0,null,null,?)";

        //here because of the order of the files
        entities.BatchItem.hasOne('dataentry', entities.DataEntry);
        entities.BatchItem.hasOne('operation', entities.Operation);
        //#endregion

        //#region Problem
        entities.Problem = persistence.define("Problem", {
            message: "TEXT"
        });

        entities.BatchItem.hasOne("problem", entities.Problem);
        //#endregion

        //#region SyncOperation
        entities.SyncOperation = persistence.define('SyncOperation', {
            startdate: "DATE",
            enddate: "DATE",
            lastcheckdate: "DATE",
            lastsyncServerVersion: "TEXT",
            //pending,complete
            status: 'TEXT',
            numberofdownloadeditems: "INT",
            numberofdownloadedsupportdata: "INT",
            hasProblems: "BOOL",
            metadatachange: "BOOL",
            items: "INT" // batches.items.length
        });

        //many batches, one per application sent, as they can be processed in parallel cpus
        entities.SyncOperation.hasMany("batches", entities.Batch, "syncoperation");
        //#endregion

        //#region Commons
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

        entities.Attachment = persistence.define('Attachment', {
            application: "TEXT", // ROOT application of the entity that has the asset (e.g. workorder, sr, etc)
            parentId: "TEXT", // local id of the ROOT entity
            // compressed: "BOOL", // whether or not the file is compressed
            path: "TEXT" // local file system path to the saved file (should be in an external storage directory)
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
        //#endregion

    }]);

})(angular, persistence);