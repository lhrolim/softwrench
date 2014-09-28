using System;
using Newtonsoft.Json.Linq;
using softWrench.sW4.Data.Entities;
using softWrench.sW4.Metadata.Applications;
using softWrench.sW4.Metadata.Entities;

namespace softWrench.sW4.Data.Persistence.Operation {
    public class OperationWrapper {
        private const string CrudFieldNotFound = "crud field expected on json of operation {0} of entity {1}";
        private String _operationName;
        private readonly JObject _json;
        private readonly EntityMetadata _entityMetadata;
        private readonly ApplicationMetadata _applicationMetadata;
        private String _id;
        private IOperationData _operationData;


        public OperationWrapper(ApplicationMetadata applicationMetadata, EntityMetadata entityMetadata, string operationName, JObject json, String id) {
            _operationName = operationName;
            _json = json;
            _entityMetadata = entityMetadata;
            _id = id;
            _applicationMetadata = applicationMetadata;
        }

        public OperationWrapper(CrudOperationData operationData, String operationName) {
            _entityMetadata = operationData.EntityMetadata;
            _id = operationData.Id;
            _operationName = operationName;
            _operationData = operationData;
        }

        public string OperationName {
            get { return _operationName; }
            set { _operationName = value; }
        }

        public EntityMetadata EntityMetadata {
            get { return _entityMetadata; }
        }

        public string Id {
            get { return _id; }
        }

        public IOperationData OperationData(Type type) {
            if (_operationData != null) {
                return _operationData;
            }

            bool isCrud = OperationConstants.IsCrud(_operationName) || typeof(CrudOperationData) == type;
            if (isCrud) {
                return EntityBuilder.BuildFromJson<CrudOperationData>(typeof(CrudOperationData), _entityMetadata, _applicationMetadata, _json, _id);
            }
            var data = (OperationData)_json.ToObject(type);
            data.EntityMetadata = EntityMetadata;
            if (!typeof(CrudOperationDataContainer).IsAssignableFrom(type)) {
                return data;
            }
            JToken crudFields;
            if (!_json.TryGetValue("crud", out crudFields)) {
                throw new InvalidOperationException(String.Format(CrudFieldNotFound, OperationName, _entityMetadata.Name));
            }
            ((CrudOperationDataContainer)data).CrudData = EntityBuilder.BuildFromJson<CrudOperationData>(typeof(CrudOperationData), _entityMetadata, _applicationMetadata, (JObject)crudFields, _id);
            data.ApplicationMetadata = _applicationMetadata;
            _operationData = data;
            return data;
        }
    }
}
