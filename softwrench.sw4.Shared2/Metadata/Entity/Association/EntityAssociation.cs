﻿using System;
using System.Collections.Generic;
using System.Linq;

namespace softwrench.sW4.Shared2.Metadata.Entity.Association {
    public class EntityAssociation {

        private string _qualifier;
        private string _to;
        public string EntityName { get; set; }

        private IEnumerable<EntityAssociationAttribute> _attributes;

        public EntityAssociation() {

        }

        public EntityAssociation(string qualifier, string to,
              IEnumerable<EntityAssociationAttribute> attributes, bool collection) {
            //            if (qualifier == null) throw new ArgumentNullException("qualifier");
            if (to == null) throw new ArgumentNullException("to");
            if (attributes == null) throw new ArgumentNullException("attributes");
            _qualifier = BuildQualifier(qualifier, to);
            _to = to;
            _attributes = attributes;
            if (PrimaryAttribute() == null) {
                throw new InvalidOperationException(String.Format("Entity must have a primary attribute on association {0}", to));
            }
            Collection = collection;
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

        public bool Collection { get; set; }


        public IEnumerable<EntityAssociationAttribute> Attributes {
            get { return _attributes; }
            set { _attributes = value; }
        }

        public IEnumerable<EntityAssociationAttribute> NonPrimaryAttributes() {
            return _attributes.Where(r => r.Primary == false);
        }

        public EntityAssociationAttribute PrimaryAttribute() {
            if (_attributes.Count() == 1) {
                return _attributes.First();
            }

            return _attributes.First(r => r.Primary);
        }




        public override string ToString() {
            return string.Format("Qualifier: {0}, To: {1}", _qualifier, _to);
        }

        public EntityAssociation CloneWithContext(string contextAlias) {
            var cloned = new EntityAssociation(contextAlias + Qualifier, To, Attributes, Collection);
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
            if (obj.GetType() != this.GetType()) return false;
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
