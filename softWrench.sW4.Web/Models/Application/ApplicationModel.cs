using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using softWrench.sW4.Data.API.Response;
using softWrench.sW4.Web.Models.Generic;

namespace softWrench.sW4.Web.Models.Application {
    public class ApplicationModel : GenericModel {

      
        public ApplicationModel(string application, string schema, string mode, string title, IApplicationResponse responseData)
            : base(title, "/Content/Controller/Application.html",responseData)
        {

            var internalData = new ApplicationInternalData(application, schema, mode);


            InitialMetadataJSON = JsonConvert.SerializeObject(internalData, Newtonsoft.Json.Formatting.None,

                    new JsonSerializerSettings() {
                        ContractResolver = new CamelCasePropertyNamesContractResolver()
                    });

        }

      

        class ApplicationInternalData {
            public ApplicationInternalData(string application, string schema, string mode) {
                Application = application;
                Schema = schema;
                Mode = mode;
            }

            public string Application { get; set; }
            public string Schema { get; set; }
            public string Mode { get; set; }

        }

       
    }
}