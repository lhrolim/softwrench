﻿using System;
using cts.commons.persistence;
using NHibernate.Mapping.Attributes;
using softwrench.sw4.user.classes.entities;

namespace softWrench.sW4.Preferences {

    [Class(Table = "PREF_GRIDFILTERASSOCIATION", Lazy = false)]
    public class GridFilterAssociation : IBaseEntity {

        public const string ByUserId = "from GridFilterAssociation where User.id =?";
        public const string ByUserIdAndFilter = "from GridFilterAssociation where User.id =? and Filter.id =?";
        public const string CountByFilter = "select count(*) from GridFilterAssociation where Filter.id =?";
        
        

        [Id(0, Name = "Id")]
        [Generator(1, Class = "native")]
        public virtual int? Id { get; set; }

        [ManyToOne(Column = "user_id", OuterJoin = OuterJoinStrategy.False, Lazy = Laziness.False, Cascade = "none")]
        public virtual User User { get; set; }

        [ManyToOne(Column = "gridfilter_id", OuterJoin = OuterJoinStrategy.False, Lazy = Laziness.False, Cascade = "save-update")]
        public virtual GridFilter Filter { get; set; }

        [Property]
        public virtual DateTime? JoiningDate { get; set; }

        

        protected bool Equals(GridFilterAssociation other) {
            return Equals(User, other.User) && Equals(Filter, other.Filter);
        }

        public override bool Equals(object obj) {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((GridFilterAssociation)obj);
        }

        public override int GetHashCode() {
            unchecked {
                return ((User != null ? User.GetHashCode() : 0) * 397) ^ (Filter != null ? Filter.GetHashCode() : 0);
            }
        }

        public override string ToString() {
            return string.Format("User: {0}, Filter: {1}", User, Filter);
        }
    }
}