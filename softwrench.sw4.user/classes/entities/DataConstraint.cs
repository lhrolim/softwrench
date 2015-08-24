using System;
using cts.commons.persistence.Util;
using NHibernate.Mapping.Attributes;

namespace softwrench.sw4.user.classes.entities {
    [Class(Table = "SW_DATACONSTRAINT")]
    public class DataConstraint {
        [Id(0, Name = "ID")]
        [Generator(1, Class = "native")]
        public virtual int? ID { get; set; }

        [Property]
        public virtual string WhereClause { get; set; }
        [Property]
        public virtual string EntityName { get; set; }

        [Property(TypeType = typeof(BooleanToIntUserType))]
        public virtual Boolean Isactive { get; set; }


        protected bool Equals(DataConstraint other) {
            return string.Equals(EntityName, other.EntityName) && string.Equals(WhereClause, other.WhereClause);
        }

        public override bool Equals(object obj) {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((DataConstraint)obj);
        }

        public override int GetHashCode() {
            unchecked {
                return ((EntityName != null ? EntityName.GetHashCode() : 0) * 397) ^ (WhereClause != null ? WhereClause.GetHashCode() : 0);
            }
        }

        public override string ToString() {
            return string.Format("EntityName: {0}, WhereClause: {1}, Isactive: {2}", EntityName, WhereClause, Isactive);
        }
    }
}
