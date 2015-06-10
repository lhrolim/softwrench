var entities = entities || {};

entities.SyncOperation = persistence.define('SyncOperation', {
    startdate: "DATE",
    enddate: "DATE",
    lastcheckdate: "DATE",
    lastsyncServerVersion: "TEXT",
    //pending,complete
    status: 'TEXT',
    numberofdownloadeditems: 'INT',
    numberofdownloadedsupportdata: 'INT',
    metadatachange: 'BOOL',
});

//many batches, one per application sent, as they can be processed in parallel cpus
entities.SyncOperation.hasMany('batch', entities.Batch, 'syncoperation');