using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using cts.commons.simpleinjector;
using softwrench.sw4.activitystream.classes.Model;
using softWrench.sW4.Data.Persistence;
using softWrench.sW4.Security.Services;

namespace softwrench.sw4.activitystream.classes.Util {
    public class NotificationResponseBuilder : ISingletonComponent {

        /// <summary>
        /// This dict holds, for each profile, including the default a dictionary of application/maxticketuid available --> used to track down creations/updates
        /// </summary>
        public static Dictionary<string, Dictionary<string, long>> MaxIdCache = new Dictionary<string, Dictionary<string, long>>();

        private readonly MaximoHibernateDAO _maxDAO;

        public NotificationResponseBuilder(MaximoHibernateDAO maxDAO) {
            _maxDAO = maxDAO;
        }


        public void InitMaxIdCache() {
            MaxIdCache.Add(ActivityStreamConstants.DefaultStreamName, new Dictionary<string, long>());
            var securityGroups = UserProfileManager.FetchAllProfiles(true);
            var query = ActivityStreamConstants.BaseIdCacheQuery;
            var result = _maxDAO.FindByNativeQuery(query, null);
            foreach (var record in result) {
                var application = record["application"];
                var idValue = int.Parse((record["max"] ?? "0"));
                foreach (var securityGroup in securityGroups) {
                    if (!MaxIdCache.ContainsKey(securityGroup.Name)) {
                        MaxIdCache.Add(securityGroup.Name, new Dictionary<string, long>());
                    }
                    MaxIdCache[securityGroup.Name].Add(application, idValue);
                }
                MaxIdCache[ActivityStreamConstants.DefaultStreamName].Add(application, idValue);
            }
        }

        public Notification BuildNotificationFromQueryResult(string groupKey, Dictionary<string, string> record) {
            var application = record["application"];
            var targetschema = record["targetschema"];
            var id = record["id"];
            var label = record["label"];
            var icon = record["icon"];
            var uid = long.Parse(record["uid"]);
            var flag = "changed";
            if (MaxIdCache[groupKey][application] < uid) {
                flag = "created";
                MaxIdCache[groupKey][application] = uid;
            }
            var parentid = record["parentid"];
            long parentuid = -1;
            if (record["parentuid"] != null) {
                long.TryParse(record["parentuid"], out parentuid);
            }
            var parentapplication = record["parentapplication"];
            string parentlabel = null;
            if (parentapplication == "servicerequest") {
                parentlabel = "service request";
            } else if (parentapplication == "WORKORDER") {
                parentlabel = "work order";
            } else if (parentapplication == "INCIDENT") {
                parentlabel = "incident";
            }
            var summary = record["summary"];
            var changeby = record["changeby"];
            var changedate = DateTime.Parse(record["changedate"]);
            var rowstamp = Convert.ToInt64(record["rowstamp"]);
            var notification = new Notification(application, targetschema, label, icon, id, uid, parentid,
                parentuid, parentapplication, parentlabel, summary, changeby, changedate, rowstamp, flag);
            return notification;
        }

        public bool IsGroupInitialized(string groupKey) {
            return MaxIdCache.ContainsKey(groupKey);
        }

        public long MaxServiceRequestId(string groupKey) {
            return MaxIdCache[groupKey]["servicerequest"];
        }
    }
}
