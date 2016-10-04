using JetBrains.Annotations;
using softwrench.sw4.Shared2.Data.Association;
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
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Xml.Linq;



namespace softWrench.sW4.Metadata.Parsing {
    /// <summary>
    ///     Provides parsing and deserialization of
    ///     application metadata stored in a XML file.
    /// </summary>
    internal sealed class XmlApplicationMetadataParser {
        private const string MissingRelationship = "application {0} references unknown entity {1}";
        private const string MissingParentSchema = "Error building schema {0} for application {1}.parentSchema {2} not found. Please assure its declared before the concrete schema";
        //        private static readonly int? Infinite = null;

        internal enum FieldRendererType {
            ASSOCIATION, COMPOSITION, OPTION, BASE, SECTION
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
                    default: {

                            if (entity != null) {
                                var attr = entity.Schema.Attributes.FirstOrDefault(a => a.Name == targetName);
                                if (attr != null && attr.Type == "timestamp") {
                                    return new FieldRenderer(FieldRenderer.BaseRendererType.DATETIME.ToString().ToLower(), null, targetName);
                                }
                            }
                            return new FieldRenderer();
                        }


                }
            }
            var type = renderer.Attribute(XmlMetadataSchema.RendererAttributeType).Value;
            var parameters = renderer.Attribute(XmlMetadataSchema.RendererAttributeParams).ValueOrDefault((string)null);

            switch (ftype) {
                case FieldRendererType.ASSOCIATION:
                    return new AssociationFieldRenderer(type, parameters, targetName);
                case FieldRendererType.COMPOSITION:
                    return new CompositionFieldRenderer(type, parameters, targetName);
                case FieldRendererType.OPTION:
                    return new OptionFieldRenderer(type, parameters, targetName);
                default:
                    return new FieldRenderer(type, parameters, targetName);
            }
        }

        private static FieldFilter ParseFilterNew(XElement renderer, string targetName) {

            if (renderer == null) {
                return null;
            }

            var operation = renderer.Attribute(XmlMetadataSchema.FilterOperationType).Value;
            var parameters = renderer.Attribute(XmlMetadataSchema.FilterAttributeParams).ValueOrDefault((string)null);
            var defaultValue = renderer.Attribute(XmlMetadataSchema.FilterAttributeDefault).ValueOrDefault((string)null);
            
            return new FieldFilter(operation, parameters, defaultValue, targetName);
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
            var isRequired = field.Attribute(XmlMetadataSchema.BaseDisplayableRequiredAttribute).ValueOrDefault("false");
            var isReadOnly = field.Attribute(XmlMetadataSchema.FieldAttributeReadOnly).ValueOrDefault(false);
            var isHidden = field.Attribute(XmlMetadataSchema.FieldAttributeHidden).ValueOrDefault(false);
            var showExpression = field.Attribute(XmlMetadataSchema.BaseDisplayableShowExpressionAtribute).ValueOrDefault("true");
            var toolTip = field.Attribute(XmlMetadataSchema.BaseDisplayableToolTipAtribute).ValueOrDefault(label);
            var defaultValue = field.Attribute(XmlMetadataSchema.FieldAttributeDefaultValue).ValueOrDefault((string)null);
            var renderer = ParseRendererNew(field.Elements().FirstOrDefault(f => f.Name.LocalName == XmlMetadataSchema.RendererElement),
                attribute, FieldRendererType.BASE, entityMetadata);
            var qualifier = field.Attribute(XmlMetadataSchema.FieldAttributeQualifier).ValueOrDefault((string)null);
            var widget = XmlWidgetParser.Parse(field, attribute, isHidden);
            var attributeToServer = field.Attribute(XmlMetadataSchema.FieldAttributeAttributeToServer).ValueOrDefault((string)null);
            var events = ParseEvents(field);
            var filterElement = field.Elements().FirstOrDefault(f => f.Name.LocalName == XmlMetadataSchema.FilterElement);
            var enableExpression = field.Attribute(XmlMetadataSchema.BaseDisplayableEnableExpressionAtribute).ValueOrDefault("true");
            var evalExpression = field.Attribute(XmlMetadataSchema.BaseDisplayableEvalExpressionAtribute).ValueOrDefault((string)null);
            return new ApplicationFieldDefinition(applicationName, attribute, label, isRequired, isReadOnly, isHidden, renderer,
                ParseFilterNew(filterElement, attribute), widget, defaultValue, qualifier, showExpression, toolTip, attributeToServer, events, enableExpression,evalExpression);
        }

        private static ISet<ApplicationEvent> ParseEvents(XElement element) {
            ISet<ApplicationEvent> result = new HashSet<ApplicationEvent>();

            var events = element.Elements().Where(e => e.Name.LocalName == XmlMetadataSchema.EventsElement);
            foreach (var applicationEvent in events) {
                result.Add(new ApplicationEvent {
                    Type = applicationEvent.Attribute(XmlMetadataSchema.EventsTypeAttribute).Value,
                    Service = applicationEvent.Attribute(XmlMetadataSchema.EventsServiceAttribute).Value,
                    Method = applicationEvent.Attribute(XmlMetadataSchema.EventsMethodAttribute).Value
                });
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
        private static List<IApplicationDisplayable> ParseDisplayables(string applicationName, XContainer schema, string entityName) {
            var displayables = new List<IApplicationDisplayable>();
            var entityMetadata = MetadataProvider.Entity(entityName);
            foreach (var xElement in schema.Elements()) {
                var applicationDisplayable = FindDisplayable(applicationName, entityName, xElement, entityMetadata);
                if (applicationDisplayable != null) {
                    displayables.Add(applicationDisplayable);
                }
            }
            return displayables;
        }

        private static ReferenceDisplayable ParseReference(XElement xElement) {
            var id = xElement.Attribute(XmlMetadataSchema.BaseDisplayableIdAttribute).Value;
            var readOnly = xElement.Attribute(XmlMetadataSchema.FieldAttributeReadOnly).ValueOrDefault((bool?)null);

            return new ReferenceDisplayable {
                Id = id,
                ShowExpression = xElement.Attribute(XmlMetadataSchema.BaseDisplayableShowExpressionAtribute).ValueOrDefault((string)null),
                Label = xElement.Attribute(XmlMetadataSchema.BaseDisplayableLabelAttribute).ValueOrDefault((string)null),
                Attribute = xElement.Attribute(XmlMetadataSchema.AttributeElement).ValueOrDefault((string)null),
                PropertiesString = xElement.Attribute(XmlMetadataSchema.ApplicationPropertiesElement).ValueOrDefault((string)null),
                ReadOnly = readOnly,
                EnableExpression = xElement.Attribute(XmlMetadataSchema.BaseDisplayableEnableExpressionAtribute).ValueOrDefault((string)null)
            };
        }

        private static IApplicationDisplayable ParseTab(string applicationName, XElement tabElement, string entityName) {
            var id = tabElement.Attribute(XmlMetadataSchema.BaseDisplayableIdAttribute).ValueOrDefault((string)null);
            var label = tabElement.Attribute(XmlMetadataSchema.BaseDisplayableLabelAttribute).ValueOrDefault((String)null);
            var showExpression = tabElement.Attribute(XmlMetadataSchema.BaseDisplayableShowExpressionAtribute).ValueOrDefault("true");
            var toolTip = tabElement.Attribute(XmlMetadataSchema.BaseDisplayableToolTipAtribute).ValueOrDefault(label);
            var displayables = ParseDisplayables(applicationName, tabElement, entityName);
            return new ApplicationTabDefinition(id, applicationName, label, displayables, toolTip, showExpression);
        }

        private static IApplicationDisplayable ParseSection(string applicationName, XElement sectionElement, EntityMetadata entityMetadata) {
            var id = sectionElement.Attribute(XmlMetadataSchema.ApplicationSectionIdAttribute).ValueOrDefault((string)null);
            var @abstract = sectionElement.Attribute(XmlMetadataSchema.ApplicationSectionAbstractAttribute).ValueOrDefault(false);
            var resourcePath = sectionElement.Attribute(XmlMetadataSchema.ApplicationSectionResourcePathAttribute).ValueOrDefault((String)null);
            var parameters = sectionElement.Attribute(XmlMetadataSchema.ApplicationSectionParametersAttribute).ValueOrDefault((String)null);
            var label = sectionElement.Attribute(XmlMetadataSchema.ApplicationSectionLabelAttribute).ValueOrDefault((String)null);
            var attribute = sectionElement.Attribute(XmlMetadataSchema.ApplicationSectionAttributeAttribute).ValueOrDefault((String)null);
            var showExpression = sectionElement.Attribute(XmlMetadataSchema.BaseDisplayableShowExpressionAtribute).ValueOrDefault("true");
            var toolTip = sectionElement.Attribute(XmlMetadataSchema.BaseDisplayableToolTipAtribute).ValueOrDefault(label);
            var displayables = ParseDisplayables(applicationName, sectionElement, entityMetadata.Name);
            var header = ParseHeader(sectionElement);
            var orientation = sectionElement.Attribute(XmlMetadataSchema.ApplicationSectionOrientationAttribute).ValueOrDefault((String)null);
            var renderer = ParseRendererNew(sectionElement.Elements().FirstOrDefault(f => f.Name.LocalName == XmlMetadataSchema.RendererElement),
                attribute, FieldRendererType.SECTION, entityMetadata);
            // Removing this code due to "Asset Specification" section in IMAC application, Update Schema
            /*if (displayables != null && displayables.Count > 0 && !String.IsNullOrWhiteSpace(resourcePath)) {
                throw new InvalidOperationException("<section> cannot contains inner elements AND resourcePath attribute");
            }*/
            return new ApplicationSection(id, applicationName, @abstract, label, attribute, resourcePath, parameters,
                displayables, showExpression, toolTip, orientation, header, renderer);
        }

        private static ApplicationHeader ParseHeader(XContainer schema) {
            foreach (var xElement in schema.Elements()) {
                var xName = xElement.Name.LocalName;
                if (xName == XmlMetadataSchema.ApplicationHeaderElement) {
                    var label = xElement.Attribute(XmlMetadataSchema.ApplicationHeaderLabelAttribute).ValueOrDefault("");
                    var parameters = xElement.Attribute(XmlMetadataSchema.ApplicationHeaderParametersAttribute).ValueOrDefault((string)null);
                    var displacement = xElement.Attribute(XmlMetadataSchema.ApplicationHeaderDisplacementAttribute).ValueOrDefault((string)null);
                    var showExpression = xElement.Attribute(XmlMetadataSchema.BaseDisplayableShowExpressionAtribute).ValueOrDefault((string)"true");

                    return new ApplicationHeader(label, parameters, displacement, showExpression);
                }
            }
            return null;
        }

        private static IApplicationDisplayable ParseOptions(XElement xElement, string applicationName) {
            var attribute = xElement.Attribute(XmlMetadataSchema.FieldAttributeAttribute).Value;
            var label = xElement.Attribute(XmlMetadataSchema.FieldAttributeLabel).ValueOrDefault("");
            var isRequired = xElement.Attribute(XmlMetadataSchema.BaseDisplayableRequiredAttribute).ValueOrDefault("false");
            var isReadOnly = xElement.Attribute(XmlMetadataSchema.FieldAttributeReadOnly).ValueOrDefault(false);
            var isHidden = xElement.Attribute(XmlMetadataSchema.FieldAttributeHidden).ValueOrDefault(false);
            var defaultValue = xElement.Attribute(XmlMetadataSchema.FieldAttributeDefaultValue).ValueOrDefault((string)null);
            var showExpression = xElement.Attribute(XmlMetadataSchema.BaseDisplayableShowExpressionAtribute).ValueOrDefault("true");
            var toolTip = xElement.Attribute(XmlMetadataSchema.BaseDisplayableToolTipAtribute).ValueOrDefault(label);
            var attributeToServer = xElement.Attribute(XmlMetadataSchema.FieldAttributeAttributeToServer).ValueOrDefault((string)null);
            var providerAttribute = xElement.Attribute(XmlMetadataSchema.OptionFieldProviderAttribute).ValueOrDefault((string)null);
            var sort = xElement.Attribute(XmlMetadataSchema.OptionFieldSortAttribute).ValueOrDefault(providerAttribute != null);
            var dependantFields = xElement.Attribute(XmlMetadataSchema.ApplicationAssociationDependantFieldsAttribute).ValueOrDefault((string)null);
            var isEnabled = xElement.Attribute(XmlMetadataSchema.BaseDisplayableEnableExpressionAtribute).ValueOrDefault("true");
            var qualifier = xElement.Attribute(XmlMetadataSchema.FieldAttributeQualifier).ValueOrDefault((string)null);
            var rendererElement = xElement.Elements().FirstOrDefault(f => f.Name.LocalName == XmlMetadataSchema.RendererElement);
            var renderer = new OptionFieldRenderer();
            if (rendererElement != null) {
                renderer = (OptionFieldRenderer)ParseRendererNew(rendererElement, attribute, FieldRendererType.OPTION);
            };
            var filterElement = xElement.Elements().FirstOrDefault(f => f.Name.LocalName == XmlMetadataSchema.FilterElement);
            var events = ParseEvents(xElement);
            return new OptionField(applicationName, label, attribute, qualifier, isRequired, isReadOnly, isHidden, renderer, ParseFilterNew(filterElement, attribute),
                xElement.Elements().Where(e => e.Name.LocalName == XmlMetadataSchema.OptionElement).Select(ParseOption).ToList(),
                defaultValue, sort, showExpression, toolTip, attributeToServer, events, providerAttribute, dependantFields, isEnabled);
        }

        private static IAssociationOption ParseOption(XElement xElement) {
            var label = xElement.Attribute(XmlMetadataSchema.OptionElementLabelAttribute).Value;
            var value = xElement.Attribute(XmlMetadataSchema.OptionElementValueAttribute).Value;
            return new AssociationOption(value, label);
        }


        //        private static IEnumerable<ApplicationAssociation> ParseAssociations(XContainer schema, String applicationName) {
        //            return schema.Element(XmlMetadataSchema.DetailElement)
        //                .Elements(XmlMetadataSchema.ApplicationAssociationElement)
        //                .Select(e => ParseAssociation(e, applicationName))
        //                .ToList();
        //        }

        private static ApplicationAssociationDefinition ParseAssociation(XElement association, String applicationName) {
            var label = association.Attribute(XmlMetadataSchema.ApplicationAssociationLabelAttribute).Value;
            var labelField = association.Attribute(XmlMetadataSchema.ApplicationAssociationLabelFieldAttribute).Value;
            var labelPattern = association.Attribute(XmlMetadataSchema.ApplicationAssociationLabelPatternAttribute).ValueOrDefault((string)null);
            var target = association.Attribute(XmlMetadataSchema.ApplicationAssociationTargetAttribute).Value;
            var defaultValue = association.Attribute(XmlMetadataSchema.ApplicationAssociationDefaultValueAttribute).ValueOrDefault((string)null);
            var labelData = new ApplicationAssociationDefinition.LabelData(label, labelPattern, labelField, applicationName);
            var showExpression = association.Attribute(XmlMetadataSchema.BaseDisplayableShowExpressionAtribute).ValueOrDefault("true");
            var enableExpression = association.Attribute(XmlMetadataSchema.BaseDisplayableEnableExpressionAtribute).ValueOrDefault("true");
            var tooltip = association.Attribute(XmlMetadataSchema.BaseDisplayableToolTipAtribute).ValueOrDefault(label);
            var extraProjectionFields = association.Attribute(XmlMetadataSchema.ApplicationAssociationExtraProjectionFieldsAttribute).ValueOrDefault((string)null);
            var qualifier = association.Attribute(XmlMetadataSchema.FieldAttributeQualifier).ValueOrDefault((string)null);
            var required = association.Attribute(XmlMetadataSchema.BaseDisplayableRequiredAttribute).ValueOrDefault("false");

            return ApplicationAssociationFactory.GetInstance(applicationName, labelData, target, qualifier, ParseAssociationSchema(association, target), showExpression, tooltip, required, ParseEvents(association), defaultValue, extraProjectionFields, enableExpression);
        }

        private static ApplicationAssociationSchemaDefinition ParseAssociationSchema(XElement association, string targetName) {
            var rendererElement = association.Elements().FirstOrDefault(
                f => f.Name.LocalName == XmlMetadataSchema.RendererElement);
            var renderer = new AssociationFieldRenderer();
            var dataProvider = new AssociationDataProvider();
            if (rendererElement != null) {
                renderer = (AssociationFieldRenderer)ParseRendererNew(rendererElement, targetName, FieldRendererType.ASSOCIATION);
            }
            var dataProviderElement = association.Elements().FirstOrDefault(f => f.Name.LocalName == XmlMetadataSchema.DataProviderElement);
            if (dataProviderElement != null) {
                dataProvider = ParseDataProvider(dataProviderElement);
            }
            var dependantFields = association.Attribute(XmlMetadataSchema.ApplicationAssociationDependantFieldsAttribute).ValueOrDefault((string)null);
            var filterElement = association.Elements().FirstOrDefault(f => f.Name.LocalName == XmlMetadataSchema.FilterElement);
            return ApplicationAssociationFactory.GetSchemaInstance(dataProvider, renderer, ParseFilterNew(filterElement, targetName), dependantFields);
        }

        private static AssociationDataProvider ParseDataProvider(XElement dataProviderElement) {
            var whereClause = dataProviderElement.Attribute(XmlMetadataSchema.DataProviderWhereClause).ValueOrDefault((string)null);
            var prefilterFunction = dataProviderElement.Attribute(XmlMetadataSchema.DataProviderPreFilterFunction).ValueOrDefault((string)null);
            var postfilterFunction = dataProviderElement.Attribute(XmlMetadataSchema.DataProviderPostFilterFunction).ValueOrDefault((string)null);
            if (whereClause == null && prefilterFunction == null && postfilterFunction == null) {
                throw new InvalidOperationException("either whereclause of filterfunction should be provided for a dataprovider");
            }
            return new AssociationDataProvider(prefilterFunction, postfilterFunction, whereClause);
        }


        private static ApplicationCompositionDefinition ParseComposition(XElement composition, string applicationName, string entityName) {
            var label = composition.Attribute(XmlMetadataSchema.ApplicationCompositionLabelAttribute).ValueOrDefault("");
            var relationship = composition.Attribute(XmlMetadataSchema.ApplicationCompositionRelationshipAttribute).Value;
            var showExpression = composition.Attribute(XmlMetadataSchema.BaseDisplayableShowExpressionAtribute).ValueOrDefault("true");
            var toolTip = composition.Attribute(XmlMetadataSchema.BaseDisplayableToolTipAtribute).ValueOrDefault(label);
            var hidden = composition.Attribute(XmlMetadataSchema.BaseDisplayableHiddenAttribute).ValueOrDefault(false);
            var schema = ParseCompositionSchema(entityName, applicationName, relationship, composition);
            return ApplicationCompositionFactory.GetInstance(applicationName, relationship, label, schema, showExpression, toolTip, hidden, ParseHeader(composition));
        }

        private static ApplicationCompositionSchema ParseCompositionSchema(string entityName, String applicationName, string relationship, XElement composition) {
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
                throw new InvalidOperationException(String.Format(MissingRelationship, applicationName, relationship));
            }
            var entityAssociation = e.Associations.FirstOrDefault(a => a.Qualifier == EntityUtil.GetRelationshipName(relationship));
            if (entityAssociation == null) {
                throw new InvalidOperationException(String.Format(MissingRelationship, applicationName, relationship));
            }
            var isCollection = entityAssociation.Collection;
            var collectionProperties = ParseCollectionProperties(composition);
            if (collectionProperties != null || isCollection) {
                return new ApplicationCompositionCollectionSchema(inline, schemaId, collectionProperties, mode,
                    (CompositionFieldRenderer)ParseRendererNew(rendererElement, e.Name, FieldRendererType.COMPOSITION), printSchema, dependantfields, fetchType);
            }
            return new ApplicationCompositionSchema(inline, schemaId, mode,
                    (CompositionFieldRenderer)ParseRendererNew(rendererElement, e.Name, FieldRendererType.COMPOSITION), printSchema, dependantfields, fetchType);

        }

        private static CompositionCollectionProperties ParseCollectionProperties(XElement composition) {
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
            var orderbyfield = collectionProperties.Attribute(XmlMetadataSchema.ApplicationCompositionCollectionOrderByField).ValueOrDefault((String)null);
            var prefilterFunction = collectionProperties.Attribute(XmlMetadataSchema.ApplicationCompositionCollectionPreFilterFunctionField).ValueOrDefault((String)null);
            return new CompositionCollectionProperties(allowRemoval, allowInsertion, allowUpdate, listSchema, autoCommit, orderbyfield,prefilterFunction);

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
        private static IDictionary<ApplicationMetadataSchemaKey, ApplicationSchemaDefinition> ParseSchemas(string applicationName, string entityName, XElement application, string idFieldName) {
            var schemasElement = application.Elements().First(f => f.Name.LocalName == XmlMetadataSchema.SchemasElement);
            var xElements = schemasElement.Elements();
            var resultDictionary = new Dictionary<ApplicationMetadataSchemaKey, ApplicationSchemaDefinition>();
            foreach (var xElement in xElements) {
                var localName = xElement.Name.LocalName;
                var id = xElement.Attribute(XmlMetadataSchema.SchemaIdAttribute).ValueOrDefault((string)null);
                var modeAttr = xElement.Attribute(XmlMetadataSchema.SchemaModeAttribute).ValueOrDefault((string)null);
                var platformAttr = xElement.Attribute(XmlMetadataSchema.SchemaPlatformAttribute).ValueOrDefault((string)null);
                var title = xElement.Attribute(XmlMetadataSchema.SchemaTitleAttribute).ValueOrDefault((string)null);
                var stereotypeAttr = xElement.Attribute(XmlMetadataSchema.SchemaStereotypeAttribute).ValueOrDefault((string)null);
                var isAbstract = xElement.Attribute(XmlMetadataSchema.SchemaAbstractAttribute).ValueOrDefault(false);
                var parentSchemaValue = xElement.Attribute(XmlMetadataSchema.SchemaParentSchemaAttribute).ValueOrDefault((string)null);
                var unionSchema = xElement.Attribute(XmlMetadataSchema.SchemaUnionSchemaAttribute).ValueOrDefault((string)null);
                var stereotype = SchemaStereotype.None;
                ClientPlatform? platform = null;
                if (stereotypeAttr != null) {
                    Enum.TryParse(stereotypeAttr, true, out stereotype);
                }
                var mode = SchemaMode.None;
                if (modeAttr != null) {
                    Enum.TryParse(modeAttr, out mode);
                }
                if (platformAttr != null) {
                    if (platformAttr == "web") {
                        platform = ClientPlatform.Web;
                    }
                    else if (platformAttr == "mobile") {
                        platform = ClientPlatform.Mobile;
                    }
                }
                if (localName == XmlMetadataSchema.DetailElement) {
                    id = ApplicationMetadataConstants.Detail;
                    if (stereotype == SchemaStereotype.None) {
                        stereotype = SchemaStereotype.Detail;
                    }

                }
                else if (localName == XmlMetadataSchema.ListElement) {
                    id = ApplicationMetadataConstants.List;
                    if (stereotype == SchemaStereotype.None) {
                        stereotype = SchemaStereotype.List;
                    }
                }
                var displayables = ParseDisplayables(applicationName, xElement, entityName);
                var schemaProperties = ParseProperties(xElement);
                ApplicationSchemaDefinition parentSchema = null;
                if (parentSchemaValue != null) {
                    parentSchema = LookupParentSchema(id, applicationName, parentSchemaValue, platform, resultDictionary, displayables);
                }

                ApplicationSchemaDefinition printSchema = null;
                String printSchemaValue = null;
                if (schemaProperties.TryGetValue("list.print.schema", out printSchemaValue) && printSchemaValue != null) {
                    printSchema = LookupSchema(id, applicationName, printSchemaValue, platform, resultDictionary);
                }
                ApplicationCommandSchema applicationCommandSchema = ParseCommandSchema(xElement);
                resultDictionary.Add(new ApplicationMetadataSchemaKey(id, modeAttr, platformAttr),
                    ApplicationSchemaFactory.GetInstance(applicationName, title, id, stereotype, mode, platform,
                    isAbstract, displayables, schemaProperties, parentSchema, printSchema, applicationCommandSchema, idFieldName, unionSchema));
            }
            return resultDictionary;
        }

        private static ApplicationSchemaDefinition LookupParentSchema(string id, string applicationName, string parentSchemaValue, ClientPlatform? platform,
            Dictionary<ApplicationMetadataSchemaKey, ApplicationSchemaDefinition> resultDictionary, IList<IApplicationDisplayable> displayables) {

            ApplicationSchemaDefinition parentSchema = LookupSchema(id, applicationName, parentSchemaValue, platform, resultDictionary);

            if (parentSchema != null && parentSchema.Displayables.Any() && displayables.Any(d => d.GetType() != typeof(ApplicationSection))) {
                throw new InvalidOperationException("concrete schemas must only declare displayables inside sections");
            }
            return parentSchema;
        }

        private static ApplicationSchemaDefinition LookupSchema(string id, string applicationName, string schemaId, ClientPlatform? platform,
            Dictionary<ApplicationMetadataSchemaKey, ApplicationSchemaDefinition> resultDictionary) {

            ApplicationSchemaDefinition schema = null;

            if (!resultDictionary.TryGetValue(new ApplicationMetadataSchemaKey(schemaId, (SchemaMode?)null, platform), out schema)) {
                throw new InvalidOperationException(String.Format(MissingParentSchema, id, applicationName, schemaId));
            }
            return schema;
        }

        private static ApplicationCommandSchema ParseCommandSchema(XElement xElement) {
            var commandsSchemaEl = xElement.Elements().FirstOrDefault(f => f.Name.LocalName == XmlMetadataSchema.ApplicationCommandsElement);
            if (commandsSchemaEl == null) {
                return new ApplicationCommandSchema();
            }
            var removeUndeclared = commandsSchemaEl.Attribute(XmlMetadataSchema.ApplicationCommandRemoveUndeclaredAttribute).ValueOrDefault(false);
            var commands = ParseCommands(commandsSchemaEl);
            return new ApplicationCommandSchema(removeUndeclared, commands);
        }

        private static IList<ApplicationCommand> ParseCommands(XElement commandsSchemaEl) {
            IList<ApplicationCommand> commands = new List<ApplicationCommand>();
            foreach (var xElement in commandsSchemaEl.Elements()) {
                var id = xElement.Attribute(XmlMetadataSchema.ApplicationCommandIdAttribute).Value;
                var label = xElement.Attribute(XmlMetadataSchema.ApplicationCommandLabelAttribute).ValueOrDefault((String)null);
                var service = xElement.Attribute(XmlMetadataSchema.EventsServiceAttribute).ValueOrDefault((String)null);
                var method = xElement.Attribute(XmlMetadataSchema.EventsMethodAttribute).ValueOrDefault((String)null);
                var remove = xElement.Attribute(XmlMetadataSchema.ApplicationCommandRemoveAttribute).ValueOrDefault(false);
                var role = xElement.Attribute(XmlMetadataSchema.ApplicationCommandRoleAttribute).ValueOrDefault((String)null);
                var stereotype = xElement.Attribute(XmlMetadataSchema.ApplicationCommandStereotypeAttribute).ValueOrDefault((String)null);
                var showExpression = xElement.Attribute(XmlMetadataSchema.BaseDisplayableShowExpressionAtribute).ValueOrDefault((String)null);
                var enableExpression = xElement.Attribute(XmlMetadataSchema.BaseDisplayableEnableExpressionAtribute).ValueOrDefault((String)null);
                var successMessage = xElement.Attribute(XmlMetadataSchema.ApplicationCommandSuccessMessage).ValueOrDefault((String)null);
                var nextSchemaId = xElement.Attribute(XmlMetadataSchema.ApplicationCommandNextSchemaId).ValueOrDefault((String)null);
                var scopeParameters = xElement.Attribute(XmlMetadataSchema.BaseParametersAttribute).ValueOrDefault((String)null);
                var defaultPosition = xElement.Attribute(XmlMetadataSchema.ApplicationCommandDefaultPositionAttribute).ValueOrDefault("left");
                var icon = xElement.Attribute(XmlMetadataSchema.ApplicationCommandIconAttribute).ValueOrDefault((String)null);
                commands.Add(new ApplicationCommand(id, label, service, method, remove, role, stereotype, showExpression, enableExpression, successMessage, nextSchemaId, scopeParameters, defaultPosition, icon));
            }
            return commands;
        }

        /// <summary>
        ///     Deseriliazes the specified XML element to its corresponding
        ///     <seealso cref="CompleteApplicationMetadata"/> representation.
        /// </summary>
        /// <param name="application">The `application` element to be deserialized.</param>
        /// <param name="entityMetadata">The catalog of entity metadata to aid in the application parsing.</param>
        private static CompleteApplicationMetadataDefinition ParseApplication(XElement application, IEnumerable<EntityMetadata> entityMetadata) {
            string guid = application.Attribute(XmlMetadataSchema.ApplicationIdAttribute).ValueOrDefault((string)null);
            var id = guid != null ? Guid.Parse(guid) : (Guid?)null;
            var name = application.Attribute(XmlMetadataSchema.ApplicationNameAttribute).Value;
            var title = application.Attribute(XmlMetadataSchema.ApplicationTitleAttribute).Value;
            var properties = ParseProperties(application);
            var entity = application.Attribute(XmlMetadataSchema.ApplicationEntityAttribute).Value;
            var service = application.Attribute(XmlMetadataSchema.ApplicationServiceAttribute).ValueOrDefault((String)null);
            var metadata = entityMetadata.FirstWithException(e => e.Name == entity, "entity {0} not found", entity);
            var idFieldName = metadata
                .Schema
                .IdAttribute
                .Name;

            return new CompleteApplicationMetadataDefinition(id, name, title, entity, idFieldName, properties, ParseSchemas(name, entity, application, idFieldName), ParseComponents(name, entity, application, idFieldName), service);
        }

        private static IEnumerable<DisplayableComponent> ParseComponents(string name, string entity, XElement application, string idFieldName) {
            IList<DisplayableComponent> resultList = new List<DisplayableComponent>();
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
                Id = component.Attribute(XmlMetadataSchema.BaseDisplayableIdAttribute).Value,
                RealDisplayables = ParseDisplayables(applicationName, component, entityName),
            };
            return resultComponent;
        }

        private static IApplicationDisplayable FindDisplayable(string applicationName, string entityName, XElement xElement, EntityMetadata entityMetadata) {
            var xName = xElement.Name.LocalName;

            if (xName == XmlMetadataSchema.FieldElement) {
                return ParseField(applicationName, xElement, entityMetadata);
            }
            if (xName == XmlMetadataSchema.ApplicationSectionElement) {
                return ParseSection(applicationName, xElement, entityMetadata);
            }
            if (xName == XmlMetadataSchema.ApplicationTabElement) {
                return ParseTab(applicationName, xElement, entityName);
            }
            if (xName == XmlMetadataSchema.ApplicationCompositionElement) {
                return ParseComposition(xElement, applicationName, entityName);
            }
            if (xName == XmlMetadataSchema.ApplicationAssociationElement) {
                return ParseAssociation(xElement, applicationName);
            }
            if (xName == XmlMetadataSchema.OptionFieldElement) {
                return ParseOptions(xElement, applicationName);
            } if (xName == XmlMetadataSchema.ReferenceElement) {
                return ParseReference(xElement);
            }
            return null;
        }

        private static IDictionary<string, string> ParseProperties(XElement xElement) {
            IDictionary<string, string> propertiesDictionary = new Dictionary<string, string>();
            var properties = xElement.Elements().FirstOrDefault(f => f.Name.LocalName == XmlMetadataSchema.ApplicationPropertiesElement);
            if (properties == null) {
                return new Dictionary<string, string>();
            }
            foreach (var property in properties.Elements()) {
                propertiesDictionary.Add(property.Attribute(XmlMetadataSchema.ApplicationPropertyKeyAttribute).Value, property.Attribute(XmlMetadataSchema.ApplicationPropertyValueAttribute).Value);
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

        public XmlApplicationMetadataParser([NotNull] IEnumerable<EntityMetadata> entityMetadata) {
            if (entityMetadata == null) throw new ArgumentNullException("entityMetadata");

            _entityMetadata = entityMetadata;
        }

        /// <summary>
        ///     Parses the XML document provided by the specified
        ///     stream and returns all application metadata.
        /// </summary>
        /// <param name="stream">The input stream containing the XML representation of the metadata file.</param>
        [NotNull]
        public IReadOnlyCollection<CompleteApplicationMetadataDefinition> Parse([NotNull] TextReader stream) {
            if (stream == null) throw new ArgumentNullException("stream");

            var document = XDocument.Load(stream);
            if (null == document.Root) throw new InvalidDataException();

            var applications = document
                .Root.Elements().FirstOrDefault(f => f.Name.LocalName == XmlMetadataSchema.ApplicationsElement);

            if (null == applications) {
                return new ReadOnlyCollection<CompleteApplicationMetadataDefinition>(Enumerable
                    .Empty<CompleteApplicationMetadataDefinition>()
                    .ToList());
            }

            IEnumerable<XElement> applicationElements = applications.Elements().Where(e => e.Name.LocalName == XmlMetadataSchema.ApplicationElement);
            return (from applicationEl in applicationElements
                    select ParseApplication(applicationEl, _entityMetadata)).ToList();
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
