using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using cts.commons.persistence;
using cts.commons.simpleinjector;
using Newtonsoft.Json.Linq;
using softWrench.sW4.Data.API;
using softWrench.sW4.Data.API.Association;
using softWrench.sW4.Data.API.Response;
using softWrench.sW4.Data.Pagination;
using softWrench.sW4.Data.Persistence.Operation;
using softWrench.sW4.Data.Persistence.WS.API;
using softWrench.sW4.Data.Relationship.Composition;
using softWrench.sW4.Metadata.Security;
using softWrench.sW4.Metadata.Stereotypes.Schema;
using softwrench.sW4.Shared2.Data;
using softwrench.sW4.Shared2.Metadata.Applications;
using softwrench.sW4.Shared2.Metadata.Applications.Relationships.Compositions;
using softwrench.sW4.Shared2.Metadata.Applications.Schema;
using softWrench.sW4.Data;
using softWrench.sW4.Data.Persistence;
using softWrench.sW4.Data.Persistence.Dataset.Commons;
using softWrench.sW4.Data.Persistence.Engine;
using softWrench.sW4.Data.Search;
using softWrench.sW4.Util;

namespace softWrench.sW4.Metadata.Applications.DataSet {
    public class BaseUserDataSet : BaseApplicationDataSet {
        private IConnectorEngine _connectorEngine;
        private IConnectorEngine _maxEngine;

        private ISWDBHibernateDAO _dao;

        protected override IConnectorEngine Engine() {
            if (_connectorEngine == null) {
                _connectorEngine = SimpleInjectorGenericFactory.Instance.GetObject<SWDBConnectorEngine>(typeof(SWDBConnectorEngine));
            }
            return _connectorEngine;
        }

        protected IConnectorEngine MaxEngine()
        {
            if (_maxEngine == null) {
                _maxEngine = SimpleInjectorGenericFactory.Instance.GetObject<MaximoConnectorEngine>(typeof(MaximoConnectorEngine));
            }
            return _maxEngine;
        }

        public override ApplicationDetailResult GetApplicationDetail(ApplicationMetadata application, InMemoryUser user, DetailRequest request) {
            var id = request.Id;
            var entityMetadata = MetadataProvider.SlicedEntityMetadata(application);
            var applicationCompositionSchemas = CompositionBuilder.InitializeCompositionSchemas(application.Schema);
            DataMap dataMap;
            if (id != null) {
                dataMap = (DataMap)Engine().FindById(application.Schema, entityMetadata, id, applicationCompositionSchemas);
                if (request.InitialValues != null) {
                    var initValDataMap = DefaultValuesBuilder.BuildDefaultValuesDataMap(application,
                        request.InitialValues, entityMetadata.Schema.MappingType);
                    dataMap = DefaultValuesBuilder.AddMissingInitialValues(dataMap, initValDataMap);
                }
            } else {
                dataMap = DefaultValuesBuilder.BuildDefaultValuesDataMap(application, request.InitialValues, entityMetadata.Schema.MappingType);
            }

            object maximopersonid;
            dataMap.Fields.TryGetValue("maximopersonid", out maximopersonid); 

            var personApplictionMetadata = MetadataProvider.Application("person").ApplyPoliciesWeb(new ApplicationMetadataSchemaKey("detail"));
            var personEntityMetadata = MetadataProvider.SlicedEntityMetadata(personApplictionMetadata);
            applicationCompositionSchemas = CompositionBuilder.InitializeCompositionSchemas(application.Schema);
            // Build a search dto to get the person by its id not uid
            var searchDto = new PaginatedSearchRequestDto();
            searchDto.AppendSearchParam("personid");
            searchDto.AppendSearchValue(maximopersonid.ToString());
            AttributeHolder[] persons = MaxEngine().Find(personEntityMetadata, searchDto, applicationCompositionSchemas).ToArray();
            DataMap personDataMap = (DataMap)persons[0];

            dataMap.SetAttribute("#firstName", personDataMap.GetAttribute("firstName"));
            dataMap.SetAttribute("#lastName", personDataMap.GetAttribute("lastName"));
            dataMap.SetAttribute("#orgId", personDataMap.GetAttribute("locationorg"));
            dataMap.SetAttribute("#siteId", personDataMap.GetAttribute("locationsite"));
            dataMap.SetAttribute("#storeroom", personDataMap.GetAttribute("storeroom"));
            dataMap.SetAttribute("#email", personDataMap.GetAttribute("email"));
            dataMap.SetAttribute("#department", personDataMap.GetAttribute("department"));
            dataMap.SetAttribute("#phone", personDataMap.GetAttribute("phone"));
            dataMap.SetAttribute("#language", personDataMap.GetAttribute("langcode"));

            var associationResults = BuildAssociationOptions(dataMap, application, request);
            var detailResult = new ApplicationDetailResult(dataMap, associationResults, application.Schema, applicationCompositionSchemas, id);
            return detailResult;
        }

        public override TargetResult Execute(ApplicationMetadata application, JObject json, string id, string operation) {
            var entityMetadata = MetadataProvider.Entity(application.Entity);
            var operationWrapper = new OperationWrapper(application, entityMetadata, operation, json, id);
            return Engine().Execute(operationWrapper);
        }

        public override string ApplicationName() {
            return "_User";
        }

        public override string ClientFilter()
        {
            return null;
        }
    }
}
