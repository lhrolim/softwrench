using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading;
using JetBrains.Annotations;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using softWrench.sW4.Data.Entities;
using softWrench.sW4.Data.Persistence.Operation;
using softWrench.sW4.Data.Persistence.Relational;
using softWrench.sW4.Data.Persistence.Relational.EntityRepository;
using softWrench.sW4.Data.Persistence.WS.API;
using softWrench.sW4.Data.Persistence.WS.Internal;
using softWrench.sW4.Metadata;
using softWrench.sW4.Metadata.Entities;
using cts.commons.simpleinjector;
using DocumentFormat.OpenXml.Vml.Spreadsheet;
using softwrench.sW4.Shared2.Metadata;
using softwrench.sW4.Shared2.Metadata.Applications.Schema;
using softWrench.sW4.Metadata.Applications;
using softWrench.sW4.Util;
using w = softWrench.sW4.Data.Persistence.WS.Internal.WsUtil;

namespace softWrench.sW4.Data.Persistence.Dataset.Commons.ServiceRequest {
    class DispatchOperationHandler : BaseMaximoCustomConnector {
        private EntityMetadata _woEntity;
        private ApplicationMetadata _woApplication;

        public DispatchOperationHandler() {
            _woEntity = MetadataProvider.Entity("WORKORDER", false);
            _woApplication = MetadataProvider.Application("WORKORDER").ApplyPoliciesWeb(new ApplicationMetadataSchemaKey("newdetail"));
        }

        public class DispatchOperationData : CrudOperationDataContainer {

        }

        public object DispatchWO(DispatchOperationData srData) {
            MaximoOperationExecutionContext maximoExecutionContext = null;
            var srCrudData = srData.CrudData;

            var woCrudData = EntityBuilder.BuildFromJson<CrudOperationData>(typeof(CrudOperationData), _woEntity, _woApplication, new JObject(), null);
            woCrudData.SetAttribute("onbehalfof", srCrudData.GetStringAttribute("affectedperson"));
            woCrudData.SetAttribute("assetnum", srCrudData.GetStringAttribute("assetnum"));
            woCrudData.SetAttribute("cinum", srCrudData.GetStringAttribute("cinum"));
            woCrudData.SetAttribute("commodity", srCrudData.GetStringAttribute("commodity"));
            woCrudData.SetAttribute("commoditygroup", srCrudData.GetStringAttribute("commoditygroup"));
            woCrudData.SetAttribute("description", srCrudData.GetStringAttribute("description"));
            woCrudData.SetAttribute("description_longdescription", srCrudData.GetStringAttribute("description_longdescription"));
            woCrudData.SetAttribute("glaccount", srCrudData.GetStringAttribute("glaccount"));
            woCrudData.SetAttribute("location", srCrudData.GetStringAttribute("location"));
            woCrudData.SetAttribute("owner", srCrudData.GetStringAttribute("owner"));
            woCrudData.SetAttribute("reportedby", srCrudData.GetStringAttribute("reportedby"));
            woCrudData.SetAttribute("phone", srCrudData.GetStringAttribute("reportedphone"));
            woCrudData.SetAttribute("classstructureid", srCrudData.GetStringAttribute("classstructureid"));
            woCrudData.SetAttribute("status", "APPR");
            woCrudData.SetAttribute("statusdate", DateTime.Now.FromServerToRightKind());
            woCrudData.SetAttribute("woclass", "WORKORDER");
            woCrudData.SetAttribute("origrecordid", srCrudData.Id);
            woCrudData.SetAttribute("origrecordclass", "SR");
            woCrudData.SetAttribute("siteid", srCrudData.GetStringAttribute("siteid"));
            woCrudData.SetAttribute("orgid", srCrudData.GetStringAttribute("orgid"));

            return Maximoengine.Create(woCrudData);
        }
    }
}


