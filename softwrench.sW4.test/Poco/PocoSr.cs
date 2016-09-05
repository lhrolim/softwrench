using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using softWrench.sW4.Data;

namespace softwrench.sW4.test.Poco {
    internal class PocoSr {

        public static DataMap BasicForRelationship(string id = "1", string siteid = "site", string extraParameters = null) {
            var dictionary = new Dictionary<string, object>() { { "ticketid", id }, { "siteid", siteid } };
            var dataMap = new DataMap("servicerequest", dictionary);
            dataMap.PopulateFromString(extraParameters);
            return dataMap;
        }
    }
}
