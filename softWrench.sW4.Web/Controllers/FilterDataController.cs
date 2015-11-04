﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Web.Http;
using cts.commons.portable.Util;
using cts.commons.simpleinjector;
using cts.commons.web.Attributes;
using DocumentFormat.OpenXml.Office2010.Excel;
using JetBrains.Annotations;
using Newtonsoft.Json.Linq;
using softwrench.sw4.api.classes.fwk.filter;
using softwrench.sw4.Shared2.Data.Association;
using softwrench.sW4.audit.Interfaces;
using softWrench.sW4.Data.API;
using softWrench.sW4.Data.API.Association;
using softWrench.sW4.Data.API.Composition;
using softWrench.sW4.Data.API.Response;
using softWrench.sW4.Data.Relationship.Composition;
using softWrench.sW4.Metadata;
using softWrench.sW4.Metadata.Applications;
using softWrench.sW4.Security.Context;
using softWrench.sW4.Security.Services;
using softwrench.sW4.Shared2.Metadata.Applications;
using softwrench.sW4.Shared2.Metadata.Applications.Relationships.Associations;
using softwrench.sW4.Shared2.Metadata.Applications.Schema;
using softWrench.sW4.Data.Entities;
using softWrench.sW4.Data.Filter;
using softWrench.sW4.Data.Pagination;
using softWrench.sW4.Data.Persistence.Dataset.Commons;
using softWrench.sW4.Data.Search;
using softWrench.sW4.Metadata.Applications.Association;
using softWrench.sW4.SPF;
using softWrench.sW4.Util;

namespace softWrench.sW4.Web.Controllers {

    [Authorize]
    [SWControllerConfiguration]
    public class FilterDataController : ApiController {


        private readonly DataSetProvider _dataSetProvider;
        private readonly ApplicationAssociationResolver _associationResolver;
        private readonly FilterWhereClauseHandler _filterWhereClauseHandler;

        public FilterDataController(DataSetProvider dataSetProvider, ApplicationAssociationResolver associationResolver, FilterWhereClauseHandler filterWhereClauseHandler) {
            _dataSetProvider = dataSetProvider;
            _associationResolver = associationResolver;
            _filterWhereClauseHandler = filterWhereClauseHandler;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="application"></param>
        /// <param name="key"></param>
        /// <param name="filterProvider">as described on MetadataOptionFilter</param>
        /// <param name="filterAttribute"></param>
        /// <param name="labelSearchString"></param>
        /// <returns></returns>
        [HttpGet]
        public IEnumerable<IAssociationOption> GetFilterOptions(string application, [FromUri]ApplicationMetadataSchemaKey key, string filterProvider, string filterAttribute, string labelSearchString) {

            if (filterProvider.StartsWith("@")) {
                var methodName = filterProvider.Substring(1);
                var dataSet = _dataSetProvider.LookupDataSet(application, key.SchemaId);
                var mi = ReflectionUtil.GetMethodNamed(dataSet, methodName);
                var filterParam = new FilterProviderParameters(labelSearchString, filterAttribute, key.SchemaId);
                return (IEnumerable<IAssociationOption>)mi.Invoke(dataSet, new object[] { filterParam });
            }
            //this is the main application, such as sr
            var app = MetadataProvider.Application(application).ApplyPoliciesWeb(key);
            var association = ApplicationAssociationFactory.GetFilterInstance(application, filterProvider, filterAttribute);

            var filter = new PaginatedSearchRequestDto();

            filter.AppendWhereClause(_filterWhereClauseHandler.GenerateFilterLookupWhereClause(filterProvider, labelSearchString, app.Schema));
            //let´s limit the filter adding an extra value so that we know there´re more to be brought
            //TODO: add a count call
            filter.PageSize = 21;
            //adopting to use an association to keep same existing service
            var result = _associationResolver.ResolveOptions(app, Entity.GetInstance(MetadataProvider.EntityByApplication(application)), association, filter);
            return result;

        }


    }
}