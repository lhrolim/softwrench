namespace softwrench.sw4.Shared2.Metadata.Applications.Filter {

    public class MetadataDateTimeFilter : BaseMetadataFilter {

        public bool AllowFuture {
            get; set;
        }

        public bool DateOnly {
            get; set;
        }


        public MetadataDateTimeFilter(string attribute, string label, string icon, string position, string tooltip, string whereClause, bool allowFuture, bool dateOnly) : base(attribute, label, icon, position, tooltip, whereClause) {
            AllowFuture = allowFuture;
            DateOnly = dateOnly;
        }
    }
}
