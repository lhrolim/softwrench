using System.Collections.Generic;
using System.Diagnostics;
using cts.commons.portable.Util;
using cts.commons.Util;
using log4net;
using softWrench.sW4.Exceptions;
using softwrench.sW4.Shared2.Metadata.Applications.Relationships.Associations;
using softwrench.sW4.Shared2.Metadata.Applications.Schema;

namespace softWrench.sW4.Metadata {
    public class MetadataProviderInternalCache {

        private static readonly ILog Log = LogManager.GetLogger(typeof(MetadataProviderInternalCache));

        public IDictionary<string, List<ApplicationRelationshipDefinition>> RelationshipsByNameCache = new Dictionary<string, List<ApplicationRelationshipDefinition>>();

        public MetadataProviderInternalCache() {
            var applications = MetadataProvider.Applications();
            var watch = Stopwatch.StartNew();
            foreach (var metadata in applications) {
                foreach (var schema in metadata.Schemas().Values) {
                    if (schema.Stereotype != SchemaStereotype.Detail && schema.Stereotype != SchemaStereotype.DetailNew) {
                        continue;
                    }
                    var compositions = schema.Compositions();
                    foreach (var composition in compositions) {
                        if (!RelationshipsByNameCache.ContainsKey(composition.Relationship)) {
                            RelationshipsByNameCache.Add(composition.Relationship, new List<ApplicationRelationshipDefinition>());
                        }
                        RelationshipsByNameCache[composition.Relationship].Add(composition);
                    }

                    var associations = schema.Associations();
                    foreach (var association in associations) {
                        if (association.EntityAssociation == null) {
                            throw new MetadataException("Missing entity relationship for association {0}. review your metadata".Fmt(association));
                        }

                        if (!RelationshipsByNameCache.ContainsKey(association.EntityAssociation.Qualifier)) {
                            RelationshipsByNameCache.Add(association.EntityAssociation.Qualifier, new List<ApplicationRelationshipDefinition>());
                        }
                        RelationshipsByNameCache[association.EntityAssociation.Qualifier].Add(association);
                    }
                }
            }
            Log.DebugFormat("Internal cache initialization took {0}", LoggingUtil.MsDelta(watch));

        }

    }
}
