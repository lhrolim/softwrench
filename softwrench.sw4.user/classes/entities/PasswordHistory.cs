using System;
using cts.commons.persistence;
using NHibernate.Mapping.Attributes;

namespace softwrench.sw4.user.classes.entities {

    [Class(Table = "USER_PASSHISTORY", Lazy = false)]
    public class PasswordHistory : IBaseEntity {

        public const string ByUserDesc = "from PasswordHistory where UserId=? order by RegisterTime desc";

        [Id(0, Name = "Id")]
        [Generator(1, Class = "native")]
        public virtual int? Id {
            get; set;
        }

        [Property]
        public virtual string Password {
            get; set;
        }

        [Property]
        public virtual int UserId {
            get; set;
        }

        [Property]
        public virtual DateTime? RegisterTime {
            get; set;
        }

    }
}
