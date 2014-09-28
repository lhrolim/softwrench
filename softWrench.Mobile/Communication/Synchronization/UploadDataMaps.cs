using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using softWrench.Mobile.Data;
using softWrench.Mobile.Persistence;
using softwrench.sW4.Shared2.Metadata.Applications.Schema;

namespace softWrench.Mobile.Communication.Synchronization {
    internal sealed class UploadDataMaps {
        private static bool MustSkipCrudSynchronization(DataMap dataMap) {
            return dataMap
                .LocalState
                .HasFlag(LocalStateFlag.SkipCrudSynchronization);
        }

        public async Task<SynchronizationResult> UploadData(IList<ApplicationSchemaDefinition> applications, CancellationToken cancellationToken = default(CancellationToken)) {
            var successes = 0;
            var errors = 0;

            //TODO: batch data and submit only once.
            var repository = new SynchronizationRepository();

            // What's in our outbox?
            var dataToSync = await repository.LoadPendingCompositeDataMapsAsync(cancellationToken);

            foreach (var composite in dataToSync) {
                // Are we allowed to perform CRUD
                // synchronization for this buddy?
                if (MustSkipCrudSynchronization(composite)) {
                    continue;
                }

                var application = applications.First(a => a.ApplicationName == composite.Application);
                var id = composite.Id(application);
                var upload = new UploadDataMap();

                // Before dispatching the data, we need
                // to gather all components (children)
                // that belong to the composition root.
                var components = (await repository
                    .LoadPendingComponentDataMapsAsync(composite, cancellationToken))
                    .ToLookup(k => applications.First(a => a.ApplicationName == k.Application));

                var data = new UploadDataMap.CompositeData(application, composite, components);

                // Lets push the data to the
                // upstream server, shall we?
                var result = await upload.ExecuteAsync(data, id, cancellationToken);

                if (result.IsSuccess) {
                    // And mark our local copy as
                    // successfully synchronized.
                    await repository.MarkCompositeDataMapAsSynchronizedAsync(composite, cancellationToken);
                    successes++;
                } else {
                    // Oops... Something fishy happened.
                    // Let's register that.
                    await repository.MarkCompositeDataMapAsBouncingAsync(composite, result.ErrorMessage, cancellationToken);
                    errors++;
                }
            }

            return new SynchronizationResult(successes, errors);
        }
    }
}