using System.Collections.Generic;
using System.Linq;
using softWrench.sW4.Metadata.Entities.Schema;
using softwrench.sW4.Shared2.Metadata.Entity.Association;

namespace softWrench.sW4.Metadata.Entities.Sliced {
    class SlicedEntityAssociation : EntityAssociation {

        public SlicedEntityAssociation(EntityAssociation innerAssociation, IEnumerable<string> slicedAttributeNames, string context = null)
           : base(innerAssociation.Qualifier, innerAssociation.To, innerAssociation.Attributes, innerAssociation.Collection, innerAssociation.Cacheable, innerAssociation.Lazy, innerAssociation.ReverseLookupAttribute, innerAssociation.IgnorePrimaryAttribute, innerAssociation.InnnerJoin) {

            var relatedEntity = MetadataProvider.Entity(innerAssociation.To);
            var slicedAttributes = new HashSet<EntityAttribute>(relatedEntity.Attributes(EntityMetadata.AttributesMode.NoCollections).Where(a => slicedAttributeNames.Contains(a.Name)).Select(s => s.ClonePrependingContext(context)));
            SlicedAttributes = slicedAttributes;
            EntityName = innerAssociation.EntityName;
        }


        public SlicedEntityAssociation(EntityAssociation innerAssociation, IEnumerable<EntityAttribute> slicedAttributes, string context = null)
            : base(innerAssociation.Qualifier, innerAssociation.To, innerAssociation.Attributes, innerAssociation.Collection, innerAssociation.Cacheable, innerAssociation.Lazy, innerAssociation.ReverseLookupAttribute, innerAssociation.IgnorePrimaryAttribute, innerAssociation.InnnerJoin) {
            if (context != null) {
                var entityAttributes = slicedAttributes.Select(slicedAttribute => slicedAttribute.ClonePrependingContext(context));
                SlicedAttributes = new HashSet<EntityAttribute>(entityAttributes);
            } else {
                SlicedAttributes = new HashSet<EntityAttribute>(slicedAttributes);
            }
            EntityName = innerAssociation.EntityName;
        }

        public ISet<EntityAttribute> SlicedAttributes {
            get; private set;
        }
    }
}
