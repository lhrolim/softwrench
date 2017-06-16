using System.Collections.Generic;
using cts.commons.simpleinjector;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using softWrench.sW4.Util;

namespace softwrench.sw4.offlineserver.services.util {
    public class AppConfigurationProvider : ISingletonComponent {

        private readonly StatusColorResolver _statusColorResolver;

        public AppConfigurationProvider(StatusColorResolver statusColorResolver) {
            _statusColorResolver = statusColorResolver;
        }

        /// <summary>
        /// </summary>
        /// <returns>JObject containing the offline app's configurations</returns>
        public JObject AppConfig() {
            var config = BaseConfig();
            var statusColor = _statusColorResolver.FetchCatalogs() ?? _statusColorResolver.FetchFallbackCatalogs();
            statusColor = statusColor ?? new JObject();
            config.Add("statuscolor", statusColor);
            return config;
        }

        private JObject BaseConfig() {
            var serverConfig = new JObject() {
                {"environment", ApplicationConfiguration.Profile},
                {"version", ApplicationConfiguration.SystemVersion},
                {"client", ApplicationConfiguration.ClientName}
            };
            return new JObject() {
                { "serverconfig", serverConfig }
            };
        }
    }
}
