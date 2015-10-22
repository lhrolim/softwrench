using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace softwrench.sw4.Shared2.Metadata.Applications.Filter {
    public class MetadataFilterOption {
        public string Label {
            get; set;
        }
        public string Value {
            get; set;
        }

        public MetadataFilterOption(string label, string value) {
            Label = label;
            Value = value;
        }
    }
}
