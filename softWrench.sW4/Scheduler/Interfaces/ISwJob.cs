using Quartz;
using cts.commons.simpleinjector;

namespace softWrench.sW4.Scheduler.Interfaces {
    public interface ISwJob : IJob, IComponent {
        string Name();
        string Description();
        string Cron();
        void ExecuteJob();
        bool IsScheduled {
            get; set;
        }
        bool IsEnabled {
            get;
        }

        void OnJobSchedule();

        bool RunAtStartup();

    }
}
