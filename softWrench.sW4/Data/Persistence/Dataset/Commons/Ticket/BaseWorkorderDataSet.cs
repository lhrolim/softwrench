using System.Collections.Generic;
using System.Linq;
using System.Text;
using softwrench.sw4.Shared2.Data.Association;
using softWrench.sW4.Data.Search;
using softWrench.sW4.Metadata.Applications.DataSet;
using softWrench.sW4.Metadata.Applications.DataSet.Filter;

namespace softWrench.sW4.Data.Persistence.Dataset.Commons.Ticket {

    public class BaseWorkorderDataSet : BaseTicketDataSet {


        public SearchRequestDto FilterStatusCodes(AssociationPreFilterFunctionParameters parameters) {
            var filter = parameters.BASEDto;
            filter.AppendWhereClauseFormat("( MAXVALUE != 'HISTEDIT' )");
            return filter;
        }

        /* Need to add this prefilter function for the problem codes !! */
        public SearchRequestDto FilterProblemCodes(AssociationPreFilterFunctionParameters parameters) {
            return ProblemCodeFilterByFailureClassFunction(parameters);
        }

        private SearchRequestDto ProblemCodeFilterByFailureClassFunction(AssociationPreFilterFunctionParameters parameters) {
            var filter = parameters.BASEDto;
            var failurecodeid = parameters.OriginalEntity.GetAttribute("failurelist_.failurelist");
            if (failurecodeid == null) {
                return filter;
            }
            filter.AppendSearchEntry("parent", failurecodeid.ToString());
            return filter;
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

        public SearchRequestDto BuildRelatedAttachmentsWhereClause(CompositionPreFilterFunctionParameters parameter) {
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
            sb.AppendFormat(@" or(ownertable = 'WARRANTYVIEW' and ownerid in (select wocontractid from wocontract where wonum ='{0}' and orgid ='{1}'))",wonum,orgid);

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
