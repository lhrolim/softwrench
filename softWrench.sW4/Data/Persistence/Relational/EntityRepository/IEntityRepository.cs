using System.Collections.Generic;
using cts.commons.simpleinjector;
using JetBrains.Annotations;
using softwrench.sW4.Shared2.Data;
using softWrench.sW4.Data.Search;
using softWrench.sW4.Metadata.Entities;
using System;

namespace softWrench.sW4.Data.Persistence.Relational.EntityRepository
{
    public interface IEntityRepository : IComponent
    {

        [NotNull]
        IReadOnlyList<DataMap> Get([NotNull] EntityMetadata entityMetadata, [NotNull] SearchRequestDto searchDto);

        [CanBeNull]
        DataMap Get([NotNull] EntityMetadata entityMetadata, [NotNull] string id);

        int Count([NotNull] EntityMetadata entityMetadata, [NotNull] SearchRequestDto searchDto);

        [CanBeNull]
        AttributeHolder ByUserIdSite([NotNull] EntityMetadata entityMetadata,
            [NotNull] Tuple<string, string> userIdSiteTuple);
    }

}