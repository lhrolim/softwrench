using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using NHibernate.Cfg.Loquacious;
using softWrench.sW4.Data.API;
using softWrench.sW4.Data.Persistence.Relational;
using softWrench.sW4.Data.Search;
using softWrench.sW4.Metadata;
using softWrench.sW4.Metadata.Applications;
using softWrench.sW4.Metadata.Security;
using softWrench.sW4.Security.Context;
using softwrench.sW4.Shared2.Metadata.Applications;
using softwrench.sW4.Shared2.Metadata.Applications.Relationships.Compositions;
using softwrench.sW4.Shared2.Util;
using softWrench.sW4.SimpleInjector;
using softWrench.sW4.SPF;

namespace softWrench.sW4.Data.Relationship.Composition {

    public class CompositionExpander : ISingletonComponent {

        private readonly EntityRepository _entityRepository;

        private readonly IContextLookuper _contextLookuper;

        public CompositionExpander(IContextLookuper contextLookuper, EntityRepository entityRepository)
        {
            _contextLookuper = contextLookuper;
            _entityRepository = entityRepository;
        }

        public IGenericResponseResult Expand(InMemoryUser user, IDictionary<string, ApplicationCompositionSchema> compositionSchemas, CompositionExpanderHelper.CompositionExpansionOptions options) {
            var resultDict = new Dictionary<string, IEnumerable<IDictionary<string, object>>>();
            var result = CompositionExpanderHelper.ParseDictionary(options.CompositionsToExpand);
            var printMode = _contextLookuper.LookupContext().PrintMode;
            foreach (var toExpand in result.DetailsToExpand) {
                var name = toExpand.Key;
                var compositionSchema = compositionSchemas[name];
                var schema = compositionSchema.Schemas.Detail;
                if (printMode) {
                    schema = compositionSchema.Schemas.Print;
                }
                var applicationMetadata = MetadataProvider.Application(EntityUtil.GetApplicationName(name))
                    .ApplyPolicies(schema.GetSchemaKey(), user, ClientPlatform.Web);
                var slicedEntityMetadata = MetadataProvider.SlicedEntityMetadata(applicationMetadata);
                var searchDTO = new SearchRequestDto();
                searchDTO.AppendSearchParam(schema.IdFieldName);
                searchDTO.AppendSearchValue(toExpand.Value);
                var compositionExpanded = _entityRepository.GetAsRawDictionary(slicedEntityMetadata, searchDTO);
                resultDict.Add(name, compositionExpanded.ResultList);
            }

            return new GenericResponseResult<Dictionary<string, IEnumerable<IDictionary<string, object>>>>(resultDict);
        }


    }


}
