using System;
using System.Collections.Generic;
using System.Linq;
using cts.commons.portable.Util;
using JetBrains.Annotations;
using softWrench.sW4.Data.Offline;
using softWrench.sW4.Util;

namespace softWrench.sW4.Metadata.Entities.Schema {
    public class EntitySchema {
        private EntityAttribute FindIdAttribute(IEnumerable<EntityAttribute> attributes, string idAttributeName) {
            return attributes.FirstWithException(a => a.Name.EqualsIc(idAttributeName), "Id attribute {0} not found on entity {1}", idAttributeName, EntityName);
        }

        private EntityAttribute FindSiteIdAttribute(IEnumerable<EntityAttribute> attributes) {
            return attributes.FirstOrDefault(a => a.Name.EqualsIc("siteid"));
        }

        private EntityAttribute FindUserIdAttribute(IEnumerable<EntityAttribute> attributes, string userIdAttribute, string idAttribute) {
            return attributes.FirstWithException(a => a.Name.EqualsIc(userIdAttribute), "User Id attribute {0} not found on entity {1}", userIdAttribute, EntityName);
        }

        private readonly ISet<EntityAttribute> _attributes;
        private readonly Lazy<EntityAttribute> _idAttribute;
        private readonly Lazy<EntityAttribute> _siteIdAttribute;
        private readonly Lazy<EntityAttribute> _userIdAttribute;
        private readonly EntityAttribute _rowstampAttribute;

        public string ParentEntity {
            get; set;
        }
        public string WhereClause {
            get; set;
        }

        public Boolean ExcludeUndeclaredAttributes {
            get; set;
        }
        public Boolean ExcludeUndeclaredAssociations {
            get; set;
        }

        public string EntityName {
            get; set;
        }

        /// <summary>
        /// A type that holds the map for this entity schema, common for SWDB mappings
        /// </summary>
        public Type MappingType {
            get; set;
        }

        public EntitySchema(string entityName, IEnumerable<EntityAttribute> attributes, [NotNull] string idAttributeName, [NotNull] string userIdAttributeName, Boolean excludeUndeclaredAttributes,
             Boolean excludeUndeclaredAssociations, string whereClause, string parentEntity, Type mappingType, bool includeRowstamp = true) {
            if (idAttributeName == null) throw new ArgumentNullException("idAttributeName");
            EntityName = entityName;
            _attributes = attributes == null ? new HashSet<EntityAttribute>() : new HashSet<EntityAttribute>(attributes);
            if (includeRowstamp) {
                _rowstampAttribute = RowStampUtil.RowstampEntityAttribute();
                _attributes.Add(_rowstampAttribute);
            }
            _idAttribute = new Lazy<EntityAttribute>(() => FindIdAttribute(_attributes, idAttributeName));
            _userIdAttribute = new Lazy<EntityAttribute>(() => FindUserIdAttribute(_attributes, userIdAttributeName, idAttributeName));
            _siteIdAttribute = new Lazy<EntityAttribute>(() => FindSiteIdAttribute(_attributes));
            ExcludeUndeclaredAttributes = excludeUndeclaredAttributes;
            ExcludeUndeclaredAssociations = excludeUndeclaredAssociations;
            ParentEntity = parentEntity;
            WhereClause = whereClause;
            if (ParentEntity != null) {

            }
            MappingType = mappingType;
        }

        [NotNull]
        public ISet<EntityAttribute> Attributes {
            get {
                return _attributes;
            }
        }

        [NotNull]
        public EntityAttribute IdAttribute {
            get {
                return _idAttribute.Value;
            }
        }

        public EntityAttribute UserIdAttribute {
            get {
                return _userIdAttribute.Value;
            }
        }


        public EntityAttribute SiteIdAttribute {
            get {
                return _siteIdAttribute.Value;
            }
        }


        public EntityAttribute RowstampAttribute {
            get {
                return _rowstampAttribute;
            }
        }
    }
}
