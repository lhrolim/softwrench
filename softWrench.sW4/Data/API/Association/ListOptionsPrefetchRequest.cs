namespace softWrench.sW4.Data.API.Association {
    /// <summary>
    /// This class should be used for optionfields inside of a list schema
    /// </summary>
    internal class ListOptionsPrefetchRequest : IAssociationPrefetcherRequest {
        public string AssociationsToFetch { get; set; }
        public bool IsShowMoreMode { get; set; }
    }
}
