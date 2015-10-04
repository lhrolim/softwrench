using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NHibernate.Mapping.Attributes;

namespace softwrench.sw4.user.classes.entities {

    [JoinedSubclass(NameType = typeof(ApplicationPermission), Lazy = false, ExtendsType = typeof(Role), Table = "SEC_APPROLE")]
    public class ApplicationPermission :Role {

        [Key(-1, Column = "AppPermissionId")]
        public virtual int? AppPermissionId {
            get; set;
        }
    }
}
