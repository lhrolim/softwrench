using System;
using softWrench.sW4.Data.Persistence.Operation;
using softWrench.sW4.Data.Persistence.WS.Commons;
using softWrench.sW4.Data.Persistence.WS.Internal;
using softWrench.sW4.Security.Services;
using softWrench.sW4.Util;
using w = softWrench.sW4.Data.Persistence.WS.Internal.WsUtil;

namespace softwrench.sw4.chicago.classes.com.cts.chicago.dataset {
    class ChicagoServiceRequestCrudConnector : BaseServiceRequestCrudConnector {
        public override void BeforeUpdate(MaximoOperationExecutionContext maximoTemplateData) {
            var sr = maximoTemplateData.IntegrationObject;

            if (w.GetRealValue(sr, "STATUS").Equals("INPROG")) {
                w.SetValueIfNull(sr, "ACTUALSTART", DateTime.Now.FromServerToRightKind());
            } else if (w.GetRealValue(sr, "STATUS").Equals("RESOLVED")) {
                w.SetValue(sr, "ACTUALFINISH", DateTime.Now.FromServerToRightKind());
            } else if (w.GetRealValue(sr, "STATUS").Equals("CLOSED")) {
                w.SetValue(sr, "ITDCLOSEDATE", DateTime.Now.FromServerToRightKind());
            }

            base.BeforeUpdate(maximoTemplateData);
        }
    }
}
