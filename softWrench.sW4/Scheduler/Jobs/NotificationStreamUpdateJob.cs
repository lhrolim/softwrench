using System;
using Common.Logging;
using Quartz;
using softWrench.sW4.Configuration;
using softWrench.sW4.Configuration.Services;
using softWrench.sW4.Configuration.Services.Api;
using softWrench.sW4.Notifications;
using softWrench.sW4.Scheduler.Interfaces;
using softWrench.sW4.SimpleInjector.Events;
using System.Threading.Tasks;

namespace softWrench.sW4.Scheduler.Jobs {
    public class NotificationStreamUpdateJob : ASwJob {

        //private ILog _log;
        private readonly IConfigurationFacade _facade;

        public NotificationStreamUpdateJob(IConfigurationFacade facade)
        {
            _facade = facade;
        }

        public override string Name() {
            return "Activity stream population";
        }

        public override string Description() {
            return "Job to populate the activity stream";
        }

        public override string Cron() {
            return "0 */5 * ? * *";
        }

        public override void ExecuteJob() {
            var notificationFacade = NotificationFacade.GetInstance();
            notificationFacade.UpdateNotificationStreams();
            notificationFacade.PurgeNotificationsFromStream();
        }

        public override void HandleEvent(ApplicationStartedEvent eventToDispatch) {
             NotificationFacade.InitNotificationStreams();
            if (RunAtStartup()) {
                Task.Factory.StartNew(DoExecute);
            }
        }

        public override bool RunAtStartup() {
            return true;
        }
    }
}
