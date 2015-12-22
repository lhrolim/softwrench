using cts.commons.web.Formatting;
using Newtonsoft.Json.Serialization;
using System.Net.Http.Formatting;
using Newtonsoft.Json;

namespace softWrench.sW4.Web.Formatting {
    public class ResponseJsonFormatter : JsonMediaTypeFormatter, ISWJsonFormatter {

        public ResponseJsonFormatter() {
            SerializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();
            SerializerSettings.Converters.Add(new JsonDateTimeConverter());
            SerializerSettings.NullValueHandling = NullValueHandling.Ignore;
        }
    }
}