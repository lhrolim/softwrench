using softwrench.sW4.Shared2.Metadata.Applications.Schema;
using cts.commons.simpleinjector;
using softwrench.sw4.api.classes.fwk.context;

namespace softWrench.sW4.Security.Context {
    public interface IContextLookuper : IMemoryContextLookuper {
        /// <summary>
        /// Retrieves a context for the current thread invocation
        /// </summary>
        /// <returns></returns>
        ContextHolder LookupContext();


    }
}