 using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NHibernate.Type;

namespace softwrench.sw4.user.classes.entities {
    public class UserCreationTypeConverter :EnumStringType<UserCreationType>{
    }
}
