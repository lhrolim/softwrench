using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using cts.commons.simpleinjector;
using log4net;
using Newtonsoft.Json.Linq;
using softwrench.sw4.activitystream.classes.Model;
using softwrench.sw4.activitystream.classes.Util;
using softWrench.sW4.Data.Persistence;
using softWrench.sW4.Util;

namespace softwrench.sw4.activitystream.classes.Controller {
    public class NotificationFacade : ISingletonComponent {

        private const int HoursToPurge = 24;

        public static readonly IDictionary<string, InMemoryNotificationStream> NotificationStreams = new ConcurrentDictionary<string, InMemoryNotificationStream>();
        public static IDictionary<string, long> Counter = new ConcurrentDictionary<string, long>();

        private readonly MaximoHibernateDAO _maxDAO;
        private readonly NotificationQueryBuilder _queryBuilder;
        private readonly ILog _log = LogManager.GetLogger(typeof(NotificationFacade));

        Dictionary<string, string> _securityGroupsNotificationsQueries = new Dictionary<string, string>();

        public NotificationFacade(MaximoHibernateDAO maxDAO, NotificationQueryBuilder queryBuilder) {
            _maxDAO = maxDAO;
            _queryBuilder = queryBuilder;
        }

        //Sets up the default notification stream.
        public void InitNotificationStreams() {
            //var notificationBuffer = new InMemoryNotificationStream();
            //var srRoleNotificationBuffer = new InMemoryNotificationStream();
            var query =
                string.Format(
                    "select max(ticketuid) as max, 'servicerequest' as application from ticket where class = 'SR' union " +
                    "select max(ticketuid) as max, 'incident' as application from ticket where class ='INCIDENT' union " +
                    "select max(workorderid) as max, 'workorder' as application from workorder union " +
                    "select max(commloguid) as max, 'commlog' as application from commlog union " +
                    "select max(worklogid) as max, 'worklog' as application from worklog");
            var result = _maxDAO.FindByNativeQuery(query, null);
            foreach (var record in result) {
                Counter.Add(record["application"], int.Parse((record["max"] ?? "0")));
            }
            //NotificationStreams["default"] = notificationBuffer;
        }

        public void InsertNotificationsIntoStreams(string securityGroup, List<Notification> notifications) {
            //var defaultStream = NotificationStreams["default"];
            foreach (var notification in notifications) {
                // If the stream has not already been created, create it
                if (!NotificationStreams.Keys.Contains(securityGroup)) {
                    NotificationStreams[securityGroup] = new InMemoryNotificationStream();
                }
                // Add to the proper security group stream
                var streamToUpdate = NotificationStreams[securityGroup];
                streamToUpdate.InsertNotificationIntoStream(notification);
                // Add to the default notification stream
                //defaultStream.InsertNotificationIntoStream(notification);
            }
        }

        public void UpdateNotificationReadFlag(string securityGroup, string application, string id, long rowstamp, bool isRead) {
            var streamToUpdate = NotificationStreams[securityGroup];
            streamToUpdate.UpdateNotificationReadFlag(application, id, rowstamp, isRead);
        }

        //Implementation to update read flag for multiple notifications
        public void UpdateNotificationReadFlag(string securityGroup, JArray notifications, bool isRead) {
            var streamToUpdate = NotificationStreams[securityGroup];

            foreach (var notification in notifications) {
                streamToUpdate.UpdateNotificationReadFlag(notification["application"].ToString(),
                    notification["id"].ToString(), Convert.ToInt64(notification["rowstamp"]), true);
            }
        }

        // This will query against the base notification dataset to pull the necessary columns
        // for notifications and will use the security groups to determine which tables to get
        // notifications for and append any necessary where clauses to the query
        public void UpdateNotificationStreams() {
            _securityGroupsNotificationsQueries = _queryBuilder.BuildNotificationsQueries();
            var currentTime = DateTime.Now.FromServerToRightKind();
            var tasks = new Task[_securityGroupsNotificationsQueries.Count];
            var i = 0;
            foreach (var securityGroupsNotificationsQuery in _securityGroupsNotificationsQueries) {
                _log.DebugFormat("Updating notifications for security group {0}", securityGroupsNotificationsQuery.Key);
                tasks[i++] = Task.Factory.NewThread(() => ExecuteNotificationsQuery(securityGroupsNotificationsQuery, currentTime));
            }
            Task.WaitAll(tasks);
        }

        private void ExecuteNotificationsQuery(KeyValuePair<string, string> securityGroupsNotificationsQuery, DateTime currentTime) {
            var formattedQuery = string.Format(securityGroupsNotificationsQuery.Value,
                        ActivityStreamConstants.HoursToPurge, currentTime, Counter["servicerequest"]);
            var queryResult = _maxDAO.FindByNativeQuery(formattedQuery, null);
            if (!queryResult.Any()) {
                return;
            }
            var notifications = new List<Notification>();
            foreach (var record in queryResult) {
                var application = record["application"];
                var targetschema = record["targetschema"];
                var id = record["id"];
                var label = record["label"];
                var icon = record["icon"];
                var uid = long.Parse(record["uid"]);
                var flag = "changed";
                if (Counter[application] < uid) {
                    flag = "created";
                    Counter[application] = uid;
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
                notifications.Add(notification);
            }
            InsertNotificationsIntoStreams(securityGroupsNotificationsQuery.Key, notifications);
        }

        //By default this is purging the notification streams
        //of any record changed more than 24 hours old
        public void PurgeNotificationsFromStream() {
            foreach (var stream in NotificationStreams) {
                _log.InfoFormat("Purging notification older than {0} hours for security group {1}", ActivityStreamConstants.HoursToPurge, stream.Key);
                stream.Value.PurgeNotificationsFromStream(ActivityStreamConstants.HoursToPurge);
            }
        }

        public NotificationResponse GetNotificationStream(string securityGroup) {
            _log.DebugFormat("Getting notifications for security group {0}", securityGroup);
            if (!NotificationStreams.ContainsKey(securityGroup)) {
                _log.WarnFormat("Unable to retrieve notifications for security group {0}. Is it a group with no permissions?", securityGroup);
                return null;
            }
            return NotificationStreams[securityGroup].GetNotifications();
        }
    }
}
