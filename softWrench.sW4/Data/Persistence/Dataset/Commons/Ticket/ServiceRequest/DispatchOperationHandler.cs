using System;
using System.Collections.Generic;
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
    public class DispatchOperationHandler : BaseMaximoCustomConnector {
        private readonly EntityMetadata _woEntity;
        private readonly ApplicationMetadata _woApplication;
        protected readonly EntityMetadata _incidentEntity;
        protected readonly ApplicationMetadata _incidentApplication;

        public override string ApplicationName() {
            return "sr,servicerequest";
        }

        public override string ActionId() {
            return "dispatch";
        }


        public DispatchOperationHandler() {
            var woApplication = MetadataProvider.Application("workorder", false);
            var incidentApp = MetadataProvider.Application("incident", false);

            if (woApplication != null) {
                _woEntity = MetadataProvider.Entity("WORKORDER", false);
                _woApplication = woApplication.ApplyPoliciesWeb(new ApplicationMetadataSchemaKey("newdetail"));
            }
            if (incidentApp != null) {
                _incidentEntity = MetadataProvider.Entity("INCIDENT", false);
                _incidentApplication = incidentApp.ApplyPoliciesWeb(new ApplicationMetadataSchemaKey("newdetail"));
            }
        }

        public class DispatchOperationData : CrudOperationDataContainer {

        }

      

        public virtual CrudOperationData CreateWoCrudData(CrudOperationData srCrudData) {
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
            return woCrudData;
        }

        

      
    }
}


