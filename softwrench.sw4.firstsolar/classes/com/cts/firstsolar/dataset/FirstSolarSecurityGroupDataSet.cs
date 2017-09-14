using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using softwrench.sw4.user.classes.entities;
using softwrench.sW4.Shared2.Metadata;
using softWrench.sW4.Data.Persistence.Dataset.Commons.SWDB;
using softWrench.sW4.Security.Services;

namespace softwrench.sw4.firstsolar.classes.com.cts.firstsolar.dataset {

    public class FirstSolarSecurityGroupDataSet : BaseUserProfileDataSet {

        public FirstSolarSecurityGroupDataSet(UserProfileManager userProfileManager) : base(userProfileManager) {
        }

        protected override IDictionary<string, object> CreateAppDict(UserProfile profileOb, CompleteApplicationMetadataDefinition app) {
            var dict = base.CreateAppDict(profileOb, app);
            if (app.ApplicationName == "otherworkorder") {
                dict["title"] = "Group Work Orders";
            }
            else if (app.ApplicationName == "workorder") {
                dict["title"] = "Technician Work Orders";
            }
            return dict;
        }

        public override string ClientFilter() {
            return "firstsolar";
        }
    }
}
