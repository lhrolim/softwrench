using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using softWrench.sW4.Data.Persistence;
using softWrench.sW4.Notifications;
using System.Collections.Concurrent;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using softWrench.sW4.Notifications.Entities;
using softwrench.sw4.Shared2.Util;
using softWrench.sW4.SimpleInjector;
using Newtonsoft.Json.Linq;
using softWrench.sW4.Util;


namespace softWrench.sW4.Notifications {
    public class NotificationFacade {

        private static NotificationFacade _instance = null;
        private const int _hoursToPurge = 24;

        public static readonly IDictionary<string, InMemoryNotificationStream> _notificationStreams = new ConcurrentDictionary<string, InMemoryNotificationStream>();
        private static MaximoHibernateDAO _maxDAO;
        public static IDictionary<string, int> _counter = new ConcurrentDictionary<string, int>();
        protected static MaximoHibernateDAO MaxDAO {
            get {
                if (_maxDAO == null) {
                    _maxDAO =
                        SimpleInjectorGenericFactory.Instance.GetObject<MaximoHibernateDAO>(
                            typeof(MaximoHibernateDAO));
                }
                return _maxDAO;
            }
        }

        //Sets up the default notification stream.
        public static void InitNotificationStreams() {
            var allRoleNotificationBuffer = new InMemoryNotificationStream();
            var query =
                string.Format(
                    "select max(ticketuid) as max, 'servicerequest' as application from ticket where class ='SR' union " +
                    "select max(ticketuid) as max, 'incident' as application from ticket where class ='INCIDENT' union " +
                    "select max(workorderid) as max, 'workoder' as application from workorder union " +
                    "select max(commloguid) as max, 'commlog' as application from commlog");
            var result = MaxDAO.FindByNativeQuery(query, null);
            foreach (var record in result){
                _counter.Add(record["application"], Int32.Parse(record["max"]));
            }
            _notificationStreams["allRole"] = allRoleNotificationBuffer;
        }

        public static InMemoryNotificationStream CurrentNotificationStream() {
            return _notificationStreams["allRole"];
        }

        public static NotificationFacade GetInstance() {
            if (_instance == null) {
                _instance = new NotificationFacade();
            }
            return _instance;
        }

        //Currently only inserts notifications into the 'allRole' stream.
        //This would need to be updated in the future to determine which
        //role stream needs to be inserted based on which roles have a
        //notificationstream attribute set to true
        public void InsertNotificationsIntoStreams(Iesi.Collections.Generic.ISet<Notification> notifications) {
            var streamToUpdate = _notificationStreams["allRole"];
            foreach (var notification in notifications) {
                streamToUpdate.InsertNotificationIntoStream(notification);
            }
        }

        //TODO: Once we figure out rowstamps, use rowstamps instead of Date
        //Currently only updates notifications into the 'allRole' stream.
        //This would need to be updated in the future to determine which
        //role stream needs to be updated based on which roles have a
        //notificationstream attribute set to true
        public void UpdateNotificationReadFlag(string role, string application, string id, string notificationDate, bool isRead)
        {
            var streamToUpdate = _notificationStreams[role];
            streamToUpdate.UpdateNotificationReadFlag(application, id, notificationDate, isRead);
        }

        //Implementation to update read flag for multiple notifications
        public void UpdateNotificationReadFlag(string role, JArray notifications, bool isRead) {
            var streamToUpdate = _notificationStreams[role];

            foreach (var notification in notifications)
            {
                streamToUpdate.UpdateNotificationReadFlag(notification["application"].ToString(),
                    notification["id"].ToString(), notification["notificationDate"].ToString(), true);
            }
        }

        public void UpdateNotificationStreams() {
            var query = string.Format("select 'commlog' as application, null as targetschema,'communication' as label, 'fa-envelope-o' as icon ,CONVERT(varchar(10), commlogid) as id, c.commloguid as uid, " +
                                      "t.ticketid as parentid, c.ownerid as parentuid, CASE c.ownertable WHEN 'SR' THEN 'servicerequest' ELSE c.ownertable END as parentapplication, c.subject as summary, " +
                                      "c.createby as changeby, c.createdate as changedate, c.rowstamp from commlog c " +
                                      "left join ticket t on t.ticketuid = c.ownerid " +
                                      "where createdate >  DATEADD(HOUR,-{0},GETDATE()) and createdate < GETDATE() union " +
                                      "select 'servicerequest' as application, 'editdetail' as targetschema, 'service request' as label, 'fa-ticket' as icon,ticketid as id, ticketuid as uid, null as parentid, null as parentuid, null as parentapplication, description as summary," +
                                      "changeby, changedate, rowstamp from ticket " +
                                      "where changedate > DATEADD(HOUR,-{0},GETDATE()) and changedate < GETDATE() and class='SR' union " +
                                      "select 'incident' as application, 'editdetail' as targetschema, 'incident' as label, 'fa-warning' as icon,ticketid as id, ticketuid as uid, null as parentid, null as parentuid, null as parentapplication, description as summary," +
                                      "changeby, changedate, rowstamp from ticket " +
                                      "where changedate > DATEADD(HOUR,-{0},GETDATE()) and changedate < GETDATE() and class='INCIDENT' union " +
                                      "select 'workorder' as application, 'editdetail' as targetschema, 'work order' as label, 'fa-wrench' as icon,wonum as id, workorderid as uid, null as parentid, null as parentuid, null as parentapplication, description as summary, " +
                                      "changeby, changedate, rowstamp from workorder " +
                                      "where changedate > DATEADD(HOUR,-{0},GETDATE()) and changedate < GETDATE() " +
                                      "order by rowstamp desc", _hoursToPurge);

            var result = MaxDAO.FindByNativeQuery(query, null);

            var streamToUpdate = _notificationStreams["allRole"];
            foreach (var record in result)
            {
                var application = record["application"];
                var targetschema = record["targetschema"];
                var id = record["id"];
                var label = record["label"];
                var icon = record["icon"];
                var uid = Int32.Parse(record["uid"]);
                var flag = "changed";
                if (_counter[application] < uid){
                    flag = "created";
                    _counter[application] = uid;
                }
                var parentid = record["parentid"];
                int parentuid;
                Int32.TryParse(record["parentuid"], out parentuid);
                
                var parentapplication = record["parentapplication"];
                string parentlabel =null;
                if (parentapplication == "servicerequest"){
                    parentlabel = "service request";
                } else if (parentapplication == "WORKORDER"){
                    parentlabel = "work order";
                } else if (parentapplication == "INCIDENT"){
                    parentlabel = "incident";
                }
                var summary = record["summary"];
                var changeby = record["changeby"];
                var changedate = DateTime.Parse(record["changedate"]);
                var rowstamp = BitConverter.ToInt64(StringUtil.GetBytes(record["rowstamp"]), 2);
                var notification = new Notification(application, targetschema, label, icon, id, uid, parentid, parentuid, parentapplication, parentlabel, summary, changeby, changedate, rowstamp, flag);
                streamToUpdate.InsertNotificationIntoStream(notification);
            }
        }


        //By default this is only purging the allRole notification stream
        //by any record changed more than 24 hours ago
        public void PurgeNotificationsFromStream()
        {
            var streamToUpdate = _notificationStreams["allRole"];
            streamToUpdate.PurgeNotificationsFromStream(24);
        }

        public List<Notification> GetNotificationStream(string role) {
            return _notificationStreams[role].GetNotifications();
        }
    }
}
