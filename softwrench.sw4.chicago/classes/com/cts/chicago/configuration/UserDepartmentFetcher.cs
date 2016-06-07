using cts.commons.persistence;
using cts.commons.simpleinjector.Events;
using log4net;
using softWrench.sW4.Metadata;
using softWrench.sW4.Security.Services;

namespace softwrench.sw4.chicago.classes.com.cts.chicago.configuration {
    class UserDepartmentFetcher : ISWEventListener<UserLoginEvent> {

        private static readonly ILog Log = LogManager.GetLogger(typeof(UserDepartmentFetcher));

        private readonly IMaximoHibernateDAO _maximoDao;


        public UserDepartmentFetcher(IMaximoHibernateDAO maximoDao) {
            _maximoDao = maximoDao;
        }

        public void HandleEvent(UserLoginEvent userEvent) {
            if (!MetadataProvider.IsApplicationEnabled("servicerequest") && !MetadataProvider.IsApplicationEnabled("quickservicerequest")) {
                Log.DebugFormat("ignoring department fetching since application is disabled");
                return;
            }
            var user = userEvent.InMemoryUser;
            var results = _maximoDao.FindByNativeQuery(@"select department from person where personid = ?", user.MaximoPersonId);
            if (results != null && results.Count == 1) {
                var result = results[0];
                var department = result["department"];
                user.Genericproperties.Add("department", department);
            }
        }
    }
}
