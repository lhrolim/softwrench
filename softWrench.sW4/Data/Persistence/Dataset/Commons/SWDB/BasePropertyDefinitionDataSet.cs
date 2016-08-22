using System;
using System.Collections.Generic;
using System.Linq;
using cts.commons.persistence;
using cts.commons.portable.Util;
using Newtonsoft.Json.Linq;
using NHibernate.Linq;
using softwrench.sW4.Shared2.Data;
using softwrench.sW4.Shared2.Metadata.Applications.Schema;
using softWrench.sW4.Configuration.Services;
using softWrench.sW4.Configuration.Services.Api;
using softWrench.sW4.Configuration.Util;
using softWrench.sW4.Data.API;
using softWrench.sW4.Data.API.Composition;
using softWrench.sW4.Data.API.Response;
using softWrench.sW4.Data.Pagination;
using softWrench.sW4.Data.Persistence.Relational.EntityRepository;
using softWrench.sW4.Data.Persistence.SWDB;
using softWrench.sW4.Data.Persistence.WS.API;
using softWrench.sW4.Data.Relationship.Composition;
using softWrench.sW4.Data.Search;
using softWrench.sW4.Metadata;
using softWrench.sW4.Metadata.Applications;
using softWrench.sW4.Metadata.Security;
using softWrench.sW4.Security.Context;
using softWrench.sW4.Util;
using PropertyDefinition = softWrench.sW4.Configuration.Definitions.PropertyDefinition;

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

        public override ApplicationDetailResult GetApplicationDetail(ApplicationMetadata application, InMemoryUser user,
            DetailRequest request) {
            var id = request.Id;
            DataMap datamap;
            if (id != null) {
                var definition = _dao.FindSingleByQuery<PropertyDefinition>(PropertyDefinition.ByKey, id);
                datamap = new DataMap(application.Name, ToDictionary(definition));
                return new ApplicationDetailResult(datamap, null, application.Schema, null, id);
            }

            datamap = new DataMap(application.Name, new Dictionary<string, object>());
            return new ApplicationDetailResult(datamap, null, application.Schema,
                CompositionBuilder.InitializeCompositionSchemas(application.Schema, user), null);
        }

        public override CompositionFetchResult GetCompositionData(ApplicationMetadata application,
            CompositionFetchRequest request, JObject currentData) {
            var searchDTO = request.PaginatedSearch;
            var compositions = new Dictionary<string, EntityRepository.SearchEntityResult>();

            searchDTO.AppendWhereClauseFormat(
                " (Visible = 1 AND (FullKey like '/Global/%' or FullKey like '/{0}/%')AND (renderer is null or renderer != 'attachment')) ".Fmt(ApplicationConfiguration.ClientName));
            var listApplication = MetadataProvider.Application("_configuration")
                .ApplyPoliciesWeb(new ApplicationMetadataSchemaKey("list"));
            var listResult = GetList(listApplication, searchDTO);

            var lookupContext = _contextLookuper.LookupContext();
            var resultList = new List<Dictionary<string, object>>();
            listResult.ResultObject.ForEach(ah => AddToResultList(resultList, ah, lookupContext));

            var searchResult = new EntityRepository.SearchEntityResult {
                ResultList = resultList,
                PaginationData = new PaginatedSearchRequestDto(listResult.TotalCount, listResult.PageNumber, listResult.PageSize, null, listResult.PaginationOptions)
            };
            compositions.Add("#properties_", searchResult);
            return new CompositionFetchResult(compositions, null);
        }

        private void AddToResultList(ICollection<Dictionary<string, object>> resultList, AttributeHolder ah, ContextHolder lookupContext) {
            var dict = (Dictionary<string, object>)ah;
            object key;
            if (!dict.TryGetValue("FullKey", out key)) {
                key = dict["fullkey"];
            }
            dict["currentvalue"] = _configService.Lookup<string>((string)key, lookupContext);
            resultList.Add(dict);
        }

        public override TargetResult Execute(ApplicationMetadata application, JObject json, OperationDataRequest operationData) {
            var compositionData = operationData.CompositionData;
            var id = compositionData.Id;
            var newValue = json
                .Value<JArray>("#properties_")
                .First(t => id.Equals(t.Value<string>("fullkey")) && t.Value<string>("#edited") != null)
                .Value<string>("value");
            _configService.UpdateGlobalDefinition(id, newValue);
            return new TargetResult(id, null, null, "Configuration " + id + " successfully updated.");
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
