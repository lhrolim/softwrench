using Quartz;
using softWrench.sW4.SimpleInjector;

namespace softWrench.sW4.Scheduler.Interfaces {
    public interface ISwJob : IJob, IComponent {
        string Name();
        string Description();
        string Cron();
        void ExecuteJob();
        bool IsScheduled { get; set; }

        bool RunAtStartup();

    }
}
