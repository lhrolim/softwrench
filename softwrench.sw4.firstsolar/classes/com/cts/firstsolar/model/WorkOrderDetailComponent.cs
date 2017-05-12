using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NHibernate.Mapping.Attributes;

namespace softwrench.sw4.firstsolar.classes.com.cts.firstsolar.model {


    [Component]
    public class WorkOrderDetailComponent {


        [Property]
        public long WorkorderId { get; set; }

        [Property]
        public string WorkType { get; set; }

        [Property]
        public DateTime Created { get; set; }

        [Property]
        public string CreatedBy { get; set; }

        [Property]
        public string Status { get; set; }

        [Property]
        public string Scheduler { get; set; }

        [Property]
        public string Site { get; set; }
        [Property]
        public string AssetNum { get; set; }

//        [Property]
//        public string MaitenanceProcedure { get; set; }

        [Property]
        public string JPNum { get; set; }
        [Property]
        public string PmNum { get; set; }
        [Property]
        public string Summary { get; set; }
        [Property]
        public long LongDescriptionId{ get; set; }





    }
}
