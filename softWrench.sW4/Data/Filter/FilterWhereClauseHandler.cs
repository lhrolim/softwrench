using System;
using System.Collections.Generic;
using System.Linq;
using cts.commons.portable.Util;
using cts.commons.simpleinjector;
using softwrench.sw4.Shared2.Metadata.Applications.Filter;
using softwrench.sW4.Shared2.Metadata.Applications.Schema;
using softWrench.sW4.Data.Pagination;
using softWrench.sW4.Data.Persistence;
using softWrench.sW4.Data.Persistence.Dataset.Commons;
using softWrench.sW4.Data.Persistence.Relational.QueryBuilder.Basic;
using softWrench.sW4.Data.Search;
using softWrench.sW4.Data.Search.QuickSearch;
using softWrench.sW4.Metadata;
using softWrench.sW4.Metadata.Applications.DataSet.Filter;
using softWrench.sW4.Util;

namespace softWrench.sW4.Data.Filter {

    public class FilterWhereClauseHandler : ISingletonComponent
    {

        private QuickSearchHelper _quickSearchHelper;

        public FilterWhereClauseHandler(QuickSearchHelper quickSearchHelper)
        {
            _quickSearchHelper = quickSearchHelper;
        }


        public PaginatedSearchRequestDto HandleDTO(ApplicationSchemaDefinition schema, PaginatedSearchRequestDto searchDto) {
            //force cache here
            var parameters = searchDto.GetParameters();
            var schemaFilters = schema.SchemaFilters;

            // if all filters are the column filters no need to take any action
            if (schemaFilters == null || !schemaFilters.HasOverridenFilter || parameters == null) {
                return searchDto;
            }

            // has QuickSearch string: use parameter in filter regardless of WhereClause
            if (searchDto.QuickSearchDTO != null) {
                return searchDto;
            }

            var entity = MetadataProvider.EntityByApplication(schema.ApplicationName);

            var allFilters = schemaFilters.Filters;

            foreach (var filter in allFilters) {
                // mark as ignored until we check whether or not it has a WhereClause
                var paramValue = searchDto.RemoveSearchParam(filter.Attribute);
                // this has not come from the client side as a client filter
                if (paramValue == null) {
                    continue;
                }

                var whereClause = filter.WhereClause;

                if (filter is MetadataOptionFilter && paramValue.SearchOperator.Equals(SearchOperator.CONTAINS) && !filter.IsTransient()) {
                    var optionFilter = (MetadataOptionFilter)filter;
                    searchDto.AppendWhereClause(GenerateFilterFreeTextWhereClause(optionFilter, paramValue.Value as string, schema));
                    // paramValue.IgnoreParameter = false;
                    // this means that we´re using a contains operation inside of an option filter, which should lead to default attribute lookup
                    continue;
                }
                // has SearchTemplate string: param has to be filtered regardless of whereclause
                if (whereClause == null) {
                    paramValue.IgnoreParameter = false;
                    //this should lead to default filter implementation
                    continue;
                }

                if (!whereClause.StartsWith("@")) {
                    var values = paramValue.Value as IEnumerable<string>;
                    if (values != null) {
                        whereClause = whereClause.Replace("!@#value", BaseQueryUtil.GenerateInString(values));
                    } else if (paramValue.Value is string) {
                        whereClause = whereClause.Replace("%!@#value%", "'%" + paramValue.Value + "%'");
                        whereClause = whereClause.Replace("%!@#value", "'%" + paramValue.Value + "'");
                        whereClause = whereClause.Replace("!@#value%", "'" + paramValue.Value + "%'");
                        whereClause = whereClause.Replace("!@#value", "'" + paramValue.Value + "'");
                        whereClause = whereClause.Replace("!@",
                            MetadataProvider.Entity(schema.EntityName).GetTableName() + ".");
                    }
                    whereClause = whereClause.Replace("!@#value", paramValue.Value as string);
                    whereClause = whereClause.Replace("!@", entity.Name + ".");
                    //vanilla string case
                    searchDto.AppendWhereClause(whereClause);
                } else {
                    HandleServiceWhereClauseHandler(schema, searchDto, paramValue, whereClause);
                }
                // TODO: starts with @: call service that builds whereclause

            }

            return searchDto;
        }

        private static void HandleServiceWhereClauseHandler(ApplicationSchemaDefinition schema,
            PaginatedSearchRequestDto searchDto, SearchParameter paramValue, string whereClause) {
            var parameter = new FilterWhereClauseParameters(schema, searchDto, paramValue);
            var result = GenericSwMethodInvoker.Invoke<string>(schema, whereClause, parameter);
            if (result != null) {
                searchDto.AppendWhereClause(result);
            }
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

            return "({0}.{1} like '{2}')".Fmt(schema.EntityName, attributeToUse, labelSearchString);

        }

        public string GenerateFilterLookupWhereClause(string filterProvider, string labelSearchString, ApplicationSchemaDefinition schema) {
            var entityAssociation = MetadataProvider.Entity(schema.EntityName).LocateAssociationByLabelField(filterProvider);
            var primaryAttribute = entityAssociation.Item1.PrimaryAttribute();

            if (!string.IsNullOrEmpty(labelSearchString)) {
                return _quickSearchHelper.BuildOrWhereClause(new List<string>
                {
                    primaryAttribute.To,
                    entityAssociation.Item2.Name
                });

            }
            return "1=1";
        }
    }
}
