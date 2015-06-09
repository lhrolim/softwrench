var entities = entities || {};

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
entities.AssociationData.InsertionPattern = "INSERT OR REPLACE INTO AssociationData (application,datamap,remoteId,rowstamp,id) values ('{0}','{1}','{2}','{3}','{4}')";

entities.AssociationData.maxRowstampQueries = "select max(rowstamp) as rowstamp,application,id from AssociationData group by application";

entities.AssociationData.maxRowstampQueryByApp = "select max(rowstamp) as rowstamp,application,id from AssociationData where application = '{0}' group by application";

entities.AssociationData.index(['application', 'remoteId'],{unique:true});
