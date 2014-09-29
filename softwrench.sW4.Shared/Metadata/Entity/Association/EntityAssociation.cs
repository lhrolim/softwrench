using System;
using System.Collections.Generic;
using System.Linq;

namespace softwrench.sW4.Shared.Metadata.Entity.Association {
    public class EntityAssociation {

        private readonly string _qualifier;
        private readonly string _to;
        private readonly bool _collection;

        private readonly IEnumerable<EntityAssociationAttribute> _attributes;

        public EntityAssociation(string qualifier,  string to,
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
            _collection = collection;
        }

        private static string BuildQualifier(string qualifier, string to) {
            string builtQualifier = qualifier ?? to;
            return builtQualifier.EndsWith("_") ? builtQualifier : builtQualifier + ("_");
        }

        
        public string Qualifier {
            get { return _qualifier; }
        }

        
        public string To {
            get { return _to; }
        }

        
        public IEnumerable<EntityAssociationAttribute> Attributes {
            get { return _attributes; }
        }

        public IEnumerable<EntityAssociationAttribute> NonPrimaryAttributes() {
            return _attributes.Where(r => r.Primary == false);
        }

        public EntityAssociationAttribute PrimaryAttribute() {
            return _attributes.First(r => r.Primary);
        }

        public bool Collection {
            get { return _collection; }
        }

        public override string ToString() {
            return string.Format("Qualifier: {0}, To: {1}", _qualifier, _to);
        }
    }
}
