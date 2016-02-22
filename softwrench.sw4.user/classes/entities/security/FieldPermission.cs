using System;
using cts.commons.persistence;
using NHibernate.Mapping.Attributes;

namespace softwrench.sw4.user.classes.entities.security {


    [Class(Table = "SEC_FIELD_PER", Lazy = false)]
    public class FieldPermission : IBaseEntity {

        [Id(0, Name = "Id")]
        [Generator(1, Class = "native")]
        public int? Id {
            get; set;
        }


        [Property]
        public string Permission { get; set; }


        [Property]
        public string FieldKey{
            get; set;
        }


    }
}
