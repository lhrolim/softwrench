using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using cts.commons.persistence;
using NHibernate.Mapping.Attributes;

namespace softwrench.sw4.user.classes.entities {

    [Class(Table = "USER_AUTHATTEMPT", Lazy = false)]
    public class AuthenticationAttempt : IBaseEntity
    {

        public const string ByUser = "from AuthenticationAttempt where UserId=?";

        [Id(0, Name = "Id")]
        [Generator(1, Class = "native")]
        public int? Id {
            get; set;
        }

        [Property]
        public DateTime? RegisterTime {
            get; set;
        }
        [Property]
        public int NumberOfAttempts {
            get; set;
        }
        [Property]
        public int GlobalNumberOfAttempts {
            get; set;
        }

        [Property]
        public int? UserId {
            get; set;
        }


        public bool IsValid() {
            return UserId != null && RegisterTime != null;
        }

        public bool IsPristine() {
            return NumberOfAttempts == 0 && GlobalNumberOfAttempts == 0;
        }

        public void Clear() {
            NumberOfAttempts = 0;
            GlobalNumberOfAttempts = 0;
            RegisterTime = null;
        }
    }
}
