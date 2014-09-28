using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using softWrench.sW4.Data.Search;

namespace softWrench.sW4.Data.Relationship.Composition {
    public class CompositionExpanderHelper {

        static internal CompositionExpanderResult ParseDictionary(string compositionsToExpand) {
            var result = new CompositionExpanderResult();
            if (string.IsNullOrEmpty(compositionsToExpand)) {
                return result;
            }
            IDictionary<string, string> detailsToExpand = new Dictionary<string, string>();
            IList<string> listsToExpand = new List<string>();
            var values = Regex.Split(compositionsToExpand, SearchUtils.SearchValueSeparator);
            foreach (var value in values) {
                var entry = value.Split('=');
                var ids = entry[1];
                if (ids == "lazy") {
                    listsToExpand.Add(entry[0]);
                } else if (!String.IsNullOrEmpty(ids)) {
                    detailsToExpand.Add(entry[0], ids);
                }
            }
            result.DetailsToExpand = detailsToExpand;
            result.ListsToExpand = listsToExpand;
            return result;
        }

        internal class CompositionExpanderResult {
            public IEnumerable<KeyValuePair<string, string>> DetailsToExpand { get; set; }
            public IEnumerable<string> ListsToExpand { get; set; }

        }

        public class CompositionExpansionOptions {
            // in the format: worklog=100,200,300,,,attachments=200,400,500;
            public string CompositionsToExpand { get; set; }
            public Boolean PrintMode { get; set; }

        }

    }
}
