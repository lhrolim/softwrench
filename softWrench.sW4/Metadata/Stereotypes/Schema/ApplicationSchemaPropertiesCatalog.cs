namespace softWrench.sW4.Metadata.Stereotypes.Schema {
    public class ApplicationSchemaPropertiesCatalog {
        /// <summary>
        /// Use this property to invoke a custom service upon click on the list page. Value should be on the format serviceName.methodName (ex: changeService.open), 
        /// and have 2 parameters:
        /// 1) datamap of the row 
        /// 2) fieldmetadata of the clicked column
        /// 3) the schema of the grid
        /// </summary>
        public const string ListClickService = "list.click.service";

        /// <summary>
        /// if false the search icon would remain static (true by default)
        /// </summary>
        public const string ListShowSearchIcon = "list.advancedfilter.showsearchicon";

        /// <summary>
        /// this property accepts single,multiple indicating that either a radiobutton or a checkbox should appear on the screen. If none provided, then there´ll be no selection
        /// </summary>
        public const string ListSelectionStyle = "list.selectionstyle";

        /// <summary>
        /// if true the selection mode will be enabled when the grid is rendered by default
        /// </summary>
        public const string ListSelectionModeByDefault = "list.selectionmodebydefault";

        /// <summary>
        /// property to specify what´s the next schema that should be routed from the current schema
        /// </summary>
        public const string RoutingNextSchemaId = "nextschema.schemaid";

        /// <summary>
        /// property to specify what´s the next schema that should be routed from the current schema
        /// </summary>
        public const string RoutingNextApplication = "routing.nextapplication";

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

        /// <summary>
        /// Specify a different order by field to be applied by default to the grid, instead of the entity id
        /// </summary>
        public const string ListSchemaOrderBy = "list.defaultorderby";

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
        ///  property for specifying the options of the pagination
        /// </summary>
        public const string PaginationOptions = "list.paginationoptions";

        /// <summary>
        ///  Destination schema of the new button of empty list.
        /// </summary>
        public const string NoResultsNewSchema = "list.noresultsnewschema";

        /// <summary>
        ///  Property to prevent the adition of the new button of empty list.
        /// </summary>
        public const string PreventNoResultsNew = "list.preventnoresultsnew";



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
        /// If true all options fields of this schema will be rendered with no initial selection
        /// </summary>
        public static string DoNotUseFirstOptionAsDefault = "optionfield.donotusefirstoptionasdefault";

        /// <summary>
        /// Type of the bottom command bar. For now only works for the detail stereotype.
        /// </summary>
        public static string CommandBarBottom = "commandbar.bottom";
    }
}
