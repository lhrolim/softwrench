using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using softWrench.sW4.Security.Entities;

namespace softWrench.sW4.Security.Setup {
    class UserLinkManager : IUserLinkManager {

        public string GenerateTokenLink(User user) {
            return "xxx";
        }

        public User RetrieveUserByLink(string link) {
            return new User {
                FirstName = "Luiz",
                LastName = "Rolim"
            };
        }

        public void DeleteLink(User user) {
            //
        }
    }
}
