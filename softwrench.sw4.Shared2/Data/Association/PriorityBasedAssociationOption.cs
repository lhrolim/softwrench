using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace softwrench.sw4.Shared2.Data.Association {
    public class PriorityBasedAssociationOption : AssociationOption {
        public PriorityBasedAssociationOption(string value, string label, int priority) : base(value, label) {
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
