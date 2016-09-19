using System.Collections.Generic;
using NHibernate.Mapping.Attributes;

namespace softwrench.sw4.user.classes.entities {

    [Class(Table = "SEC_ROLEGROUP")]
    public class RoleGroup {

        [Id(0, Name = "Id")]
        [Generator(1, Class = "native")]
        public virtual int? Id { get; set; }

        [Property]
        public virtual string Label { get; set; }

        [Property]
        public virtual string Description { get; set; }

        [Set(0, Lazy = CollectionLazy.False, Inverse = true)]
        [Key(1, Column = "rolegroup_id")]
        [OneToMany(2, ClassType = typeof(Role))]
        public virtual ISet<Role> Roles { get; set; }


    }
}
