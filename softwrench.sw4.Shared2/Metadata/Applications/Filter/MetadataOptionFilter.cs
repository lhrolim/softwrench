using System.Collections.Generic;

namespace softwrench.sw4.Shared2.Metadata.Applications.Filter {
    public class MetadataOptionFilter : BaseMetadataFilter {


        /// <summary>
        /// Can be either xxx_.yyy (an association labelfield), or @xxx --> a method invocation on the Application DataSet object
        /// </summary>
        public string Provider {
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


        public MetadataOptionFilter(string attribute, string label, string icon, string position, string tooltip, string whereClause, string provider, bool allowBlank, IEnumerable<MetadataFilterOption> options)
            : base(attribute, label, icon, position, tooltip, whereClause) {
            Provider = provider;
            Options = options;
            AllowBlank = allowBlank;
            Lazy = true;
        }

        public override bool IsValid() {
            return base.IsValid() && Provider != null;
        }
    }
}
