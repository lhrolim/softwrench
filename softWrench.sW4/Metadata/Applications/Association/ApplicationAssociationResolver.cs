using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using cts.commons.portable.Util;
using softWrench.sW4.Data;
using softWrench.sW4.Data.Pagination;
using softWrench.sW4.Data.Persistence.Dataset.Commons;
using softWrench.sW4.Data.Persistence.Relational.EntityRepository;
using softWrench.sW4.Data.Search;
using softWrench.sW4.Metadata.Applications.DataSet.Filter;
using softwrench.sW4.Shared2.Data;
using softwrench.sw4.Shared2.Data.Association;
using softwrench.sW4.Shared2.Metadata.Applications.Relationships.Associations;
using cts.commons.simpleinjector;
using JetBrains.Annotations;
using softwrench.sW4.Shared2.Metadata.Applications.Schema;
using softwrench.sW4.Shared2.Metadata.Applications;
using softWrench.sW4.Data.Search.QuickSearch;
using softWrench.sW4.Security.Services;
using softWrench.sW4.Metadata.Entities;
using softWrench.sW4.Util;

namespace softWrench.sW4.Metadata.Applications.Association {

    public class ApplicationAssociationResolver : BaseDependableResolver {


        private const string WrongPostFilterMethod = "PostfilterFunction {0} of dataset {1} was implemented with wrong signature. See IDataSet documentation";
        private const string ValueKeyConst = "value";

        private static EntityRepository EntityRepository {
            get {
                return SimpleInjectorGenericFactory.Instance.GetObject<EntityRepository>(typeof(EntityRepository));
            }
        }


        private static QuickSearchHelper QuickSearchHelper {
            get {
                return SimpleInjectorGenericFactory.Instance.GetObject<QuickSearchHelper>(typeof(QuickSearchHelper));
            }
        }

        public static ApplicationMetadata GetAssociationApplicationMetadata(ApplicationAssociationDefinition association) {

            // See if association has a schema defined
            string optionApplication;
            string optionSchemaId;
            association.Schema.RendererParameters.TryGetValueAsString("application", out optionApplication);
            association.Schema.RendererParameters.TryGetValueAsString("schemaId", out optionSchemaId);

            if (!String.IsNullOrWhiteSpace(optionApplication) && !String.IsNullOrWhiteSpace(optionSchemaId)) {
                return MetadataProvider
                    .Application(optionApplication)
                    .ApplyPolicies(new ApplicationMetadataSchemaKey(optionSchemaId), SecurityFacade.CurrentUser(), ClientPlatform.Web, null);
            }
            return null;
        }

        public IEnumerable<IAssociationOption> ResolveOptions(ApplicationMetadata applicationMetadata,
            AttributeHolder originalEntity, ApplicationAssociationDefinition association) {
            return ResolveOptions(applicationMetadata.Schema, originalEntity, association, new SearchRequestDto());
        }

        [CanBeNull]
        public IEnumerable<IAssociationOption> ResolveOptions([NotNull]ApplicationSchemaDefinition schema,
            [NotNull]AttributeHolder originalEntity, [NotNull] ApplicationAssociationDefinition association, SearchRequestDto associationFilter) {
            if (!FullSatisfied(association, originalEntity)) {
                return null;
            }
            //TODO: remove this workaround, but need to refactor a bunch of prefilters
            var applicationMetadata = ApplicationMetadata.FromSchema(schema);

            var isLookupMode = associationFilter is PaginatedSearchRequestDto && association.RendererType == "lookup";


            AppendNonPrimaryAttributesSearch(originalEntity, association, associationFilter);


            // handles quick search request
            if (associationFilter.QuickSearchDTO != null) {
                AppendQuickSearch(association, associationFilter);
            }

            // Set projections
            var numberOfLabels = BuildProjections(associationFilter, association);

            //Set the orderbyfield if any
            DefineSorting(association, associationFilter);

            // Set pre-filter functions
            var prefilterFunctionName = association.Schema.DataProvider.PreFilterFunctionName;
            if (prefilterFunctionName != null) {
                var preFilterParam = new AssociationPreFilterFunctionParameters(applicationMetadata, associationFilter, association, originalEntity);
                associationFilter = PrefilterInvoker.ApplyPreFilterFunction(DataSetProvider.GetInstance().LookupDataSet(applicationMetadata.Name, applicationMetadata.Schema.SchemaId), preFilterParam, prefilterFunctionName);
            }

            if (association.Schema.DataProvider.WhereClause != null) {
                associationFilter.AppendWhereClause(EntityUtil.EvaluateQuery(association.Schema.DataProvider.WhereClause, originalEntity));
            }

            var entityMetadata = MetadataProvider.Entity(association.EntityAssociation.To);
            associationFilter.QueryAlias = association.AssociationKey;

            //caching for multithread access
            associationFilter.GetParameters();

            var queryResponse = EntityRepository.Get(entityMetadata, associationFilter);

            // execute query count in separate thread and update the dto with the pagination data
            if (isLookupMode) {
                ExecuteCountQuery(associationFilter, entityMetadata);
            }

            var options = BuildOptions(queryResponse, association, numberOfLabels, associationFilter.SearchSort == null);
            var filterFunctionName = association.Schema.DataProvider.PostFilterFunctionName;
            if (filterFunctionName != null) {
                var filteredResult = ApplyFilters(applicationMetadata, originalEntity, filterFunctionName, options, association);
                var paginatedFilter = associationFilter as PaginatedSearchRequestDto;
                var associationOptions = filteredResult as IList<IAssociationOption> ?? filteredResult.ToList();
                if (paginatedFilter != null) {
                    //limiting the number of results to the number filtered
                    paginatedFilter.TotalCount = associationOptions.Count();
                }
                return associationOptions;
            }
            return options;

        }

        private void ExecuteCountQuery(SearchRequestDto associationFilter, EntityMetadata entityMetadata) {
            var tasks = new List<Task>(1) {
                Task.Factory.NewThread(c => {
                    var paginatedFilter = (PaginatedSearchRequestDto) associationFilter;
                    if (paginatedFilter.NeedsCountUpdate) {
                        //cloning to avoid any concurrency issues
                        var clonedDTO = (PaginatedSearchRequestDto) associationFilter.ShallowCopy();
                        paginatedFilter.TotalCount = EntityRepository.Count(entityMetadata, clonedDTO);
                    }
                }, null)
            };

            Task.WaitAll(tasks.ToArray());
        }

        private void AppendQuickSearch(ApplicationAssociationDefinition association, SearchRequestDto associationFilter) {
            var appMetadata = GetAssociationApplicationMetadata(association);
            var primaryAttribute = association.EntityAssociation.PrimaryAttribute();

            // has primary and not in a reverse association and primary does not require an extra join
            // TODO: add support for the extra join in quicksearch queries
            var shouldUsePrimary = primaryAttribute != null && !association.Reverse && !primaryAttribute.To.Contains("_");

            var listOfFields = appMetadata != null
                ? appMetadata.Schema.NonHiddenFields.Where(i => !i.DeclaredAsQueryOnEntity).Select(f => f.Attribute)
                : shouldUsePrimary
                    ? new[] { primaryAttribute.To, GetLabelFieldForQuickSearch(association) }
                    : new[] { GetLabelFieldForQuickSearch(association) };

            var quickSearchWhereClause = QuickSearchHelper.BuildOrWhereClause(listOfFields, association.EntityAssociation.To);
            associationFilter.AppendWhereClause(quickSearchWhereClause);
        }

        /// <summary>
        /// Solves the case in which the association's labelfield is a query on the target entity: 
        /// association.EntityAssociation.To#[labelfield].Query != `empty`.
        /// </summary>
        /// <param name="association"></param>
        /// <returns></returns>
        private static string GetLabelFieldForQuickSearch(ApplicationAssociationDefinition association) {
            var field = association.LabelFields.FirstOrDefault();
            if (string.IsNullOrEmpty(field))
                return field;
            var targetEntity = MetadataProvider.Entity(association.EntityAssociation.To);
            if (targetEntity == null)
                return field;
            var targetField = targetEntity.LocateAttribute(field);
            if (targetField == null || string.IsNullOrEmpty(targetField.Query))
                return field;
            return AssociationHelper.PrecompiledAssociationAttributeQuery(association.EntityAssociation.To, targetField);
        }

        private static void AppendNonPrimaryAttributesSearch(AttributeHolder originalEntity, ApplicationAssociationDefinition association, SearchRequestDto associationFilter) {
            // Set dependant lookup atributes
            foreach (var lookupAttribute in association.LookupAttributes()) {
                // case where attribute has a query
                if (lookupAttribute.Query != null) {
                    // pre-interpret the query before appending
                    var query = AssociationHelper.PrecompiledAssociationAttributeQuery(association.EntityAssociation.To, lookupAttribute);
                    associationFilter.AppendWhereClause(query);
                    continue;
                }
                // case with to->from or to->literal relationship
                var searchValue = SearchUtils.GetSearchValue(lookupAttribute, originalEntity);
                if (!string.IsNullOrEmpty(searchValue)) {
                    associationFilter.AppendSearchEntry(lookupAttribute.To, searchValue, lookupAttribute.AllowsNull);
                }
            }
        }

        /// Sorting Precedence 
        /// 1. Sort by orderbyfield - if defined
        /// 2. Sort by value - if defined
        /// 3. Sort by the first projection field - if defined
        /// 4. Sort by the first column in the SQL query
        /// 5. Sort by prefilter function - if defined
        /// 
        private static void DefineSorting(ApplicationAssociationDefinition association, SearchRequestDto associationFilter) {
            var orderByField = association.OrderByField;
            if (orderByField != null) {
                associationFilter.SearchSort = orderByField;
                associationFilter.SearchAscending = !orderByField.EndsWith("desc");
            } else if (string.IsNullOrWhiteSpace(associationFilter.SearchSort)) {
                if (associationFilter.ProjectionFields.Any(f => f.Alias == "value")) {
                    associationFilter.SearchSort = "value";
                } else {
                    // Applying 1 will cause the query to order by the first column
                    associationFilter.SearchSort = associationFilter.ProjectionFields.Any()
                        ? associationFilter.ProjectionFields.First().Alias
                        : "1";
                }
                associationFilter.SearchAscending = true;
            }
        }


        private ISet<IAssociationOption> BuildOptions(IEnumerable<AttributeHolder> queryResponse,
            ApplicationAssociationDefinition association, ProjectionResult projectionResult, bool useInMemorySort) {
            ISet<IAssociationOption> options = new HashSet<IAssociationOption>();
            if (useInMemorySort) {
                //legacy code to avoid any wrong scenarios where no sort has been specified on the query itself
                options = new SortedSet<IAssociationOption>();
            }
            foreach (var attributeHolder1 in queryResponse) {
                var attributeHolder = (DataMap)attributeHolder1;
                var value = attributeHolder.GetAttribute(projectionResult.ValueKey);
                // If the value is null, skip this conversion and continue executing
                if (value == null) {
                    continue;
                }
                if (value.GetType() == typeof(decimal)) {
                    value = Convert.ToInt32(value);
                }

                var labelNumber = association.LabelFields.Count;
                var label = labelNumber == 1
                                    ? (string)attributeHolder.GetAttribute(association.LabelFields[0])
                                    : BuildComplexLabel(attributeHolder, association);

                if (association.ExtraProjectionFields.Count > 0) {
                    options.Add(new MultiValueAssociationOption(Convert.ToString(value), label, attributeHolder, association.ForceDistinctOptions));
                } else {

                    options.Add(new AssociationOption(Convert.ToString(value), label));
                }
            }
            return options;
        }

        private string BuildComplexLabel(AttributeHolder attributeHolder, ApplicationAssociationDefinition association) {
            var fmt = new object[association.LabelFields.Count];
            for (var i = 0; i < association.LabelFields.Count; i++) {
                fmt[i] = attributeHolder.GetAttribute(association.LabelFields[i], true);
            }
            return String.Format(association.LabelPattern, fmt);
        }

        private IEnumerable<IAssociationOption> ApplyFilters(ApplicationMetadata app, AttributeHolder originalEntity, string filterFunctionName, ISet<IAssociationOption> options, ApplicationAssociationDefinition association) {
            var dataSet = FindDataSet(app.Name, app.Schema.SchemaId, filterFunctionName);
            var mi = dataSet.GetType().GetMethod(filterFunctionName);
            if (mi == null) {
                throw new InvalidOperationException(String.Format(MethodNotFound, filterFunctionName, dataSet.GetType().Name));
            }
            if (mi.GetParameters().Count() != 1) {
                throw new InvalidOperationException(String.Format(WrongPostFilterMethod, filterFunctionName, dataSet.GetType().Name));
            }
            var postFilterParam = new AssociationPostFilterFunctionParameters() {
                Options = options,
                OriginalEntity = originalEntity,
                Association = association
            };
            var result = (IEnumerable<IAssociationOption>)mi.Invoke(dataSet, new object[] { postFilterParam });



            return result;
        }

        private static ProjectionResult BuildProjections(SearchRequestDto searchRequestDto, ApplicationAssociationDefinition association) {

            var entityAssociation = association.EntityAssociation;
            var valueField = association.ValueField;
            if (entityAssociation.Reverse) {
                valueField = entityAssociation.ReverseLookupAttribute;
            }

            // See if association has a schema defined
            var associationMetadata = GetAssociationApplicationMetadata(association);
            var valueKey = ValueKeyConst;
            var fields = association.LabelFields;

            if (associationMetadata != null) {
                //if we have a schema then the projections should be all the fields out of it, except collections, instead of default labelfields
                var entityMetatada = MetadataProvider.SlicedEntityMetadata(associationMetadata);
                fields = entityMetatada.Attributes(EntityMetadata.AttributesMode.NoCollections).Select(a => a.Name).ToList();


            }

            foreach (var field in fields) {
                searchRequestDto.AppendProjectionField(new ProjectionField { Alias = field, Name = field });
                if (field == valueField) {
                    valueKey = field;
                }
            }

            if (valueKey == ValueKeyConst && valueField != null) {
                searchRequestDto.AppendProjectionField(new ProjectionField { Alias = ValueKeyConst, Name = valueField });
            }

            foreach (var extraField in association.ExtraProjectionFields) {
                searchRequestDto.AppendProjectionField(new ProjectionField { Alias = extraField, Name = extraField });
            }
            return new ProjectionResult(valueKey);
        }

        class ProjectionResult {
            internal readonly string ValueKey;

            public ProjectionResult(string valueKey) {
                ValueKey = valueKey;
            }
        }


    }
}
