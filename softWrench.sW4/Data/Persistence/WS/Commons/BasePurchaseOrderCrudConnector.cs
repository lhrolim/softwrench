using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using softWrench.sW4.Data.Persistence.Operation;
using softWrench.sW4.Data.Persistence.WS.API;
using softWrench.sW4.Data.Persistence.WS.Internal;
using softWrench.sW4.Security.Services;
using softWrench.sW4.Util;
using w = softWrench.sW4.Data.Persistence.WS.Internal.WsUtil;

namespace softWrench.sW4.Data.Persistence.WS.Commons {
    class BasePurchaseOrderCrudConnector : CrudConnectorDecorator {
        public override void BeforeUpdate(MaximoOperationExecutionContext maximoTemplateData) {
            var user = SecurityFacade.CurrentUser();
            var po = maximoTemplateData.IntegrationObject;

            w.SetValueIfNull(po, "ACTLABHRS", 0.0);
            w.SetValueIfNull(po, "ACTLABCOST", 0.0);
            w.SetValueIfNull(po, "CHANGEDATE", DateTime.Now.FromServerToRightKind(), true);
            w.SetValueIfNull(po, "CHANGEBY", user.Login);
            w.SetValueIfNull(po, "REPORTDATE", DateTime.Now.FromServerToRightKind());


            var crudData = ((CrudOperationData)maximoTemplateData.OperationData);
            LongDescriptionHandler.HandleLongDescription(po, crudData);
            HandlePolines(maximoTemplateData, crudData, po);
            base.BeforeUpdate(maximoTemplateData);
        }

        private void HandlePolines(MaximoOperationExecutionContext maximoTemplateData, CrudOperationData crudData, object po){
            if (crudData != null)
            {
                var polines = (IEnumerable<CrudOperationData>)crudData.GetRelationship("poline");
                var recordKey = crudData.Id;
            }
            var user = SecurityFacade.CurrentUser();

        }
    }
}
