using softWrench.sW4.Metadata.Security;
using cts.commons.simpleinjector;
using softwrench.sw4.user.classes.entities;

namespace softWrench.sW4.Security.Interfaces {
    public interface IRoleEvaluator : IComponent {

        /// <summary>
        /// the role name this evaluator refers to
        /// </summary>
        string RoleName { get; }

        Role Eval(InMemoryUser user);

    }
}
