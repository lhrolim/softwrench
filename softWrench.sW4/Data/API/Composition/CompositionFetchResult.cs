﻿
using System.Collections.Generic;
using Newtonsoft.Json;
using softWrench.sW4.Data.API.Response;
using softWrench.sW4.Data.Entities;
using softWrench.sW4.Data.Persistence.Operation;
using softWrench.sW4.Data.Persistence.Relational.EntityRepository;
using softwrench.sW4.Shared2.Data;

namespace softWrench.sW4.Data.API.Composition {
    public class CompositionFetchResult : GenericResponseResult<Dictionary<string, EntityRepository.SearchEntityResult>> {

        /// <summary>
        /// To use on the server side, this won´t be propagated back to the client side. Use ModifiedMap instead. 
        /// The reason is that returning the whole json that was passed is an overhead, 
        /// and any new data to the parent entry shall be passed on a custom and controlled basis
        /// </summary>
        [JsonIgnore]
        public Entity OriginalCruddata { get; set; }

        /// <summary>
        /// Use this dictionary to fill any fields that should been modified on the parent datamap
        /// </summary>
        public IDictionary<string, object> ParentModifiedFields { get; set; }

        public CompositionFetchResult(Dictionary<string, EntityRepository.SearchEntityResult> compositions, Entity originalCruddata)
            : base(compositions) {
            OriginalCruddata = originalCruddata;
            ParentModifiedFields = new Dictionary<string, object>();
        }

    }
}
