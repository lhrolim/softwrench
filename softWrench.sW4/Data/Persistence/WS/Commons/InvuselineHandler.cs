using System;
using System.Linq;
using System.Collections.Generic;
using softWrench.sW4.Util;
using softWrench.sW4.Security.Services;
using softWrench.sW4.Data.Persistence.Operation;
using softWrench.sW4.Data.Persistence.WS.Internal;
using w = softWrench.sW4.Data.Persistence.WS.Internal.WsUtil;

namespace softWrench.sW4.Data.Persistence.WS.Commons {

    class InvuselineHandler {
      
        public static void HandleInvuseline(CrudOperationData entity, object invuse)
        {
            
            var invuselines = (CrudOperationData)entity.GetRelationship("invuseline");
            var recordKey = entity.Id;
            var user = SecurityFacade.CurrentUser();

            w.CloneSingle((CrudOperationData)entity.GetRelationship("invuseline"), invuse, "INVUSELINE",
                delegate(object integrationObject, CrudOperationData crudData) {
                    w.SetValue(integrationObject, "invuselinenum", 1);
                    w.SetValueIfNull(integrationObject, "USETYPE", "TRANSFER");
                    w.SetValueIfNull(integrationObject, "LINETYPE", "ITEM");
                    w.SetValueIfNull(integrationObject, "CONVERSION", 1);

                    //Date entered not working, server thinks it is in the future
                    var dateEntered = DateTime.Now.FromServerToRightKind();
                    //dateEntered = Convert.ToDateTime("10/29/2014 1:00:00 PM");
                    w.SetValueIfNull(integrationObject, "ACTUALDATE", dateEntered);
                    w.SetValueIfNull(integrationObject, "PHYSCNTDATE", dateEntered);

                    var unitcost = w.GetRealValue(integrationObject, "unitcost");
                    var quantity = w.GetRealValue(integrationObject, "quantity");
                    var linecost = Convert.ToDouble(unitcost) * Convert.ToDouble(quantity);
                    w.SetValue(integrationObject, "LINECOST", linecost);

                    // Hard coded test values
                    w.SetValueIfNull(integrationObject, "PHYSCNT", 0);

                    ReflectionUtil.SetProperty(integrationObject, "action", OperationType.Add.ToString());
                });
        }
    }
}


