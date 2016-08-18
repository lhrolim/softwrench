using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using cts.commons.portable.Util;
using cts.commons.web.Util;
using JetBrains.Annotations;
using softwrench.sW4.Shared2.Data;
using softWrench.sW4.Data.Pagination;
using softWrench.sW4.Data.Persistence.WS.Rest;
using softWrench.sW4.Data.Search;
using softWrench.sW4.Metadata.Entities;

namespace softWrench.sW4.Data.Persistence.Relational.EntityRepository {

    public class RestEntityRepository : IEntityRepository {

        private RestResponseParser _restResponseParser;

        public RestEntityRepository(RestResponseParser restResponseParser) {
            _restResponseParser = restResponseParser;
        }


        protected virtual string KeyName {
            get {
                return null;
            }
        }

        public IReadOnlyList<AttributeHolder> Get(EntityMetadata entityMetadata, SearchRequestDto searchDto) {
            var baseURL = BuildGetUrl(entityMetadata, searchDto, KeyName);
            var responseAsText = RestUtil.CallRestApiSync(baseURL, "get", MaximoRestUtils.GetMaximoHeaders(KeyName));
            return _restResponseParser.ConvertXmlToDatamaps(entityMetadata, responseAsText);
        }

        public AttributeHolder Get(EntityMetadata entityMetadata, string id) {
            var baseURL = BuildGetUrl(entityMetadata, new SearchRequestDto() { Id = id }, KeyName);
            var responseAsText = RestUtil.CallRestApiSync(baseURL, "get", MaximoRestUtils.GetMaximoHeaders(KeyName));
            return _restResponseParser.ConvertXmlToDatamap(entityMetadata, responseAsText);
        }

        internal string BuildGetUrl([NotNull]EntityMetadata entityMetadata, [NotNull]SearchRequestDto searchDto, [CanBeNull]string keyname) {
            var id = searchDto.Id;

            var baseURL = new StringBuilder(MaximoRestUtils.GenerateRestUrlForQuery(entityMetadata, id, keyname) + "?");
            baseURL.Append("_urs=true");

            if (id != null) {
                return baseURL.ToString();
            }
            //limiting the result set to 100 by default, avoiding issues
            var countThreshold = 100;
            if (searchDto is PaginatedSearchRequestDto) {
                var d = (PaginatedSearchRequestDto)searchDto;
                countThreshold = d.PageSize;
            }
            baseURL.AppendFormat("&_maxitems={0}", countThreshold);

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


            return baseURL.ToString();
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
