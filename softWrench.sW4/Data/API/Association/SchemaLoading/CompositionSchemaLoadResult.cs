using System.Collections.Generic;
using Newtonsoft.Json;
using softwrench.sw4.Shared2.Data.Association;

namespace softWrench.sW4.Data.API.Association.SchemaLoading {

    /// <summary>
    /// Association result related to composition schemas.
    /// 
    /// The type of the eager descriptions is a code indexed dictionary of AssociationOptions,
    /// since each composition row could have one description for a given association, making it possible to return them all on a single operation.
    /// 
    /// Global should be used to indicate shared lists, which should be the most common scenario.
    /// 
    /// E.G:
    /// 
    /// _asset:{
    ///   
    ///  datamap1: [] // assets available only for datamap1,
    ///  datamap2: [] // assets available for datamap2,
    ///  
    /// },
    /// owner:{
    ///  #global: [] --> //shared eager owners to all the composition rows
    /// }
    /// 
    /// 
    /// </summary>
    public class CompositionSchemaLoadResult : BaseAssociationSchemaLoadResult<IDictionary<string, IEnumerable<IAssociationOption>>> {

        public string SchemaIdentifier { get; set; }

        
    }
}
