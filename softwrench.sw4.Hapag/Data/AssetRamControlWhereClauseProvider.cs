using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using softwrench.sw4.Hapag.Data.Sync;
using softwrench.sw4.Hapag.Security;
using softWrench.sW4.Security.Entities;
using cts.commons.simpleinjector;
using softWrench.sW4.Util;

namespace softwrench.sw4.Hapag.Data {

    public class AssetRamControlWhereClauseProvider : ISingletonComponent {

        private readonly R0017WhereClauseProvider _rooR0017WhereClauseProvider;
        private readonly IHlagLocationManager _locationManager;



        public AssetRamControlWhereClauseProvider(R0017WhereClauseProvider rooR0017WhereClauseProvider, IHlagLocationManager locationManager) {
            this._rooR0017WhereClauseProvider = rooR0017WhereClauseProvider;
            _locationManager = locationManager;
        }

        public string AssetWhereClause() {
            var locations = _locationManager.FindLocationsOfParentLocation(new PersonGroup { Name = HapagPersonGroupConstants.HapagRegionAmerica });
            if (locations == null) {
                return null;
            }
            return AssetWhereClauseFromLocations(locations.ToArray());
        }

        public string AssetWhereClauseFromLocations(HlagGroupedLocation[] locations) {
            var sb = new StringBuilder();
            var allCostCenters = new List<string>();
            if (CollectionExtensions.IsNullOrEmpty(locations)) {
                throw new InvalidOperationException(HapagErrorCatalog.Err002);
            }
            var i = 0;
            sb.Append("asset.pluspcustomer in (");
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
