namespace softwrench.sW4.Shared.Metadata.Applications.Relationships.Associations {

    public class AssociationDataProvider {

        public string PreFilterFunctionName { get; set; }
        public string PostFilterFunctionName { get; set; }

        public string WhereClause { get; set; }

        public AssociationDataProvider(string prefilterFunctionName, string postFilterFunctionName, string whereClause) {
            PreFilterFunctionName = prefilterFunctionName;
            PostFilterFunctionName = postFilterFunctionName;
            WhereClause = whereClause;
        }

        public AssociationDataProvider() { }

        public override string ToString() {
            return string.Format("PreFilterFunctionName: {0}, PostFilterFunctionName: {1}, WhereClause: {2}", PreFilterFunctionName, PostFilterFunctionName, WhereClause);
        }
    }
}
