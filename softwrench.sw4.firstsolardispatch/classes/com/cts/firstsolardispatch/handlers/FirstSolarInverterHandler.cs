using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using cts.commons.persistence;
using NHibernate.Util;
using softwrench.sw4.firstsolardispatch.classes.com.cts.firstsolardispatch.model;
using softwrench.sW4.Shared2.Metadata.Applications;
using softwrench.sW4.Shared2.Metadata.Applications.Schema;
using softwrench.sW4.Shared2.Util;
using softWrench.sW4.Data.Entities;
using softWrench.sW4.Data.Persistence.Operation;
using softWrench.sW4.Metadata;

namespace softwrench.sw4.firstsolardispatch.classes.com.cts.firstsolardispatch.handlers {
    public class FirstSolarInverterHandler : BaseHandler<Inverter> {

        [Import]
        public ISWDBHibernateDAO Dao { get; set; }

        [Import]
        public FirstSolarPartNeededHandler PartsHandler { get; set; }

        public bool HandleInverters(CrudOperationData crudoperationData, DispatchTicket ticket, ApplicationSchemaDefinition schema) {

            if (!schema.Compositions().Any(c => EntityUtil.IsRelationshipNameEquals(c.AssociationKey, "inverters"))) {
                ticket.Inverters = ticket.Inverters ?? new List<Inverter>();
                //might be disabled due to security reasons
                return false;
            }

            var existingInverters = ticket.Inverters;
            ticket.Inverters = new List<Inverter>();

            var anyNewInverter = false;

            if (crudoperationData.AssociationAttributes != null && crudoperationData.AssociationAttributes.ContainsKey("inverters_")) {
                var invertersData = crudoperationData.AssociationAttributes["inverters_"] as List<CrudOperationData>;
                if (invertersData == null) {
                    throw new Exception("Incorrect format of inverter list.");
                }
                invertersData.ForEach((data) => {
                    var inverter = GetOrCreate(data, existingInverters);
                    anyNewInverter = anyNewInverter || inverter.Id == null;
                    ticket.Inverters.Add(HandleInverter(data, inverter, ticket));
                });
            }
            existingInverters?.ForEach(callout => {
                Dao.Delete(callout);
            });
            return anyNewInverter;
        }

        private Inverter HandleInverter(CrudOperationData crudoperationData, Inverter inverter, DispatchTicket ticket) {
            inverter = EntityBuilder.PopulateTypedEntity(crudoperationData, inverter);
            inverter.AssetDescription = crudoperationData.GetStringAttribute("assetdescription");
            inverter.Ticket = ticket;
            var schema = MetadataProvider.Schema("_Inverter", "detail", ClientPlatform.Web);

            //saving the inverter first in order to obtain a valid id
            inverter = Dao.Save(inverter);

            PartsHandler.HandleParts(crudoperationData, inverter, schema);
            inverter = Dao.Save(inverter);
            return inverter;
        }

        protected override Inverter NewInstance() {
            return new Inverter();
        }
    }
}
