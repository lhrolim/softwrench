    using JetBrains.Annotations;

namespace cts.commons.simpleinjector {
    public interface IClientApplicationAwareComponent : ISingletonComponent {


        /// <summary>
        /// Comma separated list of clients that implementations of this interface should refer to. If null, any Client would accept it.
        /// </summary>
        /// <returns></returns>
        string ClientFilters();
        /// <summary>
        /// Application which this class refers to
        /// </summary>
        /// <returns></returns>
        [NotNull]
        string ApplicationName();

        /// <summary>
        /// Optional schema that that implementations of this interface refer to.
        /// </summary>
        /// <returns></returns>
        string SchemaName();

    }
}