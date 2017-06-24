using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using cts.commons.persistence;
using cts.commons.simpleinjector;
using NHibernate.Util;
using softwrench.sw4.firstsolar.classes.com.cts.firstsolar.model;
using softWrench.sW4.Data.Entities;
using softWrench.sW4.Data.Persistence.Operation;

namespace softwrench.sw4.firstsolar.classes.com.cts.firstsolar.opt {
    public class FirstSolarDailyOutageMeetingHandler : ISingletonComponent {

        [Import]
        public ISWDBHibernateDAO Dao { get; set; }

        public bool HandleDailyOutageMeetings(CrudOperationData crudoperationData, WorkPackage package) {
            var existingDom = package.DailyOutageMeetings;
            package.DailyOutageMeetings = new List<DailyOutageMeeting>();
            var anyNewDom = false;

            if (crudoperationData.AssociationAttributes != null && crudoperationData.AssociationAttributes.ContainsKey("callOuts_")) {
                var domsData = crudoperationData.AssociationAttributes["dailyOutageMeetings_"] as List<CrudOperationData>;
                if (domsData == null) {
                    throw new Exception("Incorrect format of daily outage meeting list.");
                }
                domsData.ForEach((data) => {
                    var dom = GetOurCreateDailyOutageMeeting(data, existingDom);
                    anyNewDom = anyNewDom || dom.Id == null;
                    package.DailyOutageMeetings.Add(HandleDailyOutageMeeting(data, dom, package));
                });
            }
            existingDom?.ForEach(dom => {
                Dao.Delete(dom);
            });
            return anyNewDom;
        }

        private DailyOutageMeeting HandleDailyOutageMeeting(CrudOperationData crudoperationData, DailyOutageMeeting dom, WorkPackage workpackage) {
            EntityBuilder.PopulateTypedEntity(crudoperationData, dom);
            dom.WorkPackageId = workpackage.Id ?? 0;
            dom = Dao.Save(dom);
            return dom;
        }

        private static DailyOutageMeeting GetOurCreateDailyOutageMeeting(CrudOperationData crudoperationData, IList<DailyOutageMeeting> existingDom) {
            var id = crudoperationData.GetIntAttribute("id");
            if (id == null || existingDom == null) {
                return new DailyOutageMeeting();
            }
            var found = existingDom.FirstOrDefault(dom => dom.Id == id);
            if (found == null) {
                return new DailyOutageMeeting() { Id = id };
            }
            existingDom.Remove(found);
            return found;
        }
    }
}
