using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using softWrench.sW4.Data.Persistence.Relational.Collection;
using softWrench.sW4.Data.Persistence.Relational.EntityRepository;
using softWrench.sW4.Data.Search;
using softWrench.sW4.Metadata.Entities;
using softWrench.sW4.Security.Context;

namespace softwrench.sw4.tgcs.classes.com.cts.tgcs.dataset {
    /// <summary>
    /// Uses <see cref="RestEntityRepository"/> to query data.
    /// </summary>
    public class ToshibaRestCollectionResolver : CollectionResolver {

        private readonly RestEntityRepository _restRepository;

        public ToshibaRestCollectionResolver(RestEntityRepository restRepository, EntityRepository respository,IContextLookuper lookuper): base(respository,lookuper) {
            _restRepository = restRepository;
        }

        protected override async Task<EntityRepository.SearchEntityResult> GetList(EntityMetadata entityMetadata, SearchRequestDto dto, bool offlineMode) {
            if (offlineMode) {
                return await base.GetList(entityMetadata, dto, true);
            }

            var result = await _restRepository.Get(entityMetadata, dto);

            return new EntityRepository.SearchEntityResult() {
                ResultList = result.Select(d => new Dictionary<string, object>(d.Fields, StringComparer.OrdinalIgnoreCase)).ToList(),
                IdFieldName = entityMetadata.IdFieldName,
                MaxRowstampReturned = null,
            };
        }

        protected override async Task<int> GetCount(EntityMetadata entityMetadata, SearchRequestDto dto, bool offlineMode) {
            return offlineMode
                ? await base.GetCount(entityMetadata, dto, true)
                : await _restRepository.Count(entityMetadata, dto);
        }

        
    }
}