    using JetBrains.Annotations;

namespace cts.commons.simpleinjector {
    public interface IClientApplicationAwareComponent : IClientAwareComponent {


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