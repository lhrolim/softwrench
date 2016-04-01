using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using softWrench.sW4.Data.Persistence.Operation;
using softWrench.sW4.Data.Persistence.WS.API;
using softWrench.sW4.Data.Persistence.WS.Internal;
using softWrench.sW4.Security.Services;
using softWrench.sW4.Util;

namespace softWrench.sW4.Data.Persistence.Dataset.Commons.Ticket {

    public class ChangeStatusTicketHandler : BaseMaximoCustomConnector {

        public TargetResult ChangeStatus(NewStatusData crudOperationData) {
            
            var maximoExecutionContext = GetContext(crudOperationData);
            var user = SecurityFacade.CurrentUser();
            var ticket = maximoExecutionContext.IntegrationObject;
            //just to validate that the json can be converted to a num
            var crudData = crudOperationData.CrudData;
            WsUtil.SetValue(ticket, "ticketuid", crudData.Id);
            WsUtil.SetValue(ticket, "ticketid", crudData.UserId);
            WsUtil.SetValue(ticket, "siteid", crudData.SiteId);
            WsUtil.SetValue(ticket, "class", maximoExecutionContext.ApplicationMetadata.Schema.EntityName);


            WsUtil.SetValueIfNull(ticket, "ACTLABHRS", 0.0);
            WsUtil.SetValueIfNull(ticket, "ACTLABCOST", 0.0);
            WsUtil.SetValue(ticket, "CHANGEDATE", DateTime.Now.FromServerToRightKind(), true);
            WsUtil.SetValueIfNull(ticket, "CHANGEBY", user.Login);
            WsUtil.SetValueIfNull(ticket, "REPORTDATE", DateTime.Now.FromServerToRightKind());

            var woStatus = WsUtil.SetValue(ticket, "STATUS", crudOperationData.NewStatus);
            var statusDate = WsUtil.SetValue(ticket, "STATUSDATE", DateTime.Now.FromServerToRightKind());
            var statusIFace = WsUtil.SetValue(ticket, "STATUSIFACE", true);


            WsUtil.SetChanged(statusIFace, statusDate, woStatus);
            var ob = maximoExecutionContext.InvokeProxy();

            return new TargetResult(crudData.Id,crudData.UserId,ob,"Status has been successfully updated");

        }

        public class NewStatusData : CrudOperationDataContainer {
            public string NewStatus { get; set; }
        }

    }
}
