using System.Collections.Concurrent;
using softwrench.sw4.Hapag.Data.Sync;
using softWrench.sW4.Security.Entities;
using softwrench.sw4.Shared2.Data.Association;
using System;
using System.Collections.Generic;
using System.Text;

namespace softwrench.sw4.Hapag.Security {
    public class HlagGroupedLocation : IComparable<IHlagLocation>, IHlagLocation {
        public String SubCustomer { get; set; }

        String OriginatorGroupName { get; set; }

        private readonly IDictionary<PersonGroup, string> _costCenters = new ConcurrentDictionary<PersonGroup, string>();

//        private readonly ISet<string> _costCenters = new HashSet<string>();

        public IEnumerable<string> CostCenters {
            get { return _costCenters.Values; }
        }

        public IDictionary<PersonGroup, string> CostCentersDict {
            get { return _costCenters; }
        }


        public Boolean FromSuperGroup { get; set; }


        public HlagGroupedLocation(string key) {
            SubCustomer = key;
        }

        public HlagGroupedLocation(string key, IDictionary<PersonGroup, string> value, Boolean fromSupergroup) {
            SubCustomer = key;
            _costCenters = value;
            FromSuperGroup = fromSupergroup;
        }

        public String SubCustomerSuffix {
            get {
                return SubCustomer.Replace(HapagPersonGroupConstants.PersonGroupPrefix, "");
            }
        }

        public String DashedCostCentersForQuery(bool includeSingleQuotes = true) {
            var sb = new StringBuilder();
            foreach (var costCenter in CostCenters) {
                if (includeSingleQuotes) {
                    sb.Append("'");
                }
                sb.Append(costCenter.Replace('/', '-'));
                if (includeSingleQuotes) {
                    sb.Append("'");
                }
                sb.Append(",");
            }
            return sb.ToString(0, sb.Length - 1);

        }

        public String CostCentersForQuery(string columnName) {
            var sb = new StringBuilder();
            sb.Append("(");
            foreach (var costCenter in CostCenters) {
                sb.Append(columnName).Append(" like '%").Append(costCenter).Append("%'").Append(" or ");
            }
            return sb.ToString(0, sb.Length - " or ".Length) + ")";
        }

        public object ImacDescriptionCostCentersForQuery() {
            var sb = new StringBuilder();
            sb.Append("(");
            foreach (var costCenter in CostCenters) {
                sb.Append("imac.description").Append(" like '%//").Append(costCenter.Split('/')[1]).Append("%'").Append(" or ");
            }
            return sb.ToString(0, sb.Length - " or ".Length) + ")";
        }

        protected bool Equals(HlagGroupedLocation other) {
            return string.Equals(SubCustomer, other.SubCustomer);
        }

        public override bool Equals(object obj) {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((HlagGroupedLocation)obj);
        }

        public override int GetHashCode() {
            return (SubCustomer != null ? SubCustomer.GetHashCode() : 0);
        }

        public string Value {
            //TODO: shouldn´t this be the entire subcustomer?
            get { return SubCustomerSuffix; }
        }

        public virtual string Label {
            get { return "HLAG-" + SubCustomerSuffix; }
        }

        public int CompareTo(IHlagLocation other) {
            return System.String.Compare(Label, other.Label, StringComparison.Ordinal);
        }

        public override string ToString() {
            return string.Format("CostCenters: {1}, SubCustomerSuffix: {0}", CostCenters, SubCustomerSuffix);
        }


    }
}
