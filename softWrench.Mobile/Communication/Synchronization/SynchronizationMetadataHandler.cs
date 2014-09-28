using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using softWrench.Mobile.Communication.Http;
using softWrench.Mobile.Metadata;
using softWrench.Mobile.Metadata.Extensions;
using softWrench.Mobile.Metadata.Offline;
using softWrench.Mobile.Metadata.Parsing;
using softWrench.Mobile.Persistence;
using softwrench.sW4.Shared2.Metadata.Offline;

namespace softWrench.Mobile.Communication.Synchronization {
    internal class SynchronizationMetadataHandler {

        static readonly MetadataRepository MetadataRepository = MetadataRepository.GetInstance();

        public async Task<MobileMetadataDownloadResponse> DownloadAsync(CancellationToken cancellationToken, bool resetSequences) {

            //we will now get fresh data. hence clear all caches
            MetadataRepository.ResetCaches();

            var downloadData = await GetRemoteData(cancellationToken);
            await SaveResultsToDB(cancellationToken, resetSequences, downloadData);

            return downloadData;
        }

        private static async Task SaveResultsToDB(CancellationToken cancellationToken, bool resetSequences,
            MobileMetadataDownloadResponse downloadData) {
            await MetadataRepository.SaveMenuAsync(downloadData.Menu, cancellationToken);

            foreach (var completeMetadata in downloadData.Metadatas) {
                await MetadataRepository.SaveAsync(completeMetadata.MobileSchema(), cancellationToken);
            }

            if (resetSequences) {
                await MetadataRepository.RecreateSequencesAsync(cancellationToken);
            }
        }

        private async Task<MobileMetadataDownloadResponse> GetRemoteData(CancellationToken cancellationToken = default(CancellationToken)) {
            var metadata = User.Settings.Routes.Metadata();
            using (var response = await HttpCall.GetStreamAsync(metadata, cancellationToken)) {
                var json = response.ReadToEnd();
                var jobject = JsonConvert.DeserializeObject<MobileMetadataDownloadResponseDefinition>(json);
                return JsonDownloadMetadataParser.ParseMetadata(jobject);
            }
        }



    }
}
