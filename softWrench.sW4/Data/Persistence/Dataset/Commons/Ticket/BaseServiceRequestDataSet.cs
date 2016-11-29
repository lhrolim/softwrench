using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using cts.commons.portable.Util;
using Newtonsoft.Json.Linq;
using softwrench.sw4.Shared2.Data.Association;
using softWrench.sW4.Data.API.Composition;
using softWrench.sW4.Data.Entities;
using softWrench.sW4.Data.Persistence.Dataset.Commons.Ticket.Commlog;
using softWrench.sW4.Data.Persistence.Engine;
using softWrench.sW4.Data.Persistence.Operation;
using softWrench.sW4.Data.Persistence.WS.API;
using softWrench.sW4.Data.Search;
using softWrench.sW4.Metadata;
using softWrench.sW4.Metadata.Applications;
using softWrench.sW4.Metadata.Applications.DataSet;
using softWrench.sW4.Metadata.Applications.DataSet.Filter;
using softWrench.sW4.Util;

namespace softWrench.sW4.Data.Persistence.Dataset.Commons.Ticket {
    public class BaseServiceRequestDataSet : BaseTicketDataSet {




        public override async Task<CompositionFetchResult> GetCompositionData(ApplicationMetadata application, CompositionFetchRequest request,
            JObject currentData) {
            var compList = await base.GetCompositionData(application, request, currentData);
            compList = await CommlogHelper.SetCommlogReadStatus(application, request, compList);
            return compList;
        }



        public virtual SearchRequestDto BuildRelatedAttachmentsWhereClause(CompositionPreFilterFunctionParameters parameter) {

            var appSchema = parameter.Schema.AppSchema;
            if ("true".EqualsIc(appSchema.GetProperty("attachments.skiprelatedattachments"))) {
                //let´s use ordinary query
                return parameter.BASEDto;
            }

            var originalEntity = parameter.OriginalEntity;
            var whereClause = ServiceRequestWhereClauseProvider.RelatedAttachmentsWhereClause(originalEntity);

            parameter.BASEDto.SearchValues = null;
            parameter.BASEDto.SearchParams = null;
            parameter.BASEDto.AppendWhereClause(whereClause);

            return parameter.BASEDto;
        }

        public override async Task<TargetResult> Execute(ApplicationMetadata application, JObject json, string id, string operation, bool isBatch, Tuple<string, string> userIdSite) {
            if (!string.Equals(operation, OperationConstants.CRUD_CREATE) || isBatch) {
                return await base.Execute(application, json, id, operation, isBatch, userIdSite);
            }

            var relatedOriginId = json.StringValue("#relatedrecord_recordkey");

            return string.IsNullOrEmpty(relatedOriginId)
                // regular CREATE 
                ? await base.Execute(application, json, id, operation, isBatch, userIdSite)
                // CREATE as relatedrecord
                : CreateAsRelated(application, json, relatedOriginId);
        }

        private TargetResult CreateAsRelated(ApplicationMetadata application, JObject srToCreate, string relatedSrTicketId) {
            // regular crudoperationdata building
            var entityMetadata = MetadataProvider.Entity(application.Entity);
            var operationData = EntityBuilder.BuildFromJson<CrudOperationData>(typeof(CrudOperationData), entityMetadata, application, srToCreate);
            // adding related record data
            operationData.SetAttribute("origrecordid", relatedSrTicketId);
            operationData.SetAttribute("origrecordclass", "SR");
            // creating SR with related record data -> WS will automatically create the 'relatedrecord' entries
            return (TargetResult)((MaximoConnectorEngine)Engine()).Create(operationData);
        }

        public IEnumerable<IAssociationOption> GetSRPriorityType(OptionFieldProviderParameters parameters) {
            var query = @"SELECT description AS LABEL,
	                             CAST(value AS INT) AS VALUE 
                          FROM numericdomain
                          WHERE domainid = 'TICKETPRIORITY'";

            var result = MaxDAO.FindByNativeQuery(query, null);
            var list = new List<AssociationOption>();

            if (result.Any()) {
                foreach (var record in result) {
                    list.Add(new AssociationOption(record["VALUE"], string.Format("{0} - {1}", record["VALUE"], record["LABEL"])));
                }
            } else {
                // If no values are found, then default to numeric selection 1-5
                list.Add(new AssociationOption("1", "1"));
                list.Add(new AssociationOption("2", "2"));
                list.Add(new AssociationOption("3", "3"));
                list.Add(new AssociationOption("4", "4"));
                list.Add(new AssociationOption("5", "5"));
            }

            return list;
        }

        public IEnumerable<IAssociationOption> GetSRClassStructureType(OptionFieldProviderParameters parameters) {
            return GetClassStructureType(parameters, "SR");
        }

        public IEnumerable<IAssociationOption> GetSRClassStructureTypeDescription(OptionFieldProviderParameters parameters) {
            return GetClassStructureTypeDescription(parameters, "SR");
        }

        public override string ApplicationName() {
            return "servicerequest,quickservicerequest";
        }

        public override string ClientFilter() {
            return null;
        }
    }
}