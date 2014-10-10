namespace softWrench.sW4.Metadata.Stereotypes.Schema {
    public class ApplicationSchemaPropertiesCatalog {
        /// <summary>
        /// Use this property to invoke a custom service upon click on the list page. Value should be on the format serviceName.methodName (ex: changeService.open), 
        /// and have 2 parameters:
        /// 1) datamap of the row 
        /// 2) fieldmetadata of the clicked column
        /// </summary>
        public const string ListClickService = "list.click.service";

        /// <summary>
        /// if false the search icon would remain static (true by default)
        /// </summary>
        public const string ListShowSearchIcon = "list.advancedfilter.showsearchicon";


        /// <summary>
        ///
        /// </summary>
        public const string ListClickPopup = "list.click.popup";

        /// <summary>
        /// Specify a different order by field to be applied by default to the grid, instead of the entity id
        /// </summary>
        public const string ListSchemaOrderBy = "list.defaultorderby";

        /// <summary>
        /// Use this property to specify a custom controller/action to redirect after the save has been performed.Value should be on the format serviceName.methodName
        /// </summary>
        public const string OnCrudSaveEventAction = "oncrudsaveevent.redirectaction";

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
