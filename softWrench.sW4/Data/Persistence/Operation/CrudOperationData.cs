using JetBrains.Annotations;
using Newtonsoft.Json;
using softWrench.sW4.Data.Entities;
using softWrench.sW4.Data.Persistence.WS.Internal;
using softWrench.sW4.Metadata.Applications;
using softWrench.sW4.Metadata.Entities;
using System.Collections.Generic;
using softwrench.sw4.api.classes.integration;
using softwrench.sW4.Shared2.Data;
using softWrench.sW4.Data.API.Response;

namespace softWrench.sW4.Data.Persistence.Operation {
    public sealed class CrudOperationData : Entity, IOperationData {

        private readonly string _userIdAttributeName;
        public ReloadMode ReloadMode { get; set; } = ReloadMode.None;

        private string _siteId;
        private string _orgId;


        public CrudOperationData([CanBeNull] string id, [NotNull] IDictionary<string, object> attributes,
            [NotNull] IDictionary<string, object> associationAttributes, EntityMetadata metadata, ApplicationMetadata applicationMetadata)
            : base(id, attributes, associationAttributes, metadata) {
            Id = id;
            EntityMetadata = metadata;
            ApplicationMetadata = applicationMetadata;
            _userIdAttributeName = metadata.Schema.UserIdAttribute.Name;
        }

        public new string Id { get; set; }

        public string SiteId {
            get { return _siteId; }
            set {
                this._siteId = value;
                if (value != null) {
                    SetAttribute("siteid", value);
                }

            }
        }

        [CanBeNull]
        public string OrgId {
            get { return _orgId; }
            set {
                this._orgId = value;
                if (value != null) {
                    SetAttribute("orgid", value);
                }
            }
        }


        public string UserId {
            get { return GetAttribute(_userIdAttributeName) == null ? null : GetAttribute(_userIdAttributeName).ToString(); }
            set { SetAttribute(_userIdAttributeName, value); }
        }
        public string Class => EntityMetadata.GetTableName();

        [JsonIgnore]
        public EntityMetadata EntityMetadata { get; set; }
        public OperationType OperationType { get; set; }
        public OperationProblemData ProblemData { get; set; }
        public AttributeHolder Holder => this;

        public ApplicationMetadata ApplicationMetadata { get; set; }

        protected override object BlankList() {
            return new List<CrudOperationData>();
        }

        public IDictionary<string, object> Fields => this;

        public string TableName => EntityMetadata.GetTableName().ToUpper();

        public bool IsDirty => UnmappedAttributes.ContainsKey("#isDirty");

        public bool IsCompositionCreation => UnmappedAttributes.ContainsKey("_iscreation");

        public CrudOperationData Clone() {
            var atributes = new Dictionary<string, object>(this);
            var assocAtributes = new Dictionary<string, object>(AssociationAttributes);
            var clone = new CrudOperationData(Id, atributes, assocAtributes, EntityMetadata, ApplicationMetadata) {
                SiteId = SiteId,
                OperationType = OperationType,
                ReloadMode = ReloadMode
            };
            return clone;
        }
    }
}
