﻿using System;
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
        public bool PreSelected {
            get; set;
        }
        public string Tooltip {
            get; set;
        }
        public bool? DisplayCode {
            get; set;
        }

        public MetadataFilterOption(string label, string value, bool preSelected, string tooltip, bool? displaycode) {
            Label = label;
            Value = value;
            PreSelected = preSelected;
            Tooltip = tooltip;
            DisplayCode = displaycode;
        }
    }
}