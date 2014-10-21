﻿using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using NHibernate.Hql.Ast.ANTLR;
using softWrench.sW4.Data;
using softWrench.sW4.Metadata.Entities.Connectors;
using softWrench.sW4.Metadata.Entities.Schema;
using softwrench.sW4.Shared2.Data;
using softwrench.sW4.Shared2.Metadata.Entity.Association;

namespace softWrench.sW4.Metadata.Entities.Sliced {
    public class SlicedEntityMetadata : EntityMetadata {

        private readonly string _applicationName;
        private readonly int? _fetchLimit;
        private readonly SlicedEntityMetadata _unionSchema;
        //key= base alias of the entity
        private List<SlicedEntityMetadata> _innerMetadatas = new List<SlicedEntityMetadata>();

        public string ContextAlias { get; set; }


        public SlicedEntityMetadata([NotNull] string name, [NotNull] EntitySchema schema,
               [NotNull] IEnumerable<EntityAssociation> associations, [NotNull] ConnectorParameters connectorParameters, string applicationName, IEnumerable<SlicedEntityMetadata> innerMetadatas, int? fetchLimit = 300, SlicedEntityMetadata unionSchema = null)
            : base(name, schema, associations, connectorParameters) {
            _applicationName = applicationName;
            _fetchLimit = fetchLimit;
            _innerMetadatas.AddRange(innerMetadatas);
            _unionSchema = unionSchema;
        }

        public List<SlicedEntityMetadata> InnerMetadatas {
            get { return _innerMetadatas; }
            set { _innerMetadatas = value; }
        }

        public override AttributeHolder GetAttributeHolder(IEnumerable<KeyValuePair<string, object>> keyValuePairs) {
            return new DataMap(_applicationName, keyValuePairs.ToDictionary(pair => pair.Key, pair => pair.Value),null);
        }

        public override ISet<EntityAssociation> NonListAssociations() {
            var resultList = new List<EntityAssociation>();
            var directAssociations = base.NonListAssociations();
            if (!String.IsNullOrEmpty(ContextAlias)) {
                foreach (var directAssociation in directAssociations) {
                    var innerMetadata = InnerMetadatas.FirstOrDefault(i => i.ContextAlias == directAssociation.Qualifier);
                    var association = directAssociation.CloneWithContext(ContextAlias);
                    if (innerMetadata != null) {
                        var attributes = innerMetadata.Attributes(AttributesMode.NoCollections);
                        resultList.Add(new SlicedEntityAssociation(association, attributes, ContextAlias + directAssociation.Qualifier));
                    } else {
                        resultList.Add(association);
                    }
                }
            } else {
                resultList.AddRange(directAssociations);
            }

            foreach (var innerMetadata in InnerMetadatas) {
                resultList.AddRange(innerMetadata.NonListAssociations());
            }
            return new HashSet<EntityAssociation>(resultList);
        }

        protected override IEnumerable<EntityAttribute> GetAttributesToIterate(EntityMetadata relatedEntity, EntityAssociation usedRelationship) {



            var innerMetadata = InnerMetadatas.FirstOrDefault(i => i.Name == relatedEntity.Name);
            if (innerMetadata != null) {
                return innerMetadata.Schema.Attributes;
            }

            return base.GetAttributesToIterate(relatedEntity, usedRelationship);
        }

        public override int? FetchLimit() {
            return _fetchLimit;
        }

        public SlicedEntityMetadata UnionSchema {
            get { return _unionSchema; }
        }

        public override bool HasUnion() {
            return UnionSchema != null;
        }
        public override string ToString() {
            return string.Format("Application: {0}, Name: {1}", _applicationName, Name);
        }
    }
}
