using System;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using softWrench.sW4.Data.Entities;
using softWrench.sW4.Data.Persistence.Operation;
using softWrench.sW4.Data.Persistence.Relational.EntityRepository;
using softWrench.sW4.Data.Persistence.WS.Internal;

namespace softwrench.sw4.Hapag.Data.WS {
    public class FollowUpWorkorderHandler : BaseMaximoCustomConnector {



//        [Import]
        public EntityRepository EntityRepository {
            get; set;
        }

        private const string NotFoundLog = "{0} {1} not found. Impossible to generate FollowUp Workorder";

        public async Task<Object> CreateFollowUp(FollowUpOperationData followupData) {
            await SaveOriginator(followupData);
            //save followup workorder
            return Maximoengine.Create(followupData.CrudData);
        }

        private async Task SaveOriginator(FollowUpOperationData followUpData) {
            var maximoExecutionContext = GetContext(followUpData);
            var metadataToUse = followUpData.EntityMetadata;
            var oldEntity = await EntityRepository.Get(metadataToUse, followUpData.origrecordid);
            if (oldEntity == null) {
                throw new InvalidOperationException(String.Format(NotFoundLog, metadataToUse.Name, followUpData.origrecordid));
            }
            oldEntity.Add("HASFOLLOWUPWORK", true);
            //TODO: make it easier to generate a CrudOperationData from a DataMap...
            var json = JObject.Parse(JsonConvert.SerializeObject(oldEntity.Fields));
            //TODO: should we use a different field for the userid here?
            var originalWoData = EntityBuilder.BuildFromJson<CrudOperationData>(typeof(CrudOperationData), metadataToUse,
                maximoExecutionContext.ApplicationMetadata, json, followUpData.origrecordid);
            Maximoengine.Update(originalWoData);

        }

        public partial class FollowUpOperationData : CrudOperationDataContainer {
            [NotNull]
            public string origrecordid;

            public string origrecordclass;

        }


        public override string ApplicationName() {
            return "workorder";
        }

        public override string ActionId() {
            return "createfollowup";
        }
    }
}
