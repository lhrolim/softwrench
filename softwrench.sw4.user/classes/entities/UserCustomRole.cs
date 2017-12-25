﻿using System;
using cts.commons.persistence;
using cts.commons.persistence.Util;
using Newtonsoft.Json;
using NHibernate.Mapping.Attributes;

namespace softwrench.sw4.user.classes.entities {

    [Class(Table = "SW_USER_CUSTOMROLE")]
    public class UserCustomRole : IBaseEntity {
        [Id(0, Name = "Id")]
        [Generator(1, Class = "native")]
        public virtual int? Id { get; set; }

        [JsonIgnore]
        [ManyToOne(Column = "user_id", OuterJoin = OuterJoinStrategy.False, Lazy = Laziness.False)]
        public virtual User UserId { get; set; }

        [ManyToOne(Column = "role_id", OuterJoin = OuterJoinStrategy.False, Lazy = Laziness.False)]
        public virtual Role Role { get; set; }

        [Property(TypeType = typeof(BooleanToIntUserType))]
        public virtual Boolean Exclusion { get; set; }

        protected bool Equals(UserCustomRole other) {
            return UserId == other.UserId && Equals(Role, other.Role);
        }

        public override bool Equals(object obj) {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((UserCustomRole)obj);
        }

        public override int GetHashCode() {
            unchecked {
                return (UserId.GetHashCode() * 397) ^ (Role != null ? Role.GetHashCode() : 0);
            }
        }
    }
}
