using cts.commons.portable.Util;
using cts.commons.Util;
using JetBrains.Annotations;
using softWrench.sW4.Metadata.Validator;
using softwrench.sw4.Shared2.Data.Association;
using softwrench.sw4.Shared2.Metadata.Applications.Command;
using softwrench.sw4.Shared2.Metadata.Applications.Schema;
using softwrench.sw4.Shared2.Metadata.Applications.UI;
using softwrench.sW4.Shared2.Metadata;
using softwrench.sW4.Shared2.Metadata.Applications;
using softwrench.sW4.Shared2.Metadata.Applications.Command;
using softwrench.sW4.Shared2.Metadata.Applications.Relationships.Associations;
using softwrench.sW4.Shared2.Metadata.Applications.Relationships.Compositions;
using softwrench.sW4.Shared2.Metadata.Applications.Schema;
using softwrench.sW4.Shared2.Metadata.Applications.Schema.Interfaces;
using softwrench.sW4.Shared2.Metadata.Applications.UI;
using softWrench.sW4.Data.EL;
using softWrench.sW4.Data.EL.ToString;
using softWrench.sW4.Metadata.Applications.Association;
using softWrench.sW4.Metadata.Applications.Schema;
using softWrench.sW4.Metadata.Entities;
using softWrench.sW4.Util;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using NHibernate.Linq;
using softwrench.sw4.Shared2.Metadata.Applications.Schema.Interfaces;
using softWrench.sW4.Metadata.Applications.Validator;
using softWrench.sW4.Metadata.Stereotypes;
using softWrench.sW4.Metadata.Stereotypes.Schema;


namespace softWrench.sW4.Metadata.Parsing {
    /// <summary>
    ///     Provides parsing and deserialization of
    ///     application metadata stored in a XML file.
    /// </summary>
    internal sealed class XmlApplicationMetadataParser : IXmlMetadataParser<IEnumerable<CompleteApplicationMetadataDefinition>> {
        private const string MissingRelationship = "application {0} references unknown relationship {1}";
        private const string MissingEntity = "application {0} references unknown entity {1}";
        private const string MissingParentSchema = "Error building schema {0} for application {1}.parentSchema {2} not found. Please assure its declared before the concrete schema";
        //        private static readonly int? Infinite = null;

        private readonly bool _isSWDB = false;

        private readonly bool _isTemplateParsing = false;

        public XmlApplicationMetadataParser([NotNull] IEnumerable<EntityMetadata> entityMetadata, IDictionary<string, CommandBarDefinition> commandBars, bool isSWDB, bool isTemplateParsing) {
            if (entityMetadata == null) throw new ArgumentNullException("entityMetadata");

            _entityMetadata = entityMetadata;
            _commandBars = commandBars;
            _isSWDB = isSWDB;
            _isTemplateParsing = isTemplateParsing;
            _xmlTemplateHandler = new XmlTemplateHandler(_isSWDB);
        }


        internal enum FieldRendererType {
            ASSOCIATION, COMPOSITION, OPTION, BASE, SECTION,
        }

        private static FieldRenderer ParseRendererNew(XElement renderer, string targetName, FieldRendererType ftype, EntityMetadata entity = null) {
            if (renderer == null) {
                switch (ftype) {
                    case FieldRendererType.ASSOCIATION:
                    return new AssociationFieldRenderer();
                    case FieldRendererType.COMPOSITION:
                    return new CompositionFieldRenderer();
                    case FieldRendererType.OPTION:
                    return new OptionFieldRenderer();
                    default:
                    if (entity != null) {
                        var attr = entity.Schema.Attributes.FirstOrDefault(a => a.Name.EqualsIc(targetName));
                        if (attr != null && (attr.Type == "timestamp" || attr.Type == "datetime")) {
                            return new FieldRenderer(FieldRenderer.BaseRendererType.DATETIME.ToString().ToLower(), null, targetName, null);
                        }
                    }
                    return new FieldRenderer();
                }
            }
            var type = renderer.Attribute(XmlMetadataSchema.RendererAttributeType).Value;
            var parameters = renderer.Attribute(XmlMetadataSchema.RendererAttributeParams).ValueOrDefault((string)null);
            var stereotype = renderer.Attribute(XmlMetadataSchema.RendererAttributeStereotype).ValueOrDefault((string)null);
            switch (ftype) {
                case FieldRendererType.ASSOCIATION:
                return new AssociationFieldRenderer(type, parameters, targetName, stereotype);
                case FieldRendererType.COMPOSITION:
                return new CompositionFieldRenderer(type, parameters, targetName);
                case FieldRendererType.OPTION:
                return new OptionFieldRenderer(type, parameters, targetName);
                default:
                return new FieldRenderer(type, parameters, targetName, stereotype);
            }
        }

        private static FieldFilter ParseFilterNew(XElement renderer, string targetName) {

            if (renderer == null) {
                return null;
            }

            var operation = renderer.AttributeValue(XmlMetadataSchema.FilterOperationType);
            var parameters = renderer.Attribute(XmlMetadataSchema.FilterAttributeParams).ValueOrDefault((string)null);
            var defaultValue = renderer.Attribute(XmlMetadataSchema.FilterAttributeDefault).ValueOrDefault((string)null);
            var clientFunction = renderer.AttributeValue("clientfunction");

            return new FieldFilter(operation, parameters, defaultValue, targetName, clientFunction);
        }

        /// <summary>
        ///     Deseriliazes the specified XML element to its corresponding
        ///     <seealso cref="ApplicationField"/> representation.
        /// </summary>
        /// <param name="applicationName"></param>
        /// <param name="field">The XML field to parse.</param>
        /// <param name="entityMetadata"></param>
        private static ApplicationFieldDefinition ParseField(string applicationName, XElement field, EntityMetadata entityMetadata) {
            var attribute = field.Attribute(XmlMetadataSchema.FieldAttributeAttribute).Value;
            var label = field.Attribute(XmlMetadataSchema.FieldAttributeLabel).ValueOrDefault("");
            var requiredExpression = field.Attribute(XmlBaseSchemaConstants.BaseDisplayableRequiredExpressionAttribute).ValueOrDefault("false");
            var isReadOnly = field.Attribute(XmlMetadataSchema.FieldAttributeReadOnly).ValueOrDefault(false);
            var isHidden = field.Attribute(XmlMetadataSchema.FieldAttributeHidden).ValueOrDefault(false);
            var showExpression = field.Attribute(XmlBaseSchemaConstants.BaseDisplayableShowExpressionAttribute).ValueOrDefault("true");
            var toolTip = field.Attribute(XmlBaseSchemaConstants.BaseDisplayableToolTipAttribute).ValueOrDefault((string)null);
            var helpIcon = field.Attribute(XmlBaseSchemaConstants.BaseDisplayableHelpIconAttribute).ValueOrDefault((string)null);
            var defaultValue = field.Attribute(XmlMetadataSchema.FieldAttributeDefaultValue).ValueOrDefault((string)null);
            var defaultExpression = field.Attribute(XmlMetadataSchema.FieldAttributeDefaultExpression).ValueOrDefault((string)null);
            var renderer = ParseRendererNew(field.Elements().FirstOrDefault(f => f.Name.LocalName == XmlMetadataSchema.RendererElement),
                attribute, FieldRendererType.BASE, entityMetadata);
            var qualifier = field.Attribute(XmlMetadataSchema.FieldAttributeQualifier).ValueOrDefault((string)null);
            var widget = XmlWidgetParser.Parse(field, attribute, isHidden);
            var attributeToServer = field.Attribute(XmlMetadataSchema.FieldAttributeAttributeToServer).ValueOrDefault((string)null);
            var events = ParseEvents(field, attribute);
            var filterElement = field.Elements().FirstOrDefault(f => f.Name.LocalName == XmlMetadataSchema.FilterElement);
            var enableExpression = field.Attribute(XmlBaseSchemaConstants.BaseDisplayableEnableExpressionAttribute).ValueOrDefault("true");
            var enableDefault = field.Attribute(XmlBaseSchemaConstants.BaseDisplayableEnableDefaultAttribute).ValueOrDefault("true");
            var evalExpression = field.Attribute(XmlBaseSchemaConstants.BaseDisplayableEvalExpressionAttribute).ValueOrDefault((string)null);
            var searchOperation = field.Attribute(XmlBaseSchemaConstants.BaseDisplayableSearchOperation).ValueOrDefault((string)null);
            // Flag for fields coming from attributes that use subqueries
            var fieldAttributeMetadata = entityMetadata.Schema.Attributes.FirstOrDefault(a => a.Name.EqualsIc(attribute));
            var fieldEntityQuery = fieldAttributeMetadata == null ? null : fieldAttributeMetadata.Query;
            var declaredAsQueryOnEntity = !fieldEntityQuery.NullOrEmpty() && fieldEntityQuery.Length > 0;

            var datatype = ParseDataType(entityMetadata, attribute);

            var fieldObj = new ApplicationFieldDefinition(applicationName, attribute, datatype, label, requiredExpression, isReadOnly, isHidden, renderer,
                ParseFilterNew(filterElement, attribute), widget, defaultValue, qualifier, showExpression, helpIcon, toolTip, attributeToServer, events, enableExpression, evalExpression, enableDefault,
                defaultExpression, declaredAsQueryOnEntity, searchOperation);

            AddPrimaryAttribute(fieldObj, entityMetadata);

            return fieldObj;
        }

        private static void AddPrimaryAttribute(ApplicationFieldDefinition field, EntityMetadata entityMetadata) {
            var attribute = field.Attribute;
            if (attribute == null || !attribute.Contains(".") || entityMetadata.Associations.Count == 0) {
                return;
            }

            var toEntity = attribute.Split('.')[0];
            var association = entityMetadata.Associations.FirstOrDefault(assoc => assoc.Qualifier != null && assoc.Qualifier.Equals(toEntity));
            if (association == null) {
                return;
            }

            var primaryAttribute = association.Attributes.FirstOrDefault(att => att.Primary);
            if (primaryAttribute == null) {
                return;
            }
            field.PrimaryAttribute = primaryAttribute.From;
        }

        private static string ParseDataType(EntityMetadata entityMetadata, string attribute) {
            string datatype = null;
            if (entityMetadata != null) {
                var attr = entityMetadata.Schema.Attributes.FirstOrDefault(a => a.Name.EqualsIc(attribute));
                if (attr != null) {
                    datatype = attr.Type;
                }
            }
            return datatype;
        }

        private static ISet<ApplicationEvent> ParseEvents(XElement element, string parentId) {
            ISet<ApplicationEvent> result = new HashSet<ApplicationEvent>();

            var events = element.Elements().Where(e => e.Name.LocalName == XmlMetadataSchema.EventsElement).ToList();
            foreach (var applicationEvent in events) {
                var newEvent = new ApplicationEvent {
                    Type = applicationEvent.Attribute(XmlMetadataSchema.EventsTypeAttribute).Value,
                    Service = applicationEvent.Attribute(XmlMetadataSchema.EventsServiceAttribute).ValueOrDefault((string)null),
                    Method = applicationEvent.Attribute(XmlMetadataSchema.EventsMethodAttribute).ValueOrDefault((string)null),
                    Expression = applicationEvent.Attribute(XmlMetadataSchema.EventsExpressionAttribute).ValueOrDefault((string)null)
                };
                if (string.IsNullOrEmpty(newEvent.Expression) && (string.IsNullOrEmpty(newEvent.Service) && string.IsNullOrEmpty(newEvent.Method))) {
                    var msg =
                        string.Format("Event of type {0} from {1} needs a service and method or a expression to eval.",
                            newEvent.Type, parentId);
                    throw ExceptionUtil.InvalidOperation(msg);
                }
                result.Add(newEvent);
            }
            return result;
        }

        /// <summary>
        ///     Iterates through the `detail` element in the specified
        ///     container, deserializing all `field` children elements
        ///     to its corresponding <seealso cref="ApplicationField"/> representation.
        /// </summary>
        /// <param name="applicationName"></param>
        /// <param name="schema"></param>
        /// <param name="entityName"></param>
        private static List<IApplicationDisplayable> ParseDisplayables(string applicationName, string schemaId, XContainer schema, string entityName) {
            var entityMetadata = MetadataProvider.Entity(entityName);
            return schema.Elements()
                .Select(xElement => FindDisplayable(applicationName, schemaId, entityName, xElement, entityMetadata))
                .Where(applicationDisplayable => applicationDisplayable != null)
                .ToList();
        }

        private static ReferenceDisplayable ParseReference(XElement xElement) {
            var id = xElement.Attribute(XmlBaseSchemaConstants.BaseDisplayableIdAttribute).Value;
            var readOnly = xElement.Attribute(XmlMetadataSchema.FieldAttributeReadOnly).ValueOrDefault((bool)false);

            return new ReferenceDisplayable {
                Id = id,
                ShowExpression = xElement.Attribute(XmlBaseSchemaConstants.BaseDisplayableShowExpressionAttribute).ValueOrDefault((string)null),
                Label = xElement.Attribute(XmlBaseSchemaConstants.BaseDisplayableLabelAttribute).ValueOrDefault((string)null),
                Attribute = xElement.Attribute(XmlMetadataSchema.AttributeElement).ValueOrDefault((string)null),
                PropertiesString = xElement.Attribute(XmlMetadataSchema.ApplicationPropertiesElement).ValueOrDefault((string)null),
                IsReadOnly = readOnly,
                EnableExpression = xElement.Attribute(XmlBaseSchemaConstants.BaseDisplayableEnableExpressionAttribute).ValueOrDefault((string)null)
            };
        }

        private static IApplicationDisplayable ParseTab(string applicationName, string schemaId, XElement tabElement, string entityName) {
            var id = tabElement.Attribute(XmlBaseSchemaConstants.BaseDisplayableIdAttribute).ValueOrDefault((string)null);
            var label = tabElement.Attribute(XmlBaseSchemaConstants.BaseDisplayableLabelAttribute).ValueOrDefault((string)null);
            var showExpression = tabElement.Attribute(XmlBaseSchemaConstants.BaseDisplayableShowExpressionAttribute).ValueOrDefault("true");
            var toolTip = tabElement.Attribute(XmlBaseSchemaConstants.BaseDisplayableToolTipAttribute).ValueOrDefault((string)null);
            var icon = tabElement.Attribute(XmlBaseSchemaConstants.IconAttribute).ValueOrDefault((string)null);
            var role = tabElement.Attribute(XmlBaseSchemaConstants.RoleAttribute).ValueOrDefault((string)null);
            var displayables = ParseDisplayables(applicationName, schemaId, tabElement, entityName);
            return new ApplicationTabDefinition(id, applicationName, label, displayables, toolTip, showExpression, icon, role);
        }

        private static ApplicationSection ParseSection(string applicationName, string schemaId, XElement sectionElement, EntityMetadata entityMetadata) {
            var id = sectionElement.Attribute(XmlMetadataSchema.ApplicationSectionIdAttribute).ValueOrDefault((string)null);
            var @abstract = sectionElement.Attribute(XmlMetadataSchema.ApplicationSectionAbstractAttribute).ValueOrDefault(false);
            var resourcePath = sectionElement.Attribute(XmlMetadataSchema.ApplicationSectionResourcePathAttribute).ValueOrDefault((string)null);
            var parameters = sectionElement.Attribute(XmlMetadataSchema.ApplicationSectionParametersAttribute).ValueOrDefault((string)null);
            var label = sectionElement.Attribute(XmlMetadataSchema.ApplicationSectionLabelAttribute).ValueOrDefault((string)null);
            var attribute = sectionElement.Attribute(XmlMetadataSchema.ApplicationSectionAttributeAttribute).ValueOrDefault((string)null);
            var showExpression = sectionElement.Attribute(XmlBaseSchemaConstants.BaseDisplayableShowExpressionAttribute).ValueOrDefault("true");
            var toolTip = sectionElement.Attribute(XmlBaseSchemaConstants.BaseDisplayableToolTipAttribute).ValueOrDefault((string)null);

            var displayables = ParseDisplayables(applicationName, schemaId, sectionElement, entityMetadata.Name);
            var secondaries = ValidateSingleSecondarySection(displayables);
            var secondarycontent = sectionElement.Attribute(XmlMetadataSchema.ApplicationSectionSecondaryContentAttribute).ValueOrDefault(false);
            if (secondaries > 0 && secondarycontent) {
                throw new InvalidOperationException("A section with secondarycontent=\"true\" cannot have a child section with secondarycontent=\"true\"");
            }

            var header = ParseHeader(sectionElement);
            var orientation = sectionElement.Attribute(XmlMetadataSchema.ApplicationSectionOrientationAttribute).ValueOrDefault((string)null);
            var renderer = ParseRendererNew(sectionElement.Elements().FirstOrDefault(f => f.Name.LocalName == XmlMetadataSchema.RendererElement),
                attribute, FieldRendererType.SECTION, entityMetadata);
            var role = sectionElement.Attribute(XmlMetadataSchema.ApplicationSectionRoleAttribute).ValueOrDefault((string)null);
            // Removing this code due to "Asset Specification" section in IMAC application, Update Schema
            /*if (displayables != null && displayables.Count > 0 && !String.IsNullOrWhiteSpace(resourcePath)) {
                throw new InvalidOperationException("<section> cannot contains inner elements AND resourcePath attribute");
            }*/
            return new ApplicationSection(id, applicationName, @abstract, label, attribute, resourcePath, parameters,
                displayables, showExpression, toolTip, orientation, header, renderer, role) {
                SecondaryContent = secondarycontent
            };
        }

        private static int ValidateSingleSecondarySection(IEnumerable<IApplicationDisplayable> displayables) {
            var secondaries = displayables.Count(d => d is ApplicationSection && (((ApplicationSection)d).SecondaryContent));
            if (secondaries > 1) {
                throw new InvalidOperationException("An element cannot have more than one child section with secondarycontent=\"true\"");
            }
            return secondaries;
        }

        private static IApplicationDisplayable ParseCustomization(string applicationName, string schemaId, XElement customizationElement, EntityMetadata entityMetadata) {
            var position = customizationElement.Attribute(XmlMetadataSchema.CustomizationPositionAttribute).ValueOrDefault((string)null);
            var displayables = ParseDisplayables(applicationName, schemaId, customizationElement, entityMetadata.Name);
            return new ApplicationSchemaCustomization(position, displayables);
        }

        private static ApplicationHeader ParseHeader(XContainer schema) {
            foreach (var xElement in schema.Elements()) {
                var xName = xElement.Name.LocalName;
                if (xName == XmlMetadataSchema.ApplicationHeaderElement) {
                    var label = xElement.Attribute(XmlMetadataSchema.ApplicationHeaderLabelAttribute).ValueOrDefault("");
                    var parameters = xElement.Attribute(XmlMetadataSchema.ApplicationHeaderParametersAttribute).ValueOrDefault((string)null);
                    var displacement = xElement.Attribute(XmlMetadataSchema.ApplicationHeaderDisplacementAttribute).ValueOrDefault((string)null);
                    var showExpression = xElement.Attribute(XmlBaseSchemaConstants.BaseDisplayableShowExpressionAttribute).ValueOrDefault("true");
                    var toolTip = xElement.Attribute(XmlBaseSchemaConstants.BaseDisplayableToolTipAttribute).ValueOrDefault((string)null);
                    var helpIcon = xElement.Attribute(XmlBaseSchemaConstants.BaseDisplayableHelpIconAttribute).ValueOrDefault((string)null);
                    return new ApplicationHeader(label, parameters, displacement, showExpression, helpIcon, toolTip);
                }
            }
            return null;
        }

        private static IApplicationDisplayable ParseOptions(XElement xElement, string applicationName, string schemaId) {
            var attribute = xElement.Attribute(XmlMetadataSchema.FieldAttributeAttribute).Value;
            var label = xElement.Attribute(XmlMetadataSchema.FieldAttributeLabel).ValueOrDefault("");
            var requiredExpression = xElement.Attribute(XmlBaseSchemaConstants.BaseDisplayableRequiredExpressionAttribute).ValueOrDefault("false");
            var isReadOnly = xElement.Attribute(XmlMetadataSchema.FieldAttributeReadOnly).ValueOrDefault(false);
            var isHidden = xElement.Attribute(XmlMetadataSchema.FieldAttributeHidden).ValueOrDefault(false);
            var defaultValue = xElement.Attribute(XmlMetadataSchema.FieldAttributeDefaultValue).ValueOrDefault((string)null);
            var defaultExpression = xElement.Attribute(XmlMetadataSchema.FieldAttributeDefaultExpression).ValueOrDefault((string)null);
            var showExpression = xElement.Attribute(XmlBaseSchemaConstants.BaseDisplayableShowExpressionAttribute).ValueOrDefault("true");
            var toolTip = xElement.Attribute(XmlBaseSchemaConstants.BaseDisplayableToolTipAttribute).ValueOrDefault((string)null);
            var attributeToServer = xElement.Attribute(XmlMetadataSchema.FieldAttributeAttributeToServer).ValueOrDefault((string)null);
            var extraProjectionFields = xElement.Attribute(XmlMetadataSchema.ApplicationAssociationExtraProjectionFieldsAttribute).ValueOrDefault((string)null);

            var providerAttribute = xElement.Attribute(XmlMetadataSchema.OptionFieldProviderAttribute).ValueOrDefault((string)null);
            var skipValidation = xElement.Attribute(XmlMetadataSchema.OptionFieldSkipValidationAttribute).ValueOrDefault(false);
            // marks provider attribute to be validated
            // schemaid is null when derived from component
            if (!skipValidation && schemaId != null && providerAttribute != null) {
                var methodName = DynamicOptionFieldResolver.GetMethodName(providerAttribute);
                ApplicationMetadataValidator.AddOptionProviderToValidate(applicationName, schemaId, methodName);
            }

            var extraParameter = xElement.Attribute(XmlMetadataSchema.OptionFieldProviderAttributeExtraParameter).ValueOrDefault((string)null);
            var sort = xElement.Attribute(XmlMetadataSchema.OptionFieldSortAttribute).ValueOrDefault(providerAttribute != null);
            var dependantFields = xElement.Attribute(XmlMetadataSchema.ApplicationAssociationDependantFieldsAttribute).ValueOrDefault((string)null);
            var enableExpression = xElement.Attribute(XmlBaseSchemaConstants.BaseDisplayableEnableExpressionAttribute).ValueOrDefault("true");
            var qualifier = xElement.Attribute(XmlMetadataSchema.FieldAttributeQualifier).ValueOrDefault((string)null);
            var rendererElement = xElement.Elements().FirstOrDefault(f => f.Name.LocalName == XmlMetadataSchema.RendererElement);
            var evalExpression = xElement.Attribute(XmlMetadataSchema.BaseDisplayableEvalExpressionAttribute).ValueOrDefault((string)null);
            var searchOperation = xElement.Attribute(XmlBaseSchemaConstants.BaseDisplayableSearchOperation).ValueOrDefault((string)null);
            var renderer = new OptionFieldRenderer();
            if (rendererElement != null) {
                renderer = (OptionFieldRenderer)ParseRendererNew(rendererElement, attribute, FieldRendererType.OPTION);
            };
            var filterElement = xElement.Elements().FirstOrDefault(f => f.Name.LocalName == XmlMetadataSchema.FilterElement);
            var events = ParseEvents(xElement, attribute);
            return new OptionField(applicationName, label, attribute, qualifier, requiredExpression, isReadOnly, isHidden, renderer, ParseFilterNew(filterElement, attribute),
                                   xElement.Elements().Where(e => e.Name.LocalName == XmlMetadataSchema.OptionElement).Select(ParseOption).ToList(),
                                   defaultValue, sort, showExpression, toolTip, attributeToServer, events, providerAttribute, dependantFields, enableExpression, evalExpression, extraParameter, defaultExpression, searchOperation, extraProjectionFields);
        }

        private static IAssociationOption ParseOption(XElement xElement) {
            var label = xElement.Attribute(XmlMetadataSchema.OptionElementLabelAttribute).Value;
            var value = xElement.Attribute(XmlMetadataSchema.OptionElementValueAttribute).Value;

            var extraProjection = xElement.Attribute(XmlMetadataSchema.OptionElementExtraProjection).ValueOrDefault((string)null);
            if (extraProjection != null) {
                var dict = PropertyUtil.ConvertToDictionary(extraProjection);
                return new MultiValueAssociationOption(value, label, dict);
            }

            return new AssociationOption(value, label);
        }


        //        private static IEnumerable<ApplicationAssociation> ParseAssociations(XContainer schema, String applicationName) {
        //            return schema.Element(XmlMetadataSchema.DetailElement)
        //                .Elements(XmlMetadataSchema.ApplicationAssociationElement)
        //                .Select(e => ParseAssociation(e, applicationName))
        //                .ToList();
        //        }

        private static ApplicationAssociationDefinition ParseAssociation(XElement association, string applicationName, string schemaId, EntityMetadata entityMetadata) {
            var label = association.Attribute(XmlMetadataSchema.ApplicationAssociationLabelAttribute).Value;
            var labelField = association.Attribute(XmlMetadataSchema.ApplicationAssociationLabelFieldAttribute).Value;
            var labelPattern = association.Attribute(XmlMetadataSchema.ApplicationAssociationLabelPatternAttribute).ValueOrDefault((string)null);
            var target = association.Attribute(XmlMetadataSchema.ApplicationAssociationTargetAttribute).Value;
            var defaultValue = association.Attribute(XmlMetadataSchema.ApplicationAssociationDefaultValueAttribute).ValueOrDefault((string)null);
            var defaultExpression = association.Attribute(XmlMetadataSchema.FieldAttributeDefaultExpression).ValueOrDefault((string)null);
            var forceDistinctOptions = association.Attribute(XmlMetadataSchema.ApplicationAssociationForceDistinctOptions).ValueOrDefault(false);
            var labelData = new ApplicationAssociationDefinition.LabelData(label, labelPattern, labelField, applicationName);
            var showExpression = association.Attribute(XmlBaseSchemaConstants.BaseDisplayableShowExpressionAttribute).ValueOrDefault("true");
            var enableExpression = association.Attribute(XmlBaseSchemaConstants.BaseDisplayableEnableExpressionAttribute).ValueOrDefault("true");
            var helpIcon = association.Attribute(XmlBaseSchemaConstants.BaseDisplayableHelpIconAttribute).ValueOrDefault((string)null);
            var tooltip = association.Attribute(XmlBaseSchemaConstants.BaseDisplayableToolTipAttribute).ValueOrDefault((string)null);
            var extraProjectionFields = association.Attribute(XmlMetadataSchema.ApplicationAssociationExtraProjectionFieldsAttribute).ValueOrDefault((string)null);
            var qualifier = association.Attribute(XmlMetadataSchema.FieldAttributeQualifier).ValueOrDefault((string)null);
            var requiredExpression = association.Attribute(XmlBaseSchemaConstants.BaseDisplayableRequiredExpressionAttribute).ValueOrDefault("false");
            var hideDescription = association.Attribute(XmlMetadataSchema.ApplicationAssociationHideDescription).ValueOrDefault(false);
            var orderbyfield = association.Attribute(XmlMetadataSchema.ApplicationAssociationOrderByField).ValueOrDefault((string)null);
            var valueField = association.Attribute(XmlMetadataSchema.ApplicationAssociationValueField).ValueOrDefault((string)null);
            ApplicationSection section = ParseAssociationDetails(association, schemaId, applicationName, entityMetadata);

            return ApplicationAssociationFactory.GetInstance(applicationName, labelData, target, qualifier, ParseAssociationSchema(applicationName, schemaId, association, target), showExpression, helpIcon, tooltip,
                requiredExpression, ParseEvents(association, target), defaultValue, hideDescription, orderbyfield, defaultExpression, extraProjectionFields, enableExpression, forceDistinctOptions, valueField, section);
        }

        private static ApplicationSection ParseAssociationDetails(XElement association, string applicationName, string schemaId, EntityMetadata entityMetadata) {
            var associationDetails = association.Elements().FirstOrDefault(f => f.Name.LocalName == XmlMetadataSchema.ApplicationAssociationDetailsElement);
            if (associationDetails == null) {
                return null;
            }
            return ParseSection(applicationName, schemaId, associationDetails, entityMetadata);
        }

        private static ApplicationAssociationSchemaDefinition ParseAssociationSchema(string applicationName, string schemaId, XElement association, string targetName) {
            var rendererElement = association.Elements().FirstOrDefault(
                f => f.Name.LocalName == XmlMetadataSchema.RendererElement);
            var renderer = new AssociationFieldRenderer();
            var dataProvider = new AssociationDataProvider();
            if (rendererElement != null) {
                renderer = (AssociationFieldRenderer)ParseRendererNew(rendererElement, targetName, FieldRendererType.ASSOCIATION);
            }
            var dataProviderElement = association.Elements().FirstOrDefault(f => f.Name.LocalName == XmlMetadataSchema.DataProviderElement);
            if (dataProviderElement != null) {
                dataProvider = ParseDataProvider(applicationName, schemaId, dataProviderElement);
            }
            var dependantFields = association.Attribute(XmlMetadataSchema.ApplicationAssociationDependantFieldsAttribute).ValueOrDefault((string)null);
            var filterElement = association.Elements().FirstOrDefault(f => f.Name.LocalName == XmlMetadataSchema.FilterElement);
            return ApplicationAssociationFactory.GetSchemaInstance(dataProvider, renderer, ParseFilterNew(filterElement, targetName), dependantFields);
        }

        private static AssociationDataProvider ParseDataProvider(string applicationName, string schemaId, XElement dataProviderElement) {
            var whereClause = dataProviderElement.Attribute(XmlMetadataSchema.DataProviderWhereClause).ValueOrDefault((string)null);

            var prefilterFunction = dataProviderElement.Attribute(XmlMetadataSchema.DataProviderPreFilterFunction).ValueOrDefault((string)null);
            // marks pre filter to be validated
            // sourceSchemaId is null when derived from component
            if (prefilterFunction != null && schemaId != null) {
                ApplicationMetadataValidator.AddAssociationPreFilterToValidate(applicationName, schemaId, prefilterFunction);
            }

            var postfilterFunction = dataProviderElement.Attribute(XmlMetadataSchema.DataProviderPostFilterFunction).ValueOrDefault((string)null);
            // marks post filter to be validated
            // sourceSchemaId is null when derived from component
            if (postfilterFunction != null && schemaId != null) {
                ApplicationMetadataValidator.AddAssociationPostFilterToValidate(applicationName, schemaId, postfilterFunction);
            }

            if (whereClause == null && prefilterFunction == null && postfilterFunction == null) {
                throw new InvalidOperationException("either whereclause of filterfunction should be provided for a dataprovider");
            }
            return new AssociationDataProvider(prefilterFunction, postfilterFunction, whereClause);
        }


        private static ApplicationCompositionDefinition ParseComposition(XElement composition, string applicationName, string sourceSchemaId, string entityName) {
            var label = composition.Attribute(XmlMetadataSchema.ApplicationCompositionLabelAttribute).ValueOrDefault("");
            var relationship = composition.Attribute(XmlMetadataSchema.ApplicationCompositionRelationshipAttribute).Value;
            var showExpression = composition.Attribute(XmlBaseSchemaConstants.BaseDisplayableShowExpressionAttribute).ValueOrDefault("true");
            var toolTip = composition.Attribute(XmlBaseSchemaConstants.BaseDisplayableToolTipAttribute).ValueOrDefault((string)null);
            var hidden = composition.Attribute(XmlBaseSchemaConstants.BaseDisplayableHiddenAttribute).ValueOrDefault(false);
            var printEnabled = composition.Attribute(XmlBaseSchemaConstants.BaseDisplayablePrintEnabledAttribute).ValueOrDefault(true);
            var requiredRelationshipExpression = composition.Attribute(XmlMetadataSchema.ApplicationCompositionRequiredRelationshipAttribute).ValueOrDefault((string)null);
            var schema = ParseCompositionSchema(entityName, applicationName, sourceSchemaId, relationship, composition);
            schema.RequiredRelationshipExpression = requiredRelationshipExpression;
            return ApplicationCompositionFactory.GetInstance(applicationName, relationship, label, schema, showExpression, toolTip, hidden, printEnabled, ParseHeader(composition));
        }

        private static ApplicationCompositionSchema ParseCompositionSchema(string entityName, string applicationName, string sourceSchemaId, string relationship, XElement composition) {
            var rendererElement = composition.Elements().FirstOrDefault(
                f => f.Name.LocalName == XmlMetadataSchema.RendererElement);
            var inline = composition.Attribute(XmlMetadataSchema.ApplicationCompositionInlineAttribute).ValueOrDefault(false);
            var schemaId = composition.Attribute(XmlMetadataSchema.ApplicationCompositionSchemaIdAttribute).ValueOrDefault("detail");
            var printSchema = composition.Attribute(XmlMetadataSchema.ApplicationCompositionPrintAttribute).ValueOrDefault("detail");
            var fetchTypeStr = composition.Attribute(XmlMetadataSchema.ApplicationCompositionFetchType).ValueOrDefault("lazy");
            var fetchType = (FetchType)Enum.Parse(typeof(FetchType), fetchTypeStr, true);

            var dependantfields = composition.Attribute(XmlMetadataSchema.ApplicationCompositionDependantFieldsAttribute).ValueOrDefault((string)null);
            var modeAttr = composition.Attribute(XmlMetadataSchema.ApplicationCompositionRenderModeAttribute).ValueOrDefault((string)null);
            var mode = SchemaMode.None;
            if (modeAttr != null) {
                Enum.TryParse(modeAttr, out mode);
            }
            var e = MetadataProvider.Entity(entityName);
            if (e == null) {
                throw new InvalidOperationException(string.Format(MissingEntity, applicationName, relationship));
            }
            var isCollection = false;
            if (!relationship.StartsWith("#")) {
                //# would mean a self relationship
                var entityAssociation = e.Associations.FirstOrDefault(a => a.Qualifier.EqualsIc(EntityUtil.GetRelationshipName(relationship)));
                if (entityAssociation == null) {
                    throw new InvalidOperationException(string.Format(MissingRelationship, applicationName, relationship));
                }
                isCollection = entityAssociation.Collection;
            }

            var collectionProperties = ParseCollectionProperties(composition, applicationName, sourceSchemaId);
            var applicationEvents = ParseEvents(composition, schemaId);
            if (collectionProperties != null || isCollection) {
                return new ApplicationCompositionCollectionSchema(inline, schemaId, collectionProperties, mode,
                    (CompositionFieldRenderer)ParseRendererNew(rendererElement, e.Name, FieldRendererType.COMPOSITION), printSchema, dependantfields, fetchType, applicationEvents);
            }
            return new ApplicationCompositionSchema(inline, schemaId, mode,
                    (CompositionFieldRenderer)ParseRendererNew(rendererElement, e.Name, FieldRendererType.COMPOSITION), printSchema, dependantfields, fetchType, applicationEvents);

        }

        private static CompositionCollectionProperties ParseCollectionProperties(XElement composition, string applicationName, string sourceSchemaId) {
            var collectionProperties = composition.Elements().FirstOrDefault(
                f => f.Name.LocalName == XmlMetadataSchema.ApplicationCompositionCollectionPropertiesElement);
            if (collectionProperties == null) {
                return new CompositionCollectionProperties();
            }
            var allowInsertion = collectionProperties.Attribute(XmlMetadataSchema.ApplicationCompositionCollectionAllowInsertionAttribute).ValueOrDefault("true");
            var allowUpdate = collectionProperties.Attribute(XmlMetadataSchema.ApplicationCompositionCollectionAllowUpdateAttribute).ValueOrDefault("false");
            var allowRemoval = collectionProperties.Attribute(XmlMetadataSchema.ApplicationCompositionCollectionAllowRemovalAttribute).ValueOrDefault("false");
            var listSchema = collectionProperties.Attribute(XmlMetadataSchema.ApplicationCompositionCollectionListSchemaAttribute).ValueOrDefault("list");
            var autoCommit = collectionProperties.Attribute(XmlMetadataSchema.ApplicationCompositionCollectionAutoCommitAttribute).ValueOrDefault(true);
            var hideExistingData = collectionProperties.Attribute(XmlMetadataSchema.ApplicationCompositionCollectionHideExistingDataAttribute).ValueOrDefault(false);
            var orderbyfield = collectionProperties.Attribute(XmlMetadataSchema.ApplicationCompositionCollectionOrderByField).ValueOrDefault((string)null);

            var prefilterFunction = collectionProperties.Attribute(XmlMetadataSchema.ApplicationCompositionCollectionPreFilterFunctionField).ValueOrDefault((string)null);
            // marks pre filter to be validated
            // sourceSchemaId is null when derived from component
            if (prefilterFunction != null && sourceSchemaId != null) {
                ApplicationMetadataValidator.AddCompositionPreFilterToValidate(applicationName, sourceSchemaId, prefilterFunction);
            }

            return new CompositionCollectionProperties(allowRemoval, allowInsertion, allowUpdate, listSchema, autoCommit, hideExistingData, orderbyfield, prefilterFunction);
        }


        /// <summary>
        ///     Deseriliazes the specified XML element to its corresponding
        /// 
        ///     Returns <see langword="null"/> if the application does not
        ///     contain a web schema.
        /// </summary>
        /// <param name="applicationName"></param>
        /// <param name="entityName"></param>
        /// <param name="application">The `application` element containing the web schema to be deserialized.</param>
        /// <param name="idFieldName"></param>
        private IDictionary<ApplicationMetadataSchemaKey, ApplicationSchemaDefinition> ParseSchemas(string applicationName, string applicationTitle,
            string entityName, XElement application, string idFieldName, string userIdFieldName) {
            var schemasElement = application.Elements().First(f => f.Name.LocalName == XmlMetadataSchema.SchemasElement);
            var xElements = schemasElement.Elements();
            var resultDictionary = new Dictionary<ApplicationMetadataSchemaKey, ApplicationSchemaDefinition>();
            foreach (var xElement in xElements) {
                DoParseSchema(applicationName, applicationTitle, entityName, idFieldName, userIdFieldName, xElement, resultDictionary);
            }
            return resultDictionary;
        }

        private void DoParseSchema(string applicationName, string applicationTitle, string entityName, string idFieldName, string userIdFieldName,
            XElement xElement, Dictionary<ApplicationMetadataSchemaKey, ApplicationSchemaDefinition> resultDictionary) {
            var localName = xElement.Name.LocalName;
            var id = xElement.Attribute(XmlMetadataSchema.SchemaIdAttribute).ValueOrDefault((string)null);
            //TODO: switch default redeclaring behaviour to false and fix all metadatas
            var redeclaring = xElement.Attribute(XmlMetadataSchema.SchemaRedeclaringSchemaAttribute).ValueOrDefault(true);
            var modeAttr = xElement.Attribute(XmlMetadataSchema.SchemaModeAttribute).ValueOrDefault((string)null);
            var platformAttr = xElement.Attribute(XmlMetadataSchema.SchemaPlatformAttribute).ValueOrDefault((string)null);
            var title = xElement.Attribute(XmlMetadataSchema.SchemaTitleAttribute).ValueOrDefault((string)null);
            var stereotypeAttr = xElement.Attribute(XmlMetadataSchema.SchemaStereotypeAttribute).ValueOrDefault((string)null);
            var isAbstract = xElement.Attribute(XmlMetadataSchema.SchemaAbstractAttribute).ValueOrDefault(false);
            var parentSchemaValue =
                xElement.Attribute(XmlMetadataSchema.SchemaParentSchemaAttribute).ValueOrDefault((string)null);
            var unionSchema = xElement.Attribute(XmlMetadataSchema.SchemaUnionSchemaAttribute).ValueOrDefault((string)null);

            var stereotype = SchemaStereotype.None;

            ClientPlatform? platform = null;
            if (stereotypeAttr != null) {
                stereotype = StereotypeFactory.ParseStereotype(stereotypeAttr);
            }
            var mode = SchemaMode.None;
            if (modeAttr != null) {
                Enum.TryParse(modeAttr, out mode);
            }
            if (platformAttr != null) {
                if (platformAttr == "web") {
                    platform = ClientPlatform.Web;
                } else if (platformAttr == "mobile") {
                    platform = ClientPlatform.Mobile;
                }
            }
            if (localName == XmlMetadataSchema.DetailElement) {
                id = ApplicationMetadataConstants.Detail;
                if (stereotype == SchemaStereotype.None) {
                    stereotype = SchemaStereotype.Detail;
                }
            } else if (localName == XmlMetadataSchema.ListElement) {
                id = ApplicationMetadataConstants.List;
                if (stereotype == SchemaStereotype.None) {
                    stereotype = SchemaStereotype.List;
                }
            }
            var displayables = ParseDisplayables(applicationName, id, xElement, entityName);
            ValidateSingleSecondarySection(displayables);
            var schemaProperties = ParseProperties(xElement, id);
            var filters = XmlFilterMetadataParser.ParseSchemaFilters(xElement, stereotype);
            ApplicationSchemaDefinition parentSchema = null;
            if (parentSchemaValue != null) {
                parentSchema = LookupParentSchema(id, applicationName, parentSchemaValue, platform, resultDictionary,
                    displayables);
            }

            ApplicationSchemaDefinition printSchema = null;
            string printSchemaValue = null;
            if (schemaProperties.TryGetValue("list.print.schema", out printSchemaValue) && printSchemaValue != null) {
                printSchema = LookupSchema(id, applicationName, printSchemaValue, platform, resultDictionary);
            }
            ApplicationCommandSchema applicationCommandSchema = ParseCommandSchema(xElement);

            var key = new ApplicationMetadataSchemaKey(id, modeAttr, platformAttr);
            resultDictionary.Add(key,
                ApplicationSchemaFactory.GetInstance(entityName, applicationName, applicationTitle, title, id, redeclaring, stereotypeAttr, stereotype, mode, platform,
                    isAbstract, displayables, filters, schemaProperties, parentSchema, printSchema, applicationCommandSchema, idFieldName,
                    userIdFieldName, unionSchema, ParseEvents(xElement, id)));
        }

        private static ApplicationSchemaDefinition LookupParentSchema(string id, string applicationName, string parentSchemaValue, ClientPlatform? platform,
            Dictionary<ApplicationMetadataSchemaKey, ApplicationSchemaDefinition> resultDictionary, IList<IApplicationDisplayable> displayables) {

            ApplicationSchemaDefinition parentSchema = LookupSchema(id, applicationName, parentSchemaValue, platform, resultDictionary);

            if (parentSchema != null && parentSchema.Displayables.Any() && displayables.Any(d => d.GetType() != typeof(ApplicationSection) && d.GetType() != typeof(ApplicationSchemaCustomization))) {
                throw new InvalidOperationException("concrete schemas must only declare displayables inside sections");
            }
            return parentSchema;
        }

        private static ApplicationSchemaDefinition LookupSchema(string id, string applicationName, string schemaId, ClientPlatform? platform,
            Dictionary<ApplicationMetadataSchemaKey, ApplicationSchemaDefinition> resultDictionary) {

            ApplicationSchemaDefinition schema = null;

            if (!resultDictionary.TryGetValue(new ApplicationMetadataSchemaKey(schemaId, (SchemaMode?)null, platform), out schema)) {
                throw new InvalidOperationException(string.Format(MissingParentSchema, id, applicationName, schemaId));
            }
            return schema;
        }

        private ApplicationCommandSchema ParseCommandSchema(XElement xElement) {
            var commandsSchemaEl = xElement.Elements().FirstOrDefault(f => f.IsNamed(XmlCommandSchema.CommandToolBarElements));
            return new ApplicationCommandSchema(XmlCommandBarMetadataParser.DoParse(commandsSchemaEl), _commandBars);
        }

        private static IList<ICommandDisplayable> ParseCommands(XElement commandsSchemaEl) {
            return commandsSchemaEl.Elements().Select(XmlCommandBarMetadataParser.GetCommandDisplayable).ToList();
        }

        /// <summary>
        ///     Deseriliazes the specified XML element to its corresponding
        ///     <seealso>
        ///         <cref>CompleteApplicationMetadata</cref>
        ///     </seealso>
        ///     representation.
        /// </summary>
        /// <param name="application">The `application` element to be deserialized.</param>
        /// <param name="entityMetadata">The catalog of entity metadata to aid in the application parsing.</param>
        private CompleteApplicationMetadataDefinition ParseApplication(XElement application, IEnumerable<EntityMetadata> entityMetadata) {
            var guid = application.Attribute(XmlMetadataSchema.ApplicationIdAttribute).ValueOrDefault((string)null);
            var id = guid != null ? Guid.Parse(guid) : (Guid?)null;
            var name = application.Attribute(XmlMetadataSchema.ApplicationNameAttribute).Value;
            if (_isSWDB) {
                name = "_" + name;
            }
            var title = application.Attribute(XmlMetadataSchema.ApplicationTitleAttribute).Value;
            var properties = ParseProperties(application, name);
            var entity = application.Attribute(XmlMetadataSchema.ApplicationEntityAttribute).Value;
            var role = application.Attribute(XmlMetadataSchema.RoleAttribute).ValueOrDefault((string)null);
            var auditFlag = application.Attribute(XmlMetadataSchema.ApplicationAuditFlagAttribute).ValueOrDefault(false);
            if (_isSWDB) {
                entity = entity + "_";
            }
            var service = application.Attribute(XmlMetadataSchema.ApplicationServiceAttribute).ValueOrDefault((string)null);
            var metadata = entityMetadata.FirstOrDefault(e => e.Name.EqualsIc(entity));
            if (metadata == null) {
                if (!entity.EndsWith("_")) {
                    throw new InvalidOperationException("entity {0} not found".Fmt(entity));
                }
                LoggingUtil.DefaultLog.WarnFormat("entity {0} not found. Are you missing a dll reference?", entity);
                return null;
            }

            var idFieldName = metadata
                .Schema
                .IdAttribute
                .Name;
            var userIdFieldName = metadata
             .Schema
             .UserIdAttribute
             .Name;

            var appFilters = XmlFilterMetadataParser.ParseSchemaFilters(application);

            var appComponents = ParseComponents(name, entity, application, idFieldName);
            
            MetadataProvider.AddComponents(name, appComponents);

            var completeApplicationMetadataDefinition = new CompleteApplicationMetadataDefinition(id, name, title, entity, idFieldName, userIdFieldName, properties, null, appComponents, appFilters, service, role, auditFlag);
            //
            MetadataProvider.AddTransientApplication(completeApplicationMetadataDefinition);

            var schemas = ParseSchemas(name, title, entity, application, idFieldName, userIdFieldName);
            completeApplicationMetadataDefinition.SetSchemas(schemas);
            
            return completeApplicationMetadataDefinition;
        }



        private static List<DisplayableComponent> ParseComponents(string name, string entity, XElement application, string idFieldName) {
            var resultList = new List<DisplayableComponent>();
            var firstOrDefault = application.Elements().FirstOrDefault(f => f.Name.LocalName == XmlMetadataSchema.ComponentsElement);
            if (firstOrDefault == null) {
                return resultList;
            }
            var componentsElement = firstOrDefault.Elements();
            foreach (var component in componentsElement) {
                resultList.Add(ParseComponent(component, name, entity));
            }
            return resultList;
        }

        private static DisplayableComponent ParseComponent(XElement component, string applicationName, string entityName) {
            var resultComponent = new DisplayableComponent {
                Id = component.Attribute(XmlBaseSchemaConstants.BaseDisplayableIdAttribute).Value,
                RealDisplayables = ParseDisplayables(applicationName, null, component, entityName),
            };
            return resultComponent;
        }

        private static IApplicationDisplayable FindDisplayable(string applicationName, string schemaId, string entityName, XElement xElement, EntityMetadata entityMetadata) {
            var xName = xElement.Name.LocalName;

            if (xName == XmlMetadataSchema.CustomizationElement) {
                return ParseCustomization(applicationName, schemaId, xElement, entityMetadata);
            }

            if (xName == XmlMetadataSchema.FieldElement) {
                return ParseField(applicationName, xElement, entityMetadata);
            }
            if (xName == XmlMetadataSchema.ApplicationSectionElement) {
                return ParseSection(applicationName, schemaId, xElement, entityMetadata);
            }
            if (xName == XmlMetadataSchema.ApplicationTabElement) {
                return ParseTab(applicationName, schemaId, xElement, entityName);
            }
            if (xName == XmlMetadataSchema.ApplicationCompositionElement) {
                return ParseComposition(xElement, applicationName, schemaId, entityName);
            }
            if (xName == XmlMetadataSchema.ApplicationAssociationElement) {
                return ParseAssociation(xElement, applicationName, schemaId, entityMetadata);
            }
            if (xName == XmlMetadataSchema.OptionFieldElement) {
                return ParseOptions(xElement, applicationName, schemaId);
            }
            if (xName == XmlMetadataSchema.ReferenceElement) {
                return ParseReference(xElement);
            }
            return null;
        }



        private static IDictionary<string, string> ParseProperties(XElement xElement, string schemaId) {
            IDictionary<string, string> propertiesDictionary = new Dictionary<string, string>();
            var properties = xElement.Elements().FirstOrDefault(f => f.Name.LocalName == XmlMetadataSchema.ApplicationPropertiesElement);
            if (properties == null) {
                return new Dictionary<string, string>();
            }
            foreach (var property in properties.Elements()) {
                var key = property.Attribute(XmlMetadataSchema.ApplicationPropertyKeyAttribute).Value;
                if (propertiesDictionary.ContainsKey(key)) {
                    throw new InvalidOperationException("property {0} already present for application/schema {1}".Fmt(key, schemaId));
                }
                propertiesDictionary.Add(key, property.Attribute(XmlMetadataSchema.ApplicationPropertyValueAttribute).Value);
            }
            return propertiesDictionary;
        }

        private static ToStringExpression ParseToStringExpression(XElement application) {
            var attr = application.Attribute(XmlMetadataSchema.ApplicationToStringAttribute);
            if (attr == null) {
                return null;
            }
            return ToStringELParser.ParseExpression(attr.Value);
        }

        private readonly IEnumerable<EntityMetadata> _entityMetadata;
        private readonly IDictionary<string, CommandBarDefinition> _commandBars;
        private XmlTemplateHandler _xmlTemplateHandler;

        /// <summary>
        ///     Parses the XML document provided by the specified
        ///     stream and returns all application metadata.
        /// </summary>
        /// <param name="stream">The input stream containing the XML representation of the metadata file.</param>
        /// <param name="alreadyParsedTemplates"></param>
        [NotNull]
        public IEnumerable<CompleteApplicationMetadataDefinition> Parse(TextReader stream, ISet<string> alreadyParsedTemplates = null) {
            if (stream == null) throw new ArgumentNullException("stream");

            var result = new List<CompleteApplicationMetadataDefinition>();

            var document = XDocument.Load(stream);
            if (null == document.Root) throw new InvalidDataException();

            var xElements = document.Root.Elements();
            var enumerable = xElements as XElement[] ?? xElements.ToArray();

            var applications = enumerable.FirstOrDefault(f => f.IsNamed(XmlMetadataSchema.ApplicationsElement));
            var templates = enumerable.FirstOrDefault(e => e.IsNamed(XmlMetadataSchema.TemplatesElement));
            result.AddRange(_xmlTemplateHandler.HandleTemplatesForApplications(templates, _entityMetadata, _commandBars, _isSWDB, alreadyParsedTemplates));


            if (null == applications) {
                //we just have templates, but no applications defined
                return result;
            }

            var applicationElements = applications.Elements().Where(e => e.Name.LocalName == XmlMetadataSchema.ApplicationElement);
            var overridenApplications = new List<CompleteApplicationMetadataDefinition>();
            foreach (var applicationEl in applicationElements) {
                var application = ParseApplication(applicationEl, _entityMetadata);
                if (application != null) {
                    application.Schemas()
                    .Where(s => SchemaStereotype.List.Equals(s.Value.Stereotype))
                    .ToList().ForEach(s => AddNoResultsNewSchema(application, s.Value));
                    overridenApplications.Add(application);
                }
            }
            var resultApplications = MetadataMerger.MergeApplications(result, overridenApplications).ToList();

            // verifies if fields are enabled
            resultApplications.ForEach(a => a.SchemasList.ForEach(XmlEnabledFieldsVerifier.VerifyEnabledFields));

            return resultApplications;
        }

        private static void AddNoResultsNewSchema(CompleteApplicationMetadataDefinition app,
            ApplicationSchemaDefinition schema) {
            // return if is set to prevent the new button on no result list
            string preventNoResults;
            schema.Properties.TryGetValue(ApplicationSchemaPropertiesCatalog.PreventNoResultsNew, out preventNoResults);
            if ("true".Equals(preventNoResults)) {
                return;
            }

            // if the schema of no result new is set uses it 
            string noResultsNewSchema;
            schema.Properties.TryGetValue(ApplicationSchemaPropertiesCatalog.NoResultsNewSchema, out noResultsNewSchema);
            if (noResultsNewSchema != null) {
                schema.NoResultsNewSchema = noResultsNewSchema;
                return;
            }

            // finally if there is only one detail new schema on the same app, it is set as the new schema on no results list
            var detailNewSchemas = app.Schemas().Where(s => SchemaStereotype.DetailNew.Equals(s.Value.Stereotype)).ToList();
            if (detailNewSchemas.Count() != 1) {
                return;
            }
            schema.NoResultsNewSchema = detailNewSchemas.First().Value.SchemaId;
        }

        private static class XmlWidgetParser {
            private static string ParseLookupTargetQualifier(XElement widget, string attribute) {
                var att = widget.Attribute(XmlMetadataSchema.WidgetLookupTargetQualifierAttribute);

                if (null != att && false == string.IsNullOrWhiteSpace(att.Value)) {
                    return att.Value;
                }

                var segments = attribute
                    .Split(new[] { ApplicationFieldDefinition.AttributeQualifierSeparator }, StringSplitOptions.RemoveEmptyEntries);

                var isFieldQualified = segments.Length == 2;

                return isFieldQualified
                    ? segments[0]
                    : null;
            }

            private static LookupWidgetDefinition.Filter ParseLookupFilter(XElement lookupFilter) {
                var sourceField = lookupFilter.Attribute(XmlMetadataSchema.WidgetLookupFilterAttributeSourceField).Value;
                var targetField = lookupFilter.Attribute(XmlMetadataSchema.WidgetLookupFilterAttributeTargetField).ValueOrDefault((string)null);
                var literal = lookupFilter.Attribute(XmlMetadataSchema.WidgetLookupFilterAttributeLiteral).ValueOrDefault((string)null);

                return new LookupWidgetDefinition.Filter(sourceField, targetField, literal);
            }

            private static IEnumerable<LookupWidgetDefinition.Filter> ParseLookupFilters(XElement lookup) {
                var filters = lookup.Elements().FirstOrDefault(f => f.Name.LocalName == XmlMetadataSchema.WidgetLookupFiltersElement);

                if (null == filters) {
                    return Enumerable.Empty<LookupWidgetDefinition.Filter>();
                }

                return filters
                    .Elements().Where(f => f.Name.LocalName == XmlMetadataSchema.WidgetLookupFilterElement)
                    .Select(ParseLookupFilter)
                    .ToList();
            }

            private static IWidgetDefinition ParseLookup(XElement widget, string attribute) {
                var sourceApplication = widget.Attribute(XmlMetadataSchema.WidgetLookupSourceApplicationAttribute).Value;
                var sourceField = widget.Attribute(XmlMetadataSchema.WidgetLookupSourceFieldAttribute).Value;
                var targetField = widget.Attribute(XmlMetadataSchema.WidgetLookupTargetFieldAttribute).Value;
                var targetQualifier = ParseLookupTargetQualifier(widget, attribute);
                var filters = ParseLookupFilters(widget);

                var sourceDisplay = widget
                    .Attribute(XmlMetadataSchema.WidgetLookupSourceDisplayAttribute)
                    .Value
                    .Split(XmlMetadataSchema.ItemDelimeter)
                    .Select(v => v.Trim())
                    .Where(v => v.Length > 0)
                    .ToList();

                return new LookupWidgetDefinition(sourceApplication, sourceField, sourceDisplay, targetField, targetQualifier, filters);
            }

            private static IWidgetDefinition ParseDate(XElement widget) {
                var format = widget.Attribute(XmlMetadataSchema.WidgetDateFormatAttribute).ValueOrDefault(DateWidgetDefinition.ShortFormat);
                var time = widget.Attribute(XmlMetadataSchema.WidgetDateTimeAttribute).ValueOrDefault(false);
                var min = widget.Attribute(XmlMetadataSchema.WidgetDateMinAttribute).ValueOrDefault(DateTime.MinValue);
                var max = widget.Attribute(XmlMetadataSchema.WidgetDateMaxAttribute).ValueOrDefault(DateTime.MaxValue);

                return new DateWidgetDefinition(format, time, min, max);
            }

            private static IWidgetDefinition ParseNumber(XElement widget) {
                var decimals = widget.Attribute(XmlMetadataSchema.WidgetNumberDecimalsAttribute).ValueOrDefault(0);
                var min = widget.Attribute(XmlMetadataSchema.WidgetNumberMinAttribute).ValueOrDefault((decimal?)null);
                var max = widget.Attribute(XmlMetadataSchema.WidgetNumberMaxAttribute).ValueOrDefault((decimal?)null);

                return new NumberWidgetDefinition(decimals, min, max);
            }

            private static TextWidgetDefinition ParseText() {
                return new TextWidgetDefinition();
            }

            public static IWidgetDefinition Parse(XContainer container, string attribute, bool isHidden) {
                var widget = container.Elements().FirstOrDefault(f => f.Name.LocalName == XmlMetadataSchema.WidgetHiddenElement);
                if (null != widget || isHidden) {
                    return new HiddenWidgetDefinition();
                }

                widget = container.Elements().FirstOrDefault(f => f.Name.LocalName == XmlMetadataSchema.WidgetNumberElement);
                if (null != widget) {
                    return ParseNumber(widget);
                }

                widget = container.Elements().FirstOrDefault(f => f.Name.LocalName == XmlMetadataSchema.WidgetDateElement);
                if (null != widget) {
                    return ParseDate(widget);
                }

                widget = container.Elements().FirstOrDefault(f => f.Name.LocalName == XmlMetadataSchema.WidgetLookupElement);
                if (null != widget) {
                    return ParseLookup(widget, attribute);
                }

                return ParseText();
            }
        }
    }
}
