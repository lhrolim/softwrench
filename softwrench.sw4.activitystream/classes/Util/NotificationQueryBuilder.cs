using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using cts.commons.portable.Util;
using Microsoft.Ajax.Utilities;
using softWrench.sW4.Security.Services;
using softwrench.sw4.user.classes.entities;
using softWrench.sW4.Configuration.Services;
using softWrench.sW4.Configuration.Services.Api;

namespace softwrench.sw4.activitystream.classes.Util
{
    public class NotificationQueryBuilder
    {
        private IWhereClauseFacade _whereClauseFacade;

        public NotificationQueryBuilder(IWhereClauseFacade whereClauseFacade)
        {
            _whereClauseFacade = whereClauseFacade;
        }

        public Dictionary<string, string> BuildNotificationsQueries()
        {
            Dictionary<string, string> notificationQueries = new Dictionary<string, string>();
            var securityGroups = UserProfileManager.FetchAllProfiles(true);
            foreach (var securityGroup in securityGroups) {
                var notificationsQuery = BuildNotificationsQuery(securityGroup.Name);
                if (!notificationsQuery.Value.IsNullOrWhiteSpace()) {
                    notificationQueries.Add(notificationsQuery.Key, notificationsQuery.Value);
                }
            }
            return notificationQueries;
        }
        private KeyValuePair<string, string> BuildNotificationsQuery(string securityGroupName)
        {
            var securityGroup = UserProfileManager.FindByName(securityGroupName);
            var roles = securityGroup.Roles;
            string notificationsQuery = "";
            foreach (var role in roles) {
                switch (role.Name.ToLower()) {
                    case "sr":
                        var srResult = _whereClauseFacade.Lookup("servicerequest");
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
                        var incidentResult = _whereClauseFacade.Lookup("incident");
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
                        var woResult = _whereClauseFacade.Lookup("workorder");
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
                //if(role.Name.EqualsIc("sr") || role.Name.EqualsIc("workorders") || role.Name.EqualsIc("incident")) {
                //    notificationsQuery += ActivityStreamConstants.baseQueries.Single(q => q.Key.EqualsIc(role.Name)).Value;

                //    // Append the where clause


                //    notificationsQuery += " UNION ";
                //}
            }
            if (notificationsQuery.EndsWith(" UNION ")) {
                notificationsQuery = notificationsQuery.Substring(0, notificationsQuery.Length - " UNION ".Length);
            }
            return new KeyValuePair<string, string>(securityGroupName, notificationsQuery);
        }
    }
}
