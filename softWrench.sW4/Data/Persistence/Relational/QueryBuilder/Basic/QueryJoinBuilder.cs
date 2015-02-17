﻿using softwrench.sW4.Shared2.Metadata.Entity.Association;
using softWrench.sW4.Metadata.Entities;
using cts.commons.simpleinjector;
using softWrench.sW4.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace softWrench.sW4.Data.Persistence.Relational.QueryBuilder.Basic {
    class QueryJoinBuilder :BaseQueryBuilder {


        public static string Build(EntityMetadata entityMetadata, EntityAssociation association) {
            var sb = new StringBuilder();
            sb.AppendFormat("left join {0} on (", BaseQueryUtil.AliasEntity(association.To, association.Qualifier));

            var attributes = association.Attributes as IList<EntityAssociationAttribute>
                ?? association.Attributes.ToList();

            for (var i = 0; i < attributes.Count; i++) {
                var suffix = (i < attributes.Count - 1) ? " and " : ")";
                var attribute = attributes[i];

                if (null != attribute.Query) {
                    var query = attribute.GetQueryReplacingMarkers(association.Qualifier);
                    if (query.StartsWith("@")) {
                        query = GetServiceQuery(query);
                    }
                    sb.Append(query + suffix);

                } else if (null != attribute.From) {
                    var entityNameToUse = association.EntityName ?? entityMetadata.Name;
                    var from = Parse(entityNameToUse, attribute.From);
                    var to = attribute.To != null ? Parse(association.Qualifier, attribute.To) : ParseLiteral(attribute);
                    sb.AppendFormat("{0} = {1}" + suffix, from, to);
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
