using System.Collections.Generic;
using Newtonsoft.Json;
using softWrench.Mobile.Metadata.Offline;
using softwrench.sW4.Shared2.Metadata;
using softwrench.sW4.Shared2.Metadata.Offline;

namespace softWrench.Mobile.Metadata.Parsing {
    internal class JsonDownloadMetadataParser {


        public static MobileMetadataDownloadResponse ParseMetadata(MobileMetadataDownloadResponseDefinition definition) {
            //            var metadatas =json.Value<JArray>("metadatasJSON");
            var menu = JsonParser.ParseMenu(definition.MenuJson);
            var metadatas = JsonConvert.DeserializeObject<IEnumerable<CompleteApplicationMetadataDefinition>>(definition.MetadatasJSON, JsonParser.SerializerSettings);
            return new MobileMetadataDownloadResponse(menu, metadatas);
        }


    }
}