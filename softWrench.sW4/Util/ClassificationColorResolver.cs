using JetBrains.Annotations;
using Newtonsoft.Json.Linq;
using softWrench.sW4.Security.Services;
using cts.commons.simpleinjector;
using cts.commons.simpleinjector.Events;
using System;
using System.Collections.Generic;
using System.IO;
using log4net;

namespace softWrench.sW4.Util {
    public class ClassificationColorResolver : ISingletonComponent, ISWEventListener<ClearCacheEvent> {

        private static readonly ILog Log = LogManager.GetLogger(typeof(ClassificationColorResolver));

        private const string PathPattern = "{0}App_Data\\Client\\{1}\\classificationcolors.json";

        private const string TestI18NdirPattern = "{0}\\Client\\{1}\\classificationcolors.json";

        private JObject _cachedCatalogs;

        private Boolean _cacheSet = false;

        private Dictionary<string, Dictionary<string, string>> _cachedColorDict;
        private Dictionary<string, string> _defaultColorDict;

        public ClassificationColorResolver() {
            // this should eventually use a config file
            _defaultColorDict = new Dictionary<string, string>() {
                {"acc_cat", "blue"}
            };
        }


        public JObject FetchCatalogs() {
            if (_cacheSet && !ApplicationConfiguration.IsDev() && !ApplicationConfiguration.IsUnitTest) {
                //we won´t cache it on dev, because it would be boring on the development process
                return _cachedCatalogs;
            }


            var currentClient = ApplicationConfiguration.ClientName;
            var baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
            var patternToUse = ApplicationConfiguration.IsUnitTest ? TestI18NdirPattern : PathPattern;
            var jsonPath = String.Format(patternToUse, baseDirectory, currentClient);
            if (File.Exists(jsonPath))
            {
                JObject colorJson;
                try
                {
                    colorJson = JSonUtil.BuildJSon(jsonPath);
                }
                catch (Exception)
                {
                    Log.Error("Error reading Status Color JSON");
                    colorJson = new JObject();
                }
                _cachedCatalogs = colorJson;
            }
            else {
                _cachedCatalogs = null;
            }
            _cacheSet = true;
            return _cachedCatalogs;
        }

        public JObject CachedCatalogs {
            get { return _cachedCatalogs; }
        }

        public void ClearCache() {
            _cachedCatalogs = null;
            _cacheSet = false;
            _cachedColorDict = null;
        }


        public void HandleEvent(ClearCacheEvent eventToDispatch) {
            if (_cacheSet && _cachedCatalogs != null) {
                ClearCache();
            }
        }

        public Dictionary<string, string> GetColorsAsDict([NotNull] string applicationId) {
            if (_cacheSet && !ApplicationConfiguration.IsDev() && !ApplicationConfiguration.IsUnitTest && _cachedColorDict != null) {
                return _cachedColorDict.ContainsKey(applicationId) ? _cachedColorDict[applicationId] : _defaultColorDict;
            }

            _cachedColorDict = new Dictionary<string, Dictionary<string, string>>();
            JObject catalogs = FetchCatalogs();

            if (catalogs == null)
                return _defaultColorDict;

            foreach (var currentToken in catalogs) {

                var application = currentToken.Key;
                var colors = currentToken.Value;

                var colorDict = new Dictionary<string, string>();

                foreach (var color in colors.Value<JObject>().Properties()) {
                    var name = color.Name;
                    var value = color.Value;

                    colorDict.Add(name, value.ToString());
                }

                _cachedColorDict[application] = colorDict;
            }


            return _cachedColorDict.ContainsKey(applicationId) ? _cachedColorDict[applicationId] : null;
        }

        public Dictionary<string, string> GetDefaultColorsAsDict() {
            return _defaultColorDict;
        }
    }
}
