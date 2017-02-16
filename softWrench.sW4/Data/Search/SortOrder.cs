using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace softWrench.sW4.Data.Search {
    public class SortOrder {
        public string ColumnName {
            get; set;
        }

        public bool IsAscending {
            get; set;
        }

        protected bool Equals(SortOrder other) {
            return string.Equals(ColumnName, other.ColumnName);
        }

        public override bool Equals(object obj) {
            if (ReferenceEquals(null, obj))
                return false;
            if (ReferenceEquals(this, obj))
                return true;
            if (obj.GetType() != this.GetType())
                return false;
            return Equals((SortOrder)obj);
        }

        public override int GetHashCode() {
            return (ColumnName != null ? ColumnName.GetHashCode() : 0);
        }
    }
}
