using System.Collections.Generic;
using System.Linq;
using softWrench.sW4.Data.Persistence.Relational;
using softWrench.sW4.Data.Persistence.Relational.EntityRepository;
using softWrench.sW4.Data.Search;
using softWrench.sW4.Metadata;
using softwrench.sW4.Shared2.Data;
using cts.commons.simpleinjector;

namespace softWrench.sW4.Data.Entities {
    public class AttachmentDao {

        private const string EntityName = "DOCINFO";

        private EntityRepository _repository;

        private EntityRepository EntityRepository {
            get {
                if (_repository == null) {
                    _repository =
                        SimpleInjectorGenericFactory.Instance.GetObject<EntityRepository>(typeof(EntityRepository));
                }
                return _repository;
            }
        }

        public AttributeHolder ById(string documentId) {
            var entityMetadata = MetadataProvider.Entity(EntityName);
            var searchRequestDto = SearchRequestDto.GetFromDictionary(new Dictionary<string, string>() { { "docinfoid", documentId } });
            searchRequestDto.AppendProjectionField(new ProjectionField("urlname", "urlname"));
            searchRequestDto.AppendProjectionField(new ProjectionField("document", "document"));
            searchRequestDto.AppendProjectionField(new ProjectionField("docinfoid", "docinfoid"));
            var list = EntityRepository.Get(entityMetadata, searchRequestDto);
            var result = list.FirstOrDefault();
            return result;
        }

    }
}
