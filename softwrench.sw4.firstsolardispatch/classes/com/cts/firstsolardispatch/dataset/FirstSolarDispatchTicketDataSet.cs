using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Threading.Tasks;
using cts.commons.persistence;
using cts.commons.persistence.Transaction;
using cts.commons.portable.Util;
using Castle.Core.Internal;
using Newtonsoft.Json.Linq;
using softwrench.sw4.firstsolardispatch.classes.com.cts.firstsolardispatch.handlers;
using softwrench.sw4.firstsolardispatch.classes.com.cts.firstsolardispatch.model;
using softWrench.sW4.Data;
using softWrench.sW4.Data.API;
using softWrench.sW4.Data.API.Composition;
using softWrench.sW4.Data.Entities;
using softWrench.sW4.Data.Entities.Attachment;
using softWrench.sW4.Data.Persistence.Dataset.Commons;
using softWrench.sW4.Data.Persistence.Operation;
using softWrench.sW4.Data.Persistence.WS.API;
using softWrench.sW4.Metadata.Applications;
using softWrench.sW4.Security.Services;
using softWrench.sW4.Util;

namespace softwrench.sw4.firstsolardispatch.classes.com.cts.firstsolardispatch.dataset {
    public class FirstSolarDispatchTicketDataSet : SWDBApplicationDataset {

        [Import]
        public ISWDBHibernateDAO Dao { get; set; }

        [Import]
        public FirstSolarInverterHandler InverterHandler { get; set; }


        [Transactional(DBType.Swdb)]
        public override async Task<TargetResult> DoExecute(OperationWrapper operationWrapper) {
            var crudoperationData = (CrudOperationData)operationWrapper.OperationData();
            var ticket = GetOrCreate<DispatchTicket>(operationWrapper, true);

            ticket.GpsLatitude = ConvertDecimal(crudoperationData, "gpslatitude");
            ticket.GpsLongitude = ConvertDecimal(crudoperationData, "gpslongitude");

            if (ticket.CreatedDate == null) {
                ticket.CreatedDate = DateTime.Now;
            }
            //saving the ticket first in order to obtain a valid id
            ticket = await Dao.SaveAsync(ticket);
            
            var newTickets = HandleFileExplorerDocLinks(ticket, crudoperationData, "attachments_");
            foreach (var newTicket in newTickets) {
                ticket.Attachments.Add(newTicket);
            }
            InverterHandler.HandleInverters(crudoperationData, ticket, operationWrapper.ApplicationMetadata.Schema);

            ticket = await Dao.SaveAsync(ticket);
            return new TargetResult(ticket.Id.ToString(), null, DataMap.BlankInstance("_dispatchticket"));
        }

        private decimal? ConvertDecimal(CrudOperationData data, string att) {
            var attStringValue = data.GetStringAttribute(att);
            if (string.IsNullOrEmpty(attStringValue)) {
                return null;
            }
            return Convert.ToDecimal(attStringValue);
        }

        public override string ApplicationName() {
            return "_dispatchticket";
        }
    }
}
