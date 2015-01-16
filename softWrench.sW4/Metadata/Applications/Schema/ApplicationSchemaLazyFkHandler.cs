using System.Diagnostics;
using System.Globalization;
using log4net;
using softwrench.sW4.Shared2.Metadata.Applications.Schema;
using softwrench.sW4.Shared2.Metadata.Applications.Schema.Interfaces;
using softWrench.sW4.Util;
using System.Collections.Generic;
using System.Linq;

namespace softWrench.sW4.Metadata.Applications.Schema {
    class ApplicationSchemaLazyFkHandler {

        private static readonly ILog Log = LogManager.GetLogger(typeof(ApplicationSchemaLazyFkHandler));

        public static ApplicationSchemaDefinition.LazyFkResolverDelegate SyncSchemaLazyFkResolverDelegate =
            delegate(ApplicationSchemaDefinition definition) {
                if (!MetadataProvider.FinishedParsing) {
                    return null;
                }
                var app = MetadataProvider.Application(definition.ApplicationName);
                var schemas = app.Schemas();
                var resultSet =
                    new HashSet<IApplicationAttributeDisplayable>();
                foreach (var schema in schemas.Values) {
                    if (Equals(schema.GetSchemaKey(), definition.GetSchemaKey()) || schema.IsWebPlatform()) {
                        continue;
                    }
                    var fields = schema.Fields;
                    foreach (var applicationFieldDefinition in fields) {
                        resultSet.Add(applicationFieldDefinition);
                    }
                }
                return new List<IApplicationAttributeDisplayable>(resultSet);
            };


        public static ApplicationSchemaDefinition.LazyFkResolverDelegate LazyFkResolverDelegate = delegate(ApplicationSchemaDefinition definition) {
            var watch = Stopwatch.StartNew();
            if (!MetadataProvider.FinishedParsing) {
                return null;
            }
            if (definition.Stereotype != SchemaStereotype.List &&
                definition.Stereotype != SchemaStereotype.CompositionList) {
                //blank list in order to consider it done
                return new List<IApplicationAttributeDisplayable>();
            }
            var resultList = new List<IApplicationAttributeDisplayable>();
            resultList.AddRange(DirectFKsFields(definition));
            resultList.AddRange(InverseFKsFields(definition));
            Log.DebugFormat("finished LazyFkResolverDelegate took {0} ", watch.ElapsedMilliseconds.ToString(CultureInfo.InvariantCulture));
            return resultList;
        };

        private static IEnumerable<IApplicationAttributeDisplayable> InverseFKsFields(ApplicationSchemaDefinition thisSchema) {
            var resultList = new List<IApplicationAttributeDisplayable>();
            var applications = MetadataProvider.Applications();

            if (MetadataProvider.InternalCache == null) {
                MetadataProvider.InternalCache = new MetadataProviderInternalCache();
            }

            var cache = MetadataProvider.InternalCache;

            var relationshipName = EntityUtil.GetRelationshipName(thisSchema.ApplicationName);

            if (!cache.RelationshipsByNameCache.ContainsKey(relationshipName)) {
                return resultList;
            }

            var relationships = MetadataProvider.InternalCache.RelationshipsByNameCache[relationshipName];

            foreach (var association in relationships) {
                var entityAssociation = association.EntityAssociation;
                foreach (var attribute in entityAssociation.Attributes) {
                    if (attribute.To != null) {
                        resultList.Add(ApplicationFieldDefinition.HiddenInstance(thisSchema.ApplicationName,
                            attribute.To));
                    }
                }
            }


            return resultList;
        }

        private static IEnumerable<IApplicationAttributeDisplayable> DirectFKsFields(ApplicationSchemaDefinition schema) {
            return new List<IApplicationAttributeDisplayable>();
        }



    }
}
