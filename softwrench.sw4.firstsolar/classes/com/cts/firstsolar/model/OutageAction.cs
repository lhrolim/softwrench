using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using cts.commons.persistence;
using NHibernate.Mapping.Attributes;

namespace softwrench.sw4.firstsolar.classes.com.cts.firstsolar.model {

    [Class(Table = "OPT_DAILY_OUTAGE_ACTION", Lazy = false)]
    public class OutageAction : IBaseEntity {

        [Id(0, Name = "Id")]
        [Generator(1, Class = "native")]
        public int? Id {
            get; set;
        }

        [Property]
        public string Action { get; set; }

        [Property]
        public string Assignee { get; set; }

        [Property]
        public string AssigneeLabel { get; set; }

        [Property]
        public string Completed { get; set; }

        [Property]
        public DateTime? ActionTime { get; set; }

    }
}
