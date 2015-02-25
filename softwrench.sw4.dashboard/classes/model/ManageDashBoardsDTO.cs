using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using softwrench.sw4.dashboard.classes.model.entities;

namespace softwrench.sw4.dashboard.classes.model {
    public class ManageDashBoardsDTO {

        public Boolean CanCreateOwn { get; set; }
        public Boolean CanCreateShared { get; set; }

        public IEnumerable<Dashboard> Dashboards { get; set; }

        public int? PreferredId { get; set; }

    }
}
