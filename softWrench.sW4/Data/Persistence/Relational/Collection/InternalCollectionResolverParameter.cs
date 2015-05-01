using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using softWrench.sW4.Metadata.Entities.Sliced;
using softWrench.sW4.Security.Context;
using softwrench.sW4.Shared2.Data;
using softwrench.sW4.Shared2.Metadata.Applications.Relationships.Compositions;
using softwrench.sW4.Shared2.Metadata.Entity.Association;
using softWrench.sW4.Util;

namespace softWrench.sW4.Data.Persistence.Relational.Collection {
    public class InternalCollectionResolverParameter {

        internal SlicedEntityMetadata EntityMetadata { get; set; }
        internal EntityAssociation CollectionAssociation { get; set; }
        internal IDictionary<string, ApplicationCompositionSchema> CompositionSchemas { private get; set; }
        internal IEnumerable<AttributeHolder> EntitiesList { get; set; }
        internal ContextHolder Ctx { get; set; }
        internal IDictionary<string, EntityRepository.EntityRepository.SearchEntityResult> Results { get; set; }
        internal long? Rowstamp { get; set; }

        


        public ApplicationCompositionCollectionSchema CompositionSchema {
            get {
                var applicationCompositionSchema = CompositionSchemas[CollectionAssociation.Qualifier]
                    as ApplicationCompositionCollectionSchema;

                if (applicationCompositionSchema == null) {
                    throw ExceptionUtil.InvalidOperation("collection schema {0} not found", CollectionAssociation.Qualifier);
                }
                return applicationCompositionSchema;
            }
        }

        
    }
}
