using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using cts.commons.simpleinjector;
using JetBrains.Annotations;
using softwrench.sw4.Shared2.Data.Association;
using softwrench.sw4.Shared2.Metadata.Applications.Schema.Interfaces;
using softwrench.sW4.Shared2.Data;
using softwrench.sW4.Shared2.Metadata.Applications.Relationships.Associations;
using softwrench.sW4.Shared2.Metadata.Applications.Schema;
using softWrench.sW4.Data.Search;

namespace softWrench.sW4.Metadata.Applications.Association {
    public class DataProviderResolver : ISingletonComponent {

        private readonly DynamicOptionFieldResolver _optionFieldResolver;
        private readonly ApplicationAssociationResolver _associationResolver;

        public DataProviderResolver(DynamicOptionFieldResolver optionFieldResolver, ApplicationAssociationResolver associationResolver) {
            _optionFieldResolver = optionFieldResolver;
            _associationResolver = associationResolver;
        }


        public async Task<IEnumerable<IAssociationOption>> ResolveOptions([NotNull] ApplicationSchemaDefinition schema,
            [NotNull] AttributeHolder originalEntity, [NotNull] IDataProviderContainer association,
            SearchRequestDto associationFilter) {

            if (association is ApplicationAssociationDefinition) {
                return await _associationResolver.ResolveOptions(schema, originalEntity,
                    (ApplicationAssociationDefinition)association, associationFilter);
            }
            return await _optionFieldResolver.ResolveOptions(schema, originalEntity, (OptionField)association, associationFilter);

        }
    }
}
