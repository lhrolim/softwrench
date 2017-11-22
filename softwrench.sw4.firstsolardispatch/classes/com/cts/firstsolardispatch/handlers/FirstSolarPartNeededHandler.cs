using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using cts.commons.persistence;
using NHibernate.Util;
using softwrench.sw4.firstsolardispatch.classes.com.cts.firstsolardispatch.model;
using softwrench.sW4.Shared2.Metadata.Applications.Schema;
using softwrench.sW4.Shared2.Util;
using softWrench.sW4.Data.Entities;
using softWrench.sW4.Data.Persistence.Operation;

namespace softwrench.sw4.firstsolardispatch.classes.com.cts.firstsolardispatch.handlers {
    public class FirstSolarPartNeededHandler : BaseHandler<PartNeeded> {

        [Import]
        public ISWDBHibernateDAO Dao { get; set; }

        public bool HandleParts(CrudOperationData crudoperationData, Inverter inverter, ApplicationSchemaDefinition schema) {

            if (!schema.Compositions().Any(c => EntityUtil.IsRelationshipNameEquals(c.AssociationKey, "parts"))) {
                inverter.PartsNeeded = inverter.PartsNeeded ?? new List<PartNeeded>();
                //might be disabled due to security reasons
                return false;
            }

            var existingParts = inverter.PartsNeeded;
            inverter.PartsNeeded = new List<PartNeeded>();

            var anyNewPart = false;

            if (crudoperationData.AssociationAttributes != null && crudoperationData.AssociationAttributes.ContainsKey("parts_")) {
                var partsData = crudoperationData.AssociationAttributes["parts_"] as List<CrudOperationData>;
                if (partsData == null) {
                    throw new Exception("Incorrect format of parts list.");
                }
                partsData.ForEach((data) => {
                    var part = GetOrCreate(data, existingParts);
                    anyNewPart = anyNewPart || part.Id == null;
                    inverter.PartsNeeded.Add(HandlePart(data, part, inverter));
                });
            }
            existingParts?.ForEach(part => {
                Dao.Delete(part);
            });
            return anyNewPart;
        }

        private PartNeeded HandlePart(CrudOperationData crudoperationData, PartNeeded part, Inverter inverter) {
            part = EntityBuilder.PopulateTypedEntity(crudoperationData, part);
            part.Inverter = inverter;
            if (!"shipment".Equals(part.DeliveryMethod)) {
                part.ExpectedDate = null;
                part.DeliveryLocation = null;
            }
            part = Dao.Save(part);
            return part;
        }

        protected override PartNeeded NewInstance() {
            return new PartNeeded();
        }
    }
}
