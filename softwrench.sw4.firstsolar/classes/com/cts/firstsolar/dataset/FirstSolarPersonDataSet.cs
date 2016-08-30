using System.Collections.Generic;
using System.Linq;
using cts.commons.persistence;
using Iesi.Collections;
using Iesi.Collections.Generic;
using Newtonsoft.Json.Linq;
using softwrench.sw4.firstsolar.classes.com.cts.firstsolar.configuration;
using softwrench.sw4.user.classes.entities;
using softwrench.sw4.user.classes.services;
using softwrench.sw4.user.classes.services.setup;
using softWrench.sW4.Data;
using softWrench.sW4.Data.API;
using softWrench.sW4.Data.API.Response;
using softWrench.sW4.Data.Persistence.Dataset.Commons.Person;
using softWrench.sW4.Metadata.Applications;
using softWrench.sW4.Metadata.Security;
using softWrench.sW4.Security.Services;

namespace softwrench.sw4.firstsolar.classes.com.cts.firstsolar.dataset {
    public class FirstSolarPersonDataSet : BasePersonDataSet {

        private readonly FirstSolarUserFacilityBuilder _userFacilityBuilder;

        public FirstSolarPersonDataSet(ISWDBHibernateDAO swdbDAO, UserSetupEmailService userSetupEmailService,
            UserLinkManager userLinkManager, UserStatisticsService userStatisticsService,
            UserProfileManager userProfileManager, FirstSolarUserFacilityBuilder userFacilityBuilder, UserManager userManager)
            : base(swdbDAO, userSetupEmailService, userLinkManager, userStatisticsService, userProfileManager,userManager) {
            _userFacilityBuilder = userFacilityBuilder;
        }

        protected override User PopulateSwdbUser(ApplicationMetadata application, JObject json, string id,
            string operation) {
            var baseUser = base.PopulateSwdbUser(application, json, id, operation);
            var facilitiesToken = ParseFacilities(json);
            _userFacilityBuilder.PopulatePreferredFacilities(baseUser, facilitiesToken);
            return baseUser;
        }

        public override ApplicationDetailResult GetApplicationDetail(ApplicationMetadata application, InMemoryUser user, DetailRequest request) {
            var detail = base.GetApplicationDetail(application, user, request);
            var maximoPersonId = detail.ResultObject.GetStringAttribute("personid");
            var resultProperties = user.Genericproperties;
            if (maximoPersonId != user.MaximoPersonId) {
                //sparing some queries for the myprofile scenario, since this data would already have been fetched upon login
                resultProperties =
                    _userFacilityBuilder.AdjustUserFacilityProperties(new Dictionary<string, object>(), maximoPersonId);
            }
            if (resultProperties.ContainsKey(FirstSolarConstants.FacilitiesProp) && !detail.ResultObject.ContainsAttribute("facilities")) {
                //if the facility was already stored on the database it would already have been set on the AdjustDatamapFromUser method
                detail.ResultObject.SetAttribute("facilities", resultProperties[FirstSolarConstants.FacilitiesProp]);
            }
            if (resultProperties.ContainsKey(FirstSolarConstants.AvailableFacilitiesProp)) {
                detail.ResultObject.SetAttribute("availablefacilities", resultProperties[FirstSolarConstants.AvailableFacilitiesProp]);
            }
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