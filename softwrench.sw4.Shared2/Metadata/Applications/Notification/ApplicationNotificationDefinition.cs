using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using softwrench.sW4.Shared2.Metadata.Applications.Notification;
using softwrench.sW4.Shared2.Metadata.Applications.Schema;
using softwrench.sW4.Shared2.Metadata.Applications.Schema.Interfaces;

namespace softwrench.sw4.Shared2.Metadata.Applications.Notification
{
    public class ApplicationNotificationDefinition : ApplicationSchemaDefinition
    {
        public string NotificationId { get; set; }
        public NotificationType Type { get; set; }
        public string Role { get; set; }
        public string Label { get; set; }
        public string Icon { get; set; }
        public string TargetSchema { get; set; }
        public string TargetApplication { get; set; }
        public string WhereClause { get; set; }

        public ApplicationNotificationDefinition(
            string applicationName, string notificationId, NotificationType type, string role, string label, string icon, string targetSchema, string targetApplication, string whereClause,
            List<IApplicationDisplayable> displayables) {

            if (displayables == null) throw new ArgumentNullException("displayables");

            NotificationId = notificationId;
            Type = type;
            Role = role;
            Label = label;
            Icon = icon;
            TargetSchema = targetSchema;
            TargetApplication = targetApplication;
            WhereClause = whereClause;
            Displayables = displayables;
            ApplicationName = applicationName;
            
            }

        [JsonIgnore]
        public IList<ApplicationNotificationDisplayable> Fields {
            get { return GetDisplayable<ApplicationNotificationDisplayable>(typeof(ApplicationNotificationDisplayable)); }
        }

    }
}
