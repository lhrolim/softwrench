using softWrench.sW4.Data.Persistence.Operation;
using softWrench.sW4.Data.Persistence.WS.API;
using softWrench.sW4.Data.Persistence.WS.Internal;
using softWrench.sW4.Security.Services;
using softWrench.sW4.Util;
using System;
using w = softWrench.sW4.Data.Persistence.WS.Internal.WsUtil;

namespace softWrench.sW4.Data.Persistence.WS.Commons {
    public class BaseAssetCrudConnector : CrudConnectorDecorator {

        public override void BeforeUpdate(MaximoOperationExecutionContext maximoTemplateData) {
            var user = SecurityFacade.CurrentUser();
            var sr = maximoTemplateData.IntegrationObject;

            w.SetValueIfNull(sr, "CHANGEDATE", DateTime.Now.FromServerToRightKind(), true);
            w.SetValueIfNull(sr, "CHANGEBY", user.Login);

            CommonTransaction(maximoTemplateData);

            base.BeforeUpdate(maximoTemplateData);
        }

        public override string ApplicationName() {
            return "asset";
        }

        public override void BeforeCreation(MaximoOperationExecutionContext maximoTemplateData) {
            CommonTransaction(maximoTemplateData);
            base.BeforeCreation(maximoTemplateData);
        }

        private void CommonTransaction(MaximoOperationExecutionContext maximoTemplateData) {
            var asset = maximoTemplateData.IntegrationObject;

            var crudData = (CrudOperationData)maximoTemplateData.OperationData;
            //LocationHandler.HandleLocation(crudData, asset);
        }
    }
}
