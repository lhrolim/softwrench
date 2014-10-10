using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using NHibernate.Mapping.Attributes;
using softWrench.sW4.Security.Entities;
using softWrench.sW4.Security.Interfaces;

namespace softWrench.sW4.Preferences {

    [Component]
    public class UserPreference :IBaseEntity{
        public int? Id { get; set; }

        [Set(0, Table = "PREF_FILTER",
        Lazy = CollectionLazy.False, Cascade = "all")]
        [Key(1, Column = "pref_id")]
        [OneToMany(2, ClassType = typeof(UserFilter))]
        public virtual Iesi.Collections.Generic.ISet<UserFilter> Filters { get; set; }

    }
}
