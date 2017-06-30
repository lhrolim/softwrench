using System;
using System.Collections.Generic;
using System.Linq;
using cts.commons.persistence;
using cts.commons.portable.Util;
using cts.commons.web.Formatting;
using Iesi.Collections.Generic;
using Newtonsoft.Json;
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

        [Property]
        public bool AllowView {
            get; set;
        }


        [Set(0, Lazy = CollectionLazy.False, Cascade = "all-delete-orphan")]
        [Key(1, Column = "schema_id")]
        [OneToMany(2, ClassType = typeof(FieldPermission))]
        [JsonConverter(typeof(IesiSetConverter<FieldPermission>))]
        public virtual ISet<FieldPermission> FieldPermissions {
            get; set;
        }

        [Set(0, Lazy = CollectionLazy.False, Cascade = "all-delete-orphan")]
        [Key(1, Column = "schema_id")]
        [OneToMany(2, ClassType = typeof(SectionPermission))]
        [JsonConverter(typeof(IesiSetConverter<SectionPermission>))]
        public virtual ISet<SectionPermission> SectionPermissions {
            get; set;
        }

        protected bool Equals(ContainerPermission other) {
            return string.Equals(Schema, other.Schema) && string.Equals(ContainerKey, other.ContainerKey);
        }

        public override bool Equals(object obj) {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((ContainerPermission)obj);
        }

        public override int GetHashCode() {
            unchecked {
                return ((Schema != null ? Schema.GetHashCode() : 0) * 397) ^ (ContainerKey != null ? ContainerKey.GetHashCode() : 0);
            }
        }

        public void Merge(ContainerPermission other) {
            if (other.FieldPermissions != null) {
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

            if (other.SectionPermissions != null) {
                foreach (var otherSection in other.SectionPermissions) {
                    var thisSection = SectionPermissions.FirstOrDefault(
                        f => f.SectionId.EqualsIc(otherSection.SectionId));
                    if (thisSection == null) {
                        SectionPermissions.Add(otherSection);
                    } else {
                        thisSection.Merge(otherSection);
                    }
                }
            }



        }
    }
}
