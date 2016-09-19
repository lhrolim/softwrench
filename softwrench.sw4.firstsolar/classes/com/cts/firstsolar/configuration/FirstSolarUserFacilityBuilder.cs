using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using cts.commons.persistence;
using cts.commons.simpleinjector.Events;
using Iesi.Collections.Generic;
using log4net;
using Newtonsoft.Json.Linq;
using softwrench.sw4.offlineserver.dto;
using softwrench.sw4.offlineserver.events;
using softwrench.sw4.user.classes.entities;
using softWrench.sW4.Security.Services;
using softWrench.sW4.Util;

namespace softwrench.sw4.firstsolar.classes.com.cts.firstsolar.configuration {
    public class FirstSolarUserFacilityBuilder : ISWEventListener<UserLoginEvent>, ISWEventListener<PreSyncEvent> {


        private readonly IMaximoHibernateDAO _dao;
        private readonly ISWDBHibernateDAO _swdbDao;

        private static readonly ILog Log = LogManager.GetLogger(typeof (FirstSolarOfflineSiteIdBuilder));

        private static readonly IDictionary<string, string[]> PersonGroupFacilityMap = new Dictionary<string, string[]>()
        {
            { "1801-ACS", new []{"ACS"} },
            { "1801-APX", new []{"APX"} },
            { "1801-AVV", new []{"AVV"} },
            { "1801-CIM", new []{"CIM","REE","LMS","GAL","STT","ALM","HON","MSS","MLK","CBLA","SDVL","MAN","OTE","BRS"} },
            { "1801-DCR", new []{"DCR"} },
            { "1801-GRN", new []{"GRN"} },
            { "1801-SSN", new []{"SSN"} },
            { "1801-SPX", new []{"SPX"} },
            { "1803-ADB", new []{"ADB"} },
            { "1803-AST", new []{"AST"} },
            { "1803-AVS", new []{"AVS","ALP"} },
            { "1803-KBS", new []{"KBS"} },
            { "1803-CAL", new []{"CAL"} },
            { "1803-CVS", new []{"CVS","SGSO","SGAH","SGAK","SES"} },
            { "1803-DSS", new []{"DSS","BLY"} },
            { "1803-IVS", new []{"IVS","IVW","DUNS"} },
            { "1803-RSS", new []{"RSS"} },
            { "1803-SLS", new []{"SLS"} },
            { "1803-TPZ", new []{"TPZ","AES","KNS","KSS","ORS","CID","LHS","BWS","CGL","CCS","NSS"} },
            { "1808-SAR", new []{"SAR","TIL","AMR","BEL","AMH","WAL"} },
            { "4801-ABH", new []{"ABH"} },
            { "4801-ANY", new []{"ANY"} },
            { "4801-RTA", new []{"RTA"} },
            { "4801-VER", new []{"VER"} },
            { "6801-CLN", new []{"CLN"} },
        };

        public FirstSolarUserFacilityBuilder(IMaximoHibernateDAO dao, ISWDBHibernateDAO swdbDao) {
            _dao = dao;
            _swdbDao = swdbDao;
        }


        public async void HandleEvent(UserLoginEvent userEvent) {
            if (!ApplicationConfiguration.IsClient("firstsolar")) {
                //to avoid issues on dev environments
                return;
            }
            var user = userEvent.InMemoryUser;
            await AdjustUserFacilityProperties(user.Genericproperties, user.MaximoPersonId);
        }

        public async Task<IDictionary<string, object>> AdjustUserFacilityProperties(IDictionary<string, object> genericproperties, string maximoPersonId) {
            var hasStoredFacilities = genericproperties.ContainsKey(FirstSolarConstants.FacilitiesProp);

            if (hasStoredFacilities && genericproperties.ContainsKey(FirstSolarConstants.AvailableFacilitiesProp)) {
                Log.DebugFormat("user already has stored facilities: {0}, returning", genericproperties[FirstSolarConstants.FacilitiesProp]);
                //the list of facilities is already saved on the swdb database, let's use it
                return genericproperties;
            }
            //otherwise that list should be populated defaulting to the persongroups of the user
            var groups =
                await _dao.FindByNativeQueryAsync("select persongroup from persongroupview where personid = ? and status = 'ACTIVE' and persongroup is not null",
                    maximoPersonId);

            Log.InfoFormat("fetching persongroups for user {0}",maximoPersonId);

            var facilityList = new List<string>();
            foreach (var groupRow in groups) {
                var group = groupRow["persongroup"];
                if (PersonGroupFacilityMap.ContainsKey(@group)) {
                    facilityList.AddRange(PersonGroupFacilityMap[@group]);
                }
            }

            Log.InfoFormat("Available facilities for user {0}: {1} ", maximoPersonId,facilityList);

            if (!hasStoredFacilities) {
                if (genericproperties.ContainsKey(FirstSolarConstants.FacilitiesProp)) {
                    genericproperties.Remove(FirstSolarConstants.FacilitiesProp);
                }
                genericproperties.Add(FirstSolarConstants.FacilitiesProp, facilityList);
            }

            if (genericproperties.ContainsKey(FirstSolarConstants.AvailableFacilitiesProp)) {
                genericproperties.Remove(FirstSolarConstants.AvailableFacilitiesProp);
            }
            genericproperties.Add(FirstSolarConstants.AvailableFacilitiesProp, facilityList);

            return genericproperties;
        }

        public void PopulatePreferredFacilities(User user, string facilitiesToken) {
            var preferences = user.UserPreferences;
            if (preferences.GenericProperties == null) {
                preferences.GenericProperties = new LinkedHashSet<GenericProperty>();
            }
            var facilitiesProp = preferences.GenericProperties.FirstOrDefault(f => f.Key.Equals(FirstSolarConstants.FacilitiesProp));
            if (facilitiesProp == null) {
                preferences.GenericProperties.Add(new GenericProperty() {
                    Key = FirstSolarConstants.FacilitiesProp,
                    Value = facilitiesToken,
                    UserPreferences = preferences,
                    Type = "list"
                });
            } else {
                if (facilitiesToken.Equals("")) {
                    preferences.GenericProperties.Remove(facilitiesProp);
                } else {
                    facilitiesProp.Value = facilitiesToken;
                }
            }
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
            // update swuser db
            var swUser = _swdbDao.FindSingleByQuery<User>(User.UserByUserName, user.Login);
            var selectedFacilitiesToken = string.Join(",", selectedFacilitiesList);
            PopulatePreferredFacilities(swUser, selectedFacilitiesToken);
            _swdbDao.Save(swUser);
            user.DBUser.UserPreferences = swUser.UserPreferences;
        }
    }
}