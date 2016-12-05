using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using cts.commons.persistence;
using cts.commons.portable.Util;
using cts.commons.Util;
using Newtonsoft.Json.Linq;
using NHibernate.Util;
using softwrench.sW4.Shared2.Data;
using softwrench.sW4.Shared2.Metadata.Applications.Schema;
using softWrench.sW4.Configuration.Services;
using softWrench.sW4.Data.API;
using softWrench.sW4.Data.API.Composition;
using softWrench.sW4.Data.API.Response;
using softWrench.sW4.Data.Pagination;
using softWrench.sW4.Data.Persistence.Relational.EntityRepository;
using softWrench.sW4.Data.Persistence.WS.API;
using softWrench.sW4.Data.Relationship.Composition;
using softWrench.sW4.Metadata;
using softWrench.sW4.Metadata.Applications;
using softWrench.sW4.Metadata.Security;
using softWrench.sW4.Security.Context;
using softWrench.sW4.Util;
using PropertyDefinition = softWrench.sW4.Configuration.Definitions.PropertyDefinition;
using softWrench.sW4.Configuration.Definitions;
using softWrench.sW4.Exceptions;

namespace softWrench.sW4.Data.Persistence.Dataset.Commons.SWDB {
    public class BasePropertyDefinitionDataSet : SWDBApplicationDataset {

        private readonly ConfigurationService _configService;
        private readonly IContextLookuper _contextLookuper;
        private readonly ISWDBHibernateDAO _dao;

        public BasePropertyDefinitionDataSet(ConfigurationService configService, IContextLookuper contextLookuper,
            ISWDBHibernateDAO dao) {
            _configService = configService;
            _contextLookuper = contextLookuper;
            _dao = dao;
        }

        public override async Task<ApplicationDetailResult> GetApplicationDetail(ApplicationMetadata application, InMemoryUser user,
            DetailRequest request) {
            var id = request.Id;
            DataMap datamap;
            if (id != null) {
                var definition = await _dao.FindSingleByQueryAsync<PropertyDefinition>(PropertyDefinition.ByKey, id);
                datamap = new DataMap(application.Name, ToDictionary(definition));
                return new ApplicationDetailResult(datamap, null, application.Schema, null, id);
            }

            datamap = new DataMap(application.Name, new Dictionary<string, object>());
            return new ApplicationDetailResult(datamap, null, application.Schema,
                CompositionBuilder.InitializeCompositionSchemas(application.Schema, user), null);
        }

        public override async Task<CompositionFetchResult> GetCompositionData(ApplicationMetadata application,
            CompositionFetchRequest request, JObject currentData) {
            var searchDTO = request.PaginatedSearch;
            var compositions = new Dictionary<string, EntityRepository.SearchEntityResult>();

            searchDTO.AppendWhereClauseFormat(
                " (Visible = 1 AND (FullKey like '/Global/%' or FullKey like '/{0}/%')AND (renderer is null or renderer != 'attachment')) ".Fmt(ApplicationConfiguration.ClientName));
            var listApplication = MetadataProvider.Application("_configuration")
                .ApplyPoliciesWeb(new ApplicationMetadataSchemaKey("list"));
            var listResult = await GetList(listApplication, searchDTO);

            var lookupContext = _contextLookuper.LookupContext();
            var resultList = new List<Dictionary<string, object>>();
            listResult.ResultObject.ForEach(async ah => await AddToResultList(resultList, ah, lookupContext));

            var searchResult = new EntityRepository.SearchEntityResult {
                ResultList = resultList,
                PaginationData = new PaginatedSearchRequestDto(listResult.TotalCount, listResult.PageNumber, listResult.PageSize, null, listResult.PaginationOptions)
            };
            compositions.Add("#properties_", searchResult);
            return new CompositionFetchResult(compositions, null);
        }

        private async Task AddToResultList(ICollection<Dictionary<string, object>> resultList, AttributeHolder ah, ContextHolder lookupContext) {
            var dict = (Dictionary<string, object>)ah;
            object key;
            if (!dict.TryGetValue("FullKey", out key)) {
                key = dict["fullkey"];
            }
            dict["currentvalue"] = await _configService.Lookup<string>((string)key, lookupContext);
            resultList.Add(dict);
        }

        public override async Task<TargetResult> Execute(ApplicationMetadata application, JObject json, OperationDataRequest operationData) {
            var compositionData = operationData.CompositionData;
            var id = compositionData.Id;

            var updatedConfig = json
                .Value<JArray>("#properties_")
                .First(t => id.Equals(t.Value<string>("fullkey")) && t.Value<string>("#edited") != null);

            var newValue = updatedConfig.Value<string>("value");
            var dataType = updatedConfig.Value<string>("datatype");
            var minvalue = updatedConfig.Value<long?>("minvalue");
            var maxvalue = updatedConfig.Value<long?>("maxvalue");

            Validate(dataType, newValue, minvalue, maxvalue);
            await _configService.UpdateGlobalDefinition(id, newValue);

            return new TargetResult(id, null, null, "Configuration " + id + " successfully updated.");
        }

        private void Validate(string dataType, string newValue, long? minvalue, long? maxvalue) {
            if (string.IsNullOrWhiteSpace(newValue)) {
                return;
            }

            PropertyDataType propDatatype;

            if (Enum.TryParse(dataType, true, out propDatatype) && ValueValidForDatatype(propDatatype, newValue)) {
                if ((propDatatype.Equals(PropertyDataType.INT) ||
                    propDatatype.Equals(PropertyDataType.LONG)) && !ValueWithinLimit(newValue, minvalue, maxvalue))
                {
                    string errorMessage = null;
                    if (minvalue != null && maxvalue != null) {
                        errorMessage =
                        $"Invalid configuration value. Value should be greater than {minvalue} and less than {maxvalue}";
                    }
                    else if (minvalue != null) {
                        errorMessage = $"Invalid configuration value. Value should be smaller than {minvalue}";
                    }
                    if (maxvalue != null) {
                        errorMessage = $"Invalid configuration value. Value should be greater than {minvalue}";
                    }

                    throw new Exception(errorMessage);
                }
            } else {
                var errorMessage = $"Invalid or empty configuration value. The field has the Datatype : {dataType}";
                throw new Exception(errorMessage);
            }
        }

        /// <summary>
        /// Validates if the value is within the min and max limit
        /// </summary>
        /// <param name="newValue">The value</param>
        /// <param name="minvalue">The mimimum value limit</param>
        /// <param name="maxvalue">The maximum value limit</param>
        private bool ValueWithinLimit(string newValue, long? minvalue, long? maxvalue) {
            long outVar;
            if (!long.TryParse(newValue, out outVar)) {
                return false;
            }
            if (minvalue != null && maxvalue != null) {
                return minvalue <= outVar && outVar <= maxvalue;
            }
            if (minvalue != null) {
                return minvalue <= outVar;
            }
            return outVar <= maxvalue;
        }

        /// <summary>
        /// Validates if the value is valid for the set DataType
        /// </summary>
        /// <param name="propDatatype">The datatype <see cref="PropertyDataType"/>of the configuration </param>
        /// <param name="newValue">The supplied value</param>
        private bool ValueValidForDatatype(PropertyDataType propDatatype, string newValue) {
            if (propDatatype.Equals(PropertyDataType.BOOLEAN)) {
                bool outVar;
                return Boolean.TryParse(newValue, out outVar);
            } else if (propDatatype.Equals(PropertyDataType.INT)) {
                int outVar;
                return int.TryParse(newValue, out outVar);
            } else if (propDatatype.Equals(PropertyDataType.LONG)) {
                long outVar;
                return long.TryParse(newValue, out outVar);
            } else if (propDatatype.Equals(PropertyDataType.STRING)) {
                return true;
            } else if (propDatatype.Equals(PropertyDataType.DATE)) {
                DateTime outVar;
                return DateTime.TryParse(newValue, out outVar);
            }

            return false;
        }

        private IDictionary<string, object> ToDictionary(PropertyDefinition definition) {
            var dict = new Dictionary<string, object>();
            dict["fullkey"] = definition.FullKey;
            dict["description"] = definition.Description;
            dict["simplekey"] = definition.SimpleKey;
            dict["datatype"] = definition.DataType;
            dict["renderer"] = definition.Renderer;
            dict["defaultvalue"] = definition.DefaultValue;
            dict["stringvalue"] = definition.StringValue;
            dict["minvalue"] = definition.MinValue_;
            dict["maxvalue"] = definition.MaxValue_;
            dict["value"] = _configService.GetGlobalPropertyValue<string>(definition);
            return dict;
        }

        public override string ApplicationName() {
            return "_configuration";
        }

        public override string ClientFilter() {
            return null;
        }
    }
}
