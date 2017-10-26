using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using cts.commons.portable.Util;
using softwrench.sW4.audit.classes.Model;
using softWrench.sW4.Data;
using softWrench.sW4.Data.API;
using softWrench.sW4.Data.Persistence.Dataset.Commons;
using softWrench.sW4.Metadata.Applications;
using softWrench.sW4.Metadata.Security;

namespace softWrench.sW4.Audit {

    public class BaseAuditDataSet : SWDBApplicationDataset {

        protected override async Task<DataMap> FetchDetailDataMap(ApplicationMetadata application, InMemoryUser user, DetailRequest request)
        {
            var baseMap = await base.FetchDetailDataMap(application, user, request);
//            var byteData = baseMap.GetAttribute("data") as byte[];
            var entry = await SWDAO.FindByPKAsync<AuditEntry>(baseMap.GetIntAttribute("id"));

            baseMap.SetAttribute("#data", entry.DataStringValue);
            return baseMap;
        }

        public override string ApplicationName() {
            return "_audit";
        }
    }
}
