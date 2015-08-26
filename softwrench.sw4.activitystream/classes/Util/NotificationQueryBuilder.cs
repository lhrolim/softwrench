using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using cts.commons.portable.Util;
using Microsoft.Ajax.Utilities;
using softWrench.sW4.Security.Services;
using softwrench.sw4.user.classes.entities;

namespace softwrench.sw4.activitystream.classes.Util
{
    public static class NotificationQueryBuilder
    {
        public static  Dictionary<string, string> BuildNotificationsQueries()
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
        private static KeyValuePair<string, string> BuildNotificationsQuery(string securityGroupName)
        {
            var securityGroup = UserProfileManager.FindByName(securityGroupName);
            var roles = securityGroup.Roles;
            string notificationsQuery = "";
            foreach (var role in roles) {
                switch (role.Name.ToLower()) {
                    case "sr":
                        notificationsQuery += ActivityStreamConstants.baseQueries.Single(q => q.Key.EqualsIc(role.Name)).Value;
                        //append where clause
                        notificationsQuery += " UNION ";
                        notificationsQuery += ActivityStreamConstants.baseQueries.Single(q => q.Key.EqualsIc(role.Name + "Worklogs")).Value;
                        //append where clause
                        notificationsQuery += " UNION ";
                        notificationsQuery += ActivityStreamConstants.baseQueries.Single(q => q.Key.EqualsIc(role.Name + "Commlogs")).Value;
                        //append where clause
                        notificationsQuery += " UNION ";
                        break;
                    case "incident":
                        notificationsQuery += ActivityStreamConstants.baseQueries.Single(q => q.Key.EqualsIc(role.Name)).Value;
                        //append where clause
                        notificationsQuery += " UNION ";
                        notificationsQuery += ActivityStreamConstants.baseQueries.Single(q => q.Key.EqualsIc(role.Name + "Worklogs")).Value;
                        //append where clause
                        notificationsQuery += " UNION ";
                        notificationsQuery += ActivityStreamConstants.baseQueries.Single(q => q.Key.EqualsIc(role.Name + "Commlogs")).Value;
                        //append where clause
                        notificationsQuery += " UNION ";
                        break;
                    case "workorders":
                        notificationsQuery += ActivityStreamConstants.baseQueries.Single(q => q.Key.EqualsIc(role.Name)).Value;
                        //append where clause
                        notificationsQuery += " UNION ";
                        notificationsQuery += ActivityStreamConstants.baseQueries.Single(q => q.Key.EqualsIc(role.Name + "Worklogs")).Value;
                        //append where clause
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
