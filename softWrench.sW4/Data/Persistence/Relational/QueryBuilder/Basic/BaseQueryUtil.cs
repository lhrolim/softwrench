using softWrench.sW4.Metadata;
using softWrench.sW4.Metadata.Entities;
using softWrench.sW4.Metadata.Entities.Schema;

namespace softWrench.sW4.Data.Persistence.Relational.QueryBuilder.Basic {
    class BaseQueryUtil {

        public const string LiteralDelimiter = "'";

        public static string AliasEntity(string entity, string alias) {
            var metadata = MetadataProvider.Entity(entity);
            var table = metadata.GetTableName();

            return string.Format("{0} as {1}", table, alias);
        }

        public static string QualifyAttribute(EntityMetadata entityMetadata, EntityAttribute attribute) {
            return attribute.IsAssociated
                ? attribute.Name
                : string.Format("{0}.{1}", entityMetadata.Name, attribute.Name);
        }

    }
}
