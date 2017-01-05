using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using cts.commons.portable.Util;
using Newtonsoft.Json.Linq;
using Quartz.Util;
using softWrench.sW4.Data.Entities;
using softWrench.sW4.Data.Persistence.Operation;
using softWrench.sW4.Data.Persistence.WS.API;
using softWrench.sW4.Util;

namespace softWrench.sW4.Data.Persistence.Dataset.Commons.Ticket.ServiceRequest {
    public class DispatchWorkorderConnector : DispatchOperationHandler{


        public object DispatchWO(DispatchOperationData srData) {
            var woCrudData = CreateWoCrudData(srData.CrudData);
            var result = (TargetResult)Maximoengine.Create(woCrudData);
            var id = result.Id ?? result.UserId;

            //customization for deltadental here
            var label = ApplicationConfiguration.ClientName.EqualsIc("deltadental") ? "dispatched" : "created";

            result.SuccessMessage = "Work Order {0} successfully {1}.".FormatInvariant(id, label);

            return result;
        }


        public override string ActionId() {
            return "dispatchWO";
        }

    }
}
