using System;
using System.Text;
using softWrench.sW4.Data.Persistence.Operation;
using softWrench.sW4.Data.Persistence.WS.Ism.Entities.ISMServiceEntities;

namespace softWrench.sW4.Data.Persistence.WS.Commons {
    class HapagChangeHandler {

        public static void CheckSR4ChangeGroupID(CrudOperationData entity, ServiceIncident webServiceObject) {
            // If SR is a SR4Change...
            var templateId = entity.GetAttribute("templateid") as string; 
            if (!String.IsNullOrWhiteSpace(templateId) &&
                (templateId.Equals("HLCDECHG") || templateId.Equals("HLCDECHTUI") || templateId.Equals("HLCDECHSSO"))) {

                //...update the Group ID
                webServiceObject.Problem.ProviderAssignedGroup.Group.GroupID = GroupId;
                webServiceObject.Problem.ProviderAssignedGroup.Group.Address.OrganizationID = OrganizationId;
                webServiceObject.Problem.ProviderAssignedGroup.Group.Address.LocationID = LocationId;
            }
        }

        public static string ParseSchemaBasedLongDescription(CrudOperationData entity, string schemaId) {
            var sb = new StringBuilder();

            sb.AppendFormat("Category: {0}\n", entity.GetAttribute("category"));
            sb.AppendFormat("Impact: {0}\n", entity.GetAttribute("#impact_label"));
            sb.AppendFormat("Urgency: {0}\n", entity.GetAttribute("urgency"));            
            sb.AppendFormat("Reason: {0}\n", entity.GetAttribute("reasonforchange"));
            sb.AppendFormat("Reason Details: {0}\n", entity.GetAttribute("reasondetails"));
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
            /*
            var address = new Address() { OrganizationID = OrganizationId, LocationID = LocationId };
            var group = new Group1() { GroupID = GroupId, Address = address };
            var assignedGroup = new RequesterAssignedGroup() { Group = group };
            webServiceObject.AssignedToGroup = new RequesterAssignedGroup[] { assignedGroup };
            */
        }
    }
}
