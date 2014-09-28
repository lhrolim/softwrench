using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using softWrench.Mobile.Communication.Http;
using softWrench.Mobile.Data;
using softWrench.Mobile.Metadata;
using softWrench.Mobile.Metadata.Extensions;
using softWrench.Mobile.Metadata.Offline;
using softWrench.Mobile.Metadata.Parsing;
using softWrench.Mobile.Persistence;
using softwrench.sw4.Shared2.Metadata;

namespace softWrench.Mobile.Communication.Synchronization {
    internal sealed class DownloadDataMaps {
        private readonly MetadataRepository _metadataRepository = MetadataRepository.GetInstance();

        public async Task DownloadServerChanges(MobileMetadataDownloadResponse downloadedMetadata, CancellationToken cancellationToken) {
            // TODO: this can be optimized: each granted application
            // results on a call issued to the server.
            //TODO: no need to fetch again from database

            var applications = await _metadataRepository.LoadAllApplicationsAsync(cancellationToken);
            foreach (var application in applications) {
                await DownloadApplicationChanges(application, cancellationToken);
            }
        }

        private async Task<IList<DataMap>> DownloadApplicationChanges(IApplicationIdentifier applicationSchemaDefinition,
         CancellationToken cancellationToken = default(CancellationToken)) {
            var uri = User.Settings.Routes.Sync();
            var postContent = CreatePostContent(applicationSchemaDefinition);

            using (var reader = await HttpCall.PostStreamAsync(uri, postContent, cancellationToken)) {
                var dataMaps = ReadDataMapsFromServer(reader);

                await new DataRepository().SaveAsync(applicationSchemaDefinition, dataMaps, cancellationToken);

                return dataMaps;
            }
        }

        private static StringContent CreatePostContent(IApplicationIdentifier applicationSchemaDefinition) {
            //TODO: the server API now allows multiple applications
            //      to be retrieved with a single call.
            var applications = new object[] { new { AppName = applicationSchemaDefinition.ApplicationName } };
            var json = JsonConvert.SerializeObject(new { applications = applications });

            return new StringContent(json, Encoding.UTF8, HttpCall.JsonMediaType.MediaType);
        }

        private static IList<DataMap> ReadDataMapsFromServer(TextReader reader) {
            var json = JObject.Parse(reader.ReadToEnd());
            var synchronizationData = json["synchronizationData"].First();
            var list = new List<DataMap>();
            //reversing order 
            var jTokens = synchronizationData["dataMaps"];
            foreach (var jToken in jTokens) {
                var dataMap = JsonParser.DataMap((JObject)jToken);
                dataMap.LocalState.IsLocal = false;
                list.Add(dataMap);
            }
            return list;
        }


    }
}