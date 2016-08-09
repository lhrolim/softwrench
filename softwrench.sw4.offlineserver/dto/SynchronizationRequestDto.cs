﻿using System.Collections.Generic;
using Newtonsoft.Json.Linq;

namespace softwrench.sw4.offlineserver.dto {
    public class SynchronizationRequestDto {

        public string ApplicationName { get; set; }

        /// <summary>
        /// List of ids that should be ignored on the query since they are currently part of a batch being submitted to the server
        /// </summary>
        public List<string> BatchItemsIds { get; set; }

        /// <summary>
        /// If present only the items on this list will be downlaoded, regardless of any rowstamp rules. Used for quick sync
        /// </summary>
        public List<string> ItemsToDownload { get; set; } 

        /// <summary>
        /// Comma sepparated list of current top level apps that the client has. To be used in conjuction with ReturnNewApps flag, where if true, it would be neededd to bring any extra applications besides the one being requested.
        /// That would be used on the scenario where the metadata has just changed on the server side, and the client still doesnt have the entire list of applications it needs to fetch
        /// </summary>
        public List<string> ClientCurrentTopLevelApps { get; set; }

        public bool ReturnNewApps { get; set; }

        public UserSyncData UserData { get; set; }

        public JObject RowstampMap { get; set; }
    }
}
