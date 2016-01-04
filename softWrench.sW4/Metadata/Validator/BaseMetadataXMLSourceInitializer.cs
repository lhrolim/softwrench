using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using JetBrains.Annotations;
using log4net;
using softWrench.sW4.Metadata.Entities;
using softWrench.sW4.Metadata.Parsing;
using softwrench.sW4.Shared2.Metadata;
using softwrench.sw4.Shared2.Metadata.Applications.Command;
using softWrench.sW4.Metadata.Stereotypes;
using softWrench.sW4.Util;

namespace softWrench.sW4.Metadata.Validator {
    internal abstract class BaseMetadataXmlSourceInitializer {

        protected static readonly ILog Log = LogManager.GetLogger(typeof(MetadataProvider));

        internal ICollection<EntityMetadata> Entities;

        internal IDictionary<string, MetadataStereotype> CustomerStereotypes;

        internal IReadOnlyCollection<CompleteApplicationMetadataDefinition> Applications;
        public EntityQueries Queries {
            get; set;
        }

        protected abstract IEnumerable<EntityMetadata> InitializeEntityInternalMetadata();

        protected abstract string MetadataPath();

        protected abstract bool IsSWDB();

        public void Validate(IDictionary<string, CommandBarDefinition> commandBars,  Stream data = null) {
            try {
                Entities = InitializeEntities(data);
                foreach (var entityMetadata in Entities.Where(e => e.HasParent)) {
                    entityMetadata.MergeWithParent();
                }
                CustomerStereotypes = InitializeCustomerStereotypes(data);
                Applications = InitializeApplicationMetadata(Entities, commandBars, data);
            } catch (Exception e) {
                Log.Error("error validating metadata", e);
                throw;
            }
        }

        private IDictionary<string, MetadataStereotype> InitializeCustomerStereotypes(Stream data) {
            var parser = new XmlStereotypeMetadataParser();
            using (var stream = MetadataParsingUtils.GetStream(data, MetadataPath())) {
                return parser.Parse(stream);
            }
        }


        internal IReadOnlyCollection<CompleteApplicationMetadataDefinition> InitializeApplicationMetadata(
            [NotNull] IEnumerable<EntityMetadata> entityMetadata,
            IDictionary<string, CommandBarDefinition> commandBars, Stream streamValidator = null) {
            if (entityMetadata == null) {
                throw new ArgumentNullException("entityMetadata");
            }
            var applicationMetadata = new List<CompleteApplicationMetadataDefinition>();
            var parser = new XmlApplicationMetadataParser(entityMetadata, commandBars, IsSWDB(), false);
            using (var stream = MetadataParsingUtils.GetStream(streamValidator, MetadataPath())) {
                if (stream != null) {
                    applicationMetadata.AddRange(parser.Parse(stream));
                } else if (!IsSWDB()) {
                    throw new InvalidOperationException("metadata.xml is required");
                }
            }
            return applicationMetadata;
        }




        protected ICollection<EntityMetadata> InitializeEntities(Stream data) {
            var internalEntities = InitializeEntityInternalMetadata();
            return InitializeEntityMetadata(internalEntities, data);
        }

        private ICollection<EntityMetadata> InitializeEntityMetadata(IEnumerable<EntityMetadata> sourceEntities,
            Stream streamValidator = null) {

            var resultEntities = new HashSet<EntityMetadata>();

            IEnumerable<EntityMetadata> clientEntities = new List<EntityMetadata>();
            var metadataPath = MetadataPath();
            using (var stream = MetadataParsingUtils.GetStreamImpl(metadataPath, streamValidator: streamValidator)) {
                if (stream != null) {
                    var parsingResult = new XmlEntitySourceMetadataParser(IsSWDB()).Parse(stream);
                    clientEntities = parsingResult.Item1;
                    Queries = parsingResult.Item2;
                } else if (!IsSWDB()) {
                    throw new InvalidOperationException("metadata.xml is required");
                }
            }
            var entityMetadatas = clientEntities as EntityMetadata[] ?? clientEntities.ToArray();
            resultEntities.AddAll(MetadataMerger.MergeEntities(sourceEntities, entityMetadatas));

            foreach (var clientEntity in entityMetadatas) {
                resultEntities.Add(clientEntity);
            }


            return resultEntities;
        }


    }
}
