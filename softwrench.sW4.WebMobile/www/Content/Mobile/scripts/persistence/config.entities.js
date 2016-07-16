(function (angular, persistence) {
    "use strict";
    angular.module("persistence.offline").config(["offlineEntitiesProvider", function (offlineEntitiesProvider) {

        var entities = offlineEntitiesProvider.entities;

        /**
         * Holds information of Eager-loaded OptionFields processed on the server side, by providerAttributes.
         * 
         * This is similar to an autocompleteclient implementation, whereas the server would return the full list upon the sync
         * 
         */
        entities.OptionFieldData = persistence.define("OptionFieldData", {
            application: 'TEXT',
            schema: 'TEXT',
            providerAttribute: 'TEXT', //used for matching which optionfields on the screen are bound to the data
            optionkey: 'TEXT', // the remoteid of the option (assetnum, classificationid, etc..)
            optionvalue: 'TEXT',
            extraprojectionvalues: 'JSON', //usually null, but can contain a json with extra fields
        });

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
            rowstamp: 'INT',
            // index for use on searches
            textindex01: "TEXT",
            textindex02: "TEXT",
            textindex03: "TEXT",
            textindex04: "TEXT",
            textindex05: "TEXT",
            numericindex01: "NUMERIC",
            numericindex02: "NUMERIC",
            dateindex01: "DATE",
            dateindex02: "DATE",
            dateindex03: "DATE"
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
        entities.AssociationData.InsertionPattern = "INSERT OR REPLACE INTO AssociationData (application,datamap,remoteId,rowstamp,id,textindex01,textindex02,textindex03,textindex04,textindex05,numericindex01,numericindex02,dateindex01,dateindex02,dateindex03) values (?,?,?,?,?,?,?,?,?,?,?,?,?,?,?)";

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
        /// Holds Composition Data for the items already stored on server side. 
        /// Items created/updated locally will be pushed straight into the parents datamap; this happens so that we can easily write the datamap to maximo, the exact same way as the online version.
        ///
        ///  Attachments compositions (doclinks) are an exception to this rule, as we need to generate them locally in order to be able to do a match to the attachment table
        ///
        ///  Whenever the list of compositions is to be shown, we need to do a union among the list of compositionDataEntries (persisted entries) and the list coming from the parent datamap (local entries).
        ///
        ///
        entities.CompositionDataEntry = persistence.define('CompositionDataEntry', {
            application: 'TEXT',
            datamap: 'JSON',
            remoteId: 'TEXT',//The id of this entry in maximo, it will be null when it´s created locally
            isDirty: 'BOOL', //if this flag is true, it will indicate that some change has been made to this entry locally, and it will appear on the pending sync dashboard
            rowstamp: 'INT',
        });

        entities.CompositionDataEntry.insertionQueryPattern = "insert into CompositionDataEntry (application,datamap,isDirty,remoteId,rowstamp,id) values (?,?,0,?,?,?)";
        entities.CompositionDataEntry.updateQueryPattern = "update CompositionDataEntry set datamap='{0}' rowstamp={1} where remoteId='{2}' and applicaton='{3}'";
        entities.CompositionDataEntry.syncdeletionQuery = "delete from CompositionDataEntry where remoteId in (?)";


        entities.CompositionDataEntry.maxRowstampQueries = "select max(rowstamp) as rowstamp,application,id from CompositionDataEntry  group by application";
        entities.CompositionDataEntry.selectCompositions = "select max(rowstamp) as rowstamp,application,id from CompositionDataEntry  group by application";
        //#endregion

        //#region Attachment
        
        /**
         * The attachment entity holds the raw base64 of the attachment itself. 
         * It's equivalent to the docinfo table on the server side. 
         * 
         * The id will be used a unique cross-device/user identifier so that it can be used to download an already existing attachment that got created on that device on firstplace.
         * 
         * 
         * Current implementation is storing every attachemnent's base64 but it would be possible just to point to a path of the file on the device FS instead.
         * 
         * 
         */
        entities.Attachment = persistence.define('Attachment', {
            application: "TEXT", // ROOT application of the entity that has the asset (e.g. workorder, sr, etc)
            parentId: "TEXT", // local id of the ROOT entity
            compositionRemoteId: "TEXT", // the remoteId of the composition to link
            docinfoRemoteId: "TEXT", // the remoteId of the composition to link
            path: "TEXT", // local file system path to the saved file (should be in an external storage directory), used to cache access to ios devices
            compressed: "BOOL", // whether or not the file is compressed
            content: "TEXT", // base64 encoded content,
            mimetype:"TEXT" //mimetype of the file
        });

        entities.Attachment.NonPendingAttachments = "select id,compositionRemoteId,docinfoRemoteId from Attachment where (path is not null or content is not null) and (id in ({0}) or docinfoRemoteId in ({1}) )";


        entities.Attachment.UpdateRemoteIdOfExistingAttachments = "update Attachment set 'compositionRemoteId' = ?, 'docinfoRemoteId'=? where id = ?";
        entities.Attachment.CreateNewBlankAttachments = "insert into Attachment ('application','parentId','compositionRemoteId','docinfoRemoteId','id') values (?,?,?,?,?)";
        //brings the attachments that need to be syncrhonized to the server. The ones which have a compositionRemoteId already point to a downloaded composition, and thus do not require to be uploaded
        entities.Attachment.ByApplicationAndIds = "select id,parentId,content from Attachment where application = ? and parentId in (?) and compositionRemoteId is null";
        
        /**
         * query to fetch list of attachments which are pending synchronization against the server side
         */
        entities.Attachment.PendingAttachments = "select id,docinfoRemoteId from Attachment where (path is null and content is null) and compositionRemoteId is not null";

        entities.Attachment.UpdatePendingAttachment = "update Attachment set content =? , mimetype=? where id =?";

        entities.Attachment.UpdateAttachmentPath = "update Attachment set path =? where docinfoRemoteId =?";
        
        entities.Attachment.ByDocInfoId = "select content,mimetype,path from Attachment where docinfoRemoteId = ?";
        entities.Attachment.ByHashId = "select content,mimetype,path from Attachment where id = ?";
        entities.Attachment.DeleteById = "delete from Attachment where id = ?";
        entities.Attachment.DeleteMultipleByIdsPattern = "delete from Attachment where id = in ({0})";

        //#endregion

        //('application','datamap','pending','isDirty','remoteId','rowstamp','id') values (:p0,:p1,0,0,:p2,:p3,:p4)

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
            //if this flag is true, it will indicate that the dataentry had a problem on the last sync
            hasProblem: 'BOOL',
            rowstamp: 'INT',
            // index for use on searches
            textindex01: "TEXT",
            textindex02: "TEXT",
            textindex03: "TEXT",
            textindex04: "TEXT",
            textindex05: "TEXT",
            numericindex01: "NUMERIC",
            numericindex02: "NUMERIC",
            dateindex01: "DATE",
            dateindex02: "DATE",
            dateindex03: "DATE"
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
        entities.DataEntry.insertionQueryPattern = "insert into DataEntry ('application','datamap','pending','isDirty','remoteId','rowstamp','id','textindex01','textindex02','textindex03','textindex04','textindex05','numericindex01','numericindex02','dateindex01','dateindex02','dateindex03') values (:p0,:p1,0,0,:p2,:p3,:p4,:p5,:p6,:p7,:p8,:p9,:p10,:p11,:p12,:p13,:p14)";
        //query to be performed after synchronization has occurred, for existing items
        entities.DataEntry.updateQueryPattern = "update DataEntry set 'datamap'=:p0,'pending'=0,'rowstamp'=:p1,'textindex01'=:p2,'textindex02'=:p3,'textindex03'=:p4,'textindex04'=:p5,'textindex05'=:p6,'numericindex01'=:p7,'numericindex02'=:p8,'dateindex01'=:p9,'dateindex02'=:p10,'dateindex03'=:p11 where remoteId=:p12 and application=:p13";

        entities.DataEntry.insertOrReplacePattern = "INSERT OR REPLACE INTO DataEntry (application,datamap,pending,isDirty,remoteId,rowstamp,id,textindex01,textindex02,textindex03,textindex04,textindex05,numericindex01,numericindex02,dateindex01,dateindex02,dateindex03) values (?,?,0,0,?,?,?,?,?,?,?,?,?,?,?,?,?)";

        entities.DataEntry.deleteQueryPattern = "delete from DataEntry where 'remoteId' in(?) and 'application'=?";
        entities.DataEntry.deleteInIdsStatement = "delete from DataEntry where id in(?) and application=?";
        entities.DataEntry.deleteLocalStatement = "delete from DataEntry where id=? and application=?";

        entities.DataEntry.updateLocalPattern = "update DataEntry set 'datamap'=?,'isDirty'=1,'textindex01'=?,'textindex02'=?,'textindex03'=?,'textindex04'=?,'textindex05'=?,'numericindex01'=?,'numericindex02'=?,'dateindex01'=?,'dateindex02'=?,'dateindex03'=? where id =?";
        entities.DataEntry.updateLocalSetOriginalPattern = "update DataEntry set 'datamap'=?,'originaldatamap'=?,'isDirty'=1,'textindex01'=?,'textindex02'=?,'textindex03'=?,'textindex04'=?,'textindex05'=?,'numericindex01'=?,'numericindex02'=?,'dateindex01'=?,'dateindex02'=?,'dateindex03'=? where id =?";
        entities.DataEntry.insertLocalPattern = "insert into DataEntry ('application','datamap','isDirty','pending','remoteId','rowstamp','id','textindex01','textindex02','textindex03','textindex04','textindex05','numericindex01','numericindex02','dateindex01','dateindex02','dateindex03') values (?,?,1,0,null,null,?,?,?,?,?,?,?,?,?,?,?)";

        entities.DataEntry.restoreToOriginalStateStatement = "update DataEntry set datamap=originaldatamap,isDirty=0 where id=? and application=?";

        entities.DataEntry.clearProblem = "update DataEntry set 'hasProblem'=0 where id in(?)";
        entities.DataEntry.setProblem = "update DataEntry set 'hasProblem'=1 where id in(?)";
        entities.DataEntry.findProblems = "select p.* from Problem p left join batchitem bi on p.id = bi.problem left join batch b on b.id = bi.batch where bi.dataentry = ? order by b.sentDate desc";

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

        entities.CommandBar = persistence.define("CommandBar", {
            key: "TEXT",
            data: "JSON"
        });
        //#endregion

    }]);

})(angular, persistence);