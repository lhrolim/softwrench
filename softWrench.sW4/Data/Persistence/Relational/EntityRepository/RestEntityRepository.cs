using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using cts.commons.portable.Util;
using cts.commons.web.Util;
using JetBrains.Annotations;
using log4net;
using softwrench.sW4.Shared2.Data;
using softWrench.sW4.Data.Pagination;
using softWrench.sW4.Data.Persistence.WS.Rest;
using softWrench.sW4.Data.Search;
using softWrench.sW4.Metadata;
using softWrench.sW4.Metadata.Entities;
using softWrench.sW4.Util;

namespace softWrench.sW4.Data.Persistence.Relational.EntityRepository {

    public class RestEntityRepository : IEntityRepository {

        private readonly RestResponseParser _restResponseParser;

        private readonly ILog _log = LogManager.GetLogger(typeof(RestEntityRepository));

        public string KeyName {
            get; set;
        }


        public RestEntityRepository(RestResponseParser restResponseParser) {
            _restResponseParser = restResponseParser;
            _log.DebugFormat("init");
        }




        public IReadOnlyList<DataMap> Get(EntityMetadata entityMetadata, SearchRequestDto searchDto) {
            //TODO: use async
            var baseURL = BuildGetUrl(entityMetadata, searchDto, KeyName);
            var responseAsText = RestUtil.CallRestApiSync(baseURL, "get", MaximoRestUtils.GetMaximoHeaders(KeyName));
            return _restResponseParser.ConvertXmlToDatamaps(entityMetadata, responseAsText);
        }

        public DataMap Get(EntityMetadata entityMetadata, string id) {
            var baseURL = BuildGetUrl(entityMetadata, new SearchRequestDto() { Id = id }, KeyName);
            var responseAsText = RestUtil.CallRestApiSync(baseURL, "get", MaximoRestUtils.GetMaximoHeaders(KeyName));
            return _restResponseParser.ConvertXmlToDatamap(entityMetadata, responseAsText);
        }

        internal string BuildGetUrl([NotNull]EntityMetadata entityMetadata, [NotNull]SearchRequestDto searchDto, [CanBeNull]string keyname) {
            var id = searchDto.Id;

            var baseURL = new StringBuilder(MaximoRestUtils.GenerateRestUrlForQuery(entityMetadata, id, keyname) + "?");
            baseURL.Append("_urs=true");
            HandleMultiTenantPrefix(entityMetadata, baseURL);

            if (id != null) {
                var builtGetIdUrl = baseURL.ToString();
                _log.DebugOrInfoFormat("url built : {0}", builtGetIdUrl);
                return builtGetIdUrl;
            }
            //limiting the result set to 100 by default, avoiding issues
            var countThreshold = 100;
            if (searchDto is PaginatedSearchRequestDto) {
                var d = (PaginatedSearchRequestDto)searchDto;
                countThreshold = d.PageSize;
            }
            baseURL.AppendFormat("&_maxitems={0}", countThreshold);

            HandleSort(searchDto, baseURL);


            var projectionFields = searchDto.ProjectionFields;
            if (projectionFields.Any()) {
                var includeCols = string.Join(",", projectionFields.Select(p => p.Name).OrderBy(s => s));
                baseURL.AppendFormat("&_includecols={0}", includeCols);
            }

            var searchParameters = searchDto.GetParameters();
            if (searchParameters != null) {
                foreach (var parameter in searchParameters) {
                    var searchParameter = parameter.Value;
                    var searchOperator = searchParameter.SearchOperator;
                    if (searchOperator.Equals(SearchOperator.OR) ||
                        searchOperator.Equals(SearchOperator.ORCONTAINS)) {
                        baseURL.AppendFormat("&{0}.ormode=fake", parameter.Key);
                        var rawValues = (IEnumerable<string>)searchParameter.Value;
                        foreach (var value in rawValues) {
                            baseURL.AppendFormat("&{0}={1}", parameter.Key, WebUtility.UrlEncode(value));
                        }
                        continue;
                    }
                    var valueAsString = HandleValue(searchParameter);
                    baseURL.AppendFormat("&{0}={1}", parameter.Key, GetValueConsideringOperator(searchOperator, valueAsString));

                }
            }
            var builtGetUrl = baseURL.ToString();
            _log.DebugOrInfoFormat("url built : {0}", builtGetUrl);
            return builtGetUrl;
        }

        private static void HandleSort(SearchRequestDto searchDto, StringBuilder baseURL) {
            if (searchDto.SearchSort != null) {
                if (searchDto.SearchAscending) {
                    baseURL.AppendFormat("&_orderbyasc={0}", searchDto.SearchSort);
                } else {
                    baseURL.AppendFormat("&_orderbydesc={0}", searchDto.SearchSort);
                }
            }
        }

        private static void HandleMultiTenantPrefix(EntityMetadata entityMetadata, StringBuilder baseURL) {
            var multiTenantPrefix = MetadataProvider.GlobalProperty(SwConstants.MultiTenantPrefix);
            if (multiTenantPrefix != null) {
                if (entityMetadata.Schema.Attributes.Any(a => a.Name.Equals("pluspcustomer"))) {
                    baseURL.AppendFormat("&pluspcustomer=~eq~{0}", multiTenantPrefix);
                } else if (entityMetadata.Schema.Attributes.Any(a => a.Name.Equals("pluspcustvendor"))) {
                    baseURL.AppendFormat("&pluspcustvendor=~eq~{0}", multiTenantPrefix);
                } else if (entityMetadata.Schema.Attributes.Any(a => a.Name.Equals("pluspinsertcustomer"))) {
                    baseURL.AppendFormat("&pluspinsertcustomer=~eq~{0}", multiTenantPrefix);
                }
            }
        }

        private static string HandleValue(SearchParameter searchParameter) {
            var valueAsString = searchParameter.Value.ToString();

            if (searchParameter.Value is DateTime) {
                valueAsString = ((DateTime)searchParameter.Value).ToString(DateUtil.MaximoDefaultIntegrationFormat);
            } else if (searchParameter.IsDate) {
                valueAsString = searchParameter.GetAsDate.ToString(DateUtil.MaximoDefaultIntegrationFormat);
            }
            return valueAsString;
        }

        private string GetValueConsideringOperator(SearchOperator searchOperator, string value) {
            if (searchOperator.Equals(SearchOperator.EQ)) {
                return "~eq~" + WebUtility.UrlEncode(value);
            }
            if (searchOperator.Equals(SearchOperator.GT)) {
                return "~gt~" + WebUtility.UrlEncode(value);
            }

            if (searchOperator.Equals(SearchOperator.GTE)) {
                return "~gteq~" + WebUtility.UrlEncode(value);
            }
            if (searchOperator.Equals(SearchOperator.LT)) {
                return "~lt~" + WebUtility.UrlEncode(value);
            }
            if (searchOperator.Equals(SearchOperator.LTE)) {
                return "~lteq~" + WebUtility.UrlEncode(value);
            }

            if (searchOperator.Equals(SearchOperator.STARTWITH)) {
                return "~sw~" + WebUtility.UrlEncode(value.Replace("%", ""));
            }

            return WebUtility.UrlEncode(value.Replace("%", ""));
        }









        public int Count(EntityMetadata entityMetadata, SearchRequestDto searchDto) {
            throw new NotImplementedException();
        }

        public AttributeHolder ByUserIdSite(EntityMetadata entityMetadata, Tuple<string, string> userIdSiteTuple) {
            throw new NotImplementedException();
        }
    }
}
