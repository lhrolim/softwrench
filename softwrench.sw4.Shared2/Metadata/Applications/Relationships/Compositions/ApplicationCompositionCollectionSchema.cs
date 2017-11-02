using System;
using System.Collections.Generic;
using softwrench.sW4.Shared2.Metadata.Applications.Schema;
using softwrench.sw4.Shared2.Metadata.Applications.Schema;

namespace softwrench.sW4.Shared2.Metadata.Applications.Relationships.Compositions {
    public class ApplicationCompositionCollectionSchema : ApplicationCompositionSchema {

        private CompositionCollectionProperties _collectionProperties;

        /// <summary>
        /// true if the detail schema has more data then the list schema, forcing an extra fetch upon expanding the composition
        /// </summary>
        public Boolean FetchFromServer {
            get; set;
        }

     


        public ApplicationCompositionCollectionSchema(bool inline, bool isSWDB, string detailSchema, string detailOutputSchema, CompositionCollectionProperties collectionProperties,
            SchemaMode renderMode, CompositionFieldRenderer renderer, string printSchema, string dependantfield, FetchType fetchType, ISet<ApplicationEvent> events = null) :
            base(inline, isSWDB, detailSchema, detailOutputSchema,renderMode, renderer, printSchema, dependantfield, fetchType, events) {
            _collectionProperties = collectionProperties;
        }


        public string PrefilterFunction => _collectionProperties.PrefilterFunction;

        public CompositionCollectionProperties CollectionProperties {
            get {
                return _collectionProperties;
            }
            set {
                _collectionProperties = value;
            }
        }

        public string AllowInsertion => CollectionProperties.AllowInsertion;

        public string AllowUpdate => CollectionProperties.AllowUpdate;

        public string AllowRemoval => CollectionProperties.AllowRemoval;

        public override object Clone() {
            var props = new CompositionCollectionProperties(AllowRemoval, AllowInsertion, AllowUpdate, CollectionProperties.ListSchema, CollectionProperties.AutoCommit, CollectionProperties.HideExistingData, CollectionProperties.OrderByField, CollectionProperties.PrefilterFunction);

            return new ApplicationCompositionCollectionSchema(INLINE, IsSwDB, DetailSchema,DetailOutputSchema, props, RenderMode, Renderer, PrintSchema, OriginalDependantfields, FetchType, OriginalEvents);
        }
    }
}
