using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace softwrench.sw4.Shared2.Metadata.Applications.Filter {

    /// <summary>
    /// This filter would be added to the quicksearch combobox and not to a particular column
    /// </summary>
    public class QuickSearchFilter {
        public QuickSearchFilter(string label, string whereClause, string id) {
            Label = label;
            WhereClause = whereClause;
            Id = id;
        }

        public string Type => typeof (QuickSearchFilter).Name;

        public string Label {
            get; set;
        }

        /// <summary>
        /// <seealso cref="BaseMetadataFilter.WhereClause"/>
        /// </summary>
        public string WhereClause {
            get; set;
        }

        public string Id {
            get; set;
        }

        //Adapting GridFilter.cs class
        public string Alias => Label;
    }
}
