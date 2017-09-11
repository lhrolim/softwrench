using System.Collections.Generic;
using System.Linq;
using cts.commons.portable.Util;
using cts.commons.simpleinjector;
using log4net;
using softwrench.sW4.Shared2.Metadata.Applications.Relationships.Compositions;
using softwrench.sW4.Shared2.Metadata.Applications.Schema;
using softWrench.sW4.Data.Relationship.Composition;
using softWrench.sW4.Metadata.Security;

namespace softWrench.sW4.Metadata.Applications.Security {
    public class ApplicationCompositionSecurityApplier : ISingletonComponent {

        private static readonly ILog Log = LogManager.GetLogger(typeof(CompositionBuilder));

        public ApplicationCompositionSecurityApplier() {
            Log.Debug("init...");
        }

        //TODO: make it a simpleinjector component
        public IDictionary<string, ApplicationCompositionSchema> ApplySecurity(ApplicationSchemaDefinition schema, IDictionary<string, ApplicationCompositionSchema> originalCompositions, InMemoryUser user) {
            if (user == null) {
                return originalCompositions;
            }
            var appPermission = user.MergedUserProfile.Permissions.FirstOrDefault(f => f.ApplicationName.EqualsIc(schema.ApplicationName));
            if (appPermission == null || appPermission.CompositionPermissions == null || !appPermission.CompositionPermissions.Any(c => c.Schema.EqualsIc(schema.SchemaId))) {
                Log.DebugFormat("no security constraint, removing default compositions for schema {0}", schema.GetApplicationKey());
                return originalCompositions;
            }
            var clonedDictionary = new Dictionary<string, ApplicationCompositionSchema>(originalCompositions);
            foreach (var compositionPermission in appPermission.CompositionPermissions) {
                var key = compositionPermission.CompositionKey;
                if (!key.EndsWith("_")) {
                    key = key + "_";
                }

                if (!clonedDictionary.ContainsKey(key)) {
                    Log.WarnFormat("composition {0} not found at schema {1}", compositionPermission.CompositionKey, schema.GetApplicationKey());
                    continue;
                }
                if (compositionPermission.HasNoPermissions) {
                    clonedDictionary.Remove(key);
                } else {
                    var composition = clonedDictionary[key];
                    if (!(composition is ApplicationCompositionCollectionSchema)) {
                        continue;
                    }
                    Log.InfoFormat("modifying composition {0} at schema {1} due to the presence of permissions", compositionPermission.CompositionKey, schema.GetApplicationKey());
                    var coll = (ApplicationCompositionCollectionSchema)composition.Clone();
                    coll.CollectionProperties.AllowInsertion = compositionPermission.AllowCreation.ToString().ToLower();
                    coll.CollectionProperties.AllowUpdate = compositionPermission.AllowUpdate.ToString().ToLower();
                    coll.CollectionProperties.AllowRemoval = compositionPermission.AllowRemoval.ToString().ToLower();
                }
            }
            return clonedDictionary;
        }

    }
}
