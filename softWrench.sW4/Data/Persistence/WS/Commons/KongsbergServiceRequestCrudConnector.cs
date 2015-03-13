using softWrench.sW4.Data.Persistence.Operation;
using softWrench.sW4.Data.Persistence.WS.API;
using softWrench.sW4.Data.Persistence.WS.Internal;
using softWrench.sW4.Metadata.Applications;
using softWrench.sW4.Security.Services;
using softWrench.sW4.Util;
using System;
using System.Collections.Generic;
using System.Text;
using w = softWrench.sW4.Data.Persistence.WS.Internal.WsUtil;
using System.Net.Mail;
using System.Net;
using softWrench.sW4.Configuration.Services.Api;
using cts.commons.simpleinjector;
using softWrench.sW4.Email;

namespace softWrench.sW4.Data.Persistence.WS.Commons
{
    class KongsbergServiceRequestCrudConnector: BaseServiceRequestCrudConnector {

        public override void BeforeUpdate(MaximoOperationExecutionContext maximoTemplateData) {
            var sr = maximoTemplateData.IntegrationObject;

            var user = SecurityFacade.CurrentUser();

            if (w.GetRealValue(sr, "STATUS").Equals("INPROG")) {
                w.SetValue(sr, "ACTUALSTART", DateTime.Now.FromServerToRightKind());
            }
            else if (w.GetRealValue(sr, "STATUS").Equals("RESOLVED")) {
                w.SetValue(sr, "ACTUALFINISH", DateTime.Now.FromServerToRightKind());
            }

            // TODO: Temp fix for getting change by to update with the userid. 
            // This workaround required trigger in the Maximo DB and custom attribute "SWCHANGEBY" in ticket
            w.SetValue(sr, "SWCHANGEBY", user.Login); 
            
            base.BeforeUpdate(maximoTemplateData);
        }
    }
}
