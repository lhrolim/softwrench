using System.Xml.Linq;

namespace softWrench.sW4.Metadata.Parsing {
    /// <summary>
    ///     A utility class to register all XML schema
    ///     constants related to the metadata file
    ///     (e.g. elements names, attributes names).
    /// </summary>
    internal class XmlFilterSchema :XmlBaseSchemaConstants
    {

        public const string FiltersElement = "filters";
        public const string OptionFilterElement = "optionfilter";
        public const string BooleanFilterElement = "booleanfilter";
        public const string BaseFilterElement = "filter";

        public const string OptionElement = "option";

        public const string StyleAttribute = "style";
        public const string WhereClauseAttribute = "whereclause";
        public const string ProviderAttribute = "provider";
        public const string AllowBlankAttribute = "allowblank";
        public const string DisplayCodeAttribute = "displaycode";
        public const string EagerAttribute = "eager";
        public const string DefaultSelectionAttribute = "defaultselection";
        public const string RemoveAttribute = "remove";

        public const string FilterNamespace = "http://www.controltechnologysolutions.com/filters";
    }
}