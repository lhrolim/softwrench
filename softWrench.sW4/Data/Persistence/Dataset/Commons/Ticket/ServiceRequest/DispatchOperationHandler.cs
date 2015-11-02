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

namespace softWrench.sW4.Data.Persistence.Dataset.Commons.Ticket.ServiceRequest {
    class DispatchOperationHandler : BaseMaximoCustomConnector {
        private readonly EntityMetadata _woEntity;
        private readonly ApplicationMetadata _woApplication;

        public DispatchOperationHandler() {
            _woEntity = MetadataProvider.Entity("WORKORDER", false);
            _woApplication = MetadataProvider.Application("WORKORDER").ApplyPoliciesWeb(new ApplicationMetadataSchemaKey("newdetail"));
        }

        public class DispatchOperationData : CrudOperationDataContainer {

        }

        public object DispatchWO(DispatchOperationData srData) {
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
    }
}


