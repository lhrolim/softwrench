using System;
using softwrench.sW4.Shared.Metadata.Applications.Schema;

namespace softwrench.sW4.Shared.Metadata.Applications.Relationships.Compositions {
    public class ApplicationCompositionCollectionSchema : ApplicationCompositionSchema {

        private readonly CompositionCollectionProperties _collectionProperties;

        public Boolean FetchFromServer { get; set; }


        public ApplicationCompositionCollectionSchema(bool inline, string detailSchema,
            CompositionCollectionProperties collectionProperties, SchemaMode renderMode, CompositionFieldRenderer renderer) :
            base(inline, detailSchema, renderMode, renderer) {
            _collectionProperties = collectionProperties;
        }

        public CompositionCollectionProperties CollectionProperties {
            get { return _collectionProperties; }
        }

        public Boolean AllowInsertion {
            get { return CollectionProperties.AllowInsertion; }
        }
        public Boolean AllowUpdate {
            get { return CollectionProperties.AllowUpdate; }
        }
        public Boolean AllowRemoval {
            get { return CollectionProperties.AllowRemoval; }
        }
    }
}
