using System.Threading.Tasks;
using cts.commons.simpleinjector.Events;
using cts.commons.Util;
using softWrench.sW4.Configuration.Services.Api;
using softWrench.sW4.Scheduler;
using softWrench.sW4.Util;

namespace softwrench.sw4.activitystream.classes.Controller.Jobs {
    public class NotificationStreamUpdateJob : ASwJob {

        //private ILog _log;
        private readonly NotificationFacade _notificationFacade;

        public NotificationStreamUpdateJob(NotificationFacade facade) {
            _notificationFacade = facade;
        }

        public override string Name() {
            return "Activity stream population";
        }

        public override string Description() {
            return "Job to populate the activity stream";
        }

        public override string Cron() {
            var refreshRate = ApplicationConfiguration.NotificationRefreshRate;
            return string.Format("0 */{0} * ? * *", refreshRate);
        }

        public override async Task ExecuteJob() {
            await _notificationFacade.UpdateNotificationStreams();
            _notificationFacade.PurgeNotificationsFromStream();
        }

        public override void HandleEvent(ApplicationStartedEvent eventToDispatch) {
            if (RunAtStartup() && IsEnabled) {
                _notificationFacade.InitNotificationStreams();
                AsyncHelper.RunSync(DoExecute);
            }
        }

        public override bool IsEnabled {
            get {
                return ApplicationConfiguration.ActivityStreamFlag;
            }
        }

        public override bool RunAtStartup() {
            return true;
        }
    }
}
