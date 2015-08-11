using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;

namespace softwrench.sW4.Shared2.Metadata.Offline {
    public class MobileMetadataDownloadResponseDefinition {

        public String MenuJson { get; set; }

        public String TopLevelMetadatasJson { get; set; }

        public String CompositionMetadatasJson { get; set; }

        public String AssociationMetadatasJson { get; set; }

        public JObject AppConfiguration { get; set; }

    }
}
