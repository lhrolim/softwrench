using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace softWrench.sW4.Web.Models.Generic {
    public class GenericModel {

        public string InitialDataJSON { get; set; }

        public string InitialMetadataJSON { get; set; }

        private readonly string _title;

        private readonly string _includeURL;

        public GenericModel(string title, string includeURL) {
            _title = title;
            _includeURL = includeURL;
        }

        public GenericModel(string title, string includeURL,object responseObject) {
            _title = title;
            _includeURL = includeURL;
            if (responseObject != null) {
                InitialDataJSON = JsonConvert.SerializeObject(responseObject, Newtonsoft.Json.Formatting.None,

                    new JsonSerializerSettings() {
                        ContractResolver = new CamelCasePropertyNamesContractResolver()
                    });
            }
        }


        public GenericModel(string title, string includeURL, string dataJSON, string metadataJSON = null) {
            _title = title;
            _includeURL = includeURL;
            InitialDataJSON = dataJSON;
            InitialMetadataJSON = metadataJSON;
        }


        public string Title {
            get { return _title; }
        }

        public string IncludeURL {
            get { return _includeURL; }
        }


      
    }
}