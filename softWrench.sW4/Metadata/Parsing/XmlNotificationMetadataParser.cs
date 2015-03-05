using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using JetBrains.Annotations;
using softWrench.sW4.Metadata.Properties;
using softwrench.sw4.Shared2.Metadata.Applications.Notification;

namespace softWrench.sW4.Metadata.Parsing
{
    public sealed class XmlNotificationMetadataParser
    {
        /// <summary>
        ///     Parses the XML document provided by the specified
        ///     stream and returns all application metadata.
        /// </summary>
        /// <param name="stream">The input stream containing the XML representation of the metadata file.</param>
        [NotNull]
        public IDictionary<string, NotificationDefinition> Parse(TextReader stream) {
            if (stream == null) {
                //since the commands.xml is a new concept its not needed to have it for every customer
                return new Dictionary<string, NotificationDefinition>();
            }

            var document = XDocument.Load(stream);
            if (null == document.Root) throw new InvalidDataException();

            return DoParse(document.Root);
        }

        internal static IDictionary<string, NotificationDefinition> DoParse(XElement notificationsDefinitions)
        {
            if (null == notificationsDefinitions)
            {
                return new Dictionary<string, NotificationDefinition>();
            }
            var result = new Dictionary<string, NotificationDefinition>();
            var notifications = notificationsDefinitions.Elements().Where(e => e.IsNamed(XmlNotificationMetadataSchema.NotificationsElement));
            foreach (var notification in notifications)
            {
                var bar = ParseNotification(notification);
                result[bar.id] = bar;
            }

            return result;
        }

        private static CommandBarDefinition ParseCommandBar(XElement commandbar)
        {
            var id = commandbar.AttributeValue(XmlBaseSchemaConstants.IdAttribute);
            var position = commandbar.AttributeValue(cnst.PositionAttribute);
            var excludeUndeclared = commandbar.Attribute(cnst.RemoveUndeclared).ValueOrDefault(false);
            return new CommandBarDefinition(id, position, excludeUndeclared, ParseCommandDisplayables(commandbar.Elements()));
        }

        private static IEnumerable<ICommandDisplayable> ParseCommandDisplayables(IEnumerable<XElement> elements)
        {
            return elements.Select(GetCommandDisplayable).ToList();
        }











        private static void AddProperty(XElement xElement, Dictionary<string, string> dictionary)
        {
            var key = xElement.Attribute(XmlMetadataSchema.PropertyKeyAttribute).Value;
            var value = xElement.Attribute(XmlMetadataSchema.PropertyValueAttribute).Value;
            dictionary.Add(key, value);
        }

        private EnvironmentProperties ParseEnvironment(XElement envElement)
        {
            var envKey = envElement.Attribute(XmlMetadataSchema.ApplicationPropertyKeyAttribute).Value;
            var dictionary = new Dictionary<string, string>();
            foreach (var xElement in envElement.Elements())
            {
                AddProperty(xElement, dictionary);
            }
            return new EnvironmentProperties(envKey, dictionary);
        }



    }
}
