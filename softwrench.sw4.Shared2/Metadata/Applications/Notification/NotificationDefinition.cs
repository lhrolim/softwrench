using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace softwrench.sw4.Shared2.Metadata.Applications.Notification
{
    public class NotificationDefinition
    {

        public NotificationDefinition(
            string type, string role, string label, string icon, bool navigateToComposition, string targetSchema, string targetApplication, string whereClause, 
            IDictionary<string, object> attributes) {

            if (displayables == null) throw new ArgumentNullException("displayables");

            ApplicationName = applicationName;
            Platform = platform;
            _displayables = displayables;
            ParentSchema = parentSchema;
            PrintSchema = printSchema;
            SchemaId = schemaId;
            Stereotype = stereotype;
            Abstract = @abstract;
            Mode = mode;
            CommandSchema = commandSchema;
            Title = title;
            _properties = schemaProperties;
            IdFieldName = idFieldName;
            UserIdFieldName = userIdFieldName;
            UnionSchema = unionSchema;

            if (events != null) {
                _events = events.ToDictionary(f => f.Type, f => f);
            }
        }

    }
}
