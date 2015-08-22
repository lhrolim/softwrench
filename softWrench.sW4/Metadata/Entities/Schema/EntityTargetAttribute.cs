using JetBrains.Annotations;
using softWrench.sW4.Metadata.Entities.Connectors;

namespace softWrench.sW4.Metadata.Entities.Schema {
    public class EntityTargetAttribute : EntityAttribute {

        public string TargetPath { get; set; }

        public EntityTargetAttribute([NotNull] string name, [NotNull] string type, bool requiredExpression,
            [NotNull] ConnectorParameters connectorParameters, string targetPath)
            : base(name, type, requiredExpression, false, connectorParameters, null) {
            TargetPath = targetPath;
        }


    }
}
