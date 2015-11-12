using System.Collections.Concurrent;
using Newtonsoft.Json;
using softWrench.sW4.Data.Pagination;
using softWrench.sW4.Metadata;
using softWrench.sW4.Metadata.Entities;
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

namespace softWrench.sW4.Data.Search {
    public class SearchRequestDto : IDataRequest {

        private ApplicationLookupContext _context;

        public IDictionary<string, string> CustomParameters { get; set; }
        public string CommandId { get; set; }

        public String SearchParams { get; set; }

        public String SearchValues { get; set; }

        public String SearchSort { get; set; }

        public Boolean ExpressionSort { get; set; }

        public Boolean SearchAscending { get; set; }

        private ISet<ProjectionField> _projectionFields = new HashSet<ProjectionField>();


        public string ExtraLeftJoinSection { get; set; }

        public String WhereClause { get; set; }

        public Boolean IgnoreWhereClause { get; set; }

        public ApplicationLookupContext Context { get; set; }

        public Rowstamps Rowstamps { get; set; }

        public string Id { get; set; }

        private IDictionary<string, SearchParameter> _valuesDictionary;

        public ApplicationMetadataSchemaKey Key { get; set; }

        public String Title { get; set; }

        public SearchRequestDto unionDTO;

        /// <summary>
        /// For every item in this array, the same base projection query (i.e except for its whereclause) will be replicated having the array element as the whereclause of the union query
        /// </summary>
        [CanBeNull]
        public List<string> UnionWhereClauses { get; set; }


        //used to indentify the query on the log better
        private string _queryAlias;

        public String QueryAlias {
            get {
                if (Context != null && Context.MetadataId != null) {
                    return Context.MetadataId;
                }
                return _queryAlias;
            }
            set { _queryAlias = value; }
        }

        public virtual bool ShouldPaginate {
            get { return false; }
            set { }
        }

        public string FilterFixedWhereClause { get; set; }
        public string UnionFilterFixedWhereClause { get; set; }



        internal void BuildProjection(ApplicationSchemaDefinition schema) {
            var columns = schema.Fields;
            foreach (var column in columns) {
                AppendProjectionField(new ProjectionField { Name = column.Attribute, Alias = column.Attribute });
            }
        }

        internal void BuildProjection(SlicedEntityMetadata metadata) {
            var columns = metadata.Attributes(EntityMetadata.AttributesMode.NoCollections);
            foreach (var column in columns) {
                AppendProjectionField(new ProjectionField { Name = column.Name, Alias = column.Name });
            }
        }


        internal void BuildProjection(ApplicationCompositionSchema schema, bool printMode = false) {
            var schemaToUse = printMode ? schema.Schemas.Print : schema.Schemas.List;
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

        public void BuildUnionDTO(SearchRequestDto searchDto, ApplicationSchemaDefinition originalSchema) {
            var unionSchema = MetadataProvider.FindSchemaDefinition(originalSchema.UnionSchema);
            unionDTO = new SearchRequestDto();
            unionDTO.BuildProjection(unionSchema);
            unionDTO.FilterFixedWhereClause = searchDto.UnionFilterFixedWhereClause;
            if (String.IsNullOrWhiteSpace(SearchParams)) {
                return;
            }

            var sb = new StringBuilder(SearchParams);
            var sbReplacingIdx = 0;
            var nullParamCounter = 0;

            var searchParameters = GetParameters();
            var parameters = Regex.Split(SearchParams, SearchUtils.SearchParamSpliter).Where(f => !String.IsNullOrWhiteSpace(f));

            foreach (var parameterSt in parameters) {

                var parameter = searchParameters[parameterSt];

                var key = parameterSt.Split('.').Last();

                var fields = originalSchema.Fields;
                var idx = 0;

                ApplicationFieldDefinition unionParameter = null;
                foreach (var field in fields) {
                    // need to recover field from the original schema, since on the client side it will pass this as the parameter name.
                    // the union schema has to be declared on the same order, but could have different field aliases
                    if (field.Attribute.EndsWith(key)) {
                        unionParameter = unionSchema.Fields[idx];
                        break;
                    }
                    idx++;
                }

                string newSearchParam;
                if (unionParameter != null) {
                    newSearchParam = unionParameter.Attribute + "_union";
                } else {
                    newSearchParam = "null" + nullParamCounter++;
                }

                var idxToReplace = sb.ToString().IndexOf(parameterSt, sbReplacingIdx, StringComparison.Ordinal);
                sb.Replace(parameterSt, newSearchParam, idxToReplace, parameterSt.Length);
                sbReplacingIdx += newSearchParam.Length;
            }

            unionDTO.SearchParams = sb.ToString();
            unionDTO.SearchValues = SearchValues;

            HandleUnionZeroedValueCases(unionDTO);

        }

        private void HandleUnionZeroedValueCases(SearchRequestDto unionDto) {
            unionDto.GetParameters();
            foreach (var entry in unionDto.ValuesDictionary) {
                if (entry.Key.Contains("zeroed")) {
                    //HAP-1026 difficult bug to solve generically --> if attribute is zeroed, like will fix the issue and keep it working.
                    entry.Value.Value = "%" + entry.Value.Value + "%";
                    entry.Value.SearchOperator = SearchOperator.CONTAINS;
                }
            }

        }


        public SearchRequestDto AppendSearchEntry(string searchParam, string searchValue, bool orMode=false) {
            AppendSearchParam(searchParam, orMode);
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

        public SearchRequestDto AppendSearchEntry(string searchParam, List<string> searchValue) {
            return AppendSearchEntry(searchParam, searchValue.ToArray());
        }

        public void AppendSearchParam(string searchParam, bool orMode = false) {
            if (!String.IsNullOrWhiteSpace(SearchParams)) {
                if (orMode) {
                    SearchParams += SearchUtils.SearchParamOrSeparator;
                } else {
                    SearchParams += SearchUtils.SearchParamAndSeparator;
                }

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
            get {
                return _valuesDictionary ?? (_valuesDictionary = GetParameters());
            }
        }

        public IDictionary<String, SearchParameter> GetParameters() {
            if (_valuesDictionary != null) {
                //caching for further calls
                return _valuesDictionary;
            }

            if (String.IsNullOrEmpty(SearchParams)) {
                return null;
            }
            _valuesDictionary = new ConcurrentDictionary<string, SearchParameter>();

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
                if (_valuesDictionary.ContainsKey(paramName)) {
                    //this can happen only if we already have a parameter declared with the same name (ex: a report)
                    _valuesDictionary[i + paramName] = new SearchParameter(values[i]);
                } else {
                    _valuesDictionary[paramName] = new SearchParameter(values[i]);
                }
            }
            //rebuilding search parameters cause some of the names might have been modified in the process due to duplication
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

        public void BuildFixedWhereClause(SearchRequestDto requestDTO, string entityName) {
            FilterFixedWhereClause = SearchUtils.GetWhereReplacingParameters(this, entityName);
            if (requestDTO.unionDTO != null) {
                //TODO: make this more generic, but right now only one union so why bother?
                UnionFilterFixedWhereClause = SearchUtils.GetWhereReplacingParameters(requestDTO.unionDTO, "srforchange");
            }

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

        public virtual SearchRequestDto ShallowCopy() {
            return (SearchRequestDto)MemberwiseClone();
        }


    }
}
