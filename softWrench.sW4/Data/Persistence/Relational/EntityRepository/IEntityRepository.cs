using System.Collections.Generic;
using cts.commons.simpleinjector;
using JetBrains.Annotations;
using softwrench.sW4.Shared2.Data;
using softWrench.sW4.Data.Search;
using softWrench.sW4.Metadata.Entities;
using System;
using System.Threading.Tasks;

namespace softWrench.sW4.Data.Persistence.Relational.EntityRepository
{
    public interface IEntityRepository : IComponent
    {

        [NotNull]
        Task<IReadOnlyList<DataMap>> Get([NotNull] EntityMetadata entityMetadata, [NotNull] SearchRequestDto searchDto);

        [CanBeNull]
        Task<DataMap> Get([NotNull] EntityMetadata entityMetadata, [NotNull] string id);

        Task<int> Count([NotNull] EntityMetadata entityMetadata, [NotNull] SearchRequestDto searchDto);

        [CanBeNull]
        Task<AttributeHolder> ByUserIdSite([NotNull] EntityMetadata entityMetadata,
            [NotNull] Tuple<string, string> userIdSiteTuple);
    }

}