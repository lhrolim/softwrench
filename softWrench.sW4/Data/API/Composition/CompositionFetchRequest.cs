
using System;
using System.Collections.Generic;
using DocumentFormat.OpenXml.Office2010.ExcelAc;
using JetBrains.Annotations;
using softwrench.sW4.Shared2.Metadata.Applications.Schema;

namespace softWrench.sW4.Data.API.Composition {
    public class CompositionFetchRequest {

        public string Id { get; set; }
        public string UserId { get; set; }

        public ApplicationMetadataSchemaKey Key { get; set; }

        /// <summary>
        /// If this list is null every composition will be fetched
        /// </summary>
        [CanBeNull]
        public List<String> CompositionList { get; set; }


    }
}
