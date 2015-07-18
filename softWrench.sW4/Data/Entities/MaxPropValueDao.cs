using System.Collections.Generic;
using System.Linq;
using softWrench.sW4.Data.Persistence.Relational;
using softWrench.sW4.Data.Search;
using softWrench.sW4.Metadata;
using softWrench.sW4.SimpleInjector;

namespace softWrench.sW4.Data.Entities {
    public class MaxPropValueDao {
        private const string EntityName = "maxpropvalue";

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

        public string GetValue(string propName) {
            var entityMetadata = MetadataProvider.Entity(EntityName);
            var searchRequestDto = SearchRequestDto.GetFromDictionary(new Dictionary<string, string>() { { "propname", propName } });
            searchRequestDto.AppendProjectionField(new ProjectionField("propname","propname"));
            searchRequestDto.AppendProjectionField(new ProjectionField("propvalue","propvalue"));
            var list = EntityRepository.Get(entityMetadata, searchRequestDto);
            var result =list.FirstOrDefault();
            return result ==null ? null : (string) result.GetAttribute("propvalue");
        }
    }
}
