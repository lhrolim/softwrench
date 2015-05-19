var entities = entities || {};

///
/// Holds top level application data.
/// This Entries needs to be reevaluated on every sync, since chances are that they do not needed to be present on client side after execution.
///
entities.DataEntry = persistence.define('DataEntry', {
    application: 'TEXT',
    datamap: 'JSON',
    //The id of this entry in maximo, it will be null when it´s created locally
    remoteId: 'TEXT',
    //if this flag is true, it will indicate that some change has been made to this entry locally, and it will appear on the pending sync dashboard
    isDirty: 'BOOL',
    rowstamp: 'TEXT',
});

entities.DataEntry.insertionQueryPattern = "insert into DataEntry (application,datamap,isDirty,remoteId,rowstamp,id) values ('{0}','{1}',0,'{2}','{3}','{4}')";
entities.DataEntry.updateQueryPattern = "update DataEntry set datamap='{0}',rowstamp='{1}' where remoteId='{2}' and application='{3}'";
entities.DataEntry.deleteQueryPattern = "delete from DataEntry where remoteId in({0}) and application='{1}'";