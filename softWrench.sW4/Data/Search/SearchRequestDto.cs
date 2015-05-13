﻿using Newtonsoft.Json;
using softwrench.sW4.Shared2.Metadata.Applications.Relationships.Compositions;
using softwrench.sW4.Shared2.Metadata.Applications.Schema;
using softwrench.sW4.Shared2.Metadata.Applications.Schema.Interfaces;
using softWrench.sW4.Data.API;
using softWrench.sW4.Data.Sync;
using softWrench.sW4.Metadata.Entities.Sliced;
using softWrench.sW4.Security.Context;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace softWrench.sW4.Data.Search {
    public class SearchRequestDto : IDataRequest {


        private ApplicationLookupContext _context;

        public IDictionary<string, string> CustomParameters { get; set; }
        public string CommandId { get; set; }

        /// <summary>
        /// Receives the query to apply in the format :a || :b || :c || :d. 
        /// FWK shall locate corresponding SearchValues to replace it
        /// </summary>
        public String SearchTemplate { get; set; }

        public String SearchParams { get; set; }

        public String SearchValues { get; set; }

        public String SearchSort { get; set; }

        public Boolean ExpressionSort { get; set; }

        public Boolean SearchAscending { get; set; }

        private ISet<ProjectionField> _projectionFields = new HashSet<ProjectionField>();

        public String WhereClause { get; set; }

        public Boolean IgnoreWhereClause { get; set; }

        public ApplicationLookupContext Context { get; set; }

        public Rowstamps Rowstamps { get; set; }

        public string Id { get; set; }

        private IDictionary<string, SearchParameter> _valuesDictionary;

        public ApplicationMetadataSchemaKey Key { get; set; }

        public String Title { get; set; }

        public string FilterFixedWhereClause { get; set; }



        public void BuildProjection(ApplicationSchemaDefinition schema) {
            var columns = schema.Fields;
            foreach (var column in columns) {
                AppendProjectionField(new ProjectionField { Name = column.Attribute, Alias = column.Attribute });
            }
        }


        internal void BuildProjection(ApplicationCompositionSchema schema, bool printMode = false, bool offLineMode = false) {
            var schemaToUse = schema.Schemas.List;

            if (printMode) {
                schemaToUse = schema.Schemas.Print;
            } else if (offLineMode) {
                schemaToUse = schema.Schemas.Sync;
            }

            var columns = schemaToUse.Displayables;
            foreach (var column in columns) {
                var displayable = column as IApplicationAttributeDisplayable;
                if (displayable == null || displayable.Attribute == null) {
                    //displayable.Attribute might be null in case of sections
                    continue;
                }
                AppendProjectionField(new ProjectionField { Name = displayable.Attribute, Alias = displayable.Attribute });
            }
        }

        public static SearchRequestDto GetFromDictionary(IDictionary<String, string> searchDictionary) {
            var searchRequestDto = new SearchRequestDto();

            foreach (var key in searchDictionary.Keys) {
                searchRequestDto.AppendSearchParam(key);
                searchRequestDto.AppendSearchValue(searchDictionary[key]);
            }
            return searchRequestDto;
        }

        public static SearchRequestDto GetUnionSearchRequestDto(SearchRequestDto originalSearchRequestDto, SlicedEntityMetadata unionSchema) {
            var unionSearchRequestDto = new SearchRequestDto();

            if (!String.IsNullOrWhiteSpace(originalSearchRequestDto.SearchParams)) {

                var sb = new StringBuilder(originalSearchRequestDto.SearchParams);
                var sbReplacingIdx = 0;
                var nullParamCounter = 0;

                foreach (var parameter in originalSearchRequestDto.GetParameters()) {

                    var key = parameter.Key.Split('.').Last();
                    var unionParameter = unionSchema.Schema.Attributes.Where(f => f.Name.EndsWith(key)).FirstOrDefault();

                    var newSearchParam = String.Empty;
                    if (unionParameter != null) {
                        newSearchParam = unionParameter.Name + "_union";
                    } else {
                        newSearchParam = "null" + nullParamCounter++;
                    }

                    var idxToReplace = sb.ToString().IndexOf(parameter.Key, sbReplacingIdx, StringComparison.Ordinal);
                    sb.Replace(parameter.Key, newSearchParam, idxToReplace, parameter.Key.Length);
                    sbReplacingIdx += newSearchParam.Length;
                }

                unionSearchRequestDto.SearchParams = sb.ToString();
                unionSearchRequestDto.SearchValues = originalSearchRequestDto.SearchValues;
            }
            return unionSearchRequestDto;
        }

        public SearchRequestDto AppendSearchEntry(string searchParam, string searchValue) {
            AppendSearchParam(searchParam);
            AppendSearchValue(searchValue);
            return this;
        }

        public SearchRequestDto AppendSearchEntry(string searchParam, params string[] searchValue) {
            if (searchValue != null) {
                AppendSearchParam(searchParam);
                AppendSearchValue(string.Join(",", searchValue));
            }
            return this;
        }

        public SearchRequestDto AppendSearchEntry(string searchParam, IEnumerable<string> searchValue) {
            return AppendSearchEntry(searchParam, searchValue.ToArray());
        }

        public void AppendSearchParam(string searchParam) {
            if (!String.IsNullOrWhiteSpace(SearchParams)) {
                SearchParams += SearchUtils.SearchParamAndSeparator;
            }
            SearchParams += searchParam;
        }

        public void AppendSearchValue(string searchValue) {
            if (!String.IsNullOrWhiteSpace(SearchValues)) {
                SearchValues += SearchUtils.SearchValueSeparator;
            }
            SearchValues += searchValue;
        }

        public void AppendWhereClause(string toAppendwhereclause) {
            if (string.IsNullOrEmpty(toAppendwhereclause)) {
                return;
            }
            if (!string.IsNullOrEmpty(WhereClause) && (!toAppendwhereclause.StartsWith("and") && !toAppendwhereclause.StartsWith("AND"))) {
                toAppendwhereclause = " AND " + toAppendwhereclause;
            }
            WhereClause += toAppendwhereclause;
        }

        public void AppendWhereClauseFormat(string whereclause, params object[] parameters) {
            whereclause = String.Format(whereclause, parameters);
            AppendWhereClause(whereclause);
        }

        public void AppendProjectionField(ProjectionField projectionField) {

            _projectionFields.Add(projectionField);
        }

        public ISet<ProjectionField> ProjectionFields {
            get { return _projectionFields; }
        }

        [JsonIgnore]
        public IDictionary<string, SearchParameter> ValuesDictionary {
            get { return _valuesDictionary ?? (_valuesDictionary = GetParameters()); }
        }

        public IDictionary<String, SearchParameter> GetParameters() {
            if (_valuesDictionary != null) {
                //caching for further calls
                return _valuesDictionary;
            }

            if (String.IsNullOrEmpty(SearchParams)) {
                return null;
            }
            _valuesDictionary = new Dictionary<string, SearchParameter>();

            var parameters = Regex.Split(SearchParams, SearchUtils.SearchParamSpliter).Where(f => !String.IsNullOrWhiteSpace(f)).ToList();
            SearchUtils.ValidateString(SearchValues);

            //wacky separator to avoid false positives
            var values = Regex.Split(SearchValues, SearchUtils.SearchValueSeparator);
            if (parameters.Count() != values.Count()) {
                throw new ArgumentException("parameters and values must have the same count for a given search");
            }


            for (var i = 0; i < parameters.Count(); i++) {
                var paramName = parameters[i];

                if (String.IsNullOrEmpty(paramName)) {
                    continue;
                }
                _valuesDictionary[paramName] = new SearchParameter(values[i]);
            }
            return _valuesDictionary;
        }

        public void SetFromSearchString(ApplicationSchemaDefinition appSchema, IList<String> searchFields, String searchText) {

            var sbParams = new StringBuilder();
            var sbValues = new StringBuilder();

            //First, verify if searchText is datetime...
            var param = new SearchParameter(searchText);
            var fieldsToSearch =
                appSchema.Fields.Where(f => param.IsDate ? f.RendererType == "datetime" : f.RendererType != "datetime");
            fieldsToSearch = fieldsToSearch.Where(f => searchFields.Contains(f.Attribute));
            var applicationFieldDefinitions = fieldsToSearch as ApplicationFieldDefinition[] ?? fieldsToSearch.ToArray();

            if (!applicationFieldDefinitions.Any()) {
                return;
            }

            foreach (var field in applicationFieldDefinitions) {
                sbParams.Append(field.Attribute);
                sbParams.Append(SearchUtils.SearchParamOrSeparator);

                sbValues.Append(searchText);
                sbValues.Append(SearchUtils.SearchValueSeparator);
            }
            sbParams.Remove(sbParams.Length - 3, 3);
            sbValues.Remove(sbValues.Length - 3, 3);

            SearchParams = sbParams.ToString();
            SearchValues = sbValues.ToString();
        }

        public void BuildFixedWhereClause(string entityName) {
            FilterFixedWhereClause = SearchUtils.GetWhereReplacingParameters(this, entityName);
        }

        public List<string> GetNestedFieldsToConsiderInRelationships {
            get {
                var resultFields = new List<string>();

                foreach (var projectionField in ProjectionFields) {
                    //lets add all the projection fields, so that the presence of a single projection 
                    //avoids all the relationships to be fetched
                    resultFields.Add(projectionField.Name);
                }

                var parameters = GetParameters();
                if (parameters == null) {
                    return resultFields;
                }

                foreach (var searchParameter in parameters) {
                    if (searchParameter.Key.Contains(".")) {
                        resultFields.Add(searchParameter.Key);
                    }
                }

                return resultFields;

            }
        }




    }
}
