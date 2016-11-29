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
        public const string ModalFilterElement = "modalfilter";
        public const string NumericFilterElement = "numericfilter";
        public const string DateFilterElement = "datefilter";
        public const string BaseFilterElement = "filter";

        public const string OptionElement = "option";

        public const string StyleAttribute = "style";
        public const string WhereClauseAttribute = "whereclause";
        public const string ProviderAttribute = "provider";
        public const string AllowBlankAttribute = "allowblank";
        public const string EagerAttribute = "eager";
        public const string DefaultSelectionAttribute = "defaultselection";
        public const string TrueLabelAttribute = "truelabel";
        public const string TrueValueAttribute = "truevalue";
        public const string FalseLabelAttribute = "falselabel";
        public const string FalseValueAttribute = "falsevalue";
        public const string RemoveAttribute = "remove";
        public const string TargetSchemaAttribute = "targetschema";
        public const string AdvancedFilterSchemaAttribute = "advancedfilterschema";
        public const string AllowFutureAttribute = "allowfuture";
        public const string DateOnlyAttribute = "dateonly";

        public const string FilterNamespace = "http://www.controltechnologysolutions.com/filters";
    }
}