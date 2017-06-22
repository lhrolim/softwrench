using softWrench.sW4.Metadata.Applications.Association;

namespace softWrench.sW4.Data.API.Association {
    public interface IAssociationPrefetcherRequest {

        /// <summary>
        /// 
        /// A comma sepparated list of associations to be pre-fetched, by their relationship keys
        /// 
        /// Defining #all would force all associations to be eagerly loaded, useful under certain scenarios
        /// 
        /// <see cref="AssociationHelper"/>
        /// 
        /// </summary>
        string AssociationsToFetch { get; set; }

        bool IsShowMoreMode { get; set; }
    }
}