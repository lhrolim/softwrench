using System;
using cts.commons.persistence;
using cts.commons.persistence.Util;
using NHibernate.Mapping.Attributes;

namespace softwrench.sw4.user.classes.entities {

    [Class(Table = "SW_USER_CUSTOMDATACONSTRAINT")]
    public class UserCustomConstraint : IBaseEntity {
        [Id(0, Name = "Id")]
        [Generator(1, Class = "native")]
        public virtual int? Id { get; set; }

        [ManyToOne(Column = "constraint_id", OuterJoin = OuterJoinStrategy.False, Lazy = Laziness.False)]
        public virtual DataConstraint Constraint { get; set; }

        [Property(TypeType = typeof(BooleanToIntUserType))]
        public virtual Boolean Exclusion { get; set; }

    }
}
