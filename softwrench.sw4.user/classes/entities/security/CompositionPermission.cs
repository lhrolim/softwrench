﻿using System;
using cts.commons.persistence;
using Iesi.Collections.Generic;
using Newtonsoft.Json;
using NHibernate.Mapping.Attributes;

namespace softwrench.sw4.user.classes.entities.security {


    [Class(Table = "SEC_COMPOSITION_PER", Lazy = false)]
    public class CompositionPermission : IBaseEntity {

        [Id(0, Name = "Id")]
        [Generator(1, Class = "native")]
        public int? Id {
            get; set;
        }

        [Property]
        public string CompositionKey {
            get; set;
        }

        [Property(Column = "schema_")]
        public string Schema {
            get; set;
        }


        [Set(0, Inverse = true, Lazy = CollectionLazy.False)]
        [Key(1, Column = "composition_id")]
        [OneToMany(2, ClassType = typeof(FieldPermission))]
        public virtual ISet<FieldPermission> FieldsPermissions {
            get; set;
        }

        [ComponentProperty]
        public CollectionCrudPermissions CollectionPermissions {
            get; set;
        }




    }
}
