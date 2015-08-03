using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using log4net;
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


    public class AssetControlWhereClauseProvider : ISingletonComponent {
        private readonly R0017WhereClauseProvider _rooR0017WhereClauseProvider;
        private readonly IContextLookuper _contextLookuper;
        private readonly IHlagLocationManager _locationManager;

        private static readonly ILog Log = LogManager.GetLogger(typeof(AssetControlWhereClauseProvider));


        public AssetControlWhereClauseProvider(R0017WhereClauseProvider rooR0017WhereClauseProvider, IContextLookuper contextLookuper, IHlagLocationManager locationManager) {
            _rooR0017WhereClauseProvider = rooR0017WhereClauseProvider;
            _contextLookuper = contextLookuper;
            _locationManager = locationManager;
            Log.Debug("init log");
        }

        //Implements HAP-838 + HAP-1062
        public string AssetWhereClauseIfRegionSelected() {
            var ctx = _contextLookuper.LookupContext();
            var parameters = ctx.MetadataParameters;
            var sb = new StringBuilder();
            if (ctx.IsInModule(FunctionalRole.XItc)) {
                // for xitc we need extra conditions
                sb.Append(_rooR0017WhereClauseProvider.AssetWhereClause());
            } else if (!ctx.IsInAnyModule(FunctionalRole.Tom, FunctionalRole.Itom)) {
                //HAP-838 item 6, except for TOM,ITOM and XITC with WW no one should see these
                sb.Append("asset.status !='{0}'".Fmt(AssetConstants.Decommissioned));
            }


            if (!HlagLocationUtil.ValidateRegionSelectionIsAllowed(ctx, SecurityFacade.CurrentUser()) ||
                !parameters.ContainsKey("region")) {
                //no region selected
                return sb.ToString();
            }

            var parentRegion = parameters["region"];
            try {
                var locations = _locationManager.FindLocationsOfParentLocation(new PersonGroup { Name = parentRegion });
                ISet<string> subcustomers = new HashSet<string>();
                foreach (var hlagGroupedLocation in locations) {
                    //HAP-1062 --> appending only subcustomers, but disregarding costcenters
                    subcustomers.Add(hlagGroupedLocation.SubCustomer);
                }
                if (!subcustomers.Any()) {
                    //no subcustomer added to the query
                    return sb.ToString();
                }

                if (sb.Length != 0) {
                    sb.Append(" and ");
                }


                sb.Append(" asset.pluspcustomer in ( ");

                foreach (var subcustomer in subcustomers) {
                    sb.Append("'").Append(subcustomer).Append("'").Append(",");
                }
                //removing last , and adding trailing parenthesis
                return sb.ToString(0, sb.Length - 1) + ")";



            } catch (Exception) {
                Log.WarnFormat("location {0} was not found", parentRegion);
                return sb.ToString();
            }
        }




    }
}
