using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace softWrench.sW4.Data {
    public class JObjectDatamapAdapter : JObject {

        private readonly DataMap _datamap;

        public JObjectDatamapAdapter(DataMap datamap) {
            _datamap = datamap;
        }

        public string GetStringValue(string propertyName) {
            return _datamap.GetStringAttribute(propertyName);
        }

    }
}
