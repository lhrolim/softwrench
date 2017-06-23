namespace softwrench.sW4.Shared2.Metadata.Applications.Relationships.Associations {

    public class AssociationDataProvider {

        public string PreFilterFunctionName { get; set; }
        public string PostFilterFunctionName { get; set; }

        public string WhereClause { get; set; }
        public string MetadataId { get; set; }

        public AssociationDataProvider(string prefilterFunctionName, string postFilterFunctionName, string whereClause, string metadataId) {
            PreFilterFunctionName = prefilterFunctionName;
            PostFilterFunctionName = postFilterFunctionName;
            WhereClause = whereClause;
            MetadataId = metadataId;
        }

        public AssociationDataProvider() { }

        public override string ToString() {
            return string.Format("PreFilterFunctionName: {0}, PostFilterFunctionName: {1}, WhereClause: {2}", PreFilterFunctionName, PostFilterFunctionName, WhereClause);
        }
    }
}
