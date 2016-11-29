using JetBrains.Annotations;
using Newtonsoft.Json.Linq;
using softWrench.sW4.Security.Services;
using cts.commons.simpleinjector;
using cts.commons.simpleinjector.Events;
using System;
using System.Linq;
using System.Collections.Generic;
using System.IO;
using log4net;
using Newtonsoft.Json;

namespace softWrench.sW4.Util {
    public class StatusColorResolver : ISingletonComponent, ISWEventListener<ClearCacheEvent> {

        private static readonly ILog Log = LogManager.GetLogger(typeof(StatusColorResolver));

        private const string PathPattern = "{0}App_Data\\Client\\{1}\\statuscolors.json";

        private const string FallbackPathPattern = "{0}App_Data\\Client\\@internal\\fallback\\{1}";

        private const string TestI18NdirPattern = "{0}\\Client\\{1}\\statuscolors.json";

        private JObject _cachedCatalogs;
        
        private Boolean _cacheSet = false;
        
        public StatusColorResolver() {
        }

        public virtual JObject FetchFallbackCatalogs() {
            var statusColorJsonPath = String.Format(FallbackPathPattern, AppDomain.CurrentDomain.BaseDirectory, "statuscolorsfallback.json");

            if (File.Exists(statusColorJsonPath)) {
                JObject statusColorJson;
                try {
                    statusColorJson = JSonUtil.BuildJSon(statusColorJsonPath);
                    ReplaceWithColorValues(statusColorJson);
                } catch (Exception) {
                    Log.Error("Error reading Status Color fallback JSON");
                    statusColorJson = new JObject();
                }

                return  statusColorJson;
            }

            return null;
        }
        
        public virtual JObject FetchCatalogs() {
            if (_cacheSet && !ApplicationConfiguration.IsDev() && !ApplicationConfiguration.IsUnitTest) {
                //we won´t cache it on dev, because it would be boring on the development process
                return _cachedCatalogs;
            }

            var currentClient = ApplicationConfiguration.ClientName;
            var baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
            var patternToUse = ApplicationConfiguration.IsUnitTest ? TestI18NdirPattern : PathPattern;
            var statusColorJsonPath = String.Format(patternToUse, baseDirectory, currentClient);

            if (File.Exists(statusColorJsonPath)) {
                JObject statusColorJson;
                try {
                    statusColorJson = JSonUtil.BuildJSon(statusColorJsonPath);
                    ReplaceWithColorValues(statusColorJson);
                } catch (Exception) {
                    Log.Error("Error reading Status Color JSON");
                    statusColorJson = new JObject();
                }

                _cachedCatalogs = statusColorJson;                
            } else {
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
        }


        public void HandleEvent(ClearCacheEvent eventToDispatch) {
            if (_cacheSet && _cachedCatalogs != null) {
                ClearCache();
            }
        }

        public Dictionary<string, string> GetColorsAsDict([NotNull] string applicationId, bool forceFallback = false) {            
            var _cachedColorDict = new Dictionary<string, Dictionary<string, string>>();

            JObject catalogs = FetchCatalogs();
            
            if(catalogs == null || forceFallback) {
                catalogs = FetchFallbackCatalogs();
            }
            
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
            
            return _cachedColorDict.ContainsKey(applicationId) ? _cachedColorDict[applicationId] : 
                (_cachedColorDict.ContainsKey("default") ? _cachedColorDict["default"] : null);
        }

        private void ReplaceWithColorValues(JObject statusColorJson) {
            using (var stream = new StreamReader(String.Format(FallbackPathPattern, AppDomain.CurrentDomain.BaseDirectory, "statuscolorvalues.json"))) {
                if (stream != null) {
                    Dictionary<string, string> colors = JsonConvert.DeserializeObject<Dictionary<string, string>>(stream.ReadToEnd());

                    foreach (JProperty property in statusColorJson.Properties()) {
                        var value = statusColorJson[property.Name];

                        var newTokenDictionary = new Dictionary<string, string>();
                        foreach (var val in value) {
                            var name = ((JProperty)val).Name;
                            var data = val.First.Value<string>();

                            if (!data.StartsWith("#") && colors.ContainsKey(data)) {
                                //rewite the value
                                data = colors[data];
                            }

                            newTokenDictionary.Add(name.ToLower(), data);
                        }

                        value.Replace(JToken.Parse(JsonConvert.SerializeObject(newTokenDictionary)));
                    }
                }
            }
        }
    }
}
