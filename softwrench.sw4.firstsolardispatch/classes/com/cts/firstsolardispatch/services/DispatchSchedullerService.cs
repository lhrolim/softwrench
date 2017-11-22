using System;
using cts.commons.simpleinjector;
using Quartz;
using Quartz.Impl;
using Quartz.Impl.Triggers;
using softwrench.sw4.firstsolardispatch.classes.com.cts.firstsolardispatch.model;

namespace softwrench.sw4.firstsolardispatch.classes.com.cts.firstsolardispatch.services {
    public class DispatchSchedullerService : ISingletonComponent {
        private const string DispacherJobGroupName = "Dispathcer Jobs";

        private readonly IScheduler _scheduler;

        public DispatchSchedullerService() {
            var sf = new StdSchedulerFactory();
            _scheduler = sf.GetScheduler();
            _scheduler.Start();
        }

        public void ScheduleDispatch(DispatchTicket ticket) {
            var jobDetail = new JobDetailImpl(JobName(ticket), DispacherJobGroupName, typeof(DispatcherJob));
            var trigger = new CalendarIntervalTriggerImpl(TriggerName(ticket), DispacherJobGroupName, IntervalUnit.Second, 30);
            _scheduler.ScheduleJob(jobDetail, trigger);
        }

        public void UnsheduleDispatch(DispatchTicket ticket) {

            _scheduler.UnscheduleJob(new TriggerKey(TriggerName(ticket), DispacherJobGroupName));
        }

        public class DispatcherJob : IJob {
            public void Execute(IJobExecutionContext context) {
                Console.WriteLine("test");
            }
        }

        private static string JobName(DispatchTicket ticket) {
            return "Ticket #" + ticket.Id;
        }

        private static string TriggerName(DispatchTicket ticket) {
            return "Trigger Ticket #" + ticket.Id;
        }
    }
}
