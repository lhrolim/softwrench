﻿using System;
using softWrench.sW4.Data.Persistence.Operation;
using softWrench.sW4.Data.Persistence.WS.Commons;
using softWrench.sW4.Data.Persistence.WS.Internal;
using softWrench.sW4.Security.Services;
using softWrench.sW4.Util;
using w = softWrench.sW4.Data.Persistence.WS.Internal.WsUtil;

namespace softwrench.sw4.kongsberg.classes.com.cts.kongsberg.connector
{
    public class KongsbergServiceRequestCrudConnector: BaseServiceRequestCrudConnector {

        public override void BeforeUpdate(MaximoOperationExecutionContext maximoTemplateData) {
            var sr = maximoTemplateData.IntegrationObject;
            var crudData = ((CrudOperationData)maximoTemplateData.OperationData);
            var user = SecurityFacade.CurrentUser();

            if (w.GetRealValue(sr, "STATUS").Equals("INPROG")) {
                w.SetValueIfNull(sr, "ACTUALSTART", DateTime.Now.FromServerToRightKind());
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