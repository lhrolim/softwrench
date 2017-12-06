using Newtonsoft.Json.Linq;
using softWrench.sW4.Data.Batches;
using softWrench.sW4.Data.Entities;
using softWrench.sW4.Data.Persistence.Operation;
using softWrench.sW4.Metadata.Applications;

namespace softwrench.sw4.umc.classes.com.cts.umc.connector {
    public class UmcPmBatchConverter : ABatchSubmissionConverter {
        public override string BatchProperty => "#pmlist_";

        public override string ApplicationName() {
            return "pm";
        }

        public override string ClientFilter() {
            return null;
        }

        public override string SchemaId() {
            return null;
        }

        public override OperationWrapper Convert(JObject row, ApplicationMetadata applicationMetadata) {
            var crudOperationData = EntityBuilder.BuildFromJson<CrudOperationData>(typeof(CrudOperationData), EntityMetadata, applicationMetadata, row);
            //needed because the unmapped field wouldn´t be populated automatically
            crudOperationData.SetAttribute("DESCRIPTION_LONGDESCRIPTION",crudOperationData.GetUnMappedAttribute("DESCRIPTION_LONGDESCRIPTION"));
            return new OperationWrapper(crudOperationData, OperationConstants.CRUD_UPDATE);
        }
    }
}
