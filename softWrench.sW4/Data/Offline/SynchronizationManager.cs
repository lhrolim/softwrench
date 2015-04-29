using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using cts.commons.simpleinjector;
using softWrench.sW4.Data.Persistence.Relational;
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
    class SynchronizationManager : ISingletonComponent {

        private readonly CollectionResolver _resolver;
        private readonly EntityRepository _repository;

        public SynchronizationManager(CollectionResolver resolver, EntityRepository respository) {
            _resolver = resolver;
            _repository = respository;
        }


        public SynchronizationResultDto GetData(SynchronizationRequestDto request, InMemoryUser user) {
            var topLevelApps = GetTopLevelAppsToCollect(request);


            var result = new SynchronizationResultDto();

            IDictionary<CompleteApplicationMetadataDefinition, List<CompleteApplicationMetadataDefinition>> reverseCompositionMap
                = new Dictionary<CompleteApplicationMetadataDefinition, List<CompleteApplicationMetadataDefinition>>();


            foreach (var topLevelApp in topLevelApps) {
                ResolveApplication(request, user, topLevelApp, result);
            }

            return result;
        }

        private void ResolveApplication(SynchronizationRequestDto request, InMemoryUser user,
            CompleteApplicationMetadataDefinition topLevelApp, SynchronizationResultDto result) {
            //this will return sync schema
            var userAppMetadata = topLevelApp.ApplyPolicies(ApplicationMetadataSchemaKey.GetSyncInstance(), user,ClientPlatform.Mobile);

            var applicationCompositionSchemas = CompositionBuilder.InitializeCompositionSchemas(userAppMetadata.Schema);
            var entityMetadata = MetadataProvider.SlicedEntityMetadata(userAppMetadata);
            var topLevelAppData = FetchData(entityMetadata, userAppMetadata);
            var applicationSyncData = request.Applications.FirstOrDefault(f => f.AppName.Equals(topLevelApp.ApplicationName));
            _resolver.ResolveCollections(entityMetadata, applicationCompositionSchemas, topLevelAppData);
            var filteredResults = FilterData(topLevelAppData, applicationSyncData);
            var appResultData = new SynchronizationApplicationData(topLevelApp.ApplicationName, 
                filteredResults.changedData,filteredResults.deletedIds);

            result.SynchronizationData.Add(appResultData);
        }

        private IEnumerable<CompleteApplicationMetadataDefinition> GetTopLevelAppsToCollect(SynchronizationRequestDto request) {
            var topLevelApps = MetadataProvider.FetchTopLevelApps(ClientPlatform.Mobile);
            if (request.ReturnNewApps) {
                return topLevelApps;
            }

            return topLevelApps;
        }




        private ApplicationSyncResult FilterData(List<DataMap> topLevelAppData,
            SynchronizationRequestDto.ApplicationSyncData clientCurrentData) {
            if (clientCurrentData.IdRowstampMapJson == null) {

                return new ApplicationSyncResult() {
                    changedData = topLevelAppData
                };
            }
            return null;
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
