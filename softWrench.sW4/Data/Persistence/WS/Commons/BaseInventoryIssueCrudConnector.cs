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
    class BaseInventoryIssueCrudConnector : CrudConnectorDecorator {

        public override void BeforeCreation(MaximoOperationExecutionContext maximoTemplateData) {
            var user = SecurityFacade.CurrentUser();
            var invIssue = maximoTemplateData.IntegrationObject;

            w.SetValueIfNull(invIssue, "ACTUALDATE", DateTime.Now.FromServerToRightKind());
            w.SetValueIfNull(invIssue, "TRANSDATE", DateTime.Now.FromServerToRightKind());
            w.SetValueIfNull(invIssue, "SENDERSYSID", "SW");
            double units = Convert.ToDouble(w.GetRealValue(invIssue, "QUANTITY"));
            double unitCost = Convert.ToDouble(w.GetRealValue(invIssue, "UNITCOST"));
            w.SetValueIfNull(invIssue, "ACTUALCOST", unitCost);
            double lineCost = units * unitCost;
            w.SetValueIfNull(invIssue, "LINECOST", lineCost);
            var quantity = w.GetRealValue(invIssue, "QUANTITY");
            w.SetValueIfNull(invIssue, "QTYREQUESTED", quantity);

            base.BeforeCreation(maximoTemplateData);
        }
    }
}
