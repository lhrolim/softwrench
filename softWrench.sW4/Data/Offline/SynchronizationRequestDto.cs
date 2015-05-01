using System;
using System.Collections.Generic;
using System.Linq;
using cts.commons.portable.Util;
using Newtonsoft.Json.Linq;

namespace softWrench.sW4.Data.Offline {
    public class SynchronizationRequestDto {

        public SynchronizationRequestDto() {
            Applications = new List<ApplicationSyncData>();
        }

        public String ApplicationName { get; set; }


        /// <summary>
        /// Comma sepparated list of current top level apps that the client has. To be used in conjuction with ReturnNewApps flag, where if true, it would be neededd to bring any extra applications besides the one being requested.
        /// That would be used on the scenario where the metadata has just changed on the server side, and the client still doesnt have the entire list of applications it needs to fetch
        /// </summary>
        public List<String> ClientCurrentTopLevelApps { get; set; }


        public Boolean ReturnNewApps { get; set; }


        public List<ApplicationSyncData> Applications { get; set; }

        public ApplicationSyncData GetApplicationData(string applicationName) {
            if (Applications == null) {
                return null;
            }
            return Applications.FirstOrDefault(f => f.AppName.Equals(applicationName));
        }

        public IDictionary<string, long?> GetCompositionRowstampMap(ISet<string> compositionApplicationNames) {
            var result = new Dictionary<string, long?>();
            foreach (var compositionApplicationName in compositionApplicationNames) {
                var name = compositionApplicationName;
                var appData = Applications.FirstOrDefault(f => f.AppName.EqualsIc(name));
                if (appData != null) {
                    result.Add(name, Convert.ToInt64(appData.UpperLimitRowstamp));
                }
            }
            return result;
        }


        public class ApplicationSyncData {


            public string AppName { get; set; }


            [Obsolete]
            public bool FetchMetadata { get; set; }

            public ApplicationSyncData() {
                FetchMetadata = false;
            }

            //            public string schemaId { get; set; }
            public string UpperLimitRowstamp { get; set; }
            public string LowerLimitRowstamp { get; set; }


        }
    }
}
