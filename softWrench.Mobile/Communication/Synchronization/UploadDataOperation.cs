using System;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using softWrench.Mobile.Communication.Http;
using softWrench.Mobile.Data;
using softWrench.Mobile.Metadata;
using softWrench.Mobile.Persistence;
using softwrench.sW4.Shared2.Metadata.Applications.Schema;

namespace softWrench.Mobile.Communication.Synchronization {
    internal sealed class UploadDataOperation {
        private readonly DataRepository _dataRepository;
        private readonly SynchronizationRepository _synchronizationRepository;

        public UploadDataOperation(DataRepository dataRepository, SynchronizationRepository synchronizationRepository) {
            if (dataRepository == null) throw new ArgumentNullException("dataRepository");
            if (synchronizationRepository == null) throw new ArgumentNullException("synchronizationRepository");

            _dataRepository = dataRepository;
            _synchronizationRepository = synchronizationRepository;
        }

        private static StringContent CreatePostContent(string json) {
            return new StringContent(
                json,
                Encoding.UTF8,
                HttpCall.JsonMediaType.MediaType);
        }

        private static IDataOperationHandler ResolveHandlerForOperation(DataOperation dataOperation) {
            return (IDataOperationHandler)Type
                .GetType(dataOperation.Handler)
                .GetConstructor(new Type[0])
                .Invoke(new object[0]);
        }

        private Task<string> HandleOperation(IDataOperationHandler handler, DataOperation dataOperation) {
            return handler.HandleAsync(dataOperation, _dataRepository);
        }

        public async Task<bool> ExecuteAsync(ApplicationSchemaDefinition application, DataOperation dataOperation, CancellationToken cancellationToken = default(CancellationToken)) {
            var handler = ResolveHandlerForOperation(dataOperation);

            // Handles the operation, obtaining
            // the json that must be posted to
            // the server.            
            var json = await HandleOperation(handler, dataOperation);

            var uri = User.Settings.Routes.Operation(application.Name, handler.Name);
            var content = CreatePostContent(json);

            try {
                using (await HttpCall.PostStreamAsync(uri, content, cancellationToken)) {
                    await handler.Success(_synchronizationRepository);
                    return true;
                }
            } catch (Exception) {
                return false;
            }
        }
    }
}