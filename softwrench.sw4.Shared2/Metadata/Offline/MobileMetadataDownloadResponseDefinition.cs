using System.Collections.Generic;
using Newtonsoft.Json.Linq;

namespace softwrench.sW4.Shared2.Metadata.Offline {
    public class MobileMetadataDownloadResponseDefinition {

        public string MenuJson { get; set; }

        public string TopLevelMetadatasJson { get; set; }

        public string CompositionMetadatasJson { get; set; }

        public string AssociationMetadatasJson { get; set; }

        public string CommandBarsJson { get; set; }

        public JObject AppConfiguration { get; set; }
    }

}
