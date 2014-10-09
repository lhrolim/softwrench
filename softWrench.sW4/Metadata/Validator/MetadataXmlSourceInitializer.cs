using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using JetBrains.Annotations;
using log4net;
using softWrench.sW4.Metadata.Entities;
using softWrench.sW4.Metadata.Parsing;
using softwrench.sW4.Shared2.Metadata;

namespace softWrench.sW4.Metadata.Validator {
    internal class MetadataXmlSourceInitializer {

        private static readonly ILog Log = LogManager.GetLogger(typeof(MetadataProvider));

        private const string Metadata = "metadata.xml";

        internal ICollection<EntityMetadata> Entities;
        internal IReadOnlyCollection<CompleteApplicationMetadataDefinition> Applications;
        public EntityQueries Queries { get; set; }

        public void Validate(Stream data = null) {
            try {
                var internalEntities = InitializeEntityInternalMetadata();
                Entities = InitializeEntityMetadata(internalEntities, data);
                foreach (var entityMetadata in Entities.Where(e => e.HasParent)) {
                    entityMetadata.MergeWithParent();
                }
                Applications = InitializeApplicationMetadata(Entities, data);
            } catch (Exception e) {
                Log.Error("error validating metadata", e);
                throw;
            }
        }


        internal static IEnumerable<EntityMetadata> InitializeEntityInternalMetadata() {
            using (var stream = MetadataParsingUtils.GetInternalStreamImpl(true)) {
                return new XmlEntitySourceMetadataParser().Parse(stream).Item1;
            }
        }

        internal ICollection<EntityMetadata> InitializeEntityMetadata(IEnumerable<EntityMetadata> sourceEntities,
          Stream streamValidator = null) {

            var resultEntities = new HashSet<EntityMetadata>();

            IEnumerable<EntityMetadata> clientEntities;
            using (var stream = MetadataParsingUtils.GetStreamImpl(Metadata, false, streamValidator)) {
                var parsingResult = new XmlEntitySourceMetadataParser().Parse(stream);
                clientEntities = parsingResult.Item1;
                Queries = parsingResult.Item2;
            }
            foreach (var sourceEntity in sourceEntities) {
                var overridenEntity = clientEntities.FirstOrDefault(a => a.Name == sourceEntity.Name);
                if (overridenEntity != null) {
                    MergeEntities(sourceEntity, overridenEntity);
                }
                resultEntities.Add(sourceEntity);
            }

            foreach (var clientEntity in clientEntities) {
                resultEntities.Add(clientEntity);
            }


            return resultEntities;
        }

        private static void MergeEntities(EntityMetadata sourceEntity, EntityMetadata overridenEntity) {
            foreach (var association in overridenEntity.Associations) {
                if (overridenEntity.Schema.ExcludeUndeclaredAssociations) {
                    sourceEntity.Associations.Clear();
                }
                if (sourceEntity.Associations.Contains(association)) {
                    sourceEntity.Associations.Remove(association);
                }
                sourceEntity.Associations.Add(association);
            }
            foreach (var attribute in overridenEntity.Schema.Attributes) {
                if (overridenEntity.Schema.ExcludeUndeclaredAttributes) {
                    sourceEntity.Schema.Attributes.Clear();
                }
                if (sourceEntity.Schema.Attributes.Contains(attribute)) {
                    sourceEntity.Schema.Attributes.Remove(attribute);
                }
                sourceEntity.Schema.Attributes.Add(attribute);
            }

            foreach (var parameter in overridenEntity.ConnectorParameters.Parameters) {
                if (overridenEntity.ConnectorParameters.ExcludeUndeclared) {
                    sourceEntity.ConnectorParameters.Parameters.Clear();
                }
                if (sourceEntity.ConnectorParameters.Parameters.ContainsKey(parameter.Key)) {
                    sourceEntity.ConnectorParameters.Parameters.Remove(parameter.Key);
                }
                sourceEntity.ConnectorParameters.Parameters.Add(parameter);
            }
            if (overridenEntity.HasWhereClause) {
                sourceEntity.WhereClause = overridenEntity.WhereClause;
            }
        }

        internal static IReadOnlyCollection<CompleteApplicationMetadataDefinition> InitializeApplicationMetadata([NotNull] IEnumerable<EntityMetadata> entityMetadata, Stream streamValidator = null) {
            if (entityMetadata == null) {
                throw new ArgumentNullException("entityMetadata");
            }
            var applicationMetadata = new List<CompleteApplicationMetadataDefinition>();
            var parser = new XmlApplicationMetadataParser(entityMetadata);
            using (var stream = MetadataParsingUtils.GetStream(streamValidator, Metadata)) {
                applicationMetadata.AddRange(parser.Parse(stream));
            }
            return applicationMetadata;
        }







    }
}
