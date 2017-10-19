using System.Collections.Generic;
using System.Threading.Tasks;
using cts.commons.persistence;
using cts.commons.simpleinjector;
using cts.commons.simpleinjector.Events;
using softwrench.sw4.firstsolar.classes.com.cts.firstsolar.configuration;
using softwrench.sw4.offlineserver.model.dto;
using softwrench.sw4.offlineserver.services;
using softwrench.sW4.Shared2.Metadata;
using softwrench.sW4.Shared2.Metadata.Applications;
using softWrench.sW4.Configuration.Services.Api;
using softWrench.sW4.Data.Persistence.Relational.Cache.Api;
using softWrench.sW4.Data.Persistence.Relational.Cache.Core;
using softWrench.sW4.Data.Persistence.Relational.EntityRepository;
using softWrench.sW4.Metadata;
using softWrench.sW4.Metadata.Applications;
using softWrench.sW4.Metadata.Security;
using softWrench.sW4.Security.Context;
using softWrench.sW4.Security.Services;

namespace softwrench.sw4.firstsolar.classes.com.cts.firstsolar.offline {

    [OverridingComponent(ClientFilters = "firstsolar")]
    public class FirstSolarSynchronizationManager : SynchronizationManager {

        public FirstSolarSynchronizationManager(OffLineCollectionResolver resolver, EntityRepository respository, IContextLookuper lookuper, IEventDispatcher iEventDispatcher,
            ISWDBHibernateDAO swdbDAO, SyncChunkHandler syncChunkHandler, IConfigurationFacade configFacade, DatamapRedisManager redisManager)
            : base(resolver, respository, lookuper, iEventDispatcher, swdbDAO, syncChunkHandler, configFacade, redisManager) {
        }


        protected override async Task<RedisLookupDTO> BuildRedisDTO(ApplicationMetadata appMetadata, IDictionary<string, CacheRoundtripStatus> completeCacheEntries) {
            var user = SecurityFacade.CurrentUser();
            var lookupDTO = await base.BuildRedisDTO(appMetadata, completeCacheEntries);
            if (user.Genericproperties.ContainsKey(FirstSolarConstants.FacilitiesProp)) {
                var facilities = (IEnumerable<string>)user.Genericproperties[FirstSolarConstants.FacilitiesProp];
                lookupDTO.ExtraKeys.Add("facilities", facilities);
            }
            return lookupDTO;
        }

        protected override IEnumerable<CompleteApplicationMetadataDefinition> GetTopLevelAppsToCollect(SynchronizationRequestDto request, InMemoryUser user) {
            if (request.ItemsToDownload == null) {
                return base.GetTopLevelAppsToCollect(request, user);
            }

            var topLevelApps = MetadataProvider.FetchTopLevelApps(ClientPlatform.Mobile, user);
            //for the sake of simplicity, let´s always return all the top level apps, regardless.
            // Reason is that there are several versions of workorders apps that point to the same entry 
            //(ex: pastworkorders, schedworkorders, etc). A quick sync on one of them should "force" (on this initial version) a sync on another 
            return topLevelApps;
        }
    }
}
