using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using log4net;
using log4net.Core;
using softwrench.sw4.Hapag.Data.DataSet.Helper;
using softwrench.sw4.Hapag.Data.Sync;
using softwrench.sw4.Hapag.Security;
using softWrench.sW4.Security.Entities;
using softWrench.sW4.SimpleInjector;
using softWrench.sW4.Util;

namespace softwrench.sw4.Hapag.Data {

    public class AssetRamControlWhereClauseProvider : ISingletonComponent {

        private readonly R0017WhereClauseProvider _rooR0017WhereClauseProvider;
        private readonly IHlagLocationManager _locationManager;

        private static ILog Log = LogManager.GetLogger(typeof (AssetRamControlWhereClauseProvider));


        public AssetRamControlWhereClauseProvider(R0017WhereClauseProvider rooR0017WhereClauseProvider, IHlagLocationManager locationManager) {
            this._rooR0017WhereClauseProvider = rooR0017WhereClauseProvider;
            _locationManager = locationManager;
        }

        public string AssetWhereClause() {
            return AssetWhereClauseForRegion(HapagPersonGroupConstants.HapagRegionAmerica);
        }

        public string AssetWhereClauseForRegion(String regionName){
            var locations = _locationManager.FindLocationsOfParentLocation(new PersonGroup { Name = regionName });
            var hlagGroupedLocations = locations as HlagGroupedLocation[] ?? locations.ToArray();
            if (CollectionExtensions.IsNullOrEmpty(hlagGroupedLocations)) {
                Log.WarnFormat("no locations found for region {0}, excluding everything from the filter", regionName);
                return "1=0";
            }
            return AssetWhereClauseFromLocations(hlagGroupedLocations.ToArray());
        }

        public string AssetWhereClauseFromLocations(HlagGroupedLocation[] locations) {
            var sb = new StringBuilder();
            var allCostCenters = new List<string>();
           
            var i = 0;
            sb.AppendFormat("asset.status != '{0}'", AssetConstants.Decommissioned);
            sb.Append(" and asset.pluspcustomer in (");
            foreach (var location in locations) {
                i++;
                sb.Append("'" + location.SubCustomer + "'");
                if (i < locations.Count()) {
                    sb.Append(",");
                }
            }
            sb.Append(")");
            return sb.ToString();
        }

    }
}
