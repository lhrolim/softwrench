using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using softWrench.sW4.Data.API.Composition;

namespace softWrench.sW4.Data.API.Tab {
    public class TabRequestWrapperDTO {

        public string Application { get; set; }

        public TabDetailRequest Request { get; set; }

        public JObject Data { get; set; }
    }
}
