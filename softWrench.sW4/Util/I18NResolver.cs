using JetBrains.Annotations;
using log4net;
using Newtonsoft.Json.Linq;
using softwrench.sW4.Shared2.Metadata.Applications.Schema;
using softWrench.sW4.Security.Services;
using softWrench.sW4.SimpleInjector;
using System;
using System.IO;
using System.Linq;
using System.Text;

namespace softWrench.sW4.Util {

    public class I18NResolver : ISingletonComponent {

        private JObject _cachedCatalogs;

        private JToken _jObject;

        private const string I18NdirPattern = "{0}App_Data\\Client\\{1}\\i18n";

        private const string TestI18NdirPattern = "{0}\\Client\\{1}\\i18n";


        private static readonly ILog Log = LogManager.GetLogger(typeof(I18NResolver));

        public JObject FetchCatalogs() {
            if (_cachedCatalogs != null && !ApplicationConfiguration.IsDev() && !ApplicationConfiguration.IsUnitTest) {
                //we won´t cache it on dev, because it would be boring on the development process
                return _cachedCatalogs;
            }

            var resultCatalogs = new JObject();
            var currentClient = ApplicationConfiguration.ClientName;
            var baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
            var patternToUse = ApplicationConfiguration.IsUnitTest ? TestI18NdirPattern : I18NdirPattern;
            var i18NDir = String.Format(patternToUse, baseDirectory, currentClient);
            if (Directory.Exists(i18NDir)) {
                foreach (var l in Directory.GetDirectories(i18NDir)) {
                    var language = l.Replace(i18NDir + "\\", "");
                    var files = Directory.GetFiles(l, "labels.json");
                    if (files.Length != 0) {
                        resultCatalogs.Add(language, BuildJSon(files[0]));
                    }
                }
            }
            _cachedCatalogs = resultCatalogs;
            return _cachedCatalogs;
        }

        private JObject BuildJSon(string filePath) {
            string line;
            var sb = new StringBuilder();
            FileUtils.DoWithLines(filePath, s => NormalizeJson(s, sb));
            var json = "{" + sb + "}";
            Log.Debug("Parsing labels JSON: \n " + json);
            return JObject.Parse(json);
        }

        private static void NormalizeJson(string s, StringBuilder sb) {
            if (String.IsNullOrEmpty(s)) {
                return;
            }

            if ((s.Contains("{") || s.Contains("}")) && !(s.Contains("{") && s.Contains("}"))) {
                //both at same line could indicate parameters {0}
                if (!s.Contains(",") && s.Contains("}")) {
                    //append , to indicate the end of an object
                    sb.AppendLine(s + ",");
                } else {
                    var idx = s.IndexOf(':');
                    if (idx != -1) {
                        var splitObj = new string[2] { s.Substring(0, idx), s.Substring(idx + 1, s.Length - idx - 1) };
                        s = s.Replace(splitObj[0], "'" + splitObj[0].Trim() + "'");
                    }
                    sb.AppendLine(s);
                }
                return;
            }
            var keySeparatorIdx = s.IndexOf(':');
            if (keySeparatorIdx == -1) {
                //no :, could be } or {
                sb.AppendLine(s);
                return;
            }

            var split = new string[2] { s.Substring(0, keySeparatorIdx), s.Substring(keySeparatorIdx + 1, s.Length - keySeparatorIdx - 1) };

            if (split.Count() != 2) {
                sb.AppendLine(s);
                return;
            }
            var key = split[0].Trim();
            key = "'" + key + "'";
            var value = split[1].Trim();
            if (!value.EndsWith(",")) {
                value = "'" + value + "',";
            } else {
                value = "'" + value.Substring(0, value.Length - 1) + "',";
            }
            var result = key + ":" + value;
            sb.AppendLine(result);
        }

        public JObject CachedCatalogs {
            get { return _cachedCatalogs; }
        }

        public string I18NSchemaTitle(ApplicationSchemaDefinition schema) {
            var language = SecurityFacade.CurrentUser().Language;
            return DoGetI18NSchemaTitle(schema, language);
        }

        //for testing purposes
        internal string DoGetI18NSchemaTitle(ApplicationSchemaDefinition schema, String language) {
            if (_cachedCatalogs == null) {
                _cachedCatalogs = FetchCatalogs();
            }
            if (language == null) {
                return schema.Title;
            }
            var catalog = ((JObject)_cachedCatalogs[language]);
            if (catalog == null) {
                //client has no catalog
                return schema.Title;
            }
            var appObject = catalog[schema.ApplicationName];
            if (appObject == null) {
                //default
                return schema.Title;
            }
            var titleObject = appObject["_title"];
            if (titleObject == null) {
                return schema.Title;
            }
            var value = titleObject[schema.SchemaId];
            return value == null ? schema.Title : value.ToString();
        }


        private string ValueConsideringSchema(string value, string schema) {
            if (schema == null || value == null) {
                return value;
            }

            try {
                var jsonobject = JObject.Parse(value);
                var jToken = jsonobject[schema];
                if (jToken != null) {
                    return jToken.ToString();
                }
                return jsonobject["_"].ToString();
            } catch (Exception e) {
                //this was not a json
                return value;
            }
        }



        public string I18NValue([NotNull] string key, [NotNull] string defaultValue, string schema = null, object[] parameters = null) {
            _jObject = null;
            if (key == null) {
                throw new ArgumentNullException("key");
            }
            if (defaultValue == null) {
                throw new ArgumentNullException("defaultValue");
            }
            try {
                var language = SecurityFacade.CurrentUser().Language;
                if (language == null) {
                    return defaultValue;
                }
                var catalog = FetchCatalogs()[language.ToLower()];
                if (catalog == null) {
                    return defaultValue;
                }

                var keys = key.Split('.');
                var lastKey = keys.LastOrDefault();
                foreach (var s in keys) {
                    _jObject = _jObject == null ? catalog[s] : _jObject[s];
                    if (lastKey == s && _jObject != null) {
                        var result = ApplyParameters(_jObject.ToString(), parameters);
                        return ValueConsideringSchema(result, schema);
                    }
                }
                return defaultValue;
            } catch {
                return defaultValue;
            }
        }

        private string ApplyParameters(string toString, object[] parameters) {
            return parameters == null ? toString : String.Format(toString, parameters);
        }

        public void ClearCache() {
            _cachedCatalogs = null;
        }
    }
}