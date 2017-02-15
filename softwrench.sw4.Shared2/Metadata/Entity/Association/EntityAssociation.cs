using System;
using System.Collections.Generic;
using System.Linq;

namespace softwrench.sW4.Shared2.Metadata.Entity.Association {
    public class EntityAssociation {
        public string EntityName {
            get; set;
        }
        public bool Collection {
            get; set;
        }

        public bool InnnerJoin {
            get; set;
        }

        public bool Cacheable {
            get; set;
        }
        public bool Lazy {
            get; set;
        }

        public string ReverseLookupAttribute {
            get; set;
        }
        public bool Reverse => ReverseLookupAttribute != null;

        private IEnumerable<EntityAssociationAttribute> _attributes;

        public EntityAssociation() {

        }

        public EntityAssociation(string qualifier, string to,
                                 IEnumerable<EntityAssociationAttribute> attributes, bool collection, bool cacheable, bool lazy, string reverseLookupAttribute, bool ignorePrimaryAttribute, bool innerJoin) {

            //            if (qualifier == null) throw new ArgumentNullException("qualifier");
            if (to == null)
                throw new ArgumentNullException("to");
            if (attributes == null)
                throw new ArgumentNullException("attributes");
            Qualifier = BuildQualifier(qualifier, to);
            To = to;
            _attributes = attributes;
            Cacheable = cacheable;
            Lazy = lazy;
            InnnerJoin = innerJoin;
            IgnorePrimaryAttribute = ignorePrimaryAttribute;
            if (PrimaryAttribute() == null && !ignorePrimaryAttribute) {
                throw new InvalidOperationException(
                    $"Entity must have a primary attribute on association {to}, or have the ignoreprimary marked as true");
            }
            Collection = collection;
            ReverseLookupAttribute = reverseLookupAttribute;
        }

        private static string BuildQualifier(string qualifier, string to) {
            var builtQualifier = qualifier ?? to;
            return builtQualifier.EndsWith("_") ? builtQualifier : builtQualifier + ("_");
        }

        public string Qualifier { get; set; }

        public string To { get; set; }

        public IEnumerable<EntityAssociationAttribute> Attributes {
            get {
                return _attributes;
            }
            set {
                _attributes = value;
            }
        }

        public bool IgnorePrimaryAttribute {
            get; set;
        }

        public IEnumerable<EntityAssociationAttribute> NonPrimaryAttributes() {
            return _attributes.Where(r => r.Primary == false);
        }

        public EntityAssociationAttribute PrimaryAttribute() {
            if (_attributes.Count() == 1) {
                return _attributes.First();
            }

            return _attributes.FirstOrDefault(r => r.Primary);
        }




        public override string ToString() {
            return $"Qualifier: {Qualifier}, To: {To}";
        }

        public EntityAssociation CloneWithContext(string contextAlias) {
            var cloned = new EntityAssociation(contextAlias + Qualifier, To, Attributes, Collection, Cacheable, Lazy, ReverseLookupAttribute, IgnorePrimaryAttribute, InnnerJoin);
            if (EntityName == null) {
                cloned.EntityName = contextAlias;
            } else {
                cloned.EntityName = contextAlias + EntityName;
            }
            return cloned;
        }

        protected bool Equals(EntityAssociation other) {
            return string.Equals(Qualifier, other.Qualifier) && string.Equals(To, other.To) && string.Equals(EntityName, other.EntityName);
        }

        public override bool Equals(object obj) {
            if (ReferenceEquals(null, obj))
                return false;
            if (ReferenceEquals(this, obj))
                return true;
            if (!(obj.GetType().IsAssignableFrom(GetType()) || GetType().IsInstanceOfType(obj)))
                return false;
            return Equals((EntityAssociation)obj);
        }

        public override int GetHashCode() {
            unchecked {
                var hashCode = Qualifier?.GetHashCode() ?? 0;
                hashCode = (hashCode * 397) ^ (To != null ? To.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (EntityName != null ? EntityName.GetHashCode() : 0);
                return hashCode;
            }
        }
    }
}
