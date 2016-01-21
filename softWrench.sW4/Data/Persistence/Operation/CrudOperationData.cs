using JetBrains.Annotations;
using Newtonsoft.Json;
using softWrench.sW4.Data.Entities;
using softWrench.sW4.Data.Persistence.WS.Internal;
using softWrench.sW4.Metadata.Applications;
using softWrench.sW4.Metadata.Entities;
using System.Collections.Generic;

namespace softWrench.sW4.Data.Persistence.Operation {
    public sealed class CrudOperationData : Entity, IOperationData {

        private readonly string _userIdAttributeName;

        public CrudOperationData([CanBeNull] string id, [NotNull] IDictionary<string, object> attributes,
            [NotNull] IDictionary<string, object> associationAttributes, EntityMetadata metadata, ApplicationMetadata applicationMetadata)
            : base(id, attributes, associationAttributes, metadata) {
            Id = id;
            EntityMetadata = metadata;
            ApplicationMetadata = applicationMetadata;
            _userIdAttributeName = metadata.Schema.UserIdAttribute.Name;
        }

        public new string Id { get; set; }

        public string SiteId {get; set;}


        public string UserId {
            get { return GetAttribute(_userIdAttributeName) == null ? null : GetAttribute(_userIdAttributeName).ToString(); }
            set { SetAttribute(_userIdAttributeName, value); }
        }
        public string Class { get { return EntityMetadata.GetTableName(); } }
        
        [JsonIgnore]
        public EntityMetadata EntityMetadata { get; set; }
        public OperationType OperationType { get; set; }

        public ApplicationMetadata ApplicationMetadata { get; set; }

        protected override object BlankList() {
            return new List<CrudOperationData>();
        }

        public IDictionary<string, object> Fields {
            get { return Attributes; }
        }

        public string TableName {
            get { return EntityMetadata.GetTableName().ToUpper(); }
        }

    }
}
