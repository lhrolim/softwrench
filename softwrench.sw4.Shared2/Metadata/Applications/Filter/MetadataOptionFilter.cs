using System.Collections.Generic;

namespace softwrench.sw4.Shared2.Metadata.Applications.Filter {
    public class MetadataOptionFilter : BaseMetadataFilter {


        /// <summary>
        /// Can be either xxx_.yyy (an association labelfield), or @xxx --> a method invocation on the Application DataSet object, or @xxx.yyy any simpleinjector method
        /// </summary>
        public string Provider {
            get; set;
        }

        public bool DisplayCode {
            get; set;
        }

        public bool AllowBlank {
            get; set;
        }

        public IEnumerable<MetadataFilterOption> Options {
            get; set;
        }

        public bool Lazy {
            get; set;
        }

        public string AdvancedFilterSchemaId {
            get; set;
        }

        public string AdvancedFilterAttribute {
            get; set;
        }

        public string Preselected {
            get; set;
        }
        /// <summary>
        /// Initialize a new instance of the <see cref="MetadataOptionFilter"/> class.
        /// </summary>
        public MetadataOptionFilter(string attribute, string label, string icon, string position, string tooltip, string whereClause, string provider, bool displayCode, bool allowBlank, string style, bool lazy, string advancedFilterSchemaId, string preselected, IEnumerable<MetadataFilterOption> options)
            : base(attribute, label, icon, position, tooltip, whereClause, false, style) {
            Provider = provider;
            Options = options;
            AllowBlank = allowBlank;
            DisplayCode = displayCode;
            Lazy = lazy;
            Preselected = preselected;
            AdvancedFilterSchemaId = advancedFilterSchemaId;
        }

        public override bool IsValid() {
            return base.IsValid() && Provider != null;
        }
    }
}
