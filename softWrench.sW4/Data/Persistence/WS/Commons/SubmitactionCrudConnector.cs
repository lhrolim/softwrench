using softWrench.sW4.Data.Persistence.WS.Internal;
using System;
using JetBrains.Annotations;
using softWrench.sW4.Data.Persistence.Operation;

namespace softWrench.sW4.Data.Persistence.WS.Commons {

    /// <summary>
    /// this code is called from sr_service.js to change status of 
    /// </summary>
    public class SubmitactionCrudConnector : BaseMaximoCustomConnector {
        public partial class UpdateStatusOperationData : CrudOperationDataContainer {
            //[NotNull]
            //public string ticketid;
            [NotNull]
            public string status;

        }

        public Object SubmitAction(UpdateStatusOperationData opData) {
            opData.CrudData["status"] = opData.status;
            opData.CrudData["#submittingaction"] = "true";
            return Maximoengine.Update(opData.CrudData);
        }

        public override string ApplicationName() {
            return "servicerequest";
        }

        public override string ActionId() {
            return "submitaction";
        }
    }
}
