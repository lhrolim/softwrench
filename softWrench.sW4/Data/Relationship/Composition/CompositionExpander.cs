using System.Collections.Generic;
using System.Threading.Tasks;
using softWrench.sW4.Data.API.Response;
using softWrench.sW4.Data.Persistence.Relational.EntityRepository;
using softWrench.sW4.Data.Search;
using softWrench.sW4.Metadata;
using softWrench.sW4.Metadata.Applications;
using softWrench.sW4.Metadata.Security;
using softwrench.sW4.Shared2.Metadata.Applications;
using softwrench.sW4.Shared2.Metadata.Applications.Relationships.Compositions;
using softwrench.sW4.Shared2.Util;
using cts.commons.simpleinjector;
using softWrench.sW4.SPF;

namespace softWrench.sW4.Data.Relationship.Composition {

    public class CompositionExpander : ISingletonComponent {

        private readonly EntityRepository _entityRepository;

        public CompositionExpander(EntityRepository entityRepository) {
            _entityRepository = entityRepository;
        }


        public async Task<IGenericResponseResult> Expand(InMemoryUser user, IDictionary<string, ApplicationCompositionSchema> compositionSchemas, CompositionExpanderHelper.CompositionExpansionOptions options) {
            var resultDict = new Dictionary<string, IEnumerable<IDictionary<string, object>>>();
            var result = CompositionExpanderHelper.ParseDictionary(options.CompositionsToExpand);

            foreach (var toExpand in result.DetailsToExpand) {
                var name = toExpand.Key;
                var compositionSchema = compositionSchemas[name];
                var printSchema = compositionSchema.Schemas.Print;
                var applicationMetadata = MetadataProvider.Application(EntityUtil.GetApplicationName(name))
                    .ApplyPolicies(printSchema.GetSchemaKey(), user, ClientPlatform.Web,null);
                var slicedEntityMetadata = MetadataProvider.SlicedEntityMetadata(applicationMetadata);
                var searchDTO = new SearchRequestDto();
                searchDTO.AppendSearchParam(printSchema.IdFieldName);
                searchDTO.AppendSearchValue(toExpand.Value);
                var compositionExpanded = await _entityRepository.GetAsRawDictionary(slicedEntityMetadata, searchDTO);
                resultDict.Add(name, compositionExpanded.ResultList);
            }

            return new GenericResponseResult<Dictionary<string, IEnumerable<IDictionary<string, object>>>>(resultDict);
        }


    }


}
