using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using softWrench.sW4.Preferences;
using softWrench.sW4.Util;

namespace softWrench.sW4.Metadata.Security {
    public class UserPreferences {

        public ISet<GridFilterAssociation> GridFilters { get; set; }

        /// <summary>
        /// checks if the user already has a filter created by him with the same alias for a given application.
        /// 
        /// Shared filters do not apply here, since the user might want to have a filter with a same name as a shared one.
        /// 
        /// </summary>
        /// <param name="filter"></param>
        /// <returns></returns>
        public bool ContainsFilter(GridFilter filter) {
            return GridFilters != null &&
                   GridFilters.Any(g => (g.Creator && filter.Alias.EqualsIc(g.Filter.Alias) && filter.Application.EqualsIc(g.Filter.Application)));
        }
    }
}
