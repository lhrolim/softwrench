using System;
using Newtonsoft.Json;
using NHibernate.Mapping.Attributes;

namespace softwrench.sw4.firstsolar.classes.com.cts.firstsolar.model {


    [Class(Table = "OPT_MAINTENANCE_ENG", Lazy = false)]
    public class MaintenanceEngineering {

        [Id(0, Name = "Id")]
        [Generator(1, Class = "native")]
        public int? Id {
            get; set;
        }

        [Property]
        public string Engineer { get; set; }

        [Property]
        public long WorkPackageId { get; set; }

        [Property]
        public DateTime? SendTime { get; set; }

        [Property]
        public string Status { get; set; }

        [Property]
        public string Reason { get; set; }

        [Property]
        public string Email { get; set; }
    }
}
