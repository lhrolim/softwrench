using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using JetBrains.Annotations;
using softWrench.sW4.Metadata.Entities.Connectors;

namespace softWrench.sW4.Metadata.Entities.Schema {
    public class EntityTargetAttribute : EntityAttribute {

        public string TargetPath { get; set; }

        public EntityTargetAttribute([NotNull] string name, [NotNull] string type, bool isRequired,
            [NotNull] ConnectorParameters connectorParameters, string targetPath)
            : base(name, type, isRequired, false, connectorParameters, null) {
            TargetPath = targetPath;
        }


    }
}
