using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using softWrench.sW4.Data.Relationship.Composition;
using softWrench.sW4.Metadata;
using softWrench.sW4.Metadata.Applications;
using softWrench.sW4.Metadata.Entities.Sliced;
using softwrench.sW4.Shared2.Data;
using softwrench.sW4.Shared2.Metadata.Applications.Relationships.Compositions;

namespace softWrench.sW4.Data.Persistence.Relational.Collection {

    public class CollectionResolverParameters {

        public ApplicationMetadata ApplicationMetadata { get; set; }

        public IDictionary<string, long?> RowstampMap { get; set; }
        public IEnumerable<AttributeHolder> ParentEntities { get; set; }

        public SlicedEntityMetadata SlicedEntity { get; set; }

        public IDictionary<string, ApplicationCompositionSchema> CompositionSchemas { get; set; }

        public CollectionResolverParameters(IDictionary<string, ApplicationCompositionSchema> compositionSchemas, SlicedEntityMetadata slicedEntity, IEnumerable<AttributeHolder> parentEntities) {
            SlicedEntity = slicedEntity;
            CompositionSchemas = compositionSchemas;
            ParentEntities = parentEntities;
        }


        public CollectionResolverParameters(ApplicationMetadata applicationMetadata, IEnumerable<AttributeHolder> parentEntities, IDictionary<string, long?> rowstampMap) {
            ApplicationMetadata = applicationMetadata;
            RowstampMap = rowstampMap;
            ParentEntities = parentEntities;
            CompositionSchemas = CompositionBuilder.InitializeCompositionSchemas(ApplicationMetadata.Schema);
            SlicedEntity = MetadataProvider.SlicedEntityMetadata(ApplicationMetadata);
        }




    }
}
