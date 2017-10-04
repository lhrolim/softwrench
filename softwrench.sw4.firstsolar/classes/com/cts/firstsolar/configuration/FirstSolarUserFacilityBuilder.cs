using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using cts.commons.persistence;
using cts.commons.portable.Util;
using cts.commons.simpleinjector.Events;
using Iesi.Collections.Generic;
using log4net;
using Newtonsoft.Json.Linq;
using softwrench.sw4.offlineserver.events;
using softwrench.sw4.Shared2.Data.Association;
using softwrench.sw4.user.classes.entities;
using softWrench.sW4.Metadata;
using softWrench.sW4.Security.Services;
using softWrench.sW4.Util;

namespace softwrench.sw4.firstsolar.classes.com.cts.firstsolar.configuration {
    public class FirstSolarUserFacilityBuilder : ISWEventListener<UserLoginEvent>, ISWEventListener<PreSyncEvent>, ISWEventListener<RefreshMetadataEvent> {

        private const string FacilitiesQuery = @"select omw.location, omw.siteid from omworkgroup omw  
                                                left join persongroupview pgv on pgv.persongroup = omw.persongroup 
                                                where pgv.personid = ? and pgv.status = 'ACTIVE' and pgv.persongroup is not null
                                                order by omw.location asc";

        private readonly IMaximoHibernateDAO _dao;
        private readonly ISWDBHibernateDAO _swdbDao;

        private static readonly ILog Log = LogManager.GetLogger(typeof(FirstSolarOfflineSiteIdBuilder));

        private static readonly IDictionary<string, Dictionary<string, List<string>>> FacilitiesCache = new ConcurrentDictionary<string, Dictionary<string, List<string>>>();

        public FirstSolarUserFacilityBuilder(IMaximoHibernateDAO dao, ISWDBHibernateDAO swdbDao) {
            _dao = dao;
            _swdbDao = swdbDao;
        }

        public void HandleEvent(UserLoginEvent userEvent) {
            if (!ApplicationConfiguration.IsClient("firstsolar")) {
                //to avoid issues on dev environments
                return;
            }
            var user = userEvent.InMemoryUser;
            AdjustUserFacilityProperties(user.Genericproperties, user.MaximoPersonId, user.SiteId);
        }

        public List<string> GetFacilityList(string maximoPersonId, string siteid) {
            List<string> facilityList;
            if (FacilitiesCache.ContainsKey(maximoPersonId)) {
                var siteIdDict = FacilitiesCache[maximoPersonId];
                facilityList = FilterBySiteId(siteIdDict, siteid);
                Log.InfoFormat("Facilities from user {0} already on cache, available facilities for siteid {1}: {2}", maximoPersonId, siteid, string.Join(", ", facilityList));
            } else {
                Log.InfoFormat("Fetching facilities for user {0}", maximoPersonId);
                var omworkgroups = _dao.FindByNativeQuery(FacilitiesQuery, maximoPersonId);
                var siteIdDict = new Dictionary<string, List<string>>();
                omworkgroups.ForEach(groupRow => {
                    var facility = groupRow["location"];
                    var dbSiteId = groupRow["siteid"];
                    if (!siteIdDict.ContainsKey(dbSiteId)) {
                        siteIdDict.Add(dbSiteId, new List<string>());
                    }
                    siteIdDict[dbSiteId].Add(facility);
                });
                FacilitiesCache[maximoPersonId] = siteIdDict;
                facilityList = FilterBySiteId(siteIdDict, siteid);
                Log.InfoFormat("Available facilities for user {0} and siteid {1}: {2} ", maximoPersonId, siteid, string.Join(", ", facilityList));
            }
            return facilityList;
        }

        public IDictionary<string, object> AdjustUserFacilityProperties(IDictionary<string, object> genericproperties, string maximoPersonId, string siteid) {
            var facilityList = GetFacilityList(maximoPersonId, siteid);
            // set/updates the available facilities
            if (genericproperties.ContainsKey(FirstSolarConstants.AvailableFacilitiesProp)) {
                genericproperties[FirstSolarConstants.AvailableFacilitiesProp] = facilityList;
            } else {
                genericproperties.Add(FirstSolarConstants.AvailableFacilitiesProp, facilityList);
            }

            // sets the selected facilities or filter the selected by available ones
            if (!genericproperties.ContainsKey(FirstSolarConstants.FacilitiesProp)) {
                Log.InfoFormat("Selected facilities for user {0} not found, using the list of availables facilities.", maximoPersonId);
                genericproperties.Add(FirstSolarConstants.FacilitiesProp, new List<string>(facilityList));
            } else {
                genericproperties[FirstSolarConstants.FacilitiesProp] = FilterFacilities(maximoPersonId, genericproperties[FirstSolarConstants.FacilitiesProp], facilityList);
            }
            return genericproperties;
        }

        public void PopulatePreferredFacilities(User user, string facilitiesToken) {
            var preferences = user.UserPreferences;
            if (preferences == null) {
                user.UserPreferences = new UserPreferences {User = user};
                preferences = user.UserPreferences;
            }
            if (preferences.GenericProperties == null) {
                preferences.GenericProperties = new LinkedHashSet<GenericProperty>();
            }
            var facilitiesProp = preferences.GenericProperties.FirstOrDefault(f => f.Key.Equals(FirstSolarConstants.FacilitiesProp));
            if (facilitiesProp == null) {
                if (facilitiesToken != null) {
                    preferences.GenericProperties.Add(new GenericProperty {
                        Key = FirstSolarConstants.FacilitiesProp,
                        Value = facilitiesToken,
                        UserPreferences = preferences,
                        Type = "list"
                    });
                }
            } else {
                if (facilitiesToken.Equals("")) {
                    preferences.GenericProperties.Remove(facilitiesProp);
                } else {
                    facilitiesProp.Value = facilitiesToken;
                }
            }
        }

        public List<string> FilterFacilities(string maximoPersonId, object selectedFacilities, List<string> availableFacilities) {
            var selectedArray = selectedFacilities as string[];
            var selectedList = selectedArray != null ? selectedArray.ToList() : selectedFacilities as List<string>;
            if (selectedList == null) {
                Log.InfoFormat("Selected facilities for user {0} not of right type, using the list of availables facilities.", maximoPersonId);
                return new List<string>(availableFacilities);
            }

            var filteredFacilities = selectedList.Where(selected => availableFacilities.Any(selected.EqualsIc)).ToList();
            Log.InfoFormat("Selected facilities for user {0} filtered by the available facilities resulting in: {1}", maximoPersonId, string.Join(", ", filteredFacilities));
            return filteredFacilities;
        }

        public IEnumerable<IAssociationOption> GetAvailableFacilities(string maximoPersonId, string siteid) {
            var options = new List<IAssociationOption>();
            var facilityList = GetFacilityList(maximoPersonId, siteid);
            facilityList.ForEach(facility => {
                options.Add(new AssociationOption(facility, facility));
            });
            return options;
        }

        public void HandleEvent(PreSyncEvent eventToDispatch) {
            var user = SecurityFacade.CurrentUser();
            var userSyncData = eventToDispatch.Request.UserData;
            if (userSyncData == null) {
                // did not send userdata: no changes made remotely -> keep server defined facilities
                Log.Debug("No user data sent from client: skipping setting facilities. Server facilities prevail");
                return;
            }
            var requestProperties = userSyncData.Properties;
            object selectedFacilities;
            if (!requestProperties.TryGetValue(FirstSolarConstants.FacilitiesProp, out selectedFacilities) || selectedFacilities == null) {
                return;
            }
            IEnumerable<string> selectedFacilitiesList;
            if (selectedFacilities is JArray) { // client-side payload
                var selectedFacilitiesArray = ((JArray)selectedFacilities);
                selectedFacilitiesList = selectedFacilitiesArray.ToObject<List<string>>();
            } else if (selectedFacilities is IEnumerable<string>) { // server-side payload
                selectedFacilitiesList = ((IEnumerable<string>)selectedFacilities).ToList();
            } else {
                selectedFacilitiesList = Enumerable.Empty<string>();
            }
            // update inmemory
            user.Genericproperties.Remove(FirstSolarConstants.FacilitiesProp);
            user.Genericproperties.Add(FirstSolarConstants.FacilitiesProp, selectedFacilitiesList);

            if (!eventToDispatch.UpdateSwUserDb) {
                return;
            }

            // update swuser db
            var swUser = _swdbDao.FindSingleByQuery<User>(User.UserByUserName, user.Login);
            var selectedFacilitiesToken = string.Join(",", selectedFacilitiesList);
            PopulatePreferredFacilities(swUser, selectedFacilitiesToken);
            _swdbDao.Save(swUser);
            user.DBUser.UserPreferences = swUser.UserPreferences;
        }

        public void HandleEvent(RefreshMetadataEvent eventToDispatch) {
            FacilitiesCache.Clear();
        }

        private static List<string> FilterBySiteId(IDictionary<string, List<string>> siteIdDict, string siteid) {
            if (!string.IsNullOrEmpty(siteid)) {
                return siteIdDict.ContainsKey(siteid) ? siteIdDict[siteid] : new List<string>();
            }
            var result = new List<string>();
            siteIdDict.ToList().ForEach(pair => result.AddRange(pair.Value));
            result.Sort();
            return result;
        }
    }
}