using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace softwrench.sw4.Hapag.Data.DataSet.Helper {

    public class R0042AssetKey {

        public string AssetNum { get; set; }
        public string SiteId { get; set; }
        public string AssetId { get; set; }

        protected bool Equals(R0042AssetKey other) {
            return (string.Equals(AssetNum, other.AssetNum) && string.Equals(SiteId, other.SiteId)) || string.Equals(AssetId, other.AssetId);
        }

        public override bool Equals(object obj) {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((R0042AssetKey)obj);
        }

        public override int GetHashCode() {
            unchecked {
                var hashCode = (AssetNum != null ? AssetNum.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (SiteId != null ? SiteId.GetHashCode() : 0);
                return hashCode;
            }
        }
    }
}
