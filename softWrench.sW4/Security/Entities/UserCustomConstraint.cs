using NHibernate.Mapping.Attributes;
using softWrench.sW4.Security.Interfaces;
using softWrench.sW4.Util;
using System;

namespace softWrench.sW4.Security.Entities {

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
