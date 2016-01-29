using System;
using softWrench.sW4.Data.Persistence.Operation;
using softWrench.sW4.Data.Persistence.WS.API;
using softWrench.sW4.Data.Persistence.WS.Internal;
using softWrench.sW4.Util;
using w = softWrench.sW4.Data.Persistence.WS.Internal.WsUtil;

namespace softWrench.sW4.Data.Persistence.WS.Commons {

    class BaseInvuseCrudConnector : CrudConnectorDecorator {

        public BaseInvuseCrudConnector() {
            
        }

        public override void BeforeCreation(MaximoOperationExecutionContext maximoTemplateData) {
            var entity = (CrudOperationData)maximoTemplateData.OperationData;
            var invuse = maximoTemplateData.IntegrationObject;

            var currentTime = DateTime.Now.FromServerToRightKind();
            var adjustedCurrentTime = currentTime.AddSeconds(-60);

            w.SetValueIfNull(invuse, "CHANGEDATE", adjustedCurrentTime);

            w.SetValueIfNull(invuse, "USETYPE", "TRANSFER");
            w.SetValue(invuse, "STATUS", "COMPLETE");
            InvuselineHandler.HandleInvuseline(entity, invuse);
            base.BeforeCreation(maximoTemplateData);
        }
    }
}
