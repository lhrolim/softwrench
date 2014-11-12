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
            var receipt = maximoTemplateData.IntegrationObject;
            w.SetValueIfNull(receipt, "externalrefid", "sw");
            w.SetValueIfNull(receipt, "issuetype", "RECEIPT");
            w.SetValueIfNull(receipt, "sourcesysid", "sw");
            w.SetValueIfNull(receipt, "rejectqty", 0.0);
            /*w.SetValueIfNull(receipt, "ownersysid", "sw");
            w.SetValueIfNull(receipt, "ACTUALDATE", DateTime.Now.FromServerToRightKind());
            w.SetValueIfNull(receipt, "SITEID", user.SiteId);
            w.SetValueIfNull(receipt, "TRANSDATE", DateTime.Now.FromServerToRightKind());*/
            var crudData = ((CrudOperationData)maximoTemplateData.OperationData);
            base.BeforeCreation(maximoTemplateData);
        }
    }
}
