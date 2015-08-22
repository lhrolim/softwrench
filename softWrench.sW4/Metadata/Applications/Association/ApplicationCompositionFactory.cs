using System;
using System.Linq;
using JetBrains.Annotations;
using softwrench.sW4.Shared2.Metadata.Applications.Relationships.Compositions;
using softwrench.sw4.Shared2.Metadata.Applications.Schema;
using softwrench.sW4.Shared2.Metadata.Entity.Association;
using softWrench.sW4.Util;

namespace softWrench.sW4.Metadata.Applications.Association {
    class ApplicationCompositionFactory {

        public static ApplicationCompositionDefinition GetInstance([NotNull] string @from, [NotNull] string relationship, string label, ApplicationCompositionSchema schema, string showExpression, string toolTip, bool hidden, bool printEnabled, ApplicationHeader header) {
            var composition = new ApplicationCompositionDefinition(from, relationship, label, schema, showExpression,
                toolTip, hidden, printEnabled, header);
            composition.SetLazyResolver(new Lazy<EntityAssociation>(
                    () => {
                        var metadata = MetadataProvider.Application(@from);
                        var suffixed = EntityUtil.GetRelationshipName(relationship);

                        return MetadataProvider
                            .Entity(MetadataProvider.Application(@from).Entity)
                            .Associations
                            .FirstOrDefault(a => a.Qualifier == suffixed);
                    }));
            return composition;
        }
    }
}
