using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using softWrench.Mobile.Metadata.Extensions;
using softWrench.Mobile.Metadata.Offline;
using softWrench.Mobile.Persistence;
using softwrench.sW4.Shared2.Metadata.Applications.Schema;

namespace softWrench.Mobile.Communication.Synchronization {
    internal sealed class SynchronizationFacade {

        static readonly SynchronizationMetadataHandler SynchronizationMetadataHandler = new SynchronizationMetadataHandler();

        static readonly UploadDataMaps DataMapUploadHandler = new UploadDataMaps();

        static readonly DataOperationsUploadHandler DataOperationsUploadHandler = new DataOperationsUploadHandler();

        static readonly DownloadDataMaps DownloadDataMaps = new DownloadDataMaps();

        static readonly SynchronizationRepository SynchronizationRepository = new SynchronizationRepository();
        
        private static readonly MetadataRepository MetadataRepository = MetadataRepository.GetInstance();

        public async Task<SynchronizationResult> Synchronize(CancellationToken cancellationToken = default(CancellationToken)) {
            var before = DateTime.Now;
            Console.WriteLine("Synchronization started");

            var applications = await MetadataRepository.LoadAllApplicationsAsync(cancellationToken);

            // First let's push to the
            // server all local changes.
            var dataMapResult = await DataMapUploadHandler.UploadData(applications, cancellationToken);

            // And now we post the data operations
            // that we have in our local ledger.
            var dataOperationResult = await DataOperationsUploadHandler.UploadOperations(applications, cancellationToken);

            Console.WriteLine("Upload finished");

            // Makes sure bounced data is not lost
            // when we soon fetch remote changes.

            await SynchronizationRepository.StashAsync(cancellationToken);

            // Now we'll download all metadata available on the
            // server so we can update our application catalog.
            var downloadedMetadata = await SynchronizationMetadataHandler.DownloadAsync(cancellationToken, dataMapResult.Errors == 0 && dataOperationResult.Errors == 0);

            Console.WriteLine("Metadata downloaded");

            // Finally, let's get brand new data
            // from the server...
            await DownloadDataMaps.DownloadServerChanges(downloadedMetadata, cancellationToken);

            Console.WriteLine("Data downloaded");

            // ... and handle all synchronization
            // failures. Life isn't perfect, is it?
            await SynchronizationRepository.UnstashAsync(cancellationToken);


            var result = new SynchronizationResult(dataMapResult.Successes + dataOperationResult.Successes, dataMapResult.Errors + dataOperationResult.Errors);

            Console.WriteLine("Synchronization ended in {0} s.", (DateTime.Now - before).TotalSeconds);

            return result;
        }

    }
}