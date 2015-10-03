using softwrench.sW4.Shared2.Metadata.Applications.Schema;
using cts.commons.simpleinjector;
using JetBrains.Annotations;
using softwrench.sw4.api.classes.fwk.context;
using softWrench.sW4.Metadata.Security;

namespace softWrench.sW4.Security.Context {
    public interface IContextLookuper : IMemoryContextLookuper {
        /// <summary>
        /// Retrieves a context for the current thread invocation
        /// </summary>
        /// <returns></returns>
        ContextHolder LookupContext();

        /// <summary>
        /// Fills current thread invocation context, specifically related to grid query invocations
        /// </summary>
        /// <param name="applicationName"></param>
        /// <param name="user"></param>
        void FillGridContext([NotNull]string applicationName, [NotNull]InMemoryUser user);

    }
}