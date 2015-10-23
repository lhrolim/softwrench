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
        private readonly string _name;
        private readonly EntitySchema _schema;
        private readonly ISet<EntityAssociation> _associations;
        private readonly ConnectorParameters _connectorParameters;

        private IDictionary<QueryCacheKey, string> _queryStringCache = new Dictionary<QueryCacheKey, string>();

        private readonly Lazy<IEnumerable<EntityAttribute>> _nonCollectionrelationshipAttributes;

        private readonly Lazy<IEnumerable<EntityAttribute>> _relationshipAttributes;

        public EntityMetadata([NotNull] string name, [NotNull] EntitySchema schema, [NotNull] IEnumerable<EntityAssociation> associations,
            [NotNull] ConnectorParameters connectorParameters) {

            Validate.NotNull(name, "name");
            Validate.NotNull(schema, "schema");
            Validate.NotNull(associations, "associations");
            Validate.NotNull(connectorParameters, "connectorParameters");

            _name = name;
            _schema = schema;
            _associations = new HashSet<EntityAssociation>(associations);
            _connectorParameters = connectorParameters;
            _relationshipAttributes = new Lazy<IEnumerable<EntityAttribute>>(AddRelationshipAttributes);
            _nonCollectionrelationshipAttributes = new Lazy<IEnumerable<EntityAttribute>>(AddNonCollectionRelationshipAttributes);
        }

        internal void MergeWithParent() {
            var parent = MetadataProvider.Entity(_schema.ParentEntity);
            var parentAssociations = parent.Associations;
            var parentAttributes = parent.Schema.Attributes;
            var parentConnectors = parent.ConnectorParameters;

            if (!Schema.ExcludeUndeclaredAssociations) {
                foreach (var parentAssociation in parentAssociations) {
                    _associations.Add(parentAssociation);
                }
            }

            if (!_connectorParameters.ExcludeUndeclared) {
                var thisParameters = _connectorParameters.Parameters;
                foreach (var parentParameter in parentConnectors.Parameters) {
                    if (!thisParameters.ContainsKey(parentParameter.Key)) {
                        thisParameters.Add(parentParameter.Key, parentParameter.Value);
                    }
                }
            }

            if (!_schema.ExcludeUndeclaredAttributes) {
                foreach (var parentAttribute in parentAttributes) {
                    Schema.Attributes.Add(parentAttribute);
                }
            }

            if (!HasWhereClause && parent.HasWhereClause) {
                _schema.WhereClause = parent.Schema.WhereClause;
            }
        }


        [NotNull]
        public string Name {
            get {
                return _name;
            }
        }

        [NotNull]
        public EntitySchema Schema {
            get {
                return _schema;
            }
        }


        [NotNull]
        public ConnectorParameters ConnectorParameters {
            get {
                return _connectorParameters;
            }
        }

        [NotNull]
        public ISet<EntityAssociation> Associations {
            get {
                return _associations;
            }
        }

        public ISet<EntityAssociation> ReverseAssociations() {
            return new HashSet<EntityAssociation>(_associations.Where(entityAssociation => (entityAssociation.Reverse)));
        }

        public ISet<EntityAssociation> ListAssociations() {
            return new HashSet<EntityAssociation>(_associations.Where(entityAssociation => (entityAssociation.Collection)));
        }

        public virtual ISet<EntityAssociation> NonListAssociations() {
            return new HashSet<EntityAssociation>(_associations.Where(entityAssociation => (!entityAssociation.Collection && !entityAssociation.Reverse)));
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

        public IDictionary<QueryCacheKey, string> QueryStringCache {
            get {
                return _queryStringCache;
            }
            set {
                _queryStringCache = value;
            }
        }

        public IEnumerable<EntityAttribute> Attributes(AttributesMode includeCollections) {
            var entityAttributes = _schema.Attributes.ToList();
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

        public EntityMetadata RelatedEntityMetadata(string relationshipName) {
            relationshipName = softWrench.sW4.Util.EntityUtil.GetRelationshipName(relationshipName);
            var association = Associations.FirstOrDefault(r => r.Qualifier == relationshipName);
            return association != null ? MetadataProvider.Entity(association.To) : null;
        }

        public string GetTableName() {
            string tableName;
            var value = !ConnectorParameters.Parameters.TryGetValue(ConnectorTableName, out tableName) ? Name : tableName;
            return value;
        }

        public Boolean HasParent {
            get {
                return _schema.ParentEntity != null;
            }
        }

        public Boolean HasWhereClause {
            get {
                return !string.IsNullOrEmpty(_schema.WhereClause);
            }
        }

        public string IdFieldName {
            get {
                return Schema.IdAttribute.Name;
            }
        }

        public string UserIdFieldName {
            get {
                return Schema.UserIdAttribute.Name;
            }
        }

        public string WhereClause {
            get {
                return _schema.WhereClause;
            }
            set {
                _schema.WhereClause = value;
            }
        }

        public Tuple<EntityAssociation,EntityAttribute> LocateAssociationByLabelField(string labelField) {
            var indexOf = labelField.IndexOf(".", StringComparison.Ordinal);
            var firstPart = labelField.Substring(0, indexOf);
            var lookupString = firstPart.EndsWith("_") ? firstPart : firstPart + "_";
            if (char.IsNumber(lookupString[0])) {
                lookupString = lookupString.Substring(1);
            }
            var entityAssociations = Associations;
            var association = entityAssociations.FirstOrDefault(a => a.Qualifier.EqualsIc(lookupString));

            if (association == null) {
                throw new MetadataException("association {0} cannot be located on entity {1}".Fmt(lookupString, Name));
            }
            var relatedEntity = MetadataProvider.Entity(association.To);
            var field = labelField.Substring(indexOf + 1);

            var attribute = relatedEntity.Attributes(AttributesMode.NoCollections)
                .FirstWithException(a => a.Name.EqualsIc(field), "field {0} not found on entity {1}", field,
                    relatedEntity.Name);
            return new Tuple<EntityAssociation, EntityAttribute>(association,attribute);

        }

        public EntityAttribute LocateAttribute(string attributeName) {
            return
                Attributes(AttributesMode.NoCollections).FirstOrDefault(a => a.Name.EqualsIc(attributeName));
        }

        public override string ToString() {
            return string.Format("Name: {0}", _name);
        }

        protected bool Equals(EntityMetadata other) {
            return string.Equals(_name, other._name);
        }

        public override bool Equals(object obj) {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (!(obj.GetType().IsAssignableFrom(GetType()) || GetType().IsInstanceOfType(obj))) return false;
            return Equals((EntityMetadata)obj);
        }

        public override int GetHashCode() {
            return (_name != null ? _name.GetHashCode() : 0);
        }

        public bool SWEntity() {
            return this._name.StartsWith("_");
        }
    }
}