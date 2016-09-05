using System;
using System.Collections.Generic;
using System.Linq;
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

        protected override EntityRepository.SearchEntityResult GetList(EntityMetadata entityMetadata, SearchRequestDto dto, bool offlineMode) {
            if (offlineMode) {
                return base.GetList(entityMetadata, dto, true);
            }

            var result = _restRepository.Get(entityMetadata, dto);

            return new EntityRepository.SearchEntityResult() {
                ResultList = result.Select(d => new Dictionary<string, object>(d.Fields, StringComparer.OrdinalIgnoreCase)).ToList(),
                IdFieldName = entityMetadata.IdFieldName,
                MaxRowstampReturned = null,
            };
        }

        protected override int GetCount(EntityMetadata entityMetadata, SearchRequestDto dto, bool offlineMode) {
            return offlineMode
                ? base.GetCount(entityMetadata, dto, true)
                : _restRepository.Count(entityMetadata, dto);
        }

        
    }
}