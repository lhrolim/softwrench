using Newtonsoft.Json;
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
using JetBrains.Annotations;
using NHibernate.Util;
using softWrench.sW4.Data.Search.QuickSearch;

namespace softWrench.sW4.Data.Search {
    public class SearchRequestDto : IDataRequest {

        private QuickSearchDTO _quickSearchDTO;


        public IDictionary<string, string> CustomParameters {
            get; set;
        }
        public string CommandId {
            get; set;
        }

        /// <summary>
        /// Receives the query to apply in the format :a || :b || :c || :d. 
        /// FWK shall locate corresponding SearchValues to replace it
        /// </summary>
        public string SearchTemplate {
            get; set;
        }

        public string SearchParams {
            get; set;
        }

        public string SearchValues {
            get; set;
        }

        public string SearchSort {
            get; set;
        }

        public bool ExpressionSort {
            get; set;
        }

        public bool SearchAscending {
            get; set;
        }

        private readonly ISet<ProjectionField> _projectionFields = new HashSet<ProjectionField>();

        public string WhereClause {
            get; set;
        }

        public bool IgnoreWhereClause {
            get; set;
        }

        public ApplicationLookupContext Context {
            get; set;
        }

        public Rowstamps Rowstamps {
            get; set;
        }

        public string Id {
            get; set;
        }

        [CanBeNull]
        public QuickSearchDTO QuickSearchDTO {
            get {
                return _quickSearchDTO;
            }
            set {
                if (value == null || value.QuickSearchData == null) {
                    //assuring that if no string was passed, we do not consider the DTO
                    this._quickSearchDTO = null;
                } else {
                    this._quickSearchDTO = value;
                }

            }
        }


        private IDictionary<string, SearchParameter> _valuesDictionary;

        public ApplicationMetadataSchemaKey Key {
            get; set;
        }

        public string Title {
            get; set;
        }

        /// <summary>
        /// Similar to an ordinary whereclause, but this acts statefully on the client-side so that the future screen iteractions (while within the same schema) would still pass that very same whereclause.
        /// </summary>
        public string FilterFixedWhereClause {
            get; set;
        }

        public bool IsDefaultInstance {
            get; set;
        }

        public bool AddPreSelectedFilters {
            get; set;
        }

        [CanBeNull]
        public List<string> UnionWhereClauses {
            get; set;
        }

        //used to indentify the query on the log better
        private string _queryAlias;

        public string QueryAlias {
            get {
                if (Context != null && Context.MetadataId != null) {
                    return Context.MetadataId;
                }
                return _queryAlias;
            }
            set {
                _queryAlias = value;
            }
        }


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

        public static SearchRequestDto GetFromDictionary(IDictionary<string, string> searchDictionary) {
            var searchRequestDto = new SearchRequestDto();

            foreach (var key in searchDictionary.Keys) {
                searchRequestDto.AppendSearchParam(key);
                searchRequestDto.AppendSearchValue(searchDictionary[key]);
            }
            return searchRequestDto;
        }

        public static SearchRequestDto GetUnionSearchRequestDto(SearchRequestDto originalSearchRequestDto, SlicedEntityMetadata unionSchema) {
            var unionSearchRequestDto = new SearchRequestDto();

            if (!string.IsNullOrWhiteSpace(originalSearchRequestDto.SearchParams)) {

                var sb = new StringBuilder(originalSearchRequestDto.SearchParams);
                var sbReplacingIdx = 0;
                var nullParamCounter = 0;

                foreach (var parameter in originalSearchRequestDto.GetParameters()) {

                    var key = parameter.Key.Split('.').Last();
                    var unionParameter = unionSchema.Schema.Attributes.Where(f => f.Name.EndsWith(key)).FirstOrDefault();

                    var newSearchParam = string.Empty;
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

        public SearchRequestDto AppendSearchEntry(string searchParam, string searchValue, bool allowNull = false) {
            AppendSearchParam(searchParam);
            if (!allowNull) {
                AppendSearchValue(searchValue);
            } else {
                AppendSearchValue(SearchUtils.NullOrPrefix + searchValue);
            }
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
            if (!string.IsNullOrWhiteSpace(SearchParams)) {
                SearchParams += SearchUtils.SearchParamAndSeparator;
            }
            SearchParams += searchParam;
        }

        public void AppendSearchValue(string searchValue, bool allownulls = false) {
            if (!string.IsNullOrWhiteSpace(SearchValues)) {
                SearchValues += SearchUtils.SearchValueSeparator;
            }
            if (!allownulls) {
                SearchValues += searchValue;
            } else {
                SearchValues += SearchUtils.NullOrPrefix + searchValue;
            }
            _valuesDictionary = null; // cache is not valid anymore
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

        [StringFormatMethod("whereclause")]
        public void AppendWhereClauseFormat(string whereclause, params object[] parameters) {
            whereclause = string.Format(whereclause, parameters);
            AppendWhereClause(whereclause);
        }

        public void AppendProjectionField(ProjectionField projectionField) {
            _projectionFields.Add(projectionField);
        }

        public void AppendProjectionFields(params string[] fields) {
            foreach (var field in fields) {
                AppendProjectionField(ProjectionField.Default(field));
            }
        }

        public ISet<ProjectionField> ProjectionFields {
            get {
                return _projectionFields;
            }
        }

        [JsonIgnore]
        public IDictionary<string, SearchParameter> ValuesDictionary {
            get {
                return _valuesDictionary ?? (_valuesDictionary = GetParameters());
            }
        }

        public SearchParameter GetSearchParameter(string param) {
            return !_valuesDictionary.ContainsKey(param) ? null : _valuesDictionary[param];
        }

        public SearchParameter RemoveSearchParam(string toRemove) {
            if (!_valuesDictionary.ContainsKey(toRemove)) {
                return null;
            }
            var originalParam = _valuesDictionary[toRemove];
            originalParam.IgnoreParameter = true;
            return originalParam;
        }

        [CanBeNull]
        public IDictionary<string, SearchParameter> GetParameters() {
            if (_valuesDictionary != null) {
                //caching for further calls
                return _valuesDictionary;
            }

            if (string.IsNullOrEmpty(SearchParams)) {
                return null;
            }
            _valuesDictionary = new LinkedHashMap<string, SearchParameter>();

            var parameters = Regex.Split(SearchParams, SearchUtils.SearchParamSpliter).Where(f => !string.IsNullOrWhiteSpace(f)).ToList();
            SearchUtils.ValidateString(SearchValues);

            //wacky separator to avoid false positives
            var values = Regex.Split(SearchValues, SearchUtils.SearchValueSeparator);
            if (parameters.Count > values.Length) {
                throw new ArgumentException("parameters and values must have the same count for a given search");
            }


            for (var i = 0; i < parameters.Count(); i++) {
                var paramName = parameters[i];

                if (string.IsNullOrEmpty(paramName)) {
                    continue;
                }
                String rawValue = values[i];
                if (rawValue.Contains("__")) {
                    //between case
                    var splittedInterval = Regex.Split(rawValue, "__");
                    _valuesDictionary[paramName + "_begin"] = new SearchParameter(">=" + splittedInterval[0]);
                    _valuesDictionary[paramName + "_end"] = new SearchParameter("<=" + splittedInterval[1]);
                } else {
                    _valuesDictionary[paramName] = new SearchParameter(rawValue);
                }


            }
            return _valuesDictionary;
        }

        public void SetFromSearchString(ApplicationSchemaDefinition appSchema, IList<string> searchFields, string searchText) {

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

        public string ExtraLeftJoinSection {
            get; set;
        }

        public virtual SearchRequestDto ShallowCopy() {
            return (SearchRequestDto)MemberwiseClone();
        }

        /// <summary>
        /// True: query should not be made returning a ampty result list instead and count = 0
        /// False: default. Normal behavior
        /// </summary>
        public bool ForceEmptyResult {
            get; set;
        }

    }
}
