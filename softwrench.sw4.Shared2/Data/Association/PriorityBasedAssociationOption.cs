using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace softwrench.sw4.Shared2.Data.Association {
    public class PriorityBasedAssociationOption : MultiValueAssociationOption {

        public PriorityBasedAssociationOption(string value, string label, int priority, IDictionary<string,object>extraFields=null) : base(value, label,extraFields) {
            Priority = priority;
        }

        public int CompareTo(PriorityBasedAssociationOption other) {
            return Priority.CompareTo(other.Priority);
        }

        public int Priority {
            get; set;
        }
    }
}
