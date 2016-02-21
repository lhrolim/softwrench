using System;
using cts.commons.persistence;
using NHibernate.Mapping.Attributes;

namespace softwrench.sw4.user.classes.entities.security {


    [Class(Table = "SEC_ACTION_PER", Lazy = false)]
    public class ActionPermission : IBaseEntity {

        [Id(0, Name = "Id")]
        [Generator(1, Class = "native")]
        public int? Id {
            get; set;
        }

        [Property]
        public string ActionId {
            get; set;
        }

        [Property(Column = "schema_")]
        public string Schema {
            get; set;
        }


    }
}
