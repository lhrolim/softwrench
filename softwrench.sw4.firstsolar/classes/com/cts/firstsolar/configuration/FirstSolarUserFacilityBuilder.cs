using System.Collections.Generic;
using System.Linq;
using cts.commons.persistence;
using cts.commons.simpleinjector.Events;
using softwrench.sw4.offlineserver.events;
using softWrench.sW4.Metadata.Security;
using softWrench.sW4.Security.Services;
using softWrench.sW4.Util;

namespace softwrench.sw4.firstsolar.classes.com.cts.firstsolar.configuration {
    public class FirstSolarUserFacilityBuilder : ISWEventListener<UserLoginEvent>, ISWEventListener<PreSyncEvent> {


        private readonly IMaximoHibernateDAO _dao;

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

        public FirstSolarUserFacilityBuilder(IMaximoHibernateDAO dao) {
            _dao = dao;
        }


        public void HandleEvent(UserLoginEvent userEvent) {
            if (!ApplicationConfiguration.IsClient("firstsolar")) {
                //to avoid issues on dev environments
                return;
            }
            var user = userEvent.InMemoryUser;
            AdjustUserFacilityProperties(user.Genericproperties, user.MaximoPersonId);
        }

        public IDictionary<string, object> AdjustUserFacilityProperties(IDictionary<string, object> genericproperties, string maximoPersonId) {
            var hasStoredFacilities = genericproperties.ContainsKey("sync.facilities");

            if (hasStoredFacilities && genericproperties.ContainsKey("sync.availablefacilities")) {
                //the list of facilities is already saved on the swdb database, let's use it
                return genericproperties;
            }
            //otherwise that list should be populated defaulting to the persongroups of the user
            var groups =
                _dao.FindByNativeQuery("select persongroup from persongroupview where personid = ? and status = 'ACTIVE'",
                    maximoPersonId);


            var facilityList = new List<string>();
            foreach (var groupRow in groups) {
                var group = groupRow["persongroup"];
                if (PersonGroupFacilityMap.ContainsKey(@group)) {
                    facilityList.AddRange(PersonGroupFacilityMap[@group]);
                }
            }

            if (genericproperties.ContainsKey("sync.availablefacilities")) {
                genericproperties.Remove("sync.availablefacilities");
            }
            genericproperties.Add("sync.availablefacilities", facilityList);

            return genericproperties;

        }


        public void HandleEvent(PreSyncEvent eventToDispatch) {
            var user = SecurityFacade.CurrentUser();
            AdjustUserFacilityProperties(user.Genericproperties, user.MaximoPersonId);
        }
    }
}