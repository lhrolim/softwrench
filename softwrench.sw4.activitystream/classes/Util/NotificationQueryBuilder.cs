using System.Collections.Generic;
using System.Linq;
using System.Text;
using cts.commons.portable.Util;
using cts.commons.simpleinjector;
using log4net;
using Microsoft.Ajax.Utilities;
using softwrench.sw4.activitystream.classes.Model;
using softWrench.sW4.Security.Services;
using softwrench.sw4.user.classes.entities;
using softWrench.sW4.Configuration.Services.Api;
using softWrench.sW4.Data.Persistence.Relational.QueryBuilder;
using softWrench.sW4.Metadata;
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
            var defaultQuery = GetDefaultQuery();
            if (!string.IsNullOrEmpty(defaultQuery)) {
                notificationQueries.Add(ActivityStreamConstants.DefaultStreamName, defaultQuery);
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
                context.CurrentSelectedProfile = securityGroup.Id;
            }
            foreach (var role in roles) {
                _log.DebugFormat("Appending {0} query for security group {1}", role.Name.ToLower(), securityGroup.Name);
                notificationsQuery += AppendQuery(role.Name.ToLower(), context);
            }
            if (notificationsQuery.EndsWith(" UNION ")) {
                notificationsQuery = notificationsQuery.Substring(0, notificationsQuery.Length - " UNION ".Length);
            }
            return new KeyValuePair<string, string>(securityGroup.Name, notificationsQuery);
        }

        private string AppendQuery(string key, ContextHolder context) {
            var applicationName = NotificationSecurityGroupHelper.GetApplicationNameByRole(key);
            if (!applicationName.EqualsAny("servicerequest", "workorder", "incident")) {
                return "";
            }
            var sb = new StringBuilder();

            var whereClauseResult = _whereClauseFacade.Lookup(applicationName, null, context);
            //to apply eventual method implementations
            var convertedValue = DataConstraintsWhereBuilder.GetConvertedWhereClause(whereClauseResult, SecurityFacade.CurrentUser(), "");
            var whereClause = whereClauseResult.IsEmpty() ? " UNION " : " AND " + convertedValue + " UNION ";

            sb.Append(GetRoleQuery(applicationName)).Append(whereClause);
            sb.Append(GetRoleQuery(applicationName + "worklogs")).Append(whereClause);

            if (!applicationName.EqualsIc("workorder")) {
                sb.Append(GetRoleQuery(applicationName + "commlogs")).Append(whereClause);
            }

            return sb.ToString();
        }




        private string GetRoleQuery(string key) {
            if (!ActivityStreamConstants.BaseQueries.ContainsKey(key)) {
                _log.WarnFormat("base query {0} not found for activitystream setup", key);
                return "";
            }
            return ActivityStreamConstants.BaseQueries.Single(q => q.Key.EqualsIc(key)).Value;
        }

        private string GetDefaultQuery() {
            var notificationsQuery = "";

            if (MetadataProvider.IsApplicationEnabled("servicerequest")) {
                notificationsQuery += AppendQuery("sr", null);
            }

            if (MetadataProvider.IsApplicationEnabled("incident")) {
                notificationsQuery += AppendQuery("incident", null);
            }

            if (MetadataProvider.IsApplicationEnabled("workorder")) {
                notificationsQuery += AppendQuery("workorders", null);
            }


            if (notificationsQuery.EndsWith(" UNION ")) {
                notificationsQuery = notificationsQuery.Substring(0, notificationsQuery.Length - " UNION ".Length);
            }
            if (string.IsNullOrEmpty(notificationsQuery)) {
                return null;
            }

            return notificationsQuery;
        }
    }
}
