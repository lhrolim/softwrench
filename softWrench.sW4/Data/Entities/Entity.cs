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
        public Entity([CanBeNull] string id, [NotNull] IDictionary<string, object> attributes,
            [NotNull] IDictionary<string, object> associationAttributes, [NotNull] EntityMetadata metadata)
            : base(attributes) {
            if (attributes == null)
                throw new ArgumentNullException("attributes");

            Id = id;
            AssociationAttributes = associationAttributes;
            Metadata = metadata;
        }

        public static Entity GetInstance(EntityMetadata metadata, string id = null) {
            return new Entity(id, new Dictionary<string, object>(), new Dictionary<string, object>(), metadata);
        }

        [CanBeNull]
        public string Id {
            get;
        }

        [NotNull]
        public string Name {
            get; private set;
        }

        public IDictionary<string, object> AssociationAttributes {
            get; private set;
        }

        public EntityMetadata Metadata {
            get;
        }


        public object GetRelationship(string attributeName, bool isSingleAssociation =false) {
            object relationship;
            attributeName = attributeName.EndsWith("_") ? attributeName : attributeName + "_";
            AssociationAttributes.TryGetValue(attributeName, out relationship);
            if (relationship == null) {
                if (!isSingleAssociation) {
                    return BlankList();
                }
            }

            return relationship;
        }

        public void ClearRelationShips(params string[] exceptFor) {
            if (exceptFor == null) {
                AssociationAttributes.Clear();
            } else {
                var tempDict = new Dictionary<string, object>();
                var set = new HashSet<string>(exceptFor);
                foreach (var association in AssociationAttributes) {
                    if (set.Contains(association.Key)) {
                        tempDict.Add(association.Key, association.Value);
                    }
                }
                AssociationAttributes = tempDict;
            }
        }

        public static Entity TestInstance([NotNull] IDictionary<string, object> attributes) {
            return new Entity(null, attributes, null, null);
        }

        protected virtual object BlankList() {
            return new List<Entity>();
        }

        public IDictionary<string, string> UnmappedAttributes { get; } = new Dictionary<string, string>(StringComparer.CurrentCultureIgnoreCase);

        public override string GetStringAttribute(string attributeName, bool remove = false, bool throwException = false) {
            var value = GetAttribute(attributeName, remove, throwException);
            return value?.ToString();
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

        public override bool ContainsAttribute(string attributeName, bool checksForNonNull = false) {
            var lowerAttribute = attributeName.ToLowerInvariant();
            if (lowerAttribute.Contains(".")) {
                string resultAttributeName;
                var relationshipName = EntityUtil.GetRelationshipName(lowerAttribute, out resultAttributeName);
                var relationship = GetRelationship(relationshipName);
                if (relationship is Entity) {
                    var containsAttribute = ((Entity)relationship).ContainsAttribute(resultAttributeName);
                    if (containsAttribute && checksForNonNull) {
                        var attribute = ((Entity)relationship).GetAttribute(resultAttributeName);
                        if (attribute is string) {
                            return !string.IsNullOrEmpty((string)attribute);
                        }
                        return attribute != null;
                    }
                    return containsAttribute;
                }
            }
            var containsKey = this.ContainsKey(lowerAttribute);
            var containsUnmapped = UnmappedAttributes.ContainsKey(lowerAttribute);
            if (!checksForNonNull) {
                return containsKey || containsUnmapped;
            }

            if (containsUnmapped) {
                var attribute = UnmappedAttributes[lowerAttribute];
                return !string.IsNullOrEmpty((string)attribute);
            }
            if (containsKey) {
                var attribute = this[lowerAttribute];
                if (attribute is string) {
                    return !string.IsNullOrEmpty((string)attribute);
                }
                return attribute != null;
            }
            return false;
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
            return Metadata.Name;
        }
    }
}
