using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using softWrench.sW4.Security.Entities;
using softWrench.sW4.Security.Interfaces;

namespace softwrench.sW4.batches.com.cts.softwrench.sw4.batches.services {
    public class WoBatchesRoleEvaluator : AfterLoginRoleEvaluator {
        public override string RoleName { get { return "allowwobatches"; } }
        public override Role Eval() {
            return new RoleWithErrorMessage(){Name = RoleName,Authorized = false,UnauthorizedMessage = "you don´t have permission to... "};
        }
    }
}
