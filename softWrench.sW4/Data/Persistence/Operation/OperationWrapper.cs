﻿using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using Newtonsoft.Json.Linq;
using softwrench.sw4.api.classes.integration;
using softWrench.sW4.Data.Entities;
using softWrench.sW4.Data.Persistence.WS.Internal.Constants;
using softWrench.sW4.Metadata.Applications;
using softWrench.sW4.Metadata.Entities;

namespace softWrench.sW4.Data.Persistence.Operation {
    public class OperationWrapper : IOperationWrapper {
        private const string CrudFieldNotFound = "crud field expected on json of operation {0} of entity {1}";

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

        [NotNull]
        public ICommonOperationData GetOperationData => OperationData();

        public string GetStringAttribute(string attribute) {
            return GetOperationData.Holder?.GetStringAttribute(attribute);
        }

        public WsProvider? Wsprovider {
            get; set;
        }

        private IOperationData _operationData;


        public OperationWrapper(ApplicationMetadata applicationMetadata, EntityMetadata entityMetadata, string operationName, JObject json, string id) {
            OperationName = operationName;
            JSON = json;
            _entityMetadata = entityMetadata;
            Id = id;
            ApplicationMetadata = applicationMetadata;
        }

        public OperationWrapper(IOperationData operationData, string operationName) {
            _entityMetadata = operationData.EntityMetadata;
            Id = operationData.Id;
            UserId = operationData.UserId;
            OperationName = operationName;
            _operationData = operationData;
        }

        public string OperationName { get; set; }

        public EntityMetadata EntityMetadata => _entityMetadata;


        public string Id {
            get; private set;
        }

        public EntityBuilder.EntityBuilderOptions EntityBuilderOptions { get; set; }

        public bool IsCreation => OperationConstants.CRUD_CREATE.Equals(OperationName) || Id == null;
        public IDictionary<string, object> CustomParameters { get; set; } = new Dictionary<string, object>();

        public IOperationData OperationData(Type type = null) {
            if (_operationData != null) {
                return _operationData;
            }

            var isCrud = OperationConstants.IsCrud(OperationName) || typeof(CrudOperationData) == type;
            if (isCrud || type== null) {
                var crudOperationData = EntityBuilder.BuildFromJson<CrudOperationData>(typeof(CrudOperationData), _entityMetadata, ApplicationMetadata, JSON, Id, EntityBuilderOptions);
                if (UserId != null && crudOperationData.UserId == null) {
                    crudOperationData.UserId = UserId;
                }
                if (SiteId == null) {
                    //fallback logic to picksiteid from json
                    SiteId = crudOperationData.GetStringAttribute("siteid");
                }

                crudOperationData.SiteId = SiteId;
                _operationData = crudOperationData;
                return crudOperationData;
            }


            var data = (OperationData)JSON.ToObject(type);
            data.EntityMetadata = EntityMetadata;
            if (!typeof(CrudOperationDataContainer).IsAssignableFrom(type)) {
                return data;
            }
            JToken crudFields;
            if (!JSON.TryGetValue("crud", out crudFields)) {
                throw new InvalidOperationException(string.Format(CrudFieldNotFound, OperationName, _entityMetadata.Name));
            }
            ((CrudOperationDataContainer)data).CrudData = EntityBuilder.BuildFromJson<CrudOperationData>(typeof(CrudOperationData), _entityMetadata, ApplicationMetadata, (JObject)crudFields, Id, EntityBuilderOptions);
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
