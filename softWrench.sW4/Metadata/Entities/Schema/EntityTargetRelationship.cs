using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace softWrench.sW4.Metadata.Entities.Schema {
    public class EntityTargetRelationship {

        public string Targetpath { get; set; }
        public string Attribute { get; set; }

        private EntityTargetSchema _entityTarget;

        public EntityTargetRelationship(EntityTargetSchema entityTarget, string targetpath, string attribute) {
            _entityTarget = entityTarget;
            Targetpath = targetpath;
            Attribute = attribute;
        }

    }
}
