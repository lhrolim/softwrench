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
    // marks the CRUD operation being executed: update, create, etc
    crudoperation: "TEXT"
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
entities.DataEntry.insertionQueryPattern = "insert into DataEntry (application,originaldatamap,datamap,pending,isDirty,remoteId,rowstamp,id) values ('{0}','{1}','{1}',0,0,'{2}','{3}','{4}')";
//query to be performed after synchronization has occurred, for existing items
entities.DataEntry.updateQueryPattern = "update DataEntry set originaldatamap='{0}',datamap='{0}',pending=0,rowstamp='{1}' where remoteId='{2}' and application='{3}'";
entities.DataEntry.deleteQueryPattern = "delete from DataEntry where remoteId in({0}) and application='{1}'";

entities.DataEntry.updateLocalPattern = "update DataEntry set datamap='{0}',isDirty=1,crudoperation='crud_update' where id ='{1}'";
entities.DataEntry.insertLocalPattern = "insert into DataEntry (application,datamap,isDirty,remoteId,rowstamp,id,crudoperation) values ('{0}','{1}',1,null,null,'{2}','crud_create')";

//here because of the order of the files
entities.BatchItem.hasOne('dataentry', entities.DataEntry);
entities.BatchItem.hasOne('operation', entities.Operation);