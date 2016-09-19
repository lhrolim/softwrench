using System;
using System.Diagnostics;
using System.Threading.Tasks;
using cts.commons.Util;
using log4net;
using Quartz;
using softWrench.sW4.Scheduler.Interfaces;
using cts.commons.simpleinjector.Core.Order;
using cts.commons.simpleinjector.Events;
using softWrench.sW4.Util;
using LogicalThreadContext = Quartz.Util.LogicalThreadContext;

namespace softWrench.sW4.Scheduler {
    public abstract class ASwJob : ISwJob, ISWEventListener<ApplicationStartedEvent>, IOrdered {

        protected static readonly ILog Log = LogManager.GetLogger(SwConstants.JOB_LOG);

        protected ASwJob() {

        }

        public async void Execute(IJobExecutionContext context) {
            await DoExecute();
        }

        protected async Task DoExecute() {
            if (!IsEnabled) {
                Log.InfoFormat("Skipping disabled job {0}", Name());
                return;
            }

            LogicalThreadContext.SetData("user", "swjobuser");
            var before = Stopwatch.StartNew();
            try {
                Log.InfoFormat("Starting execution of {0}", Name());
                await ExecuteJob();
            } catch (Exception e) {
                Log.Error(string.Format("error executing job {0} ", Name()), e);
            } finally {
                Log.Info(LoggingUtil.BaseDurationMessageFormat(before, "Finished execution of {0}", Name()));
                LogicalThreadContext.SetData("user", null);
            }
        }

        public abstract string Name();
        public abstract string Description();
        public abstract string Cron();
        public abstract Task ExecuteJob();

        public virtual void OnJobSchedule(){
            //NOOP by default
        }

        public abstract bool RunAtStartup();

        public virtual async void HandleEvent(ApplicationStartedEvent eventToDispatch) {
            if (RunAtStartup()) {
                await DoExecute();
            }
        }

        public bool IsScheduled {
            get; set;
        }

        public virtual bool IsEnabled {
            get {
                return true;
            }
        }

        //run at the end
        public int Order {
            get {
                return 1000;
            }
        }
    }
}
