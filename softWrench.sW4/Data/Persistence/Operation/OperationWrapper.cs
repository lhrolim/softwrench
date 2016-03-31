using System;
using Newtonsoft.Json.Linq;
using softWrench.sW4.Data.Entities;
using softWrench.sW4.Metadata.Applications;
using softWrench.sW4.Metadata.Entities;

namespace softWrench.sW4.Data.Persistence.Operation {
    public class OperationWrapper {
        private const string CrudFieldNotFound = "crud field expected on json of operation {0} of entity {1}";
        private String _operationName;

        private readonly EntityMetadata _entityMetadata;

        public ApplicationMetadata ApplicationMetadata {
            get; set;
        }

        public JObject JSON {
            get; set;
        }

        public string UserId {
            get; set;
        }
        public string SiteId {
            get; set;
        }

        private IOperationData _operationData;


        public OperationWrapper(ApplicationMetadata applicationMetadata, EntityMetadata entityMetadata, string operationName, JObject json, String id) {
            _operationName = operationName;
            JSON = json;
            _entityMetadata = entityMetadata;
            Id = id;
            ApplicationMetadata = applicationMetadata;
        }

        public OperationWrapper(CrudOperationData operationData, String operationName) {
            _entityMetadata = operationData.EntityMetadata;
            Id = operationData.Id;
            UserId = operationData.UserId;
            _operationName = operationName;
            _operationData = operationData;
        }

        public string OperationName {
            get {
                return _operationName;
            }
            set {
                _operationName = value;
            }
        }

        public EntityMetadata EntityMetadata {
            get {
                return _entityMetadata;
            }
        }

        public string Id {
            get; private set;
        }

        public IOperationData OperationData(Type type = null) {
            if (_operationData != null) {
                return _operationData;
            }

            var isCrud = OperationConstants.IsCrud(_operationName) || typeof(CrudOperationData) == type;
            if (isCrud) {
                var crudOperationData = EntityBuilder.BuildFromJson<CrudOperationData>(typeof(CrudOperationData), _entityMetadata, ApplicationMetadata, JSON, Id);
                if (UserId != null && crudOperationData.UserId == null) {
                    crudOperationData.UserId = UserId;
                }
                crudOperationData.SiteId = SiteId;
                return crudOperationData;
            }


            var data = (OperationData)JSON.ToObject(type);
            data.EntityMetadata = EntityMetadata;
            if (!typeof(CrudOperationDataContainer).IsAssignableFrom(type)) {
                return data;
            }
            JToken crudFields;
            if (!JSON.TryGetValue("crud", out crudFields)) {
                throw new InvalidOperationException(String.Format(CrudFieldNotFound, OperationName, _entityMetadata.Name));
            }
            ((CrudOperationDataContainer)data).CrudData = EntityBuilder.BuildFromJson<CrudOperationData>(typeof(CrudOperationData), _entityMetadata, ApplicationMetadata, (JObject)crudFields, Id);
            if (SiteId == null) {
                //fallback logic to picksiteid from json
                SiteId = ((CrudOperationDataContainer)data).CrudData.GetStringAttribute("siteid");
            }

            ((CrudOperationDataContainer)data).CrudData.SiteId = SiteId;
            data.ApplicationMetadata = ApplicationMetadata;
            _operationData = data;
            return data;
        }
    }
}
