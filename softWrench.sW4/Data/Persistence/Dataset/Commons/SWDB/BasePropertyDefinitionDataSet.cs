using System;
using System.Collections.Generic;
using System.Linq;
using cts.commons.persistence;
using Newtonsoft.Json.Linq;
using softWrench.sW4.Configuration.Services;
using softWrench.sW4.Configuration.Services.Api;
using softWrench.sW4.Configuration.Util;
using softWrench.sW4.Data.API;
using softWrench.sW4.Data.API.Response;
using softWrench.sW4.Data.Persistence.SWDB;
using softWrench.sW4.Data.Persistence.WS.API;
using softWrench.sW4.Data.Relationship.Composition;
using softWrench.sW4.Metadata.Applications;
using softWrench.sW4.Metadata.Security;
using softWrench.sW4.Security.Context;
using PropertyDefinition = softWrench.sW4.Configuration.Definitions.PropertyDefinition;

namespace softWrench.sW4.Data.Persistence.Dataset.Commons.SWDB {
    public class BasePropertyDefinitionDataSet : SWDBApplicationDataset {

        private readonly ConfigurationService _configService;
        private readonly IContextLookuper _contextLookuper;
        private readonly ISWDBHibernateDAO _dao;

        public BasePropertyDefinitionDataSet(ConfigurationService configService, IContextLookuper contextLookuper, ISWDBHibernateDAO dao) {
            _configService = configService;
            _contextLookuper = contextLookuper;
            _dao = dao;
        }

        public override ApplicationDetailResult GetApplicationDetail(ApplicationMetadata application, InMemoryUser user, DetailRequest request) {
            var id = request.Id;
            var lookupContext = _contextLookuper.LookupContext();
            DataMap datamap;
            if (id != null) {
                var definition = _dao.FindSingleByQuery<PropertyDefinition>(PropertyDefinition.ByKey, id);
                datamap = new DataMap(application.Name, ToDictionary(definition, lookupContext, true));
                return new ApplicationDetailResult(datamap, null, application.Schema, null, id);
            }

            datamap = new DataMap(application.Name, new Dictionary<string, object>());
            var definitions = _configService.GetAllVisibleDefinitions(ConfigTypes.Global);
            var definitionsMapList = definitions.Select(definition => ToDictionary(definition, lookupContext, false)).Where(dict => !"attachment".Equals(dict["renderer"])).ToList();
            datamap.SetAttribute("fields", new Dictionary<string, object> { { "#properties_", definitionsMapList } });
            return new ApplicationDetailResult(datamap, null, application.Schema, CompositionBuilder.InitializeCompositionSchemas(application.Schema, user), null);
        }

        public override TargetResult Execute(ApplicationMetadata application, JObject json, OperationDataRequest operationData) {
            var compositionData = operationData.CompositionData;
            var id = compositionData.Id;
            var newValue = json
                .Value<JToken>("fields")
                .Value<JArray>("#properties_")
                .First(t => id.Equals(t.Value<string>("fullkey")) && t.Value<string>("#edited") != null)
                .Value<string>("value");
            _configService.UpdateGlobalDefinition(id, newValue);
            return new TargetResult(id, null, null, "Configuration " + id + " successfully updated.");
        }

        private IDictionary<string, object> ToDictionary(PropertyDefinition definition, ContextHolder lookupContext, bool getValueWithoutDefault) {
            var dict = new Dictionary<string, object>();
            dict["fullkey"] = definition.FullKey;
            dict["alternatveFullkey"] = definition.FullKey; // workaround to avoid use id as column on grid
            dict["description"] = definition.Description;
            dict["simplekey"] = definition.SimpleKey;
            dict["datatype"] = definition.DataType;
            dict["renderer"] = definition.Renderer;
            dict["defaultvalue"] = definition.DefaultValue;
            dict["stringvalue"] = definition.StringValue;

            var cacheContext = new ConfigurationCacheContext() {
                IgnoreCache = true,
                PreDefinition = definition
            };

            dict["currentvalue"] = _configService.Lookup<string>(definition.FullKey, lookupContext, cacheContext);
            if (getValueWithoutDefault) {
                dict["value"] = _configService.GetGlobalPropertyValue<string>(definition);
            }
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
