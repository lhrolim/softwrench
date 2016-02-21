using System;
using cts.commons.persistence;
using Iesi.Collections.Generic;
using NHibernate.Mapping.Attributes;

namespace softwrench.sw4.user.classes.entities.security {

    /// <summary>
    /// Relates to anything which is not a collection-based container, such as a table, or any kind of composition renderer, but rather a container of displayables,
    /// such as the "#main" tab or to any generic tab
    /// </summary>
    [Class(Table = "SEC_CONTAINER_PER", Lazy = false)]
    public class ContainerPermission : IBaseEntity {

        [Id(0, Name = "Id")]
        [Generator(1, Class = "native")]
        public int? Id {
            get; set;
        }

        [Property(Column = "schema_")]
        public string Schema {
            get; set;
        }

        [Property]
        public string ContainerKey {
            get; set;
        }


        [Set(0, Inverse = true, Lazy = CollectionLazy.False)]
        [Key(1, Column = "schema_id")]
        [OneToMany(2, ClassType = typeof(FieldPermission))]
        public virtual ISet<FieldPermission> FielsPermissions {
            get; set;
        }


    }
}
