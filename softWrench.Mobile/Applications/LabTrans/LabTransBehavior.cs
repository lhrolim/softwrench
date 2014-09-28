using System;
using System.Collections.Generic;
using softWrench.Mobile.Behaviors;
using softWrench.Mobile.Data;

namespace softWrench.Mobile.Applications.LabTrans
{
    internal class LabTransBehavior : ApplicationBehavior
    {
        private const string Type = "WORK";

        public LabTransBehavior(IEnumerable<IApplicationCommand> commands) : base(commands)
        {
        }

        public override void OnNew(OnNewContext context, DataMap dataMap)
        {
            base.OnNew(context, dataMap);
        
            dataMap.Value("transtype", Type);
            dataMap.Value("refwo", context.Composite.Composite.Value("wonum"));
            dataMap.Value("enterdate", DateTime.Now);
            dataMap.Value("enterby", context.User.UserName);
        }
    }
}