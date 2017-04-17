using System;
using System.Collections.Generic;

namespace softWrench.sW4.Data.Sync {
    public class Rowstamps {
        private readonly string _lowerlimit;
        private readonly string _upperlimit;

        public Rowstamps(string lowerlimit, string upperlimit) {
            this._lowerlimit = lowerlimit;
            this._upperlimit = upperlimit;
        }

        public Rowstamps() {

        }

        public string Lowerlimit => _lowerlimit;

        public string Upperlimit => _upperlimit;

        public bool BothLimits() {
            return _lowerlimit != null && _upperlimit != null;
        }

        public bool NotBound() {
            return _lowerlimit == null && _upperlimit == null;
        }

        public RowstampMode CurrentMode() {
            if (NotBound()) {
                return RowstampMode.None;
            }
            if (BothLimits()) {
                return RowstampMode.Both;
            }
            if (_lowerlimit != null) {
                return RowstampMode.Lower;
            }
            return RowstampMode.Upper;
        }

        public enum RowstampMode {
            None, Upper, Lower, Both,
        }

        public IDictionary<string, object> GetParameters() {
            var dictionary = new Dictionary<String, object>();
            if (_lowerlimit != null) {
                dictionary.Add("lowerrowstamp", _lowerlimit);
            }
            if (_upperlimit != null) {
                dictionary.Add("upperrowstamp", _upperlimit);
            }
            return dictionary;
        }

     
    }
}
