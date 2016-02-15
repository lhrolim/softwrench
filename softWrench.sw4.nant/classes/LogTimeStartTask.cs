using System;
using System.Text;
using NAnt.Core;
using Task = NAnt.Core.Task;

namespace softWrench.sw4.nant.classes {
    public class LogTimeStartTask : Task {
        protected override void ExecuteTask() {
            var target = Project.CurrentTarget;
            if (target == null || LogTimeCommons.SkipLog(Project)) {
                return;
            }

            // gets the start time and logs
            var now = DateTime.Now;
            LogTimeCommons.StartTimes.Add(target.Name, now.Ticks);
            var sb = new StringBuilder();
            sb.Append("\n\n\n==== Start of ").Append(target.Name).Append(" at ");
            sb.Append(now).Append(" =======================================\n\n\n");
            Log(Level.Info, sb.ToString());
        }
    }
}
