using System;
using System.Linq;
using System.Text;
using softWrench.sW4.Data.Persistence.Operation;
using softWrench.sW4.Data.Persistence.WS.Ism.Entities.ISMServiceEntities;
using softWrench.sW4.Util;

namespace softWrench.sW4.Data.Persistence.WS.Commons {
    public class HapagChangeHandler {

        public static void CheckSR4ChangeGroupID(CrudOperationData entity, ServiceIncident webServiceObject) {
            // If SR is a SR4Change...
            var templateId = entity.GetAttribute("templateid") as string; 
            if (!String.IsNullOrWhiteSpace(templateId) && GetAllChangeTemplates().Contains(templateId)) {

                //...update the Group ID
                webServiceObject.Problem.ProviderAssignedGroup.Group.GroupID = GroupId;
                webServiceObject.Problem.ProviderAssignedGroup.Group.Address.OrganizationID = OrganizationId;
                webServiceObject.Problem.ProviderAssignedGroup.Group.Address.LocationID = LocationId;
            }
        }

        public static string ParseSchemaBasedLongDescription(CrudOperationData entity, string schemaId) {
            var sb = new StringBuilder();

            var exceptioncode = String.Empty;
            if (!String.IsNullOrWhiteSpace(entity.GetAttribute("exceptioncode") as String)) {
                exceptioncode = String.Format("{0} - {1}", entity.GetAttribute("exceptioncode"), entity.GetAttribute("#exceptioncode_label"));
            }

            var impact = String.Empty;
            if (entity.GetAttribute("impact") != null) {
                impact = String.Format("{0} - {1}", entity.GetAttribute("impact"), entity.GetAttribute("#impact_label"));
            }

            var urgency = String.Empty;
            if (entity.GetAttribute("urgency") != null) {
                urgency = String.Format("{0} - {1}", entity.GetAttribute("urgency"), entity.GetAttribute("#urgency_label"));
            }

            sb.AppendFormat("Category: {0}\n", entity.GetAttribute("category"));
            sb.AppendFormat("Impact: {0}\n", impact);
            sb.AppendFormat("Urgency: {0}\n", urgency);
            sb.AppendFormat("Reason: {0}\n", entity.GetAttribute("reasonforchange"));
            sb.AppendFormat("Reason Details: {0}\n", entity.GetAttribute("reasondetails"));
            sb.AppendFormat("Priority: {0}\n", entity.GetAttribute("#priority_label"));
            sb.AppendFormat("Exception Reason: {0}\n", entity.GetAttribute("exceptionreason"));            
            sb.AppendFormat("Exception Code: {0}\n", exceptioncode);
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

        public static string[] GetAllChangeTemplates() {
            return ApplicationConfiguration.DefaultChangeTeamplateId
                .Union(ApplicationConfiguration.SsoChangeTeamplateId)
                .Union(ApplicationConfiguration.TuiChangeTeamplateId)
                .ToArray(); 
        }

        public static string GetSSOTemplateString() {
            return TemplateIdHandler(ApplicationConfiguration.SsoChangeTeamplateId);
        }

        public static string GetTUITemplateString() {
            return TemplateIdHandler(ApplicationConfiguration.TuiChangeTeamplateId);
        }
        public static string GetAllTemplateString() {
            return TemplateIdHandler(GetAllChangeTemplates());
        }

        private static string TemplateIdHandler(string[] templateids) {
            var strtemplateids = String.Join("','", templateids);
            strtemplateids = "'" + strtemplateids + "'";

            return strtemplateids;
        }
    }
}
