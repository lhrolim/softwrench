using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Linq;
using cts.commons.simpleinjector;
using Iesi.Collections.Generic;
using Newtonsoft.Json.Linq;
using Quartz.Util;
using softwrench.sw4.activitystream.classes.Model;
using softwrench.sw4.activitystream.classes.Util;
using softwrench.sw4.Shared2.Metadata.Applications.Notification;
using softWrench.sW4.Configuration.Services.Api;
using softWrench.sW4.Data.Persistence;
using softWrench.sW4.Data.Persistence.Relational;
using softWrench.sW4.Data.Persistence.Relational.EntityRepository;
using softWrench.sW4.Data.Search;
using softWrench.sW4.Metadata;
using softWrench.sW4.Metadata.Applications.DataSet;
using softWrench.sW4.Util;

namespace softwrench.sw4.activitystream.classes.Controller {
    public class NotificationFacade : ISingletonComponent
    {
        private const int HoursToPurge = 24;

        public static readonly IDictionary<string, InMemoryNotificationStream> NotificationStreams = new ConcurrentDictionary<string, InMemoryNotificationStream>();
        public static IDictionary<string, long> Counter = new ConcurrentDictionary<string, long>();

        private readonly MaximoHibernateDAO MaxDAO;
        private NotificationQueryBuilder _queryBuilder;

        private DataSet BaseNotificationDataset = new DataSet();
        private DateTime lastRun;

        Dictionary<string, string> securityGroupsNotificationsQueries = new Dictionary<string, string>();

        public NotificationFacade(MaximoHibernateDAO maxDAO, IWhereClauseFacade whereClauseFacade) {
            MaxDAO = maxDAO;
            _queryBuilder = new NotificationQueryBuilder(whereClauseFacade);
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
            var result = MaxDAO.FindByNativeQuery(query, null);
            foreach (var record in result) {
                Counter.Add(record["application"], Int32.Parse((record["max"] ?? "0")));
            }
            //NotificationStreams["default"] = notificationBuffer;
        }

        public void InsertNotificationsIntoStreams(string securityGroup, List<Notification> notifications) {
            //var defaultStream = NotificationStreams["default"];
            foreach (var notification in notifications)
            {
                // If the stream has not already been created, create it
                if (!NotificationStreams.Keys.Contains(securityGroup))
                {
                    NotificationStreams[securityGroup] = new InMemoryNotificationStream();
                }
                // Add to the proper security group stream
                var streamToUpdate = NotificationStreams[securityGroup];
                streamToUpdate.InsertNotificationIntoStream(notification);
                // Add to the default notification stream
                //defaultStream.InsertNotificationIntoStream(notification);
            }
        }

        //Currently only updates notifications into the 'allRole' stream.
        //This would need to be updated in the future to determine which
        //role stream needs to be updated based on which roles have a
        //notificationstream attribute set to true
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
        public void UpdateNotificationStreams()
        {
            securityGroupsNotificationsQueries = _queryBuilder.BuildNotificationsQueries();
            var slicedMetadataEntities = MetadataProvider.GetSlicedMetadataNotificationEntities();
            var currentTime = DateTime.Now.FromServerToRightKind();
            foreach (var notificationQuery in securityGroupsNotificationsQueries)
            {
                var formattedQuery = string.Format(notificationQuery.Value , HoursToPurge, currentTime, Counter["servicerequest"]);
                var queryResult = MaxDAO.FindByNativeQuery(formattedQuery, null);
                List<Notification> notifications = new List<Notification>();
                foreach (var record in queryResult)
                {
                    var application = record["application"];
                    var targetschema = record["targetschema"];
                    var id = record["id"];
                    var label = record["label"];
                    var icon = record["icon"];
                    var uid = Int64.Parse(record["uid"]);
                    var flag = "changed";
                    if (Counter[application] < uid)
                    {
                        flag = "created";
                        Counter[application] = uid;
                    }
                    var parentid = record["parentid"];
                    long parentuid = -1;
                    if (record["parentuid"] != null)
                    {
                        Int64.TryParse(record["parentuid"], out parentuid);
                    }
                    var parentapplication = record["parentapplication"];
                    string parentlabel = null;
                    if (parentapplication == "servicerequest")
                    {
                        parentlabel = "service request";
                    }
                    else if (parentapplication == "WORKORDER")
                    {
                        parentlabel = "work order";
                    }
                    else if (parentapplication == "INCIDENT")
                    {
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
                InsertNotificationsIntoStreams(notificationQuery.Key, notifications);
            }
        }


        //By default this is purging the notification streams
        //of any record changed more than 24 hours old
        public void PurgeNotificationsFromStream() {
            foreach (var stream in NotificationStreams) {
                stream.Value.PurgeNotificationsFromStream(24);
            }
        }
        
        public NotificationResponse GetNotificationStream(string securityGroup) {
            return NotificationStreams[securityGroup].GetNotifications();
        }
    }
}
