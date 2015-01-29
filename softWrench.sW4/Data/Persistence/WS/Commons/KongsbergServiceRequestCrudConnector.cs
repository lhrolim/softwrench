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
using softWrench.sW4.SimpleInjector;
using softWrench.sW4.Email;

namespace softWrench.sW4.Data.Persistence.WS.Commons
{
    class KongsbergServiceRequestCrudConnector: BaseServiceRequestCrudConnector {

        public override void BeforeUpdate(MaximoOperationExecutionContext maximoTemplateData) {
            var sr = maximoTemplateData.IntegrationObject;

            if (w.GetRealValue(sr, "STATUS").Equals("INPROG")) {
                w.SetValue(sr, "ACTUALSTART", DateTime.Now.FromServerToRightKind());
            }
            else if (w.GetRealValue(sr, "STATUS").Equals("RESOLVED"))
            {
                w.SetValue(sr, "ACTUALFINISH", DateTime.Now.FromServerToRightKind());
            }

            base.BeforeUpdate(maximoTemplateData);
        }
    }
}
