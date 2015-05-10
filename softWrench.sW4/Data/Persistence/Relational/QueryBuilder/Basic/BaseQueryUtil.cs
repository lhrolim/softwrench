using System.Collections.Generic;
using System.Linq;
using System.Text;
using softWrench.sW4.Metadata;
using softWrench.sW4.Metadata.Entities;
using softWrench.sW4.Metadata.Entities.Schema;
using softwrench.sW4.Shared2.Data;

namespace softWrench.sW4.Data.Persistence.Relational.QueryBuilder.Basic {
    public class BaseQueryUtil {

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

        public static string GenerateInString(IEnumerable<string> items) {
            var sb = new StringBuilder();
            foreach (var item in items) {
                sb.Append("'").Append(item).Append("'");
                sb.Append(",");
            }
            return sb.ToString(0, sb.Length - 1);
        }

        public static string GenerateInString(IEnumerable<DataMap> items) {
            var dataMaps = items as DataMap[] ?? items.ToArray();
            if (items== null || !dataMaps.Any()) {
                return null;
            }

            var sb = new StringBuilder();
            foreach (var item in dataMaps) {
                sb.Append("'").Append(item.Id).Append("'");
                sb.Append(",");
            }
            return sb.ToString(0, sb.Length - 1);
        }

    }
}
