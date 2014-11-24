using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using softWrench.sW4.Data.Persistence.WS.API;
using softWrench.sW4.Data.Persistence.WS.Internal;
using softWrench.sW4.Security.Services;
using softWrench.sW4.Data.Persistence.Operation;
using softWrench.sW4.Util;
using w = softWrench.sW4.Data.Persistence.WS.Internal.WsUtil;

namespace softWrench.sW4.Data.Persistence.WS.Commons {
    class BaseReceiptCrudConnector: CrudConnectorDecorator {

        public BaseReceiptCrudConnector(){
            
        }

        public override void BeforeUpdate(MaximoOperationExecutionContext maximoTemplateData){
            var user = SecurityFacade.CurrentUser();
            var sr = maximoTemplateData.IntegrationObject;
            var crudData = ((CrudOperationData)maximoTemplateData.OperationData);
            base.BeforeUpdate(maximoTemplateData);
        }

        public override void BeforeCreation(MaximoOperationExecutionContext maximoTemplateData) {
            var user = SecurityFacade.CurrentUser();
            var crudData = ((CrudOperationData)maximoTemplateData.OperationData);
            var receipt = maximoTemplateData.IntegrationObject;
            w.SetValueIfNull(receipt, "receiptquantity", Convert.ToDouble(crudData.GetAttribute("quantity")));
            w.SetValueIfNull(receipt, "externalrefid", "sw");
            w.SetValueIfNull(receipt, "issuetype", "RECEIPT");
            w.SetValueIfNull(receipt, "gldebitacct", crudData.GetAttribute("gldebitacct"));
            base.BeforeCreation(maximoTemplateData);
        }
    }
}
