using System;
using System.Linq;
using softWrench.sW4.Data.Search;
using softWrench.sW4.Metadata.Entities;
using softWrench.sW4.Metadata.Entities.Schema;
using softWrench.sW4.Metadata.Entities.Sliced;

namespace softWrench.sW4.Data.Persistence.Relational.QueryBuilder.Basic {
    public class QuerySearchSortBuilder {

        private const string AliasDelimiter = "\"";
        private const string LiteralDelimiter = "'";

        private const string CountClause = "select count(*) ";
        private const string SelectSeparator = ", ";
        private const EntityMetadata.AttributesMode NoCollections = EntityMetadata.AttributesMode.NoCollections;

        public static string BuildSearchSort(EntityMetadata entityMetadata, SearchRequestDto dto) {
            var searchSort = dto.SearchSort;
            if (String.IsNullOrWhiteSpace(searchSort)) {
                return String.Format(" order by {1} desc", entityMetadata.Name, entityMetadata.Schema.UserIdAttribute.Name);
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
                //                if (!dto.ExpressionSort) {
                //                    return String.Format(" order by {0}.{1} {2}", entityMetadata.Name, searchSort, suffix);
                //                }
                return String.Format(" order by {0} {1}", searchSort, suffix);
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
