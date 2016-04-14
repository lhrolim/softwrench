using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using cts.commons.persistence;
using cts.commons.portable.Util;
using Newtonsoft.Json.Linq;
using softwrench.sw4.Shared2.Data.Association;
using softwrench.sW4.Shared2.Metadata.Applications;
using softwrench.sW4.Shared2.Metadata.Applications.Schema;
using softWrench.sW4.Data.API.Composition;
using softWrench.sW4.Data.Entities;
using softWrench.sW4.Data.Persistence.Dataset.Commons.Ticket.Commlog;
using softWrench.sW4.Data.Persistence.Engine;
using softWrench.sW4.Data.Persistence.Operation;
using softWrench.sW4.Data.Persistence.SWDB;
using softWrench.sW4.Data.Persistence.WS.API;
using softWrench.sW4.Data.Search;
using softWrench.sW4.Metadata;
using softWrench.sW4.Metadata.Applications;
using softWrench.sW4.Metadata.Applications.DataSet;
using softWrench.sW4.Metadata.Applications.DataSet.Filter;
using softWrench.sW4.Security.Services;
using softWrench.sW4.Util;

namespace softWrench.sW4.Data.Persistence.Dataset.Commons.Ticket {
    public class BaseServiceRequestDataSet : BaseTicketDataSet {
        private readonly ISWDBHibernateDAO _swdbDao;
        /* Need to add this prefilter function for the problem codes !! 
        public SearchRequestDto FilterProblemCodes(AssociationPreFilterFunctionParameters parameters)
        {

        }*/

        public BaseServiceRequestDataSet(ISWDBHibernateDAO swdbDao) {
            _swdbDao = swdbDao;
        }

        public override CompositionFetchResult GetCompositionData(ApplicationMetadata application, CompositionFetchRequest request,
            JObject currentData) {
            var compList = base.GetCompositionData(application, request, currentData);
            compList = CommlogHelper.SetCommlogReadStatus(application, request, compList);
            return compList;
        }

        public SearchRequestDto BuildRelatedAttachmentsWhereClause(CompositionPreFilterFunctionParameters parameter) {

            var appSchema = parameter.Schema.AppSchema;
            if ("true".EqualsIc(appSchema.GetProperty("attachments.skiprelatedattachments"))) {
                //let´s use ordinary query
                return parameter.BASEDto;
            }

            var originalEntity = parameter.OriginalEntity;

            var origrecordid = parameter.BASEDto.ValuesDictionary["ownerid"].Value;
            var origrecordclass = parameter.BASEDto.ValuesDictionary["ownertable"].Value as string;

            var ticketuid = originalEntity.GetAttribute("ticketuid");


            var siteId = originalEntity.GetAttribute("siteid");

            var assetnum = originalEntity.GetAttribute("assetnum");
            var location = originalEntity.GetAttribute("location");
            var solution = originalEntity.GetAttribute("solution");
            var cinum = originalEntity.GetAttribute("cinum");
            var failureCode = originalEntity.GetAttribute("failurecode");

            var sb = new StringBuilder();
            //base section
            sb.AppendFormat(
                @"(ownertable = 'SR' and ownerid = {0})
                or (  ownertable='PROBLEM' and ownerid in (select ticketuid from problem where ticketid={1} and class={2} )) 
                or (  ownertable='INCIDENT' and ownerid in (select ticketuid from incident where ticketid={1} and class={2}))
                or (  ownertable='WOCHANGE' and ownerid in (select workorderid from wochange where wonum={1} and woclass={2}))
                or (  ownertable='WORELEASE' and ownerid in (select workorderid from worelease where wonum={1} and woclass={2}))
                or (ownertable='WOACTIVITY' and ownerid in (select workorderid from woactivity where origrecordid={1} and origrecordclass={2}))
                or (ownertable='JOBPLAN' and ownerid in (select jobplanid from jobplan where jpnum in (select jpnum from woactivity where origrecordid={1} and origrecordclass={2})))
            ", "'" + ticketuid + "'", "'" + origrecordid + "'", "'" + origrecordclass + "'");

            if (assetnum != null) {
                sb.AppendFormat(
                    @" or(ownertable = 'ASSET' and ownerid in (select assetuid from asset where assetnum ='{0}' and siteid ='{1}'))",
                    assetnum, siteId);
            }

            if (location != null) {
                sb.AppendFormat(@" or (ownertable = 'LOCATIONS' and ownerid in (select locationsid from locations where location ='{0}' and siteid ='{1}'))",
                    location, siteId);
            }

            if (solution != null) {
                sb.AppendFormat(@" or (ownertable='SOLUTION' and ownerid in (select solutionid from solution where solution='{0}'))", solution);
            }

            if (cinum != null) {
                sb.AppendFormat(@" or(ownertable = 'CI' and ownerid in (select ciid from ci where cinum='{0}' and assetlocsiteid ='{1}'))",
                    cinum, siteId);
            }

            if (failureCode != null) {
                sb.AppendFormat(@" or(ownertable = 'FAILURELIST' and ownerid in (select failurelist from failurelist where failurecode='{0}' ))",
                    failureCode);
            }

            sb.AppendFormat(@" or(ownertable = 'TKSERVICEADDRESS' and ownerid in (select tkserviceaddressid from TKSERVICEADDRESS where ticketid ='{0}' and siteid = '{1}' and class = 'SR'))", origrecordid, siteId);

            sb.AppendFormat(
                @" or (ownertable='COMMLOG' and ownerid in (select commloguid from commlog where ownertable='SR' and ownerid='{0}')) ", ticketuid);

            parameter.BASEDto.SearchValues = null;
            parameter.BASEDto.SearchParams = null;
            parameter.BASEDto.AppendWhereClause(sb.ToString());

            return parameter.BASEDto;
        }

        public override TargetResult Execute(ApplicationMetadata application, JObject json, string id, string operation, bool isBatch, Tuple<string, string> userIdSite) {
            if (!string.Equals(operation, OperationConstants.CRUD_CREATE) || isBatch) {
                return base.Execute(application, json, id, operation, isBatch, userIdSite);
            }

            var relatedOriginId = json.StringValue("#relatedrecord_recordkey");

            return string.IsNullOrEmpty(relatedOriginId)
                // regular CREATE 
                ? base.Execute(application, json, id, operation, isBatch, userIdSite) 
                // CREATE as relatedrecord
                : CreateAsRelated(application, json, relatedOriginId);
        }

        private TargetResult CreateAsRelated(ApplicationMetadata application, JObject srToCreate, string relatedSrTicketId) {
            // regular crudoperationdata building
            var entityMetadata = MetadataProvider.Entity(application.Entity);
            var operationData = EntityBuilder.BuildFromJson<CrudOperationData>(typeof (CrudOperationData), entityMetadata, application, srToCreate);
            // adding related record data
            operationData.SetAttribute("origrecordid", relatedSrTicketId);
            operationData.SetAttribute("origrecordclass", "SR");
            // creating SR with related record data -> WS will automatically create the 'relatedrecord' entries
            return (TargetResult)((MaximoConnectorEngine)Engine()).Create(operationData);
        }

        public IEnumerable<IAssociationOption> GetSRPriorityType(OptionFieldProviderParameters parameters)
        {
            var query = @"SELECT description AS LABEL,
	                             CAST(value AS INT) AS VALUE 
                          FROM numericdomain
                          WHERE domainid = 'TICKETPRIORITY'";

            var result = MaxDAO.FindByNativeQuery(query, null);
            var list = new List<AssociationOption>();

            if (result.Any())
            {
                foreach (var record in result)
                {
                    list.Add(new AssociationOption(record["VALUE"].ToString(), string.Format("{0} - {1}", record["VALUE"], record["LABEL"])));
                }
            }
            else {
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

        public IEnumerable<IAssociationOption> GetSRClassStructureTypeDescription(OptionFieldProviderParameters parameters)
        {
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