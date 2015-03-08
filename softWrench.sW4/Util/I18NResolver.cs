using JetBrains.Annotations;
using Newtonsoft.Json.Linq;
using softwrench.sW4.Shared2.Metadata.Applications.Schema;
using softWrench.sW4.Security.Services;
using cts.commons.simpleinjector;
using System;
using System.IO;
using System.Linq;

namespace softWrench.sW4.Util {

    public class I18NResolver : ISingletonComponent {

        private JObject _cachedCatalogs;

        private JToken _jObject;

        private const string I18NdirPattern = "{0}App_Data\\Client\\{1}\\i18n";

        private const string TestI18NdirPattern = "{0}\\Client\\{1}\\i18n";


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
                        resultCatalogs.Add(language, JSonUtil.BuildJSon(files[0]));
                    }
                }
            }
            _cachedCatalogs = resultCatalogs;
            return _cachedCatalogs;
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



        public string I18NValue([NotNull] string key, [NotNull] string defaultValue, object[] parameters = null) {
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
                        return result;
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