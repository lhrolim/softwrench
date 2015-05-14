using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using log4net;
using softwrench.sw4.Hapag.Data.DataSet.Helper;
using softwrench.sw4.Hapag.Data.Sync;
using softwrench.sw4.Hapag.Security;
using softWrench.sW4.Security.Context;
using softWrench.sW4.Security.Entities;
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

        public string AssetWhereClauseIfRegionSelected() {
            var parameters = _contextLookuper.LookupContext().MetadataParameters;
            if (parameters.ContainsKey("region")) {
                var parentRegion = parameters["region"];
                try {
                    var locations = _locationManager.FindLocationsOfParentLocation(new PersonGroup { Name = parentRegion });
                    return _rooR0017WhereClauseProvider.AssetWhereClauseFromLocations(locations.ToArray());
                } catch (Exception) {
                    Log.WarnFormat("location {0} was not found", parentRegion);
                    return null;
                }

            }
            return null;
        }

        //HAP-838 item 6
        public string AssetWhereClause() {
            return "asset.status !='{0}'".Fmt(AssetConstants.Decommissioned);
        }



    }
}
