namespace softWrench.sW4.Data.API.Association {
    public interface IAssociationPrefetcherRequest {

        string AssociationsToFetch { get; set; }

        bool IsShowMoreMode { get; set; }
    }
}