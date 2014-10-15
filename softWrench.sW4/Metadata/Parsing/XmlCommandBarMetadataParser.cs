using JetBrains.Annotations;
using softwrench.sw4.Shared2.Metadata.Applications.Command;
using softwrench.sW4.Shared2.Metadata.Applications.Command;
using softWrench.sW4.Util;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Xml.Linq;

using cnst = softWrench.sW4.Metadata.Parsing.XmlCommandSchema;


namespace softWrench.sW4.Metadata.Parsing {
    /// <summary>
    ///     Provides parsing and deserialization of
    ///     application metadata stored in a XML file.
    /// </summary>
    internal sealed class XmlCommandBarMetadataParser {


        /// <summary>
        ///     Parses the XML document provided by the specified
        ///     stream and returns all application metadata.
        /// </summary>
        /// <param name="stream">The input stream containing the XML representation of the metadata file.</param>
        [NotNull]
        public IDictionary<string, CommandBarDefinition> Parse([NotNull] TextReader stream) {
            Validate.NotNull(stream, "stream");

            var document = XDocument.Load(stream);
            if (null == document.Root) throw new InvalidDataException();

            return DoParse(document.Root);
        }

        internal static IDictionary<string, CommandBarDefinition> DoParse(XElement commandBarsDefinitions) {
            if (null == commandBarsDefinitions) {
                return new Dictionary<string, CommandBarDefinition>();
            }
            var result = new Dictionary<string, CommandBarDefinition>();
            var commandBars = commandBarsDefinitions.Elements().Where(e => e.IsNamed(cnst.CommandsElement));
            foreach (var commandBar in commandBars) {
                var bar = ParseCommandBar(commandBar);
                result[bar.Id] = bar;
            }

            return result;
        }

        private static CommandBarDefinition ParseCommandBar(XElement commandbar) {
            var id = commandbar.AttributeValue(XmlBaseSchemaConstants.IdAttribute);
            var position = commandbar.AttributeValue(cnst.PositionAttribute);
            return new CommandBarDefinition(id, position, ParseCommandDisplayables(commandbar.Elements()));
        }

        private static IEnumerable<ICommandDisplayable> ParseCommandDisplayables(IEnumerable<XElement> elements) {
            return elements.Select(GetCommandDisplayable).ToList();
        }

        public static ICommandDisplayable GetCommandDisplayable(XElement xElement) {
            var id = xElement.AttributeValue(XmlBaseSchemaConstants.IdAttribute, true);
            var role = xElement.AttributeValue(cnst.RemoveAttribute);
            if (xElement.IsNamed(cnst.ResourceCommand)) {
                var path = xElement.AttributeValue(cnst.ResourceCommandPath);
                return new ResourceCommand(id, path, role);
            }
            if (xElement.IsNamed(cnst.CommandElement)) {
                return GetApplicationCommand(xElement, id, role, null);
            }
            if (xElement.IsNamed(cnst.ContainerCommand)) {
                var label = xElement.AttributeValue(XmlBaseSchemaConstants.LabelAttribute);
                var tooltip = xElement.AttributeValue(XmlBaseSchemaConstants.BaseDisplayableToolTipAtribute);
                var position = xElement.AttributeValue(cnst.PositionAttribute);
                return new ContainerCommand(id, label, tooltip, role, position, ParseCommandDisplayables(xElement.Elements()));
            }
            if (xElement.IsNamed(cnst.RemoveCommand)) {
                return new RemoveCommand(id);
            }

            throw new InvalidOperationException("Invalid command option");
        }

        public static ApplicationCommand GetApplicationCommand(XElement xElement) {
            var id = xElement.AttributeValue(XmlBaseSchemaConstants.IdAttribute, true);
            var role = xElement.AttributeValue(cnst.RemoveAttribute);
            var position = xElement.AttributeValue(cnst.PositionAttribute);
            return GetApplicationCommand(xElement, id,  role, position);
        }

        private static ApplicationCommand GetApplicationCommand(XElement xElement, string id, string role, string position) {
            var label = xElement.AttributeValue(XmlBaseSchemaConstants.LabelAttribute);
            var tooltip = xElement.AttributeValue(XmlBaseSchemaConstants.BaseDisplayableToolTipAtribute);
            if (tooltip == null) {
                //by default Tooltip will be samething as the label it self.
                tooltip = label;
            }
            var service = xElement.AttributeValue(XmlBaseSchemaConstants.ServiceAttribute);
            var method = xElement.AttributeValue(XmlBaseSchemaConstants.MethodAttribute);
            var stereotype = xElement.AttributeValue(XmlBaseSchemaConstants.StereotypeAttribute);
            var showExpression = xElement.AttributeValue(XmlBaseSchemaConstants.BaseDisplayableShowExpressionAtribute);
            var successMessage = xElement.AttributeValue(XmlMetadataSchema.ApplicationCommandSuccessMessage);
            var nextSchemaId = xElement.AttributeValue(XmlMetadataSchema.ApplicationCommandNextSchemaId);
            var scopeParameters = xElement.AttributeValue(XmlBaseSchemaConstants.BaseParametersAttribute);
            var icon = xElement.AttributeValue(XmlMetadataSchema.ApplicationCommandIconAttribute);
            var applicationCommand = new ApplicationCommand(id, label, service, method, role, stereotype, showExpression,
                successMessage, nextSchemaId, scopeParameters, position, icon, tooltip);
            return applicationCommand;
        }
    }
}

