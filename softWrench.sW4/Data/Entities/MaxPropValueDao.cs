using System.Collections.Generic;
using System.Linq;
using softWrench.sW4.Data.Persistence.Relational;
using softWrench.sW4.Data.Search;
using softWrench.sW4.Metadata;

namespace softWrench.sW4.Data.Entities {
    public class MaxPropValueDao {
        private const string EntityName = "maxpropvalue";

        private readonly EntityRepository _entityRepository = new EntityRepository();

        public string GetValue(string propName) {
            var entityMetadata = MetadataProvider.Entity(EntityName);
            var searchRequestDto = SearchRequestDto.GetFromDictionary(new Dictionary<string, string>() { { "propname", propName } });
            searchRequestDto.AppendProjectionField(new ProjectionField("propname","propname"));
            searchRequestDto.AppendProjectionField(new ProjectionField("propvalue","propvalue"));
            var list =_entityRepository.Get(entityMetadata, searchRequestDto);
            var result =list.FirstOrDefault();
            return result ==null ? null : (string) result.GetAttribute("propvalue");
        }
    }
}
