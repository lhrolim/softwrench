using System;
using System.Globalization;
using JetBrains.Annotations;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using softWrench.sW4.Data.Entities;
using softWrench.sW4.Data.Persistence.Operation;
using softWrench.sW4.Data.Persistence.Relational;
using softWrench.sW4.Data.Persistence.WS.Internal;
using softWrench.sW4.Metadata;
using softWrench.sW4.Metadata.Entities;
using w = softWrench.sW4.Data.Persistence.WS.Internal.WsUtil;

namespace softWrench.sW4.Data.Persistence.Dataset.Commons.Workorder {
    class FollowUpWorkorderHandler : BaseMaximoCustomConnector {
        private static readonly EntityRepository EntityRepository = new EntityRepository();

        private const string NotFoundLog = "{0} {1} not found. Impossible to generate FollowUp Workorder";

        public object CreateFollowUp(FollowUpOperationData followupData) {
            SaveOriginator(followupData);
            //save followup workorder
            return Maximoengine.Create(followupData.CrudData);
        }

        private void SaveOriginator(FollowUpOperationData followUpData) {
            var maximoExecutionContext = GetContext(followUpData);
            var metadataToUse = followUpData.EntityMetadata;
            var oldEntity = (DataMap)EntityRepository.Get(metadataToUse, followUpData.origrecordid);
            if (oldEntity == null) {
                throw new InvalidOperationException(String.Format(NotFoundLog, metadataToUse.Name, followUpData.origrecordid));
            }
            oldEntity.Attributes.Add("HASFOLLOWUPWORK", true);
            //TODO: make it easier to generate a CrudOperationData from a DataMap...
            var json = JObject.Parse(JsonConvert.SerializeObject(oldEntity.Fields));
            var originalWoData = EntityBuilder.BuildFromJson<CrudOperationData>(typeof(CrudOperationData), metadataToUse,
                maximoExecutionContext.ApplicationMetadata,json, followUpData.origrecordid);
            Maximoengine.Update(originalWoData);

        }

        public partial class FollowUpOperationData : CrudOperationDataContainer {
            [NotNull]
            public string origrecordid;

            public string origrecordclass;

        }


    }
}
