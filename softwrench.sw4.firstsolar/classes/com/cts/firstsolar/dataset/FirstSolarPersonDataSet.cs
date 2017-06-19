using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using cts.commons.persistence;
using cts.commons.portable.Util;
using Iesi.Collections;
using Iesi.Collections.Generic;
using Newtonsoft.Json.Linq;
using softwrench.sw4.firstsolar.classes.com.cts.firstsolar.configuration;
using softwrench.sw4.Shared2.Data.Association;
using softwrench.sw4.user.classes.entities;
using softwrench.sw4.user.classes.services;
using softwrench.sw4.user.classes.services.setup;
using softWrench.sW4.Data;
using softWrench.sW4.Data.API;
using softWrench.sW4.Data.API.Response;
using softWrench.sW4.Data.Persistence.Dataset.Commons.Person;
using softWrench.sW4.Metadata.Applications;
using softWrench.sW4.Metadata.Applications.DataSet;
using softWrench.sW4.Metadata.Security;
using softWrench.sW4.Security.Services;

namespace softwrench.sw4.firstsolar.classes.com.cts.firstsolar.dataset {
    public class FirstSolarPersonDataSet : BasePersonDataSet {

        private readonly FirstSolarUserFacilityBuilder _userFacilityBuilder;

        public FirstSolarPersonDataSet(ISWDBHibernateDAO swdbDAO, UserSetupEmailService userSetupEmailService,
            UserLinkManager userLinkManager, UserStatisticsService userStatisticsService,
            UserProfileManager userProfileManager, FirstSolarUserFacilityBuilder userFacilityBuilder, UserManager userManager, SecurityFacade securityFacade)
            : base(swdbDAO, userSetupEmailService, userLinkManager, userStatisticsService, userProfileManager,userManager, securityFacade) {
            _userFacilityBuilder = userFacilityBuilder;
        }

        protected override async Task<User> PopulateSwdbUser(ApplicationMetadata application, JObject json, string id,
            string operation) {
            var baseUser = await base.PopulateSwdbUser(application, json, id, operation);
            var facilitiesToken = ParseFacilities(json);
            _userFacilityBuilder.PopulatePreferredFacilities(baseUser, facilitiesToken);
            return baseUser;
        }

        public override async Task<ApplicationDetailResult> GetApplicationDetail(ApplicationMetadata application, InMemoryUser user, DetailRequest request) {
            var detail = await base.GetApplicationDetail(application, user, request);
            var maximoPersonId = detail.ResultObject.GetStringAttribute("personid");
            var siteid = detail.ResultObject.GetStringAttribute("locationsite");

            IDictionary<string, object> props = new Dictionary<string, object>();
            if (!string.IsNullOrEmpty(maximoPersonId)) {
                props = _userFacilityBuilder.AdjustUserFacilityProperties(new Dictionary<string, object>(), maximoPersonId, siteid);
            }

            var available = props.ContainsKey(FirstSolarConstants.AvailableFacilitiesProp)
                ? props[FirstSolarConstants.AvailableFacilitiesProp] as List<string>
                : null;
            detail.ResultObject.SetAttribute("availablefacilities", available);

            // no selected prop, nothing possible to do
            if (!props.ContainsKey(FirstSolarConstants.FacilitiesProp)) {
                return detail;
            }

            var result = detail.ResultObject;
            if (!result.ContainsAttribute("facilities")) {
                detail.ResultObject.SetAttribute("facilities", props[FirstSolarConstants.FacilitiesProp]);
                return detail;
            }

            // not possible to filter by available
            if (available == null) {
                return detail;
            }
            
            // filters by available facilities
            var fromDatamap = result.GetAttribute("facilities");
            var filtered = _userFacilityBuilder.FilterFacilities(maximoPersonId, fromDatamap, available);
            detail.ResultObject.SetAttribute("facilities", filtered);

            return detail;
        }

        protected override void AdjustDatamapFromUser(User swUser, DataMap dataMap) {
            base.AdjustDatamapFromUser(swUser, dataMap);
            if (swUser.UserPreferences != null &&
                swUser.UserPreferences.GenericProperties.Any(p => p.Key.Equals(FirstSolarConstants.FacilitiesProp))) {
                var fac = swUser.UserPreferences.GenericProperties.FirstOrDefault(p => p.Key.Equals(FirstSolarConstants.FacilitiesProp));
                if (fac != null) {
                    dataMap.SetAttribute("facilities", fac.Convert());
                }
            }
        }

        public IEnumerable<IAssociationOption> GetAvailableFacilities(OptionFieldProviderParameters parameters) {
            var maximoPersonId = parameters.OriginalEntity.GetStringAttribute("personid");
            var siteid = parameters.OriginalEntity.GetStringAttribute("locationsite");
            return _userFacilityBuilder.GetAvailableFacilities(maximoPersonId, siteid);
        }

        private static string ParseFacilities(JObject json) {
            IList<string> facilities = new List<string>();
            json.GetValue("facilities");
            var facilitiesArray = (((JArray)(json.GetValue("facilities"))));
            if (facilitiesArray == null) {
                return null;
            }
            foreach (var facility in facilitiesArray) {
                facilities.Add(facility.ToString());
            }
            return string.Join(",", facilities);
        }

        public override string ClientFilter() {
            return "firstsolar";
        }
    }
}