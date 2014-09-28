using System.Collections.Generic;
using System.Linq;
using softWrench.sW4.Data.Persistence.Relational;
using softWrench.sW4.Data.Search;
using softWrench.sW4.Metadata;
using softwrench.sW4.Shared2.Data;

namespace softWrench.sW4.Data.Entities {
    public class AttachmentDao {

        private const string EntityName = "DOCINFO";

        private readonly EntityRepository _entityRepository = new EntityRepository();

        public AttributeHolder ById(string documentId) {
            var entityMetadata = MetadataProvider.Entity(EntityName);
            var searchRequestDto = SearchRequestDto.GetFromDictionary(new Dictionary<string, string>() { { "docinfoid", documentId } });
            searchRequestDto.AppendProjectionField(new ProjectionField("urlname", "urlname"));
            searchRequestDto.AppendProjectionField(new ProjectionField("document", "document"));
            searchRequestDto.AppendProjectionField(new ProjectionField("docinfoid", "docinfoid"));
            var list = _entityRepository.Get(entityMetadata, searchRequestDto);
            var result = list.FirstOrDefault();
            return result;
        }

    }
}
