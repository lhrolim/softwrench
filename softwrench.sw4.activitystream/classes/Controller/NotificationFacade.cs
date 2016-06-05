using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using cts.commons.portable.Util;
using cts.commons.simpleinjector;
using JetBrains.Annotations;
using log4net;
using Newtonsoft.Json.Linq;
using softwrench.sw4.activitystream.classes.Model;
using softwrench.sw4.activitystream.classes.Util;
using softwrench.sw4.api.classes.user;
using softwrench.sw4.user.classes.entities;
using softWrench.sW4.Data.Persistence;
using softWrench.sW4.Metadata.Security;
using softWrench.sW4.Util;
using softWrench.sW4.Security.Services;

namespace softwrench.sw4.activitystream.classes.Controller {
    public class NotificationFacade : ISingletonComponent {


        public static readonly IDictionary<string, InMemoryNotificationStream> NotificationStreams = new ConcurrentDictionary<string, InMemoryNotificationStream>();



        private readonly MaximoHibernateDAO _maxDAO;
        private readonly NotificationQueryBuilder _queryBuilder;
        private readonly ILog _log = LogManager.GetLogger(typeof(NotificationFacade));

        Dictionary<string, string> _securityGroupsNotificationsQueries = new Dictionary<string, string>();
        private readonly NotificationResponseBuilder _responseBuilder;

        public NotificationFacade(MaximoHibernateDAO maxDAO, NotificationQueryBuilder queryBuilder, NotificationResponseBuilder responseBuilder) {
            _maxDAO = maxDAO;
            _queryBuilder = queryBuilder;
            _responseBuilder = responseBuilder;
        }

        //Sets up the default notification stream.
        public void InitNotificationStreams() {
            _responseBuilder.InitMaxIdCache();
        }

        private void UpdateCounters() {

        }

        public void UpdateNotificationReadFlag(int? securityGroup, string application, string id, long rowstamp, bool isRead) {
            var groupName = NotificationSecurityGroupHelper.GetGroupNameById(securityGroup, SecurityFacade.CurrentUser());
            var streamToUpdate = NotificationStreams[groupName];
            streamToUpdate.UpdateNotificationReadFlag(application, id, rowstamp, isRead);
        }

        //Implementation to update read flag for multiple notifications
        public void UpdateNotificationReadFlag(int? securityGroup, JArray notifications, bool isRead) {
            var groupName = NotificationSecurityGroupHelper.GetGroupNameById(securityGroup, SecurityFacade.CurrentUser());
            var streamToUpdate = NotificationStreams[groupName];

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
                var query = securityGroupsNotificationsQuery;
                tasks[i++] = Task.Factory.NewThread(() => ExecuteNotificationsQuery(query.Key, query.Value, currentTime));
            }
            Task.WaitAll(tasks);
        }

        private void ExecuteNotificationsQuery(string groupKey, string query, DateTime currentTime) {
            // TODO: Add checking for security group key in the counter. If a Security Group is added while the application is running this will break the activity stream until the application is restarted.
            if (!_responseBuilder.IsGroupInitialized(groupKey)) {
                return;
            }
            if (!NotificationStreams.Keys.Contains(groupKey)) {
                NotificationStreams[groupKey] = new InMemoryNotificationStream();
            }
            //TODO: why always servicerequest here?
            var formattedQuery = query.Fmt(ActivityStreamConstants.HoursToPurge, _responseBuilder.MaxServiceRequestId(groupKey));
            var queryResult = _maxDAO.FindByNativeQuery(formattedQuery);
            if (!queryResult.Any()) {
                return;
            }
            foreach (var record in queryResult) {
                var notification = _responseBuilder.BuildNotificationFromQueryResult(groupKey, record);
                NotificationStreams[groupKey].InsertNotificationIntoStream(notification);
            }
        }



        //By default this is purging the notification streams
        //of any record changed more than 24 hours old
        public void PurgeNotificationsFromStream() {
            foreach (var stream in NotificationStreams) {
                _log.InfoFormat("Purging notification older than {0} hours for security group {1}", ActivityStreamConstants.HoursToPurge, stream.Key);
                stream.Value.PurgeNotificationsFromStream(ActivityStreamConstants.HoursToPurge);
            }
        }

        [CanBeNull]
        public NotificationResponse GetNotificationStream(string securityGroup) {
            _log.DebugFormat("Getting notifications for security group {0}", securityGroup);
            if (!NotificationStreams.ContainsKey(securityGroup)) {
                _log.WarnFormat("Unable to retrieve notifications for security group {0}. Is it a group with no permissions?", securityGroup);
                return null;
            }
            return NotificationStreams[securityGroup].GetNotifications();
        }

        [NotNull]
        internal NotificationSecurityGroupHelper.NotificationSecurityGroupDTO GetNotificationProfile(int? clientSelectedProfile, InMemoryUser user) {
            return NotificationSecurityGroupHelper.GetNotificationProfile(NotificationStreams, clientSelectedProfile, user);
        }


    }
}
