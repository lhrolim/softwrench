using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using softwrench.sw4.dashboard.classes.model.entities;
using softwrench.sw4.Shared2.Data.Association;
using softwrench.sW4.Shared2.Metadata.Applications.Schema;

namespace softwrench.sw4.dashboard.classes.model {
    public class ManageDashBoardsDTO {

        public Boolean CanCreateOwn { get; set; }
        public Boolean CanCreateShared { get; set; }

        public IEnumerable<Dashboard> Dashboards { get; set; }

        public IEnumerable<IAssociationOption> Profiles { get; set; }



        public int? PreferredId { get; set; }
        
        public ApplicationSchemaDefinition NewPanelSchema { get; set; }
        public ApplicationSchemaDefinition SaveDashboardSchema { get; set; }

        public IDictionary<string,ApplicationSchemaDefinition> PanelSchemas{ get; set; }


        public IEnumerable<IAssociationOption> Applications { get; set; }
    }
}
