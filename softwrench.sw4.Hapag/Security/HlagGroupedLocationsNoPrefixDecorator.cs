using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using softwrench.sw4.Shared2.Data.Association;

namespace softwrench.sw4.Hapag.Security {
    /// <summary>
    /// For SRs we don´t want to show the HLAG- in front each label
    /// </summary>
    public class HlagGroupedLocationsNoPrefixDecorator : IHlagLocation {
        private readonly HlagGroupedLocation _groupedLocation;

        public HlagGroupedLocationsNoPrefixDecorator(HlagGroupedLocation groupedLocation) {
            _groupedLocation = groupedLocation;
        }


        public string Value { get { return _groupedLocation.Value; } }

        public string Label {
            get { return _groupedLocation.SubCustomerSuffix; }
        }


        public string SubCustomer { get { return _groupedLocation.SubCustomer; }  }

        public string SubCustomerSuffix { get { return _groupedLocation.SubCustomerSuffix; } }
        
        public string CostCentersForQuery(string columnName) {
            return _groupedLocation.CostCentersForQuery(columnName);
        }

        public IEnumerable<string> CostCenters { get { return _groupedLocation.CostCenters; } }
    }
}
