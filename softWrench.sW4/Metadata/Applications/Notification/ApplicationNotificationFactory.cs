using JetBrains.Annotations;
using softwrench.sW4.Shared2.Metadata.Applications.Schema;
using softwrench.sW4.Shared2.Metadata.Applications.Schema.Interfaces;
using System.Collections.Generic;
using softwrench.sw4.Shared2.Metadata.Applications.Notification;
using softwrench.sW4.Shared2.Metadata.Applications.Notification;

namespace softWrench.sW4.Metadata.Applications.Notification {

    public static class ApplicationNotificationFactory {

        public static ApplicationNotificationDefinition GetInstance(string applicationName, string title, string notificationId, SchemaStereotype stereotype,
          NotificationType type, string role, string label, string icon, string targetSchema, string targetApplication, string whereClause,
            [NotNull]List<IApplicationDisplayable> displayables)
        {

            var notificationLabel = label ?? title;
            var notification = new ApplicationNotificationDefinition(applicationName, notificationId, type, role, notificationLabel, icon, targetSchema, targetApplication, whereClause,
            displayables);

            return notification;
        }

    }

}

