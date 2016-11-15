using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using log4net;
using softwrench.sw4.Hapag.Data.Configuration;
using softwrench.sw4.Hapag.Data.DataSet.Helper;
using softwrench.sw4.Hapag.Data.Init;
using softwrench.sw4.Hapag.Data.Sync;
using softwrench.sw4.Hapag.Security;
using softWrench.sW4.Security.Context;
using softWrench.sW4.Security.Entities;
using softWrench.sW4.Security.Services;
using softWrench.sW4.SimpleInjector;
using softWrench.sW4.Util;

namespace softwrench.sw4.Hapag.Data {

    public class R0017WhereClauseProvider : ISingletonComponent {

        private HlagLocationManager _locationManager;
        private readonly IContextLookuper _contextLookuper;

        private static readonly ILog Log = LogManager.GetLogger(typeof(R0017WhereClauseProvider));
        public R0017WhereClauseProvider(HlagLocationManager locationManager, IContextLookuper contextLookuper) {
            _locationManager = locationManager;
            _contextLookuper = contextLookuper;
        }

        #region DelegateMEthods
        public string AssetWhereClause() {
            return AssetWhereClauseFromLocations(_locationManager.GetLocationsOfLoggedUser());
        }

        public string LocalITCSRWhereClause() {
            return SRWhereClause() + " OR (" + HapagQueryConstants.EndUserSR + ")";
        }

        public string ImacWhereClause() {
            return InnerTicketWhereClause("imac");
        }

        public string IncidentWhereClause() {
            return InnerTicketWhereClause("incident");
        }

        public string SRWhereClause() {
            return InnerTicketWhereClause();
        }
        #endregion

        public string AssetWhereClauseFromLocations(HlagGroupedLocation[] locations) {
            var sb = new StringBuilder();
            var isWWUser = SecurityFacade.CurrentUser().IsWWUser();
            if (isWWUser) {
                return "1=1";
            }

            if (CollectionExtensions.IsNullOrEmpty(locations)) {
                //if u dont have any location you should not be able to see anything
                sb.Append("0=1");
                return sb.ToString();
            }
            var i = 0;
            var ctx = _contextLookuper.LookupContext();
            if (!(ctx.IsInModule(FunctionalRole.XItc) && isWWUser)) {
                //HAP-838 item 6, only XITC ww users should see it, or tom itom (who actually see asset)
                sb.AppendFormat("asset.status != '{0}' and ", AssetConstants.Decommissioned);
            }
            foreach (var location in locations) {
                if (i == 0) {
                    sb.Append(" ( ");
                }

                i++;
                sb.Append(String.Format("(asset.pluspcustomer in ('{0}') and {1})",
                    location.SubCustomer, location.CostCentersForQuery("asset.glaccount")));
                if (i < locations.Count()) {
                    sb.Append(" or ");
                } else {
                    sb.Append(" ) ");
                }
            }
            return sb.ToString();
        }







        private string InnerTicketWhereClause(string ticketQualifier = "SR") {
            var isWWUser = SecurityFacade.CurrentUser().IsWWUser();
            if (isWWUser) {
                return "1=1";
            }
            var sb = new StringBuilder();
            sb.AppendFormat("{0}.pluspcustomer = 'HLC-00' or", ticketQualifier);
            var locations = _locationManager.GetLocationsOfLoggedUser();
            var i = 0;
            //if itc is not bound to any location let´s fix at least HLC-00 to it...
            var subCustomers = new StringBuilder("'HLC-00',");
            foreach (var location in locations) {
                i++;
                sb.Append(AppendLocationCondition(ticketQualifier, location));
                sb.Append(" or ");
                subCustomers.Append("'").Append(location.SubCustomer).Append("'").Append(",");
            }
            sb.Append("  ");
            sb.Append(AppendExtraCondition(ticketQualifier, subCustomers));
            sb.Append(" and ");
            sb.Append(HapagQueryUtils.GetDefaultQuery(ticketQualifier));
            return sb.ToString();
        }

        /// <summary>
        /// This is the base ticket restriction which states that a given ticket is only visible if:
        /// 
        /// 1) it´s declared in the same customer as the user
        /// 2) either its costcenter (either from asset or affectedperson_ itdcomment) is equal to the location´s or it has no costcenter at all declared
        /// </summary>
        /// <param name="ticketQualifier"></param>
        /// <param name="location"></param>
        /// <returns></returns>
        private static string AppendLocationCondition(string ticketQualifier, HlagGroupedLocation location) {
            var isImac = ticketQualifier == "imac";
            if (isImac) {
                return String.Format(
                @"({0}.pluspcustomer in('{1}')
                    and (
                            ({0}.assetnum is not null and {2}) or (imac.CLASSIFICATIONID = '81515000' and {3})
                        )
                 )"
                , ticketQualifier, location.SubCustomer, location.CostCentersForQuery("asset_.glaccount"),
                location.ImacDescriptionCostCentersForQuery());
            }
            return String.Format(
               @"({0}.pluspcustomer in('{1}')
                    and (
                            (({2} or affectedperson_.itdcomment is null))
                        )
                 )"
               , ticketQualifier, location.SubCustomer,
               location.CostCentersForQuery("affectedperson_.itdcomment"));
        }



        /// <summary>
        /// Here we´re appending a condition that makes all tickets that follows the given 2 conditions visible:
        /// 
        /// 1) are within this user subcustomers;
        /// 2) where we cannot find any persongroup (despiteof the user actually belonging to it or not) named C-HLC-WW-LC-XXX-YYY where XXX stands for subcustomer and YYY for glaccount.
        /// 
        /// The idea is that the tickets which are not restricted to anybody in a customer should be seen by everybody in that customer; If, the ticket is otherwise visible just for some persongroups, then, 
        /// just the users within those should see it (but this is handled by query in method AppendLocationCondition)
        /// 
        /// </summary>
        /// <param name="ticketQualifier"></param>
        /// <param name="subCustomers"></param>
        private static string AppendExtraCondition(string ticketQualifier, StringBuilder subCustomers) {
            var conditionToCheck = GetConditionToCheck(ticketQualifier);
            return String.Format(@"
                ({0}.pluspcustomer in({1}) and {2})",
                ticketQualifier, subCustomers.ToString(0, subCustomers.Length - 1), conditionToCheck);
        }

        private static string GetConditionToCheck(string ticketQualifier) {
            var conditionToCheck = ticketQualifier == "imac"
                ? String.Format("(asset_assetglaccount_.GLACCOUNT is not null and 'C-HLC-WW-LC-'|| SUBSTRING({0}.PLUSPCUSTOMER,8,3,CODEUNITS32)|| '_' || substr(asset_assetglaccount_.glcomp02,LOCATE('/',asset_assetglaccount_.glcomp02)+1,LOCATE('/',asset_assetglaccount_.glcomp02,LOCATE ('/',asset_assetglaccount_.glcomp02)+1) -LOCATE('/',asset_assetglaccount_.glcomp02)-1) not in (select persongroup from persongroup))", ticketQualifier)
                : String.Format("(affectedperson_personglaccount_.glcomp02 is not null and 'C-HLC-WW-LC-'|| SUBSTRING({0}.PLUSPCUSTOMER,8,3,CODEUNITS32)|| '_' || substr(affectedperson_personglaccount_.glcomp02,LOCATE('/',affectedperson_personglaccount_.glcomp02)+1,LOCATE('/',affectedperson_personglaccount_.glcomp02,LOCATE ('/',affectedperson_personglaccount_.glcomp02)+1) -LOCATE('/',affectedperson_personglaccount_.glcomp02)-1)  not in (select persongroup from persongroup))", ticketQualifier);
            return conditionToCheck;
        }



    }
}
