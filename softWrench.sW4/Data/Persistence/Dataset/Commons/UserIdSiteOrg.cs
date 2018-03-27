using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using softWrench.sW4.Data.API;
using softWrench.sW4.Data.Persistence.WS.API;

namespace softWrench.sW4.Data.Persistence.Dataset.Commons {

    /// <summary>
    /// Wrapper class to hold these 3 fields which are the core of any maximo entity
    /// </summary>
    public class UserIdSiteOrg {

        public UserIdSiteOrg() {

        }

        public UserIdSiteOrg(OperationDataRequest operationData) {
            UserId = operationData.UserId;
            OrgId = operationData.OrgId;
            SiteId = operationData.SiteId;
        }

        public UserIdSiteOrg(TargetResult targetResult) {
            UserId = targetResult.UserId;
            SiteId = targetResult.SiteId;
        }

        public Tuple<string, string> ToUserIdSite => new Tuple<string, string>(UserId, SiteId);

        public string UserId { get; set; }
        public string SiteId { get; set; }
        public string OrgId { get; set; }
    }
}
