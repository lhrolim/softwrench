﻿using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using NHibernate.Hql.Ast.ANTLR;
using softWrench.sW4.Data;
using softWrench.sW4.Metadata.Entities.Connectors;
using softWrench.sW4.Metadata.Entities.Schema;
using softwrench.sW4.Shared2.Data;
using softwrench.sW4.Shared2.Metadata.Applications.Schema;
using softwrench.sW4.Shared2.Metadata.Entity.Association;

namespace softWrench.sW4.Metadata.Entities.Sliced {
    public class SlicedEntityMetadata : EntityMetadata {

        private readonly ApplicationSchemaDefinition _appSchema;
        private readonly int? _fetchLimit;
        private readonly SlicedEntityMetadata _unionSchema;
        //key= base alias of the entity
        private List<SlicedEntityMetadata> _innerMetadatas = new List<SlicedEntityMetadata>();

        public string ContextAlias { get; set; }


        public SlicedEntityMetadata([NotNull] string name, [NotNull] EntitySchema schema,
               [NotNull] IEnumerable<EntityAssociation> associations, [NotNull] ConnectorParameters connectorParameters, ApplicationSchemaDefinition appSchema,
            IEnumerable<SlicedEntityMetadata> innerMetadatas, int? fetchLimit = 300, SlicedEntityMetadata unionSchema = null)
            : base(name, schema, associations, connectorParameters) {
            _appSchema = appSchema;
            _fetchLimit = fetchLimit;
            _innerMetadatas.AddRange(innerMetadatas);
            _unionSchema = unionSchema;
        }

        public List<SlicedEntityMetadata> InnerMetadatas {
            get { return _innerMetadatas; }
            set { _innerMetadatas = value; }
        }

        public override AttributeHolder GetAttributeHolder(IEnumerable<KeyValuePair<string, object>> keyValuePairs) {
            return new DataMap(ApplicationName,IdFieldName, keyValuePairs.ToDictionary(pair => pair.Key, pair => pair.Value));
        }

        public override ISet<EntityAssociation> NonListAssociations() {
            var resultList = new List<EntityAssociation>();
            var directAssociations = base.NonListAssociations();
            if (!directAssociations.Any() && !InnerMetadatas.Any()) {
                return new HashSet<EntityAssociation>(resultList);
            }

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
                var nonListAssociations = innerMetadata.NonListAssociations();
                if (nonListAssociations.Any()) {
                    //to make debug easier
                    if (ContextAlias != null) {
                        foreach (var nonListAssociation in nonListAssociations) {
                            if (!nonListAssociation.Qualifier.StartsWith(ContextAlias)) {
                                //this is a workaround used to handle 3 or more level relationships (ex: asset_aucisowner_person_displayname)
                                nonListAssociation.Qualifier = ContextAlias + nonListAssociation.Qualifier;
                                nonListAssociation.EntityName = ContextAlias + nonListAssociation.EntityName;
                                if (nonListAssociation is SlicedEntityAssociation) {
                                    ((SlicedEntityAssociation)nonListAssociation).RefreshSlicedAttributes(ContextAlias);
                                }
                            }
                        }
                    }
                    resultList.AddRange(nonListAssociations);
                }
            }
            return new HashSet<EntityAssociation>(resultList);
        }

        protected override IEnumerable<EntityAttribute> GetAttributesToIterate(EntityMetadata relatedEntity, EntityAssociation usedRelationship) {
            SlicedEntityMetadata innerMetadata;
            if (usedRelationship.Qualifier != null) {
                innerMetadata = InnerMetadatas.FirstOrDefault(i => i.ContextAlias == usedRelationship.Qualifier);
            } else {
                innerMetadata = InnerMetadatas.FirstOrDefault(i => i.Name == relatedEntity.Name);
            }
            if (innerMetadata != null) {
                return innerMetadata.Schema.Attributes;
            }

            return base.GetAttributesToIterate(relatedEntity, usedRelationship);
        }

        public override int? FetchLimit() {
            return _fetchLimit;
        }

        public string ApplicationName {
            get { return _appSchema.ApplicationName; }
        }

        public ApplicationSchemaDefinition AppSchema {
            get { return _appSchema; }
        }

        public SlicedEntityMetadata UnionSchema {
            get { return _unionSchema; }
        }

        public override bool HasUnion() {
            return UnionSchema != null;
        }
        public override string ToString() {
            return string.Format("Context: {0}, Name: {1}", ContextAlias, Name);
        }


    }
}
