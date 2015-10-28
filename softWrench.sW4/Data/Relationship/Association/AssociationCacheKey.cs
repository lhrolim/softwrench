using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace softWrench.sW4.Data.Relationship.Association {
    public class AssociationCacheKey {

        private readonly string _relQualifier;
        private readonly string _parentEntity;

        public AssociationCacheKey(string relQualifier, string parentEntity) {
            _relQualifier = relQualifier;
            _parentEntity = parentEntity;
        }

        protected bool Equals(AssociationCacheKey other) {
            return string.Equals(_relQualifier, other._relQualifier) && string.Equals(_parentEntity, other._parentEntity);
        }

        public override bool Equals(object obj) {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((AssociationCacheKey)obj);
        }

        public override int GetHashCode() {
            unchecked {
                return ((_relQualifier != null ? _relQualifier.GetHashCode() : 0) * 397) ^ (_parentEntity != null ? _parentEntity.GetHashCode() : 0);
            }
        }
    }
}
