using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace softWrench.sW4.Metadata.Parsing
{
    public class XmlNotificationMetadataSchema
    {
        public const string NotificationsElement = "notifications";
        public const string NotificationElement = "notifications";

        public const string NotificationAttributeId = "id";
        public const string NotificationAttributeType = "type";
        public const string NotificationAttributeRole = "role";
        public const string NotificationAttributeLabel = "label";
        public const string NotificationAttributeIcon = "icon";
        public const string NotificationAttributeTargetSchema = "targetSchema";
        public const string NotificationAttributeTargetApplication = "targetApplication";
        public const string NotificationAttributeWhereClause = "whereClause";

        public const string NotificationAttributesElement = "attributes";
        public const string NotificationAttributeUIdElement = "uid";
        public const string NotificationAttributeSummaryElement = "summary";
        public const string NotificationAttributeCreateDateElement = "createddate";
        public const string NotificationAttributeChangeByElement = "changeby";

        public const string NotificationParentAttributesElement = "parentAttributes";
        public const string NotificationParentAttributeApplicationElement = "application";
        public const string NotificationParentAttributeParentUIdElement = "parentuids";

        public const string NotificationExtraAttributesElement = "extraAttributes";
        public const string NotificationExtraAttributeElement = "extraAttribute";

        public const string NotificationAttributeElementAttribute = "attribute";

    }
}
