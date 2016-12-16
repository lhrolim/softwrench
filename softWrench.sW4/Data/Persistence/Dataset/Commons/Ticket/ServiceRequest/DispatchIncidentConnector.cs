using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using Quartz.Util;
using softWrench.sW4.Data.Entities;
using softWrench.sW4.Data.Persistence.Operation;
using softWrench.sW4.Data.Persistence.WS.API;

namespace softWrench.sW4.Data.Persistence.Dataset.Commons.Ticket.ServiceRequest {
    public class DispatchIncidentConnector : DispatchOperationHandler{

        public object DispatchIncident(DispatchOperationData srData) {
            var srCrudData = srData.CrudData;

            var descriptionAttribute = "longdescription_";
            var longdescription = srCrudData.GetAttribute(descriptionAttribute + ".ldtext");
            if (longdescription == null) {
                descriptionAttribute = "ld_";
                longdescription = srCrudData.GetAttribute(descriptionAttribute + ".ldtext");
            }
            var incident = new Dictionary<string, object>() {
                { descriptionAttribute + ".ldtext", longdescription }
            };
            var incidentJson = JObject.FromObject(incident);

            var incidentCrudData = EntityBuilder.BuildFromJson<CrudOperationData>(typeof(CrudOperationData), _incidentEntity, _incidentApplication, incidentJson, null);
            incidentCrudData.SetAttribute("affectedemail", srCrudData.GetStringAttribute("affectedemail"));
            incidentCrudData.SetAttribute("affectedperson", srCrudData.GetStringAttribute("affectedperson"));
            incidentCrudData.SetAttribute("affectedphone", srCrudData.GetStringAttribute("affectedphone"));
            incidentCrudData.SetAttribute("assetnum", srCrudData.GetStringAttribute("assetnum"));
            incidentCrudData.SetAttribute("assetsiteid", srCrudData.GetStringAttribute("assetsiteid"));
            incidentCrudData.SetAttribute("cinum", srCrudData.GetStringAttribute("cinum"));
            incidentCrudData.SetAttribute("classstructureid", srCrudData.GetStringAttribute("classstructureid"));
            incidentCrudData.SetAttribute("commodity", srCrudData.GetStringAttribute("commodity"));
            incidentCrudData.SetAttribute("commoditygroup", srCrudData.GetStringAttribute("commoditygroup"));
            incidentCrudData.SetAttribute("description", srCrudData.GetStringAttribute("description"));
            incidentCrudData.SetAttribute("glaccount", srCrudData.GetStringAttribute("glaccount"));
            incidentCrudData.SetAttribute("location", srCrudData.GetStringAttribute("location"));
            incidentCrudData.SetAttribute("orgid", srCrudData.GetStringAttribute("orgid"));
            incidentCrudData.SetAttribute("siteid", srCrudData.GetStringAttribute("siteid"));
            incidentCrudData.SetAttribute("reportedby", srCrudData.GetStringAttribute("reportedby"));
            incidentCrudData.SetAttribute("reportedemail", srCrudData.GetStringAttribute("reportedemail"));
            incidentCrudData.SetAttribute("reportedphone", srCrudData.GetStringAttribute("reportedphone"));
            incidentCrudData.SetAttribute("reportedpriority", srCrudData.GetAttribute("reportedpriority"));
            incidentCrudData.SetAttribute("source", srCrudData.GetStringAttribute("source"));
            incidentCrudData.SetAttribute("virtualenv", srCrudData.GetStringAttribute("virtualenv"));
            incidentCrudData.SetAttribute("origrecordid", srCrudData.UserId);
            incidentCrudData.SetAttribute("origrecordclass", "SR");
            incidentCrudData.SetAttribute("relatetype", "followup");
            incidentCrudData.SetAttribute("status", "NEW");

            var result = (TargetResult)Maximoengine.Create(incidentCrudData);
            var id = result.Id ?? result.UserId;

            result.SuccessMessage = "Incident {0} sucessfully created.".FormatInvariant(id);

            return result;
        }

        public override string ActionId() {
            return "dispatchincident";
        }

    }
}
