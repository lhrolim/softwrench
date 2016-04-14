using System.Collections.Generic;
using System.Linq;
using softwrench.sw4.dashboard.classes.model.entities;
using softwrench.sw4.Shared2.Data.Association;
using softWrench.sW4.Data.Persistence.Dataset.Commons;
using softWrench.sW4.Metadata.Applications.DataSet;
using softWrench.sW4.Security.Services;

namespace softwrench.sw4.dashboard.classes.controller {
    public class DashboardDataSet : SWDBApplicationDataset {


        public IEnumerable<IAssociationOption> GetExistingDashboards(OptionFieldProviderParameters parameters) {
            var user = SecurityFacade.CurrentUser();
            var profiles = user.Profiles;
            var applications = user.MergedUserProfile.Permissions.Where(p => !p.HasNoPermissions).Select(p => p.ApplicationName).ToArray();

            var list = SWDAO.FindByQuery<Dashboard>(Dashboard.ByUserAndApplications(profiles.Select(s => s.Id), true), user.UserId, applications);
            var options = new List<MultiValueAssociationOption>();
            foreach (var dashboard in list) {
                var label = dashboard.Active ? dashboard.Title : dashboard.Title + " (INACTIVE)";
                var dict = new Dictionary<string, object>();
                dict.Add("active", dashboard.Active);
                options.Add(new MultiValueAssociationOption(dashboard.Id.ToString(), label, dict));
            }

            return options;
        }


        public override string ApplicationName() {
            return "_dashboard";
        }
    }
}
