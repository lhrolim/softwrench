using cts.commons.simpleinjector.Events;
using softWrench.sW4.Data.Persistence.SWDB;
using softWrench.sW4.Util;

namespace softWrench.sW4.Security.Init.Com {
    class ComInitializer : ISWEventListener<ApplicationStartedEvent> {
        private static SWDBHibernateDAO _dao;
        private static ComProfileInitializer _comProfileInitializer;

        public void HandleEvent(ApplicationStartedEvent eventToDispatch) {
            if (ApplicationConfiguration.ClientName != "manchester") {
                return;
            }
            _comProfileInitializer.SaveProfiles();
        }

        public ComInitializer(SWDBHibernateDAO dao, ComProfileInitializer comProfileInitializer) {
            _dao = dao;
            _comProfileInitializer = comProfileInitializer;
        }
    }
}
