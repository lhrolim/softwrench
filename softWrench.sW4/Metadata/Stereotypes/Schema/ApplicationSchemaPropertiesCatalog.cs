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
        ///
        /// </summary>
        public const string ListClickPopup = "list.click.popup";

        /// <summary>
        /// controls whether to open details in input or output mode
        /// </summary>
        public const string ListClickMode = "list.click.mode";

        /// <summary>
        /// Specify a different order by field to be applied by default to the grid, instead of the entity id
        /// </summary>
        public const string ListSchemaOrderBy = "list.defaultorderby";

        /// <summary>
        /// Use this property to specify a custom controller/action to redirect after the save has been performed.Value should be on the format serviceName.methodName
        /// </summary>
        public const string OnCrudSaveEventAction = "oncrudsaveevent.action";

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
        ///  if true, the associations will be prefetched on the same request to the server to get the details itself.
        ///  This is a optimization property, that should be marked only if the schema has very few associations, and they are not heavy
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
    }
}
