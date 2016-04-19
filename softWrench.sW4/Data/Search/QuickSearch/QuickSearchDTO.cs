using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using JetBrains.Annotations;

namespace softWrench.sW4.Data.Search.QuickSearch {

    public class QuickSearchDTO {

        /// <summary>
        /// List of composition (by their relationship), that should be included on the quicksearch. 
        /// The search would be performed expecting the quickSearchData to be found within any of the list fields of the composition
        /// 
        /// List instead of set, cause .net doesnt translate sets automatically 
        /// </summary>
        [CanBeNull]
        public List<string> CompositionsToInclude {
            get; set;
        }

        /// <summary>
        /// 
        /// </summary>
        [CanBeNull]
        public List<string> HiddenFieldsToInclude {
            get; set;
        }

        [NotNull]
        public string QuickSearchData {
            get; set;
        }

        public static QuickSearchDTO Basic(string toSearch) {
            return new QuickSearchDTO {
                QuickSearchData = toSearch
            };
        }

    }
}
