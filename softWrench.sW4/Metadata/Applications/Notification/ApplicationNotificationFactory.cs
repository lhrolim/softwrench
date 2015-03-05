using JetBrains.Annotations;
using softWrench.sW4.Metadata.Stereotypes.Schema;
using softwrench.sw4.Shared2.Metadata;
using softwrench.sw4.Shared2.Metadata.Applications.Schema;
using softwrench.sW4.Shared2.Metadata;
using softwrench.sW4.Shared2.Metadata.Applications;
using softwrench.sW4.Shared2.Metadata.Applications.Command;
using softwrench.sw4.Shared2.Metadata.Applications.Command;
using softwrench.sW4.Shared2.Metadata.Applications.Schema;
using softwrench.sW4.Shared2.Metadata.Applications.Schema.Interfaces;
using softWrench.sW4.Metadata.Applications.Association;
using softWrench.sW4.Metadata.Applications.Reference;
using softWrench.sW4.Metadata.Stereotypes;
using softWrench.sW4.Security.Entities;
using softWrench.sW4.Security.Services;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using softwrench.sw4.Shared2.Metadata.Applications.Notification;
using softwrench.sW4.Shared2.Metadata.Applications.Notification;
using softWrench.sW4.Metadata.Applications.Schema;
using softWrench.sW4.Util;

namespace softWrench.sW4.Metadata.Applications.Notification {

    public static class ApplicationNotificationFactory {

        public static ApplicationNotificationDefinition GetInstance(string applicationName, string title, string notificationId, SchemaStereotype stereotype,
          NotificationType type, string role, string label, string icon, string targetSchema, string targetApplication, string whereClause,
            [NotNull]List<IApplicationDisplayable> displayables)
        {

            var notificationLabel = title ?? label;
            var notification = new ApplicationNotificationDefinition(applicationName, notificationId, type, role, notificationLabel, icon, targetSchema, targetApplication, whereClause,
            displayables);

            return notification;
        }

    }

}

