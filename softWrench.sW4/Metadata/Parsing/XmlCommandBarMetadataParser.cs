using JetBrains.Annotations;
using softwrench.sw4.Shared2.Metadata.Applications.Command;
using softwrench.sW4.Shared2.Metadata.Applications.Command;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using DotLiquid.Tags;
using log4net;
using cnst = softWrench.sW4.Metadata.Parsing.XmlCommandSchema;


namespace softWrench.sW4.Metadata.Parsing {
    /// <summary>
    ///     Provides parsing and deserialization of
    ///     application metadata stored in a XML file.
    /// </summary>
    internal sealed class XmlCommandBarMetadataParser {

        private static readonly ILog Log = LogManager.GetLogger(typeof(XmlCommandBarMetadataParser));


        public XmlCommandBarMetadataParser() {
            Log.Debug("init Command logger");
        }

        /// <summary>
        ///     Parses the XML document provided by the specified
        ///     stream and returns all application metadata.
        /// </summary>
        /// <param name="stream">The input stream containing the XML representation of the metadata file.</param>
        [NotNull]
        public IDictionary<string, CommandBarDefinition> Parse(TextReader stream) {
            if (stream == null) {
                //since the commands.xml is a new concept its not needed to have it for every customer
                return new Dictionary<string, CommandBarDefinition>();
            }

            var document = XDocument.Load(stream);
            if (null == document.Root) throw new InvalidDataException();

            return DoParse(document.Root);
        }

        internal static IDictionary<string, CommandBarDefinition> DoParse(XElement commandBarsDefinitions) {
            if (null == commandBarsDefinitions) {
                return new Dictionary<string, CommandBarDefinition>();
            }
            var result = new SortedDictionary<string, CommandBarDefinition>();
            var commandBars = commandBarsDefinitions.Elements().Where(e => e.IsNamed(cnst.CommandsElement));


            foreach (var bar in commandBars.Select(ParseCommandBar)) {
                result[bar.Id] = bar;
            }

            var composedCommands = new List<CommandBarDefinition>(result.Values.Where(r => r.Id.Contains(".")));

            foreach (var composedCommand in composedCommands) {
                var originalCommand =
                    result.Values.LastOrDefault(
                        f => composedCommand.Id.StartsWith(f.Id) && !composedCommand.Id.Equals(f.Id));
                if (originalCommand != null) {
                    Log.DebugFormat("merging command {0} from base definition {1} ", composedCommand.Id, originalCommand.Id);
                    result[composedCommand.Id] = ApplicationCommandMerger.DoMergeBars(composedCommand, originalCommand);
                }
            }
            return result;
        }

        private static CommandBarDefinition ParseCommandBar(XElement commandbar) {
            var id = commandbar.AttributeValue(XmlBaseSchemaConstants.IdAttribute);
            var position = commandbar.AttributeValue(cnst.PositionAttribute);
            var excludeUndeclared = commandbar.Attribute(cnst.RemoveUndeclared).ValueOrDefault(false);
            return new CommandBarDefinition(id, position, excludeUndeclared, ParseCommandDisplayables(commandbar.Elements()));
        }

        private static IEnumerable<ICommandDisplayable> ParseCommandDisplayables(IEnumerable<XElement> elements) {
            return elements.Select(GetCommandDisplayable).ToList();
        }

        public static ICommandDisplayable GetCommandDisplayable(XElement xElement) {
            var id = xElement.AttributeValue(XmlBaseSchemaConstants.IdAttribute, true);
            var role = xElement.AttributeValue(XmlBaseSchemaConstants.RoleAttribute);
            var showExpression = xElement.AttributeValue(XmlBaseSchemaConstants.BaseDisplayableShowExpressionAttribute);
            var position = xElement.AttributeValue(cnst.PositionAttribute);
            if (xElement.IsNamed(cnst.ResourceCommand)) {
                var path = xElement.AttributeValue(cnst.ResourceCommandPath);
                var parameters = xElement.AttributeValue(XmlBaseSchemaConstants.BaseParametersAttribute);
                return new ResourceCommand(id, path, role, position, parameters);
            }
            if (xElement.IsNamed(cnst.CommandElement)) {
                return GetApplicationCommand(xElement, id, role, position);
            }
            if (xElement.IsNamed(cnst.ContainerCommand)) {
                var label = xElement.AttributeValue(XmlBaseSchemaConstants.LabelAttribute);
                var tooltip = xElement.AttributeValue(XmlBaseSchemaConstants.BaseDisplayableToolTipAttribute);
                var icon = xElement.AttributeValue(XmlMetadataSchema.ApplicationCommandIconAttribute);
                var service = xElement.AttributeValue(XmlBaseSchemaConstants.ServiceAttribute);
                var method = xElement.AttributeValue(XmlBaseSchemaConstants.MethodAttribute);
                return new ContainerCommand(id, label, tooltip, role, position, icon, service, method, ParseCommandDisplayables(xElement.Elements()));
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
            return GetApplicationCommand(xElement, id, role, position);
        }

        private static ApplicationCommand GetApplicationCommand(XElement xElement, string id, string role, string position) {
            var label = xElement.AttributeValue(XmlBaseSchemaConstants.LabelAttribute);
            var tooltip = xElement.AttributeValue(XmlBaseSchemaConstants.BaseDisplayableToolTipAttribute);
            if (tooltip == null) {
                //by default Tooltip will be samething as the label it self.
                tooltip = label;
            }
            var icon = xElement.AttributeValue(XmlMetadataSchema.ApplicationCommandIconAttribute);
            var service = xElement.AttributeValue(XmlBaseSchemaConstants.ServiceAttribute);
            var method = xElement.AttributeValue(XmlBaseSchemaConstants.MethodAttribute);

            var stereotype = xElement.AttributeValue(XmlBaseSchemaConstants.StereotypeAttribute);
            var showExpression = xElement.AttributeValue(XmlBaseSchemaConstants.BaseDisplayableShowExpressionAttribute);
            var enableExpression = xElement.AttributeValue(XmlBaseSchemaConstants.BaseDisplayableEnableExpressionAttribute);
            var successMessage = xElement.AttributeValue(XmlMetadataSchema.ApplicationCommandSuccessMessage);
            var nextSchemaId = xElement.AttributeValue(XmlMetadataSchema.ApplicationCommandNextSchemaId);
            var scopeParameters = xElement.AttributeValue(XmlBaseSchemaConstants.BaseParametersAttribute);
            var properties = xElement.AttributeValue(XmlBaseSchemaConstants.BasePropertiesAttribute);
            var cssClasses = xElement.AttributeValue(XmlBaseSchemaConstants.CssClassesAttribute);
            var primary = xElement.Attribute(XmlBaseSchemaConstants.PrimaryAttribute).ValueOrDefault(false);

            var applicationCommand = new ApplicationCommand(id, label, service, method, role, stereotype, showExpression, enableExpression,
                successMessage, nextSchemaId, scopeParameters, properties, position, icon, tooltip, cssClasses,primary);
            return applicationCommand;
        }
    }
}

