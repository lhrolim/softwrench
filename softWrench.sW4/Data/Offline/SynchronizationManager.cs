using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using cts.commons.simpleinjector;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using softWrench.sW4.Data.Persistence.Relational;
using softWrench.sW4.Data.Persistence.Relational.Collection;
using softWrench.sW4.Data.Persistence.Relational.EntityRepository;
using softWrench.sW4.Data.Relationship.Composition;
using softWrench.sW4.Data.Sync;
using softWrench.sW4.Metadata;
using softWrench.sW4.Metadata.Applications;
using softWrench.sW4.Metadata.Entities.Sliced;
using softWrench.sW4.Metadata.Security;
using softwrench.sW4.Shared2.Metadata;
using softwrench.sW4.Shared2.Metadata.Applications;
using softwrench.sW4.Shared2.Metadata.Applications.Schema;

namespace softWrench.sW4.Data.Offline {
    public class SynchronizationManager : ISingletonComponent {

        private readonly OffLineCollectionResolver _resolver;
        private readonly EntityRepository _repository;

        public SynchronizationManager(OffLineCollectionResolver resolver, EntityRepository respository) {
            _resolver = resolver;
            _repository = respository;
        }


        public SynchronizationResultDto GetData(SynchronizationRequestDto request, InMemoryUser user, JObject rowstampMap) {
            var topLevelApps = GetTopLevelAppsToCollect(request);


            var result = new SynchronizationResultDto();

            IDictionary<CompleteApplicationMetadataDefinition, List<CompleteApplicationMetadataDefinition>> reverseCompositionMap
                = new Dictionary<CompleteApplicationMetadataDefinition, List<CompleteApplicationMetadataDefinition>>();

            foreach (var topLevelApp in topLevelApps) {
                ResolveApplication(request, user, topLevelApp, result, rowstampMap);
            }


            return result;
        }

        private void ResolveApplication(SynchronizationRequestDto request, InMemoryUser user, CompleteApplicationMetadataDefinition topLevelApp, SynchronizationResultDto result, JObject rowstampMap) {
            //this will return sync schema
            var userAppMetadata = topLevelApp.ApplyPolicies(ApplicationMetadataSchemaKey.GetSyncInstance(), user, ClientPlatform.Mobile);

            var entityMetadata = MetadataProvider.SlicedEntityMetadata(userAppMetadata);
            var topLevelAppData = FetchData(entityMetadata, userAppMetadata);

            var parameters = new CollectionResolverParameters() {
                ApplicationMetadata = userAppMetadata,
                ParentEntities = topLevelAppData

            };
            var compositionData = _resolver.ResolveCollections(parameters);
            var filteredResults = FilterData(topLevelAppData, rowstampMap);
            var appResultData = new SynchronizationApplicationData(topLevelApp.ApplicationName,
                filteredResults.changedData, filteredResults.deletedIds);

            result.AddTopApplicationData(appResultData);
            foreach (var compositionDict in compositionData) {
                var dict = compositionDict;
                result.AddCompositionData(new SynchronizationApplicationData(compositionDict.Key, compositionDict.Value.ResultList.Select(s => new DataMap(dict.Key, s,dict.Value.IdFieldName))));
            }
        }

        private IEnumerable<CompleteApplicationMetadataDefinition> GetTopLevelAppsToCollect(SynchronizationRequestDto request) {
            if (request.ApplicationName == null) {
                //no application in special was requested, lets return them all.
                return OffLineMetadataProvider.FetchTopLevelApps();
            }

            return new List<CompleteApplicationMetadataDefinition> { MetadataProvider.Application(request.ApplicationName) };

        }




        private ApplicationSyncResult FilterData(List<DataMap> topLevelAppData, JObject rowstampMap) {
            if (rowstampMap == null) {

                return new ApplicationSyncResult() {
                    changedData = topLevelAppData
                };
            }
            return new ApplicationSyncResult() {
                changedData = topLevelAppData
            }; ;
        }

        private List<DataMap> FetchData(SlicedEntityMetadata entityMetadata, ApplicationMetadata appMetadata) {
            var enumerable = _repository.GetSynchronizationData(entityMetadata, new Rowstamps());
            if (!enumerable.Any()) {
                return new List<DataMap>();
            }
            var dataMaps = new List<DataMap>();
            foreach (var row in enumerable) {
                var dataMap = DataMap.Populate(appMetadata, row);
                dataMaps.Add(dataMap);
            }
            return dataMaps;
        }

        class ApplicationSyncResult {



            internal List<DataMap> changedData;
            internal IList<String> deletedIds = new List<string>();
        }
    }
}
