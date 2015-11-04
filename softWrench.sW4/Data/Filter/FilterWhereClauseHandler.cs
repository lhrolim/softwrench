using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using cts.commons.portable.Util;
using cts.commons.simpleinjector;
using softwrench.sw4.Shared2.Metadata.Applications.Filter;
using softwrench.sW4.Shared2.Metadata.Applications.Schema;
using softWrench.sW4.Data.Pagination;
using softWrench.sW4.Data.Persistence.Relational.QueryBuilder.Basic;
using softWrench.sW4.Data.Search;
using softWrench.sW4.Metadata;
using softWrench.sW4.Metadata.Applications;

namespace softWrench.sW4.Data.Filter {

    public class FilterWhereClauseHandler : ISingletonComponent {


        public PaginatedSearchRequestDto HandleDTO(ApplicationSchemaDefinition schema, PaginatedSearchRequestDto searchDto) {
            //force cache here
            var parameters = searchDto.GetParameters();
            var schemaFilters = schema.SchemaFilters;
            if (!schemaFilters.HasOverridenFilter || parameters == null) {
                //if all filters are the column filters no need to take any action
                return searchDto;
            }

            var entity = MetadataProvider.EntityByApplication(schema.ApplicationName);

            var allFilters = schemaFilters.Filters;


            foreach (var filter in allFilters) {
                SearchParameter paramValue = searchDto.RemoveSearchParam(filter.Attribute);
                if (paramValue == null) {
                    //this has not came from the client side as a client filter
                    continue;
                }

                var whereClause = filter.WhereClause;

                if (filter is MetadataOptionFilter && paramValue.SearchOperator.Equals(SearchOperator.CONTAINS) && !filter.IsTransient()) {
                    var optionFilter = (MetadataOptionFilter)filter;
                    searchDto.AppendWhereClause(GenerateFilterFreeTextWhereClause(optionFilter, paramValue.Value as string, schema));

                    //                    paramValue.IgnoreParameter = false;
                    //this means that we´re using a contains operation inside of an option filter, which should lead to default attribute lookup
                    continue;
                }

                if (whereClause == null) {
                    //this should lead to default filter implementation
                    continue;
                }

                if (!whereClause.StartsWith("@")) {
                    var values = paramValue.Value as IEnumerable<string>;
                    if (values != null) {
                        whereClause = whereClause.Replace("!@#value", BaseQueryUtil.GenerateInString(values));
                    } else if (paramValue.Value is string) {
                        whereClause = whereClause.Replace("!@#value", "'" + paramValue.Value + "'");
                    }
                    whereClause = whereClause.Replace("!@#value", paramValue.Value as string);
                    whereClause = whereClause.Replace("!@", entity.Name + ".");
                    //vanilla string case
                    searchDto.AppendWhereClause(whereClause);

                }
            }

            return searchDto;
        }

        public string GenerateFilterFreeTextWhereClause(MetadataOptionFilter filterProvider, string labelSearchString, ApplicationSchemaDefinition schema) {
            var attributeToUse = filterProvider.Attribute;
            if (attributeToUse.Equals(filterProvider.Provider) && filterProvider.Position != null) {
                //this means we´re applying a customization and the original attribute should rather be position. Check KOGT or DD metadata for servicerequest
                attributeToUse = filterProvider.Position;
            }

            var canUseProvider = schema.Fields.Any(f => f.Attribute.EqualsIc(filterProvider.Provider));

            if (canUseProvider) {
                //provider has to be declared as a field or a hidden field
                return "({0} like '{1}' or {2} like '{1}')".Fmt(filterProvider.Provider, labelSearchString,
                    attributeToUse);
            }
            return "({0} like '{1}')".Fmt(attributeToUse, labelSearchString);

        }

        public string GenerateFilterLookupWhereClause(string filterProvider, string labelSearchString, ApplicationSchemaDefinition schema) {
            var entityAssociation = MetadataProvider.Entity(schema.EntityName).LocateAssociationByLabelField(filterProvider);
            var primaryAttribute = entityAssociation.Item1.PrimaryAttribute();

            if (!string.IsNullOrEmpty(labelSearchString)) {
                return "({0} like '%{1}%' or {2} like '%{1}%')".Fmt(primaryAttribute.To, labelSearchString, entityAssociation.Item2.Name);
            }
            return "1=1";
        }
    }
}
