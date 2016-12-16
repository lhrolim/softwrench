using System;
using JetBrains.Annotations;
using softWrench.sW4.Data.Persistence.Operation;
using softWrench.sW4.Data.Persistence.WS.Internal;
using softWrench.sW4.Security.Services;
using softWrench.sW4.Util;

namespace softWrench.sW4.Data.Persistence.Dataset.Commons.Ticket.Workorder {
    public class UpdateStatusWorkorderHandler : BaseMaximoCustomConnector {
        public class UpdateStatusOperationData : OperationData {
            [NotNull]
            public string wonum;
            [NotNull]
            public string status;

            public DateTime? statusdate;
        }

        public void UpdateStatus(UpdateStatusOperationData opData) {
            MaximoOperationExecutionContext maximoExecutionContext = GetContext(opData);
            var user = SecurityFacade.CurrentUser();
            object wo = maximoExecutionContext.IntegrationObject;
            //just to validate that the json can be converted to a num
            WsUtil.SetValue(wo, "wonum", opData.wonum);
            var woStatus = WsUtil.SetValue(wo, "STATUS", opData.status);
            var statusDate = WsUtil.SetValue(wo, "STATUSDATE", opData.statusdate ?? DateTime.Now.FromServerToRightKind());
            object statusIFace = WsUtil.SetValue(wo, "STATUSIFACE", true);
            var nemo = ReflectionUtil.InstantiateProperty(wo, "NP_STATUSMEMO", new {
                Value = WsUtil.GetRealValue(wo, "MEMO")
            });
            WsUtil.SetChanged(nemo, statusIFace, statusDate, woStatus);
            maximoExecutionContext.InvokeProxy();
        }

        public override string ApplicationName() {
            return "workorder";
        }

        public override string ActionId() {
            return "updatestatus";
        }
    }
}
