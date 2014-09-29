using System;
using NHibernate.Mapping.Attributes;
using softWrench.sW4.Security.Interfaces;

namespace softWrench.sW4.Security.Entities {

    [Class(Table = "SW_USER_CUSTOMROLE")]
    public class UserCustomRole : IBaseEntity {
        [Id(0, Name = "Id")]
        [Generator(1, Class = "native")]
        public virtual int? Id { get; set; }

        [Property(Column = "user_id")]
        public virtual int? UserId { get; set; }

        [ManyToOne(Column = "role_id", OuterJoin = OuterJoinStrategy.False, Lazy = Laziness.False)]
        public virtual Role Role { get; set; }

        [Property]
        public virtual Boolean Exclusion { get; set; }

        protected bool Equals(UserCustomRole other) {
            return UserId == other.UserId && Equals(Role, other.Role);
        }

        public override bool Equals(object obj) {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((UserCustomRole)obj);
        }

        public override int GetHashCode() {
            unchecked {
                return (UserId.GetHashCode() * 397) ^ (Role != null ? Role.GetHashCode() : 0);
            }
        }
    }
}
