using System;
using System.Linq;
using cts.commons.persistence;
using cts.commons.portable.Util;
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


        [Set(0, Inverse = true, Lazy = CollectionLazy.False, Cascade = "all")]
        [Key(1, Column = "composition_id")]
        [OneToMany(2, ClassType = typeof(FieldPermission))]
        [JsonConverter(typeof(IesiSetConverter<FieldPermission>))]
        public virtual ISet<FieldPermission> FieldPermissions {
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


        public void Merge(CompositionPermission other) {
            AllowCreation = AllowCreation && other.AllowCreation;
            AllowUpdate = AllowUpdate && other.AllowUpdate;
            AllowRemoval = AllowRemoval && other.AllowRemoval;
            AllowViewOnly = AllowViewOnly && other.AllowViewOnly;

            if (other.FieldPermissions == null) {
                return;
            }

            foreach (var otherField in other.FieldPermissions) {
                var thisField = FieldPermissions.FirstOrDefault(
                    f => f.FieldKey.EqualsIc(otherField.FieldKey));
                if (thisField == null) {
                    FieldPermissions.Add(otherField);
                } else {
                    thisField.Merge(otherField);
                }
            }

        }
    }
}
