using System;
using System.Linq;
using softWrench.sW4.Data.Search;
using softWrench.sW4.Metadata.Entities;
using softWrench.sW4.Metadata.Entities.Schema;
using softWrench.sW4.Metadata.Entities.Sliced;
using System.Text;

namespace softWrench.sW4.Data.Persistence.Relational.QueryBuilder.Basic {
    public class QuerySearchSortBuilder {

        private const string AliasDelimiter = "\"";
        private const string LiteralDelimiter = "'";

        private const string CountClause = "select count(*) ";
        private const string SelectSeparator = ", ";
        private const EntityMetadata.AttributesMode NoCollections = EntityMetadata.AttributesMode.NoCollections;

        public static string BuildSearchSort(EntityMetadata entityMetadata, SearchRequestDto dto) {
            var searchSort = dto.TranslatedSearchSort ?? dto.SearchSort;
            var multiSearchSort = dto.TranslatedMultiSearchSort?? dto.MultiSearchSort;

            //third condition due to http://stackoverflow.com/questions/39559858/mvc-4-converting-to-one-element-list-with-null
            if (string.IsNullOrWhiteSpace(searchSort) && (multiSearchSort == null || multiSearchSort.Count == 0 || multiSearchSort.All(a => a == null))) {
                return $" order by {entityMetadata.Schema.UserIdAttribute.Name} desc";
            }

            if (multiSearchSort != null && multiSearchSort.Any(a => a != null)) {
                var builder = new StringBuilder();
                //TODO: review this method
                foreach (var column in multiSearchSort) {
                    if (!string.IsNullOrWhiteSpace(column.ColumnName)) {
                        var sort = $" {column.ColumnName} {(column.IsAscending ? " asc " : " desc ")}, ";
                        builder.Append(sort);
                    }
                }
                if (builder.Length > 0) {
                    return $" order by {builder.ToString().TrimEnd(',', ' ')}";
                }
                if (string.IsNullOrWhiteSpace(searchSort)) {
                    return $" order by {entityMetadata.Schema.UserIdAttribute.Name} desc";
                }
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
                return $" order by {searchSort} {suffix}";
            }


            return $" order by {searchSort} {suffix}";
        }

        private static string GetQuerySortBy(EntityMetadata entityMetadata, EntityAttribute attribute, string suffix) {
            if (entityMetadata is SlicedEntityMetadata) {
                var a = (SlicedEntityMetadata)entityMetadata;
                if (a.HasUnion()) {
                    //TODO: review this entirely
                    return $" order by {attribute.Name} {suffix}";
                }
            }
            return $" order by {attribute.GetQueryReplacingMarkers(entityMetadata.Name)} {suffix}";
        }
    }




}
