using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using cts.commons.persistence;
using cts.commons.portable.Util;
using Newtonsoft.Json.Linq;
using softwrench.sw4.Shared2.Data.Association;
using softwrench.sW4.Shared2.Metadata.Applications;
using softwrench.sW4.Shared2.Metadata.Applications.Schema;
using softWrench.sW4.Data.API.Composition;
using softWrench.sW4.Data.Entities;
using softWrench.sW4.Data.Persistence.Dataset.Commons.Ticket.Commlog;
using softWrench.sW4.Data.Persistence.Engine;
using softWrench.sW4.Data.Persistence.Operation;
using softWrench.sW4.Data.Persistence.SWDB;
using softWrench.sW4.Data.Persistence.WS.API;
using softWrench.sW4.Data.Search;
using softWrench.sW4.Metadata;
using softWrench.sW4.Metadata.Applications;
using softWrench.sW4.Metadata.Applications.DataSet;
using softWrench.sW4.Metadata.Applications.DataSet.Filter;
using softWrench.sW4.Security.Services;
using softWrench.sW4.Util;

namespace softWrench.sW4.Data.Persistence.Dataset.Commons {
    public class BaseRelatedRecordDataSet : MaximoApplicationDataSet {
        private readonly IDictionary<string, string> _entities = new Dictionary<string, string>
        {
            {"SR", "Service Request"},
            {"INCIDENT", "Incident"},
            {"WORKORDER", "Work Order"}
        };

        private readonly ISWDBHibernateDAO _swdbDao;
        public BaseRelatedRecordDataSet(ISWDBHibernateDAO swdbDao) {
            _swdbDao = swdbDao;
        }

        public IEnumerable<IAssociationOption> GetRelatedRecordTypes(OptionFieldProviderParameters parameters) {
            var customerApps = MetadataProvider.FetchTopLevelApps(ClientPlatform.Web, null);

            var result = _entities.Where(entity => customerApps.Any(a => a.Entity.ToLower() == entity.Key.ToLower())).Select(e => new AssociationOption(e.Key, e.Value));

            return result;
        }

        public override string ApplicationName() {
            return "relatedrecord";
        }

        public override string ClientFilter() {
            return null;
        }
    }
}