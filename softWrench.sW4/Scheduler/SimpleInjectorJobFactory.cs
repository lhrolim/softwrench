using System;
using Quartz;
using Quartz.Spi;
using Quartz.Util;
using SimpleInjector;
using softWrench.sW4.Security.Services;
using softWrench.sW4.SimpleInjector;

namespace softWrench.sW4.Scheduler {
    public class SimpleInjectorJobFactory : IJobFactory, ISingletonComponent {
        private readonly Container _container;



        public SimpleInjectorJobFactory(Container container) {
            this._container = container;
        }


        public IJob NewJob(TriggerFiredBundle bundle, IScheduler scheduler) {
            try {
                var jobDetail = bundle.JobDetail;
                var jobType = jobDetail.JobType;
                // Return job registrated in container
                return (IJob)_container.GetInstance(jobType);
            } catch (Exception ex) {
                throw new SchedulerException(
                    "Problem instantiating class", ex);
            }
        }

        public void ReturnJob(IJob job) {
            //NOOP
        }
    }
}
