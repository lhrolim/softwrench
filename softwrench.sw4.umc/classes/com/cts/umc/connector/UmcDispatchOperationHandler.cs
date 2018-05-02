using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using softwrench.sw4.umc.classes.com.cts.umc.util;
using softWrench.sW4.Data.Persistence.Dataset.Commons.Ticket.ServiceRequest;
using softWrench.sW4.Data.Persistence.Operation;

namespace softwrench.sw4.umc.classes.com.cts.umc.connector {
    public class UmcDispatchOperationHandler : DispatchWorkorderConnector {

        public override CrudOperationData CreateWoCrudData(CrudOperationData srCrudData) {
            var dm = base.CreateWoCrudData(srCrudData);
            //TODO: move to some so
            UmcWorkorderUtil.PopulateDefaultValues(dm);
            dm.SetAttribute("status", "WAPPR");
            return dm;
        }

        public override string ClientFilter() {
            return "umc";
        }
    }
}
