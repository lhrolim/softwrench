using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using softWrench.sW4.Data.Offline;
using softWrench.sW4.Data.Sync;
using softWrench.sW4.Util;

namespace softWrench.sW4.Metadata.Entities.Schema {
    public class EntitySchema {
        private EntityAttribute FindIdAttribute(IEnumerable<EntityAttribute> attributes, string idAttributeName) {
            return attributes.FirstWithException(a => a.Name == idAttributeName, "Id attribute {0} not found on entity {1}", idAttributeName, EntityName);
        }

        private readonly ISet<EntityAttribute> _attributes;
        private readonly Lazy<EntityAttribute> _idAttribute;
        private readonly EntityAttribute _rowstampAttribute;

        public String ParentEntity { get; set; }
        public String WhereClause { get; set; }

        public Boolean ExcludeUndeclaredAttributes { get; set; }
        public Boolean ExcludeUndeclaredAssociations { get; set; }

        public string EntityName { get; set; }

        public EntitySchema(string entityName, IEnumerable<EntityAttribute> attributes, [NotNull] string idAttributeName, Boolean excludeUndeclaredAttributes,
             Boolean excludeUndeclaredAssociations, string whereClause, string parentEntity, bool includeRowstamp = true) {
            if (idAttributeName == null) throw new ArgumentNullException("idAttributeName");
            EntityName = entityName;
            _attributes = attributes == null ? new HashSet<EntityAttribute>() : new HashSet<EntityAttribute>(attributes);
            if (includeRowstamp) {
                _rowstampAttribute = RowStampUtil.RowstampEntityAttribute();
                _attributes.Add(_rowstampAttribute);
            }
            _idAttribute = new Lazy<EntityAttribute>(() => FindIdAttribute(_attributes, idAttributeName));
            ExcludeUndeclaredAttributes = excludeUndeclaredAttributes;
            ExcludeUndeclaredAssociations = excludeUndeclaredAssociations;
            ParentEntity = parentEntity;
            WhereClause = whereClause;
            if (ParentEntity != null) {

            }
        }

        [NotNull]
        public ISet<EntityAttribute> Attributes {
            get { return _attributes; }
        }

        [NotNull]
        public EntityAttribute IdAttribute {
            get { return _idAttribute.Value; }
        }

        public EntityAttribute RowstampAttribute {
            get { return _rowstampAttribute; }
        }
    }
}
