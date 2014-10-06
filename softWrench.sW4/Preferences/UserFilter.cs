using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NHibernate.Mapping.Attributes;
using softWrench.sW4.Security.Interfaces;

namespace softWrench.sW4.Preferences {

    [Class(Table = "PREF_USERFILTER", Lazy = false)]
    public class UserFilter : IBaseEntity {
        public int? Id { get; set; }

        public string Alias { get; set; }

        [Property]
        public string Application{ get; set; }

        [Property]
        public string Schema { get; set; }

        [Property]
        public string QueryString { get; set; }

    }
}
