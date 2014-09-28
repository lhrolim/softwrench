using softWrench.sW4.Security.Context;

namespace softWrench.sW4.Configuration.Util {
    class ConfigCacheKey {
        public string ConfigKey { get; set; }
        public ContextHolder Conditions { get; set; }

        public ConfigCacheKey(string configKey, ContextHolder conditions) {
            ConfigKey = configKey;
            Conditions = conditions;
        }

        protected bool Equals(ConfigCacheKey other) {
            if (this is KeyMatchingConfigCacheKey || other is KeyMatchingConfigCacheKey) {
                return string.Equals(ConfigKey, other.ConfigKey);
            }
            return string.Equals(ConfigKey, other.ConfigKey) && Equals(Conditions, other.Conditions);
        }

        public override bool Equals(object obj) {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            return Equals((ConfigCacheKey)obj);
        }

        public override int GetHashCode() {
            unchecked {
                return ((ConfigKey != null ? ConfigKey.GetHashCode() : 0) * 397);
            }
        }

        public class KeyMatchingConfigCacheKey : ConfigCacheKey {
            public KeyMatchingConfigCacheKey(string configKey)
                : base(configKey, null) {

            }


        }
    }
}
