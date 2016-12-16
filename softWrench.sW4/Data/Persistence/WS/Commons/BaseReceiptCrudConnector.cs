using System;
using softWrench.sW4.Data.Persistence.WS.API;
using softWrench.sW4.Data.Persistence.WS.Internal;
using softWrench.sW4.Security.Services;
using softWrench.sW4.Data.Persistence.Operation;
using softWrench.sW4.Util;
using w = softWrench.sW4.Data.Persistence.WS.Internal.WsUtil;

namespace softWrench.sW4.Data.Persistence.WS.Commons {
    class BaseReceiptCrudConnector : CrudConnectorDecorator {



        public override void BeforeCreation(MaximoOperationExecutionContext maximoTemplateData) {
            var user = SecurityFacade.CurrentUser();
            var crudData = ((CrudOperationData)maximoTemplateData.OperationData);
            var receipt = maximoTemplateData.IntegrationObject;
            w.SetValueIfNull(receipt, "receiptquantity", Convert.ToDouble(crudData.GetAttribute("quantity")));
            w.SetValueIfNull(receipt, "externalrefid", "sw");
            w.SetValueIfNull(receipt, "issuetype", "RECEIPT");
            w.SetValueIfNull(receipt, "consignment", 0);
            w.SetValueIfNull(receipt, "porevisionnum", 0);
            w.SetValueIfNull(receipt, "status", "WASSET");
            w.SetValueIfNull(receipt, "tostoreloc", "RECEIVING");
            w.SetValueIfNull(receipt, "conversion", 1.00);
            w.SetValueIfNull(receipt, "sourcesysid", "sw");
            w.SetValue(receipt, "enterby", user.Login);
            w.SetValue(receipt, "positeid", crudData.GetAttribute("siteid"));
            w.SetValueIfNull(receipt, "transdate", DateTime.Now.FromServerToRightKind());
            w.SetValueIfNull(receipt, "actualdate", DateTime.Now.AddMinutes(-1.00).FromServerToRightKind());
            base.BeforeCreation(maximoTemplateData);
        }

        public override string ApplicationName() {
            return "receipt";
        }
    }
}
