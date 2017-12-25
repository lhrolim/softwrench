﻿namespace softWrench.sW4.Metadata.Stereotypes.Schema {
    public class ApplicationSchemaPropertiesCatalog {

        /// <summary>
        /// Marks a schema as a non internal
        /// <seealso cref="MetadataProvider.FetchNonInternalSchemas"/>
        /// </summary>
        public const string NonInternalSchema = "schema.noninternal";

        /// <summary>
        /// Marks a schema as a non internal
        /// </summary>
        public const string SchemaAliasUrl = "schema.aliasurl";

        /// <summary>
        /// Marks a schema as a non internal
        /// </summary>
        public const string AppAliasUrl = "application.aliasurl";


        /// <summary>
        /// Name of the schema to be used on the url /{applicationname}/
        /// </summary>
        public const string MainListSchema = "application.mainlistschema";


        /// <summary>
        /// Name of the schema to be used on the url /{applicationname}/new
        /// </summary>
        public const string MainNewDetailSchema = "application.mainnewdetailschema";

        /// <summary>
        /// Name of the schema to be used on the url /{applicationname}/{userid} and /{applicationname}/uid/{id}
        /// </summary>
        public const string MainDetailSchema = "application.maindetailschema";

        /// <summary>
        /// The schema that should serve as the base for the customization
        /// </summary>
        public const string OriginalSchema = "application.originalschema";

        /// <summary>
        /// The aplication that should serve as the base for the customization
        /// </summary>
        public const string OriginalApplication = "application.original";

        /// <summary>
        /// Overrides the title of a given application especifically for security group purposes
        /// </summary>
        public const string ApplicationSecurityTitle = "application.securitytitle";

        /// <summary>
        /// Whether to force using the original application from a template file (ex: fswokorder which points to workorder but workorder is also redeclared on the medata... avoid using the metadata version)
        /// </summary>
        public const string OriginalApplicationUseTemplate = "application.originalusetemplate";

        /// <summary>
        /// Mark an sw aplication as viewable by users with system admin role
        /// </summary>
        public const string SystemAdminApplication = "application.security.systemadmin";

        /// <summary>
        /// Mark an sw aplication as viewable by users with system admin role
        /// </summary>
        public const string DefaultUserApplication = "application.security.defaultuser";

        /// <summary>
        /// Mark an sw aplication as viewable by users with client admin role
        /// </summary>
        public const string ClientAdminApplication = "application.security.clientadmin";

        /// <summary>
        /// Indicates that a presence of the given role would be enough to allow the user to manipulate the given schema regardless of their application permissions. Useful to account for extra roles from users/security groups
        /// </summary>
        public const string SchemaOverridenRole = "schema.security.overridenrole";

        /// <summary>
        /// Use this property to invoke a custom service upon click on the list page. Value should be on the format serviceName.methodName (ex: changeService.open), 
        /// and have 2 parameters:
        /// 1) datamap of the row 
        /// 2) fieldmetadata of the clicked column
        /// 3) the schema of the grid
        /// </summary>
        public const string ListClickService = "list.click.service";

        /// <summary>
        /// Use this property to define a client side method (column, rowdatamap, schema) which should return an object containing 2 props:
        /// { 
        /// bgcolor, 
        /// fgcolor
        /// }
        /// 
        /// whenever the secondparameter (column) is null, the service will have the oportunity to return the entire row scheme
        /// 
        /// </summary>
        public const string ListColorSchemeService = "list.colorscheme.service";


        /// <summary>
        /// if false the search icon would remain static (true by default)
        /// </summary>
        public const string ListShowSearchIcon = "list.advancedfilter.showsearchicon";

        /// <summary>
        /// if false the search icon would remain static (true by default)
        /// </summary>
        public const string ListHideSearchOptions = "list.advancedfilter.hidesearchoptions";

        /// <summary>
        /// this property accepts single,multiple indicating that either a radiobutton or a checkbox should appear on the screen. If none provided, then there´ll be no selection
        /// </summary>
        public const string ListSelectionStyle = "list.selectionstyle";

        /// <summary>
        /// if true the selection mode will be enabled when the grid is rendered by default
        /// </summary>
        public const string ListSelectionModeByDefault = "list.selectionmodebydefault";

        /// <summary>
        ///
        /// </summary>
        public const string ListClickPopup = "list.click.popup";

        /// <summary>
        /// controls whether to open details in input or output mode
        /// </summary>
        public const string ListClickMode = "list.click.mode";

        /// <summary>
        /// Property to specify the schema navigated to on list click
        /// </summary>
        public const string ListClickSchema = "list.click.schema";
        

        public static string PrintDetailedListSchemaId = "list.print.detailedlistschemaid";
        public static string PrintListSchemaId = "list.print.listschemaid";
        
        /// <summary>
        /// Specify a different order by field to be applied by default to the grid, instead of the entity id
        /// </summary>
        public const string ListSchemaOrderBy = "list.defaultorderby";

        /// <summary>
        /// Comma separated list of fields to be included on quicksearch fot the given schema. If not specified any visible text fields would be included
        /// </summary>
        public const string ListQuickSearchFields = "list.search.quicksearchfields";

        /// <summary>
        /// Comma separated list of fields to be excluded on quicksearch fot the given schema. If not specified any visible text fields would be included
        /// </summary>
        public const string ListQuickSearchFieldsToExclude = "list.search.quicksearchfieldstoexclude";

        /// <summary>
        /// If true the quick search does not requires a enter or click to start a search.
        /// </summary>
        public const string ListAutoQuickSearch = "list.quicksearch.auto";

        /// <summary>
        /// Comma separated list of compositions that should not be included in the list of available search options
        /// </summary>
        public const string ListQuickSearchCompositionsToExclude = "list.search.compositionstoexclude";

       

        /// <summary>
        /// Comma separated list of text fields that should be used as index on offline search.
        /// </summary>
        public const string ListOfflineTextIndexes = "list.offline.text.indexlist";

        /// <summary>
        /// Comma separated list of numeric fields that should be used as index on offline search.
        /// </summary>
        public const string ListOfflineNumericIndexes = "list.offline.numeric.indexlist";

        /// <summary>
        /// Comma separated list of date fields that should be used as index on offline search.
        /// </summary>
        public const string ListOfflineDateIndexes = "list.offline.date.indexlist";

        /// <summary>
        ///  property for specifying the default size of the pagination
        /// </summary>
        public const string DefaultPaginationSize = "list.paginationsize";

        /// <summary>
        /// If true, the grid will render all the entries and the pagination will be entirely removed
        /// </summary>
        public const string DisablePagination = "list.disablepagination";

        /// <summary>
        /// If true, all the filters will be disabled
        /// </summary>
        public const string DisableFilter = "list.disablefilter";
        /// <summary>
        /// 1) server --> the default if not declared. Sorting will be performed on server side
        /// 2) client --> the sort will be done on the client side, just on current page
        /// 3) none --> not allowed
        /// </summary>
        public const string DisableSort = "list.sortmode";

        /// <summary>
        /// If true, we will disable the detail selection of the grid
        /// </summary>
        public const string DisableDetails = "list.disabledetails";

        /// <summary>
        /// property for specifying the options of the pagination
        /// </summary>
        public const string PaginationOptions = "list.paginationoptions";

        /// <summary>
        /// Destination schema of the new button of empty list.
        /// </summary>
        public const string NoResultsNewSchema = "list.noresultsnewschema";

        /// <summary>
        /// Destination schema of the new button of empty list.
        /// </summary>
        public const string NewSchema = "list.newschema";

        /// <summary>
        /// Function to be executed before the create new page to be opened from no results button.
        /// Can return a datamap to be pre filled on the create new page.
        /// </summary>
        public const string NoResultsPreAction = "list.noresultspreaction";

        /// <summary>
        /// Property to prevent the adition of the new button of empty list.
        /// </summary>
        public const string PreventNoResultsNew = "list.preventnoresultsnew";



        /// <summary>
        /// property to specify what´s the next schema that should be routed from the current schema
        /// </summary>
        public const string RoutingNextSchemaId = "nextschema.schemaid";

        /// <summary>
        /// property to specify what´s the next schema that should be routed from the current schema
        /// </summary>
        public const string RoutingNextApplication = "routing.nextapplication";

        /// <summary>
        /// If true forces the sync of an application as association data.
        /// </summary>
        public const string OfflineForceAssocSync = "offline.force.assoc.sync";


        /// <summary>
        /// If true forces the sync of an application as an application data.
        /// </summary>
        public const string OfflineForceSync = "offline.force.sync";

        /// <summary>
        /// Use this property to specify a custom controller/action to redirect after the save has been performed.Value should be on the format serviceName.methodName
        /// </summary>
        public const string AfterSubmitAction = "aftersubmit.redirectaction";

        /// <summary>
        /// Use this property to specify a custom javascript validation to be called whenever the form is about to be submitted
        /// </summary>
        public const string OnCrudSaveEventValidationService = "beforesubmit.onvalidation";

        /// <summary>
        /// Set this property to true to allow the framework to pass not only the selected values, but also the labels of it on the json unmapedattributes. 
        /// These can be fetched using (#+attributename + "_label") ==> xxx becomes "#xxx_label"
        /// </summary>
        public const string AddAssociationLables = "addassociationlabels";

     


        /// <summary>
        ///  Comma seppareted list of associations to be prefetched,on the same request to the server to get the details itself, regardless of their oringal lazyness nature.
        /// </summary>
        public const string PreFetchAssociations = "associationstoprefetch";

        /// <summary>
        ///  if true, the compositions will be prefetched on the same request to the server to get the details itself.
        ///  This is a optimization property, that should be marked only if the schema has very few associations, and they are not heavy
        /// </summary>
        public const string PreFetchCompositions = "prefetchcompositions";

        /// <summary>
        ///  property for specifying the success message time out
        /// </summary>
        public const string SuccessMessageTimeOut = "successmessagetimeout";

        /// <summary>
        /// Defines strategy on how to render the popup titltes
        /// </summary>
        public static string WindowPopupTitleStrategy = "popup.window.titlestrategy";
        /// <summary>
        /// whether to show or not the print command
        /// </summary>
        public static string ShowPrintCommandStrategy = "show.printcommand";

        /// <summary>
        /// Describes the icon that will appear on the compoisition tab
        /// </summary>
        public static string CompositionTabIcon = "icon.composition.tab";

        /// <summary>
        /// Describes the icon that will appear on the compoisition add button (add worklog)
        /// </summary>
        public static string CompositionAddIcon = "icon.composition.addbutton";

        /// <summary>
        /// By default the cancel click on a detail page will try to locate a list schema for the given application. Use this property when you need to overload it
        /// </summary>
        public static string CancelDetailSchema = "detail.cancel.click";

        /// <summary>
        /// A title expression to use to display the title on the crud_body.js. this would replace completely the original title. Use the same syntax as other expressions
        /// </summary>
        public static string DetailTitleExpression = "detail.titleexpression";

        /// <summary>
        /// The string to use as the title id (first part of crud_body.html title) 
        /// </summary>
        public static string DetailTitleId = "detail.titleid";


        /// <summary>
        /// If true, the title of the detail schema will be shown
        /// </summary>
        public static string DetailShowTitle = "detail.showtitle";

        /// <summary>
        /// If true, the title of the detail schema will be hidden
        /// </summary>
        public static string DetailPreFetchCompositions = "detail.prefetchcompositions";

        /// <summary>
        /// If present:
        ///    - Only the given fields and options (attributes separated by comma) will not be read only.
        ///    - Only the given associations(targets separated by comma) will be enabled.
        ///    - Only the given compositions(relationships separated by comma) will allow insertions e updates.
        ///    - Only the given references (ids separated by comma) will not be read only.
        /// </summary>
        public static string DetailEnabledFields = "detail.enabledfields";

        /// <summary>
        /// If "true" alows the focus on form to move backward.
        /// </summary>
        public static string DetailFocusAllowMoveBackward = "detail.focus.allowmovingbackward";

        /// <summary>
        /// Allows schema id to be used on printinfg detail.
        /// </summary>
        public static string PrintDetailSchemaId = "detail.print.schemaid";

        /// <summary>
        /// If "true" show detail toolbar even to anonymous user.
        /// </summary>
        public static string DetailShowToolbarToAnonymous = "detail.show.toolbar.anonymous";

        /// <summary>
        /// If "true" reloads de current schema kepping filters, page and ordering.
        /// </summary>
        public static string CompositionSoftRefresh = "compositions.keepfilters";

        /// <summary>
        /// If "true" disables the filter of a list schema used on composition.
        /// </summary>
        public static string CompositionDisableFilter = "compositions.disable.filter";

        /// <summary>
        /// If true all options fields of this schema will be rendered with no initial selection
        /// </summary>
        public static string DoNotUseFirstOptionAsDefault = "optionfield.donotusefirstoptionasdefault";

        /// <summary>
        /// Type of the bottom command bar. For now only works for the detail stereotype.
        /// </summary>
        public static string CommandBarBottom = "commandbar.bottom";

        /// <summary>
        /// The search schema id of current schema.
        /// </summary>
        public static string SearchSchemaId = "search.schemaid";

     

        /// <summary>
        /// The the target schema id of current  search schema. If not preent "list" will be used
        /// </summary>
        public static string SearchTargetSchemaId = "search.target.schemaid";

        /// <summary>
        /// If "true" the crud search side panel will start expanded.
        /// </summary>
        public static string SearchStartExpanded = "search.startexpanded";

        /// <summary>
        /// The width of the search panel handle.
        /// </summary>
        public static string SearchHandleWidthExpanded = "search.panelwidth";

        /// <summary>
        /// Used to specify a service.function() on a schema that is currently called when changing pages in pagination to add custom parameters to the call.
        /// </summary>
        public static string SchemaCustomParamProvider = "schema.customparamprovider";

        /// <summary>
        /// Comma separated list of entities which should be considered as related to a given application on whereclauses.
        /// By default any associations, plus the main entity would already bee included
        /// 
        /// <see cref="MetadataProvider.FetchAvailableAppsAndEntities"/>
        /// </summary>
        public static string SchemaRelatedEntities = "schema.whereclause.related";
        
    }
}
