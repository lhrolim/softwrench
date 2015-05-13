using System;
using System.Linq;
using cts.commons.persistence;
using cts.commons.simpleinjector;
using Newtonsoft.Json.Linq;
using softWrench.sW4.Data.API;
using softWrench.sW4.Data.API.Response;
using softWrench.sW4.Data.Pagination;
using softWrench.sW4.Data.Persistence.Operation;
using softWrench.sW4.Data.Persistence.WS.API;
using softWrench.sW4.Data.Relationship.Composition;
using softWrench.sW4.Metadata.Security;
using softwrench.sW4.Shared2.Data;
using softwrench.sW4.Shared2.Metadata.Applications.Schema;
using softWrench.sW4.Data;
using softWrench.sW4.Data.Persistence;
using softWrench.sW4.Data.Persistence.Dataset.Commons;
using softWrench.sW4.Data.Persistence.Engine;
using softWrench.sW4.Security.Entities;
using softWrench.sW4.Security.Services;

namespace softWrench.sW4.Metadata.Applications.DataSet {
    public class BaseUserDataSet : BaseApplicationDataSet {
        private IConnectorEngine _connectorEngine;
        private IConnectorEngine _maxEngine;
        private SecurityFacade _securityFacade;
        private ISWDBHibernateDAO _dao;

        // SW connector engine not implemented
        // TODO: Implement the softwrench connector engine and other dependencies
        protected override IConnectorEngine Engine() {
            if (_connectorEngine == null) {
                _connectorEngine = SimpleInjectorGenericFactory.Instance.GetObject<SWDBConnectorEngine>(typeof(SWDBConnectorEngine));
            }
            return _connectorEngine;
        }

        protected ISWDBHibernateDAO SWDAO()
        {
            if (_dao == null)
            {
                _dao = SimpleInjectorGenericFactory.Instance.GetObject<ISWDBHibernateDAO>(typeof (ISWDBHibernateDAO));
            }
            return _dao;
        }

        protected IConnectorEngine MaxEngine()
        {
            if (_maxEngine == null) {
                _maxEngine = SimpleInjectorGenericFactory.Instance.GetObject<MaximoConnectorEngine>(typeof(MaximoConnectorEngine));
            }
            return _maxEngine;
        }

        public override ApplicationListResult GetList(ApplicationMetadata application, PaginatedSearchRequestDto searchDto) {

            return base.GetList(application, searchDto);
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
            // remove password data from datamap
            dataMap.SetAttribute("password", "");
            var isactive = (bool)dataMap.GetAttribute("isactive") ? "1" : "0";
            dataMap.SetAttribute("isactive", isactive);
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

            dataMap.SetAttribute("#personid", personDataMap.GetAttribute("personid"));
            dataMap.SetAttribute("#personuid", personDataMap.GetAttribute("personuid"));
            dataMap.SetAttribute("#firstname", personDataMap.GetAttribute("firstname"));
            dataMap.SetAttribute("#lastname", personDataMap.GetAttribute("lastname"));
            dataMap.SetAttribute("#orgid", personDataMap.GetAttribute("locationorg"));
            dataMap.SetAttribute("#siteid", personDataMap.GetAttribute("locationsite"));
            dataMap.SetAttribute("#storeroom", personDataMap.GetAttribute("storeroom"));
            dataMap.SetAttribute("#department", personDataMap.GetAttribute("department"));
            dataMap.SetAttribute("#language", personDataMap.GetAttribute("langcode"));
            dataMap.SetAttribute("#emailid", personDataMap.GetAttribute("email_.emailid"));
            dataMap.SetAttribute("#email", personDataMap.GetAttribute("email_.emailaddress"));
            dataMap.SetAttribute("#phoneid", personDataMap.GetAttribute("phone_.phoneid"));
            dataMap.SetAttribute("#phone", personDataMap.GetAttribute("phone_.phonenum"));

            var associationResults = BuildAssociationOptions(dataMap, application, request);
            var detailResult = new ApplicationDetailResult(dataMap, associationResults, application.Schema, applicationCompositionSchemas, id);
            return detailResult;
        }

        public override TargetResult Execute(ApplicationMetadata application, JObject json, string id, string operation) {
            // Build user json to save from json parameter
            JObject userJson = new JObject();
            userJson.Add("id", id);
            userJson.Add("username", json.Value<string>("username"));
            var isactive = json.Value<string>("isactive").Equals("1") ? true : false;
            userJson.Add("isactive", isactive);
            userJson.Add("password", json.Value<string>("password"));
            userJson.Add("maximoPersonId", json.Value<string>("maximoPersonId"));
            // Handle associated values
            //HandlePerson(json, operation);
            //HandleEmail(json, operation);
            //HandlePhone(json, operation);

            return new TargetResult("", "", SecurityFacade.GetInstance().SaveUser(User.fromJson(userJson)));
        }

        private void HandlePerson(JObject json, string operation)
        {
            const string applicationName = "person";
            var personAppliction = MetadataProvider.Application(applicationName).ApplyPoliciesWeb(new ApplicationMetadataSchemaKey("detail"));
            var entityMetadata = MetadataProvider.Entity(personAppliction.Entity);
            JObject personJson = new JObject();

            personJson.Add("firstname", json.GetValue("#firstname"));
            personJson.Add("lastname", json.GetValue("#lastname"));
            personJson.Add("locationorg", json.GetValue("#orgid"));
            personJson.Add("locationsite", json.GetValue("#siteid"));
            personJson.Add("storeroom", json.GetValue("#storeroom"));
            personJson.Add("department", json.GetValue("#department"));
            personJson.Add("langcode", json.GetValue("#language"));
            personJson.Add("personid", json.GetValue("#personid"));
            personJson.Add("personuid", json.GetValue("#personuid"));

            var id = personJson.Value<string>("personuid");
            var operationWrapper = new OperationWrapper(personAppliction, entityMetadata, operation, personJson, id);
            _maxEngine.Execute(operationWrapper);
        }

        private void HandleEmail(JObject json, string operation) {
            const string applicationName = "email";
            var emailAppliction = MetadataProvider.Application(applicationName).ApplyPoliciesWeb(new ApplicationMetadataSchemaKey("detail"));
            var entityMetadata = MetadataProvider.Entity(emailAppliction.Entity);
            JObject emailJson = new JObject();

            emailJson.Add("emailid", json.GetValue("#emailid"));
            emailJson.Add("emailaddress", json.GetValue("#email"));

            var id = emailJson.Value<string>("emailid");
            var operationWrapper = new OperationWrapper(emailAppliction, entityMetadata, operation, emailJson, id);
            _maxEngine.Execute(operationWrapper);
        }

        private void HandlePhone(JObject json, string operation) {
            const string applicationName = "phone";
            var phoneAppliction = MetadataProvider.Application(applicationName).ApplyPoliciesWeb(new ApplicationMetadataSchemaKey("detail"));
            var entityMetadata = MetadataProvider.Entity(phoneAppliction.Entity);
            JObject phoneJson = new JObject();

            phoneJson.Add("phoneid", json.GetValue("#phoneid"));
            phoneJson.Add("phonenum", json.GetValue("#phone"));

            var id = phoneJson.Value<string>("phoneid");
            var operationWrapper = new OperationWrapper(phoneAppliction, entityMetadata, operation, phoneJson, id);
            _maxEngine.Execute(operationWrapper);
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
