﻿using System;
using System.Collections.Generic;
using System.Linq;
using cts.commons.portable.Util;

namespace softwrench.sw4.offlineserver.dto {
    public class SynchronizationRequestDto {

        public String ApplicationName { get; set; }

        /// <summary>
        /// List of ids that should be ignored on the query since they are currently part of a batch being submitted to the server
        /// </summary>
        public List<String> BatchItemsIds { get; set; }


        /// <summary>
        /// Comma sepparated list of current top level apps that the client has. To be used in conjuction with ReturnNewApps flag, where if true, it would be neededd to bring any extra applications besides the one being requested.
        /// That would be used on the scenario where the metadata has just changed on the server side, and the client still doesnt have the entire list of applications it needs to fetch
        /// </summary>
        public List<String> ClientCurrentTopLevelApps { get; set; }


        public Boolean ReturnNewApps { get; set; }





    }
}
