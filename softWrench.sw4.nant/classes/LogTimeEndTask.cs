using System;
using System.Text;
using NAnt.Core;
using Task = NAnt.Core.Task;

namespace softWrench.sw4.nant.classes {
    public class LogTimeEndTask : Task {
        protected override void ExecuteTask() {
            var target = Project.CurrentTarget;
            if (target == null || LogTimeCommons.SkipLog(Project) || !LogTimeCommons.StartTimes.ContainsKey(target.Name)) {
                return;
            }

            // calcs the time delta and logs
            var start = LogTimeCommons.StartTimes[target.Name];
            var now = DateTime.Now;
            var delta = (now.Ticks - start) / 10000;
            var sb = new StringBuilder();
            sb.Append("\n\n\n==== End of ").Append(target.Name).Append(" at ");
            sb.Append(now).Append(" ==== Task ran in ").Append(delta).Append("ms =================\n\n\n");
            Log(Level.Info, sb.ToString());
        }
    }
}
