using System;
using System.Collections.Generic;
using softwrench.sw4.dashboard.classes.model.entities;
using softwrench.sw4.Shared2.Data.Association;
using softwrench.sW4.Shared2.Metadata.Applications.Schema;

namespace softwrench.sw4.dashboard.classes.model {
    public class ManageDashBoardsDTO {
        public IEnumerable<Dashboard> Dashboards { get; set; }
        public IEnumerable<IAssociationOption> Profiles { get; set; }
        public int? PreferredId { get; set; }
        public IEnumerable<IAssociationOption> Applications { get; set; }
        public ManageDashboardsPermissionDTO Permissions { get; set; }
        public ManageDashboardsSchemasDTO Schemas { get; set; }

        public class ManageDashboardsPermissionDTO {
            public bool CanCreateOwn { get; set; }
            public bool CanCreateShared { get; set; }
            public bool CanDeleteOwn { get; set; }
            public bool CanDeleteShared { get; set; }
        }

        public class ManageDashboardsSchemasDTO {
            public ApplicationSchemaDefinition NewPanelSchema { get; set; }
            public ApplicationSchemaDefinition SaveDashboardSchema { get; set; }
            public IDictionary<string, ApplicationSchemaDefinition> PanelSchemas { get; set; }
        }
    }
   

}
