using Newtonsoft.Json.Serialization;
using softWrench.sW4.Util;
using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Threading.Tasks;

namespace softWrench.sW4.Web.Formatting {
    public class ResponseJsonFormatter : JsonMediaTypeFormatter {

        public ResponseJsonFormatter() : base() {
            this.SerializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();
            this.SerializerSettings.Converters.Add(new JsonDateTimeConverter());
        }
    }
}