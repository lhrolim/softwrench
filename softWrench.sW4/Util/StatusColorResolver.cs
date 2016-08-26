using JetBrains.Annotations;
using Newtonsoft.Json.Linq;
using softWrench.sW4.Security.Services;
using cts.commons.simpleinjector;
using cts.commons.simpleinjector.Events;
using System;
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

        private Dictionary<string, Dictionary<string, string>> _cachedColorDict;
        private Dictionary<string, string> _defaultColorDict;

        public StatusColorResolver() {
            // this should eventually use a config file
            _defaultColorDict = new Dictionary<string, string>() {
                {"acc_cat", "blue"},
                {"appr", "blue"},
                {"assesses", "blue"}, 
                {"auth", "blue"},
                {"authorized", "blue"},
                {"can", "red"},
                {"cancelled", "red"},
                {"close", "green"},
                {"closed", "green"},
                {"comp", "green"},
                {"draft", "white"},
                {"fail", "red"},
                {"failpir", "red"},
                {"histedit", "green"},
                {"holdinprg", "blue"},
                {"impl", "green"},
                {"inprg", "blue"},
                {"inprog", "yellow"},
                {"new", "orange"},
                {"notreq", "red"},
                {"null", "yellow"},
                {"pending", "yellow"},
                {"planned", "blue"},
                {"queued", "yellow"},
                {"rejected", "red"},
                {"resolvconf", "green"},
                {"resolved", "blue"},
                {"review", "green"},
                {"sched", "blue"},
                {"slahold", "blue"},
                {"wappr", "orange"},
                {"active", "orange"},
                {"cantreprod", "yellow"},
                {"waitoninfo", "yellow"},
                {"wont fix", "red"},
                {"wontimplnt", "red"},
                {"wontrespnd", "red"},
                {"postponed", "red"},
                {"fixed", "blue"},
                {"implemented", "green"},
                {"appfm", "blue"},
                {"applm", "blue"},
                {"by design", "blue"},
                {"duplicate", "blue"},
                {"completed", "green"},
                {"spam", "red"},
                {"wsch", "orange"}
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
            var statusColorJsonPath = String.Format(patternToUse, baseDirectory, currentClient);
            statusColorJsonPath = File.Exists(statusColorJsonPath) ? statusColorJsonPath : String.Format(FallbackPathPattern, baseDirectory, "statuscolors.json");

            if (File.Exists(statusColorJsonPath)) {
                JObject statusColorJson;
                try {
                    statusColorJson = JSonUtil.BuildJSon(statusColorJsonPath);

                    using (var stream = new StreamReader(String.Format(FallbackPathPattern, baseDirectory, "statuscolorvalues.json"))) {
                        if (stream != null) {
                            Dictionary<string, string> colors = JsonConvert.DeserializeObject<Dictionary<string, string>>(stream.ReadToEnd());

                            foreach(JProperty property in statusColorJson.Properties()) {
                                var value = statusColorJson[property.Name];
                                
                                var newTokenDictionary = new Dictionary<string, string>();
                                var hasChanged = false;
                                foreach (var val in value) {
                                    var name = ((JProperty)val).Name;
                                    var data = val.First.Value<string>();

                                    if (!data.StartsWith("#") && colors.ContainsKey(data)) {
                                        //rewite the value
                                        data = colors[data];
                                        hasChanged = true;
                                    }

                                    newTokenDictionary.Add(name, data);
                                }

                                if (hasChanged) {
                                    value.Replace(JToken.Parse(JsonConvert.SerializeObject(newTokenDictionary)));
                                }
                            }
                        }
                    }

                } catch (Exception e) {
                    Log.Error("Error reading Status Color JSON");
                    statusColorJson = new JObject();
                }

                _cachedCatalogs = statusColorJson;
                _cacheSet = true;
            }
            
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


            return _cachedColorDict.ContainsKey(applicationId) ? _cachedColorDict[applicationId] : _cachedColorDict["default"];
        }

        public Dictionary<string, string> GetDefaultColorsAsDict() {
            return _defaultColorDict;
        }
    }
}
