using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using softWrench.sW4.Data.Search;
using softWrench.sW4.Metadata.Entities;
using softWrench.sW4.Metadata.Entities.Schema;
using softWrench.sW4.Metadata.Entities.Sliced;
using softWrench.sW4.Util;

namespace softWrench.sW4.Data.Persistence.Relational.QueryBuilder.Basic {
    class QuerySearchSortBuilder {

        private const string AliasDelimiter = "\"";
        private const string LiteralDelimiter = "'";

        private const string CountClause = "select count(*) ";
        private const string SelectSeparator = ", ";
        private const EntityMetadata.AttributesMode NoCollections = EntityMetadata.AttributesMode.NoCollections;

        public static string BuildSearchSort(EntityMetadata entityMetadata, SearchRequestDto dto) {
            var searchSort = dto.SearchSort;
            if (String.IsNullOrWhiteSpace(searchSort)) {
                if (entityMetadata.HasUnion()) {
                    return " order by {0} desc".Fmt(entityMetadata.Schema.IdAttribute.Name);
                }
                return String.Format(" order by {0}.{1} desc", entityMetadata.Name, entityMetadata.Schema.IdAttribute.Name);
            }
            var suffix = dto.SearchAscending ? " asc " : " desc ";
            if (searchSort.EndsWith("asc") || searchSort.EndsWith("desc")) {
                suffix = "";
            }

            var attrs = entityMetadata.Attributes(EntityMetadata.AttributesMode.NoCollections);
            var attribute = attrs.FirstOrDefault(f => f.Name.Equals(dto.SearchSort.Trim(), StringComparison.CurrentCultureIgnoreCase));

            if (attribute != null && attribute.Query != null) {
                return GetQuerySortBy(entityMetadata, attribute, suffix);
            }

            if (!searchSort.Contains(".")) {
                //if union we cannot use full qualified names
                if (!dto.ExpressionSort && !entityMetadata.HasUnion()) {
                    return String.Format(" order by {0}.{1} {2}", entityMetadata.Name, searchSort, suffix);
                }
                return String.Format(" order by {0} {1}", searchSort, suffix);
            }
            if (entityMetadata.HasUnion()) {
                //this might replace sr_.ticketid for sr_ticketid, which is the right alias for paginated queries with union...
                searchSort = searchSort.Replace(".", "");
            }
            return String.Format(" order by {0} {1}", searchSort, suffix);
        }

        private static string GetQuerySortBy(EntityMetadata entityMetadata, EntityAttribute attribute, string suffix) {
            if (entityMetadata is SlicedEntityMetadata) {
                var a = (SlicedEntityMetadata)entityMetadata;
                if (a.HasUnion()) {
                    //TODO: review this entirely
                    return String.Format(" order by {0} {1}", attribute.Name, suffix);
                }
            }
            return String.Format(" order by {0} {1}", attribute.GetQueryReplacingMarkers(entityMetadata.Name), suffix);
        }
    }




}
