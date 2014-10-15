using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using softWrench.sW4.Metadata.Security;
using softWrench.sW4.Security.Entities;
using softWrench.sW4.SimpleInjector;

namespace softWrench.sW4.Security.Interfaces {
    public interface IRoleEvaluator : IComponent {

        /// <summary>
        /// the role name this evaluator refers to
        /// </summary>
        string RoleName { get; }

        Role Eval(InMemoryUser user);

    }
}
