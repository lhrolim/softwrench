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
using w = softWrench.sW4.Data.Persistence.WS.Internal.WsUtil;

namespace softWrench.sW4.Data.Persistence.Dataset.Commons.Workorder {
    class ProgressMultiAssetHandler : BaseMaximoCustomConnector {
        
        private readonly EntityMetadata _multiAssetLocciEntity;
        private EntityMetadata _woEntity;

        public ProgressMultiAssetHandler() {
            _multiAssetLocciEntity = MetadataProvider.Entity("MULTIASSETLOCCI", false);
            _woEntity = MetadataProvider.Entity("WORKORDER", false);
        }

        public class MultiAssetOperationData : CrudOperationDataContainer {

        }


//        public object MarkMultiAssetComplete(MultiAssetOperationData woData) {
//            MaximoOperationExecutionContext maximoExecutionContext = null;
//            var wonum = woData.CrudData.GetAttribute("wonum");
//            var siteid = woData.CrudData.GetAttribute("siteid");
//            var multiassetlocci = (IEnumerable<Entity>)woData.CrudData.GetRelationship("multiassetlocci");
//
//           
//            foreach (var multiasset in multiassetlocci) {
//                //only one
//                var crudData = new CrudOperationData(multiasset.GetStringAttribute("multiid"), multiasset.Attributes, multiasset.AssociationAttributes, _multiAssetLocciEntity, null);
//                GetContext()
//                if (!multiasset.GetAttribute("#isDirty").Equals(true)) {
//                    //let´s ignore the ones which were not filled by the user
//                    continue;
//                }
//                crudData.SetAttribute("newreading", assetMeter.GetUnMappedAttribute("newreading"));
//                crudData.SetAttribute("newreadingdate", assetMeter.GetUnMappedAttribute("newreadingdate"));
//                crudData.SetAttribute("inspector", assetMeter.GetUnMappedAttribute("inspector"));
//                return Maximoengine.Update(crudData);
//            }
//
//            
//            return new TargetResult(null, null, null);
//        }




    }
}


