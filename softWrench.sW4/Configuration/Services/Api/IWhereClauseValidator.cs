using JetBrains.Annotations;
using softWrench.sW4.Configuration.Definitions.WhereClause;
using IComponent = cts.commons.simpleinjector.IComponent;

namespace softWrench.sW4.Configuration.Services.Api {
    public interface IWhereClauseValidator : IComponent {
        bool DoesValidate([NotNull] string applicationName, WhereClauseCondition conditionToValidateAgainst = null);

        void Validate([NotNull] string applicationName, [NotNull] string whereClause, WhereClauseCondition conditionToValidateAgainst = null);
    }
}
