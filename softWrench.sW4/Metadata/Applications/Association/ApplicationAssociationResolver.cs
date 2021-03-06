﻿using System;
using System.Collections.Generic;
using System.Linq;
using softWrench.sW4.Data;
using softWrench.sW4.Data.Pagination;
using softWrench.sW4.Data.Persistence.Relational;
using softWrench.sW4.Data.Search;
using softWrench.sW4.Metadata.Applications.DataSet;
using softWrench.sW4.Metadata.Applications.DataSet.Filter;
using softwrench.sW4.Shared2.Data;
using softwrench.sw4.Shared2.Data.Association;
using softwrench.sW4.Shared2.Metadata.Applications.Relationships.Associations;
using softWrench.sW4.SimpleInjector;
using softWrench.sW4.Util;
using softwrench.sW4.Shared2.Metadata.Applications.Schema;
using softwrench.sW4.Shared2.Metadata.Applications;
using softWrench.sW4.Security.Services;
using softWrench.sW4.Metadata.Entities;
using softWrench.sW4.Metadata.Entities.Schema;
using softWrench.sW4.Data.Persistence.Relational.QueryBuilder.Basic;

namespace softWrench.sW4.Metadata.Applications.Association {

    class ApplicationAssociationResolver : BaseDependableResolver {

        
        private const string WrongPostFilterMethod = "PostfilterFunction {0} of dataset {1} was implemented with wrong signature. See IDataSet documentation";
        private const string ValueKeyConst = "value";

        private EntityRepository _repository;

        private EntityRepository EntityRepository {
            get {
                if (_repository == null) {
                    _repository =
                        SimpleInjectorGenericFactory.Instance.GetObject<EntityRepository>(typeof(EntityRepository));
                }
                return _repository;
            }
        }


        public static ApplicationMetadata GetAssociationApplicationMetadata(ApplicationAssociationDefinition association) {

            // See if association has a schema defined
            string optionApplication;
            string optionSchemaId;
            association.Schema.RendererParameters.TryGetValue("application", out optionApplication);
            association.Schema.RendererParameters.TryGetValue("schemaId", out optionSchemaId);

            if (!String.IsNullOrWhiteSpace(optionApplication) && !String.IsNullOrWhiteSpace(optionSchemaId)) {
                return MetadataProvider
                    .Application(optionApplication)
                    .ApplyPolicies(new ApplicationMetadataSchemaKey(optionSchemaId), SecurityFacade.CurrentUser(), ClientPlatform.Web);
            }
            return null;
        }

        public IEnumerable<IAssociationOption> ResolveOptions(ApplicationMetadata applicationMetadata,
            AttributeHolder originalEntity, ApplicationAssociationDefinition association) {
            return ResolveOptions(applicationMetadata, originalEntity, association, new SearchRequestDto());
        }

        public IEnumerable<IAssociationOption> ResolveOptions(ApplicationMetadata applicationMetadata,
            AttributeHolder originalEntity, ApplicationAssociationDefinition association, SearchRequestDto associationFilter) {
            if (!FullSatisfied(association, originalEntity)) {
                return null;
            }

            // Set dependante lookup atributes
            var lookupAttributes = association.LookupAttributes();
            foreach (var lookupAttribute in lookupAttributes) {
                var searchValue = SearchUtils.GetSearchValue(lookupAttribute, originalEntity);
                if (!String.IsNullOrEmpty(searchValue)) {
                    associationFilter.AppendSearchParam(lookupAttribute.To);
                    associationFilter.AppendSearchValue(searchValue);
                }  else if (lookupAttribute.Query != null) {
                    associationFilter.AppendWhereClause(lookupAttribute.GetQueryReplacingMarkers(association.EntityAssociation.To));
                }

            }

            // Set projections and pre filter functions
            var numberOfLabels = BuildProjections(associationFilter, association);
            var prefilterFunctionName = association.Schema.DataProvider.PreFilterFunctionName;
            if (prefilterFunctionName != null) {
                var preFilterParam = new AssociationPreFilterFunctionParameters(applicationMetadata, associationFilter, association, originalEntity);
                associationFilter = PrefilterInvoker.ApplyPreFilterFunction(DataSetProvider.GetInstance().LookupDataSet(applicationMetadata.Name),preFilterParam, prefilterFunctionName);
            }

            var entityMetadata = MetadataProvider.Entity(association.EntityAssociation.To);
            associationFilter.QueryAlias = association.AssociationKey;
            var queryResponse = EntityRepository.Get(entityMetadata, associationFilter);

            if (associationFilter is PaginatedSearchRequestDto) {
                var paginatedFilter = (PaginatedSearchRequestDto)associationFilter;
                if (paginatedFilter.NeedsCountUpdate) {
                    paginatedFilter.TotalCount = EntityRepository.Count(entityMetadata, associationFilter);
                }
            }

            var options = BuildOptions(queryResponse, association, numberOfLabels);
            string filterFunctionName = association.Schema.DataProvider.PostFilterFunctionName;
            return filterFunctionName != null ? ApplyFilters(applicationMetadata.Name, originalEntity, filterFunctionName, options, association) : options;
        }




        private ISet<IAssociationOption> BuildOptions(IEnumerable<AttributeHolder> queryResponse,
            ApplicationAssociationDefinition association, ProjectionResult projectionResult) {
            ISet<IAssociationOption> options = new SortedSet<IAssociationOption>();
            foreach (var attributeHolder1 in queryResponse) {
                var attributeHolder = (DataMap)attributeHolder1;
                var value = attributeHolder.GetAttribute(projectionResult.ValueKey);

                var labelNumber = association.LabelFields.Count;
                var label = labelNumber == 1
                                    ? (string)attributeHolder.GetAttribute(association.LabelFields[0])
                                    : BuildComplexLabel(attributeHolder, association);

                if (association.ExtraProjectionFields.Count > 0) {
                    options.Add(new MultiValueAssociationOption((string)value, label, attributeHolder));
                } else {
                    options.Add(new AssociationOption((string)value, label));
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

     

        private IEnumerable<IAssociationOption> ApplyFilters(string applicationName, AttributeHolder originalEntity, string filterFunctionName, ISet<IAssociationOption> options, ApplicationAssociationDefinition association) {
            var dataSet = FindDataSet(applicationName, filterFunctionName);
            var mi = dataSet.GetType().GetMethod(filterFunctionName);
            if (mi == null) {
                throw new InvalidOperationException(String.Format(MethodNotFound, filterFunctionName, dataSet.GetType().Name));
            }
            if (mi.GetParameters().Count() != 1)
            {
                throw new InvalidOperationException(String.Format(WrongPostFilterMethod, filterFunctionName, dataSet.GetType().Name));
            }
            var postFilterParam = new AssociationPostFilterFunctionParameters()
            {
                Options = options,
                OriginalEntity = originalEntity,
                Association = association
            };
            return (IEnumerable<IAssociationOption>)mi.Invoke(dataSet, new object[] { postFilterParam });
        }

        private static ProjectionResult BuildProjections(SearchRequestDto searchRequestDto, ApplicationAssociationDefinition association) {

            var valueField = association.EntityAssociation.PrimaryAttribute().To;

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

            if (valueKey == ValueKeyConst) {
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
