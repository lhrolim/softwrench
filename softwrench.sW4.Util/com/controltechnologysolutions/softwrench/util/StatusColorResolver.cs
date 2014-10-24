using Newtonsoft.Json.Linq;
using softWrench.sW4.Security.Services;
using softWrench.sW4.SimpleInjector;
using softWrench.sW4.SimpleInjector.Events;
using System;
using System.IO;

namespace softWrench.sW4.Util {
    public class StatusColorResolver : ISingletonComponent, ISWEventListener<ClearCacheEvent> {

        private const string PathPattern = "{0}App_Data\\Client\\{1}\\statuscolors.json";

        private const string TestI18NdirPattern = "{0}\\Client\\{1}\\statuscolors.json";

        private JObject _cachedCatalogs;

        private Boolean _cacheSet =false;

        public JObject FetchCatalogs() {
            if (_cacheSet && !ApplicationConfiguration.IsDev() && !ApplicationConfiguration.IsUnitTest) {
                //we won´t cache it on dev, because it would be boring on the development process
                return _cachedCatalogs;
            }


            var currentClient = ApplicationConfiguration.ClientName;
            var baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
            var patternToUse = ApplicationConfiguration.IsUnitTest ? TestI18NdirPattern : PathPattern;
            var statusColorJsonPath = String.Format(patternToUse, baseDirectory, currentClient);
            if (File.Exists(statusColorJsonPath)) {
                _cachedCatalogs = JSonUtil.BuildJSon(statusColorJsonPath);
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
    }
}
