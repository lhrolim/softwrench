using System.Collections.Generic;
using System.Threading.Tasks;
using cts.commons.persistence;
using cts.commons.simpleinjector;
using cts.commons.simpleinjector.Events;
using softwrench.sw4.firstsolar.classes.com.cts.firstsolar.configuration;
using softwrench.sw4.offlineserver.services;
using softWrench.sW4.Configuration.Services.Api;
using softWrench.sW4.Data.Persistence.Relational.Cache.Api;
using softWrench.sW4.Data.Persistence.Relational.Cache.Core;
using softWrench.sW4.Data.Persistence.Relational.EntityRepository;
using softWrench.sW4.Metadata.Applications;
using softWrench.sW4.Security.Context;
using softWrench.sW4.Security.Services;

namespace softwrench.sw4.firstsolar.classes.com.cts.firstsolar.offline {

    [OverridingComponent(ClientFilters = "firstsolar")]
    public class FirstSolarSynchronizationManager : SynchronizationManager {

        public FirstSolarSynchronizationManager(OffLineCollectionResolver resolver, EntityRepository respository, IContextLookuper lookuper, IEventDispatcher iEventDispatcher,
            ISWDBHibernateDAO swdbDAO, SyncChunkHandler syncChunkHandler, IConfigurationFacade configFacade, RedisManager redisManager)
            : base(resolver, respository, lookuper, iEventDispatcher, swdbDAO, syncChunkHandler, configFacade, redisManager) {
        }

        protected override async Task<RedisLookupDTO> BuildRedisDTO(ApplicationMetadata appMetadata, IDictionary<string, CacheRoundtripStatus> completeCacheEntries) {
            var user = SecurityFacade.CurrentUser();
            var lookupDTO = await base.BuildRedisDTO(appMetadata, completeCacheEntries);
            if (user.Genericproperties.ContainsKey(FirstSolarConstants.FacilitiesProp)){
                var facilities = (IEnumerable<string>)user.Genericproperties[FirstSolarConstants.FacilitiesProp];
                lookupDTO.ExtraKeys.Add("facilities", facilities);
            }
            return lookupDTO;
        }
    }
}
