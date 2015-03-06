using cts.commons.web.Formatting;
using Newtonsoft.Json.Serialization;
using softWrench.sW4.Util;
using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Threading.Tasks;

namespace softWrench.sW4.Web.Formatting {
    public class ResponseJsonFormatter : JsonMediaTypeFormatter, ISWJsonFormatter {

        public ResponseJsonFormatter() {
            SerializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();
            SerializerSettings.Converters.Add(new JsonDateTimeConverter());
        }
    }
}