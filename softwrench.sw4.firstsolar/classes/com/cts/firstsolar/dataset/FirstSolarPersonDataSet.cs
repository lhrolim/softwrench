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
using softWrench.sW4.Data.Persistence.Operation;
using softWrench.sW4.Data.Search;
using softWrench.sW4.Metadata.Applications;
using softWrench.sW4.Metadata.Applications.DataSet;
using softWrench.sW4.Metadata.Applications.DataSet.Filter;
using softWrench.sW4.Metadata.Security;
using softWrench.sW4.Security.Services;

namespace softwrench.sw4.firstsolar.classes.com.cts.firstsolar.dataset {
    public class FirstSolarPersonDataSet : BasePersonDataSet {

        private readonly FirstSolarUserFacilityBuilder _userFacilityBuilder;

        public FirstSolarPersonDataSet(ISWDBHibernateDAO swdbDAO, UserSetupEmailService userSetupEmailService,
            UserLinkManager userLinkManager, UserStatisticsService userStatisticsService,
            UserProfileManager userProfileManager, FirstSolarUserFacilityBuilder userFacilityBuilder, UserManager userManager, SecurityFacade securityFacade)
            : base(swdbDAO, userSetupEmailService, userLinkManager, userStatisticsService, userProfileManager, userManager, securityFacade) {
            _userFacilityBuilder = userFacilityBuilder;
        }

        protected override async Task<User> PopulateSwdbUser(ApplicationMetadata application, OperationWrapper wrapper) {
            var baseUser = await base.PopulateSwdbUser(application, wrapper);
            var facilitiesToken = ParseFacilities(wrapper.JSON);

            var crudData = (CrudOperationData)wrapper.OperationData();
            var secondOrg = crudData.GetStringAttribute("secondorg");
            var secondSite = crudData.GetStringAttribute("secondsite");
            var fsUserName = crudData.GetStringAttribute("maxuser_.loginid");

            baseUser.UserName = fsUserName ?? baseUser.UserName;
            _userFacilityBuilder.PopulatePreferredFacilities(baseUser, facilitiesToken);
            HandleSecondaryOrgAndSite(baseUser, secondOrg, secondSite);

            return baseUser;
        }

        private void HandleSecondaryOrgAndSite(User baseUser, string secondOrg, string secondSite) {
            if (secondOrg != null) {
                var savedSecondOrg = baseUser.UserPreferences.GetGenericProperty(FirstSolarConstants.SecondaryOrg);
                if (savedSecondOrg == null) {
                    savedSecondOrg = new GenericProperty {
                        Key = FirstSolarConstants.SecondaryOrg,
                        UserPreferences = baseUser.UserPreferences
                    };
                    baseUser.UserPreferences.GenericProperties.Add(savedSecondOrg);
                }
                savedSecondOrg.Value = secondOrg;
            }

            if (secondSite != null) {
                var savedSecondSite = baseUser.UserPreferences.GetGenericProperty(FirstSolarConstants.SecondarySite);

                if (savedSecondSite == null) {
                    savedSecondSite = new GenericProperty {
                        Key = FirstSolarConstants.SecondarySite,
                        UserPreferences = baseUser.UserPreferences
                    };
                    baseUser.UserPreferences.GenericProperties.Add(savedSecondSite);
                }
                savedSecondSite.Value = secondSite;
            }
        }


        public override async Task<ApplicationDetailResult> GetApplicationDetail(ApplicationMetadata application, InMemoryUser user, DetailRequest request) {
            var detail = await base.GetApplicationDetail(application, user, request);
            var maximoPersonId = detail.ResultObject.GetStringAttribute("personid");
            var siteid = detail.ResultObject.GetStringAttribute("locationsite");

            IDictionary<string, object> props = new Dictionary<string, object>();
            if (!string.IsNullOrEmpty(maximoPersonId)) {
                props = _userFacilityBuilder.AdjustUserFacilityProperties(new Dictionary<string, object>(), maximoPersonId, siteid, user.UserPreferences?.GetGenericProperty(FirstSolarConstants.SecondarySite)?.Value);
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
            if (swUser.UserPreferences != null ) {
                var fac = swUser.UserPreferences.GetGenericProperty(FirstSolarConstants.FacilitiesProp);
                if (fac != null) {
                    dataMap.SetAttribute("facilities", fac.Convert());
                }
                dataMap.SetAttribute("secondorg", swUser.UserPreferences.GetGenericProperty(FirstSolarConstants.SecondaryOrg)?.Value);
                dataMap.SetAttribute("secondsite", swUser.UserPreferences.GetGenericProperty(FirstSolarConstants.SecondarySite)?.Value);
                
            }
        }

        public IEnumerable<IAssociationOption> GetAvailableFacilities(OptionFieldProviderParameters parameters) {
            var maximoPersonId = parameters.OriginalEntity.GetStringAttribute("personid");
            var siteid = parameters.OriginalEntity.GetStringAttribute("locationsite");
            var secondSite = parameters.OriginalEntity.GetStringAttribute("secondsite");
            return _userFacilityBuilder.GetAvailableFacilities(maximoPersonId, siteid,secondSite);
        }

        public override SearchRequestDto FilterSites(AssociationPreFilterFunctionParameters parameters) {

            var searchDto = parameters.BASEDto;
            var orgIdParam = parameters.Relationship.Target.Equals("secondsite") ? "secondorg" : "locationorg";
            var orgId = parameters.OriginalEntity.GetStringAttribute(orgIdParam);
            if (!string.IsNullOrEmpty(orgId)) {
                searchDto.AppendSearchEntry("site.orgid", orgId);
            }
            return searchDto;
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