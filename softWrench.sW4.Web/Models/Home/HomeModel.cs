using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.Office.Interop.Excel;
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

        public string InitialModule { get; set; }

        public string I18NJsons { get; set; }
        public string Message { get; set; }
        public string MessageType { get; set; }
        public string WindowTitle { get; set; }

        public long InitTimeMillis { get; set; }
        public bool Allowed { get; set; }

        public HomeModel(string url, string title, HomeConfigs configs, InMemoryUser user, bool hasLogoPopup, 
            JObject i18NJsons, string clientName,string initialModule, string windowTitle = null, string message = null, string messageType = null) {
            Allowed = true;
            Url = url;
            InitTimeMillis = configs.InitTimeMillis;
            Title = title;
            InitialModule = initialModule;
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
            WindowTitle = windowTitle;
            Message = message;
            MessageType = messageType;
        }
        

    }
}