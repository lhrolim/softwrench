using System;
using System.Collections.Generic;
using softwrench.sW4.Shared2.Metadata.Applications.Schema;
using softwrench.sw4.Shared2.Metadata.Applications.Schema;

namespace softwrench.sW4.Shared2.Metadata.Applications.Relationships.Compositions {
    public class ApplicationCompositionCollectionSchema : ApplicationCompositionSchema {

        private readonly CompositionCollectionProperties _collectionProperties;
        
        /// <summary>
        /// true if the detail schema has more data then the list schema, forcing an extra fetch upon expanding the composition
        /// </summary>
        public Boolean FetchFromServer { get; set; }

        

        public ApplicationCompositionCollectionSchema(bool inline, string detailSchema, CompositionCollectionProperties collectionProperties,
            SchemaMode renderMode, CompositionFieldRenderer renderer, string printSchema, string dependantfield,FetchType fetchType, ISet<ApplicationEvent> events = null) :
            base(inline, detailSchema, renderMode, renderer, printSchema, dependantfield,fetchType, events) {
            _collectionProperties = collectionProperties;
        }


        public string PrefilterFunction { get { return _collectionProperties.PrefilterFunction; } }

        public CompositionCollectionProperties CollectionProperties {
            get { return _collectionProperties; }
        }

        public String AllowInsertion {
            get { return CollectionProperties.AllowInsertion; }
        }
        public String AllowUpdate {
            get { return CollectionProperties.AllowUpdate; }
        }
        public String AllowRemoval {
            get { return CollectionProperties.AllowRemoval; }
        }
    }
}
