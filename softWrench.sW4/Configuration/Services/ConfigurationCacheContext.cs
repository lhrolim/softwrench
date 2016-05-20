using softWrench.sW4.Configuration.Definitions;

namespace softWrench.sW4.Configuration.Services {
    public class ConfigurationCacheContext {

        /// <summary>
        /// An already searched definition. Usable on cases of cache not used to avoid to search the definition again.
        /// </summary>
        public PropertyDefinition PreDefinition { get; set; }

        /// <summary>
        /// If the lookup should ignore the cache, if true the cache is not used or updated.
        /// </summary>
        public bool IgnoreCache { get; set; }
    }
}
