using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using cts.commons.persistence;
using NHibernate.Mapping.Attributes;

namespace softWrench.sW4.Email {
    [Class(Table = "EMAIL_HISTORY", Lazy = false)]
    class Email : IBaseEntity  {

        [Id(0, Name = "Id")]
        [Generator(1, Class = "native")]
        public virtual int? Id { get; set; }

        [Property]
        public virtual string UserID { get; set; }

        [Property]
        public virtual string EmailAddress { get; set; }

        public Email() {
            
        }

        public Email(int? id, string userid, string emailaddress) {
            Id = id;
            UserID = userid;
            EmailAddress = emailaddress;
        }
    }
}
