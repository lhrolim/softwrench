using System;
using System.Diagnostics;
using System.Threading.Tasks;
using log4net;
using Quartz;
using softWrench.sW4.Scheduler.Interfaces;
using softWrench.sW4.SimpleInjector.Core.Order;
using softWrench.sW4.SimpleInjector.Events;
using softWrench.sW4.Util;
using LogicalThreadContext = Quartz.Util.LogicalThreadContext;

namespace softWrench.sW4.Scheduler {
    public abstract class ASwJob : ISwJob, ISWEventListener<ApplicationStartedEvent>, IOrdered {

        protected static readonly ILog Log = LogManager.GetLogger(SwConstants.JOB_LOG);

        protected ASwJob() {

        }

        public void Execute(IJobExecutionContext context) {
            DoExecute();
        }

        protected void DoExecute() {
            LogicalThreadContext.SetData("user", "swjobuser");
            var before = Stopwatch.StartNew();
            try {
                Log.InfoFormat("Starting execution of {0}", Name());
                ExecuteJob();
            } catch (Exception e) {
                Log.Error(String.Format("error executing job {0} ", Name()), e);
            } finally {
                Log.Info(LoggingUtil.BaseDurationMessageFormat(before, "Finished execution of {0}", Name()));
                LogicalThreadContext.SetData("user", null);
            }
        }

        public abstract string Name();
        public abstract string Description();
        public abstract string Cron();
        public abstract void ExecuteJob();
        public abstract bool IsScheduled { get; set; }
        public abstract bool RunAtStartup();

        public void HandleEvent(ApplicationStartedEvent eventToDispatch) {
            if (RunAtStartup()) {
                Task.Factory.StartNew(DoExecute);
            }
        }
        //run at the end
        public int Order { get { return 1000; } }
    }
}
