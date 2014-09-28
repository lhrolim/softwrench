using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using softWrench.sW4.Metadata.Security;

namespace softWrench.sW4.Security.Services {
    public class UserLoginEvent {

        public readonly InMemoryUser InMemoryUser;

        public UserLoginEvent(InMemoryUser inMemoryUser) {
            InMemoryUser = inMemoryUser;
        }



        public bool AllowMultiThreading { get { return false; } }
    }
}
