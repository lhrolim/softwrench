using System.Collections.Generic;
using System.Linq;
using cts.commons.portable.Util;
using Microsoft.Ajax.Utilities;
using softWrench.sW4.Security.Services;
using softwrench.sw4.user.classes.entities;
using softWrench.sW4.Configuration.Services.Api;
using softWrench.sW4.Security.Context;

namespace softwrench.sw4.activitystream.classes.Util {
    public class NotificationQueryBuilder {
        private readonly IWhereClauseFacade _whereClauseFacade;
        private readonly IContextLookuper _contextLookuper;

        public NotificationQueryBuilder(IWhereClauseFacade whereClauseFacade, IContextLookuper contextLookuper) {
            _whereClauseFacade = whereClauseFacade;
            _contextLookuper = contextLookuper;
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
            var roles = securityGroup.Roles;
            string notificationsQuery = "";
            var context = new ContextHolder();
            if (securityGroup.Id != null) {
                context.UserProfiles = new SortedSet<int?> { securityGroup.Id };
            }
            foreach (var role in roles) {
                switch (role.Name.ToLower()) {
                    case "sr":
                        var srResult = _whereClauseFacade.Lookup("servicerequest", null, context);
                        var srQuery = srResult.Query.Trim() != "" ? " AND " + srResult.Query : "";
                        notificationsQuery += ActivityStreamConstants.baseQueries.Single(q => q.Key.EqualsIc(role.Name)).Value;
                        //append where clause
                        notificationsQuery += srQuery;
                        notificationsQuery += " UNION ";
                        notificationsQuery += ActivityStreamConstants.baseQueries.Single(q => q.Key.EqualsIc(role.Name + "Worklogs")).Value;
                        //append where clause
                        notificationsQuery += srQuery;
                        notificationsQuery += " UNION ";
                        notificationsQuery += ActivityStreamConstants.baseQueries.Single(q => q.Key.EqualsIc(role.Name + "Commlogs")).Value;
                        //append where clause
                        notificationsQuery += srQuery;
                        notificationsQuery += " UNION ";
                        break;
                    case "incident":
                        var incidentResult = _whereClauseFacade.Lookup("incident", null, context);
                        var incidentQuery = incidentResult.Query.Trim() != "" ? " AND " + incidentResult.Query : "";
                        notificationsQuery += ActivityStreamConstants.baseQueries.Single(q => q.Key.EqualsIc(role.Name)).Value;
                        //append where clause
                        notificationsQuery += incidentQuery;
                        notificationsQuery += " UNION ";
                        notificationsQuery += ActivityStreamConstants.baseQueries.Single(q => q.Key.EqualsIc(role.Name + "Worklogs")).Value;
                        //append where clause
                        notificationsQuery += incidentQuery;
                        notificationsQuery += " UNION ";
                        notificationsQuery += ActivityStreamConstants.baseQueries.Single(q => q.Key.EqualsIc(role.Name + "Commlogs")).Value;
                        //append where clause
                        notificationsQuery += incidentQuery;
                        notificationsQuery += " UNION ";
                        break;
                    case "workorders":
                        var woResult = _whereClauseFacade.Lookup("workorder", null, context);
                        var woQuery = woResult.Query.Trim() != "" ? " AND " + woResult.Query : "";
                        notificationsQuery += ActivityStreamConstants.baseQueries.Single(q => q.Key.EqualsIc(role.Name)).Value;
                        //append where clause
                        notificationsQuery += woQuery;
                        notificationsQuery += " UNION ";
                        notificationsQuery += ActivityStreamConstants.baseQueries.Single(q => q.Key.EqualsIc(role.Name + "Worklogs")).Value;
                        //append where clause
                        notificationsQuery += woQuery;
                        notificationsQuery += " UNION ";
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
