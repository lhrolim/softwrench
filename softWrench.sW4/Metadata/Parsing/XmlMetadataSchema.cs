using System.Xml.Linq;

namespace softWrench.sW4.Metadata.Parsing {
    /// <summary>
    ///     A utility class to register all XML schema
    ///     constants related to the metadata file
    ///     (e.g. elements names, attributes names).
    /// </summary>
    internal class XmlMetadataSchema : XmlBaseSchemaConstants {
        public static readonly char[] ItemDelimeter = { ',' };






        public const string EventsElement = "event";
        public const string EventsTypeAttribute = "type";
        public const string EventsServiceAttribute = "service";
        public const string EventsMethodAttribute = "method";
        public const string EventsExpressionAttribute = "expression";


        public const string EnvironmentElement = "environment";
        public const string GlobalPropertiesElement = "globalproperties";
        public const string PropertyElement = "property";
        public const string PropertyKeyAttribute = "key";
        public const string PropertyValueAttribute = "value";

        public const string QueriesElement = "queries";
        public const string QueryElement = "query";



        public const string TemplatesElement = "templates";
        public const string TemplateElement = "template";
        public const string TemplatePathAttribute = "path";

        public const string EntitiesElement = "entities";
        public const string EntityElement = "entity";
        public const string EntityAttributeId = "id";
        public const string EntityAttributeName = "name";
        public const string EntityAttributeIdAttribute = "idAttribute";
        public const string EntityAttributeUserIdAttribute = "useridAttribute";
        public const string EntityAttributeWhereClause = "whereclause";
        public const string EntityAttributeParentEntity = "parententity";

        public const string SchemaElement = "schema";
        public const string SchemasElement = "schemas";
        public const string SchemaIdAttribute = "id";
        public const string SchemaRedeclaringSchemaAttribute = "redeclaring";
        public const string SchemaPlatformAttribute = "platform";
        public const string SchemaStereotypeAttribute = "stereotype";
        public const string StereotypesAttribute = "stereotypes";
        public const string SchemaAbstractAttribute = "abstract";
        public const string SchemaParentSchemaAttribute = "parentschema";
        public const string SchemaUnionSchemaAttribute = "unionschema";
        public const string SchemaModeAttribute = "mode";
        public const string SchemaTitleAttribute = "title";



        public const string AttributesElement = "attributes";
        public const string TargetPathAttribute = "targetpath";
        public const string ExcludeUndeclared = "excludeundeclared";




        public const string AttributeElement = "attribute";
        public const string AttributeAttributeName = "name";
        public const string AttributeAttributeLabel = "label";
        public const string AttributeAttributeType = "type";

        public const string RelationshipsElement = "relationships";
        public const string RelationshipElement = "relationship";
        public const string RelationshipAttributeQualifier = "qualifier";
        public const string RelationshipAttributeTo = "to";
        public const string RelationshipAttributeToAttribute = "attribute";
        public const string RelationshipAttributePrimary = "primary";
        public const string RelationshipAttributeIncludeOnSync = "includeonsync";
        public const string RelationshipAttributeCollection = "collection";
        public const string RelationshipCacheableAttribute = "cacheable";
        public const string RelationshipLazyAttribute = "lazy";
        public const string RelationshipAttributeReverse = "reverselookupattribute";
        public const string IgnorePrimaryAttribute = "ignoreprimary";
        public const string InnerJoinAttribute = "innerjoin";
        public const string RelationshipAttributeElement = "relationshipAttribute";
        public const string RelationshipAttributeAttributeTo = "to";
        public const string RelationshipAttributeAttributeFrom = "from";
        public const string RelationshipAttributeAttributeAllowsNull = "allowsnull";
        public const string RelationshipAttributeAttributeLiteral = "literal";
        public const string RelationshipAttributeAttributeQuoteLiteral = "quoteLiteral";

        //<association label="Location" value="location" labelfield="location.description" />
        public const string ApplicationAssociationElement = "association";
        public const string ApplicationAssociationDetailsElement = "details";
        public const string ApplicationSectionElement = "section";
        public const string ApplicationAssociationLabelAttribute = "label";
        public const string ApplicationAssociationExtraProjectionFieldsAttribute = "extraprojectionvalues";
        public const string ApplicationAssociationDependantFieldsAttribute = "dependantfields";
        public const string ApplicationAssociationLabelFieldAttribute = "labelfield";
        public const string ApplicationAssociationLabelPatternAttribute = "labelpattern";
        public const string ApplicationAssociationTargetAttribute = "target";
        public const string ApplicationAssociationDefaultValueAttribute = "default";
        public const string ApplicationAssociationAssociationAttributeElement = "associationAttribute";
        public const string ApplicationAssociationHideDescription = "hideDescription";
        public const string ApplicationAssociationOrderByField = "orderbyfield";
        public const string ApplicationAssociationForceDistinctOptions = "forcedistinctoptions";
        public const string ApplicationAssociationValueField = "valuefield";

        public const string DataProviderElement = "dataprovider";
        public const string DataProviderWhereClause = "whereClause";
        public const string DataProviderPreFilterFunction = "prefilterfunction";
        public const string DataProviderMetadataId = "metadataid";
        public const string DataProviderPostFilterFunction = "postfilterfunction";

        //           <composition attribute="worklog" label="Worklogs" >
        //            <field attribute="description" label="Description"/>
        //            <field attribute="createby"   label="Created By"   readOnly="true"/>
        //            <field attribute="createdate" label="Created On" readOnly="true"/>
        //          </composition>
        public const string ApplicationCompositionElement = "composition";
        public const string ApplicationCompositionRelationshipAttribute = "relationship";
        public const string ApplicationCompositionLabelAttribute = "label";
        public const string ApplicationCompositionInlineAttribute = "inline";
        public const string ApplicationCompositionRenderModeAttribute = "rendermode";
        public const string ApplicationCompositionSchemaIdAttribute = "detailschema";
        public const string ApplicationCompositionOutputSchemaIdAttribute = "outputschema";
        public const string ApplicationCompositionPrintAttribute = "printschema";
        public const string ApplicationCompositionFetchType = "fetchtype";
        public const string ApplicationCompositionDependantFieldsAttribute = "dependantfields";
        public const string ApplicationCompositionRequiredRelationshipAttribute = "requiredrelationshipexpression";

        public const string ApplicationCompositionCollectionPropertiesElement = "collectionproperties";

        public const string ApplicationCompositionCollectionAllowInsertionAttribute = "allowcreation";
        public const string ApplicationCompositionCollectionAllowUpdateAttribute = "allowupdate";
        public const string ApplicationCompositionCollectionAllowRemovalAttribute = "allowremoval";

        public const string ApplicationCompositionCollectionListSchemaAttribute = "listschema";
        public const string ApplicationCompositionCollectionAutoCommitAttribute = "autocommit";
        public const string ApplicationCompositionCollectionHideExistingDataAttribute = "hideexistingdata";
        public const string ApplicationCompositionCollectionOrderByField = "orderbyfield";
        public const string ApplicationCompositionCollectionPreFilterFunctionField = "prefilterfunction";


        public const string ApplicationCommandElement = "command";
        public const string ApplicationCommandRemoveUndeclaredAttribute = "removeundeclared";
        public const string ApplicationCommandIdAttribute = "id";
        public const string ApplicationCommandLabelAttribute = "label";
        public const string ApplicationCommandRoleAttribute = "role";
        public const string ApplicationCommandStereotypeAttribute = "stereotype";
        public const string ApplicationCommandDefaultPositionAttribute = "defaultposition";
        public const string ApplicationCommandClientResourceAttribute = "clientresourcepath";
        public const string ApplicationCommandIconAttribute = "icon";
        public const string ApplicationCommandClientFunctionAttribute = "clientfunction";
        public const string ApplicationCommandRemoveAttribute = "remove";
        public const string ApplicationCommandSuccessMessage = "successmessage";
        public const string ApplicationCommandNextSchemaId = "nextSchemaId";


        public const string ConnectorParametersElement = "connectorParameters";
        public const string ConnectorParameterElement = "connectorParameter";
        public const string ConnectorParameterAttributeKey = "key";
        public const string ConnectorParameterAttributeValue = "value";

        public static string AttributeAttributeAutoGenerated = "auto-generated";
        public static string AttributeQuery = "query";
        public const string ApplicationsElement = "applications";
        public const string ApplicationElement = "application";
        public const string ApplicationIdAttribute = "id";
        public const string ApplicationDataSetAttribute = "dataset";
        public const string ApplicationNameAttribute = "name";
        public const string ApplicationTitleAttribute = "title";
        public const string ApplicationAuditFlagAttribute = "audit";
        public const string ApplicationFetchLimitAttribute = "fetchlimit";
        public const string ApplicationEntityMirrorAttribute = "entitymirror";
        public const string ApplicationEntityAttribute = "entity";
        public const string ApplicationServiceAttribute = "service";
        public const string ApplicationToStringAttribute = "tostring";
        public const string ApplicationSchemaAttributeUserInteractionEnabled = "userInteractionEnabled";

        public const string ApplicationPropertiesElement = "properties";
        public const string ApplicationPropertyElement = "property";
        public const string ApplicationPropertyKeyAttribute = "key";
        public const string ApplicationPropertyValueAttribute = "value";

        public const string ApplicationSectionAbstractAttribute = "abstract";
        public const string ApplicationSectionIdAttribute = "id";
        public const string ApplicationSectionResourcePathAttribute = "resourcepath";
        public const string ApplicationSectionParametersAttribute = "parameters";
        public const string ApplicationSectionLabelAttribute = "label";
        public const string ApplicationSectionAttributeAttribute = "attribute";
        public const string ApplicationSectionOrientationAttribute = "orientation";
        public const string ApplicationSectionRoleAttribute = "role";
        public const string ApplicationSectionSecondaryContentAttribute = "secondarycontent";

        public const string ApplicationTabElement = "tab";

        public const string ApplicationHeaderElement = "header";
        public const string ApplicationHeaderLabelAttribute = "label";
        public const string ApplicationHeaderParametersAttribute = "params";
        public const string ApplicationHeaderDisplacementAttribute = "displacement";

        public const string DetailElement = "detail";
        public const string ListElement = "list";
        public const string FieldElement = "field";

        public const string FieldAttributeAttribute = "attribute";
        public const string FieldAttributeLabel = "label";
        public const string FieldAttributeReadOnly = "readonly";
        public const string FieldAttributeHidden = "hidden";
        public const string FieldAttributeQualifier = "qualifier";
        public const string FieldAttributeAttributeToServer = "attributeToServer";

        public const string CustomizationElement = "customization";
        public const string CustomizationPositionAttribute = "position";


        public const string ComponentsElement = "components";
        public const string ComponentElement = "componentdisplayable";



        public static XName FieldAttributeDefaultValue = "default";
        public const string FieldAttributeDefaultExpression = "defaultexpression";
        //        public const string FieldAttributeOptions = "options";
        public const string FieldAttributeType = "type";

        public const string WebElement = "web";
        public const string WebListElement = "list";
        public const string WebColumnElement = "column";
        public const string WebColumnAttributeAttribute = "attribute";
        public const string WebColumnAttributeLabel = "label";

        public const string MobileElement = "mobile";
        public const string MobileFetchLimitAttribute = "fetchLimit";
        public const string MobilePreviewElement = "preview";
        public const string MobilePreviewTitle = "title";
        public const string MobilePreviewSubtitle = "subtitle";
        public const string MobilePreviewFeatured = "featured";
        public const string MobilePreviewExcerpt = "excerpt";
        public const string MobilePreviewAttributeAttribute = "attribute";
        public const string MobilePreviewAttributeLabel = "label";

        public const string RendererElement = "renderer";
        public const string RendererAttributeType = "type";
        public const string RendererAttributeDataProvider = "dataprovider";
        public const string RendererAttributeParams = "params";
        public const string RendererAttributeStereotype = "stereotype";

        public const string FilterElement = "filter";
        public const string FilterOperationType = "operation";
        public const string FilterAttributeDefault = "default";
        public const string FilterAttributeParams = "params";

        public const string WidgetHiddenElement = "hidden";
        public const string WidgetNumberElement = "number";
        public const string WidgetNumberDecimalsAttribute = "decimals";
        public const string WidgetNumberMinAttribute = "min";
        public const string WidgetNumberMaxAttribute = "max";
        public const string WidgetDateElement = "date";
        public const string WidgetDateFormatAttribute = "format";
        public const string WidgetDateTimeAttribute = "time";
        public const string WidgetDateMinAttribute = "min";
        public const string WidgetDateMaxAttribute = "max";
        public const string WidgetLookupElement = "lookup";
        public const string WidgetLookupSourceApplicationAttribute = "sourceApplication";
        public const string WidgetLookupSourceFieldAttribute = "sourceField";
        public const string WidgetLookupSourceDisplayAttribute = "sourceDisplay";
        public const string WidgetLookupTargetFieldAttribute = "targetField";
        public const string WidgetLookupTargetQualifierAttribute = "targetQualifier";
        public const string WidgetLookupFiltersElement = "lookupFilters";
        public const string WidgetLookupFilterElement = "lookupFilter";
        public const string WidgetLookupFilterAttributeSourceField = "sourceField";
        public const string WidgetLookupFilterAttributeTargetField = "targetField";
        public const string WidgetLookupFilterAttributeLiteral = "literal";


        public const string OptionFieldElement = "optionfield";
        public const string OptionFieldSortAttribute = "sort";
        public const string OptionFieldProviderAttribute = "providerattribute";
        public const string OptionFieldSkipValidationAttribute = "skipvalidation";
        public const string OptionFieldProviderAttributeExtraParameter = "extraparameter";
        public const string OptionElement = "option";
        public const string OptionElementValueAttribute = "value";
        public const string OptionElementLabelAttribute = "label";
        public const string OptionElementExtraProjection = "extraprojections";
        public const string OptionElementHelp = "help";

        public const string ReferenceElement = "reference";

        public const string SecurityElement = "security";
        public const string RefAttribute = "ref";
        public const string RemoveApplicationElement = "removeapplication";
        public const string RolesElement = "roles";
        public const string RoleElement = "role";
        public const string RoleIdAttribute = "id";
        public const string RoleNameAttribute = "name";
        public const string PermissionsElement = "permissions";
        public const string AllowElement = "allow";
        public const string AllowApplicationAttribute = "application";
        public const string DenyElement = "deny";
        public const string DenyFieldAttribute = "field";
        public const string UsersElement = "users";
        public const string UserElement = "user";
        public const string UserIdAttribute = "id";
        public const string UserLoginAttribute = "login";
        public const string MembershipElement = "membership";
        public const string MemberElement = "member";

        public static string ConstElement = "const";

        
    }
}