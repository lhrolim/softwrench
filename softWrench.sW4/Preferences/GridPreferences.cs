using System.Collections.Generic;
using System.Linq;
using cts.commons.portable.Util;
using softWrench.sW4.Metadata.Security;

namespace softWrench.sW4.Preferences {
    public class GridPreferences {

        public ISet<GridFilterAssociation> GridFilters { get; set; }

        /// <summary>
        /// checks if the user already has a filter created by him with the same alias for a given application.
        /// 
        /// Shared filters do not apply here, since the user might want to have a filter with a same name as a shared one.
        /// 
        /// </summary>
        /// <param name="filter"></param>
        /// <param name="user"></param>
        /// <returns></returns>
        public bool ContainsFilter(GridFilter filter, InMemoryUser user) {
            return GridFilters != null &&
                   GridFilters.Any(g => (g.User.Id == user.DBId && filter.Alias.EqualsIc(g.Filter.Alias) && filter.Application.EqualsIc(g.Filter.Application)));
        }
    }
}
