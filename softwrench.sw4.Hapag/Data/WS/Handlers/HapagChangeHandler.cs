using System.Text;
using softWrench.sW4.Data.Persistence.Operation;
using softWrench.sW4.Data.Persistence.WS.Ism.Entities.ISMServiceEntities;

namespace softwrench.sw4.Hapag.Data.WS.Handlers {
    class HapagChangeHandler {

        public static string ParseSchemaBasedLongDescription(CrudOperationData entity, string schemaId) {
            var sb = new StringBuilder();

            sb.AppendFormat("Category: {0}\n", entity.GetAttribute("category"));
            sb.AppendFormat("Impact: {0}\n", entity.GetAttribute("#impact_label"));
            sb.AppendFormat("Urgency: {0}\n", entity.GetAttribute("urgency"));
            sb.AppendFormat("Reason: {0}\n", entity.GetAttribute("reasonforchange"));
            sb.AppendFormat("Priority: {0}\n", entity.GetAttribute("#priority_label"));
            sb.AppendFormat("Exception Reason: {0}\n", entity.GetAttribute("exceptionreason"));
            sb.AppendFormat("Exception Code: {0}\n", entity.GetAttribute("exceptioncode"));
            sb.AppendFormat("Exception Details: {0}\n", entity.GetAttribute("exceptiondetails"));
            sb.AppendFormat("Target Start Date: {0}\n", entity.GetAttribute("targstartdate"));
            sb.AppendFormat("Target Finish Date: {0}\n", entity.GetAttribute("targcompdate"));
            sb.AppendFormat("Remarks: {0}\n", entity.GetAttribute("remarks"));
            sb.AppendFormat("Infrastructure Asset: {0}\n", entity.GetAttribute("infrastructureasset"));

            return sb.ToString();
        }

        private const string GroupId = "I-SM-DE-SDM-SDM-HLC-SDM";
        private const string OrganizationId = "ITD-ESS6";
        private const string LocationId = "ESS6";
        private const string CustomerId = "HLC-00";

        public static void FillDefaultValuesNewChange(ServiceIncident webServiceObject) {
            webServiceObject.Problem.ProviderAssignedGroup.Group.GroupID = GroupId;
            webServiceObject.Problem.ProviderAssignedGroup.Group.Address.OrganizationID = OrganizationId;
            webServiceObject.Problem.ProviderAssignedGroup.Group.Address.LocationID = LocationId;
            webServiceObject.Problem.ProblemType = "SR";
            webServiceObject.Problem.System = "21030000";
            webServiceObject.Problem.CustomerID = CustomerId;
        }

        public static void FillDefaultValuesUpadteChange(ChangeRequest webServiceObject) {
            webServiceObject.Change = new Change() { CustomerID = CustomerId, Description = "@@Use Case for updating a Change record" };
        }
    }
}
