using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using softwrench.sW4.Shared2.Metadata.Applications;
using softwrench.sW4.Shared2.Metadata.Applications.Schema;

namespace softwrench.sw4.Shared2.Util {
    public class SchemaUtil {

        public static ApplicationMetadataSchemaKey ParseKey(string schemaKey, bool throwException = true) {
            var keys = schemaKey.Split('.');
            if (keys.Length == 0) {
                if (throwException) {
                    throw new InvalidOperationException("wrong schemakey " + schemaKey);
                }
                return null;
            }
            string mode = null;
            var platform = ClientPlatform.Web.ToString().ToLower();
            var schemaId = keys[0];
            if (keys.Length == 3) {

                if (keys.Length > 1) {
                    mode = String.IsNullOrWhiteSpace(keys[1]) ? null : keys[1];
                }

                if (keys.Length > 2) {
                    platform = String.IsNullOrWhiteSpace(keys[2]) ? null : keys[2];
                }

            }
            return new ApplicationMetadataSchemaKey(schemaId, mode, platform);
        }


        /// <summary>
        /// this data is serialized into a '.' separated string, in the format, schemaId.mode.platform. 
        /// see aa_utils.js on the web client
        /// </summary>
        public static ApplicationMetadataSchemaKey GetSchemaKeyFromString(string schemaKey, ClientPlatform clientPlatform) {
            if (ClientPlatform.Mobile == clientPlatform) {
                //for now, the mobile schema can only be detail.input
                //TODO: fix it on mobile side
                return new ApplicationMetadataSchemaKey(SchemaStereotype.Detail.ToString().ToLower(), SchemaMode.input, ClientPlatform.Mobile);
            }
            if (schemaKey == null || String.IsNullOrWhiteSpace(schemaKey)) {
                return null;
            }
            return ParseKey(schemaKey, false);
        }

        public static Tuple<String, ApplicationMetadataSchemaKey> ParseApplicationAndKey(string key) {
            var idx = key.IndexOf('.');
            return new Tuple<string, ApplicationMetadataSchemaKey>(key.Substring(0, idx), ParseKey(key.Substring(idx + 1)));
        }
    }
}
