using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using softWrench.sW4.Data.Entities;
using softWrench.sW4.Metadata.Applications;

namespace softWrench.sW4.Data
{
    internal class EntitySlicer
    {
        [CanBeNull]
        public DataMap Slice([NotNull] Entity entity, ApplicationMetadata applicationMetadata)
        {
            if (entity == null) throw new ArgumentNullException("entity");
            if (applicationMetadata == null) throw new ArgumentNullException("applicationMetadata");

            var fields = new Dictionary<string, object>();

            foreach (var field in applicationMetadata.Schema.Fields)
            {
                fields[field.Attribute] = Convert.ToString(entity.Attributes[field.Attribute]);
            }

            return new DataMap(applicationMetadata.Name,applicationMetadata.IdFieldName, fields);
        }
    }
}
