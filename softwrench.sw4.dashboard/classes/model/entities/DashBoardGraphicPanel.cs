using System.Collections.Generic;
using System.Collections.ObjectModel;
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

        private readonly IDictionary<string, string> _config = new Dictionary<string, string>();

        [Key(-1, Column = "GPID")]
        public virtual int? GpId { get; set; }

        [Property]
        public string Provider { get; set; }

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
                return _config.Any() ? string.Join(";", _config.Select(entry => entry.Key + "=" + entry.Value).ToList()) : null;
            }
            set {
                if (string.IsNullOrEmpty(value)) return;
                value.Split(';')
                    .Select(config => {
                        var entry = config.Split('=');
                        return new KeyValuePair<string, string>(entry[0], entry[1]);
                    })
                    .ForEach(entry => _config[entry.Key] = entry.Value);
            }
        }

        public void PutConfiguration(string key, string value) {
            _config[key] = value;
        }

        public string ConfigurationValue(string key) {
            string value;
            return _config.TryGetValue(key, out value) ? value : null;
        }

        /// <summary>
        /// READONLY dictionary representation of this entity's <see cref="Configuration"/>
        /// </summary>
        public IDictionary<string, string> ConfigurationDictionary {
            get { return new ReadOnlyDictionary<string, string>(_config); }
        }

    }
}
