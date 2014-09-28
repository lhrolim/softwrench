using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using softWrench.Mobile.Persistence;
using softwrench.sW4.Shared2.Metadata.Applications.Schema;

namespace softWrench.Mobile.Communication.Synchronization {
    internal sealed class DataOperationsUploadHandler {
        public async Task<SynchronizationResult> UploadOperations(IList<ApplicationSchemaDefinition> applications, CancellationToken cancellationToken = default(CancellationToken)) {
            var successes = 0;
            var errors = 0;

            //TODO: batch data and submit only once.
            var synchronizationRepository = new SynchronizationRepository();
            var dataRepository = new DataRepository();

            // What's in our outbox?
            var operationsToSync = await synchronizationRepository.LoadPendingDataOperationsAsync(cancellationToken);

            foreach (var operation in operationsToSync) {
                var application = applications.First(m => m.Name == operation.Application);
                var upload = new UploadDataOperation(dataRepository, synchronizationRepository);

                // Post the operation data.
                var success = await upload.ExecuteAsync(application, operation, cancellationToken);

                if (success) {
                    // Great, one less to go.
                    await synchronizationRepository.MarkDataOperationAsSynchronizedAsync(operation, cancellationToken);
                    successes++;
                } else {
                    errors++;
                }
            }

            return new SynchronizationResult(successes, errors);
        }
    }
}