using System;
using System.Collections.Generic;
using System.Linq;
using cts.commons.portable.Util;
using cts.commons.Util;
using JetBrains.Annotations;
using softWrench.sW4.Data.Persistence.Relational;
using softWrench.sW4.Data.Persistence.Relational.QueryBuilder;
using softWrench.sW4.Metadata.Entities.Connectors;
using softWrench.sW4.Metadata.Entities.Schema;
using softWrench.sW4.Metadata.Entities.Sliced;
using softwrench.sW4.Shared2.Data;
using softwrench.sW4.Shared2.Metadata.Entity.Association;
using softWrench.sW4.Exceptions;
using softWrench.sW4.Util;

namespace softWrench.sW4.Metadata.Entities {
    public class EntityMetadata {

        private const string ConnectorTableName = "dbtable";

        private readonly Lazy<IEnumerable<EntityAttribute>> _nonCollectionrelationshipAttributes;

        private readonly Lazy<IEnumerable<EntityAttribute>> _relationshipAttributes;


        public EntityMetadata([NotNull] string name, [NotNull] EntitySchema schema, [NotNull] IEnumerable<EntityAssociation> associations,
            [NotNull] ConnectorParameters connectorParameters, Type backendType = null) {

            Validate.NotNull(name, "name");
            Validate.NotNull(schema, "schema");
            Validate.NotNull(associations, "associations");
            Validate.NotNull(connectorParameters, "connectorParameters");

            Name = name;
            Schema = schema;
            BackEndType = backendType;
            Associations = new HashSet<EntityAssociation>(associations);
            ConnectorParameters = connectorParameters;
            _relationshipAttributes = new Lazy<IEnumerable<EntityAttribute>>(AddRelationshipAttributes);
            _nonCollectionrelationshipAttributes = new Lazy<IEnumerable<EntityAttribute>>(AddNonCollectionRelationshipAttributes);
        }

        internal void MergeWithParent() {
            var parent = MetadataProvider.Entity(Schema.ParentEntity);
            var parentAssociations = parent.Associations;
            var parentAttributes = parent.Schema.Attributes;
            var parentConnectors = parent.ConnectorParameters;

            if (!Schema.ExcludeUndeclaredAssociations) {
                foreach (var parentAssociation in parentAssociations) {
                    Associations.Add(parentAssociation);
                }
            }

            if (!ConnectorParameters.ExcludeUndeclared) {
                var thisParameters = ConnectorParameters.Parameters;
                foreach (var parentParameter in parentConnectors.Parameters) {
                    if (!thisParameters.ContainsKey(parentParameter.Key)) {
                        thisParameters.Add(parentParameter.Key, parentParameter.Value);
                    }
                }
            }

            if (!Schema.ExcludeUndeclaredAttributes) {
                foreach (var parentAttribute in parentAttributes) {
                    Schema.Attributes.Add(parentAttribute);
                }
            }

            if (!HasWhereClause && parent.HasWhereClause) {
                Schema.WhereClause = parent.Schema.WhereClause;
            }
        }

        [CanBeNull]
        public Type BackEndType { get; }


        [NotNull]
        public string Name { get; }

        [NotNull]
        public EntitySchema Schema { get; }


        [NotNull]
        public ConnectorParameters ConnectorParameters { get; }

        [NotNull]
        public ISet<EntityAssociation> Associations { get; }

        public ISet<EntityAssociation> ReverseAssociations() {
            return new HashSet<EntityAssociation>(Associations.Where(entityAssociation => (entityAssociation.Reverse)));
        }

        public ISet<EntityAssociation> ListAssociations() {
            return new HashSet<EntityAssociation>(Associations.Where(entityAssociation => (entityAssociation.Collection)));
        }

        public virtual ISet<EntityAssociation> NonListAssociations(bool innerCall = false) {
            return new HashSet<EntityAssociation>(Associations.Where(entityAssociation => (!entityAssociation.Collection && !entityAssociation.Reverse)));
        }

        public virtual AttributeHolder GetAttributeHolder(IEnumerable<KeyValuePair<string, object>> keyValuePairs) {
            return new EntityPopulator().Populate(this, keyValuePairs);
        }

        public virtual int? FetchLimit() {
            return null;
        }

        public virtual bool HasUnion() {
            return false;
        }

        public EntityTargetSchema Targetschema {
            get; set;
        }

        public IDictionary<QueryCacheKey, string> QueryStringCache { get; set; } = new Dictionary<QueryCacheKey, string>();

        public IEnumerable<EntityAttribute> Attributes(AttributesMode includeCollections) {
            var entityAttributes = Schema.Attributes.ToList();
            var associationAttributes = includeCollections == AttributesMode.IncludeCollections ?
                _relationshipAttributes.Value : _nonCollectionrelationshipAttributes.Value;
            entityAttributes.AddRange(associationAttributes);
            return new HashSet<EntityAttribute>(entityAttributes);
        }

        public enum AttributesMode {
            NoCollections, IncludeCollections
        }

        private IEnumerable<EntityAttribute> AddGenericRelationshipAttributes(AttributesMode includeCollections, string prefix = null) {
            var relationshipAttributes = new List<EntityAttribute>();
            var associations = includeCollections == AttributesMode.IncludeCollections ? Associations : NonListAssociations();
            foreach (var usedRelationship in associations) {
                if (usedRelationship is SlicedEntityAssociation) {
                    //add only the attributes that are used for a given relationship
                    relationshipAttributes.AddRange(((SlicedEntityAssociation)usedRelationship).SlicedAttributes);
                } else {
                    var relatedEntity = MetadataProvider.Entity(usedRelationship.To);
                    foreach (var entityAttribute in GetAttributesToIterate(relatedEntity, usedRelationship)) {
                        var key = prefix == null ? usedRelationship.Qualifier : prefix + usedRelationship.Qualifier;
                        var clonedAttribute = entityAttribute.ClonePrependingContext(key);
                        relationshipAttributes.Add(clonedAttribute);
                    }
                }
            }
            return relationshipAttributes;
        }

        protected virtual IEnumerable<EntityAttribute> GetAttributesToIterate(EntityMetadata relatedEntity, EntityAssociation usedRelationship) {
            return relatedEntity.Schema.Attributes;
        }

        private IEnumerable<EntityAttribute> AddRelationshipAttributes() {
            return AddGenericRelationshipAttributes(AttributesMode.IncludeCollections);
        }

        private IEnumerable<EntityAttribute> AddNonCollectionRelationshipAttributes() {
            return AddGenericRelationshipAttributes(AttributesMode.NoCollections);
        }

        public EntityMetadata RelatedEntityMetadata(string originalRelationshipName) {
            var relationshipName = softWrench.sW4.Util.EntityUtil.GetRelationshipName(originalRelationshipName);
            var association = Associations.FirstOrDefault(r => r.Qualifier.EqualsIc(relationshipName));
            if (association == null) {
                //fallback
                association = Associations.FirstOrDefault(r => r.To.EqualsIc(originalRelationshipName));
            }

            return association != null ? MetadataProvider.Entity(association.To) : null;
        }

        public string GetTableName() {
            string tableName;
            var value = !ConnectorParameters.Parameters.TryGetValue(ConnectorTableName, out tableName) ? Name : tableName;
            return value;
        }

        public Boolean HasParent => Schema.ParentEntity != null;

        public Boolean HasWhereClause => !string.IsNullOrEmpty(Schema.WhereClause);

        public string IdFieldName => Schema.IdAttribute.Name;

        public string UserIdFieldName => Schema.UserIdAttribute.Name;

        public string WhereClause {
            get {
                return Schema.WhereClause;
            }
            set {
                Schema.WhereClause = value;
            }
        }

        public Tuple<EntityAttribute, string> LocateNonCollectionAttribute(string attributeName, IEnumerable<EntityAttribute> attributes = null) {
            if (attributes == null) {
                //this invocation is a bit slow, therefore we can cache it whenever possible
                attributes = Attributes(AttributesMode.NoCollections) as IList<EntityAttribute> ?? Attributes(AttributesMode.NoCollections).ToList();
            }

            if (!attributeName.Contains('.')) {
                var resultAttribute = attributes.FirstOrDefault(f => f.Name.EqualsIc(attributeName));
                if (resultAttribute == null) {
                    return null;
                }
                return new Tuple<EntityAttribute, string>(resultAttribute, null);
            }
            var currentAttributeName = attributeName;
            string resultName;
            var innerMetadata = this;
            var context = "";
            do {
                var relationshipName = EntityUtil.GetRelationshipName(currentAttributeName, out resultName);
                context += relationshipName;
                innerMetadata = innerMetadata.RelatedEntityMetadata(relationshipName);
                currentAttributeName = resultName;
            } while (currentAttributeName.Contains("_") && innerMetadata != null);
            if (innerMetadata == null) {
                return null;
            }
            var attribute = innerMetadata.Attributes(AttributesMode.NoCollections).FirstOrDefault(f => f.Name.EqualsIc(resultName));
            return new Tuple<EntityAttribute, string>(attribute, context);
        }

        /// <summary>
        ///  Locates an association given its qualified name
        /// </summary>
        /// <param name="name">Name of the relationship to search, that might or not contain _ or . in the end</param>
        /// <returns></returns>
        [NotNull]
        public EntityAssociation LocateAssociationByName(string name) {
            return LocateAssociationByLabelField(name + "." + "fake").Item1;
        }


        public Tuple<EntityAssociation, EntityAttribute> LocateAssociationByLabelField(string labelField, bool validate = false) {
            var indexOf = labelField.IndexOf(".", StringComparison.Ordinal);
            var firstPart = labelField.Substring(0, indexOf);
            var lookupString = firstPart.EndsWith("_") ? firstPart : firstPart + "_";
            if (char.IsNumber(lookupString[0])) {
                //deprecated, in flavor of using it on the final of the string to avoid angular errors
                lookupString = lookupString.Substring(1);
            } else if (char.IsNumber(lookupString[lookupString.Length - 2])) {
                //disconsidering the _ and the number itself
                lookupString = lookupString.Substring(0, lookupString.Length - 2) + "_";
            }
            var entityAssociations = Associations;
            var association = entityAssociations.FirstOrDefault(a => a.Qualifier.EqualsIc(lookupString));

            if (association == null) {
                throw new MetadataException("association {0} cannot be located on entity {1}".Fmt(lookupString, Name));
            }
            var relatedEntity = MetadataProvider.Entity(association.To);
            var field = labelField.Substring(indexOf + 1);
            var attribute = relatedEntity.Attributes(AttributesMode.NoCollections).FirstOrDefault(a => a.Name.EqualsIc(field));
            if (validate && attribute == null) {
                throw new InvalidOperationException("field {0} not found on entity {1}".Fmt(field, relatedEntity.Name));
            }
            return new Tuple<EntityAssociation, EntityAttribute>(association, attribute);
        }

        public EntityAttribute LocateAttribute(string attributeName) {
            return
                Attributes(AttributesMode.NoCollections).FirstOrDefault(a => a.Name.EqualsIc(attributeName));
        }

        public override string ToString() {
            return string.Format("Name: {0}", Name);
        }

        protected bool Equals(EntityMetadata other) {
            return string.Equals(Name, other.Name);
        }

        public override bool Equals(object obj) {
            if (ReferenceEquals(null, obj))
                return false;
            if (ReferenceEquals(this, obj))
                return true;
            if (!(obj.GetType().IsAssignableFrom(GetType()) || GetType().IsInstanceOfType(obj)))
                return false;
            return Equals((EntityMetadata)obj);
        }

        public override int GetHashCode() {
            return (Name != null ? Name.GetHashCode() : 0);
        }

        public bool SWEntity() {
            return this.Name.EndsWith("_");
        }
    }
}