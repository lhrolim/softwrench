using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using softwrench.sW4.Shared2.Metadata.Applications.Schema.Interfaces;

namespace softwrench.sw4.Shared2.Metadata.Applications.Schema {
    public class DisplayableComponent {
        public String Id {
            get; set;
        }

        public IEnumerable<IApplicationDisplayable> RealDisplayables {
            get; set;
        }

        protected bool Equals(DisplayableComponent other) {
            return string.Equals(Id, other.Id);
        }

        public override bool Equals(object obj) {
            if (ReferenceEquals(null, obj))
                return false;
            if (ReferenceEquals(this, obj))
                return true;
            if (obj.GetType() != this.GetType())
                return false;
            return Equals((DisplayableComponent)obj);
        }

        public override int GetHashCode() {
            return (Id != null ? Id.GetHashCode() : 0);
        }
    }
}
