using System;
using System.Collections.Generic;
using System.Linq;

namespace softwrench.sW4.Shared2.Metadata.Entity.Association {
    public class EntityAssociation {

        private string _qualifier;
        private string _to;
        public string EntityName { get; set; }
        public bool Collection { get; set; }

        public bool Cacheable { get; set; }

        public string ReverseLookupAttribute { get; set; }
        public bool Reverse { get { return ReverseLookupAttribute != null; } }

        private IEnumerable<EntityAssociationAttribute> _attributes;

        public EntityAssociation() {

        }

        public EntityAssociation(string qualifier, string to,
                                 IEnumerable<EntityAssociationAttribute> attributes, bool collection,bool cacheable, string reverseLookupAttribute,bool ignorePrimaryAttribute) {
            
            //            if (qualifier == null) throw new ArgumentNullException("qualifier");
            if (to == null) throw new ArgumentNullException("to");
            if (attributes == null) throw new ArgumentNullException("attributes");
            _qualifier = BuildQualifier(qualifier, to);
            _to = to;
            _attributes = attributes;
            Cacheable = cacheable;
            IgnorePrimaryAttribute = ignorePrimaryAttribute;
            if (PrimaryAttribute() == null && !ignorePrimaryAttribute) {
                throw new InvalidOperationException(String.Format("Entity must have a primary attribute on association {0}, or have the ignoreprimary marked as true", to));
            }
            Collection = collection;
            ReverseLookupAttribute = reverseLookupAttribute;
        }

        private static string BuildQualifier(string qualifier, string to) {
            string builtQualifier = qualifier ?? to;
            return builtQualifier.EndsWith("_") ? builtQualifier : builtQualifier + ("_");
        }

        public string Qualifier {
            get { return _qualifier; }
            set { _qualifier = value; }
        }

        public string To {
            get { return _to; }
            set { _to = value; }
        }

        public IEnumerable<EntityAssociationAttribute> Attributes {
            get { return _attributes; }
            set { _attributes = value; }
        }

        public bool IgnorePrimaryAttribute { get; set; }

        public IEnumerable<EntityAssociationAttribute> NonPrimaryAttributes() {
            return _attributes.Where(r => r.Primary == false);
        }

        public EntityAssociationAttribute PrimaryAttribute() {
            return _attributes.FirstOrDefault(r => r.Primary);
        }




        public override string ToString() {
            return string.Format("Qualifier: {0}, To: {1}", _qualifier, _to);
        }

        public EntityAssociation CloneWithContext(string contextAlias) {
            var cloned = new EntityAssociation(contextAlias + Qualifier, To, Attributes, Collection,Cacheable, ReverseLookupAttribute,IgnorePrimaryAttribute);
            if (EntityName == null) {
                cloned.EntityName = contextAlias;
            } else {
                cloned.EntityName = contextAlias + EntityName;
            }
            return cloned;
        }

        protected bool Equals(EntityAssociation other) {
            return string.Equals(_qualifier, other._qualifier) && string.Equals(_to, other._to) && string.Equals(EntityName, other.EntityName);
        }

        public override bool Equals(object obj) {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (!(obj.GetType().IsAssignableFrom(GetType()) || GetType().IsInstanceOfType(obj))) return false;
            return Equals((EntityAssociation)obj);
        }

        public override int GetHashCode() {
            unchecked {
                var hashCode = (_qualifier != null ? _qualifier.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (_to != null ? _to.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (EntityName != null ? EntityName.GetHashCode() : 0);
                return hashCode;
            }
        }
    }
}
