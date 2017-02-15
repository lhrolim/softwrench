using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Threading.Tasks;
using cts.commons.persistence;
using Newtonsoft.Json.Linq;
using softwrench.sw4.Shared2.Data.Association;
using softwrench.sW4.Shared2.Metadata.Applications;
using softWrench.sW4.Configuration.Definitions;
using softWrench.sW4.Configuration.Services;
using softWrench.sW4.Configuration.Services.Api;
using softWrench.sW4.Data.API;
using softWrench.sW4.Data.API.Response;
using softWrench.sW4.Data.Pagination;
using softWrench.sW4.Data.Persistence.Operation;
using softWrench.sW4.Data.Persistence.WS.API;
using softWrench.sW4.Metadata;
using softWrench.sW4.Metadata.Applications;
using softWrench.sW4.Metadata.Applications.DataSet;
using softWrench.sW4.Metadata.Security;
using softWrench.sW4.Security.Context;
using softWrench.sW4.Security.Services;

namespace softWrench.sW4.Data.Persistence.Dataset.Commons.SWDB.WhereClause {
    public class BaseWhereClauseDataSet : SWDBApplicationDataset {

        private readonly ConfigurationService _configService;
        private readonly IContextLookuper _contextLookuper;
        private readonly ISWDBHibernateDAO _dao;

        [Import]
        public WhereClauseRegisterService WhereClauseRegisterService {
            get; set;
        }

        public BaseWhereClauseDataSet(ConfigurationService configService, IContextLookuper contextLookuper,
            ISWDBHibernateDAO dao) {
            _configService = configService;
            _contextLookuper = contextLookuper;
            _dao = dao;
        }

        public override async Task<ApplicationListResult> GetList(ApplicationMetadata application, PaginatedSearchRequestDto searchDto) {
            var originalResult = await base.GetList(application, searchDto);
            var originalList = originalResult.ResultObject;
            foreach (var datamap in originalList) {
                datamap.SetAttribute("#application", datamap.GetStringAttribute("definition_id").Split('/')[2]);
            }
            return originalResult;
        }

        public override async Task<ApplicationDetailResult> GetApplicationDetail(ApplicationMetadata application, InMemoryUser user, DetailRequest request) {

            var result = await base.GetApplicationDetail(application, user, request);
            if (!request.IsEditionRequest) {
                return result;
            }
            var datamap = result.ResultObject;
            datamap.SetAttribute("#application", datamap.GetStringAttribute("definition_id").Split('/')[2]);
            if (datamap.GetStringAttribute("value") == null) {
                datamap.SetAttribute("#value", datamap.GetStringAttribute("systemvalue"));
            } else {
                datamap.SetAttribute("#value", datamap.GetStringAttribute("value"));
            }

            return result;
        }

        public override async Task<TargetResult> Execute(ApplicationMetadata application, JObject json, OperationDataRequest operationData) {
            var entityMetadata = MetadataProvider.Entity(application.Entity);
            var crudoperationData = (CrudOperationData)BuildOperationWrapper(application, json, operationData.Id, operationData.Operation, entityMetadata).OperationData();
            if (operationData.Operation.Equals(OperationConstants.CRUD_DELETE)) {
                await WhereClauseRegisterService.DeleteExisting(int.Parse(operationData.Id));
                return new TargetResult(operationData.Id, operationData.UserId, null);
            }

            var query = crudoperationData.GetStringAttribute("#value");
            var applicationName = crudoperationData.GetStringAttribute("#application");
            var metadataId = crudoperationData.GetStringAttribute("#metadataid");
            var profileId = crudoperationData.GetIntAttribute("userprofile");
            var offline = crudoperationData.GetBooleanAttribute("#offline");
            var schema = crudoperationData.GetStringAttribute("#schema");

            ValidateSchema(schema, offline, applicationName);

            var registerCondition = WhereClauseRegisterCondition.FromDataOrNull(metadataId, profileId, offline, schema);

            WhereClauseRegisterService.ValidateWhereClause(applicationName, query, registerCondition);

            if (operationData.Operation.Equals(OperationConstants.CRUD_UPDATE)) {
                await WhereClauseRegisterService.UpdateExisting(int.Parse(crudoperationData.Id), query);
                return new TargetResult(operationData.Id, operationData.UserId, null);
            }

            var propertyValue = await WhereClauseRegisterService.Create(applicationName, query, registerCondition);

            return new TargetResult(propertyValue.Id.ToString(), null, null);
        }

        private static void ValidateSchema(string schema, bool? offline, string applicationName)
        {
            if (schema != null)
            {
                var platform = (offline.HasValue && offline.Value) ? ClientPlatform.Mobile : ClientPlatform.Web;
                var resultSchema = MetadataProvider.Schema(applicationName, schema, platform);
                if (resultSchema == null)
                {
                    throw new InvalidOperationException($"Schema {schema} not found for application {applicationName}");
                }
            }
        }

        public IEnumerable<IAssociationOption> GetApplications(OptionFieldProviderParameters parameter) {
            var names = MetadataProvider.FetchAvailableAppsAndEntities(false);
            var applications = names.Select(name => new GenericAssociationOption(name, name)).Cast<IAssociationOption>().ToList().OrderBy(a => a.Label);
            return applications;
        }

        public IEnumerable<IAssociationOption> GetProfiles(OptionFieldProviderParameters parameter) {
            return
                SecurityFacade.GetInstance()
                    .FetchAllProfiles(false)
                    .Select(p => new GenericAssociationOption(p.Id.Value.ToString(), p.Description)).Cast<IAssociationOption>().ToList().OrderBy(a => a.Label);
        }


        public override string ApplicationName() {
            return "_whereclause";
        }

        public override string ClientFilter() {
            return null;
        }
    }
}
