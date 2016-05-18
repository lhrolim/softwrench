using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using cts.commons.portable.Util;
using softWrench.sW4.Util;

namespace softWrench.sW4.Metadata.Properties {
    public class MetadataProperties {

        private const string PropertyNotFound = "property {0} not found for client {1} environment {2}";

        public const string MaximoDBProvider = "maximo_DB_provider";
        public const string MaximoDBUrl = "maximo_DB_url";
        public const string SWDBUrl = "swdb_url";
        public const string SWDBProvider = "swdb_provider";
        public const string MaximoSiteIds = "maximo_siteids";
        public const string Target = "targetmapping";
        public const string Source = "sourcemapping";
        public const string MaximoTimezone = "maximoutc";
        public const string I18NRequiredConst = "i18nrequired";

        private readonly IDictionary<string, string> _globalProperties = new Dictionary<string, string>();
        private readonly IList<EnvironmentProperties> _envProperties = new List<EnvironmentProperties>();


        public MetadataProperties() {
        }

        public MetadataProperties(IDictionary<string, string> properties, IList<EnvironmentProperties> environments) {
            _globalProperties = properties;
            _envProperties = environments;

        }

        public void ValidateRequiredProperties() {
            GlobalProperty(Source, true);
            GlobalProperty(MaximoDBProvider, true);
            GlobalProperty(MaximoDBUrl, true);
            GlobalProperty(SWDBUrl, true);
            GlobalProperty(SWDBProvider, true);
        }

        public IDictionary<string, string> Properties {
            get {
                return _globalProperties;
            }
        }

        

        /// <summary>
        /// 
        /// </summary>
        /// <param name="key"></param>
        /// <param name="throwException"></param>
        /// <param name="testRequired"></param>
        /// <param name="fixedProfile">we need to fallback devpr profiles to dev, except when this flag is true </param>
        /// <returns></returns>
        public string GlobalProperty(string key, bool throwException = false, bool testRequired = false, bool fixedProfile = false) {
            string value;
            var profile = ApplicationConfiguration.Profile;
            if (ApplicationConfiguration.IsDevPR() && !fixedProfile) {
                profile = "dev";
            }

            //First web.config
            var configValue = ConfigurationManager.AppSettings[key];
            if (configValue != null) {
                return configValue;
            }

            //Then environment specific
            var env = _envProperties.FirstOrDefault(e => e.Key == profile);
            if (env != null) {
                if (env.Properties.TryGetValue(key, out value)) {
                    return value;
                }
            }

            //Last global
            if (!Properties.TryGetValue(key, out value) && throwException) {
                if (ApplicationConfiguration.IsUnitTest && !testRequired) {
                    return null;
                }
                throw new InvalidOperationException(String.Format(PropertyNotFound, key, ApplicationConfiguration.ClientName, profile));
            }
            return value;
        }


        public string MaximoTimeZone() {
            return GlobalProperty(MaximoTimezone);
        }

        public Boolean I18NRequired() {
            var i18NRequired = GlobalProperty(I18NRequiredConst);
            return "true".Equals(i18NRequired, StringComparison.CurrentCultureIgnoreCase);
        }

        public string MultiTenantQueryConstraint() {
            return GlobalProperty("multitenantprefix");
        }

        /// <summary>
        /// Override any property declared on the properties.xml of the given customer with the ones eventually present on the "local" properties.xml
        /// 
        /// In Summary Local properties has precedence over the customer
        ///  
        /// </summary>
        /// <param name="localProperties"></param>
        public void MergeWithLocal(MetadataProperties localProperties) {
            foreach (var property in localProperties.Properties) {
                Properties[property.Key] = property.Value;
            }
            foreach (var localEnvironment in localProperties._envProperties) {
                var customerEnvironment = _envProperties.FirstOrDefault(f => f.Key.EqualsIc(localEnvironment.Key));
                if (customerEnvironment == null) {
                    _envProperties.Add(localEnvironment);
                } else {
                    foreach (var property in localEnvironment.Properties) {
                        customerEnvironment.Properties[property.Key] = property.Value;
                    }
                }
            }
        }
    }
}
