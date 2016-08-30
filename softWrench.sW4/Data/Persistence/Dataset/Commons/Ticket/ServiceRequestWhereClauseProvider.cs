using System.Text;
using softwrench.sW4.Shared2.Data;

namespace softWrench.sW4.Data.Persistence.Dataset.Commons.Ticket {
    public static class ServiceRequestWhereClauseProvider {

        /// <summary>
        /// Whereclause to fetch related attachments
        /// </summary>
        /// <param name="originalEntity">parent SR</param>
        /// <param name="includeParentSR">whether or not to include the parent SR's attachments</param>
        /// <returns></returns>
        public static string RelatedAttachmentsWhereClause(AttributeHolder originalEntity, bool? includeParentSR = true) {
            const string origrecordclass = "SR";
            var ticketid = originalEntity.GetAttribute("ticketid");
            var ticketuid = originalEntity.GetAttribute("ticketuid");


            var siteId = originalEntity.GetAttribute("siteid");

            var assetnum = originalEntity.GetAttribute("assetnum");
            var location = originalEntity.GetAttribute("location");
            var solution = originalEntity.GetAttribute("solution");
            var cinum = originalEntity.GetAttribute("cinum");
            var failureCode = originalEntity.GetAttribute("failurecode");

            var sb = new StringBuilder();
            
            // base section
            if (includeParentSR == null || includeParentSR == true) {
                sb.AppendFormat("(ownertable = 'SR' and ownerid = {0}) or", ticketuid);
            }
            sb.AppendFormat(
                @" (  ownertable='PROBLEM' and ownerid in (select ticketuid from problem where ticketid='{0}' and class='{1}' )) 
                or (  ownertable='INCIDENT' and ownerid in (select ticketuid from incident where ticketid='{0}' and class='{1}'))
                or (  ownertable='WOCHANGE' and ownerid in (select workorderid from wochange where wonum='{0}' and woclass='{1}'))
                or (  ownertable='WORELEASE' and ownerid in (select workorderid from worelease where wonum='{0}' and woclass='{1}'))
                or (ownertable='WOACTIVITY' and ownerid in (select workorderid from woactivity where origrecordid='{0}' and origrecordclass='{1}'))
                or (ownertable='JOBPLAN' and ownerid in (select jobplanid from jobplan where jpnum in (select jpnum from woactivity where origrecordid='{0}' and origrecordclass='{1}')))
            ", ticketuid, origrecordclass);

            sb.AppendFormat(
                @"or (ownertable='WORKLOG' and ownerid in (select worklogid from worklog where recordkey='{0}' and class='{1}'))",
                ticketid, origrecordclass);

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

            sb.AppendFormat(@" or(ownertable = 'TKSERVICEADDRESS' and ownerid in (select tkserviceaddressid from TKSERVICEADDRESS where ticketid ='{0}' and siteid = '{1}' and class = 'SR'))", ticketid, siteId);

            sb.AppendFormat(
                @" or (ownertable='COMMLOG' and ownerid in (select commloguid from commlog where ownertable='SR' and ownerid='{0}')) ", ticketuid);

            return sb.ToString();
        }

    }
}