using softwrench.sW4.Shared2.Metadata.Entity.Association;
using softWrench.sW4.Metadata.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using softWrench.sW4.Metadata.Applications.Association;

namespace softWrench.sW4.Data.Persistence.Relational.QueryBuilder.Basic {
    class QueryJoinBuilder : BaseQueryBuilder {


        public static string Build(EntityMetadata entityMetadata, EntityAssociation association) {
            var sb = new StringBuilder();
            sb.AppendFormat("left join {0} on (", BaseQueryUtil.AliasEntity(association.To, association.Qualifier));

            sb.Append(AppendJoinConditions(entityMetadata, association)).Append(")");
            return sb.ToString();
        }

        public static string AppendJoinConditions(EntityMetadata entityMetadata, EntityAssociation association) {

            StringBuilder sb = new StringBuilder();

            var attributes = association.Attributes as IList<EntityAssociationAttribute>
              ?? association.Attributes.ToList();

            for (var i = 0; i < attributes.Count; i++) {
                var suffix = "";
                if (i < attributes.Count - 1) {
                    suffix = " and ";
                }
                var attribute = attributes[i];

                if (null != attribute.Query) {
                    var query = AssociationHelper.PrecompiledAssociationAttributeQuery(association.Qualifier, attribute);
                    sb.Append(query + suffix);
                } else if (null != attribute.From) {
                    var entityNameToUse = association.EntityName ?? entityMetadata.Name;
                    var from = Parse(entityNameToUse, attribute.From);
                    var to = attribute.To != null ? Parse(association.Qualifier, attribute.To) : ParseLiteral(attribute);
                    if (!attribute.AllowsNull) {
                        sb.AppendFormat("{0} = {1}" + suffix, @from, to);
                    } else {
                        sb.AppendFormat("({0} = {1} or {0} is null or {1} is null)" + suffix, @from, to);
                    }
                } else {
                    var value = ParseLiteral(attribute);
                    sb.AppendFormat("{0}.{1} = {2}" + suffix, association.Qualifier, attribute.To, value);
                }
            }
            return sb.ToString();
        }


        private static string ParseLiteral(EntityAssociationAttribute attribute) {
            var literal = attribute.Literal;
            var quoteLiteral = attribute.QuoteLiteral;

            var literalDelimeter = quoteLiteral ? BaseQueryUtil.LiteralDelimiter : "";
            var value = String.Format("{0}{1}{0}", literalDelimeter, literal);
            return value;
        }

        private static string Parse(string entityNameToUse, string attribute) {
            if (attribute.Contains("@")) {
                //example: replace(@itdcomment,'/','-') --> replace(person_.itdcomment,'/','-')
                return attribute.Replace("@", entityNameToUse + ".");
            }
            return String.Format("{0}.{1}", entityNameToUse, attribute);
        }
    }
}
