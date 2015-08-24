using softWrench.sW4.Data.Persistence.WS.Internal;
using softWrench.sW4.Security.Services;
using softWrench.sW4.Util;
using System;
using w = softWrench.sW4.Data.Persistence.WS.Internal.WsUtil;

namespace softWrench.sW4.Data.Persistence.WS.Commons
{
    class KongsbergIncidentCrudConnector: BaseServiceRequestCrudConnector {

        public override void BeforeUpdate(MaximoOperationExecutionContext maximoTemplateData) {
            var incident = maximoTemplateData.IntegrationObject;

            var user = SecurityFacade.CurrentUser();

            if (w.GetRealValue(incident, "STATUS").Equals("INPROG")) {
                w.SetValueIfNull(incident, "ACTUALSTART", DateTime.Now.FromServerToRightKind());
            } else if (w.GetRealValue(incident, "STATUS").Equals("RESOLVED")) {
                w.SetValue(incident, "ACTUALFINISH", DateTime.Now.FromServerToRightKind());
            }

            // TODO: Temp fix for getting change by to update with the userid. 
            // This workaround required trigger in the Maximo DB and custom attribute "SWCHANGEBY" in ticket
            w.SetValue(incident, "SWCHANGEBY", user.Login); 

            base.BeforeUpdate(maximoTemplateData);
        }
    }
}
