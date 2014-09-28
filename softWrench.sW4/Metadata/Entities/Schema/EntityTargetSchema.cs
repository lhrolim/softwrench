using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace softWrench.sW4.Metadata.Entities.Schema {
    public class EntityTargetSchema {

        private HashSet<EntityTargetConstant> _constValues = new HashSet<EntityTargetConstant>();

        private HashSet<EntityTargetAttribute> _targetAttributes = new HashSet<EntityTargetAttribute>();

        private HashSet<EntityTargetRelationship> _relationships = new HashSet<EntityTargetRelationship>();



        public EntityTargetSchema() { }


        public EntityTargetSchema(HashSet<EntityTargetConstant> constValues, IEnumerable<EntityTargetAttribute> targetAttributes, IEnumerable<EntityTargetRelationship> targetRelationships) {
            _constValues = constValues;
            if (targetAttributes != null) {
                foreach (var entityTargetAttribute in targetAttributes) {
                    _targetAttributes.Add(entityTargetAttribute);
                }
            }

            if (targetRelationships != null) {
                foreach (var relationship in targetRelationships) {
                    _relationships.Add(relationship);
                }
            }
        }

        public HashSet<EntityTargetConstant> ConstValues {
            get { return _constValues; }
            set { _constValues = value; }
        }

        public HashSet<EntityTargetAttribute> TargetAttributes {
            get { return _targetAttributes; }
            set { _targetAttributes = value; }
        }

        public HashSet<EntityTargetRelationship> Relationships {
            get { return _relationships; }
            set { _relationships = value; }
        }


        public void Merge(EntityTargetSchema targetSchema) {
            foreach (var constValue in targetSchema.ConstValues) {
                if (_constValues.Contains(constValue)) {
                    _constValues.Remove(constValue);
                }
                _constValues.Add(constValue);
            }

            foreach (var targetAttribute in targetSchema.TargetAttributes) {
                var oldValue = _targetAttributes.FirstOrDefault(
                    t => t.Name.Equals(targetAttribute.Name, StringComparison.CurrentCultureIgnoreCase));
                if (oldValue != null) {
                    _targetAttributes.Remove(oldValue);
                }
                _targetAttributes.Add(targetAttribute);
            }
        }
    }
}
