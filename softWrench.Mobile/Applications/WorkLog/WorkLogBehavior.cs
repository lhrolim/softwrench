using System;
using System.Collections.Generic;
using softWrench.Mobile.Applications.WorkOrder;
using softWrench.Mobile.Behaviors;
using softWrench.Mobile.Data;

namespace softWrench.Mobile.Applications.WorkLog
{
    internal class WorkLogBehavior : ApplicationBehavior
    {
        public WorkLogBehavior(IEnumerable<IApplicationCommand> commands) : base(commands)
        {
        }

        public override void OnNew(OnNewContext context, DataMap dataMap)
        {
            base.OnNew(context, dataMap);

            dataMap.Value("class", WorkOrderBehavior.Class);
            dataMap.Value("createby", context.User.UserName);
            dataMap.Value("createdate", DateTime.Now);
            dataMap.Value("recordkey", context.Composite.Composite.Value("wonum"));
        }
    }
}
