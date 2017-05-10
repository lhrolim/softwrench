using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using softWrench.sW4.Data.Persistence.WS.Applications.Workorder;
using softWrench.sW4.Data.Persistence.WS.Internal;
using softWrench.sW4.Security.Context;
using softWrench.sW4.Security.Services;
using softWrench.sW4.Util;

namespace softwrench.sw4.firstsolar.classes.com.cts.firstsolar.connector {
    public class FirstSolarWorkorderCrudConnector : BaseWorkOrderCrudConnector {

        [Import]
        public IContextLookuper ContextLookuper { get; set; }

        public override void BeforeCreation(MaximoOperationExecutionContext maximoTemplateData) {
            base.BeforeCreation(maximoTemplateData);
            if (ContextLookuper.LookupContext().OfflineMode) {

                var user = SecurityFacade.CurrentUser();

                var root = maximoTemplateData.IntegrationObject;
                var arr = ReflectionUtil.InstantiateArrayWithBlankElements(root, "ASSIGNMENT", 1);
                var assignment = arr.GetValue(0);

                WsUtil.SetValue(assignment, "LABORCODE", user.GetProperty("laborcode"));
                WsUtil.SetValue(assignment, "SCHEDULEDATE", DateTime.Now.FromServerToRightKind());
                WsUtil.SetValue(assignment, "FINISHDATE", DateTime.Now.AddMonths(2).FromServerToRightKind());
                WsUtil.CopyFromRootEntity(root, assignment, "orgid", user.OrgId);


            }
        }

        public override string ClientFilter() {
            return "firstsolar";
        }
    }
}
