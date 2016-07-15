using System.Linq;
using cts.commons.persistence;
using Newtonsoft.Json.Linq;
using softwrench.sw4.user.classes.entities;
using softwrench.sw4.user.classes.services;
using softwrench.sw4.user.classes.services.setup;
using softWrench.sW4.Data.Persistence.Dataset.Commons.Person;
using softWrench.sW4.Metadata.Applications;
using softWrench.sW4.Security.Services;

namespace softwrench.sw4.firstsolar.classes.com.cts.firstsolar.dataset {
    public class FirstSolarPersonDataSet : BasePersonDataSet {
        public FirstSolarPersonDataSet(ISWDBHibernateDAO swdbDAO, UserSetupEmailService userSetupEmailService,
            UserLinkManager userLinkManager, UserStatisticsService userStatisticsService,
            UserProfileManager userProfileManager)
            : base(swdbDAO, userSetupEmailService, userLinkManager, userStatisticsService, userProfileManager) {
        }

        protected override User PopulateSwdbUser(ApplicationMetadata application, JObject json, string id,
            string operation) {
            var baseUser = base.PopulateSwdbUser(application, json, id, operation);
            var facilitiesToken = json.GetValue("facilities").ToString();
            var facilitiesProp = baseUser.UserPreferences.GenericProperties.FirstOrDefault(f => f.Key.Equals("facilities"));
            if (facilitiesProp == null) {
                baseUser.UserPreferences.GenericProperties.Add(new GenericProperties() {
                    Key = "facilities",
                    Value = facilitiesToken
                });
            } else {
                facilitiesProp.Value = facilitiesToken;
            }

            return baseUser;
        }

        public override string ClientFilter() {
            return "firstsolar";
        }
    }
}
