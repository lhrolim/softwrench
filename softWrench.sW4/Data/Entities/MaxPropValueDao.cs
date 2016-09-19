using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using softWrench.sW4.Data.Persistence.Relational.EntityRepository;
using softWrench.sW4.Data.Search;
using softWrench.sW4.Metadata;
using cts.commons.simpleinjector;

namespace softWrench.sW4.Data.Entities {
    public class MaxPropValueDao {
        private const string EntityName = "maxpropvalue";

        private EntityRepository EntityRepository {
            get {
                return SimpleInjectorGenericFactory.Instance.GetObject<EntityRepository>(typeof(EntityRepository));
            }
        }

        public async Task<string> GetValue(string propName) {
            var entityMetadata = MetadataProvider.Entity(EntityName);
            var searchRequestDto = SearchRequestDto.GetFromDictionary(new Dictionary<string, string>() { { "propname", propName } });
            searchRequestDto.AppendProjectionField(new ProjectionField("propname", "propname"));
            searchRequestDto.AppendProjectionField(new ProjectionField("propvalue", "propvalue"));
            var list = await EntityRepository.Get(entityMetadata, searchRequestDto);
            var result = list.FirstOrDefault();
            return result == null ? null : (string)result.GetAttribute("propvalue");
        }
    }
}
