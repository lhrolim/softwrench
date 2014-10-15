using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using softWrench.sW4.Security.Entities;

namespace softWrench.sW4.Security.Interfaces {
    interface IRoleEvaluator {

        /// <summary>
        /// the role name this evaluator refers to
        /// </summary>
        string RoleName { get; }

        RoleEvalResult Eval();

    }
}
