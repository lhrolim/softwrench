using System.Collections.Generic;
using softWrench.sW4.Data.Entities;
using softWrench.sW4.Data.Persistence.Operation;
using softWrench.sW4.Data.Search;
using softWrench.sW4.Metadata.Entities;

namespace softWrench.sW4.Data.Persistence.WS.API
{
    internal interface IGenericConnector
    {
        object Execute(OperationWrapper operationWrapper);
        object Update(CrudOperationData entity);
        object Create(CrudOperationData entity);
        int Count(EntityMetadata entityMetadata, SearchRequestDto searchDto);
        Entity FindById(EntityMetadata metaData, string id);
        IReadOnlyList<Entity> Find(EntityMetadata entityMetadata, SearchRequestDto searchDto);
    }
}