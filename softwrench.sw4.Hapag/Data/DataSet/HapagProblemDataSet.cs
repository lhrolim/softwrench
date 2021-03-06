﻿using softWrench.sW4.Data.Persistence;
using softWrench.sW4.Data.Persistence.Relational;
using softwrench.sw4.Hapag.Security;
using softwrench.sW4.Shared2.Metadata.Applications.Schema;
using softWrench.sW4.Data;
using softWrench.sW4.Data.API;
using softWrench.sW4.Metadata.Applications;
using softWrench.sW4.Metadata.Applications.DataSet;
using softWrench.sW4.Metadata.Security;

namespace softwrench.sw4.Hapag.Data.DataSet {
    class HapagProblemDataSet : HapagBaseApplicationDataSet {
        public HapagProblemDataSet(IHlagLocationManager locationManager, EntityRepository entityRepository, MaximoHibernateDAO maxDao)
            : base(locationManager, entityRepository, maxDao) {
        }

        protected override ApplicationDetailResult GetApplicationDetail(ApplicationMetadata application, InMemoryUser user, DetailRequest request) {
            var dbDetail = base.GetApplicationDetail(application, user, request);
            var resultObject = dbDetail.ResultObject;
            if (application.Schema.Mode == SchemaMode.output) {
                HandleClosedDate(resultObject, application);
            }
            return dbDetail;
        }

        private void HandleClosedDate(DataMap resultObject, ApplicationMetadata application) {
            var status = resultObject.GetAttribute("status");
            var statusDate = resultObject.GetAttribute("statusdate");
            if ("CLOSED".Equals(status)) {
                resultObject.Attributes["#closeddate"] = statusDate;
            } else {
                resultObject.Attributes["#closeddate"] = "";
            }
        }

        public override string ApplicationName() {
            return "problem";
        }

        public override string ClientFilter() {
            return "hapag";
        }
    }
}
