var entities = entities || {};

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

//query to be performed after synchronization has occurred, for new items
entities.DataEntry.insertionQueryPattern = "insert into DataEntry ('application','originaldatamap','datamap','pending','isDirty','remoteId','rowstamp','id') values (:p0,:p1,:p1,0,0,:p2,:p3,:p4)";
//query to be performed after synchronization has occurred, for existing items
entities.DataEntry.updateQueryPattern = "update DataEntry set 'originaldatamap'=:p0,'datamap'=:p0,'pending'=0,'rowstamp'=':p1 where 'remoteId'=:p2 and 'application'=:p3";
entities.DataEntry.deleteQueryPattern = "delete from DataEntry where 'remoteId' in(?) and 'application'=?";

entities.DataEntry.updateLocalPattern = "update DataEntry set 'datamap'=?,'isDirty'=1 where id =?";
entities.DataEntry.insertLocalPattern = "insert into DataEntry ('application','datamap','isDirty','pending','remoteId','rowstamp','id') values (?,?,1,0,null,null,?)";

//here because of the order of the files
entities.BatchItem.hasOne('dataentry', entities.DataEntry);
entities.BatchItem.hasOne('operation', entities.Operation);