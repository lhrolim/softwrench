using cts.commons.persistence.Util;
using cts.commons.portable.Util;
using softwrench.sW4.Shared2.Data;
using softwrench.sW4.Shared2.Metadata.Entity.Association;
using softWrench.sW4.Data.Persistence;
using softWrench.sW4.Data.Persistence.Relational.QueryBuilder;
using softWrench.sW4.Metadata;
using softWrench.sW4.Metadata.Entities;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using softWrench.sW4.Security.Services;
using softWrench.sW4.Util;

namespace softWrench.sW4.Data.Search {
    public class SearchUtils : IWhereBuilder {

        private readonly SearchRequestDto _searchDTO;
        private readonly string _entityName;
        private readonly string _tableName;

        private static List<SearchParameterUtils> _searchParameterUtilsList;

        public const string SearchValueSeparator = ",,,";

        public const string NullOrPrefix = "nullor:";

        public const string SearchParamAndSeparator = "&&";

        public const string SearchParamOrSeparator = "||,";

        public const string SearchParamSpliter = @"[^\w.]";

        public const string DateSearchParamBegin = "_begin";

        public const string DateSearchParamEnd = "_end";

        public const string SearchBetweenSeparator = "___";

        public SearchUtils(SearchRequestDto searchDto, string entityName, string tableName) {
            _searchDTO = searchDto;
            _entityName = entityName;
            _tableName = tableName;

        }

        public SearchUtils(SearchRequestDto searchDto, string entityName, string tableName, List<SearchParameterUtils> searchParameterUtilsList) {
            _searchDTO = searchDto;
            _entityName = entityName;
            _searchParameterUtilsList = searchParameterUtilsList;
            _tableName = tableName;
        }

        public static void ValidateString(string jsString) {
            if (jsString.Contains("--") || jsString.Contains(";")) {
                throw new ArgumentException("this query could lead to sql injection. Aborting operation");
            }
        }

        public string BuildWhereClause(string entityName, SearchRequestDto searchDto) {
            return BuildWhereClause(entityName);
        }

        public string BuildWhereClause(string entityName) {
            return GetWhere(_searchDTO, _tableName, _entityName);
        }

        public IDictionary<string, object> GetParameters() {
            return GetParameters(_searchDTO);
        }

        public static string GetWhereReplacingParameters(SearchRequestDto dto, string entityName) {
            var where = GetWhere(dto, entityName);
            var parameters = GetParameters(dto);
            foreach (var parameter in parameters) {
                @where = @where.Replace(":" + parameter.Key, "'" + (string)parameter.Value + "'");
            }
            return @where;
        }

        public static string GetWhere(SearchRequestDto listDto, string tableName, string entityName = null) {
            if (entityName == null) {
                entityName = tableName;
            }

            if (string.IsNullOrEmpty(listDto.WhereClause) && string.IsNullOrEmpty(listDto.SearchParams)) {
                return null;
            }

            var sb = new StringBuilder();

            if (!string.IsNullOrEmpty(listDto.SearchParams)) {
                sb.Append(HandleSearchParams(listDto, entityName));
            }

            if (!string.IsNullOrEmpty(listDto.WhereClause)) {
                if (sb.Length > 0) {
                    sb.Append(" AND ");
                }

                sb.Append(listDto.WhereClause);
            }

            return sb.ToString();
        }

        private static string HandleSearchParams(SearchRequestDto listDto, string entityName) {


            //            var parameters = Regex.Split(listDto.SearchParams, SearchParamSpliter).Where(f => !string.IsNullOrWhiteSpace(f));
            var searchParameters = listDto.GetParameters();

            if (searchParameters == null) {
                return "";
            }


            var sbReplacingIdx = 0;
            var sb = new StringBuilder(BuildSearchTemplate(listDto, searchParameters));

            foreach (var searchParameterEntry in searchParameters) {

                var searchParameter = searchParameterEntry.Value;
                var param = searchParameterEntry.Key;

                var statement = new StringBuilder();

                //                var searchParameter = searchParameters[param];
                if (searchParameter.IgnoreParameter) {
                    //this search parameter needs to be ignored
                    continue;
                }

                var parameterData = GetParameterData(entityName, searchParameter, param);
                searchParameter.IsNumber = parameterData.Item2 == ParameterType.Number;
                var operatorPrefix = searchParameter.SearchOperator.OperatorPrefix();

                if (searchParameter.SearchOperator == SearchOperator.BETWEEN) {
                    statement.Append("( " + parameterData.Item1 + " >= :" + param + "_start" + "&&" + parameterData.Item1 + " <= :" + param + "_end" + ")");

                } else if (searchParameter.SearchOperator == SearchOperator.ORCONTAINS) {

                    var values = ((IEnumerable)searchParameter.Value).Cast<string>().ToList();
                    statement.Append("( ");

                    foreach (string value in values) {
                        statement.Append(parameterData.Item1);
                        // this next line would be the ideal, but it will be complicade passing this parameters to BaseHibernateDAO. 
                        //statement.Append(GetDefaultParam(operatorPrefix, param + i)); 
                        // TODO: refactor later
                        statement.Append(operatorPrefix + "'%" + value + "%'");
                        statement.Append(" OR ");
                    }
                    statement.Remove(statement.Length - 4, 4); // remove the last " OR "
                    statement.Append(" )");
                } else if (searchParameter.IsBlankNumber || searchParameter.IsBlankDate) {
                    //https://controltechnologysolutions.atlassian.net/browse/YGSI-15
                    statement.Append("( " + parameterData.Item1 + " IS NULL )");
                } else {
                    statement.Append("( " + parameterData.Item1);

                    if (searchParameter.IsList) {
                        statement.Append(operatorPrefix).Append(string.Format(HibernateUtil.ListParameterPrefixPattern, param));
                    } else if (searchParameter.IsDate || parameterData.Item2 == ParameterType.Date) {
                        statement.Append(HandleDateAttribute(param, searchParameter, operatorPrefix));
                    } else {
                        statement.Append(GetDefaultParam(operatorPrefix, param));
                    }

                    if (searchParameter.NullOr || (searchParameter.SearchOperator == SearchOperator.NOTEQ) || (searchParameter.SearchOperator == SearchOperator.NCONTAINS) || (searchParameter.SearchOperator == SearchOperator.BLANK)) {
                        statement.Append(" OR " + parameterData.Item3 + " IS NULL " + " )");
                    } else {
                        statement.Append(" )");
                    }
                }
                var idxToReplace = sb.ToString().IndexOf(param, sbReplacingIdx, StringComparison.Ordinal);
                sb.Replace(param, statement.ToString(), idxToReplace, param.Length);
                sbReplacingIdx += statement.ToString().Length;

            }
            sb.Replace("&&", " AND ");
            sb.Replace("||,", " OR ");
            sb.Replace("||", " OR ");
            return sb.ToString();
        }

        private static string BuildSearchTemplate(SearchRequestDto listDto, IDictionary<string, SearchParameter> searchParameters) {
            var sb = new StringBuilder();
            var searchTemplate = listDto.SearchTemplate;
            if (searchTemplate != null) {
                sb.Append(listDto.SearchTemplate);
                return sb.ToString();
            }
            var logicOperator = "&&";
            if (listDto.SearchParams.Contains(SearchParamOrSeparator)) {
                //legacy code, keep hapag funcionality up
                logicOperator = SearchParamOrSeparator;
            }
            foreach (var param in searchParameters) {
                if (!param.Value.IgnoreParameter) {
                    sb.Append(param.Key).Append(logicOperator);
                }
            }
            return sb.Length == 0 ? sb.ToString() : sb.ToString(0, sb.Length - logicOperator.Length);
        }

        public static IDictionary<string, object> GetParameters(SearchRequestDto listDto) {
            IDictionary<string, object> resultDictionary = new Dictionary<string, object>();
            // quicksearch statement parameter 
            if (QuickSearchHelper.HasQuickSearchData(listDto)) {
                resultDictionary[QuickSearchHelper.QuickSearchParamName] = QuickSearchHelper.QuickSearchDataValue(listDto.QuickSearchData);
            }
            // filter parameters
            var searchParameters = listDto.GetParameters();
            if (searchParameters == null) return resultDictionary;
            foreach (var searchParameter in searchParameters) {
                var parameter = searchParameter.Value;
                if (parameter.IsBlankNumber || parameter.IsBlankDate) {
                    //this will reflect in only a ISNULL comparison
                    continue;
                }

                if (parameter.IsDate) {
                    var dt = parameter.GetAsDate;
                    HandleDateParameter(parameter, resultDictionary, searchParameter, dt);
                } else if (parameter.IsNumber && (parameter.Value is string)) {
                    try {
                        var int32 = Convert.ToInt32(parameter.Value);
                        resultDictionary.Add(searchParameter.Key, int32);
                    } catch {
                        //its declared as a number, but the client passed a string like %10%, for contains, or even SR123 
                        resultDictionary.Add(searchParameter.Key, parameter.Value);
                    }
                } else if (parameter.Value != null && parameter.Value.ToString().StartsWith("@")) {
                    resultDictionary.Add(searchParameter.Key,
                        DefaultValuesBuilder.GetDefaultValue(parameter.Value.ToString(), null, DefaultValuesBuilder.DBDateTimeFormat));
                } else {
                    resultDictionary.Add(searchParameter.Key, parameter.Value);
                }
            }
            return resultDictionary;
        }

        private static void HandleDateParameter(SearchParameter parameter, IDictionary<string, object> resultDictionary,
          KeyValuePair<string, SearchParameter> searchParameter, DateTime dt) {
            var paramName = searchParameter.Key;
            //if it was a between operation, the parameters might have already been set named correctly at GetParameters.GetParameters
            var beginParam = paramName.EndsWith("_begin") ? paramName : paramName + DateSearchParamBegin;
            var endParam = paramName.EndsWith("_end") ? paramName : paramName + DateSearchParamEnd;

            if (parameter.IsEqualOrNotEqual()) {
                if (!parameter.HasHour) {
                    //this shall create a between interval
                    resultDictionary.Add(beginParam, DateUtil.BeginOfDay(dt));
                    resultDictionary.Add(endParam, DateUtil.EndOfDay(dt));
                } else {
                    //EQ 16:46 should become BETWEEN 16:46:00 and 16:46:59.999
                    resultDictionary.Add(beginParam, dt);
                    resultDictionary.Add(endParam, dt.AddSeconds(59).AddMilliseconds(999));

                    //resultDictionary.Add(searchParameter.Key, dt);
                }
            } else if (parameter.IsGtOrGte()) {
                if (!parameter.HasHour) {
                    if (parameter.SearchOperator == SearchOperator.GT) {
                        //if GT, then we need to exclude the current day from the search
                        dt = dt.AddDays(1);
                    }
                    resultDictionary.Add(beginParam, DateUtil.BeginOfDay(dt));
                } else {
                    if (parameter.SearchOperator == SearchOperator.GT) {
                        //if GT let's add one minute since screen doesn't show seconds --> so GT > 16:36 becomes actually GT > 16:36:59.999
                        dt = dt.AddSeconds(59).AddMilliseconds(999);
                    }
                    //if GTE: GTE>= 16:36 keep it as it is
                    resultDictionary.Add(beginParam, dt.FromUserToMaximo(SecurityFacade.CurrentUser()));
                }
            } else if (parameter.IsLtOrLte()) {
                if (!parameter.HasHour) {
                    if (parameter.SearchOperator == SearchOperator.LT) {
                        //if GT, then we need to exclude the current day from the search, making the beggining of yesterday instead
                        dt = dt.AddDays(-1);
                    }
                    resultDictionary.Add(endParam, DateUtil.EndOfDay(dt));
                } else {
                    dt = dt.AddSeconds(59).AddMilliseconds(999);
                    if (parameter.SearchOperator == SearchOperator.LT) {
                        //if LT let's subtract one minute since screen doesn't show seconds --> LT < 16:36 becomes LT <16:35.59.999
                        dt = dt.AddMinutes(-1);
                    }
                    resultDictionary.Add(endParam, dt.FromUserToMaximo(SecurityFacade.CurrentUser()));
                }
            }
        }


        private static Tuple<string, ParameterType, string> GetParameterData(string entityName, SearchParameter searchParameter, string paramName) {

            // UNION statements cases
            if (paramName.StartsWith("null")) {
                return new Tuple<string, ParameterType, string>("null", ParameterType.Default, paramName);
            }

            var entity = MetadataProvider.Entity(entityName);
            paramName = NormalizeParameterName(paramName);
            var baseResult = paramName.Contains(".") ? paramName : entityName + "." + paramName;
            var attributeDefinition = entity.Attributes(EntityMetadata.AttributesMode.NoCollections).FirstOrDefault(f => f.Name == paramName);
            var resultType = ParameterType.Default;

            if (attributeDefinition != null) {
                if (attributeDefinition.Query != null) {
                    baseResult = attributeDefinition.GetQueryReplacingMarkers(entityName);
                } else if (attributeDefinition.IsDate) {
                    resultType = ParameterType.Date;
                } else if (attributeDefinition.IsNumber) {
                    resultType = ParameterType.Number;
                }
            }
            if (resultType == ParameterType.Date || resultType == ParameterType.Number) {
                return new Tuple<string, ParameterType, string>(baseResult, resultType, baseResult);
            }
            if (searchParameter.FilterSearch) {
                //if this is a filter search input lets make it case insensitive
                return new Tuple<string, ParameterType, string>("UPPER(COALESCE(" + baseResult + ",''))", resultType, baseResult);
            }
            return new Tuple<string, ParameterType, string>(baseResult, resultType, baseResult);
        }

        private static string NormalizeParameterName(string paramName) {
            if (paramName.EndsWith("_union")) {
                paramName = paramName.Substring(0, paramName.Length - "_union".Length);
            }

            if (paramName.EndsWith("_begin")) {
                paramName = paramName.Substring(0, paramName.Length - "_begin".Length);
            }

            if (paramName.EndsWith("_end")) {
                paramName = paramName.Substring(0, paramName.Length - "_end".Length);
            }
            //TODO: remove, this is legacy
            paramName = paramName.Contains("___")
                ? paramName.Split(new[] { "___" }, StringSplitOptions.RemoveEmptyEntries)[0]
                : paramName;
            return paramName;
        }

        enum ParameterType {
            Default, Date, Number
        }

        private static string GetDefaultParam(string operatorPrefix, string param) {
            return operatorPrefix + (string.Format(HibernateUtil.ParameterPrefixPattern, param));
        }


        private static string HandleDateAttribute(string param, SearchParameter searchParameter, string operatorPrefix) {

            var formatedParam = string.Format(HibernateUtil.ParameterPrefixPattern, param);
            if (searchParameter.SearchOperator == SearchOperator.BLANK) {
                return " IS NULL";
            }
            if (!searchParameter.IsEqualOrNotEqual()) {
                var parameterName = param;
                if (searchParameter.SearchOperator == SearchOperator.GTE || searchParameter.SearchOperator == SearchOperator.GT) {
                    if (!parameterName.EndsWith(DateSearchParamBegin)) {
                        parameterName += DateSearchParamBegin;
                    }
                }
                if (searchParameter.SearchOperator == SearchOperator.LTE || searchParameter.SearchOperator == SearchOperator.LT) {
                    if (!parameterName.EndsWith(DateSearchParamEnd)) {
                        parameterName += DateSearchParamEnd;
                    }
                }
                return operatorPrefix + string.Format(HibernateUtil.ParameterPrefixPattern, parameterName);
            }
            var prefix = searchParameter.SearchOperator == SearchOperator.EQ ? " BETWEEN " : " NOT BETWEEN ";
            var sb = new StringBuilder();
            return sb.Append(prefix)
                .Append(formatedParam)
                .Append(DateSearchParamBegin)
                .Append(" AND ")
                .Append(formatedParam)
                .Append(DateSearchParamEnd).ToString();
        }



        public static string GetSearchValue(EntityAssociationAttribute lookupAttribute, AttributeHolder originalEntity) {
            if (lookupAttribute.From != null) {
                var attribute = originalEntity.GetAttribute(lookupAttribute.From);
                return attribute == null ? null : attribute.ToString();
            }
            //quotes do not need to be added at this layer, since they are already being added when evaluating the parameters at the SearchUtils
            return lookupAttribute.Literal;
        }


    }
}
