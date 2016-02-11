using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NHibernate.Mapping.Attributes;

namespace softwrench.sw4.user.classes.entities.security {


    [Class(Table = "SEC_SCHEMAGROUP_PER", Lazy = false)]
    public class SchemaPermissionGroup {

        [Id(0, Name = "Id")]
        [Generator(1, Class = "native")]
        public int? Id {
            get; set;
        }

        [Property]
        public string Mode {
            get; set;
        }

        [Property]
        public string Schema {
            get; set;
        }

        [Set(0, Inverse = true, Lazy = CollectionLazy.False)]
        [Key(1, Column = "schemagroup_id")]
        [OneToMany(2, ClassType = typeof(SchemaPermission))]
        public ISet<SchemaPermission> SchemaPermissions {
            get; set;
        }


        [Set(0, Inverse = true, Lazy = CollectionLazy.False)]
        [Key(1, Column = "schemagroup_id")]
        [OneToMany(2, ClassType = typeof(CompositionPermission))]
        public ISet<CompositionPermission> CompositionPermissions {
            get; set;
        }


        [Set(0, Inverse = true, Lazy = CollectionLazy.False)]
        [Key(1, Column = "schemagroup_id")]
        [OneToMany(2, ClassType = typeof(ActionPermission))]
        public ISet<ActionPermission> ActionPermissions {
            get; set;
        }


    }
}
