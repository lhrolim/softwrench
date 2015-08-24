using softWrench.sW4.Data.Persistence.SWDB;
using cts.commons.simpleinjector.Events;
using softWrench.sW4.Util;

namespace softwrench.sw4.Hapag.Data.Init {
    class HapagInitializer : ISWEventListener<ApplicationStartedEvent> {
        private static SWDBHibernateDAO _dao;
        private static HapagRoleInitializer _hapagRoleInitializer;
        private static HapagProfileInitializer _hapagProfileInitializer;

        public void HandleEvent(ApplicationStartedEvent eventToDispatch) {
            if (ApplicationConfiguration.ClientName != "hapag") {
                return;
            }
            var roles =_hapagRoleInitializer.SaveRoles();
            _hapagProfileInitializer.SaveProfiles(roles);
        }

        public HapagInitializer(SWDBHibernateDAO dao, HapagRoleInitializer hapagRoleInitializer, HapagProfileInitializer hapagProfileInitializer) {
            _dao = dao;
            _hapagRoleInitializer = hapagRoleInitializer;
            _hapagProfileInitializer = hapagProfileInitializer;
        }
    }
}
