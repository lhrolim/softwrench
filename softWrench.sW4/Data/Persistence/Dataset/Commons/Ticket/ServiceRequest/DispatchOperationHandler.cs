using System;
using cts.commons.portable.Util;
using Newtonsoft.Json.Linq;
using Quartz.Util;
using softwrench.sW4.Shared2.Metadata.Applications.Schema;
using softWrench.sW4.Data.Entities;
using softWrench.sW4.Data.Persistence.Operation;
using softWrench.sW4.Data.Persistence.WS.API;
using softWrench.sW4.Data.Persistence.WS.Internal;
using softWrench.sW4.Metadata;
using softWrench.sW4.Metadata.Applications;
using softWrench.sW4.Metadata.Entities;
using softWrench.sW4.Util;

namespace softWrench.sW4.Data.Persistence.Dataset.Commons.Ticket.ServiceRequest
{
    class DispatchOperationHandler : BaseMaximoCustomConnector
    {
        private readonly EntityMetadata _woEntity;
        private readonly ApplicationMetadata _woApplication;
        private readonly EntityMetadata _incidentEntity;
        private readonly ApplicationMetadata _incidentApplication;

        public DispatchOperationHandler()
        {
            _woEntity = MetadataProvider.Entity("WORKORDER", false);
            _woApplication = MetadataProvider.Application("WORKORDER").ApplyPoliciesWeb(new ApplicationMetadataSchemaKey("newdetail"));
            _incidentEntity = MetadataProvider.Entity("INCIDENT", false);
            _incidentApplication = MetadataProvider.Application("INCIDENT").ApplyPoliciesWeb(new ApplicationMetadataSchemaKey("newdetail"));
        }

        public class DispatchOperationData : CrudOperationDataContainer
        {

        }

        public object DispatchWO(DispatchOperationData srData)
        {
            var srCrudData = srData.CrudData;

            var woCrudData = EntityBuilder.BuildFromJson<CrudOperationData>(typeof(CrudOperationData), _woEntity, _woApplication, new JObject(), null);
            woCrudData.SetAttribute("onbehalfof", srCrudData.GetStringAttribute("affectedperson"));
            woCrudData.SetAttribute("assetnum", srCrudData.GetStringAttribute("assetnum"));
            woCrudData.SetAttribute("cinum", srCrudData.GetStringAttribute("cinum"));
            woCrudData.SetAttribute("commodity", srCrudData.GetStringAttribute("commodity"));
            woCrudData.SetAttribute("commoditygroup", srCrudData.GetStringAttribute("commoditygroup"));
            woCrudData.SetAttribute("description", srCrudData.GetStringAttribute("description"));
            woCrudData.SetAttribute("ld_.ldtext", srCrudData.GetStringAttribute("ld_.ldtext"));
            woCrudData.SetAttribute("glaccount", srCrudData.GetStringAttribute("glaccount"));
            woCrudData.SetAttribute("location", srCrudData.GetStringAttribute("location"));
            woCrudData.SetAttribute("reportedby", srCrudData.GetStringAttribute("reportedby"));
            woCrudData.SetAttribute("phone", srCrudData.GetStringAttribute("reportedphone"));
            woCrudData.SetAttribute("classstructureid", srCrudData.GetStringAttribute("classstructureid"));
            woCrudData.SetAttribute("status", "APPR");
            woCrudData.SetAttribute("statusdate", DateTime.Now.FromServerToRightKind());
            woCrudData.SetAttribute("reportdate", DateTime.Now.FromServerToRightKind());
            woCrudData.SetAttribute("woclass", "WORKORDER");
            woCrudData.SetAttribute("origrecordid", srCrudData.Id);
            woCrudData.SetAttribute("origrecordclass", "SR");
            woCrudData.SetAttribute("siteid", srCrudData.GetStringAttribute("siteid"));
            woCrudData.SetAttribute("orgid", srCrudData.GetStringAttribute("orgid"));

            var result = (TargetResult)Maximoengine.Create(woCrudData);
            var id = result.Id ?? result.UserId;

            //customization for deltadental here
            var label = ApplicationConfiguration.ClientName.EqualsIc("deltadental") ? "dispatched" : "created";

            result.SuccessMessage = "Work Order {0} sucessfully {1}.".FormatInvariant(id, label);

            return result;
        }

        public object DispatchIncident(DispatchOperationData srData)
        {
            var srCrudData = srData.CrudData;
            var incidentCrudData = EntityBuilder.BuildFromJson<CrudOperationData>(typeof(CrudOperationData), _incidentEntity, _incidentApplication, new JObject(), null);
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
            incidentCrudData.SetAttribute("description_longdescription", srCrudData.GetStringAttribute("description_longdescription"));
            incidentCrudData.SetAttribute("glaccount", srCrudData.GetStringAttribute("glaccount"));
            incidentCrudData.SetAttribute("location", srCrudData.GetStringAttribute("location"));
            incidentCrudData.SetAttribute("orgid", srCrudData.GetStringAttribute("orgid"));
            incidentCrudData.SetAttribute("siteid", srCrudData.GetStringAttribute("siteid"));
            incidentCrudData.SetAttribute("reportedby", srCrudData.GetStringAttribute("reportedby"));
            incidentCrudData.SetAttribute("reportedemail", srCrudData.GetStringAttribute("reportedemail"));
            incidentCrudData.SetAttribute("reportedphone", srCrudData.GetStringAttribute("reportedphone"));
            incidentCrudData.SetAttribute("reportedpriority", srCrudData.GetStringAttribute("reportedpriority"));
            incidentCrudData.SetAttribute("source", srCrudData.GetStringAttribute("source"));
            incidentCrudData.SetAttribute("virtualenv", srCrudData.GetStringAttribute("virtualenv"));
            incidentCrudData.SetAttribute("origrecordid", srCrudData.UserId);
            incidentCrudData.SetAttribute("origrecordclass", "SR");
            incidentCrudData.SetAttribute("relatetype", "followup");

            var result = (TargetResult)Maximoengine.Create(incidentCrudData);
            var id = result.Id ?? result.UserId;

            result.SuccessMessage = "Incident {0} sucessfully created.".FormatInvariant(id);

            return result;
        }
    }
}


