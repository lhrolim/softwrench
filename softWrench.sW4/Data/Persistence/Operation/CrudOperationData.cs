﻿using System.Collections.Generic;
using JetBrains.Annotations;
using Newtonsoft.Json;
using softWrench.sW4.Data.Entities;
using softWrench.sW4.Data.Persistence.WS.Internal;
using softWrench.sW4.Metadata.Applications;
using softWrench.sW4.Metadata.Entities;

namespace softWrench.sW4.Data.Persistence.Operation {
    public sealed class CrudOperationData : Entity, IOperationData {

        public CrudOperationData([CanBeNull] string id, [NotNull] IDictionary<string, object> attributes,
            [NotNull] IDictionary<string, object> associationAttributes, EntityMetadata metadata, ApplicationMetadata applicationMetadata)
            : base(id, attributes, associationAttributes, metadata) {
            Id = id;
            EntityMetadata = metadata;
            ApplicationMetadata = applicationMetadata;
        }

        public new string Id { get; set; }
        public string Class { get { return EntityMetadata.GetTableName(); }  }
        [JsonIgnore]
        public EntityMetadata EntityMetadata { get; set; }
        public OperationType OperationType { get; set; }
        [JsonIgnore]
        public ApplicationMetadata ApplicationMetadata { get; set; }

        protected override object BlankList() {
            return new List<CrudOperationData>();
        }



        public string TableName {
            get { return EntityMetadata.GetTableName().ToUpper(); }
        }

    }
}
