using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using NHibernate.Mapping.Attributes;
using WebGrease.Css.Extensions;

namespace softwrench.sw4.dashboard.classes.model.entities {
    [JoinedSubclass(
        NameType = typeof(DashboardGraphicPanel),
        Lazy = false,
        ExtendsType = typeof(DashboardBasePanel),
        Table = "DASH_GRAPHICPANEL")]
    public class DashboardGraphicPanel : DashboardBasePanel {

        [Key(-1, Column = "GPID")]
        public virtual int? GpId { get; set; }

        [Property]
        public string Provider { get; set; }

        public IDictionary<string, string> ConfigurationDictionary { get; set; }

        public DashboardGraphicPanel() : base() {
            ConfigurationDictionary = new Dictionary<string, string>();
        }

        /// <summary>
        /// Configuration Dictionary as single string in the form "key_1=value_1;key_2=value_2;...;key_N=value_N".
        /// <para>
        /// Intended to be used by DataBase engine. Use <see cref="PutConfiguration"/>, 
        /// <see cref="ConfigurationValue"/> or <see cref="ConfigurationDictionary"/> to operate on it as dictionary instead.
        /// </para>
        /// </summary>
        [Property]
        [JsonIgnore]
        public string Configuration {
            get {
                return ConfigurationDictionary.Any() ? string.Join(";", ConfigurationDictionary.Select(entry => entry.Key + "=" + entry.Value).ToList()) : null;
            }
            set {
                if (string.IsNullOrEmpty(value)) return;
                value.Split(';')
                    .Select(config => {
                        var entry = config.Split('=');
                        return new KeyValuePair<string, string>(entry[0], entry[1]);
                    })
                    .ForEach(entry => ConfigurationDictionary[entry.Key] = entry.Value);
            }
        }

        public DashboardGraphicPanel PutConfiguration(string key, string value) {
            ConfigurationDictionary[key] = value;
            return this;
        }

        public string ConfigurationValue(string key) {
            string value;
            return ConfigurationDictionary.TryGetValue(key, out value) ? value : null;
        }

    }
}
