using System;
using System.Collections.Generic;

namespace softWrench.sW4.Data.Offline {

    public class Rowstamps {
        public string LowerUid { get; set; }
        public string Lowerlimit { get; set; }

        public string Upperlimit { get; set; }

        public Rowstamps(string lowerlimit, string upperlimit) {
            this.Lowerlimit = lowerlimit;
            this.Upperlimit = upperlimit;
        }

        public Rowstamps() {

        }

        

        public bool BothLimits() {
            return Lowerlimit != null && Upperlimit != null;
        }

        public bool NotBound() {
            return Lowerlimit == null && Upperlimit == null;
        }

        public RowstampMode CurrentMode() {
            if (NotBound()) {
                return RowstampMode.None;
            }
            if (BothLimits()) {
                return RowstampMode.Both;
            }
            if (Lowerlimit != null) {
                return RowstampMode.Lower;
            }
            return RowstampMode.Upper;
        }

        public enum RowstampMode {
            None, Upper, Lower, Both,
        }

        public IDictionary<string, object> GetParameters() {
            var dictionary = new Dictionary<String, object>();
            if (Lowerlimit != null) {
                dictionary.Add("lowerrowstamp", Lowerlimit);
            }
            if (Upperlimit != null) {
                dictionary.Add("upperrowstamp", Upperlimit);
            }
            return dictionary;
        }

     
    }
}
