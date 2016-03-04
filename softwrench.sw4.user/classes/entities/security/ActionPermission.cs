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


        protected bool Equals(ActionPermission other) {
            return string.Equals(ActionId, other.ActionId) && string.Equals(Schema, other.Schema);
        }

        public override bool Equals(object obj) {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((ActionPermission)obj);
        }

        public override int GetHashCode() {
            unchecked {
                return ((ActionId != null ? ActionId.GetHashCode() : 0) * 397) ^ (Schema != null ? Schema.GetHashCode() : 0);
            }
        }
    }
}
