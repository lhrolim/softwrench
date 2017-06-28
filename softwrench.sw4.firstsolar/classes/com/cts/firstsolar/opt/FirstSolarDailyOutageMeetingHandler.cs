using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using cts.commons.persistence;
using cts.commons.simpleinjector;
using NHibernate.Util;
using softwrench.sw4.firstsolar.classes.com.cts.firstsolar.model;
using softwrench.sW4.Shared2.Data;
using softWrench.sW4.Data.Entities;
using softWrench.sW4.Data.Persistence.Operation;

namespace softwrench.sw4.firstsolar.classes.com.cts.firstsolar.opt {
    public class FirstSolarDailyOutageMeetingHandler : ISingletonComponent {

        [Import]
        public ISWDBHibernateDAO Dao { get; set; }

        public bool HandleDailyOutageMeetings(CrudOperationData crudoperationData, WorkPackage package) {
            if (package.DailyOutageMeetings == null) {
                package.DailyOutageMeetings = new List<DailyOutageMeeting>();
            }

            var toKeepDom = new List<DailyOutageMeeting>();
            var anyNewDom = false;

            if (crudoperationData.AssociationAttributes != null && crudoperationData.AssociationAttributes.ContainsKey("callOuts_")) {
                var domsData = crudoperationData.AssociationAttributes["dailyOutageMeetings_"] as List<CrudOperationData>;
                if (domsData == null) {
                    throw new Exception("Incorrect format of daily outage meeting list.");
                }
                domsData.ForEach((data) => {
                    var dom = GetOurCreateDailyOutageMeeting(data, package.DailyOutageMeetings, toKeepDom);
                    anyNewDom = anyNewDom || dom.Id == null;
                    EntityBuilder.PopulateTypedEntity(data, dom);
                    if (dom.Id != null) {
                        return;
                    }
                    package.DailyOutageMeetings.Add(dom);
                    toKeepDom.Add(dom);
                });
            }

            var deleted = new List<DailyOutageMeeting>();
            package.DailyOutageMeetings.ForEach(dom => {
                if (toKeepDom.Contains(dom)) {
                    return;
                }
                Dao.Delete(dom);
                deleted.Add(dom);
            });
            deleted.ForEach(dom => package.DailyOutageMeetings.Remove(dom));
            return anyNewDom;
        }

        private static DailyOutageMeeting GetOurCreateDailyOutageMeeting(AttributeHolder crudoperationData, ICollection<DailyOutageMeeting> existingDom, ICollection<DailyOutageMeeting> toKeepDom) {
            var id = crudoperationData.GetIntAttribute("id");
            if (id == null || existingDom == null) {
                return new DailyOutageMeeting();
            }
            var found = existingDom.FirstOrDefault(dom => dom.Id == id);
            if (found == null) {
                return new DailyOutageMeeting() { Id = id };
            }
            toKeepDom.Add(found);
            return found;
        }
    }
}
