using JetBrains.Annotations;
using softwrench.sW4.Shared2.Data;
using softWrench.sW4.Metadata.Entities;
using softWrench.sW4.Util;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using cts.commons.portable.Util;


namespace softWrench.sW4.Data.Entities {
    public class Entity : AttributeHolder {

        private readonly String _id;
        private readonly IDictionary<string, object> _associationAttributes;
        private readonly IDictionary<string, string> _unmappedAttributes = new Dictionary<string, string>();
        private readonly EntityMetadata _metadata;

        public Entity([CanBeNull] string id, [NotNull] IDictionary<string, object> attributes,
            [NotNull] IDictionary<string, object> associationAttributes, [NotNull] EntityMetadata metadata)
            : base(attributes) {
            if (attributes == null) throw new ArgumentNullException("attributes");

            _id = id;
            _associationAttributes = associationAttributes;
            _metadata = metadata;
        }

        public static Entity GetInstance(EntityMetadata metadata, string id = null) {
            return new Entity(id, new Dictionary<string, object>(), new Dictionary<string, object>(), metadata);
        }

        [CanBeNull]
        public string Id {
            get { return _id; }
        }

        [NotNull]
        public string Name { get; private set; }

        public IDictionary<string, object> AssociationAttributes {
            get { return _associationAttributes; }
        }

        public EntityMetadata Metadata {
            get { return _metadata; }
        }


        public object GetRelationship(string attributeName) {
            object relationship;
            attributeName = attributeName.EndsWith("_") ? attributeName : attributeName + "_";
            _associationAttributes.TryGetValue(attributeName, out relationship);
            if (relationship == null) {
                if (_metadata.ListAssociations().Any(l => l.Qualifier.EqualsAny(attributeName))) {
                    return BlankList();
                }
            }

            return relationship;
        }

        public static Entity TestInstance([NotNull] IDictionary<string, object> attributes) {
            return new Entity(null, attributes, null, null);
        }

        protected virtual object BlankList() {
            return new List<Entity>();
        }

        public IDictionary<string, string> UnmappedAttributes {
            get { return _unmappedAttributes; }
        }

        public string GetStringAttribute(string attributeName, bool remove = false, bool throwException = false) {
            var value = GetAttribute(attributeName, remove, throwException);
            return value != null ? value.ToString() : null;
        }

        public override object GetAttribute(string attributeName, bool remove = false, bool throwException = false) {
            var lowerAttribute = attributeName.ToLowerInvariant();
            if (UnmappedAttributes.ContainsKey(lowerAttribute)) {
                return GetUnMappedAttribute(lowerAttribute);
            }

            if (lowerAttribute.Contains(".")) {
                string resultAttributeName;
                var relationshipName = EntityUtil.GetRelationshipName(lowerAttribute, out resultAttributeName);
                var relationship = GetRelationship(relationshipName);
                if (relationship is Entity) {
                    return ((Entity)relationship).GetAttribute(resultAttributeName);
                }
                if (relationship is IEnumerable) {
                    //todo: implement
                    return null;
                }
            }
            return base.GetAttribute(lowerAttribute, remove, throwException);
        }

        public override bool ContainsAttribute(string attributeName)
        {
            var lowerAttribute = attributeName.ToLowerInvariant();
            if (lowerAttribute.Contains(".")) {
                string resultAttributeName;
                var relationshipName = EntityUtil.GetRelationshipName(lowerAttribute, out resultAttributeName);
                var relationship = GetRelationship(relationshipName);
                if (relationship is Entity) {
                    return ((Entity)relationship).ContainsAttribute(resultAttributeName);
                }
            }
            return UnmappedAttributes.ContainsKey(lowerAttribute) || Attributes.ContainsKey(lowerAttribute);
        }

        /// <summary>
        /// these are application attributes which do not have a corresponding entry on the entity metadata
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public string GetUnMappedAttribute(string key) {
            return UnmappedAttributes.ContainsKey(key) ? UnmappedAttributes[key] : null;
        }

        public override string HolderName() {
            return _metadata.Name;
        }
    }
}
