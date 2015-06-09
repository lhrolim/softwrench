var entities = entities || {};

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

entities.CompositionDataEntry.insertionQueryPattern = "insert into CompositionDataEntry (application,datamap,isDirty,remoteId,rowstamp,id) values ('{0}','{1}',0,'{2}','{3}','{4}')";
entities.CompositionDataEntry.updateQueryPattern = "update CompositionDataEntry set datamap='{0}' rowstamp='{1}' where remoteId='{2}' and applicaton='{3}'";
entities.CompositionDataEntry.syncdeletionQuery = "delete from CompositionDataEntry where remoteId in ({0})";


entities.CompositionDataEntry.maxRowstampQueries = "select max(rowstamp) as rowstamp,application,id from CompositionDataEntry  group by application";
entities.CompositionDataEntry.selectCompositions = "select max(rowstamp) as rowstamp,application,id from CompositionDataEntry  group by application";