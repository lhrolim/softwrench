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
using System.Text.RegularExpressions;
using softWrench.sW4.Data.Pagination;

namespace softWrench.sW4.Data.Search {
    public class SearchUtils : IWhereBuilder {

        private readonly SearchRequestDto _searchDTO;
        private readonly String _entityName;
        private readonly String _tableName;

        private static List<SearchParameterUtils> _searchParameterUtilsList;

        public const string SearchValueSeparator = ",,,";

        public const string NullOrPrefix = "nullor:";

        public const string SearchParamAndSeparator = "&&";

        public const string SearchParamOrSeparator = "||,";

        public const string SearchParamSpliter = @"[^\w.]";

        public const string DateSearchParamBegin = "_begin";

        public const string DateSearchParamEnd = "_end";

        public const string SearchBetweenSeparator = "___";

        public SearchUtils(SearchRequestDto searchDto, String entityName, string tableName) {
            _searchDTO = searchDto;
            _entityName = entityName;
            _tableName = tableName;

        }

        public SearchUtils(SearchRequestDto searchDto, String entityName, string tableName, List<SearchParameterUtils> searchParameterUtilsList) {
            _searchDTO = searchDto;
            _entityName = entityName;
            _searchParameterUtilsList = searchParameterUtilsList;
            _tableName = tableName;
        }

        public static void ValidateString(String jsString) {
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

        public static string GetWhere(SearchRequestDto listDto, String tableName, String entityName = null) {
            if (entityName == null) {
                entityName = tableName;
            }

            if (String.IsNullOrEmpty(listDto.WhereClause) && String.IsNullOrEmpty(listDto.SearchParams)) {
                return null;
            }

            var sb = new StringBuilder();

            if (!String.IsNullOrEmpty(listDto.SearchParams)) {
                sb.Append(HandleSearchParams(listDto, entityName));
            }

            if (!String.IsNullOrEmpty(listDto.WhereClause)) {
                if (sb.Length > 0) {
                    sb.Append(" AND ");
                }

                sb.Append(listDto.WhereClause);
            }

            return sb.ToString();
        }

        private static string HandleSearchParams(SearchRequestDto listDto, string entityName) {


            var parameters = Regex.Split(listDto.SearchParams, SearchParamSpliter).Where(f => !string.IsNullOrWhiteSpace(f));
            var searchParameters = listDto.GetParameters();

            var sbReplacingIdx = 0;
            var sb = new StringBuilder(BuildSearchTemplate(listDto, searchParameters));

            foreach (var param in parameters) {
                var statement = new StringBuilder();

                var searchParameter = searchParameters[param];
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

                    var values = (searchParameter.Value as IEnumerable).Cast<string>().ToList();
                    if (values != null) {
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
                    }

                } else if (searchParameter.IsBlankNumber || searchParameter.IsBlankDate) {
                    //https://controltechnologysolutions.atlassian.net/browse/YGSI-15
                    statement.Append("( " + parameterData.Item1 + " IS NULL )");
                } else {
                    statement.Append("( " + parameterData.Item1);

                    if (searchParameter.IsList) {
                        statement.Append(operatorPrefix).Append(String.Format(HibernateUtil.ListParameterPrefixPattern, param));
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
                try
                {
                    sb.Replace(param, statement.ToString(), idxToReplace, param.Length);
                    sbReplacingIdx += statement.ToString().Length;
                }
                catch
                {
                    //atest
                    throw;
                }
                
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

                if (parameter.IsDate && !parameter.HasHour) {
                    var dt = parameter.GetAsDate;
                    if (parameter.IsEqualOrNotEqual()) {
                        resultDictionary.Add(searchParameter.Key + DateSearchParamBegin, DateUtil.BeginOfDay(dt));
                        resultDictionary.Add(searchParameter.Key + DateSearchParamEnd, DateUtil.EndOfDay(dt));
                    } else if (parameter.IsGtOrGte()) {
                        //Adding one day in case of Greater Than
                        if (parameter.SearchOperator == SearchOperator.GT) {
                            dt = dt.AddDays(1);
                        }
                        resultDictionary.Add(searchParameter.Key + DateSearchParamBegin, DateUtil.BeginOfDay(dt));
                    } else if (parameter.IsLtOrLte()) {
                        //Removing one day in case of Less than
                        if (parameter.SearchOperator == SearchOperator.LT) {
                            dt = dt.AddDays(-1);
                        }
                        resultDictionary.Add(searchParameter.Key + DateSearchParamEnd, DateUtil.EndOfDay(dt));
                    }
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

        private static Tuple<string, ParameterType,string> GetParameterData(string entityName, SearchParameter searchParameter, string paramName) {

            // UNION statements cases
            if (paramName.StartsWith("null")) {
                return new Tuple<string, ParameterType,string>("null", ParameterType.Default,paramName);
            }
            if (paramName.EndsWith("_union")) {
                paramName = paramName.Substring(0, paramName.Length - "_union".Length);
            }

            var entity = MetadataProvider.Entity(entityName);
            paramName = paramName.Contains("___") ? paramName.Split(new string[] { "___" }, StringSplitOptions.RemoveEmptyEntries)[0] : paramName;
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
                return new Tuple<string, ParameterType,string>(baseResult, resultType, baseResult);
            }
            if (searchParameter.FilterSearch) {
                //if this is a filter search input lets make it case insensitive
                return new Tuple<string, ParameterType,string>("UPPER(COALESCE(" + baseResult + ",''))", resultType,baseResult);
            }
            return new Tuple<string, ParameterType,string>(baseResult, resultType, baseResult);
        }

        enum ParameterType {
            Default, Date, Number
        }

        private static string GetDefaultParam(string operatorPrefix, string param) {
            return operatorPrefix + (String.Format(HibernateUtil.ParameterPrefixPattern, param));
        }


        private static string HandleDateAttribute(string param, SearchParameter searchParameter, string operatorPrefix) {

            var formatedParam = String.Format(HibernateUtil.ParameterPrefixPattern, param);
            if (searchParameter.SearchOperator == SearchOperator.BLANK) {
                return " IS NULL";
            } else if (searchParameter.HasHour || !searchParameter.IsEqualOrNotEqual()) {
                var parameterName = param;
                if (searchParameter.SearchOperator == SearchOperator.GTE || searchParameter.SearchOperator == SearchOperator.GT) {
                    parameterName += DateSearchParamBegin;
                }
                if (searchParameter.SearchOperator == SearchOperator.LTE || searchParameter.SearchOperator == SearchOperator.LT) {
                    parameterName += DateSearchParamEnd;
                }
                return operatorPrefix + String.Format(HibernateUtil.ParameterPrefixPattern, parameterName);
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

            return lookupAttribute.Literal;

        }


    }
}
