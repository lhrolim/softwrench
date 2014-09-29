using System;
using NHibernate.Mapping.Attributes;
using softWrench.sW4.Security.Interfaces;

namespace softWrench.sW4.Security.Entities {

    [Class(Table = "SW_USER_CUSTOMDATACONSTRAINT")]
    public class UserCustomConstraint : IBaseEntity {
        [Id(0, Name = "Id")]
        [Generator(1, Class = "native")]
        public virtual int? Id { get; set; }

        [ManyToOne(Column = "constraint_id", OuterJoin = OuterJoinStrategy.False, Lazy = Laziness.False)]
        public virtual DataConstraint Constraint { get; set; }

        [Property]
        public virtual Boolean Exclusion { get; set; }

    }
}
