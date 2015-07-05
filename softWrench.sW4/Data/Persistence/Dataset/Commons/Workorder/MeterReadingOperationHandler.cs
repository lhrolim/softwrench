﻿using System;
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
    class MeterReadingOperationHandler : BaseMaximoCustomConnector {
        private readonly EntityMetadata _meterReadingEntity;
        private EntityMetadata _woEntity;

        public MeterReadingOperationHandler() {
            _meterReadingEntity = MetadataProvider.Entity("METERREADING", false);
            _woEntity = MetadataProvider.Entity("WORKORDER", false);
        }

        public class EnterMeterOperationData : CrudOperationDataContainer {

        }


        public object EnterMeter(EnterMeterOperationData woData) {
            MaximoOperationExecutionContext maximoExecutionContext = null;
            var assetMeters = (IEnumerable<Entity>)woData.CrudData.GetRelationship("assetmeter");
            var locationMeters = (IEnumerable<Entity>)woData.CrudData.GetRelationship("locationmeter");

            foreach (var assetMeter in assetMeters) {
                var crudData = new CrudOperationData(assetMeter.GetStringAttribute("assetmeterid"), assetMeter.Attributes, assetMeter.AssociationAttributes, _meterReadingEntity, null);
                if (assetMeter.GetAttribute("newreading") == null) {
                    //let´s ignore the ones which were not filled by the user
                    continue;
                }
                crudData.SetAttribute("newreading", assetMeter.GetUnMappedAttribute("newreading"));
                crudData.SetAttribute("newreadingdate", assetMeter.GetUnMappedAttribute("newreadingdate"));
                crudData.SetAttribute("inspector", assetMeter.GetUnMappedAttribute("inspector"));
                Maximoengine.Update(crudData);
            }

            foreach (var locationMeter in locationMeters) {
                var crudData = new CrudOperationData(locationMeter.GetStringAttribute("locationmeterid"), locationMeter.Attributes, locationMeter.AssociationAttributes, _meterReadingEntity, null);
                if (locationMeter.GetAttribute("newreading") == null) {
                    //let´s ignore the ones which were not filled by the user
                    continue;
                }
                crudData.SetAttribute("newreading", locationMeter.GetUnMappedAttribute("newreading"));
                crudData.SetAttribute("newreadingdate", locationMeter.GetUnMappedAttribute("newreadingdate"));
                crudData.SetAttribute("inspector", locationMeter.GetUnMappedAttribute("inspector"));
                Maximoengine.Update(crudData);
            }
            return new TargetResult(null, null, null);
        }




    }
}


