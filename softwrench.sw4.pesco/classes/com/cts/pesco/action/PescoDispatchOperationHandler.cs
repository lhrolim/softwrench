using softWrench.sW4.Data.Persistence.Dataset.Commons.Ticket.ServiceRequest;
using softWrench.sW4.Data.Persistence.Operation;

namespace softwrench.sw4.pesco.classes.com.cts.pesco.action {
    public class PescoDispatchOperationHandler : DispatchOperationHandler {
        public override CrudOperationData CreateWoCrudData(CrudOperationData srCrudData) {
            var woCrudData = base.CreateWoCrudData(srCrudData);
            var lostenergy3 = srCrudData.GetAttribute("lostenergy3");
            var lostenergyamount = (lostenergy3 != null && lostenergy3.Equals(true)) ? srCrudData.GetStringAttribute("lostenergyamount") : "";
            woCrudData.SetAttribute("lostenergy", lostenergy3);
            woCrudData.SetAttribute("lostenergyamount", lostenergyamount);
            return woCrudData;
        }
    }
}
