﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using softWrench.sW4.Metadata.Entities.Schema;
using softwrench.sW4.Shared2.Metadata.Entity.Association;

namespace softWrench.sW4.Metadata.Entities.Sliced {
    class SlicedEntityAssociation : EntityAssociation {


        public SlicedEntityAssociation(EntityAssociation innerAssociation, IEnumerable<EntityAttribute> slicedAttributes, string context = null)
            : base(innerAssociation.Qualifier, innerAssociation.To, innerAssociation.Attributes, innerAssociation.Collection, innerAssociation.ReverseLookupAttribute,innerAssociation.IgnorePrimaryAttribute)
        {
            if (context != null) {
                SlicedAttributes = slicedAttributes.Select(slicedAttribute => slicedAttribute.ClonePrependingContext(context)).ToList();
            } else {
                SlicedAttributes = slicedAttributes;
            }
            EntityName = innerAssociation.EntityName;
        }

        public IEnumerable<EntityAttribute> SlicedAttributes { get; set; }
    }
}
