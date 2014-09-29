using System;
using NHibernate.Mapping.Attributes;
namespace softWrench.sW4.Security.Entities {
    [Class(Table = "SW_DATACONSTRAINT")]
    public class DataConstraint {
        [Id(0, Name = "ID")]
        [Generator(1, Class = "native")]
        public virtual int? ID { get; set; }

        [Property]
        public virtual string WhereClause { get; set; }
        [Property]
        public virtual string EntityName { get; set; }

        [Property]
        public virtual Boolean Isactive { get; set; }


        protected bool Equals(DataConstraint other) {
            return string.Equals(EntityName, other.EntityName) && string.Equals(WhereClause, other.WhereClause);
        }

        public override bool Equals(object obj) {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
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
