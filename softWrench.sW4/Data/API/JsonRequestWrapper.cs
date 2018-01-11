using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace softWrench.sW4.Data.API {
    public class JsonRequestWrapper
    {

        public OperationDataRequest RequestData { get; set; }

        public JObject Json { get; set; }

        /// <summary>
        /// Optional parameter for anonymous calls where this value would be signed at the backend for consistency checking
        /// </summary>
        public string MessageToSign { get; set; }

        /// <summary>
        /// The signature sent to be regenerated and validated
        /// </summary>
        public string HashSignature { get; set; }


    }
}
