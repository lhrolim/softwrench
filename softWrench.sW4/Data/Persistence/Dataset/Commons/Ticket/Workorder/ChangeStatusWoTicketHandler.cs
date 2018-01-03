using System;
using softWrench.sW4.Data.Persistence.Operation;
using softWrench.sW4.Data.Persistence.WS.API;
using softWrench.sW4.Data.Persistence.WS.Internal;
using softWrench.sW4.Security.Services;
using softWrench.sW4.Util;

namespace softWrench.sW4.Data.Persistence.Dataset.Commons.Ticket.Workorder {

    public class ChangeStatusWoTicketHandler : BaseMaximoCustomConnector {

        public virtual TargetResult ChangeStatus(NewStatusData crudOperationData) {

            var crudData = crudOperationData.CrudData;
            crudData.SetAttribute("#hasstatuschange", true);
            crudData.SetAttribute("#forcestatusreturn", true);
            crudData.SetAttribute("status",crudOperationData.NewStatus);
            return (TargetResult) Maximoengine.Update(crudOperationData.CrudData);
            
        }


      

        public class NewStatusData : CrudOperationDataContainer {
            public string NewStatus {
                get; set;
            }
        }

        public override string ApplicationName() {
            return "workorder";
        }

        public override string ActionId() {
            return "changestatus";
        }
    }
}
