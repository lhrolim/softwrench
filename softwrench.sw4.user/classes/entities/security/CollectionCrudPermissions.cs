using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NHibernate.Mapping.Attributes;

namespace softwrench.sw4.user.classes.entities.security {

    [Component]
    public class CollectionCrudPermissions {

        [Property]
        public bool AllowCreation {
            get; set;
        }

        [Property]
        public bool AllowUpdate {
            get; set;
        }


        [Property]
        public bool AllowRemoval {
            get; set;
        }


        [Property]
        public bool AllowViewOnly {
            get; set;
        }


    }
}
