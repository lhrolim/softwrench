using JetBrains.Annotations;
using softwrench.sw4.api.classes.fwk.context;
using softWrench.sW4.Metadata.Security;

namespace softWrench.sW4.Security.Context {
    public interface IContextLookuper : IMemoryContextLookuper {
        /// <summary>
        /// Retrieves a context for the current thread invocation.
        /// The <see cref="ContextHolder"/> data is treated internally as an immutable object.
        /// For updating the context it is required to add it again after it's been updated with <see cref="AddContext"/>.
        /// <example>
        /// <code>
        /// var context = lookuper.LookupContext();
        /// context.OfflineMode = true;
        /// lookuper.AddContext(context);
        /// </code>
        /// </example>
        /// </summary>
        /// <returns></returns>
        ContextHolder LookupContext();

        /// <summary>
        /// Fills current thread invocation context, specifically related to grid query invocations
        /// </summary>
        /// <param name="applicationName"></param>
        /// <param name="user"></param>
        void FillGridContext([NotNull]string applicationName, [NotNull]InMemoryUser user);

        /// <summary>
        /// Sets current thread invocation context.
        /// </summary>
        /// <param name="holder"></param>
        /// <returns></returns>
        ContextHolder AddContext(ContextHolder holder);

    }
}