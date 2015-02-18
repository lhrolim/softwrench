using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using softWrench.sW4.Data.Persistence;
using softWrench.sW4.Notifications;
using System.Collections.Concurrent;
using softWrench.sW4.Notifications.Entities;
using softwrench.sw4.Shared2.Util;
using softWrench.sW4.SimpleInjector;


namespace softWrench.sW4.Notifications {
    public class NotificationFacade {

        private static NotificationFacade _instance = null;

        public static readonly IDictionary<string, InMemoryNotificationStream> _notificationStreams = new ConcurrentDictionary<string, InMemoryNotificationStream>();
        private MaximoHibernateDAO _maxDAO;

        protected MaximoHibernateDAO MaxDAO {
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

        //Currently only updates notifications into the 'allRole' stream.
        //This would need to be updated in the future to determine which
        //role stream needs to be updated based on which roles have a
        //notificationstream attribute set to true
        public void UpdateNotificationHiddenFlag(string role, string application, string id, bool isHidden) {
            var streamToUpdate = _notificationStreams[role];
            streamToUpdate.UpdateNotificationHiddenFlag(application, id, isHidden);
        }

        //Currently only updates notifications into the 'allRole' stream.
        //This would need to be updated in the future to determine which
        //role stream needs to be updated based on which roles have a
        //notificationstream attribute set to true
        public void UpdateNotificationReadFlag(string role, string application, string id, bool isRead)
        {
            var streamToUpdate = _notificationStreams[role];
            streamToUpdate.UpdateNotificationReadFlag(application, id, isRead);
        }

        public void UpdateNotificationStreams() {
            var hoursToPurge = 24;
            var query = string.Format("select 'CL' + ownertable as application, CONVERT(varchar(10), commlogid) as id, ownerid as parentid, subject as summary, " +
                                      "createby as changeby, createdate as changedate, rowstamp from commlog " +
                                      "where createdate >  DATEADD(HOUR,-{0},GETDATE()) and createdate < GETDATE() union " +
                                      "select class as application, ticketid as id, null as parentid, description as summary," +
                                      "changeby, changedate, rowstamp from ticket " +
                                      "where changedate > DATEADD(HOUR,-{0},GETDATE()) and changedate < GETDATE() union " +
                                      "select 'WO' as application, wonum as id, null as parentid, description as summary, " +
                                      "changeby, changedate, rowstamp from workorder " +
                                      "where changedate > DATEADD(HOUR,-{0},GETDATE()) and changedate < GETDATE() union " +
                                      "select 'PR' as application, prnum as id, null as parentid, description as summary, " +
                                      "changeby, changedate, rowstamp from pr " +
                                      "where changedate > DATEADD(HOUR,-{0},GETDATE()) and changedate < GETDATE() " +
                                      "order by rowstamp desc", hoursToPurge);

            var result = MaxDAO.FindByNativeQuery(query, null);

            var streamToUpdate = _notificationStreams["allRole"];
            foreach (var record in result)
            {
                var application = record["application"];
                var id = record["id"];
                var parentid = record["parentid"];
                var summary = record["summary"];
                var changeby = record["changeby"];
                var changedate = DateTime.Parse(record["changedate"]);
                var rowstamp = BitConverter.ToInt64(StringUtil.GetBytes(record["rowstamp"]), 0);
                var notification = new Notification(application, id, parentid, summary, changeby, changedate, rowstamp);
                streamToUpdate.InsertNotificationIntoStream(notification);
            }
        }

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
