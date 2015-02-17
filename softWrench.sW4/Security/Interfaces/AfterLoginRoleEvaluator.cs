using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using softWrench.sW4.Metadata.Security;
using softWrench.sW4.Security.Entities;
using softWrench.sW4.Security.Services;
using cts.commons.simpleinjector.Events;
using softWrench.sW4.Util;

namespace softWrench.sW4.Security.Interfaces {

    /// <summary>
    /// An evaluator that should run immediately after the user login
    /// </summary>
    public abstract class AfterLoginRoleEvaluator : IRoleEvaluator, ISWEventListener<UserLoginEvent> {
        public abstract string RoleName { get; }
        public abstract Role Eval(InMemoryUser user);
        public void HandleEvent(UserLoginEvent userLoginEvent) {
            var user = userLoginEvent.InMemoryUser;
            var result = Eval(user);
            if (result == null) {
                return;
            }
            var roles = user.Roles;
            if (user.IsInRole(result.Name)) {
                //the user already has this role for some crazy reason... let´s make sure to clean it and place the dynamic evaluator result.
                var oldRole = roles.First(r => r.Name.EqualsIc(result.Name));
                roles.Remove(oldRole);
            }
            roles.Add(result);
        }
    }
}
