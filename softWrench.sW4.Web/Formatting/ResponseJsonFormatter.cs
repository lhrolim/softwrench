using cts.commons.web.Formatting;
using System.Net.Http.Formatting;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace softWrench.sW4.Web.Formatting {
    public class ResponseJsonFormatter : JsonMediaTypeFormatter, ISWJsonFormatter {

        public ResponseJsonFormatter() {
            // SerializerSettings.ContractResolver = new LeanJsonContractResolver();
            SerializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();
            SerializerSettings.Converters.Add(new JsonDateTimeConverter());
            SerializerSettings.NullValueHandling = NullValueHandling.Ignore;
            // SerializerSettings.DefaultValueHandling = DefaultValueHandling.Ignore;
        }
    }
}