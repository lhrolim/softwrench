using System.Collections.Generic;
using JetBrains.Annotations;
using softwrench.sw4.Shared2.Data.Association;

namespace softWrench.sW4.Data.API.Association.SchemaLoading {

    /// <summary>
    /// This class holds the data structure to return when a given schema has been loaded on the server side.
    /// </summary>
    public abstract class BaseAssociationSchemaLoadResult<TEager> {

        //pre instantiating to avoid null pointers
        private IDictionary<string, IDictionary<string, IAssociationOption>> _preFetchLazyOptions = new Dictionary<string, IDictionary<string, IAssociationOption>>();
        private IDictionary<string, TEager> _eagerOptions = new Dictionary<string, TEager>();


        /// <summary>
        /// Lists of eager collections, containing n options here.
        /// 
        /// The type of the TEager can vary across the different implementations, since theoritically,
        /// compositions can have different option lists for each of their rows
        /// 
        /// </summary>
        [NotNull]
        public IDictionary<string, TEager> EagerOptions {
            get { return _eagerOptions; }
            set { _eagerOptions = value; }
        }


        /// <summary>
        /// List for lazy load colletions, such as lookups. 
        /// This will contain, basically the descriptions of the underlying values selected on the mainEntity, 
        /// but could also have related extra projection fields
        /// 
        /// (e.g --> what´s the description of that asset defined on the assetnum of a given wo?)
        /// 
        /// Ex:
        /// 
        ///  _asset:{
        ///  "1": {code:'1',description:'Description of Asset of Id 1'}
        ///  "2": {code:'1',description:'Description of Asset of Id 1'}
        ///  },
        ///  
        /// _owner: {
        /// "1": {code:'1',description:'Description of Owner of Id 1'}
        /// },
        /// 
        /// .
        /// .
        /// .
        /// 
        /// </summary>
        [NotNull]
        public virtual IDictionary<string, IDictionary<string, IAssociationOption>> PreFetchLazyOptions {
            get { return _preFetchLazyOptions; }
            set { _preFetchLazyOptions = value; }
        }
    }




}
