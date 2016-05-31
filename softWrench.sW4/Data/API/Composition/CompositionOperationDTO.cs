using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace softWrench.sW4.Data.API.Composition {
    public class CompositionOperationDTO {

        public string DispatcherComposition {
            get; set;
        }


        public string Id{
            get; set;
        }

        public string Operation {
            get; set;
        }

        public JObject CompositionItem { get; set; }
    }
}
