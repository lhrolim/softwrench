using System;
using System.Collections.Generic;
using System.IO;
using log4net;
using softWrench.sW4.Metadata.Entities;
using softWrench.sW4.Metadata.Parsing;
using softwrench.sW4.Shared2.Metadata;

namespace softWrench.sW4.Metadata.Validator {
    internal class MetadataXmlTargetInitializer {

        private static readonly ILog Log = LogManager.GetLogger(typeof(MetadataProvider));

        internal ICollection<EntityMetadata> Entities;
        internal IReadOnlyCollection<CompleteApplicationMetadataDefinition> Applications;

        private const string Target = "target.xml";

        public void Validate(Stream data = null) {
            try {
                var internalTargets = new XmlEntityTargetMetadataParser().Parse(MetadataParsingUtils.GetInternalStreamImpl(false, data));
                try {
                    var streamReader = MetadataParsingUtils.GetStreamImpl(Target, streamValidator: data);
                    if (streamReader != null) {
                        var targets = new XmlEntityTargetMetadataParser().Parse(streamReader);
                        MergeTargets(internalTargets, targets);
                    }
                } catch (IOException) {
                    //file not found, ok
                }
                MergeEntities(internalTargets);



            } catch (Exception e) {
                Log.Error("error validating metadata", e);
                throw;
            }
        }

        private void MergeEntities(IEnumerable<KeyValuePair<string, XmlEntityTargetMetadataParser.TargetParsingResult>> targets) {
            foreach (var target in targets) {
                try {
                    var entity = MetadataProvider.Entity(target.Key);
                    entity.Targetschema = target.Value.TargetSchema;
                    entity.ConnectorParameters.Merge(target.Value.Parameters);
                } catch (Exception) {
                    Log.WarnFormat("Target entity {0} not found. This entity will be ignored.", target.Key); 
                }
            }
        }

        private void MergeTargets(IEnumerable<KeyValuePair<string, XmlEntityTargetMetadataParser.TargetParsingResult>> internalTargets,
            IDictionary<string, XmlEntityTargetMetadataParser.TargetParsingResult> targets) {
            foreach (var internalTarget in internalTargets) {
                if (!targets.ContainsKey(internalTarget.Key)) {
                    continue;
                }
                var overridenTarget = targets[internalTarget.Key];
                internalTarget.Value.TargetSchema.Merge(overridenTarget.TargetSchema);
                internalTarget.Value.Parameters.Merge(overridenTarget.Parameters);
            }
        }

    }
}
