using System;
using cts.commons.persistence;
using cts.commons.web.Formatting;
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
        [JsonConverter(typeof(IesiSetConverter<FieldPermission>))]
        public virtual ISet<FieldPermission> FieldsPermissions {
            get; set;
        }

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
