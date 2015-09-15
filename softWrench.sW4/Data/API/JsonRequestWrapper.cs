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



    }
}
