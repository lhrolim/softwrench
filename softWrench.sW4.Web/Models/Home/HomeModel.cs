using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using softWrench.sW4.Metadata.Security;

namespace softWrench.sW4.Web.Models.Home {
    public class HomeModel :IBaseLayoutModel{

        public string Url { get; set; }
        public string Title { get; set; }
        public bool HasPopupLogo { get; set; }
        public string ConfigJSON { get; set; }

        public string UserJSON { get; set; }

        public string I18NJsons { get; set; }

        public string StatusColorJson { get; set; }

        public string StatusColorFallbackJson { get; set; }

        public string ClassificationColorJson { get; set; }

        public string Message { get; set; }
        public string WindowTitle { get; set; }

        public string ClientName { get; set; }

        public long InitTimeMillis { get; set; }

        public string MenuJSON { get; set; }


        public HomeModel(string url, string title, HomeConfigs configs, MenuModel MenuModel, InMemoryUser user, bool hasLogoPopup,
            JObject i18NJsons, JObject statusColorJson, JObject statusColorFallbackJson, JObject classificationColorJson, string clientName, string windowTitle = null, string message = null) {
            Url = url;
            InitTimeMillis = configs.InitTimeMillis;
            Title = title;
            ClientName = clientName;

            ConfigJSON = JsonConvert.SerializeObject(configs, Newtonsoft.Json.Formatting.None,
            new JsonSerializerSettings() {
                ContractResolver = new CamelCasePropertyNamesContractResolver()
            });

            UserJSON = JsonConvert.SerializeObject(user, Newtonsoft.Json.Formatting.None,
            new JsonSerializerSettings() {
                ContractResolver = new CamelCasePropertyNamesContractResolver()
            });

            if (MenuModel != null) {
                MenuJSON = JsonConvert.SerializeObject(MenuModel, Newtonsoft.Json.Formatting.None,
                    new JsonSerializerSettings() {
                        ContractResolver = new CamelCasePropertyNamesContractResolver()
                    });
            }

            HasPopupLogo = hasLogoPopup;
            I18NJsons = i18NJsons.ToString(Newtonsoft.Json.Formatting.Indented);
            if (statusColorJson != null) {
                StatusColorJson = statusColorJson.ToString(Newtonsoft.Json.Formatting.Indented);
            }

            if (statusColorFallbackJson != null) {
                StatusColorFallbackJson = statusColorFallbackJson.ToString(Newtonsoft.Json.Formatting.Indented);
            }
            
            if (classificationColorJson != null) {
                ClassificationColorJson = classificationColorJson.ToString(Newtonsoft.Json.Formatting.Indented);
            }

            WindowTitle = windowTitle;
            Message = message;
        }


    }
}