using System;
using softWrench.sW4.Data.Persistence.WS.API;
using softWrench.sW4.Data.Persistence.WS.Internal;
using softWrench.sW4.Security.Services;
using softWrench.sW4.Util;
using w = softWrench.sW4.Data.Persistence.WS.Internal.WsUtil;

namespace softWrench.sW4.Data.Persistence.WS.Commons {
    class PaeAssetCrudConnector : CrudConnectorDecorator {

        public override void BeforeUpdate(MaximoOperationExecutionContext maximoTemplateData) {
            var asset = maximoTemplateData.IntegrationObject;
            var currentTime = DateTime.Now.FromServerToRightKind();

            w.SetValue(asset, "invdate", currentTime);
            w.SetValue(asset, "invpostdate", currentTime);
            w.SetValue(asset, "invpostdateby", SecurityFacade.CurrentUser().DBUser.UserName);
            w.SetValue(asset, "invposttype", "Automatic");

            // Additional required fields
            w.SetValueIfNull(asset, "plustsoldamt", 0);
            w.SetValueIfNull(asset, "plusttotalmprevenue", 0);
            w.SetValueIfNull(asset, "taxpercent", 0);

            base.BeforeUpdate(maximoTemplateData);
        }
    }
}
