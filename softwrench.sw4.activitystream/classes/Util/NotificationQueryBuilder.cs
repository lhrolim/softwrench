using System.Collections.Generic;
using System.Linq;
using cts.commons.portable.Util;
using cts.commons.simpleinjector;
using Common.Logging;
using Microsoft.Ajax.Utilities;
using softWrench.sW4.Security.Services;
using softwrench.sw4.user.classes.entities;
using softWrench.sW4.Configuration.Services.Api;
using softWrench.sW4.Security.Context;

namespace softwrench.sw4.activitystream.classes.Util {
    public class NotificationQueryBuilder : ISingletonComponent {
        private readonly IWhereClauseFacade _whereClauseFacade;
        private readonly ILog _log = LogManager.GetLogger(typeof(NotificationQueryBuilder));

        public NotificationQueryBuilder(IWhereClauseFacade whereClauseFacade) {
            _whereClauseFacade = whereClauseFacade;
        }
        public Dictionary<string, string> BuildNotificationsQueries() {
            Dictionary<string, string> notificationQueries = new Dictionary<string, string>();
            var securityGroups = UserProfileManager.FetchAllProfiles(true);
            foreach (var securityGroup in securityGroups) {
                var notificationsQuery = BuildNotificationsQuery(securityGroup);
                if (!notificationsQuery.Value.IsNullOrWhiteSpace()) {
                    notificationQueries.Add(notificationsQuery.Key, notificationsQuery.Value);
                }
            }
            return notificationQueries;
        }
        private KeyValuePair<string, string> BuildNotificationsQuery(UserProfile securityGroup) {
            _log.DebugFormat("Building notifiations query for security group {0}", securityGroup.Name);
            var roles = securityGroup.Roles;
            string notificationsQuery = "";
            var context = new ContextHolder();
            if (securityGroup.Id != null) {
                context.UserProfiles = new SortedSet<int?> { securityGroup.Id };
            }
            foreach (var role in roles) {
                switch (role.Name.ToLower()) {
                    case "sr":
                        _log.DebugFormat("Appening Service Request query for security group {0}", securityGroup.Name);
                        var srResult = _whereClauseFacade.Lookup("servicerequest", null, context);
                        var srQuery = srResult.Query.Trim() != "" ? " AND " + srResult.Query + " UNION " : " UNION ";
                        notificationsQuery += ActivityStreamConstants.baseQueries.Single(q => q.Key.EqualsIc(role.Name)).Value;
                        //append where clause
                        notificationsQuery += srQuery;
                        notificationsQuery += ActivityStreamConstants.baseQueries.Single(q => q.Key.EqualsIc(role.Name + "Worklogs")).Value;
                        //append where clause
                        notificationsQuery += srQuery;
                        notificationsQuery += ActivityStreamConstants.baseQueries.Single(q => q.Key.EqualsIc(role.Name + "Commlogs")).Value;
                        //append where clause
                        notificationsQuery += srQuery;
                        break;
                    case "incident":
                        _log.DebugFormat("Appening Incident query for security group {0}", securityGroup.Name);
                        var incidentResult = _whereClauseFacade.Lookup("incident", null, context);
                        var incidentQuery = incidentResult.Query.Trim() != "" ? " AND " + incidentResult.Query + " UNION " : " UNION ";
                        notificationsQuery += ActivityStreamConstants.baseQueries.Single(q => q.Key.EqualsIc(role.Name)).Value;
                        //append where clause
                        notificationsQuery += incidentQuery;
                        notificationsQuery += ActivityStreamConstants.baseQueries.Single(q => q.Key.EqualsIc(role.Name + "Worklogs")).Value;
                        //append where clause
                        notificationsQuery += incidentQuery;
                        notificationsQuery += ActivityStreamConstants.baseQueries.Single(q => q.Key.EqualsIc(role.Name + "Commlogs")).Value;
                        //append where clause
                        notificationsQuery += incidentQuery;
                        break;
                    case "workorders":
                        _log.DebugFormat("Appening Workorder query for security group {0}", securityGroup.Name);
                        var woResult = _whereClauseFacade.Lookup("workorder", null, context);
                        var woQuery = woResult.Query.Trim() != "" ? " AND " + woResult.Query + " UNION " : " UNION ";
                        notificationsQuery += ActivityStreamConstants.baseQueries.Single(q => q.Key.EqualsIc(role.Name)).Value;
                        //append where clause
                        notificationsQuery += woQuery;
                        notificationsQuery += ActivityStreamConstants.baseQueries.Single(q => q.Key.EqualsIc(role.Name + "Worklogs")).Value;
                        //append where clause
                        notificationsQuery += woQuery;
                        break;
                }
            }
            if (notificationsQuery.EndsWith(" UNION ")) {
                notificationsQuery = notificationsQuery.Substring(0, notificationsQuery.Length - " UNION ".Length);
            }
            return new KeyValuePair<string, string>(securityGroup.Name, notificationsQuery);
        }
    }
}
