using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using softWrench.sW4.Metadata.Security;
using softWrench.sW4.Util;

namespace softWrench.sW4.Web.Models.Home {
    public class HomeModel {

        public string Url { get; set; }
        public string Title { get; set; }
        public bool HasPopupLogo { get; set; }
        public String ConfigJSON { get; set; }

        public String UserJSON { get; set; }

        public string I18NJsons { get; set; }

        public string StatusColorJson { get; set; }

        public string Message { get; set; }
        public string WindowTitle { get; set; }

        public long InitTimeMillis { get; set; }

        public HomeModel(string url, string title, HomeConfigs configs, InMemoryUser user, bool hasLogoPopup,
            JObject i18NJsons, JObject statusColorJson, string clientName, string windowTitle = null, string message = null) {
            Url = url;
            InitTimeMillis = configs.InitTimeMillis;
            Title = title;
            ConfigJSON = JsonConvert.SerializeObject(configs, Newtonsoft.Json.Formatting.None,
          new JsonSerializerSettings() {
              ContractResolver = new CamelCasePropertyNamesContractResolver()
          });

            UserJSON = JsonConvert.SerializeObject(user, Newtonsoft.Json.Formatting.None,
       new JsonSerializerSettings() {
           ContractResolver = new CamelCasePropertyNamesContractResolver()
       });
            HasPopupLogo = hasLogoPopup;
            I18NJsons = i18NJsons.ToString(Newtonsoft.Json.Formatting.Indented);
            if (statusColorJson != null) {
                StatusColorJson = statusColorJson.ToString(Newtonsoft.Json.Formatting.Indented);
            }
            WindowTitle = windowTitle;
            Message = message;
        }


    }
}