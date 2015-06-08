var entities = entities || {};


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
    label:'TEXT',
    //either pending, or completed
    status: 'TEXT',
    problemId: 'TEXT',
});

entities.Batch.hasMany('items', entities.BatchItem, 'batch');
