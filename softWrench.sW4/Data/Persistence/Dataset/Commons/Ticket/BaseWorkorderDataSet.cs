using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using cts.commons.persistence;
using cts.commons.portable.Util;
using Newtonsoft.Json.Linq;
using softwrench.sw4.Shared2.Data.Association;
using softwrench.sW4.Shared2.Data;
using softWrench.sW4.Data.API;
using softWrench.sW4.Data.API.Composition;
using softWrench.sW4.Data.API.Response;
using softWrench.sW4.Data.Entities;
using softWrench.sW4.Data.Persistence.Dataset.Commons.Ticket.Commlog;
using softWrench.sW4.Data.Search;
using softWrench.sW4.Metadata.Applications;
using softWrench.sW4.Metadata.Applications.DataSet;
using softWrench.sW4.Metadata.Applications.DataSet.Filter;
using softWrench.sW4.Metadata.Security;
using softWrench.sW4.Security.Services;

namespace softWrench.sW4.Data.Persistence.Dataset.Commons.Ticket {

    public class BaseWorkorderDataSet : BaseTicketDataSet {
        private readonly ISWDBHibernateDAO _swdbDao;

        public BaseWorkorderDataSet(ISWDBHibernateDAO swdbDao) {
            _swdbDao = swdbDao;
        }

        public SearchRequestDto FilterStatusCodes(AssociationPreFilterFunctionParameters parameters) {
            var filter = parameters.BASEDto;
            var user = SecurityFacade.CurrentUser();
            var onLaborerGroup = user.Profiles.Any(p => p.Name.EqualsIc("laborers"));

            filter.AppendWhereClauseFormat("( MAXVALUE != 'HISTEDIT' )");
            if (onLaborerGroup) {
                filter.AppendWhereClauseFormat("VALUE not in ('FIELD WORK COMP','Comp')");
            }

            return filter;
        }

        public override async Task<ApplicationDetailResult> GetApplicationDetail(ApplicationMetadata application, InMemoryUser user, DetailRequest request) {
            var baseDetail = await base.GetApplicationDetail(application, user, request);
            if (request.IsEditionRequest) {
                var items = await MaxDAO.FindByNativeQueryAsync("select failurecode,linenum,type from failurereport where wonum = ?",
                    baseDetail.ResultObject["wonum"]);
                foreach (var item in items) {
                    var type = item["type"];
                    var projectedFieldName = "";
                    if (type.EqualsIc("PROBLEM")) {
                        projectedFieldName = "#problemlist_.failurelist";
                    } else if (type.EqualsIc("CAUSE")) {
                        projectedFieldName = "#causelist_.failurelist";
                    } else {
                        //remedy is useless as no one relies on him
                        continue;
                    }
                    baseDetail.ResultObject.SetAttribute(projectedFieldName, item["linenum"]);
                }
            }
            return baseDetail;

        }




        #region Problem, Cause, Remedy Filters
        /* Need to add this prefilter function for the problem codes !! */
        public SearchRequestDto FilterProblemCodes(AssociationPreFilterFunctionParameters parameters) {
            return FilterByFailureClass(parameters, "failurelist_.failurelist");
        }

        /* Need to add this prefilter function for the causes !! */
        public SearchRequestDto FilterCauses(AssociationPreFilterFunctionParameters parameters) {
            return FilterByFailureClass(parameters, "problemlist_.failurelist");
        }

        /* Need to add this prefilter function for the remedies !! */
        public SearchRequestDto FilterRemedies(AssociationPreFilterFunctionParameters parameters) {
            return FilterByFailureClass(parameters, "causelist_.failurelist");
        }

        private SearchRequestDto FilterByFailureClass(AssociationPreFilterFunctionParameters parameters, string parentField, string fallbackField = null) {
            var filter = parameters.BASEDto;
            var primaryAttribute = parameters.Relationship.EntityAssociation.PrimaryAttribute();
            //            if (parameters.OriginalEntity.ContainsAttribute(primaryAttribute.From, true)) {
            //                return filter;
            //            }

            var failurecodeid = FailureCodeId(parameters.OriginalEntity, parentField);
            if (string.IsNullOrEmpty(failurecodeid)) {
                // the code comes as a hidden field when the entity first opens, and later as extrafields, if the parent field is changed on screen
                var originalDataCode = parameters.OriginalEntity.GetStringAttribute(fallbackField ?? parentField);
                if (string.IsNullOrEmpty(originalDataCode)) {
                    filter.ForceEmptyResult = true;
                } else {
                    filter.AppendSearchEntry("parent", originalDataCode);
                }
            } else {
                filter.AppendSearchEntry("parent", failurecodeid);
            }
            return filter;
        }

        private static string FailureCodeId(AttributeHolder entity, string parentField) {
            var extrafields = ((Entity)entity).GetUnMappedAttribute("extrafields");
            if (extrafields == null) return null;
            dynamic fields = JObject.Parse(extrafields);
            return fields[parentField].Value.ToString();
        }
        #endregion Problem, Cause, Remedy Filters

        public override async Task<CompositionFetchResult> GetCompositionData(ApplicationMetadata application, CompositionFetchRequest request,
            JObject currentData) {
            var compList = await base.GetCompositionData(application, request, currentData);
            compList = await CommlogHelper.SetCommlogReadStatus(application, request, compList);
            return compList;
        }

        public IEnumerable<IAssociationOption> GetWOPriorityType(OptionFieldProviderParameters parameters) {
            var query = @"SELECT description AS LABEL,
	                             CAST(value AS INT) AS VALUE 
                          FROM numericdomain
                          WHERE domainid = 'WO PRIORITY'";

            var result = MaxDAO.FindByNativeQuery(query, null);
            var list = new List<AssociationOption>();

            if (result.Any()) {
                foreach (var record in result) {
                    list.Add(new AssociationOption(record["VALUE"].ToString(), string.Format("{0} - {1}", record["VALUE"], record["LABEL"])));
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

        public IEnumerable<IAssociationOption> GetWOClassStructureType(OptionFieldProviderParameters parameters) {
            return GetClassStructureType(parameters, "WORKORDER");
        }

        public IEnumerable<IAssociationOption> GetWOClassStructureTypeDescription(OptionFieldProviderParameters parameters) {
            return GetClassStructureTypeDescription(parameters, "WORKORDER");
        }

        public SearchRequestDto BuildRelatedAttachmentsWhereClause(CompositionPreFilterFunctionParameters parameter) {

            var appSchema = parameter.Schema.AppSchema;
            if ("true".EqualsIc(appSchema.GetProperty("attachments.skiprelatedattachments"))) {
                //let´s use ordinary query
                return parameter.BASEDto;
            }


            var originalEntity = parameter.OriginalEntity;
            var siteId = originalEntity.GetAttribute("siteid");
            var orgid = originalEntity.GetAttribute("orgid");
            var workorderid = originalEntity.GetAttribute("workorderid");
            var woclass = originalEntity.GetAttribute("woclass");
            var wonum = originalEntity.GetAttribute("wonum");
            var assetnum = originalEntity.GetAttribute("assetnum");
            var location = originalEntity.GetAttribute("location");
            var jpNum = originalEntity.GetAttribute("jpnum");
            var pmNum = originalEntity.GetAttribute("pmnum");
            var cinum = originalEntity.GetAttribute("cinum");
            var failureCode = originalEntity.GetAttribute("failurecode");

            var sb = new StringBuilder();
            //base section
            sb.AppendFormat(
                @"(ownertable = 'WORKORDER' and ownerid ='{0}') or(ownertable = 'WORKORDER' and ownerid in (select workorderid from workorder
                        where parent ='{1}' and istask = 1 and siteid ='{2}'))", workorderid, wonum, siteId);

            sb.AppendFormat(
                @"or (ownertable='WORKLOG' and ownerid in (select worklogid from worklog where recordkey='{0}' and class='{1}'))",
                wonum, woclass);

            if (assetnum != null) {
                sb.AppendFormat(
                    @" or(ownertable = 'ASSET' and ownerid in (select assetuid from asset where assetnum ='{0}' and siteid ='{1}'))",
                    assetnum, siteId);
            }

            if (location != null) {
                sb.AppendFormat(@" or (ownertable = 'LOCATIONS' and ownerid in (select locationsid from locations where location ='{0}' and siteid ='{1}'))",
                    location, siteId);
            }

            if (jpNum != null) {
                sb.AppendFormat(@" or (ownertable = 'JOBPLAN' and ownerid in (select jobplanid from jobplan where jpnum ='{0}' and (siteid is null or siteid = '{1}')) )",
                    jpNum, siteId);
            }

            if (pmNum != null) {
                sb.AppendFormat(@" or(ownertable = 'PM' and ownerid in 
                        (select pmuid from pm where pmnum ='{0}' and siteid ='{1}'))",
                    pmNum, siteId);
            }

            if (cinum != null) {
                sb.AppendFormat(@" or(ownertable = 'CI' and ownerid in (select ciid from ci where cinum='{0}' and assetlocsiteid ='{1}'))",
                    cinum, siteId);
            }

            if (failureCode != null) {
                sb.AppendFormat(@" or(ownertable = 'FAILURELIST' and ownerid in (select failurelist from failurelist where failurecode='{0}' ))",
                    failureCode);
            }

            //CONTRACTS
            sb.AppendFormat(@" or(ownertable = 'WARRANTYVIEW' and ownerid in (select wocontractid from wocontract where wonum ='{0}' and orgid ='{1}'))", wonum, orgid);

            sb.AppendFormat(@" or(ownertable = 'WOSERVICEADDRESS' and ownerid in (select woserviceaddressid from woserviceaddress where wonum ='{0}' and siteid = '{1}'))", wonum, siteId);


            sb.AppendFormat(@" or(ownertable = 'SAFETYPLAN' and ownerid in 
                        (select safetyplanuid from safetyplan, wosafetyplan where safetyplan.safetyplanid = wosafetyplan.safetyplanid 
                        and wosafetyplan.wonum ={0} and wosafetyplan.siteid ={1}))
                        or(ownertable in ('SR', 'INCIDENT', 'PROBLEM') and 
                            ownerid in (select ticketuid from ticket,relatedrecord where ticketid = recordkey and ticket.class = relatedrecord.class 
                                and relatedrecclass={2} and relatedreckey={0} and relatedrecsiteid={1}))
                        or(ownertable in ('WOCHANGE','WORELEASE','WOACTIVITY') 
                        and ownerid in (select workorderid from workorder, relatedrecord where wonum = recordkey and workorder.woclass = relatedrecord.class 
                        and relatedrecclass={2} and relatedreckey={0} and relatedrecsiteid={1}))
                        or(ownertable= 'COMMLOG' and ownerid in (select commloguid from 
                            commlog where ownerid ={3} and ownertable in (select value from synonymdomain where domainid = 'WOCLASS')))
                        or (ownertable= 'SLA' and ownerid in (select slaid from sla, slarecords, workorder where sla.slanum= slarecords.slanum and 
                            slarecords.ownerid= workorder.workorderid and sla.objectname= 'WORKORDER' and slarecords.ownertable= 'WORKORDER' and workorder.wonum={0}))
", "'" + wonum + "'", "'" + siteId + "'", "'" + woclass + "'", "'" + workorderid + "'");

            parameter.BASEDto.SearchValues = null;
            parameter.BASEDto.SearchParams = null;
            parameter.BASEDto.AppendWhereClause(sb.ToString());

            return parameter.BASEDto;
        }


        public override string ApplicationName() {
            return "workorder";
        }

        public override string ClientFilter() {
            return null;
        }
    }
}
